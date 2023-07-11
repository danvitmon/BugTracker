﻿using System.Security.Claims;
using System.Security.Principal;

namespace BugTracker.Extensions;

public static class IdentityExtensions
{
  public static int GetCompanyId(this IIdentity identity)
  {
    var claim = ((ClaimsIdentity)identity).FindFirst("CompanyId")!;

    return int.Parse(claim.Value);
  }
}