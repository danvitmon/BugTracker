#nullable disable

using System.Text.Json;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using BugTracker.Models;

namespace BugTracker.Areas.Identity.Pages.Account.Manage;

public class DownloadPersonalDataModel : PageModel
{
  private readonly ILogger<DownloadPersonalDataModel> _logger;
  private readonly UserManager<BTUser>                _userManager;

  public DownloadPersonalDataModel(UserManager<BTUser> userManager, ILogger<DownloadPersonalDataModel> logger)
  {
    _userManager = userManager;
    _logger      = logger;
  }

  public IActionResult OnGet() => NotFound();

  public async Task<IActionResult> OnPostAsync()
  {
    var user = await _userManager.GetUserAsync(User);
    if (user == null) 
      return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

    _logger.LogInformation("User with ID '{UserId}' asked for their personal data.", _userManager.GetUserId(User));

    // Only include personal data for download
    var personalData           = new Dictionary<string, string>();
    var personalDataProperties = typeof(BTUser).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
    
    foreach (var property in personalDataProperties) 
      personalData.Add(property.Name, property.GetValue(user)?.ToString() ?? "null");

    var logins = await _userManager.GetLoginsAsync(user);
    foreach (var login in logins)
      personalData.Add($"{login.LoginProvider} external login provider key", login.ProviderKey);

    personalData.Add("Authenticator Key", await _userManager.GetAuthenticatorKeyAsync(user));

    Response.Headers.Add("Content-Disposition", "attachment; filename=PersonalData.json");
    
    return new FileContentResult(JsonSerializer.SerializeToUtf8Bytes(personalData), "application/json");
  }
}