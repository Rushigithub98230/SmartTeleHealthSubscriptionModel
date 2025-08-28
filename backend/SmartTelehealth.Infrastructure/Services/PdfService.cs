using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace SmartTelehealth.Infrastructure.Services;

public class PdfService : IPdfService
{
    private readonly ILogger<PdfService> _logger;

    public PdfService(ILogger<PdfService> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(BillingRecordDto billingRecord, UserDto user, SubscriptionDto? subscription = null)
    {
        try
        {
            return await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header().Element(ComposeHeader);
                        page.Content().Element(x => ComposeInvoiceContent(x, billingRecord, user, subscription));
                        page.Footer().Element(ComposeFooter);
                    });
                });

                return document.GeneratePdf();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice PDF for billing record {BillingRecordId}", billingRecord.Id);
            throw;
        }
    }

    public async Task<byte[]> GenerateSubscriptionSummaryPdfAsync(SubscriptionDto subscription, UserDto user)
    {
        try
        {
            return await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header().Element(ComposeHeader);
                        page.Content().Element(x => ComposeSubscriptionSummaryContent(x, subscription, user));
                        page.Footer().Element(ComposeFooter);
                    });
                });

                return document.GeneratePdf();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating subscription summary PDF for subscription {SubscriptionId}", subscription.Id);
            throw;
        }
    }

    public async Task<byte[]> GenerateBillingHistoryPdfAsync(IEnumerable<BillingRecordDto> billingRecords, UserDto user)
    {
        try
        {
            return await Task.Run(() =>
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header().Element(ComposeHeader);
                        page.Content().Element(x => ComposeBillingHistoryContent(x, billingRecords, user));
                        page.Footer().Element(ComposeFooter);
                    });
                });

                return document.GeneratePdf();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating billing history PDF for user {UserId}", user.Id);
            throw;
        }
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("Smart Telehealth").FontSize(20).Bold();
                column.Item().Text("Healthcare Made Simple").FontSize(12).FontColor(Colors.Grey.Medium);
            });

            row.ConstantItem(100).Column(column =>
            {
                column.Item().AlignRight().Text("Invoice").FontSize(16).Bold();
                column.Item().AlignRight().Text(DateTime.Now.ToString("MMM dd, yyyy")).FontSize(10);
            });
        });
    }

    private void ComposeInvoiceContent(IContainer container, BillingRecordDto billingRecord, UserDto user, SubscriptionDto? subscription)
    {
        container.PaddingVertical(20).Column(column =>
        {
            // Bill To Section
            column.Item().Text("Bill To:").FontSize(12).Bold();
            column.Item().Text($"{user.FirstName} {user.LastName}").FontSize(10);
            column.Item().Text(user.Email ?? "").FontSize(10);
            column.Item().PaddingBottom(20);

            // Invoice Details
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(1);
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Text("Description").Bold();
                    header.Cell().AlignRight().Text("Amount").Bold();
                });

                // Content
                table.Cell().Text(billingRecord.Description ?? "Subscription Payment");
                table.Cell().AlignRight().Text($"${billingRecord.Amount:F2}");

                // Total Row
                table.Cell().Text("Total:").Bold();
                table.Cell().AlignRight().Text($"${billingRecord.Amount:F2}").Bold();
            });

            // Additional Information
            if (subscription != null)
            {
                column.Item().PaddingTop(20).Text("Subscription Details:").FontSize(12).Bold();
                column.Item().Text($"Plan: {subscription.PlanName}").FontSize(10);
                column.Item().Text($"Status: {subscription.Status}").FontSize(10);
                column.Item().Text($"Next Billing: {subscription.NextBillingDate:MMM dd, yyyy}").FontSize(10);
            }

            // Payment Information
            column.Item().PaddingTop(20).Text("Payment Information:").FontSize(12).Bold();
            column.Item().Text($"Invoice ID: {billingRecord.Id}").FontSize(10);
            column.Item().Text($"Payment Date: {billingRecord.PaidDate?.ToString("MMM dd, yyyy") ?? "Pending"}").FontSize(10);
            column.Item().Text($"Status: {billingRecord.BillingStatusName}").FontSize(10);
            if (!string.IsNullOrEmpty(billingRecord.StripePaymentIntentId))
            {
                column.Item().Text($"Payment Intent: {billingRecord.StripePaymentIntentId}").FontSize(10);
            }
        });
    }

    private void ComposeSubscriptionSummaryContent(IContainer container, SubscriptionDto subscription, UserDto user)
    {
        container.PaddingVertical(20).Column(column =>
        {
            column.Item().Text("Subscription Summary").FontSize(16).Bold();
            column.Item().PaddingTop(10);

            // User Information
            column.Item().Text("Subscriber:").FontSize(12).Bold();
            column.Item().Text($"{user.FirstName} {user.LastName}").FontSize(10);
            column.Item().Text(user.Email ?? "").FontSize(10);
            column.Item().PaddingBottom(20);

            // Subscription Details
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                table.Cell().Text("Plan Name:").Bold();
                table.Cell().Text(subscription.PlanName);

                table.Cell().Text("Status:").Bold();
                table.Cell().Text(subscription.Status.ToString());

                table.Cell().Text("Start Date:").Bold();
                table.Cell().Text(subscription.StartDate.ToString("MMM dd, yyyy"));

                table.Cell().Text("Next Billing:").Bold();
                table.Cell().Text(subscription.NextBillingDate.ToString("MMM dd, yyyy"));

                table.Cell().Text("Current Price:").Bold();
                table.Cell().Text($"${subscription.CurrentPrice:F2}");

                table.Cell().Text("Billing Frequency:").Bold();
                table.Cell().Text(subscription.BillingCycleId.ToString());
            });

            // Usage Statistics
            column.Item().PaddingTop(20).Text("Usage Statistics:").FontSize(12).Bold();
            // Remove table rows for Consultations Used, Messages Used, Next Delivery
            // Optionally, add privilege-based usage summary here
        });
    }

    private void ComposeBillingHistoryContent(IContainer container, IEnumerable<BillingRecordDto> billingRecords, UserDto user)
    {
        container.PaddingVertical(20).Column(column =>
        {
            column.Item().Text("Billing History").FontSize(16).Bold();
            column.Item().PaddingTop(10);

            // User Information
            column.Item().Text("Customer:").FontSize(12).Bold();
            column.Item().Text($"{user.FirstName} {user.LastName}").FontSize(10);
            column.Item().Text(user.Email ?? "").FontSize(10);
            column.Item().PaddingBottom(20);

            // Billing Records Table
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Text("Date").Bold();
                    header.Cell().AlignRight().Text("Amount").Bold();
                    header.Cell().AlignRight().Text("Status").Bold();
                    header.Cell().AlignRight().Text("Type").Bold();
                });

                // Content
                foreach (var record in billingRecords.OrderByDescending(r => r.CreatedAt))
                {
                    table.Cell().Text(record.CreatedAt.ToString("MMM dd, yyyy"));
                    table.Cell().AlignRight().Text($"${record.Amount:F2}");
                    table.Cell().AlignRight().Text(record.BillingStatusName);
                    table.Cell().AlignRight().Text("Subscription"); // Use default type since BillingRecordDto doesn't have Type
                }
            });

            // Summary
            var totalPaid = billingRecords.Where(r => r.BillingStatusName == "Paid").Sum(r => r.Amount);
            var totalPending = billingRecords.Where(r => r.BillingStatusName == "Pending").Sum(r => r.Amount);

            column.Item().PaddingTop(20).Text("Summary:").FontSize(12).Bold();
            column.Item().Text($"Total Paid: ${totalPaid:F2}").FontSize(10);
            column.Item().Text($"Total Pending: ${totalPending:F2}").FontSize(10);
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("Smart Telehealth").FontSize(10).Bold();
                column.Item().Text("123 Healthcare Ave, Medical City, MC 12345").FontSize(8);
                column.Item().Text("support@smarttelehealth.com | (555) 123-4567").FontSize(8);
            });

            row.ConstantItem(100).Column(column =>
            {
                column.Item().AlignRight().Text($"Generated: {DateTime.Now:MMM dd, yyyy HH:mm}").FontSize(8);
            });
        });
    }
} 