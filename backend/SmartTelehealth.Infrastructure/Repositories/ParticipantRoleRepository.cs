using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace SmartTelehealth.Infrastructure.Repositories
{
    public class ParticipantRoleRepository : RepositoryBase<ParticipantRole>, IParticipantRoleRepository
    {
        private readonly ApplicationDbContext _context;
        public ParticipantRoleRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<ParticipantRole?> GetByIdAsync(Guid id)
        {
            return await _context.ParticipantRoles.FindAsync(id);
        }
    }
} 