using Claims.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Claims.Application.Interfaces
{
    public interface IClaimRepository
    {
        Task AddAsync(Claim claim);
        Task<Claim?> GetByIdAsync(string id);
        Task<List<Claim>> GetAllAsync();
        Task DeleteAsync(string id);
    }
}
