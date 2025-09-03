using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Core.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using System.Text;

namespace SmartTelehealth.Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IBillingRepository _billingRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUserRepository _userRepository;
      
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(
        IBillingRepository billingRepository,
        ISubscriptionRepository subscriptionRepository,
        IUserRepository userRepository,
          
        ILogger<InvoiceService> logger)
    {
        _billingRepository = billingRepository;
        _subscriptionRepository = subscriptionRepository;
        _userRepository = userRepository;
          
        _logger = logger;
    }

    public async Task<JsonModel> GenerateInvoiceAsync(string billingRecordId, TokenModel tokenModel)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByIdAsync(Guid.Parse(billingRecordId));
            if (billingRecord == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Billing record not found",
                    StatusCode = 404
                };
            }

            var user = await _userRepository.GetByIdAsync(billingRecord.UserId);
            if (user == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "User not found",
                    StatusCode = 404
                };
            }

            // Generate invoice number
            var invoiceNumber = await GenerateInvoiceNumberAsync();
            
            // Create invoice content
            var invoiceContent = await GenerateInvoiceContentAsync(billingRecord, user, invoiceNumber);
            
            // Update billing record with invoice number
            billingRecord.InvoiceNumber = invoiceNumber;
            await _billingRepository.UpdateAsync(billingRecord);
            await _billingRepository.SaveChangesAsync();

            

            var result = new
            {
                InvoiceNumber = invoiceNumber,
                BillingRecordId = billingRecordId,
                UserId = billingRecord.UserId,
                Amount = billingRecord.TotalAmount,
                GeneratedAt = DateTime.UtcNow,
                GeneratedBy = tokenModel.UserID
            };

            return new JsonModel
            {
                data = result,
                Message = $"Invoice {invoiceNumber} generated successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice for billing record {BillingRecordId} by user {UserId}", 
                billingRecordId, tokenModel.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to generate invoice",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetInvoiceAsync(string invoiceNumber, TokenModel tokenModel)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByInvoiceNumberAsync(invoiceNumber);
            if (billingRecord == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Invoice not found",
                    StatusCode = 404
                };
            }

            // Check access permissions
            if (tokenModel.RoleID != 1 && tokenModel.UserID != billingRecord.UserId)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Access denied",
                    StatusCode = 403
                };
            }

            var user = await _userRepository.GetByIdAsync(billingRecord.UserId);
            var invoiceContent = await GenerateInvoiceContentAsync(billingRecord, user, invoiceNumber);

            var result = new
            {
                InvoiceNumber = invoiceNumber,
                BillingRecord = billingRecord,
                User = user,
                InvoiceContent = invoiceContent,
                GeneratedAt = billingRecord.CreatedDate
            };

            return new JsonModel
            {
                data = result,
                Message = "Invoice retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoice {InvoiceNumber} by user {UserId}", 
                invoiceNumber, tokenModel.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to retrieve invoice",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> GetUserInvoicesAsync(int userId, int page = 1, int pageSize = 20, TokenModel tokenModel = null)
    {
        try
        {
            // Check access permissions
            if (tokenModel != null && tokenModel.RoleID != 1 && tokenModel.UserID != userId)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Access denied",
                    StatusCode = 403
                };
            }

            var invoices = await _billingRepository.GetInvoicesByUserIdAsync(userId, page, pageSize);
            var totalCount = await _billingRepository.GetInvoiceCountByUserIdAsync(userId);

            var result = new
            {
                Invoices = invoices,
                Meta = new
                {
                    TotalRecords = totalCount,
                    PageSize = pageSize,
                    CurrentPage = page,
                    TotalPages = Math.Ceiling((double)totalCount / pageSize)
                }
            };

            return new JsonModel
            {
                data = result,
                Message = "User invoices retrieved successfully",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoices for user {UserId}", userId);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to retrieve user invoices",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> DownloadInvoiceAsync(string invoiceNumber, string format, TokenModel tokenModel)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByInvoiceNumberAsync(invoiceNumber);
            if (billingRecord == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Invoice not found",
                    StatusCode = 404
                };
            }

            // Check access permissions
            if (tokenModel.RoleID != 1 && tokenModel.UserID != billingRecord.UserId)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Access denied",
                    StatusCode = 403
                };
            }

            var user = await _userRepository.GetByIdAsync(billingRecord.UserId);
            var invoiceContent = await GenerateInvoiceContentAsync(billingRecord, user, invoiceNumber);

            byte[] fileContent;
            string fileName;
            string contentType;

            switch (format.ToLower())
            {
                case "pdf":
                    fileContent = await GeneratePdfInvoiceAsync(invoiceContent);
                    fileName = $"Invoice_{invoiceNumber}.pdf";
                    contentType = "application/pdf";
                    break;
                case "csv":
                    fileContent = await GenerateCsvInvoiceAsync(invoiceContent);
                    fileName = $"Invoice_{invoiceNumber}.csv";
                    contentType = "text/csv";
                    break;
                default:
                    return new JsonModel
                    {
                        data = new object(),
                        Message = "Unsupported format. Use 'pdf' or 'csv'",
                        StatusCode = 400
                    };
            }

            var result = new
            {
                FileContent = Convert.ToBase64String(fileContent),
                FileName = fileName,
                ContentType = contentType,
                FileSize = fileContent.Length
            };

            

            return new JsonModel
            {
                data = result,
                Message = $"Invoice {invoiceNumber} downloaded successfully in {format} format",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading invoice {InvoiceNumber} in {Format} format by user {UserId}", 
                invoiceNumber, format, tokenModel.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to download invoice",
                StatusCode = 500
            };
        }
    }

    public async Task<JsonModel> SendInvoiceAsync(string invoiceNumber, string email, TokenModel tokenModel)
    {
        try
        {
            var billingRecord = await _billingRepository.GetByInvoiceNumberAsync(invoiceNumber);
            if (billingRecord == null)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Invoice not found",
                    StatusCode = 404
                };
            }

            // Check access permissions
            if (tokenModel.RoleID != 1 && tokenModel.UserID != billingRecord.UserId)
            {
                return new JsonModel
                {
                    data = new object(),
                    Message = "Access denied",
                    StatusCode = 403
                };
            }

            var user = await _userRepository.GetByIdAsync(billingRecord.UserId);
            var invoiceContent = await GenerateInvoiceContentAsync(billingRecord, user, invoiceNumber);

            // Generate PDF for email
            var pdfContent = await GeneratePdfInvoiceAsync(invoiceContent);
            
           

            var result = new
            {
                InvoiceNumber = invoiceNumber,
                Email = email,
                SentAt = DateTime.UtcNow,
                SentBy = tokenModel.UserID
            };

            return new JsonModel
            {
                data = result,
                Message = $"Invoice {invoiceNumber} sent successfully to {email}",
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending invoice {InvoiceNumber} to {Email} by user {UserId}", 
                invoiceNumber, email, tokenModel.UserID);
            return new JsonModel
            {
                data = new object(),
                Message = "Failed to send invoice",
                StatusCode = 500
            };
        }
    }

    private async Task<string> GenerateInvoiceNumberAsync()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        return $"INV-{timestamp}-{random}";
    }

    private async Task<string> GenerateInvoiceContentAsync(BillingRecord billingRecord, User user, string invoiceNumber)
    {
        var sb = new StringBuilder();
        
        // Invoice Header
        sb.AppendLine("INVOICE");
        sb.AppendLine("=======");
        sb.AppendLine($"Invoice Number: {invoiceNumber}");
        sb.AppendLine($"Date: {billingRecord.BillingDate:yyyy-MM-dd}");
        sb.AppendLine($"Due Date: {billingRecord.DueDate:yyyy-MM-dd}");
        sb.AppendLine();

        // Customer Information
        sb.AppendLine("BILL TO:");
        sb.AppendLine($"{user.FirstName} {user.LastName}");
        sb.AppendLine(user.Email);
        sb.AppendLine();

        // Billing Details
        sb.AppendLine("BILLING DETAILS:");
        sb.AppendLine($"Description: {billingRecord.Description}");
        sb.AppendLine($"Amount: ${billingRecord.Amount:F2}");
        sb.AppendLine($"Tax: ${billingRecord.TaxAmount:F2}");
        sb.AppendLine($"Total: ${billingRecord.TotalAmount:F2}");
        sb.AppendLine();

        // Payment Information
        sb.AppendLine("PAYMENT INFORMATION:");
        sb.AppendLine($"Status: {billingRecord.Status}");
        if (billingRecord.PaidAt.HasValue)
        {
            sb.AppendLine($"Paid At: {billingRecord.PaidAt:yyyy-MM-dd HH:mm:ss}");
        }
        sb.AppendLine();

        // Footer
        sb.AppendLine("Thank you for your business!");

        return sb.ToString();
    }

    private async Task<byte[]> GeneratePdfInvoiceAsync(string invoiceContent)
    {
        // TODO: Implement actual PDF generation using a library like iText7 or PDFsharp
        // For now, return the content as bytes
        return Encoding.UTF8.GetBytes(invoiceContent);
    }

    private async Task<byte[]> GenerateCsvInvoiceAsync(string invoiceContent)
    {
        // Convert invoice content to CSV format
        var csvContent = invoiceContent.Replace(":", ",").Replace("\n", "\r\n");
        return Encoding.UTF8.GetBytes(csvContent);
    }
}
