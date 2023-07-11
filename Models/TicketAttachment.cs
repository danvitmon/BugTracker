using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BugTracker.Extensions;

namespace BugTracker.Models;

public class TicketAttachment
{
  public int Id { get; set; }

  [StringLength(500, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
  public string? Description { get; set; }

  [DataType(DataType.Date)]
  [Display(Name = "Created Date")]
  public DateTime Created { get; set; }

  //Image
  [NotMapped]
  [DisplayName("Select a file")]
  [DataType(DataType.Upload)]
  [MaxFileSize(1024 * 1024)]
  [AllowedExtensions(new[] { ".jpg", ".png", ".doc", ".docx", ".xls", ".xlsx", ".pdf" })]
  [Display(Name = "Attached File")]
  public IFormFile? FormFile { get; set; }

  public byte[]? FileData { get; set; }
  public string? FileType { get; set; }
  public string? FileName { get; set; }

  //Foreign Keys
  public int TicketId { get; set; }

  [Required] public string? BTUserId { get; set; }

  //Navigation
  public virtual Ticket? Ticket { get; set; }
  public virtual BTUser? BTUser { get; set; }
}