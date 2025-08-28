using SmartTelehealth.Core.Entities;
using System;
using System.Threading.Tasks;

namespace SmartTelehealth.Core.Interfaces
{
    public interface IParticipantRoleRepository
    {
        Task<ParticipantRole?> GetByIdAsync(Guid id);
    }
} 