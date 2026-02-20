using Claims.Application.Interfaces;
using Claims.Domain;
using Claims.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure.Repositories
{
    public class ClaimRepository : IClaimRepository
    {
        private readonly ClaimsContext _context;

        public ClaimRepository(ClaimsContext context)
        {
            _context = context;
        }

        public async Task<List<Claim>> GetAllAsync()
        {
            return await _context.Claims.ToListAsync();
        }

        public async Task<Claim?> GetByIdAsync(string id)
        {
            return await _context.Claims.FindAsync(id);
        }

        public async Task AddAsync(Claim claim)
        {
            await _context.Claims.AddAsync(claim);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var claim = await GetByIdAsync(id);
            if (claim == null) return;

            _context.Claims.Remove(claim);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Claim claim)
        {
            _context.Claims.Update(claim);
            await _context.SaveChangesAsync();
        }
    }
}