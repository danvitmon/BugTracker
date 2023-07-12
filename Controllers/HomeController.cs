using System.Diagnostics;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using BugTracker.Extensions;
using BugTracker.Models;
using BugTracker.Models.ViewModels;

namespace BugTracker.Controllers;

[Authorize]
public class HomeController : Controller
{
  public IActionResult Dashboard()
  {
    var viewmodel = new DashboardViewModel();
    int? companyId = User.Identity!.GetCompanyId();

    return View();
  }

  [AllowAnonymous]
  public IActionResult Landing()
  {
    return View();
  }

  public IActionResult Index()
  {
    return View();
  }

  [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
  public IActionResult Error()
  {
    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
  }
}