using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Models.ViewModels;

public class AssignDeveloperViewModel
{
  public Ticket?     Ticket  { get; set; }
  public SelectList? DevList { get; set; }
  public string?     DevId   { get; set; }
}