using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using BugTracker.Models.Enums;
using BugTracker.Services;
using BugTracker.Services.Interfaces;
using System.ComponentModel.Design;
using BugTracker.Extensions;
using BugTracker.Models.ViewModels;
using System.IO;

namespace BugTracker.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTTicketService _ticketService;
        private readonly IBTProjectService _projectService;
        private readonly IBTFileService _fileService;
        private readonly IBTCompanyService _companyService;
        private readonly IBTTicketHistoryService _ticketHistoryService;

        public TicketsController(UserManager<BTUser> userManager,
                                 IBTTicketService ticketService,
                                 IBTProjectService projectService,
                                 IBTFileService fileService,
                                 IBTCompanyService companyService,
                                 IBTTicketHistoryService ticketHistoryService)
        {
            _userManager = userManager;
            _ticketService = ticketService;
            _projectService = projectService;
            _fileService = fileService;
            _companyService = companyService;
            _ticketHistoryService = ticketHistoryService;
        }

        // GET: Tickets
        [Authorize]
        public async Task<IActionResult> Index()
        {
            int companyId = User.Identity!.GetCompanyId();

            

            return View(await _ticketService.GetTicketsByCompanyIdAsync(companyId));
        }

        // GET: AssignPM
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> AssignDeveloper(int? id)
        {
            if (id is null or 0)
            {
                return NotFound();
            }

            Ticket? ticket = await _ticketService.GetTicketByIdAsync(id.Value, User.Identity!.GetCompanyId());

            if (ticket == null)
            {
                return NotFound();
            }

            List<BTUser> developers = await _projectService.GetProjectMembersByRoleAsync(ticket.ProjectId, nameof(BTRoles.Developer), User.Identity!.GetCompanyId());
            BTUser? currentDev = ticket.DeveloperUser;

            AssignDeveloperViewModel viewModel = new AssignDeveloperViewModel()
            {
                Ticket = ticket,
                DevId = currentDev?.Id,
                DevList = new SelectList(developers, "Id", "FullName", currentDev?.Id)
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDeveloper(AssignDeveloperViewModel viewModel)
        {

            if (viewModel.Ticket?.Id is not null)
            {
                Ticket? ticket = await _ticketService.GetTicketByIdAsync(viewModel.Ticket.Id, User.Identity!.GetCompanyId());
                if (ticket is not null)
                {
                    ticket.DeveloperUserId = viewModel.DevId;

                    await _ticketService.UpdateTicketAsync(ticket, User.Identity!.GetCompanyId());

                    return RedirectToAction(nameof(Details), new { id = viewModel.Ticket!.Id });
                }
            }

            return BadRequest();
        }

        // GET: Tickets/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            var ticket = await _ticketService.GetTicketByIdAsync(id.Value, companyId);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            var projects = await _projectService.GetProjectsByCompanyIdAsync(User.Identity!.GetCompanyId());
            var priorities = await _ticketService.GetTicketPriorities();
            var types = await _ticketService.GetTicketTypes();

            ViewData["ProjectId"] = new SelectList(projects, "Id", "Name");
            ViewData["TicketPriorityId"] = new SelectList(priorities, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(types, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId")] Ticket ticket)
        {
            ModelState.Remove(nameof(Ticket.SubmitterUserId));

            if (ModelState.IsValid)
            {
                int companyId = User.Identity!.GetCompanyId();

                ticket.Created = DateTime.UtcNow;
                ticket.SubmitterUserId = _userManager.GetUserId(User);

                await _ticketService.AddTicketAsync(ticket);

                await _ticketHistoryService.AddHistoryAsync(null, ticket, _userManager.GetUserId(User)!);

                return RedirectToAction(nameof(Index));
            }


            List<Project> projects = await _projectService.GetProjectsByCompanyIdAsync(User.Identity!.GetCompanyId());

            ViewData["ProjectId"] = new SelectList(projects, "Id", "Name");
            ViewData["TicketPriorityId"] = new SelectList(await _ticketService.GetTicketPriorities(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _ticketService.GetTicketTypes(), "Id", "Name");
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _ticketService.GetTicketByIdAsync(id.Value, User.Identity!.GetCompanyId());
            if (ticket == null)
            {
                return NotFound();
            }

            var priorities = await _ticketService.GetTicketPriorities();
            var statuses = await _ticketService.GetTicketStatuses();
            var types = await _ticketService.GetTicketTypes();

            ViewData["TicketPriorityId"] = new SelectList(priorities, "Id", "Name");
            ViewData["TicketStatusId"] = new SelectList(statuses, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(types, "Id", "Name");
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, User.Identity!.GetCompanyId());
                    ticket.Created = DateTime.UtcNow;

                    await _ticketService.UpdateTicketAsync(ticket, User.Identity!.GetCompanyId());
                    ticket = (await _ticketService.GetTicketByIdAsync(ticket.Id, User.Identity!.GetCompanyId()))!;

                    await _ticketHistoryService.AddHistoryAsync(oldTicket, ticket, _userManager.GetUserId(User)!);
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var priorities = await _ticketService.GetTicketPriorities();
            var statuses = await _ticketService.GetTicketStatuses();
            var types = await _ticketService.GetTicketTypes();

            ViewData["TicketPriorityId"] = new SelectList(priorities, "Id", "Name");
            ViewData["TicketStatusId"] = new SelectList(statuses, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(types, "Id", "Name");
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        [Authorize]
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _ticketService.GetTicketByIdAsync(id.Value, User.Identity!.GetCompanyId());
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id, User.Identity!.GetCompanyId());
            if (ticket != null)
            {
                await _ticketService.ArchiveTicketAsync(ticket, User.Identity!.GetCompanyId());
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Archived(int? id)
        {
            int companyId = User.Identity!.GetCompanyId();



            return View(await _ticketService.GetArchivedTicketsAsync(companyId));
        }

        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _ticketService.GetTicketByIdAsync(id.Value, User.Identity!.GetCompanyId());
            if (ticket == null || ticket.ArchivedByProject)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id, User.Identity!.GetCompanyId());
            if (ticket != null && !ticket.ArchivedByProject)
            {
                await _ticketService.RestoreTicketAsync(ticket, User.Identity!.GetCompanyId());
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> MyTickets()
        {
            return View(await _ticketService.GetUserTicketsAsync(_userManager.GetUserId(User)!));
        }

        public async Task<IActionResult> AllTickets()
        {
            return View(await _ticketService.GetTicketsByCompanyIdAsync(User.Identity!.GetCompanyId()));
        }

        public async Task<IActionResult> ArchivedTickets()
        {
            return View(await _ticketService.GetArchivedTicketsAsync(User.Identity!.GetCompanyId()));
        }

        public async Task<IActionResult> UnassignedTickets()
        {
            return View(await _ticketService.GetUnassignedTicketsAsync(User.Identity!.GetCompanyId()));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
        {
            string statusMessage;
            ModelState.Remove("BTUserId");

            if (ModelState.IsValid && ticketAttachment.FormFile != null)
            {
                ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                ticketAttachment.FileType = ticketAttachment.FormFile.ContentType;

                ticketAttachment.BTUserId = _userManager.GetUserId(User);
                ticketAttachment.Created = DateTime.UtcNow;

                await _ticketService.AddTicketAttachmentAsync(ticketAttachment);
                statusMessage = "Success: New attachment added to Ticket.";

                await _ticketHistoryService.AddHistoryAsync(ticketAttachment.TicketId, nameof(TicketAttachment), ticketAttachment.BTUserId!);
            }
            else
            {
                statusMessage = "Error: Invalid data.";
            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
        }

        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttachment? ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
            if (ticketAttachment == null)
            {
                return NotFound();
            }
            string fileName = ticketAttachment.FileName!;
            byte[] fileData = ticketAttachment.FileData!;
            string ext = Path.GetExtension(fileName).Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{ext}");
        }

        public async Task<IActionResult> AddTicketComment(TicketComment? ticketComment)
        {
            if (ticketComment is not null && ticketComment.TicketId is not 0 && !string.IsNullOrEmpty(ticketComment.Comment))
            {
                Ticket? ticket = await _ticketService.GetTicketByIdAsync(ticketComment.TicketId, User.Identity!.GetCompanyId());
                if (ticket is null) return NotFound();

                string userId = _userManager.GetUserId(User);

                if (User.IsInRole(nameof(BTRoles.Admin))
                    || (User.IsInRole(nameof(BTRoles.ProjectManager)) && ticket.Project?.Members.Any(m => m.Id == userId) == true)
                    || ticket.DeveloperUserId == userId
                    || ticket.SubmitterUserId == userId)
                {
                    ticketComment.Created = DateTime.UtcNow;
                    ticketComment.UserId = userId;

                    await _ticketService.AddTicketCommentAsync(ticketComment);

                    await _ticketHistoryService.AddHistoryAsync(ticketComment.TicketId, nameof(TicketComment), ticketComment.UserId!);
                }

                return RedirectToAction(nameof(Details), new { id = ticket.Id });
            }
            
            return BadRequest();
        }
    }
}