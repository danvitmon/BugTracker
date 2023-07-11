using BugTracker.Extensions;
using BugTracker.Models;
using BugTracker.Models.Enums;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Controllers;

[Authorize(Roles = nameof(BTRoles.Admin))]
public class InvitesController : Controller
{
  private readonly IBTCompanyService _companyService;
  private readonly IEmailSender _emailSender;
  private readonly IBTInviteService _inviteService;
  private readonly IBTProjectService _projectService;
  private readonly IDataProtector _protector;
  private readonly string _protectorPurpose;
  private readonly UserManager<BTUser> _userManager;

  public InvitesController(IBTInviteService inviteService,
    IBTProjectService projectService,
    IBTCompanyService companyService,
    IEmailSender emailSender,
    UserManager<BTUser> userManager,
    IDataProtectionProvider protectionProvider)
  {
    _projectService = projectService;
    _companyService = companyService;
    _emailSender = emailSender;
    _userManager = userManager;
    _inviteService = inviteService;

    _protectorPurpose = "Blackjack/21";
    _protector = protectionProvider.CreateProtector(_protectorPurpose);
  }

  // GET: Invites/Create
  public async Task<IActionResult> Create()
  {
    var companyProjects = await _projectService.GetProjectsByCompanyIdAsync(User.Identity!.GetCompanyId());

    ViewData["ProjectId"] = new SelectList(companyProjects, "Id", "Name");
    return View();
  }

  // POST: Invites/Create
  // To protect from overposting attacks, enable the specific properties you want to bind to.
  // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Create(
    [Bind("ProjectId,InviteeEmail,InviteeFirstName,InviteeLastName,Message")] Invite invite)
  {
    var companyId = User.Identity!.GetCompanyId();

    ModelState.Remove(nameof(Invite.InvitorId));

    if (ModelState.IsValid)
      // assign invite values
      // save it
      // send the invite email
      var guid = Guid.NewGuid();

      invite.CompanyToken = guid;
      invite.CompanyId = companyId;
      invite.InviteDate = DateTime.UtcNow;
      invite.InvitorId = _userManager.GetUserId(User);
      invite.IsValid = true;
      await _inviteService.AddNewInviteAsync(invite);

      var token = _protector.Protect(guid.ToString());
      var email = _protector.Protect(invite.InviteeEmail!);
      var company = _protector.Protect(companyId.ToString());

      var callbackUrl = Url.Action("ProcessInvite", "Invites", new { token, email, company }, Request.Scheme);

      var body = $@"<h4>You've been invited to join the bug tracker!</h4><br />
                                         {invite.Message}<br /><br />
                                         <a href=""{callbackUrl}"">Click here</a> to join our team.";

      var subject = "You've been invited to join the bug tracker";

      await _emailSender.SendEmailAsync(invite.InviteeEmail!, subject, body);

      return RedirectToAction("Index", "Home", new { SwalMessage = "Invite Sent!" });

    var companyProjects = await _projectService.GetProjectsByCompanyIdAsync(User.Identity!.GetCompanyId());
    ViewData["ProjectId"] = new SelectList(companyProjects, "Id", "Name");
    return View(invite);
  }

  [AllowAnonymous]
  public async Task<IActionResult> ProcessInvite(string? token, string? email, string? company)
  {
    if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(company)) return NotFound();

    var companyToken = Guid.Parse(_protector.Unprotect(token));
    var inviteeEmail = _protector.Unprotect(email);
    var companyId = int.Parse(_protector.Unprotect(company));

    var invite = await _inviteService.GetInviteAsync(companyToken, inviteeEmail, companyId);

    if (invite == null) return NotFound();

    return View(invite);
  }
}