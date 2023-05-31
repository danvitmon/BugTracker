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

namespace BugTracker.Services
{
    public class BTProjectService
    {
        private readonly ApplicationDbContext _context;
        public BTProjectService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddProjectAsync(Project project)
        {
            _context.Add(project);
            await _context.SaveChangesAsync();
        }

        public async Task ArchiveProjectAsync(Project project, int companyId)
        {
            try
            {
                if (project.CompanyId == companyId)
                {
                    project.Archived = true;

                    // archive all the tickets
                    foreach(Ticket ticket in project.Tickets)
                    {
                        // archived by project if the ticket is not already archived
                        // if (ticket.ArchivedByProject == true) ticket.Archived = false; same as below
                        ticket.ArchivedByProject = !ticket.Archived;

                        ticket.Archived = true;
                    }

                    _context.Update(project);
                    await _context.SaveChangesAsync();

                }
            }
            catch 
            {
                throw;
            }
            //if (project.CompanyId != companyId)
            //{
            //    return;
            //}

            //var ptickets = await _context.Tickets.Where(t => t.ProjectId == project.Id).ToListAsync();
            //foreach (Ticket ticket in ptickets)
            //{
            //    ticket.ArchivedByProject = true;
            //    _context.Update(ticket);
            //}

            //project.Archived = true;
            //_context.Update(project);
            //await _context.SaveChangesAsync();
        }

        public async Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId)
        {
            return await _context.Projects.Where(p => p.CompanyId == companyId).Include(p => p.Company).Include(p => p.ProjectPriority).ToListAsync();
        }

        public async Task<List<Project>> GetAllProjectsByPriorityAsync(int companyId, string priority)
        {
            try
            {
                List<Project> projects = await _context.Projects
                                                      .Where(p => p.CompanyId == companyId && p.Archived == false)
                                                      .Include(p => p.Tickets)
                                                      .Include(p => p.ProjectPriority)
                                                      .Include(p => p.Members)
                                                      .Where(p => string.Equals(priority, p.ProjectPriority!.Name))
                                                      .ToListAsync();

                return projects;
            }
            catch (Exception)
            {
                return new List<Project>();
                throw;
            }
            //return await _context.Projects.Include(p => p.ProjectPriority).Where(p => p.CompanyId == companyId && p.ProjectPriority!.Name == priority).Include(p => p.Company).ToListAsync();
        }

        public async Task<List<Project>> GetAllUserProjectsAsync(string userId)
        {
            return await _context.Projects.Where(p => p.Members.Any(u => u.Id == userId)).Include(p => p.Company).Include(p => p.ProjectPriority).ToListAsync();
        }

        public async Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int companyId)
        {
            return await _context.Projects.Where(p => p.CompanyId == companyId && p.Archived).Include(p => p.Company).Include(p => p.ProjectPriority).ToListAsync();
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
            //return await _context.Projects.Where(p => p.CompanyId == companyId).Include(p => p.Company).Include(p => p.ProjectPriority).FirstOrDefaultAsync(p => p.Id == projectId);
        }

        public async Task<List<ProjectPriority>> GetProjectPrioritiesAsync()
        {
            return await _context.ProjectPriorities.ToListAsync();
        }

        public async Task RestoreProjectAsync(Project project, int companyId)
        {
            if (project.CompanyId != companyId)
            {
                return;
            }

            var ptickets = await _context.Tickets.Where(t => t.ProjectId == project.Id).ToListAsync();
            foreach (Ticket ticket in ptickets)
            {
                ticket.ArchivedByProject = false;
                _context.Update(ticket);
            }

            project.Archived = false;
            _context.Update(project);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProjectAsync(Project project, int companyId)
        {
            if (project.CompanyId != companyId)
            {
                return;
            }

            _context.Update(project);
            await _context.SaveChangesAsync();
        }
    }
}
