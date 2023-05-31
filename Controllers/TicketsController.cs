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

namespace BugTracker.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTTicketService _ticketService;
        private readonly IBTProjectService _projectService;

        public TicketsController(UserManager<BTUser> userManager, IBTTicketService ticketService, IBTProjectService projectService)
        {
            _userManager = userManager;
            _ticketService = ticketService;
            _projectService = projectService;
        }

        // GET: Tickets
        [Authorize]
        public async Task<IActionResult> Index()
        {
            int companyId = User.Identity!.GetCompanyId();

            

            return View(await _ticketService.GetTicketsByCompanyIdAsync(companyId));
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
            BTUser? user = await _userManager.GetUserAsync(User);

            var projects = await _projectService.GetAllProjectsByCompanyIdAsync(user!.CompanyId);
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
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ProjectId,TicketTypeId,TicketPriorityId,DeveloperUserId")] Ticket ticket)
        {
            BTUser? user = await _userManager.GetUserAsync(User);
            ModelState.Remove("SubmitterUserId");

            if (ModelState.IsValid)
            {
                ticket.Created = DateTime.UtcNow;

                ticket.SubmitterUserId = user!.Id;

                var statuses = await _ticketService.GetTicketStatuses();

                foreach (TicketStatus ts in statuses)
                {
                    if (ts.Name == nameof(BTTicketStatuses.New))
                    {
                        ticket.TicketStatusId = ts.Id;
                        break;
                    }
                }

                await _ticketService.AddTicketAsync(ticket);
                return RedirectToAction(nameof(Index));
            }

            var projects = await _projectService.GetAllProjectsByCompanyIdAsync(user!.CompanyId);
            var priorities = await _ticketService.GetTicketPriorities();
            var types = await _ticketService.GetTicketTypes();

            ViewData["ProjectId"] = new SelectList(projects, "Id", "Name");
            ViewData["TicketPriorityId"] = new SelectList(priorities, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(types, "Id", "Name");
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

            BTUser? user = await _userManager.GetUserAsync(User);

            var ticket = await _ticketService.GetTicketByIdAsync(id.Value, user!.CompanyId);
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
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    BTUser? user = await _userManager.GetUserAsync(User);

                    ticket.Created = DateTime.SpecifyKind(ticket.Created, DateTimeKind.Utc);
                    ticket.Updated = DateTime.UtcNow;

                    await _ticketService.UpdateTicketAsync(ticket, user!.CompanyId);
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

            BTUser? user = await _userManager.GetUserAsync(User);

            var ticket = await _ticketService.GetTicketByIdAsync(id.Value, user!.CompanyId);
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
            BTUser? user = await _userManager.GetUserAsync(User);

            var ticket = await _ticketService.GetTicketByIdAsync(id, user!.CompanyId);
            if (ticket != null)
            {
                await _ticketService.ArchiveTicketAsync(ticket, user.CompanyId);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}