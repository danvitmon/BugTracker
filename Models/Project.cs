using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.Models;

public class Project
{
  // Primary Key
  public int Id { get; set; }

  // Foreign Key
  [Display(Name = "Company")] public int CompanyId { get; set; }

  [Required]
  [StringLength(300, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
  public string? Name { get; set; }

  [Required]
  [StringLength(800, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
  public string? Description { get; set; }

  [DataType(DataType.DateTime)] public DateTime Created { get; set; }

  [DataType(DataType.DateTime)] public DateTime StartDate { get; set; }

  [DataType(DataType.DateTime)] public DateTime EndDate { get; set; }

  [Display(Name = "Project Priority")] public int ProjectPriorityId { get; set; }

  [NotMapped] public IFormFile? ImageFormFile { get; set; }

  public byte[]? ImageFileData { get; set; }

  public string? ImageFileType { get; set; }

  public bool Archived { get; set; }

  // Navigation Properties
  public virtual Company? Company { get; set; }

  public virtual ProjectPriority? ProjectPriority { get; set; }

  public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();

  public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();
}