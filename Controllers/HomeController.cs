﻿using System.Diagnostics;
using BugTracker.Extensions;
using BugTracker.Models;
using BugTracker.Models.ViewModels;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BugTracker.Controllers;

[Authorize]
public class HomeController : Controller
{
  private readonly IBTCompanyService _companyService;
  private readonly IBTFileService _fileService;
  private readonly ILogger<HomeController> _logger;
  private readonly IBTProjectService _projectService;
  private readonly IBTRolesService _roleService;
  private readonly IBTTicketService _ticketService;

  public HomeController(ILogger<HomeController> logger, IBTProjectService projectService,
    IBTTicketService ticketService, IBTCompanyService companyService, IBTRolesService roleService,
    IBTFileService fileService)
  {
    _logger = logger;
    _projectService = projectService;
    _ticketService = ticketService;
    _companyService = companyService;
    _roleService = roleService;
    _fileService = fileService;
  }

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