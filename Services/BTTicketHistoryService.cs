using Microsoft.EntityFrameworkCore;

using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;

namespace BugTracker.Services;

public class BTTicketHistoryService : IBTTicketHistoryService
{
  private readonly ApplicationDbContext _context;

  public BTTicketHistoryService(ApplicationDbContext context) => _context = context;

  public async Task AddHistoryAsync(Ticket? oldTicket, Ticket newTicket, string userId)
  {
    if (oldTicket is null)
    {
      // create a history item "new ticket created"
      var history = new TicketHistory
      {
        TicketId     = newTicket.Id,
        PropertyName = string.Empty,
        OldValue     = string.Empty,
        NewValue     = string.Empty,
        Created      = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
        UserId       = userId,
        Description  = "New Ticket created"
      };

      _context.Add(history);
      await _context.SaveChangesAsync();
    }
    else
    {
      // check each property and make a ticket history item for anything that's changed
      if (!string.Equals(oldTicket.Title, newTicket.Title))
      {
        TicketHistory history = new()
        {
          TicketId     = newTicket.Id,
          PropertyName = nameof(Ticket.Title),
          OldValue     = oldTicket.Title,
          NewValue     = newTicket.Title,
          Created      = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
          UserId       = userId,
          Description  = $"Ticket title was changed to {newTicket.Title}"
        };

        _context.Add(history);
      }

      if (!string.Equals(oldTicket.Description, newTicket.Description))
      {
        TicketHistory history = new()
        {
          TicketId     = newTicket.Id,
          PropertyName = nameof(Ticket.Description),
          OldValue     = oldTicket.Description,
          NewValue     = newTicket.Description,
          Created      = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
          UserId       = userId,
          Description  = $"Ticket title was changed to {newTicket.Description}"
        };

        _context.Add(history);
      }

      if (oldTicket.Archived != newTicket.Archived)
      {
        TicketHistory history = new()
        {
          TicketId     = newTicket.Id,
          PropertyName = nameof(Ticket.Archived),
          OldValue     = oldTicket.Archived.ToString(),
          NewValue     = newTicket.Archived.ToString(),
          Created      = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
          UserId       = userId,
          Description  = newTicket.Archived ? "Ticket archived" : "Ticket restored"
        };

        _context.Add(history);
      }

      if (oldTicket.TicketTypeId != newTicket.TicketTypeId)
      {
        TicketHistory history = new()
        {
          TicketId     = newTicket.Id,
          PropertyName = nameof(Ticket.TicketType),
          OldValue     = oldTicket.TicketType!.Name,
          NewValue     = newTicket.TicketType!.Name,
          Created      = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
          UserId       = userId,
          Description  = $"Ticket type changed to {newTicket.TicketType!.Name}"
        };

        _context.Add(history);
      }

      if (oldTicket.TicketStatusId != newTicket.TicketStatusId)
      {
        TicketHistory history = new()
        {
          TicketId     = newTicket.Id,
          PropertyName = nameof(Ticket.TicketStatus),
          OldValue     = oldTicket.TicketStatus!.Name,
          NewValue     = newTicket.TicketStatus!.Name,
          Created      = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
          UserId       = userId,
          Description  = $"Ticket type changed to {newTicket.TicketStatus!.Name}"
        };

        _context.Add(history);
      }

      if (oldTicket.TicketPriorityId != newTicket.TicketPriorityId)
      {
        TicketHistory history = new()
        {
          TicketId     = newTicket.Id,
          PropertyName = nameof(Ticket.TicketPriority),
          OldValue     = oldTicket.TicketPriority!.Name,
          NewValue     = newTicket.TicketPriority!.Name,
          Created      = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
          UserId       = userId,
          Description  = $"Ticket type changed to {newTicket.TicketPriority !.Name}"
        };

        _context.Add(history);
      }

      if (!string.Equals(oldTicket.DeveloperUserId, newTicket.DeveloperUserId))
      {
        TicketHistory history = new()
        {
          TicketId     = newTicket.Id,
          PropertyName = "Developer",
          OldValue     = oldTicket.DeveloperUser!.FullName ?? "Unassigned",
          NewValue     = newTicket.DeveloperUser!.FullName ?? "Unassigned",
          Created      = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
          UserId       = userId,
          Description  = $"Ticket developer changed: {newTicket.DeveloperUser!.FullName ?? "Unassigned"}"
        };

        _context.Add(history);
      }

      await _context.SaveChangesAsync();
    }
  }

  public async Task AddHistoryAsync(int ticketId, string model, string userId)
  {
    var ticket = await _context.Tickets.FindAsync(ticketId);
    if (ticket is null) 
      return;

    var description = model.ToLower().Replace("ticket", ""); // TicketComment -> Comment
    description     = $"New {description} added to ticket: {ticket.Title}";

    TicketHistory history = new()
    {
      TicketId     = ticketId,
      PropertyName = model,
      OldValue     = string.Empty,
      NewValue     = string.Empty,
      Created      = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
      UserId       = userId,
      Description  = description
    };

    _context.Add(history);
    await _context.SaveChangesAsync();
  }

  public async Task<List<TicketHistory>> GetCompanyTicketHistoriesAsync(int companyId)
  {
    var company = await _context.Companies
      .Include            (c => c.Projects)
      .ThenInclude        (t => t.Tickets)
      .ThenInclude        (t => t.History)
      .ThenInclude        (h => h.User)
      .FirstOrDefaultAsync(c => c.Id == companyId);

    if (company is not null)
      //List<Project> projects = company.Projects.ToList();
      // [ ticket, ticket, ticket, ticket ]
      //List<Ticket> tickets = projects.SelectMany(p => p.Tickets).ToList();
      // [
      // [ticket, ticket,]
      // [ticket, ticket, ticket]
      // [ticket],
      //]
      //
      //var otherTickets = projects.Select(p => p.Tickets).ToList();
      // same as below code
      return company.Projects
        .SelectMany(p => p.Tickets)
        .SelectMany(t => t.History)
        .ToList    ();
    
    return new List<TicketHistory>();
  }

  public async Task<List<TicketHistory>> GetProjectTicketHistoriesAsync(int projectId, int companyId)
  {
    var project = await _context.Projects
      .Include            (p => p.Tickets)
      .ThenInclude        (t => t.History)
      .ThenInclude        (h => h.User)
      .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

    if (project is null) 
      return new List<TicketHistory>();

    var history = project.Tickets.SelectMany(t => t.History).ToList();

    return history;
  }
}