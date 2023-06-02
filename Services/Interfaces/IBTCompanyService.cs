using BugTracker.Models;

namespace BugTracker.Services.Interfaces
{
    public interface IBTCompanyService
    {
        Task<List<BTUser>> GetUsersByCompanyIdAsync(int companyId);
    }
}
