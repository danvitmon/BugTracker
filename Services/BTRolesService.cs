using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Models.Enums;
using BugTracker.Services.Interfaces;

namespace BugTracker.Services;

public class BTRolesService : IBTRolesService
{
  private readonly ApplicationDbContext _context;
  private readonly UserManager<BTUser>  _userManager;

  public BTRolesService(UserManager<BTUser> userManager, ApplicationDbContext context)
  {
    _userManager = userManager;
    _context     = context;
  }

  public async Task<bool> AddUserToRoleAsync(BTUser user, string roleName)
  {
    return (await _userManager.AddToRoleAsync(user, roleName)).Succeeded;
  }

  public async Task<List<IdentityRole>> GetRolesAsync()
  {
    return await _context.Roles.Where(r => r.Name != nameof(BTRoles.DemoUser)).ToListAsync();
  }

  public async Task<IEnumerable<string>> GetUserRolesAsync(BTUser user)
  {
    return await _userManager.GetRolesAsync(user);
  }

  public async Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId)
  {
    var usersInRoles = (await _userManager.GetUsersInRoleAsync(roleName)).ToList();

    return usersInRoles.Where(u => u.CompanyId == companyId).ToList();
  }

  public async Task<bool> IsUserInRole(BTUser member, string roleName)
  {
    return await _userManager.IsInRoleAsync(member, roleName);
  }

  public async Task<bool> RemoveUserFromRoleAsync(BTUser user, string roleName)
  {
    return (await _userManager.RemoveFromRoleAsync(user, roleName)).Succeeded;
  }

  public async Task<bool> RemoveUserFromRolesAsync(BTUser user, IEnumerable<string> roleNames)
  {
    return (await _userManager.RemoveFromRolesAsync(user, roleNames)).Succeeded;
  }
}