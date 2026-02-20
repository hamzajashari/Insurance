using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Claims.Application.Interfaces
{
    /// <summary>
    /// Provides operations related to insurance covers.
    /// </summary>
    public interface ICoverService
    {
        /// <summary>
        /// Retrieves all covers.
        /// </summary>
        Task<IEnumerable<Cover>> GetAllAsync();
        /// <summary>
        /// Retrieves a cover by its identifier.
        /// </summary>
        Task<Cover?> GetByIdAsync(string id);
        /// <summary>
        /// Creates a new cover and calculates its premium.
        /// </summary>
        Task<Cover> CreateAsync(Cover cover);
        /// <summary>
        /// Deletes a cover.
        /// </summary>
        Task DeleteAsync(string id);
        /// <summary>
        /// Calculates premium based on duration and cover type.
        /// </summary>
        decimal ComputePremium(CoverType coverType,int insuranceDays);
    }
}
