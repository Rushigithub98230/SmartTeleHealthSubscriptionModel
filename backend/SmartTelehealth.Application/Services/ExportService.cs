using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Services;

/// <summary>
/// Service for exporting data in various formats (Excel, CSV, PDF)
/// </summary>
public class ExportService
{
    private readonly ILogger<ExportService> _logger;

    public ExportService(ILogger<ExportService> logger)
    {
        _logger = logger;
        
        // Set EPPlus license context
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    /// <summary>
    /// Export user analytics to Excel
    /// </summary>
    public byte[] ExportUserAnalyticsToExcel(UserAnalyticsDto analytics)
    {
        try
        {
            using var package = new ExcelPackage();
            
            // Summary Sheet
            var summarySheet = package.Workbook.Worksheets.Add("Summary");
            AddUserAnalyticsSummary(summarySheet, analytics);
            
            // Subscription Details Sheet
            var subSheet = package.Workbook.Worksheets.Add("Subscriptions");
            AddSubscriptionDetails(subSheet, analytics);
            
            // Financial Details Sheet
            var finSheet = package.Workbook.Worksheets.Add("Financial");
            AddFinancialDetails(finSheet, analytics);
            
            // Payment Details Sheet
            var paySheet = package.Workbook.Worksheets.Add("Payments");
            AddPaymentDetails(paySheet, analytics);
            
            return package.GetAsByteArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting user analytics to Excel");
            throw;
        }
    }

    /// <summary>
    /// Export user analytics to CSV
    /// </summary>
    public byte[] ExportUserAnalyticsToCsv(UserAnalyticsDto analytics)
    {
        try
        {
            var csv = new StringBuilder();
            
            // Headers
            csv.AppendLine("User Analytics Export");
            csv.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            csv.AppendLine($"User: {analytics.UserName} ({analytics.UserEmail})");
            csv.AppendLine();
            
            // Subscription Metrics
            csv.AppendLine("Subscription Metrics");
            csv.AppendLine("Metric,Value");
            csv.AppendLine($"Total Subscriptions,{analytics.TotalSubscriptions}");
            csv.AppendLine($"Active Subscriptions,{analytics.ActiveSubscriptions}");
            csv.AppendLine($"Past Subscriptions,{analytics.PastSubscriptions}");
            csv.AppendLine($"Cancelled Subscriptions,{analytics.CancelledSubscriptions}");
            csv.AppendLine($"Average Duration (Days),{analytics.AverageSubscriptionDurationDays:F2}");
            csv.AppendLine($"Current Plan,{analytics.CurrentPlan ?? "None"}");
            csv.AppendLine();
            
            // Financial Metrics
            csv.AppendLine("Financial Metrics");
            csv.AppendLine("Metric,Value");
            csv.AppendLine($"Total Revenue,${analytics.TotalRevenue:F2}");
            csv.AppendLine($"Total Paid,${analytics.TotalPaid:F2}");
            csv.AppendLine($"Total Refunded,${analytics.TotalRefunded:F2}");
            csv.AppendLine($"Average Monthly Spend,${analytics.AverageMonthlySpend:F2}");
            csv.AppendLine();
            
            // Payment Metrics
            csv.AppendLine("Payment Metrics");
            csv.AppendLine("Metric,Value");
            csv.AppendLine($"Total Payments,{analytics.TotalPayments}");
            csv.AppendLine($"Successful Payments,{analytics.SuccessfulPayments}");
            csv.AppendLine($"Failed Payments,{analytics.FailedPayments}");
            csv.AppendLine($"Success Rate,{analytics.PaymentSuccessRate:F2}%");
            csv.AppendLine();
            
            // Privilege Metrics
            csv.AppendLine("Privilege Metrics");
            csv.AppendLine("Metric,Value");
            csv.AppendLine($"Active Privileges,{analytics.ActivePrivileges}");
            csv.AppendLine($"Usage Rate,{analytics.PrivilegeUsageRate:F2}%");
            csv.AppendLine($"Has Overage,{(analytics.HasOverageCharges ? "Yes" : "No")}");
            csv.AppendLine();
            
            // Account Metrics
            csv.AppendLine("Account Metrics");
            csv.AppendLine("Metric,Value");
            csv.AppendLine($"Created Date,{analytics.AccountCreatedDate:yyyy-MM-dd}");
            csv.AppendLine($"Account Age (Days),{analytics.AccountAgeDays}");
            csv.AppendLine($"Last Login,{analytics.LastLoginDate?.ToString("yyyy-MM-dd HH:mm") ?? "Never"}");
            csv.AppendLine($"Is Active,{(analytics.IsActiveAccount ? "Yes" : "No")}");
            
            return Encoding.UTF8.GetBytes(csv.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting user analytics to CSV");
            throw;
        }
    }

    #region Private Helper Methods

    private void AddUserAnalyticsSummary(ExcelWorksheet sheet, UserAnalyticsDto analytics)
    {
        // Title
        sheet.Cells["A1"].Value = "User Analytics Summary";
        sheet.Cells["A1"].Style.Font.Size = 16;
        sheet.Cells["A1"].Style.Font.Bold = true;
        
        sheet.Cells["A2"].Value = $"User: {analytics.UserName}";
        sheet.Cells["A3"].Value = $"Email: {analytics.UserEmail}";
        sheet.Cells["A4"].Value = $"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";
        
        int row = 6;
        
        // Subscription Metrics
        sheet.Cells[$"A{row}"].Value = "Subscription Metrics";
        sheet.Cells[$"A{row}"].Style.Font.Bold = true;
        row++;
        
        sheet.Cells[$"A{row}"].Value = "Metric";
        sheet.Cells[$"B{row}"].Value = "Value";
        sheet.Cells[$"A{row}:B{row}"].Style.Font.Bold = true;
        sheet.Cells[$"A{row}:B{row}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
        sheet.Cells[$"A{row}:B{row}"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
        row++;
        
        sheet.Cells[$"A{row}"].Value = "Total Subscriptions";
        sheet.Cells[$"B{row}"].Value = analytics.TotalSubscriptions;
        row++;
        
        sheet.Cells[$"A{row}"].Value = "Active Subscriptions";
        sheet.Cells[$"B{row}"].Value = analytics.ActiveSubscriptions;
        row++;
        
        sheet.Cells[$"A{row}"].Value = "Past Subscriptions";
        sheet.Cells[$"B{row}"].Value = analytics.PastSubscriptions;
        row++;
        
        sheet.Cells[$"A{row}"].Value = "Cancelled Subscriptions";
        sheet.Cells[$"B{row}"].Value = analytics.CancelledSubscriptions;
        row++;
        
        sheet.Cells[$"A{row}"].Value = "Current Plan";
        sheet.Cells[$"B{row}"].Value = analytics.CurrentPlan ?? "None";
        row += 2;
        
        // Financial Metrics
        sheet.Cells[$"A{row}"].Value = "Financial Metrics";
        sheet.Cells[$"A{row}"].Style.Font.Bold = true;
        row++;
        
        sheet.Cells[$"A{row}"].Value = "Metric";
        sheet.Cells[$"B{row}"].Value = "Value";
        sheet.Cells[$"A{row}:B{row}"].Style.Font.Bold = true;
        sheet.Cells[$"A{row}:B{row}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
        sheet.Cells[$"A{row}:B{row}"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
        row++;
        
        sheet.Cells[$"A{row}"].Value = "Total Revenue";
        sheet.Cells[$"B{row}"].Value = analytics.TotalRevenue;
        sheet.Cells[$"B{row}"].Style.Numberformat.Format = "$#,##0.00";
        row++;
        
        sheet.Cells[$"A{row}"].Value = "Total Paid";
        sheet.Cells[$"B{row}"].Value = analytics.TotalPaid;
        sheet.Cells[$"B{row}"].Style.Numberformat.Format = "$#,##0.00";
        row++;
        
        sheet.Cells[$"A{row}"].Value = "Total Refunded";
        sheet.Cells[$"B{row}"].Value = analytics.TotalRefunded;
        sheet.Cells[$"B{row}"].Style.Numberformat.Format = "$#,##0.00";
        row++;
        
        sheet.Cells[$"A{row}"].Value = "Average Monthly Spend";
        sheet.Cells[$"B{row}"].Value = analytics.AverageMonthlySpend;
        sheet.Cells[$"B{row}"].Style.Numberformat.Format = "$#,##0.00";
        row += 2;
        
        // Payment Metrics
        sheet.Cells[$"A{row}"].Value = "Payment Metrics";
        sheet.Cells[$"A{row}"].Style.Font.Bold = true;
        row++;
        
        sheet.Cells[$"A{row}"].Value = "Metric";
        sheet.Cells[$"B{row}"].Value = "Value";
        sheet.Cells[$"A{row}:B{row}"].Style.Font.Bold = true;
        sheet.Cells[$"A{row}:B{row}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
        sheet.Cells[$"A{row}:B{row}"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);
        row++;
        
        sheet.Cells[$"A{row}"].Value = "Total Payments";
        sheet.Cells[$"B{row}"].Value = analytics.TotalPayments;
        row++;
        
        sheet.Cells[$"A{row}"].Value = "Successful Payments";
        sheet.Cells[$"B{row}"].Value = analytics.SuccessfulPayments;
        row++;
        
        sheet.Cells[$"A{row}"].Value = "Failed Payments";
        sheet.Cells[$"B{row}"].Value = analytics.FailedPayments;
        row++;
        
        sheet.Cells[$"A{row}"].Value = "Success Rate";
        sheet.Cells[$"B{row}"].Value = analytics.PaymentSuccessRate / 100;
        sheet.Cells[$"B{row}"].Style.Numberformat.Format = "0.00%";
        
        // Auto-fit columns
        sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
    }

    private void AddSubscriptionDetails(ExcelWorksheet sheet, UserAnalyticsDto analytics)
    {
        sheet.Cells["A1"].Value = "Subscription Details";
        sheet.Cells["A1"].Style.Font.Size = 14;
        sheet.Cells["A1"].Style.Font.Bold = true;
        
        int row = 3;
        sheet.Cells[$"A{row}"].Value = "Metric";
        sheet.Cells[$"B{row}"].Value = "Value";
        sheet.Cells[$"A{row}:B{row}"].Style.Font.Bold = true;
        row++;
        
        sheet.Cells[$"A{row}"].Value = "Total Subscriptions";
        sheet.Cells[$"B{row}"].Value = analytics.TotalSubscriptions;
        row++;
        
        sheet.Cells[$"A{row}"].Value = "Average Duration";
        sheet.Cells[$"B{row}"].Value = $"{analytics.AverageSubscriptionDurationDays:F0} days";
        row++;
        
        if (analytics.CurrentPlan != null)
        {
            sheet.Cells[$"A{row}"].Value = "Current Plan";
            sheet.Cells[$"B{row}"].Value = analytics.CurrentPlan;
            row++;
        }
        
        if (analytics.NextBillingDate.HasValue)
        {
            sheet.Cells[$"A{row}"].Value = "Next Billing Date";
            sheet.Cells[$"B{row}"].Value = analytics.NextBillingDate.Value.ToString("yyyy-MM-dd");
            row++;
        }
        
        sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
    }

    private void AddFinancialDetails(ExcelWorksheet sheet, UserAnalyticsDto analytics)
    {
        sheet.Cells["A1"].Value = "Financial Details";
        sheet.Cells["A1"].Style.Font.Size = 14;
        sheet.Cells["A1"].Style.Font.Bold = true;
        
        int row = 3;
        sheet.Cells[$"A{row}"].Value = "Metric";
        sheet.Cells[$"B{row}"].Value = "Amount";
        sheet.Cells[$"A{row}:B{row}"].Style.Font.Bold = true;
        row++;
        
        sheet.Cells[$"A{row}"].Value = "Total Revenue";
        sheet.Cells[$"B{row}"].Value = analytics.TotalRevenue;
        sheet.Cells[$"B{row}"].Style.Numberformat.Format = "$#,##0.00";
        row++;
        
        sheet.Cells[$"A{row}"].Value = "Total Paid";
        sheet.Cells[$"B{row}"].Value = analytics.TotalPaid;
        sheet.Cells[$"B{row}"].Style.Numberformat.Format = "$#,##0.00";
        row++;
        
        sheet.Cells[$"A{row}"].Value = "Total Refunded";
        sheet.Cells[$"B{row}"].Value = analytics.TotalRefunded;
        sheet.Cells[$"B{row}"].Style.Numberformat.Format = "$#,##0.00";
        row++;
        
        sheet.Cells[$"A{row}"].Value = "Average Monthly Spend";
        sheet.Cells[$"B{row}"].Value = analytics.AverageMonthlySpend;
        sheet.Cells[$"B{row}"].Style.Numberformat.Format = "$#,##0.00";
        
        sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
    }

    private void AddPaymentDetails(ExcelWorksheet sheet, UserAnalyticsDto analytics)
    {
        sheet.Cells["A1"].Value = "Payment Details";
        sheet.Cells["A1"].Style.Font.Size = 14;
        sheet.Cells["A1"].Style.Font.Bold = true;
        
        int row = 3;
        sheet.Cells[$"A{row}"].Value = "Metric";
        sheet.Cells[$"B{row}"].Value = "Count/Value";
        sheet.Cells[$"A{row}:B{row}"].Style.Font.Bold = true;
        row++;
        
        sheet.Cells[$"A{row}"].Value = "Total Payments";
        sheet.Cells[$"B{row}"].Value = analytics.TotalPayments;
        row++;
        
        sheet.Cells[$"A{row}"].Value = "Successful";
        sheet.Cells[$"B{row}"].Value = analytics.SuccessfulPayments;
        row++;
        
        sheet.Cells[$"A{row}"].Value = "Failed";
        sheet.Cells[$"B{row}"].Value = analytics.FailedPayments;
        row++;
        
        sheet.Cells[$"A{row}"].Value = "Success Rate";
        sheet.Cells[$"B{row}"].Value = analytics.PaymentSuccessRate / 100;
        sheet.Cells[$"B{row}"].Style.Numberformat.Format = "0.00%";
        
        sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
    }

    #endregion
}

