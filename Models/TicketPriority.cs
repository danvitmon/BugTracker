using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models;

public class TicketPriority
{
  public int Id { get; set; }

  [StringLength(300, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
  public string? Name { get; set; }
}