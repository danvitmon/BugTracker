using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Services
{
    public class BTTicketService
    {
        private readonly ApplicationDbContext _context;
        public BTTicketService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddTicketAsync(Ticket ticket)
        {
            _context.Add(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task ArchiveTicketAsync(Ticket ticket, int companyId)
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

        public async Task<List<Ticket>> GetArchivedTicketsAsync(int companyId)
        {
            return await _context.Tickets
                .Include(t => t.Project)
                .Where(t => t.Project.CompanyId == companyId && t.Archived)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .Include(t => t.DeveloperUser)
                .Include(t => t.SubmitterUser)
                .ToListAsync();
        }

        public async Task<Ticket?> GetTicketByIdAsync(int ticketId, int companyId)
        {
            return await _context.Tickets
                .Include(t => t.Project)
                .Where(t => t.Project!.CompanyId == companyId)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .Include(t => t.DeveloperUser)
                .Include(t => t.SubmitterUser)
                .FirstOrDefaultAsync(t => t.Id == ticketId);
        }

        public async Task<List<TicketPriority>> GetTicketPriorities()
        {
            return await _context.TicketPriorities.ToListAsync();
        }

        public async Task<List<Ticket>> GetTicketsByCompanyIdAsync(int companyId)
        {
            return await _context.Tickets
                .Include(t => t.Project)
                .Where(t => t.Project.CompanyId == companyId)
                .Include(t => t.TicketPriority)
                .Include(t => t.TicketStatus)
                .Include(t => t.TicketType)
                .Include(t => t.DeveloperUser)
                .Include(t => t.SubmitterUser)
                .ToListAsync();
        }

        public async Task<List<TicketStatus>> GetTicketStatuses()
        {
            return await _context.TicketStatuses.ToListAsync();
        }

        public async Task<List<TicketType>> GetTicketTypes()
        {
            return await _context.TicketTypes.ToListAsync();
        }

        public async Task RestoreTicketAsync(Ticket ticket, int companyId)
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

        public async Task UpdateTicketAsync(Ticket ticket, int companyId)
        {
            var project = await _context.Projects.Where(p => p.CompanyId == companyId).FirstOrDefaultAsync(p => p.Id == ticket.ProjectId);
            if (project == null)
            {
                return;
            }

            _context.Update(ticket);
            await _context.SaveChangesAsync();
        }
    }
}
