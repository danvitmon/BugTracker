using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models;

public class Ticket
{
  // Primary Key
  public int Id { get; set; }

  [Required]
  [StringLength(300, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
  public string? Title { get; set; }

  [Required]
  [StringLength(300, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
  public string? Description { get; set; }

  [DataType(DataType.DateTime)] public DateTime  Created { get; set; }
  [DataType(DataType.DateTime)] public DateTime? Updated { get; set; }

  public bool Archived          { get; set; }
  public bool ArchivedByProject { get; set; }

  // Foreign Keys
  [Display(Name = "Project")]   public int     ProjectId        { get; set; }
  [Display(Name = "Type")]      public int     TicketTypeId     { get; set; }
  [Display(Name = "Status")]    public int     TicketStatusId   { get; set; }
  [Display(Name = "Priority")]  public int     TicketPriorityId { get; set; }
  [Display(Name = "Developer")] public string? DeveloperUserId  { get; set; }
  [Required]                    public string? SubmitterUserId  { get; set; }

  // Navigation Properties
  public virtual Project?        Project        { get; set; }
  public virtual TicketType?     TicketType     { get; set; }
  public virtual TicketPriority? TicketPriority { get; set; }
  public virtual TicketStatus?   TicketStatus   { get; set; }
  public virtual BTUser?         DeveloperUser  { get; set; }
  public virtual BTUser?         SubmitterUser  { get; set; }

  public virtual ICollection<TicketHistory>    History     { get; set; } = new HashSet<TicketHistory>();
  public virtual ICollection<TicketComment>    Comments    { get; set; } = new HashSet<TicketComment>();
  public virtual ICollection<TicketAttachment> Attachments { get; set; } = new HashSet<TicketAttachment>();
}