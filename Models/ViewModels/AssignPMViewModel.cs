using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModels;

public class AssignPmViewModel
{
  public Project?    Project  { get; set; }
  public SelectList? PmList   { get; set; }
  public string?     PmId     { get; set; }
  public string?     MemberId { get; set; }
}