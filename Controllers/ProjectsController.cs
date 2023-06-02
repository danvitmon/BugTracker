using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Models.Enums;
using Microsoft.AspNetCore.Identity;
using BugTracker.Services.Interfaces;
using System.ComponentModel.Design;
using BugTracker.Extensions;
using BugTracker.Models.ViewModels;

namespace BugTracker.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTFileService _fileService;
        private readonly IBTProjectService _projectService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTCompanyService _companyService;


        public ProjectsController(UserManager<BTUser> userManager,
                                  IBTFileService fileService,
                                  IBTProjectService projectService,
                                  IBTRolesService rolesService,
                                  IBTCompanyService companyService)
        {
            _userManager = userManager;
            _fileService = fileService;
            _projectService = projectService;
            _rolesService = rolesService;
            _companyService = companyService;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            return View(await _projectService.GetProjectsByCompanyIdAsync(User.Identity!.GetCompanyId()));
        }

        [HttpGet]
        [Authorize(Roles = nameof(BTRoles.Admin))]
        public async Task<IActionResult> AssignPM(int? id)
        {
            if (id is null or 0)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            Project? project = await _projectService.GetProjectByIdAsync(id.Value, User.Identity!.GetCompanyId());

            if (project is null)
            {
                return NotFound();
            }

            List<BTUser> projectManagers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId);
            BTUser? currentPM = await _projectService.GetProjectManagerAsync(id.Value, companyId);

            AssignPMViewModel viewModel = new AssignPMViewModel()
            {
                Project = project,
                PMId = currentPM?.Id,
                PMList = new SelectList(projectManagers, "Id", "FullName", currentPM?.Id)
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = nameof(BTRoles.Admin))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPM(AssignPMViewModel viewModel)
        {
            if (viewModel.Project?.Id != null)
            {
                if (string.IsNullOrEmpty(viewModel.PMId))
                {
                    await _projectService.RemoveProjectManagerAsync(viewModel.Project.Id, User.Identity!.GetCompanyId());
                }
                else
                {
                    await _projectService.AddProjectManagerAsync(viewModel.PMId, viewModel.Project.Id, User.Identity!.GetCompanyId());
                }

                return RedirectToAction(nameof(Details), new { id = viewModel.Project!.Id });
            }

            return BadRequest();
        }

        // GET: AssignPM
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> EditMembers(int? id)
        {
            if (id is null or 0)
            {
                return NotFound();
            }

            Project? project = await _projectService.GetProjectByIdAsync(id.Value, User.Identity.GetCompanyId());

            if (project == null)
            {
                return NotFound();
            }

            List<BTUser> members = await _companyService.GetUsersByCompanyIdAsync(User.Identity!.GetCompanyId());
            List<BTUser> current = new();
            List<BTUser> unassigned = new();

            foreach (BTUser member in members)
            {
                if (await _rolesService.IsUserInRole(member, nameof(BTRoles.Developer)) || await _rolesService.IsUserInRole(member, nameof(BTRoles.Submitter)))
                {
                    if (project.Members.Contains(member))
                    {
                        current.Add(member);
                    }
                    else
                    {
                        unassigned.Add(member);
                    }
                }
            }

            AssignMemberViewModel viewModel = new AssignMemberViewModel()
            {
                Project = project,
                CurrentList = new SelectList(current, "Id", "FullName"),
                UnassignedList = new SelectList(unassigned, "Id", "FullName")
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMembers(AssignMemberViewModel viewModel)
        {
            if (viewModel.Project?.Id is not null)
            {
                Project? project = await _projectService.GetProjectByIdAsync(viewModel.Project.Id, User.Identity!.GetCompanyId());

                if (project == null)
                {
                    return BadRequest();
                }

                List<BTUser> members = await _companyService.GetUsersByCompanyIdAsync(User.Identity!.GetCompanyId());

                if (!string.IsNullOrEmpty(viewModel.MemberId))
                {
                    BTUser? member = members.FirstOrDefault(u => u.Id == viewModel.MemberId);
                    if (member is not null)
                    {
                        if (project.Members.Contains(member))
                        {
                            await _projectService.RemoveMemberFromProjectAsync(member, project.Id, User.Identity!.GetCompanyId());
                        }
                        else
                        {
                            await _projectService.AddMemberToProjectAsync(member, project.Id, User.Identity!.GetCompanyId());
                        }

                    }
                }

                List<BTUser> current = new();
                List<BTUser> unassigned = new();

                foreach (BTUser member in members)
                {
                    if (await _rolesService.IsUserInRole(member, nameof(BTRoles.Developer)) || await _rolesService.IsUserInRole(member, nameof(BTRoles.Submitter)))
                    {
                        if (project.Members.Contains(member))
                        {
                            current.Add(member);
                        }
                        else
                        {
                            unassigned.Add(member);
                        }
                    }
                }

                AssignMemberViewModel newViewModel = new AssignMemberViewModel()
                {
                    Project = project,
                    CurrentList = new SelectList(current, "Id", "FullName"),
                    UnassignedList = new SelectList(unassigned, "Id", "FullName")
                };

                return View(newViewModel);
            }

            return BadRequest();
        }

        // GET: Projects/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Project? project = await _projectService.GetProjectByIdAsync(id.Value, User.Identity!.GetCompanyId());

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Create()
        {
            var priorities = await _projectService.GetProjectPrioritiesAsync();
            ViewData["ProjectPriorityId"] = new SelectList(priorities, "Id", "Name");
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
                project.Created = DateTime.UtcNow;
                project.StartDate = DateTime.SpecifyKind(project.StartDate, DateTimeKind.Utc);
                project.EndDate = DateTime.SpecifyKind(project.EndDate, DateTimeKind.Utc);

                BTUser? user = await _userManager.GetUserAsync(User);
                project.CompanyId = user!.CompanyId;

                if (project.ImageFormFile is not null)
                {
                    project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                    project.ImageFileType = project.ImageFormFile.ContentType;
                }

                if (User.IsInRole(nameof(BTRoles.ProjectManager)))
                {
                    project.Members.Add(user);
                }

                await _projectService.AddProjectAsync(project);
                return RedirectToAction(nameof(Index));
            }

            var priorities = await _projectService.GetProjectPrioritiesAsync();
            ViewData["ProjectPriorityId"] = new SelectList(priorities, "Id", "Name");
            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            BTUser? user = await _userManager.GetUserAsync(User);

            var project = await _projectService.GetProjectByIdAsync(id.Value, user!.CompanyId);
            if (project == null)
            {
                return NotFound();
            }

            var priorities = await _projectService.GetProjectPrioritiesAsync();
            ViewData["ProjectPriorityId"] = new SelectList(priorities, "Id", "Name");
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
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    project.Created = DateTime.SpecifyKind(project.Created, DateTimeKind.Utc);
                    project.StartDate = DateTime.SpecifyKind(project.StartDate, DateTimeKind.Utc);
                    project.EndDate = DateTime.SpecifyKind(project.EndDate, DateTimeKind.Utc);

                    if (project.ImageFormFile is not null)
                    {
                        project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                        project.ImageFileType = project.ImageFormFile.ContentType;
                    }

                    BTUser? user = await _userManager.GetUserAsync(User);

                    await _projectService.UpdateProjectAsync(project, user!.CompanyId);
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var priorities = await _projectService.GetProjectPrioritiesAsync();
            ViewData["ProjectPriorityId"] = new SelectList(priorities, "Id", "Name");
            return View(project);
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _projectService.GetProjectByIdAsync(id.Value, User.Identity!.GetCompanyId());

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id, User.Identity!.GetCompanyId());

            if (project != null)
            {
                await _projectService.ArchiveProjectAsync(project, User.Identity!.GetCompanyId());
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _projectService.GetProjectByIdAsync(id.Value, User.Identity!.GetCompanyId());

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id, User.Identity!.GetCompanyId());
            if (project != null)
            {
                await _projectService.RestoreProjectAsync(project, User.Identity!.GetCompanyId());
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> MyProjects()
        {
            return View(await _projectService.GetUserProjectsAsync(_userManager.GetUserId(User)!));
        }

        public async Task<IActionResult> AllProjects()
        {
            return View(await _projectService.GetProjectsByCompanyIdAsync(User.Identity!.GetCompanyId()));
        }

        public async Task<IActionResult> ArchivedProjects()
        {
            return View(await _projectService.GetArchivedProjectsAsync(User.Identity!.GetCompanyId()));
        }

        public async Task<IActionResult> UnassignedProjects()
        {
            return View(await _projectService.GetUnassignedProjectsAsync(User.Identity!.GetCompanyId()));
        }
    }
}