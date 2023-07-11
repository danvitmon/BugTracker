#nullable disable

using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using BugTracker.Models;

namespace BugTracker.Areas.Identity.Pages.Account;

public class LoginModel : PageModel
{
  private readonly IConfiguration        _configuration;
  private readonly ILogger<LoginModel>   _logger;
  private readonly SignInManager<BTUser> _signInManager;

  public LoginModel(SignInManager<BTUser> signInManager, ILogger<LoginModel> logger, IConfiguration configuration)
  {
    _signInManager = signInManager;
    _logger        = logger;
    _configuration = configuration;
  }

  /// <summary>
  ///   This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
  ///   directly from your code. This API may change or be removed in future releases.
  /// </summary>
  [BindProperty]
  public InputModel Input { get; set; }

  /// <summary>
  ///   This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
  ///   directly from your code. This API may change or be removed in future releases.
  /// </summary>
  public IList<AuthenticationScheme> ExternalLogins { get; set; }

  /// <summary>
  ///   This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
  ///   directly from your code. This API may change or be removed in future releases.
  /// </summary>
  public string ReturnUrl { get; set; }

  /// <summary>
  ///   This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
  ///   directly from your code. This API may change or be removed in future releases.
  /// </summary>
  [TempData]
  public string ErrorMessage { get; set; }

  public async Task OnGetAsync(string returnUrl = null)
  {
    if (!string.IsNullOrEmpty(ErrorMessage)) ModelState.AddModelError(string.Empty, ErrorMessage);

    returnUrl ??= Url.Content("~/");

    // Clear the existing external cookie to ensure a clean login process
    await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

    ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    ReturnUrl      = returnUrl;
  }

  public async Task<IActionResult> OnPostAsync(string returnUrl = null, string demoEmail = null)
  {
    returnUrl ??= Url.Content("~/Home/Dashboard");

    if (!string.IsNullOrEmpty(demoEmail))
    {
      var email    = _configuration[demoEmail]          ?? Environment.GetEnvironmentVariable(demoEmail);
      var password = _configuration["DemoUserPassword"] ?? Environment.GetEnvironmentVariable("DemoUserPassword");
      var result   = await _signInManager.PasswordSignInAsync(email!, password!, false, false);

      if (result.Succeeded)
      {
        _logger.LogInformation("Demo User logged in.");
        return LocalRedirect(returnUrl);
      }

      ModelState.AddModelError(string.Empty, "Invalid demo login attempt.");
      return Page();
    }

    if (ModelState.IsValid)
    {
      // This doesn't count login failures towards account lockout
      // To enable password failures to trigger account lockout, set lockoutOnFailure: true
      var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, false);
      if (result.Succeeded)
      {
        _logger.LogInformation("User logged in.");
        return LocalRedirect(returnUrl);
      }

      if (result.RequiresTwoFactor)

        return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, Input.RememberMe });
      if (result.IsLockedOut)
      {
        _logger.LogWarning("User account locked out.");
        return RedirectToPage("./Lockout");
      }

      ModelState.AddModelError(string.Empty, "Invalid login attempt.");
      return Page();
    }

    // If we got this far, something failed, redisplay form
    return Page();
  }

  /// <summary>
  ///   This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
  ///   directly from your code. This API may change or be removed in future releases.
  /// </summary>
  public class InputModel
  {
    /// <summary>
    ///   This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///   directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    /// <summary>
    ///   This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///   directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    /// <summary>
    ///   This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///   directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }
  }
}