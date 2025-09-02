using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;

namespace SmartTelehealth.API.Tests.Mocks;

public class MockPdfService : IPdfService
{
    public Task<byte[]> GenerateInvoicePdfAsync(BillingRecordDto billingRecord, UserDto user, SubscriptionDto? subscription = null)
    {
        return Task.FromResult(new byte[] { 0x25, 0x50, 0x44, 0x46 }); // Mock PDF header
    }

    public Task<byte[]> GenerateSubscriptionSummaryPdfAsync(SubscriptionDto subscription, UserDto user)
    {
        return Task.FromResult(new byte[] { 0x25, 0x50, 0x44, 0x46 }); // Mock PDF header
    }

    public Task<byte[]> GenerateBillingHistoryPdfAsync(IEnumerable<BillingRecordDto> billingRecords, UserDto user)
    {
        return Task.FromResult(new byte[] { 0x25, 0x50, 0x44, 0x46 }); // Mock PDF header
    }
}
