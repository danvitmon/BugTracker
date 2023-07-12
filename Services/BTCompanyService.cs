using Microsoft.EntityFrameworkCore;

using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;

namespace BugTracker.Services;

public class BTCompanyService : IBTCompanyService
{
  private readonly ApplicationDbContext _context;

  public BTCompanyService(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task<Company?> GetCompanyInfoAsync(int companyId)
  {
    var company = await _context.Companies
      .Include(c => c.Members)
      .Include(c => c.Projects)
      .ThenInclude(p => p.Tickets)
      .Include(c => c.Invites)
      .FirstOrDefaultAsync(c => c.Id == companyId);
    
      return company;
  }

  public async Task<List<BTUser>> GetCompanyMembersAsync(int companyId)
  {
    var users = await _context.Users.Where(u => u.CompanyId == companyId).ToListAsync();
    
    return users;
  }

  public async Task<List<BTUser>> GetUsersByCompanyIdAsync(int companyId)
  {
    return await _context.Users.Where(u => u.CompanyId == companyId).ToListAsync();
  }
}