using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Claims.Application.Interfaces
{
    public interface ICoverRepository
    {
        Task AddAsync(Cover cover);
        Task<Cover?> GetByIdAsync(string id);
        Task<List<Cover>> GetAllAsync();
        Task DeleteAsync(string id);
    }
}
