using Claims.Application.Interfaces;
using Claims.Infrastructure.DbContexts;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Claims.Application.Repository
{
    public class CoverRepository : ICoverRepository
    {
        private readonly ClaimsContext _context;
        public CoverRepository(ClaimsContext context)
        {
            _context = context;
        }

        public async Task<List<Cover>> GetAllAsync()
        {
            return await _context.Covers.ToListAsync();
        }
        public async Task<Cover?> GetByIdAsync(string id)
        {
            return await _context.Covers.FindAsync(id);
        }
        public async Task AddAsync(Cover cover)
        {
            await _context.Covers.AddAsync(cover);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var cover = await GetByIdAsync(id);
            if (cover == null) return;

            _context.Covers.Remove(cover);
            await _context.SaveChangesAsync();
        }
    }
}
