using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Models.Enums;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Services;

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
    _context.Add(ticket);
    await _context.SaveChangesAsync();
  }

  public async Task ArchiveTicketAsync(Ticket ticket, int companyId)
  {
    var project = await _context.Projects.Where(p => p.CompanyId == companyId)
      .FirstOrDefaultAsync(p => p.Id == ticket.ProjectId);
    if (project == null) return;

    ticket.Archived = true;
    _context.Update(ticket);
    await _context.SaveChangesAsync();
  }

  public async Task<List<Ticket>> GetUserTicketsAsync(string userId)
  {
    var user = await _context.Users.FindAsync(userId);
    if (user == null) return new List<Ticket>();

    if (await _rolesService.IsUserInRole(user, nameof(BTRoles.Admin)))
      return await GetTicketsByCompanyIdAsync(user.CompanyId);
    if (await _rolesService.IsUserInRole(user, nameof(BTRoles.ProjectManager)))
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

  public async Task<List<Ticket>> GetArchivedTicketsAsync(int companyId)
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

  public async Task<Ticket?> GetTicketByIdAsync(int ticketId, int companyId)
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

  public async Task<Ticket?> GetTicketAsNoTrackingAsync(int ticketId, int companyId)
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

  public async Task<List<TicketPriority>> GetTicketPriorities()
  {
    return await _context.TicketPriorities.ToListAsync();
  }

  public async Task<List<Ticket>> GetTicketsByCompanyIdAsync(int companyId)
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

  public async Task<List<TicketStatus>> GetTicketStatuses()
  {
    return await _context.TicketStatuses.ToListAsync();
  }

  public async Task<List<TicketType>> GetTicketTypes()
  {
    return await _context.TicketTypes.ToListAsync();
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
    var project = await _context.Projects.Where(p => p.CompanyId == companyId)
      .FirstOrDefaultAsync(p => p.Id == ticket.ProjectId);
    if (project == null) return;

    ticket.Archived = false;
    _context.Update(ticket);
    await _context.SaveChangesAsync();
  }

  public async Task UpdateTicketAsync(Ticket ticket, int companyId)
  {
    var project = await _context.Projects.Where(p => p.CompanyId == companyId)
      .FirstOrDefaultAsync(p => p.Id == ticket.ProjectId);
    if (project == null) return;

    _context.Update(ticket);
    await _context.SaveChangesAsync();
  }

  public async Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment)
  {
    await _context.AddAsync(ticketAttachment);
    await _context.SaveChangesAsync();
  }

  public async Task<TicketAttachment?> GetTicketAttachmentByIdAsync(int ticketAttachmentId)
  {
    var ticketAttachment = await _context.TicketAttachments
      .Include(t => t.BTUser)
      .FirstOrDefaultAsync(t => t.Id == ticketAttachmentId);
    return ticketAttachment;
  }

  public async Task AddTicketCommentAsync(TicketComment comment)
  {
    await _context.AddAsync(comment);
    await _context.SaveChangesAsync();
  }
}