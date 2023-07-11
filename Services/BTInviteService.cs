using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Services;

public class BTInviteService : IBTInviteService
{
  private readonly ApplicationDbContext _context;

  public BTInviteService(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task<bool> AcceptInviteAsync(Guid? token, string userId, int companyId)
  {
    try
    {
      var invite = await _context.Invites
        .FirstOrDefaultAsync(i => i.CompanyToken == token
                                  && i.CompanyId == companyId
                                  && i.IsValid == true);

      if (invite == null) return false;

      invite.IsValid = false;
      invite.InviteeId = userId;
      await _context.SaveChangesAsync();

      return true;
    }
    catch (Exception)
    {
      return false;
      throw;
    }
  }

  public async Task AddNewInviteAsync(Invite invite)
  {
    _context.Add(invite);
    await _context.SaveChangesAsync();
  }

  public async Task<bool> AnyInviteAsync(Guid token, string email, int companyId)
  {
    var result = await _context.Invites.Where(i => i.CompanyId == companyId)
      .AnyAsync(i => i.CompanyToken == token && i.InviteeEmail == email);

    return result;
  }

  public async Task CancelInviteAsync(int inviteId, int companyId)
  {
    var invite = await _context.Invites.FirstOrDefaultAsync(i => i.Id == inviteId && i.CompanyId == companyId);

    if (invite is not null)
    {
      invite.IsValid = false;

      await _context.SaveChangesAsync();
    }
  }

  public async Task<Invite?> GetInviteAsync(int inviteId, int companyId)
  {
    var invite = await _context.Invites
      .Include(i => i.Company)
      .Include(i => i.Invitor)
      .Include(i => i.Invitee)
      .Include(i => i.Project)
      .FirstOrDefaultAsync(i => i.Id == inviteId && i.CompanyId == companyId);

    return invite;
  }

  public async Task<Invite?> GetInviteAsync(Guid token, string email, int companyId)
  {
    var invite = await _context.Invites
      .Include(i => i.Company)
      .Include(i => i.Invitor)
      .Include(i => i.Invitee)
      .Include(i => i.Project)
      .FirstOrDefaultAsync(i => i.CompanyId == companyId && i.CompanyToken == token && i.InviteeEmail == email);

    return invite;
  }

  public async Task UpdateInviteAsync(Invite invite)
  {
    _context.Update(invite);
    await _context.SaveChangesAsync();
  }

  public async Task<bool> ValidateInviteCodeAsync(Guid? token)
  {
    if (token is null) return false;

    var result = false;

    var invite = await _context.Invites.FirstOrDefaultAsync(i => i.CompanyToken == token);

    if (invite != null)
    {
      var inviteDate = invite.InviteDate;

      var notExpired = (DateTime.UtcNow - inviteDate).TotalDays <= 7;

      if (notExpired) result = invite.IsValid;
    }

    return result;
  }
}