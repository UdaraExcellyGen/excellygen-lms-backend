// Path: ExcellyGenLMS.Application/Interfaces/ProjectManager/IPMTechnologyService.cs

using System.Collections.Generic;
using System.Threading.Tasks;
using ExcellyGenLMS.Application.DTOs.ProjectManager;

namespace ExcellyGenLMS.Application.Interfaces.ProjectManager
{
    public interface IPMTechnologyService
    {
        Task<IEnumerable<TechnologyDto>> GetTechnologiesAsync();
    }
}