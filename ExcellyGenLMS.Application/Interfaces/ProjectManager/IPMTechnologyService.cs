// Path: ExcellyGenLMS.Application/Interfaces/ProjectManager/IPMTechnologyService.cs

using ExcellyGenLMS.Application.DTOs.ProjectManager;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Interfaces.ProjectManager
{
    public interface IPMTechnologyService
    {
        Task<IEnumerable<TechnologyDto>> GetTechnologiesAsync();
    }
}