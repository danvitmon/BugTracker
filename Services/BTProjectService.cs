using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Models.Enums;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Services;

public class BTProjectService : IBTProjectService
{
  private readonly ApplicationDbContext _context;
  private readonly IBTRolesService _rolesService;

  public BTProjectService(ApplicationDbContext context, IBTRolesService rolesService)
  {
    _context = context;
    _rolesService = rolesService;
  }

  public async Task AddProjectAsync(Project project)
  {
    _context.Add(project);
    await _context.SaveChangesAsync();
  }

  public async Task ArchiveProjectAsync(Project project, int companyId)
  {
    if (project.CompanyId != companyId) return;

    foreach (var ticket in project.Tickets)
    {
      ticket.ArchivedByProject = !ticket.Archived;
      ticket.Archived = true;
    }

    project.Archived = true;
    _context.Update(project);
    await _context.SaveChangesAsync();
  }

  public async Task<List<Project>> GetProjectsByCompanyIdAsync(int companyId)
  {
    return await _context.Projects.Where(p => p.CompanyId == companyId).Include(p => p.Tickets)
      .Include(p => p.ProjectPriority).Include(p => p.Members).ToListAsync();
  }

  public async Task<List<Project>> GetProjectsByPriorityAsync(int companyId, string priority)
  {
    return await _context.Projects.Include(p => p.ProjectPriority)
      .Where(p => p.CompanyId == companyId && !p.Archived && string.Equals(p.ProjectPriority!.Name, priority))
      .Include(p => p.Tickets).Include(p => p.Members).ToListAsync();
  }

  public async Task<List<Project>> GetUserProjectsAsync(string userId)
  {
    return await _context.Projects.Include(p => p.Members).Where(p => p.Members.Any(u => u.Id == userId) && !p.Archived)
      .Include(p => p.Tickets).Include(p => p.ProjectPriority).ToListAsync();
  }

  public async Task<List<Project>> GetArchivedProjectsAsync(int companyId)
  {
    return await _context.Projects.Where(p => p.CompanyId == companyId && p.Archived).Include(p => p.Tickets)
      .Include(p => p.ProjectPriority).Include(p => p.Members).ToListAsync();
  }

  public async Task<Project?> GetProjectByIdAsync(int projectId, int companyId)
  {
    return await _context.Projects
      .Include(p => p.Company)
      .Include(p => p.ProjectPriority)
      .Include(p => p.Members)
      .Include(p => p.Tickets)
      .ThenInclude(t => t.DeveloperUser)
      .Include(p => p.Tickets)
      .ThenInclude(t => t.SubmitterUser)
      .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);
  }

  public async Task<List<ProjectPriority>> GetProjectPrioritiesAsync()
  {
    return await _context.ProjectPriorities.ToListAsync();
  }

  public async Task<List<Project>> GetUnassignedProjectsAsync(int companyId)
  {
    var projects = await GetProjectsByCompanyIdAsync(companyId);
    List<Project> unassigned = new();
    foreach (var project in projects)
    {
      var pm = await GetProjectManagerAsync(project.Id, companyId);
      if (pm is null) unassigned.Add(project);
    }

    return unassigned;
  }

  public async Task RestoreProjectAsync(Project project, int companyId)
  {
    if (project.CompanyId != companyId) return;

    foreach (var ticket in project.Tickets)
    {
      ticket.Archived = !ticket.ArchivedByProject;
      ticket.ArchivedByProject = false;
    }

    project.Archived = false;
    _context.Update(project);
    await _context.SaveChangesAsync();
  }

  public async Task UpdateProjectAsync(Project project, int companyId)
  {
    if (project.CompanyId != companyId) return;

    _context.Update(project);
    await _context.SaveChangesAsync();
  }

  public async Task<BTUser?> GetProjectManagerAsync(int projectId, int companyId)
  {
    var project = await _context.Projects
      .AsNoTracking()
      .Include(p => p.Members)
      .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

    if (project is not null)
      foreach (var member in project.Members)
        if (await _rolesService.IsUserInRole(member, nameof(BTRoles.ProjectManager)))
          return member;

    return null;
  }

  public async Task<bool> AddProjectManagerAsync(string userId, int projectId, int companyId)
  {
    var project = await _context.Projects.Include(p => p.Members)
      .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);
    var projectManager = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.CompanyId == companyId);

    if (project is not null && projectManager is not null)
    {
      if (!await _rolesService.IsUserInRole(projectManager, nameof(BTRoles.ProjectManager))) return false;

      await RemoveProjectManagerAsync(projectId, companyId);

      project.Members.Add(projectManager);
      await _context.SaveChangesAsync();

      return true;
    }

    return false;
  }

  public async Task RemoveProjectManagerAsync(int projectId, int companyId)
  {
    var project = await _context.Projects.Include(p => p.Members)
      .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

    if (project is not null)
    {
      foreach (var member in project.Members)
        if (await _rolesService.IsUserInRole(member, nameof(BTRoles.ProjectManager)))
          project.Members.Remove(member);

      await _context.SaveChangesAsync();
    }
  }

  public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string roleName, int companyId)
  {
    var project = await _context.Projects
      .AsNoTracking()
      .Include(p => p.Members)
      .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

    if (project is not null)
    {
      var members = project.Members.ToList();
      List<BTUser> rolemembers = new();
      foreach (var member in members)
        if (await _rolesService.IsUserInRole(member, roleName))
          rolemembers.Add(member);
      return rolemembers;
    }

    return new List<BTUser>();
  }

  public async Task<bool> AddMemberToProjectAsync(BTUser member, int projectId, int companyId)
  {
    var project = await _context.Projects
      .Include(p => p.Members)
      .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

    if (project is not null && member.CompanyId == companyId)
    {
      project.Members.Add(member);
      _context.Update(project);
      await _context.SaveChangesAsync();
      return true;
    }

    return false;
  }

  public async Task<bool> RemoveMemberFromProjectAsync(BTUser member, int projectId, int companyId)
  {
    var project = await _context.Projects
      .Include(p => p.Members)
      .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

    if (project is not null)
    {
      project.Members.Remove(member);
      _context.Update(project);
      await _context.SaveChangesAsync();
      return true;
    }

    return false;
  }

  public async Task<List<Project>> GetUnassignedProjectsByCompanyIdAsync(int companyId)
  {
    var allProjects = await GetProjectsByCompanyIdAsync(companyId);
    List<Project> unassignedProjects = new();

    foreach (var project in allProjects)
    {
      var projectManager = await GetProjectManagerAsync(project.Id, companyId);
      if (projectManager is null) unassignedProjects.Add(project);
    }

    return unassignedProjects;
  }
}