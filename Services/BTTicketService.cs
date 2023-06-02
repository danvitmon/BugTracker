using System.ComponentModel.Design;
using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Models.Enums;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;

        public BTTicketService(ApplicationDbContext context, IBTRolesService rolesService)
        {
            _context = context;
            _rolesService = rolesService;
        }

        public async Task AddTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task ArchiveTicketAsync(Ticket ticket, int companyId)
        {
            try
            {
                var project = await _context.Projects.Where(p => p.CompanyId == companyId).FirstOrDefaultAsync(p => p.Id == ticket.ProjectId);
                if (project == null)
                {
                    return;
                }

                ticket.Archived = true;
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetUserTicketsAsync(string userId)
        {
            try
            {
                BTUser? user = await _context.Users.FindAsync(userId);
                if (user == null) { return new List<Ticket>(); }

                if (await _rolesService.IsUserInRole(user, nameof(BTRoles.Admin)))
                {
                    return await GetTicketsByCompanyIdAsync(user.CompanyId);
                }
                else if (await _rolesService.IsUserInRole(user, nameof(BTRoles.ProjectManager)))
                {
                    return await _context.Tickets
                                    .Include(t => t.DeveloperUser)
                                    .Include(t => t.SubmitterUser)
                                    .Include(t => t.Project)
                                        .ThenInclude(p => p!.Members)
                                    .Include(t => t.TicketPriority)
                                    .Include(t => t.TicketStatus)
                                    .Include(t => t.TicketType)
                                    .Where(t => !t.Archived && t.Project!.Members.Any(m => m.Id == userId))
                                    .ToListAsync();
                }
                else
                {
                    return await _context.Tickets
                                    .Include(t => t.DeveloperUser)
                                    .Include(t => t.SubmitterUser)
                                    .Include(t => t.Project)
                                        .ThenInclude(p => p!.Members)
                                    .Include(t => t.TicketPriority)
                                    .Include(t => t.TicketStatus)
                                    .Include(t => t.TicketType)
                                    .Where(t => !t.Archived && (t.DeveloperUserId == userId || t.SubmitterUserId == userId))
                                    .ToListAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetArchivedTicketsAsync(int companyId)
        {
            try
            {
                return await _context.Tickets
                .Include(t => t.Project)
                    .ThenInclude(p => p!.Members)
                .Include(t => t.TicketPriority)
                .Where(t => t.Project!.CompanyId == companyId && t.Archived)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .Include(t => t.DeveloperUser)
                .Include(t => t.SubmitterUser)
                .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Ticket?> GetTicketByIdAsync(int ticketId, int companyId)
        {
            try
            {
                return await _context.Tickets
                .Include(t => t.Project)
                    .ThenInclude(p => p!.Members)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .Include(t => t.DeveloperUser)
                .Include(t => t.SubmitterUser)
                .Include(t => t.SubmitterUser)
                .Include(t => t.Attachments)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(t => t.Id == ticketId && t.Project!.CompanyId == companyId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Ticket?> GetTicketAsNoTrackingAsync(int ticketId, int companyId)
        {
            try
            {
                return await _context.Tickets
                .AsNoTracking()
                .Include(t => t.Project)
                    .ThenInclude(p => p!.Members)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .Include(t => t.DeveloperUser)
                .Include(t => t.SubmitterUser)
                .Include(t => t.SubmitterUser)
                .Include(t => t.Attachments)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(t => t.Id == ticketId && t.Project!.CompanyId == companyId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<TicketPriority>> GetTicketPriorities()
        {
            try
            {
                return await _context.TicketPriorities.ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetTicketsByCompanyIdAsync(int companyId)
        {
            try
            {
                return await _context.Tickets
                    .Include(t => t.Project)
                        .ThenInclude(p => p!.Members)
                    .Where(t => t.Project!.CompanyId == companyId && !t.Archived)
                    .Include(t => t.TicketPriority)
                    .Include(t => t.TicketStatus)
                    .Include(t => t.TicketType)
                    .Include(t => t.DeveloperUser)
                    .Include(t => t.SubmitterUser)
                    .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<List<TicketStatus>> GetTicketStatuses()
        {
            try
            {
                return await _context.TicketStatuses.ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<TicketType>> GetTicketTypes()
        {
            try
            {
                return await _context.TicketTypes.ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetUnassignedTicketsAsync(int companyId)
        {
            return await _context.Tickets
                    .Include(t => t.Project)
                        .ThenInclude(p => p!.Members)
                    .Where(t => t.Project!.CompanyId == companyId && t.DeveloperUserId == null && !t.Archived)
                    .Include(t => t.DeveloperUser)
                    .Include(t => t.TicketPriority)
                    .Include(t => t.TicketStatus)
                    .Include(t => t.TicketType)
                    .Include(t => t.SubmitterUser)
                    .ToListAsync();
        }

        public async Task RestoreTicketAsync(Ticket ticket, int companyId)
        {
            try
            {
                var project = await _context.Projects.Where(p => p.CompanyId == companyId).FirstOrDefaultAsync(p => p.Id == ticket.ProjectId);
                if (project == null)
                {
                    return;
                }

                ticket.Archived = false;
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateTicketAsync(Ticket ticket, int companyId)
        {
            try
            {
                var project = await _context.Projects.Where(p => p.CompanyId == companyId).FirstOrDefaultAsync(p => p.Id == ticket.ProjectId);
                if (project == null)
                {
                    return;
                }

                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment)
        {
            try
            {
                await _context.AddAsync(ticketAttachment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<TicketAttachment?> GetTicketAttachmentByIdAsync(int ticketAttachmentId)
        {
            try
            {
                TicketAttachment? ticketAttachment = await _context.TicketAttachments
                                                                  .Include(t => t.BTUser)
                                                                  .FirstOrDefaultAsync(t => t.Id == ticketAttachmentId);
                return ticketAttachment;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddTicketCommentAsync(TicketComment comment)
        {
            try
            {
                await _context.AddAsync(comment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}