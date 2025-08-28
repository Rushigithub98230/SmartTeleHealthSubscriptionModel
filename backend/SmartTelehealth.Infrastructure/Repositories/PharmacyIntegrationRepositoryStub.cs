using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Core.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace SmartTelehealth.Infrastructure.Repositories
{
    public class PharmacyIntegrationRepositoryStub : IPharmacyIntegrationRepository
    {
        public Task<PharmacyIntegration> GetByIdAsync(Guid id) => Task.FromResult<PharmacyIntegration>(null);
        public Task<PharmacyIntegration> GetActiveIntegrationAsync() => Task.FromResult<PharmacyIntegration>(null);
        public Task<IEnumerable<PharmacyIntegration>> GetAllAsync() => Task.FromResult<IEnumerable<PharmacyIntegration>>(new List<PharmacyIntegration>());
        public Task<PharmacyIntegration> CreateAsync(PharmacyIntegration integration) => Task.FromResult<PharmacyIntegration>(null);
        public Task<PharmacyIntegration> UpdateAsync(PharmacyIntegration integration) => Task.FromResult<PharmacyIntegration>(null);
        public Task<bool> DeleteAsync(Guid id) => Task.FromResult(false);
        public Task<bool> TestConnectionAsync(Guid integrationId) => Task.FromResult(false);
        public Task<DateTime> GetLastSyncTimeAsync(Guid integrationId) => Task.FromResult(DateTime.MinValue);
        public Task<bool> UpdateLastSyncTimeAsync(Guid integrationId, DateTime syncTime) => Task.FromResult(false);
    }
} 