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
        /// <summary>
        /// Retrieves a participant role by its unique identifier
        /// </summary>
        public override async Task<ParticipantRole?> GetByIdAsync(object id)
        {
            if (id is not Guid roleId)
                return null;

            return await _context.ParticipantRoles.FindAsync(roleId);
        }

        /// <summary>
        /// Retrieves all participant roles
        /// </summary>
        public override async Task<IEnumerable<ParticipantRole>> GetAllAsync()
        {
            return await _context.ParticipantRoles.ToListAsync();
        }

        /// <summary>
        /// Creates a new participant role
        /// </summary>
        public override async Task<ParticipantRole> CreateAsync(ParticipantRole role)
        {
            role.CreatedDate = DateTime.UtcNow;
            return await base.CreateAsync(role);
        }

        /// <summary>
        /// Updates an existing participant role
        /// </summary>
        public override async Task<ParticipantRole> UpdateAsync(ParticipantRole role)
        {
            role.UpdatedDate = DateTime.UtcNow;
            return await base.UpdateAsync(role);
        }

        /// <summary>
        /// Deletes a participant role by its unique identifier (hard delete)
        /// </summary>
        public override async Task<bool> DeleteAsync(object id)
        {
            if (id is not Guid roleId)
                return false;

            var role = await _context.ParticipantRoles.FindAsync(roleId);
            if (role != null)
            {
                _context.ParticipantRoles.Remove(role);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if a participant role exists
        /// </summary>
        public override async Task<bool> ExistsAsync(object id)
        {
            if (id is not Guid roleId)
                return false;

            return await _context.ParticipantRoles.AnyAsync(x => x.Id == roleId);
        }
    }
} 