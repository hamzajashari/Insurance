using Claims.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Claims.Application.Interfaces
{
    /// <summary>
    /// Provides operations related to claims.
    /// </summary>
    public interface IClaimService
    {
        /// <summary>
        /// Retrieves all claims.
        /// </summary>
        Task<IEnumerable<Claim>> GetAllAsync();
        /// <summary>
        /// Retrieves a claim by its identifier.
        /// </summary>
        Task<Claim?> GetByIdAsync(string id);
        /// <summary>
        /// Creates a new claim and performs required business validation.
        /// </summary>
        Task<Claim> CreateAsync(Claim claim);
        /// <summary>
        /// Deletes a claim.
        /// </summary>
        Task DeleteAsync(string id);
    }
}
