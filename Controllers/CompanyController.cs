using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using BugTracker.Extensions;
using BugTracker.Models;
using BugTracker.Models.Enums;
using BugTracker.Models.ViewModels;
using BugTracker.Services.Interfaces;

namespace BugTracker.Controllers;

[Authorize]
public class CompanyController : Controller
{
  private readonly IBTCompanyService   _companyService;
  private readonly IBTRolesService     _rolesService;
  private readonly UserManager<BTUser> _userManager;

  public CompanyController(IBTCompanyService companyService, IBTRolesService rolesService, UserManager<BTUser> userManager)
  {
    _companyService = companyService;
    _rolesService   = rolesService;
    _userManager    = userManager;
  }

  // GET: Companies/Index
  public async Task<IActionResult> Index()
  {
    var companyId = User.Identity!.GetCompanyId();
    var company   = await _companyService.GetCompanyInfoAsync(companyId);

    if (company == null) 
      return NotFound();

    return View(company);
  }

  [HttpGet]
  [Authorize(Roles = nameof(BTRoles.Admin))]
  public async Task<IActionResult> ManageUserRoles()
  {
    var members = await _companyService.GetUsersByCompanyIdAsync(User.Identity!.GetCompanyId());
    var roles   = await _rolesService.GetRolesAsync();

    List<ManageUserRolesViewModel> model = new();

    foreach (var member in members)
      if (member.Id != _userManager.GetUserId(User) && !await _rolesService.IsUserInRole(member, nameof(BTRoles.DemoUser)))
      {
        var userRoles = await _rolesService.GetUserRolesAsync(member);

        ManageUserRolesViewModel viewModel = new()
        {
          Roles = new SelectList(roles, "Name", "Name", userRoles.FirstOrDefault()),
          User  = member
        };

        model.Add(viewModel);
      }

    return View(model);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  [Authorize(Roles = nameof(BTRoles.Admin))]
  public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel viewModel)
  {
    var selectedRole = viewModel.SelectedRole;
    var userId       = viewModel.User?.Id;

    if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(selectedRole)) 
      return NotFound();

    var user = await _userManager.FindByIdAsync(userId);
    if (user == null)
      return NotFound();

    var currentRoles = await _rolesService.GetUserRolesAsync(user);

    if (await _rolesService.RemoveUserFromRolesAsync(user, currentRoles))
      await _rolesService.AddUserToRoleAsync(user, selectedRole);

    return RedirectToAction(nameof(ManageUserRoles));
  }
}