using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Models.Enums;
using Microsoft.AspNetCore.Identity;
using BugTracker.Services.Interfaces;
using System.ComponentModel.Design;
using System.Security.Policy;

namespace BugTracker.Services
{
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
            try
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task ArchiveProjectAsync(Project project, int companyId)
        {
            try
            {
                if (project.CompanyId != companyId)
                {
                    return;
                }

                foreach (Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = !ticket.Archived;
                    ticket.Archived = true;
                }

                project.Archived = true;
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Project>> GetProjectsByCompanyIdAsync(int companyId)
        {
            try
            {
                return await _context.Projects.Where(p => p.CompanyId == companyId).Include(p => p.Tickets).Include(p => p.ProjectPriority).Include(p => p.Members).ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Project>> GetProjectsByPriorityAsync(int companyId, string priority)
        {
            try
            {
                return await _context.Projects.Include(p => p.ProjectPriority).Where(p => p.CompanyId == companyId && !p.Archived && string.Equals(p.ProjectPriority!.Name, priority)).Include(p => p.Tickets).Include(p => p.Members).ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Project>> GetUserProjectsAsync(string userId)
        {
            try
            {
                return await _context.Projects.Include(p => p.Members).Where(p => p.Members.Any(u => u.Id == userId) && !p.Archived).Include(p => p.Tickets).Include(p => p.ProjectPriority).ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Project>> GetArchivedProjectsAsync(int companyId)
        {
            try
            {
                return await _context.Projects.Where(p => p.CompanyId == companyId && p.Archived).Include(p => p.Tickets).Include(p => p.ProjectPriority).Include(p => p.Members).ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Project?> GetProjectByIdAsync(int projectId, int companyId)
        {
            try
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
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<ProjectPriority>> GetProjectPrioritiesAsync()
        {
            try
            {
                return await _context.ProjectPriorities.ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<List<Project>> GetUnassignedProjectsAsync(int companyId)
        {
            try
            {
                List<Project> projects = await GetProjectsByCompanyIdAsync(companyId);
                List<Project> unassigned = new();
                foreach (Project project in projects)
                {
                    BTUser? pm = await GetProjectManagerAsync(project.Id, companyId);
                    if (pm is null)
                    {
                        unassigned.Add(project);
                    }
                }
                return unassigned;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task RestoreProjectAsync(Project project, int companyId)
        {
            try
            {
                if (project.CompanyId != companyId)
                {
                    return;
                }

                foreach (Ticket ticket in project.Tickets)
                {
                    ticket.Archived = !ticket.ArchivedByProject;
                    ticket.ArchivedByProject = false;
                }

                project.Archived = false;
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateProjectAsync(Project project, int companyId)
        {
            try
            {
                if (project.CompanyId != companyId)
                {
                    return;
                }

                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<BTUser?> GetProjectManagerAsync(int projectId, int companyId)
        {
            try
            {
                Project? project = await _context.Projects
                                            .AsNoTracking()
                                            .Include(p => p.Members)
                                            .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                if (project is not null)
                {
                    foreach (BTUser member in project.Members)
                    {
                        if (await _rolesService.IsUserInRole(member, nameof(BTRoles.ProjectManager)))
                        {
                            return member;
                        }
                    }
                }

                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> AddProjectManagerAsync(string userId, int projectId, int companyId)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);
                BTUser? projectManager = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.CompanyId == companyId);

                if (project is not null && projectManager is not null)
                {
                    if (!await _rolesService.IsUserInRole(projectManager, nameof(BTRoles.ProjectManager))) { return false; }

                    await RemoveProjectManagerAsync(projectId, companyId);

                    project.Members.Add(projectManager);
                    await _context.SaveChangesAsync();

                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task RemoveProjectManagerAsync(int projectId, int companyId)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                if (project is not null)
                {
                    foreach (BTUser member in project.Members)
                    {
                        if (await _rolesService.IsUserInRole(member, nameof(BTRoles.ProjectManager)))
                        {
                            project.Members.Remove(member);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string roleName, int companyId)
        {
            try
            {
                Project? project = await _context.Projects
                                    .AsNoTracking()
                                    .Include(p => p.Members)
                                    .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                if (project is not null)
                {
                    List<BTUser> members = project.Members.ToList();
                    List<BTUser> rolemembers = new();
                    foreach (BTUser member in members)
                    {
                        if (await _rolesService.IsUserInRole(member, roleName))
                        {
                            rolemembers.Add(member);
                        }
                    }
                    return rolemembers;
                }

                return new List<BTUser>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> AddMemberToProjectAsync(BTUser member, int projectId, int companyId)
        {
            try
            {
                Project? project = await _context.Projects
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
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> RemoveMemberFromProjectAsync(BTUser member, int projectId, int companyId)
        {
            try
            {
                Project? project = await _context.Projects
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
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Project>> GetUnassignedProjectsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Project> allProjects = await GetProjectsByCompanyIdAsync(companyId);
                List<Project> unassignedProjects = new();

                foreach (Project project in allProjects)
                {
                    BTUser? projectManager = await GetProjectManagerAsync(project.Id, companyId);
                    if (projectManager is null) unassignedProjects.Add(project);
                }

                return unassignedProjects;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}