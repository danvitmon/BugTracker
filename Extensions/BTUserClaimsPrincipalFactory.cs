using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using BugTracker.Models;

namespace BugTracker.Extensions;

public class BTUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<BTUser, IdentityRole>
{
  public BTUserClaimsPrincipalFactory(UserManager<BTUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<IdentityOptions> options) : base(userManager, roleManager, options) { }

  protected override async Task<ClaimsIdentity> GenerateClaimsAsync(BTUser user)
  {
    var identity     = await base.GenerateClaimsAsync(user);
    var companyClaim = new Claim("CompanyId", user.CompanyId.ToString());

    identity.AddClaim(companyClaim);

    return identity;
  }
}