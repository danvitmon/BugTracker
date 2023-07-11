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
public class ProjectsController : Controller
{
  private readonly IBTCompanyService   _companyService;
  private readonly IBTFileService      _fileService;
  private readonly IBTProjectService   _projectService;
  private readonly IBTRolesService     _rolesService;
  private readonly UserManager<BTUser> _userManager;


  public ProjectsController(UserManager<BTUser> userManager,
                            IBTFileService      fileService,
                            IBTProjectService   projectService,
                            IBTRolesService     rolesService,
                            IBTCompanyService   companyService)
  {
    _userManager    = userManager;
    _fileService    = fileService;
    _projectService = projectService;
    _rolesService   = rolesService;
    _companyService = companyService;
  }

  // GET: Projects
  public async Task<IActionResult> Index() => View(await _projectService.GetProjectsByCompanyIdAsync(User.Identity!.GetCompanyId()));

  [HttpGet]
  [Authorize(Roles = nameof(BTRoles.Admin))]
  public async Task<IActionResult> AssignPm(int? id)
  {
    if (id is null or 0)
      return NotFound();

    var companyId = User.Identity!.GetCompanyId();
    var project   = await _projectService.GetProjectByIdAsync(id.Value, User.Identity!.GetCompanyId());

    if (project is null)
      return NotFound();

    var projectManagers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId);
    var currentPm       = await _projectService.GetProjectManagerAsync(id.Value, companyId);

    var viewModel = new AssignPmViewModel
    {
      Project = project,
      PmId    = currentPm?.Id,
      PmList  = new SelectList(projectManagers, nameof(BTUser.Id), nameof(BTUser.FullName), currentPm?.Id)
    };

    return View(viewModel);
  }

  [HttpPost]
  [Authorize(Roles = nameof(BTRoles.Admin))]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> AssignPm(AssignPmViewModel viewModel)
  {
    if (viewModel.Project?.Id == null)
      return BadRequest();

    if (string.IsNullOrEmpty(viewModel.PmId))
      await _projectService.RemoveProjectManagerAsync(viewModel.Project.Id, User.Identity!.GetCompanyId());
    else
      await _projectService.AddProjectManagerAsync(viewModel.PmId, viewModel.Project.Id,
        User.Identity!.GetCompanyId());

    return RedirectToAction(nameof(Details), new { id = viewModel.Project!.Id });
   
  }

  // GET: AssignPm
  [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
  public async Task<IActionResult> EditMembers(int? id)
  {
    if (id is null or 0)
      return NotFound();

    var project = await _projectService.GetProjectByIdAsync(id.Value, User.Identity!.GetCompanyId());

    if (project == null)
      return NotFound();

    var          members    = await _companyService.GetUsersByCompanyIdAsync(User.Identity!.GetCompanyId());
    List<BTUser> current    = new();
    List<BTUser> unassigned = new();

    foreach (var member in members)
      if (await _rolesService.IsUserInRole(member, nameof(BTRoles.Developer)) ||
          await _rolesService.IsUserInRole(member, nameof(BTRoles.Submitter)))
      {
        if (project.Members.Contains(member))
          current.Add(member);
        else
          unassigned.Add(member);
      }

    var viewModel = new AssignMemberViewModel
    {
      Project        = project,
      CurrentList    = new SelectList(current,    nameof(BTUser.Id), nameof(BTUser.FullName)),
      UnassignedList = new SelectList(unassigned, nameof(BTUser.Id), nameof(BTUser.FullName))
    };

    return View(viewModel);
  }

  [HttpPost]
  [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> EditMembers(AssignMemberViewModel viewModel)
  {
    if (viewModel.Project?.Id == null)
      return BadRequest();

    var project = await _projectService.GetProjectByIdAsync(viewModel.Project.Id, User.Identity!.GetCompanyId());

    if (project == null)
      return BadRequest();

    var members = await _companyService.GetUsersByCompanyIdAsync(User.Identity!.GetCompanyId());

    if (!string.IsNullOrEmpty(viewModel.MemberId))
    {
      var member = members.FirstOrDefault(u => u.Id == viewModel.MemberId);

      if (member is not null)
      {
        if (project.Members.Contains(member))
          await _projectService.RemoveMemberFromProjectAsync(member, project.Id, User.Identity!.GetCompanyId());
        else
          await _projectService.AddMemberToProjectAsync(member, project.Id, User.Identity!.GetCompanyId());
      }
    }

    List<BTUser> current    = new();
    List<BTUser> unassigned = new();

    foreach (var member in members)
      if (await _rolesService.IsUserInRole(member, nameof(BTRoles.Developer)) ||
          await _rolesService.IsUserInRole(member, nameof(BTRoles.Submitter)))
      {
        if (project.Members.Contains(member))
          current.Add(member);
        else
          unassigned.Add(member);
      }

    var newViewModel = new AssignMemberViewModel
    {
      Project        = project,
      CurrentList    = new SelectList(current,    nameof(BTUser.Id), nameof(BTUser.FullName)),
      UnassignedList = new SelectList(unassigned, nameof(BTUser.Id), nameof(BTUser.FullName))
    };

    return View(newViewModel);
  }

  // GET: Projects/Details/5
  [Authorize]
  public async Task<IActionResult> Details(int? id)
  {
    if (id == null)
      return NotFound();

    var project = await _projectService.GetProjectByIdAsync(id.Value, User.Identity!.GetCompanyId());

    return project == null ? NotFound() : View(project);
  }

  // GET: Projects/Create
  [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
  public async Task<IActionResult> Create()
  {
    var priorities = await _projectService.GetProjectPrioritiesAsync();
    ViewData["ProjectPriorityId"] = new SelectList(priorities, nameof(ProjectPriority.Id), nameof(ProjectPriority.Name));
    
    return View();
  }

  // POST: Projects/Create
  // To protect from overposting attacks, enable the specific properties you want to bind to.
  // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
  [HttpPost]
  [ValidateAntiForgeryToken]
  [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
  public async Task<IActionResult> Create([Bind("Name,Description,StartDate,EndDate,ProjectPriorityId,ImageFormFile")] Project project)
  {
    ModelState.Remove("CompanyId");

    if (ModelState.IsValid)
    {
      project.Created   = DateTime.UtcNow;
      project.StartDate = DateTime.SpecifyKind(project.StartDate, DateTimeKind.Utc);
      project.EndDate   = DateTime.SpecifyKind(project.EndDate, DateTimeKind.Utc);

      var user          = await _userManager.GetUserAsync(User);
      project.CompanyId = user!.CompanyId;

      if (project.ImageFormFile is not null)
      {
        project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
        project.ImageFileType = project.ImageFormFile.ContentType;
      }

      if (User.IsInRole(nameof(BTRoles.ProjectManager)))
        project.Members.Add(user);

      await _projectService.AddProjectAsync(project);

      return RedirectToAction(nameof(Index));
    }

    var priorities = await _projectService.GetProjectPrioritiesAsync();
    ViewData["ProjectPriorityId"] = new SelectList(priorities, nameof(ProjectPriority.Id), nameof(ProjectPriority.Name));
    
    return View(project);
  }

  // GET: Projects/Edit/5
  [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
  public async Task<IActionResult> Edit(int? id)
  {
    if (id == null)
      return NotFound();

    var user    = await _userManager.GetUserAsync(User);
    var project = await _projectService.GetProjectByIdAsync(id.Value, user!.CompanyId);

    if (project == null)
      return NotFound();

    var priorities = await _projectService.GetProjectPrioritiesAsync();
    ViewData["ProjectPriorityId"] = new SelectList(priorities, nameof(ProjectPriority.Id), nameof(ProjectPriority.Name));
    
    return View(project);
  }

  // POST: Projects/Edit/5
  // To protect from overposting attacks, enable the specific properties you want to bind to.
  // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
  [HttpPost]
  [ValidateAntiForgeryToken]
  [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
  public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Created,StartDate,EndDate,Archived,ImageFormFile,ImageFileData,ImageFileType,CompanyId,ProjectPriorityId")] Project project)
  {
    if (id != project.Id) 
      return NotFound();

    if (ModelState.IsValid)
    {
      project.Created   = DateTime.SpecifyKind(project.Created, DateTimeKind.Utc);
      project.StartDate = DateTime.SpecifyKind(project.StartDate, DateTimeKind.Utc);
      project.EndDate   = DateTime.SpecifyKind(project.EndDate, DateTimeKind.Utc);

      if (project.ImageFormFile is not null)
      {
        project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
        project.ImageFileType = project.ImageFormFile.ContentType;
      }

      var user = await _userManager.GetUserAsync(User);

      await _projectService.UpdateProjectAsync(project, user!.CompanyId);
      
      return RedirectToAction(nameof(Index));
    }

    var priorities = await _projectService.GetProjectPrioritiesAsync();
    ViewData["ProjectPriorityId"] = new SelectList(priorities, nameof(ProjectPriority.Id), nameof(ProjectPriority.Name));
    
    return View(project);
  }

  // GET: Projects/Delete/5
  [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
  public async Task<IActionResult> Archive(int? id)
  {
    if (id == null) 
      return NotFound();

    var project = await _projectService.GetProjectByIdAsync(id.Value, User.Identity!.GetCompanyId());

    if (project == null) 
      return NotFound();

    return View(project);
  }

  // POST: Projects/Delete/5
  [HttpPost]
  [ActionName("Archive")]
  [ValidateAntiForgeryToken]
  [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
  public async Task<IActionResult> ArchiveConfirmed(int id)
  {
    var project = await _projectService.GetProjectByIdAsync(id, User.Identity!.GetCompanyId());

    if (project != null) 
      await _projectService.ArchiveProjectAsync(project, User.Identity!.GetCompanyId());

    return RedirectToAction(nameof(Index));
  }

  // GET: Projects/Delete/5
  [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
  public async Task<IActionResult> Restore(int? id)
  {
    if (id == null) 
      return NotFound();

    var project = await _projectService.GetProjectByIdAsync(id.Value, User.Identity!.GetCompanyId());

    if (project == null) 
      return NotFound();

    return View(project);
  }

  // POST: Projects/Delete/5
  [HttpPost]
  [ActionName("Restore")]
  [ValidateAntiForgeryToken]
  [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
  public async Task<IActionResult> RestoreConfirmed(int id)
  {
    var project = await _projectService.GetProjectByIdAsync(id, User.Identity!.GetCompanyId());
    if (project != null) 
      await _projectService.RestoreProjectAsync(project, User.Identity!.GetCompanyId());

    return RedirectToAction(nameof(Index));
  }

  public async Task<IActionResult> MyProjects()         => View(await _projectService.GetUserProjectsAsync      (_userManager.GetUserId(User)!));
  public async Task<IActionResult> ArchivedProjects()   => View(await _projectService.GetArchivedProjectsAsync  (User.Identity!.GetCompanyId()));
  public async Task<IActionResult> UnassignedProjects() => View(await _projectService.GetUnassignedProjectsAsync(User.Identity!.GetCompanyId()));
}