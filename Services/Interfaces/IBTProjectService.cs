using BugTracker.Models;

namespace BugTracker.Services.Interfaces;

public interface IBTProjectService
{
  Task                        AddProjectAsync             (Project project);
  Task<List<Project>>         GetProjectsByCompanyIdAsync (int     companyId);
  Task<List<Project>>         GetProjectsByPriorityAsync  (int     companyId, string priority);
  Task<List<Project>>         GetUnassignedProjectsAsync  (int     companyId);
  Task<List<Project>>         GetUserProjectsAsync        (string  userId);
  Task<List<Project>>         GetArchivedProjectsAsync    (int     companyId);
  Task<Project?>              GetProjectByIdAsync         (int     projectId, int    companyId);
  Task<List<ProjectPriority>> GetProjectPrioritiesAsync   ();
  Task                        ArchiveProjectAsync         (Project project,   int    companyId);
  Task                        RestoreProjectAsync         (Project project,   int    companyId);
  Task                        UpdateProjectAsync          (Project project,   int    companyId);
  Task<BTUser?>               GetProjectManagerAsync      (int     projectId, int    companyId);
  Task<bool>                  AddProjectManagerAsync      (string  userId,    int    projectId, int companyId);
  Task                        RemoveProjectManagerAsync   (int     projectId, int    companyId);
  Task<List<BTUser>>          GetProjectMembersByRoleAsync(int     projectId, string roleName,  int companyId);
  Task<bool>                  AddMemberToProjectAsync     (BTUser  member,    int    projectId, int companyId);
  Task<bool>                  RemoveMemberFromProjectAsync(BTUser  member,    int    projectId, int companyId);
}