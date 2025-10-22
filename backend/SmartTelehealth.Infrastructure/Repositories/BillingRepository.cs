using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace SmartTelehealth.Infrastructure.Repositories
{
    public class BillingRepository : RepositoryBase<BillingRecord>, IBillingRepository
    {
        private readonly ApplicationDbContext _context;

        public BillingRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a billing record by its unique identifier with related entities
        /// </summary>
        public async Task<BillingRecord?> GetByIdWithDetailsAsync(Guid billingId)
        {
            return await _context.BillingRecords
                .Include(b => b.User)
                .Include(b => b.Subscription)
                .Include(b => b.Currency)
                .FirstOrDefaultAsync(b => b.Id == billingId);
        }

        public async Task<IEnumerable<BillingRecord>> GetByUserIdAsync(int userId)
        {
            return await _context.BillingRecords
                .Include(b => b.Subscription)
                .Include(b => b.Currency)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BillingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BillingRecord>> GetBySubscriptionIdAsync(Guid subscriptionId)
        {
            return await _context.BillingRecords
                .Include(b => b.User)
                .Include(b => b.Currency)
                .Where(b => b.SubscriptionId == subscriptionId)
                .OrderByDescending(b => b.BillingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BillingRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.BillingRecords
                .Include(b => b.User)
                .Include(b => b.Subscription)
                .Include(b => b.Currency)
                .Where(b => b.BillingDate >= startDate && b.BillingDate <= endDate)
                .OrderByDescending(b => b.BillingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BillingRecord>> GetByStatusAsync(BillingRecord.BillingStatus status)
        {
            return await _context.BillingRecords
                .Include(b => b.User)
                .Include(b => b.Subscription)
                .Include(b => b.Currency)
                .Where(b => b.Status == status)
                .OrderByDescending(b => b.BillingDate)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all billing records with related entities
        /// </summary>
        public async Task<IEnumerable<BillingRecord>> GetAllWithDetailsAsync()
        {
            return await _context.BillingRecords
                .Include(b => b.User)
                .Include(b => b.Subscription)
                .Include(b => b.Currency)
                .ToListAsync();
        }

        /// <summary>
        /// Gets all billing records for admin dashboard aggregation (Phase 2)
        /// WARNING: Returns all records - use with caution for large datasets
        /// </summary>
        public async Task<IEnumerable<BillingRecord>> GetAllBillingRecordsAsync()
        {
            return await _context.BillingRecords
                .Include(b => b.User)
                .Include(b => b.Subscription)
                .Include(b => b.Currency)
                .AsNoTracking()  // Performance optimization - no change tracking needed
                .ToListAsync();
        }

        /// <summary>
        /// Creates a new billing record
        /// </summary>
        public async Task<BillingRecord> CreateBillingRecordAsync(BillingRecord billingRecord)
        {
            return await base.CreateAsync(billingRecord);
        }

        /// <summary>
        /// Updates an existing billing record
        /// </summary>
        public async Task<BillingRecord> UpdateBillingRecordAsync(BillingRecord billingRecord)
        {
            return await base.UpdateAsync(billingRecord);
        }

        /// <summary>
        /// Deletes a billing record by its unique identifier (hard delete)
        /// </summary>
        public async Task<bool> DeleteBillingRecordAsync(Guid billingId)
        {
            var billingRecord = await _context.BillingRecords.FindAsync(billingId);
            if (billingRecord == null)
                return false;

            _context.BillingRecords.Remove(billingRecord);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Checks if a billing record exists
        /// </summary>
        public async Task<bool> ExistsBillingRecordAsync(Guid billingId)
        {
            return await _context.BillingRecords.AnyAsync(b => b.Id == billingId);
        }

        public async Task<BillingRecord?> GetByInvoiceNumberAsync(string invoiceNumber)
        {
            return await _context.BillingRecords
                .Include(b => b.User)
                .Include(b => b.Subscription)
                .Include(b => b.Currency)
                .FirstOrDefaultAsync(b => b.InvoiceNumber == invoiceNumber);
        }

        public async Task<IEnumerable<BillingRecord>> GetInvoicesByUserIdAsync(int userId, int page, int pageSize)
        {
            return await _context.BillingRecords
                .Include(b => b.Subscription)
                .Include(b => b.Currency)
                .Where(b => b.UserId == userId && !string.IsNullOrEmpty(b.InvoiceNumber))
                .OrderByDescending(b => b.BillingDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetInvoiceCountByUserIdAsync(int userId)
        {
            return await _context.BillingRecords
                .Where(b => b.UserId == userId && !string.IsNullOrEmpty(b.InvoiceNumber))
                .CountAsync();
        }

        /// <summary>
        /// Retrieves all overdue billing records that require immediate attention
        /// </summary>
        public async Task<IEnumerable<BillingRecord>> GetOverdueBillingRecordsAsync()
        {
            var currentDate = DateTime.UtcNow;
            return await _context.BillingRecords
                .Include(b => b.User)
                .Include(b => b.Subscription)
                .Include(b => b.Currency)
                .Where(b => b.DueDate.HasValue && 
                           b.DueDate.Value < currentDate && 
                           b.Status == BillingRecord.BillingStatus.Pending &&
                           !b.IsDeleted)
                .OrderBy(b => b.DueDate)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all billing records with pending payment status
        /// </summary>
        public async Task<IEnumerable<BillingRecord>> GetPendingBillingRecordsAsync()
        {
            return await _context.BillingRecords
                .Include(b => b.User)
                .Include(b => b.Subscription)
                .Include(b => b.Currency)
                .Where(b => b.Status == BillingRecord.BillingStatus.Pending && !b.IsDeleted)
                .OrderBy(b => b.DueDate)
                .ToListAsync();
        }



        public async Task<IEnumerable<BillingRecord>> GetBillingRecordsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.BillingRecords
                .Include(b => b.User)
                .Include(b => b.Subscription)
                .Include(b => b.Currency)
                .Where(b => b.BillingDate >= startDate && b.BillingDate <= endDate)
                .OrderByDescending(b => b.BillingDate)
                .ToListAsync();
        }

        // Additional methods needed by BillingService
        public async Task<IEnumerable<BillingAdjustment>> GetAdjustmentsByBillingRecordIdAsync(Guid billingRecordId)
        {
            return await _context.BillingAdjustments
                .Where(ba => ba.BillingRecordId == billingRecordId)
                .ToListAsync();
        }

        public async Task<IEnumerable<BillingRecord>> GetByBillingCycleIdAsync(Guid billingCycleId)
        {
            return await _context.BillingRecords
                .Include(b => b.User)
                .Include(b => b.Subscription)
                .Include(b => b.Currency)
                .Where(b => b.BillingCycleId == billingCycleId)
                .OrderByDescending(b => b.BillingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BillingRecord>> GetOverdueRecordsAsync()
        {
            return await _context.BillingRecords
                .Include(b => b.User)
                .Include(b => b.Subscription)
                .Include(b => b.Currency)
                .Where(b => b.Status == BillingRecord.BillingStatus.Pending && b.DueDate < DateTime.UtcNow)
                .OrderBy(b => b.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BillingRecord>> GetPendingRecordsAsync()
        {
            return await _context.BillingRecords
                .Include(b => b.User)
                .Include(b => b.Subscription)
                .Include(b => b.Currency)
                .Where(b => b.Status == BillingRecord.BillingStatus.Pending)
                .OrderBy(b => b.DueDate)
                .ToListAsync();
        }
        
        // Webhook support methods
        public async Task<BillingRecord?> GetByStripePaymentIntentIdAsync(string stripePaymentIntentId)
        {
            return await _context.BillingRecords
                .Include(b => b.User)
                .Include(b => b.Subscription)
                .Include(b => b.Currency)
                .FirstOrDefaultAsync(b => b.StripePaymentIntentId == stripePaymentIntentId);
        }

        public async Task<BillingRecord?> GetByStripeInvoiceIdAsync(string stripeInvoiceId)
        {
            return await _context.BillingRecords
                .Include(b => b.User)
                .Include(b => b.Subscription)
                .Include(b => b.Currency)
                .FirstOrDefaultAsync(b => b.StripeInvoiceId == stripeInvoiceId);
        }

        public async Task<BillingRecord?> GetByTransactionIdAsync(string transactionId)
        {
            return await _context.BillingRecords
                .Include(b => b.User)
                .Include(b => b.Subscription)
                .Include(b => b.Currency)
                .FirstOrDefaultAsync(b => b.TransactionId == transactionId);
        }

        /// <summary>
        /// Retrieves billing records with comprehensive filtering using filter DTO
        /// </summary>
        public async Task<(IEnumerable<BillingRecord> BillingRecords, int TotalCount)> GetBillingRecordsWithAdvancedFilteringAsync(BillingFilterDto filter)
        {
            var query = _context.BillingRecords
                .Include(b => b.User)
                .Include(b => b.Subscription)
                    .ThenInclude(s => s.SubscriptionPlan)
                .Include(b => b.Currency)
                .Include(b => b.Adjustments)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower();
                query = query.Where(b => 
                    (b.InvoiceNumber != null && b.InvoiceNumber.ToLower().Contains(term)) ||
                    (b.Description != null && b.Description.ToLower().Contains(term)) ||
                    b.User.Email.ToLower().Contains(term) ||
                    (b.TransactionId != null && b.TransactionId.ToLower().Contains(term)) ||
                    (b.StripeInvoiceId != null && b.StripeInvoiceId.ToLower().Contains(term)) ||
                    (b.StripePaymentIntentId != null && b.StripePaymentIntentId.ToLower().Contains(term)));
            }

            // Apply ID filters
            if (filter.BillingRecordId.HasValue)
            {
                query = query.Where(b => b.Id == filter.BillingRecordId.Value);
            }

            if (filter.SubscriptionId.HasValue)
            {
                query = query.Where(b => b.SubscriptionId == filter.SubscriptionId.Value);
            }

            if (filter.UserId.HasValue)
            {
                query = query.Where(b => b.UserId == filter.UserId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.UserEmail))
            {
                var email = filter.UserEmail.ToLower();
                query = query.Where(b => b.User.Email.ToLower().Contains(email));
            }

            // Apply status filters
            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(b => b.Status.ToString() == filter.Status);
            }

            if (filter.Statuses != null && filter.Statuses.Any())
            {
                var statusValues = filter.Statuses.Select(s => Enum.Parse<BillingRecord.BillingStatus>(s)).ToList();
                query = query.Where(b => statusValues.Contains(b.Status));
            }

            // Apply type filters
            if (!string.IsNullOrWhiteSpace(filter.Type))
            {
                query = query.Where(b => b.Type.ToString() == filter.Type);
            }

            if (filter.Types != null && filter.Types.Any())
            {
                var typeValues = filter.Types.Select(t => Enum.Parse<BillingRecord.BillingType>(t)).ToList();
                query = query.Where(b => typeValues.Contains(b.Type));
            }

            // Apply boolean status filters
            if (filter.IsActive.HasValue)
            {
                query = query.Where(b => b.IsActive == filter.IsActive.Value);
            }

            if (filter.IsPaid.HasValue)
            {
                query = query.Where(b => b.IsPaid == filter.IsPaid.Value);
            }

            if (filter.IsOverdue.HasValue)
            {
                var now = DateTime.UtcNow;
                if (filter.IsOverdue.Value)
                {
                    query = query.Where(b => b.DueDate < now && !b.IsPaid);
                }
                else
                {
                    query = query.Where(b => b.DueDate >= now || b.IsPaid);
                }
            }

            if (filter.IsPending.HasValue)
            {
                if (filter.IsPending.Value)
                {
                    query = query.Where(b => b.Status == BillingRecord.BillingStatus.Pending);
                }
                else
                {
                    query = query.Where(b => b.Status != BillingRecord.BillingStatus.Pending);
                }
            }

            if (filter.IsFailed.HasValue)
            {
                if (filter.IsFailed.Value)
                {
                    query = query.Where(b => b.Status == BillingRecord.BillingStatus.Failed);
                }
                else
                {
                    query = query.Where(b => b.Status != BillingRecord.BillingStatus.Failed);
                }
            }

            if (filter.IsRefunded.HasValue)
            {
                if (filter.IsRefunded.Value)
                {
                    query = query.Where(b => b.Status == BillingRecord.BillingStatus.Refunded);
                }
                else
                {
                    query = query.Where(b => b.Status != BillingRecord.BillingStatus.Refunded);
                }
            }

            // Apply amount filters
            if (filter.MinAmount.HasValue)
            {
                query = query.Where(b => b.Amount >= filter.MinAmount.Value);
            }

            if (filter.MaxAmount.HasValue)
            {
                query = query.Where(b => b.Amount <= filter.MaxAmount.Value);
            }

            if (filter.ExactAmount.HasValue)
            {
                query = query.Where(b => b.Amount == filter.ExactAmount.Value);
            }

            if (filter.CurrencyId.HasValue)
            {
                query = query.Where(b => b.CurrencyId == filter.CurrencyId.Value);
            }

            // Apply date range filters
            if (filter.CreatedDateFrom.HasValue)
            {
                query = query.Where(b => b.CreatedDate >= filter.CreatedDateFrom.Value);
            }

            if (filter.CreatedDateTo.HasValue)
            {
                query = query.Where(b => b.CreatedDate <= filter.CreatedDateTo.Value);
            }

            if (filter.UpdatedDateFrom.HasValue)
            {
                query = query.Where(b => b.UpdatedDate >= filter.UpdatedDateFrom.Value);
            }

            if (filter.UpdatedDateTo.HasValue)
            {
                query = query.Where(b => b.UpdatedDate <= filter.UpdatedDateTo.Value);
            }

            if (filter.DueDateFrom.HasValue)
            {
                query = query.Where(b => b.DueDate >= filter.DueDateFrom.Value);
            }

            if (filter.DueDateTo.HasValue)
            {
                query = query.Where(b => b.DueDate <= filter.DueDateTo.Value);
            }

            if (filter.PaidDateFrom.HasValue)
            {
                query = query.Where(b => b.PaidAt >= filter.PaidDateFrom.Value);
            }

            if (filter.PaidDateTo.HasValue)
            {
                query = query.Where(b => b.PaidAt <= filter.PaidDateTo.Value);
            }

            if (filter.ProcessedDateFrom.HasValue)
            {
                query = query.Where(b => b.ProcessedAt >= filter.ProcessedDateFrom.Value);
            }

            if (filter.ProcessedDateTo.HasValue)
            {
                query = query.Where(b => b.ProcessedAt <= filter.ProcessedDateTo.Value);
            }

            // Apply payment method filters
            if (!string.IsNullOrWhiteSpace(filter.PaymentMethod))
            {
                query = query.Where(b => b.PaymentMethod == filter.PaymentMethod);
            }

            if (!string.IsNullOrWhiteSpace(filter.PaymentStatus))
            {
                query = query.Where(b => b.Status.ToString() == filter.PaymentStatus);
            }

            if (!string.IsNullOrWhiteSpace(filter.PaymentMethodType))
            {
                query = query.Where(b => b.PaymentMethod == filter.PaymentMethodType);
            }

            if (!string.IsNullOrWhiteSpace(filter.TransactionId))
            {
                query = query.Where(b => b.TransactionId == filter.TransactionId);
            }

            if (!string.IsNullOrWhiteSpace(filter.StripeInvoiceId))
            {
                query = query.Where(b => b.StripeInvoiceId == filter.StripeInvoiceId);
            }

            if (!string.IsNullOrWhiteSpace(filter.StripePaymentIntentId))
            {
                query = query.Where(b => b.StripePaymentIntentId == filter.StripePaymentIntentId);
            }

            if (!string.IsNullOrWhiteSpace(filter.StripeChargeId))
            {
                query = query.Where(b => b.TransactionId == filter.StripeChargeId);
            }

            // Apply Stripe integration filters
            if (filter.HasStripeIntegration.HasValue)
            {
                if (filter.HasStripeIntegration.Value)
                {
                    query = query.Where(b => !string.IsNullOrEmpty(b.StripeInvoiceId) || !string.IsNullOrEmpty(b.StripePaymentIntentId));
                }
                else
                {
                    query = query.Where(b => string.IsNullOrEmpty(b.StripeInvoiceId) && string.IsNullOrEmpty(b.StripePaymentIntentId));
                }
            }

            if (filter.HasPaymentMethod.HasValue)
            {
                if (filter.HasPaymentMethod.Value)
                {
                    query = query.Where(b => !string.IsNullOrEmpty(b.PaymentMethod));
                }
                else
                {
                    query = query.Where(b => string.IsNullOrEmpty(b.PaymentMethod));
                }
            }

            if (filter.HasTransactionId.HasValue)
            {
                if (filter.HasTransactionId.Value)
                {
                    query = query.Where(b => !string.IsNullOrEmpty(b.TransactionId));
                }
                else
                {
                    query = query.Where(b => string.IsNullOrEmpty(b.TransactionId));
                }
            }

            // Apply list filters
            if (filter.BillingRecordIds != null && filter.BillingRecordIds.Any())
            {
                query = query.Where(b => filter.BillingRecordIds.Contains(b.Id));
            }

            if (filter.ExcludeBillingRecordIds != null && filter.ExcludeBillingRecordIds.Any())
            {
                query = query.Where(b => !filter.ExcludeBillingRecordIds.Contains(b.Id));
            }

            if (filter.SubscriptionIds != null && filter.SubscriptionIds.Any())
            {
                query = query.Where(b => b.SubscriptionId.HasValue && filter.SubscriptionIds.Contains(b.SubscriptionId.Value));
            }

            if (filter.UserIds != null && filter.UserIds.Any())
            {
                query = query.Where(b => filter.UserIds.Contains(b.UserId));
            }

            // Apply retry and failure count filters
            // Note: RetryCount and FailureCount properties don't exist in BillingRecord entity
            // These filters are commented out as they're not applicable to the current entity structure
            // if (filter.MinRetryCount.HasValue)
            // {
            //     query = query.Where(b => b.RetryCount >= filter.MinRetryCount.Value);
            // }

            // if (filter.MaxRetryCount.HasValue)
            // {
            //     query = query.Where(b => b.RetryCount <= filter.MaxRetryCount.Value);
            // }

            // if (filter.MinFailureCount.HasValue)
            // {
            //     query = query.Where(b => b.FailureCount >= filter.MinFailureCount.Value);
            // }

            // if (filter.MaxFailureCount.HasValue)
            // {
            //     query = query.Where(b => b.FailureCount <= filter.MaxFailureCount.Value);
            // }

            // Apply text filters
            if (!string.IsNullOrWhiteSpace(filter.FailureReason))
            {
                var reason = filter.FailureReason.ToLower();
                query = query.Where(b => b.FailureReason.ToLower().Contains(reason));
            }

            // Note: Notes property doesn't exist in BillingRecord entity
            // This filter is commented out as it's not applicable to the current entity structure
            // if (!string.IsNullOrWhiteSpace(filter.Notes))
            // {
            //     var notes = filter.Notes.ToLower();
            //     query = query.Where(b => b.Notes.ToLower().Contains(notes));
            // }

            if (!string.IsNullOrWhiteSpace(filter.Description))
            {
                var description = filter.Description.ToLower();
                query = query.Where(b => b.Description.ToLower().Contains(description));
            }

            // Apply billing type filters
            if (filter.IsRecurring.HasValue)
            {
                if (filter.IsRecurring.Value)
                {
                    query = query.Where(b => b.Type == BillingRecord.BillingType.Recurring);
                }
                else
                {
                    query = query.Where(b => b.Type != BillingRecord.BillingType.Recurring);
                }
            }

            if (filter.IsOneTime.HasValue)
            {
                if (filter.IsOneTime.Value)
                {
                    query = query.Where(b => b.Type == BillingRecord.BillingType.Upfront);
                }
                else
                {
                    query = query.Where(b => b.Type != BillingRecord.BillingType.Upfront);
                }
            }

            if (filter.IsAdjustment.HasValue)
            {
                if (filter.IsAdjustment.Value)
                {
                    query = query.Where(b => b.Type == BillingRecord.BillingType.Refund);
                }
                else
                {
                    query = query.Where(b => b.Type != BillingRecord.BillingType.Refund);
                }
            }

            if (filter.IsRefund.HasValue)
            {
                if (filter.IsRefund.Value)
                {
                    query = query.Where(b => b.Type == BillingRecord.BillingType.Refund);
                }
                else
                {
                    query = query.Where(b => b.Type != BillingRecord.BillingType.Refund);
                }
            }

            // Apply billing cycle filters
            // Note: BillingRecord doesn't have a BillingCycle navigation property
            // This filter is commented out as it's not applicable to the current entity structure
            // if (!string.IsNullOrWhiteSpace(filter.BillingCycle))
            // {
            //     var cycle = filter.BillingCycle.ToLower();
            //     query = query.Where(b => b.BillingCycle.ToLower().Contains(cycle));
            // }

            if (filter.BillingCycleId.HasValue)
            {
                query = query.Where(b => b.BillingCycleId == filter.BillingCycleId.Value);
            }

            // Apply retry date filters
            // Note: LastRetryDate and NextRetryDate properties don't exist in BillingRecord entity
            // These filters are commented out as they're not applicable to the current entity structure
            // if (filter.LastRetryDateFrom.HasValue)
            // {
            //     query = query.Where(b => b.LastRetryDate >= filter.LastRetryDateFrom.Value);
            // }

            // if (filter.LastRetryDateTo.HasValue)
            // {
            //     query = query.Where(b => b.LastRetryDate <= filter.LastRetryDateTo.Value);
            // }

            // if (filter.NextRetryDateFrom.HasValue)
            // {
            //     query = query.Where(b => b.NextRetryDate >= filter.NextRetryDateFrom.Value);
            // }

            // if (filter.NextRetryDateTo.HasValue)
            // {
            //     query = query.Where(b => b.NextRetryDate <= filter.NextRetryDateTo.Value);
            // }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply dynamic sorting
            query = ApplySorting(query, filter.SortColumn, filter.SortOrder);

            // Apply pagination
            var skip = (filter.Page - 1) * filter.PageSize;
            var billingRecords = await query
                .Skip(skip)
                .Take(filter.PageSize)
                .ToListAsync();

            return (billingRecords, totalCount);
        }

        private static IQueryable<BillingRecord> ApplySorting(IQueryable<BillingRecord> query, string? sortColumn, string? sortOrder)
        {
            // Default sorting if parameters are null or empty
            if (string.IsNullOrEmpty(sortColumn) || string.IsNullOrEmpty(sortOrder))
            {
                return query.OrderByDescending(b => b.CreatedDate);
            }

            return sortColumn.ToLower() switch
            {
                "createddate" => sortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(b => b.CreatedDate)
                    : query.OrderBy(b => b.CreatedDate),
                "updateddate" => sortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(b => b.UpdatedDate)
                    : query.OrderBy(b => b.UpdatedDate),
                "billingdate" => sortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(b => b.BillingDate)
                    : query.OrderBy(b => b.BillingDate),
                "duedate" => sortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(b => b.DueDate)
                    : query.OrderBy(b => b.DueDate),
                "paiddate" => sortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(b => b.PaidAt)
                    : query.OrderBy(b => b.PaidAt),
                "processeddate" => sortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(b => b.ProcessedAt)
                    : query.OrderBy(b => b.ProcessedAt),
                "amount" => sortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(b => b.Amount)
                    : query.OrderBy(b => b.Amount),
                "status" => sortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(b => b.Status)
                    : query.OrderBy(b => b.Status),
                "type" => sortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(b => b.Type)
                    : query.OrderBy(b => b.Type),
                "invoicenumber" => sortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(b => b.InvoiceNumber)
                    : query.OrderBy(b => b.InvoiceNumber),
                "useremail" => sortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(b => b.User.Email)
                    : query.OrderBy(b => b.User.Email),
                // Note: RetryCount and FailureCount properties don't exist in BillingRecord entity
                // "retrycount" => sortOrder.ToLower() == "desc" 
                //     ? query.OrderByDescending(b => b.RetryCount)
                //     : query.OrderBy(b => b.RetryCount),
                // "failurecount" => sortOrder.ToLower() == "desc" 
                //     ? query.OrderByDescending(b => b.FailureCount)
                //     : query.OrderBy(b => b.FailureCount),
                _ => query.OrderByDescending(b => b.CreatedDate)
            };
        }

        // New methods for BillingAdjustment management
        public async Task<BillingAdjustment?> GetAdjustmentByIdAsync(Guid adjustmentId)
        {
            return await _context.BillingAdjustments
                .Include(ba => ba.BillingRecord)
                .Include(ba => ba.AppliedByUser)
                .FirstOrDefaultAsync(ba => ba.Id == adjustmentId);
        }

        public async Task<BillingAdjustment> CreateAdjustmentAsync(BillingAdjustment adjustment)
        {
            _context.BillingAdjustments.Add(adjustment);
            await _context.SaveChangesAsync();
            return adjustment;
        }

        public async Task<BillingAdjustment> UpdateAdjustmentAsync(BillingAdjustment adjustment)
        {
            _context.BillingAdjustments.Update(adjustment);
            await _context.SaveChangesAsync();
            return adjustment;
        }

        // === DATABASE-LEVEL ANALYTICS AGGREGATION METHODS ===

        public async Task<int> GetFailedPaymentsCountAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.BillingRecords
                .Where(br => br.CreatedDate >= startDate && 
                            br.CreatedDate <= endDate && 
                            br.Status == BillingRecord.BillingStatus.Failed)
                .CountAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.BillingRecords
                .Where(br => br.CreatedDate >= startDate && 
                            br.CreatedDate <= endDate && 
                            br.Status == BillingRecord.BillingStatus.Paid)
                .SumAsync(br => br.TotalAmount);
        }

        public async Task<List<MonthlyRevenueData>> GetMonthlyRevenueBreakdownAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.BillingRecords
                .Where(br => br.CreatedDate.HasValue && 
                            br.CreatedDate >= startDate && 
                            br.CreatedDate <= endDate && 
                            br.Status == BillingRecord.BillingStatus.Paid)
                .GroupBy(br => new { br.CreatedDate!.Value.Year, br.CreatedDate!.Value.Month })
                .Select(g => new MonthlyRevenueData
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Revenue = g.Sum(br => br.TotalAmount),
                    SubscriptionCount = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToListAsync();
        }

        public async Task<List<CategoryRevenueData>> GetRevenueByCategoryAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.BillingRecords
                .Where(br => br.CreatedDate.HasValue && 
                            br.CreatedDate >= startDate && 
                            br.CreatedDate <= endDate && 
                            br.Status == BillingRecord.BillingStatus.Paid)
                .Include(br => br.Subscription)
                .ThenInclude(s => s.SubscriptionPlan)
                .ThenInclude(sp => sp.Category)
                .GroupBy(br => br.Subscription.SubscriptionPlan.Category.Name)
                .Select(g => new CategoryRevenueData
                {
                    CategoryName = g.Key,
                    Revenue = g.Sum(br => br.TotalAmount),
                    SubscriptionCount = g.Count()
                })
                .OrderByDescending(x => x.Revenue)
                .ToListAsync();
        }

        public async Task<decimal> GetAverageRevenuePerUserAsync(DateTime startDate, DateTime endDate)
        {
            var totalRevenue = await GetTotalRevenueAsync(startDate, endDate);
            var uniqueUsers = await _context.BillingRecords
                .Where(br => br.CreatedDate >= startDate && 
                            br.CreatedDate <= endDate && 
                            br.Status == BillingRecord.BillingStatus.Paid)
                .Select(br => br.UserId)
                .Distinct()
                .CountAsync();

            return uniqueUsers > 0 ? totalRevenue / uniqueUsers : 0;
        }

        public async Task<List<PaymentMethodAnalytics>> GetPaymentMethodAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.BillingRecords
                .Where(br => br.CreatedDate >= startDate && br.CreatedDate <= endDate)
                .GroupBy(br => br.PaymentMethod ?? "Unknown")
                .Select(g => new PaymentMethodAnalytics
                {
                    PaymentMethod = g.Key,
                    UsageCount = g.Count(),
                    TotalAmount = g.Where(br => br.Status == BillingRecord.BillingStatus.Paid).Sum(br => br.TotalAmount),
                    SuccessRate = g.Any() ? (decimal)g.Count(br => br.Status == BillingRecord.BillingStatus.Paid) / g.Count() * 100 : 0,
                    AverageAmount = g.Any() ? g.Average(br => br.TotalAmount) : 0
                })
                .OrderByDescending(x => x.UsageCount)
                .ToListAsync();
        }

        public async Task<List<BillingStatusAnalytics>> GetBillingStatusAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            var totalRecords = await _context.BillingRecords
                .Where(br => br.CreatedDate >= startDate && br.CreatedDate <= endDate)
                .CountAsync();

            return await _context.BillingRecords
                .Where(br => br.CreatedDate >= startDate && br.CreatedDate <= endDate)
                .GroupBy(br => br.Status)
                .Select(g => new BillingStatusAnalytics
                {
                    Status = g.Key.ToString(),
                    Count = g.Count(),
                    TotalAmount = g.Sum(br => br.TotalAmount),
                    Percentage = totalRecords > 0 ? (decimal)g.Count() / totalRecords * 100 : 0
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();
        }

        public async Task<decimal> GetPaymentSuccessRateAsync(DateTime startDate, DateTime endDate)
        {
            var totalPayments = await _context.BillingRecords
                .Where(br => br.CreatedDate >= startDate && br.CreatedDate <= endDate)
                .CountAsync();

            if (totalPayments == 0) return 0;

            var successfulPayments = await _context.BillingRecords
                .Where(br => br.CreatedDate >= startDate && 
                            br.CreatedDate <= endDate && 
                            br.Status == BillingRecord.BillingStatus.Paid)
                .CountAsync();

            return (decimal)successfulPayments / totalPayments * 100;
        }

        public async Task<List<RevenueTrendData>> GetRevenueTrendAsync(DateTime startDate, DateTime endDate)
        {
            var monthlyData = await _context.BillingRecords
                .Where(br => br.CreatedDate.HasValue && 
                            br.CreatedDate >= startDate && 
                            br.CreatedDate <= endDate && 
                            br.Status == BillingRecord.BillingStatus.Paid)
                .GroupBy(br => new { br.CreatedDate!.Value.Year, br.CreatedDate!.Value.Month })
                .Select(g => new { 
                    Period = $"{g.Key.Year}-{g.Key.Month:D2}", 
                    Revenue = g.Sum(br => br.TotalAmount),
                    BillingCount = g.Count()
                })
                .OrderBy(x => x.Period)
                .ToListAsync();

            var trendData = new List<RevenueTrendData>();
            decimal? previousRevenue = null;

            foreach (var data in monthlyData)
            {
                var growthRate = previousRevenue.HasValue && previousRevenue.Value > 0
                    ? (data.Revenue - previousRevenue.Value) / previousRevenue.Value * 100
                    : 0;

                trendData.Add(new RevenueTrendData
                {
                    Period = data.Period,
                    Revenue = data.Revenue,
                    SubscriptionCount = data.BillingCount,
                    GrowthRate = Math.Round(growthRate, 2)
                });

                previousRevenue = data.Revenue;
            }

            return trendData;
        }

        public async Task<OverageChargesAnalytics> GetOverageChargesAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            var overageRecords = await _context.BillingRecords
                .Where(br => br.CreatedDate >= startDate && 
                            br.CreatedDate <= endDate && 
                            br.Description.Contains("overage", StringComparison.OrdinalIgnoreCase))
                .Include(br => br.Subscription)
                .ThenInclude(s => s.SubscriptionPlan)
                .ToListAsync();

            var overageByPlan = overageRecords
                .GroupBy(br => br.Subscription?.SubscriptionPlan?.Name ?? "Unknown")
                .Select(g => new OverageByPlanDto
                {
                    PlanName = g.Key,
                    OverageAmount = g.Sum(br => br.TotalAmount),
                    OverageCount = g.Count()
                })
                .ToList();

            var overageTrend = overageRecords
                .Where(br => br.CreatedDate.HasValue)
                .GroupBy(br => new { br.CreatedDate!.Value.Year, br.CreatedDate!.Value.Month })
                .Select(g => new OverageTrendDto
                {
                    Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                    OverageAmount = g.Sum(br => br.TotalAmount),
                    OverageCount = g.Count()
                })
                .OrderBy(x => x.Period)
                .ToList();

            return new OverageChargesAnalytics
            {
                TotalOverageCharges = overageRecords.Sum(br => br.TotalAmount),
                OverageCount = overageRecords.Count,
                AverageOverageAmount = overageRecords.Any() ? overageRecords.Average(br => br.TotalAmount) : 0,
                OverageByPlan = overageByPlan,
                OverageTrend = overageTrend
            };
        }

        public async Task<BillingEfficiencyMetrics> GetBillingEfficiencyMetricsAsync(DateTime startDate, DateTime endDate)
        {
            var records = await _context.BillingRecords
                .Where(br => br.CreatedDate >= startDate && br.CreatedDate <= endDate)
                .ToListAsync();

            var totalRecords = records.Count;
            var successfulRecords = records.Count(br => br.Status == BillingRecord.BillingStatus.Paid);
            var failedRecords = records.Count(br => br.Status == BillingRecord.BillingStatus.Failed);

            var overallEfficiency = totalRecords > 0 ? (decimal)successfulRecords / totalRecords * 100 : 0;
            var paymentSuccessRate = overallEfficiency;
            var revenueRecoveryRate = failedRecords > 0 ? (decimal)successfulRecords / (successfulRecords + failedRecords) * 100 : 100;

            var averageBillingCycleTime = (decimal)records
                .Where(br => br.Status == BillingRecord.BillingStatus.Paid && br.PaidAt.HasValue && br.CreatedDate.HasValue)
                .Select(br => (br.PaidAt!.Value - br.CreatedDate!.Value).TotalDays)
                .DefaultIfEmpty(0)
                .Average();

            var efficiencyByMethod = records
                .GroupBy(br => br.PaymentMethod ?? "Unknown")
                .Select(g => new BillingEfficiencyByMethodDto
                {
                    PaymentMethod = g.Key,
                    Efficiency = g.Any() ? (decimal)g.Count(br => br.Status == BillingRecord.BillingStatus.Paid) / g.Count() * 100 : 0,
                    SuccessRate = g.Any() ? (decimal)g.Count(br => br.Status == BillingRecord.BillingStatus.Paid) / g.Count() * 100 : 0,
                    AverageProcessingTime = (decimal)g.Where(br => br.Status == BillingRecord.BillingStatus.Paid && br.PaidAt.HasValue && br.CreatedDate.HasValue)
                        .Select(br => (br.PaidAt!.Value - br.CreatedDate!.Value).TotalDays)
                        .DefaultIfEmpty(0)
                        .Average()
                })
                .ToList();

            return new BillingEfficiencyMetrics
            {
                OverallEfficiency = Math.Round(overallEfficiency, 2),
                PaymentSuccessRate = Math.Round(paymentSuccessRate, 2),
                AverageBillingCycleTime = Math.Round((decimal)averageBillingCycleTime, 2),
                RevenueRecoveryRate = Math.Round(revenueRecoveryRate, 2),
                EfficiencyByMethod = efficiencyByMethod
            };
        }

        // === END DATABASE-LEVEL ANALYTICS AGGREGATION METHODS ===
    }
} 