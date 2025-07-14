using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.Learner; // DTOs are in ExcellyGenLMS.Application.DTOs.Learner

namespace ExcellyGenLMS.Application.Interfaces.Learner // Namespace matches the file location
{
    public interface ICvService
    {
        /// <summary>
        /// Retrieves the aggregated data required to populate a user's CV.
        /// </summary>
        /// <param name="userId">The ID of the user for whom to generate CV data.</param>
        /// <returns>A CvDto containing the user's CV information.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if the user profile is not found.</exception>
        Task<CvDto> GetCvDataAsync(string userId);
    }
}