﻿#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Models.Enums;

namespace BugTracker.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
  private readonly ApplicationDbContext    _context;
  private readonly IEmailSender            _emailSender;
  private readonly IUserEmailStore<BTUser> _emailStore;
  private readonly ILogger<RegisterModel>  _logger;
  private readonly SignInManager<BTUser>   _signInManager;
  private readonly UserManager<BTUser>     _userManager;
  private readonly IUserStore<BTUser>      _userStore;

  public RegisterModel(
    UserManager<BTUser>    userManager,
    IUserStore<BTUser>     userStore,
    SignInManager<BTUser>  signInManager,
    ILogger<RegisterModel> logger,
    IEmailSender           emailSender,
    ApplicationDbContext   context)
  {
    _userManager   = userManager;
    _userStore     = userStore;
    _emailStore    = GetEmailStore();
    _signInManager = signInManager;
    _logger        = logger;
    _emailSender   = emailSender;
    _context       = context;
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
  public string ReturnUrl { get; set; }

  /// <summary>
  ///   This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
  ///   directly from your code. This API may change or be removed in future releases.
  /// </summary>
  public IList<AuthenticationScheme> ExternalLogins { get; set; }


  public async Task OnGetAsync(string returnUrl = null)
  {
    ReturnUrl      = returnUrl;
    ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
  }

  public async Task<IActionResult> OnPostAsync(string returnUrl = null)
  {
    returnUrl    ??= Url.Content("~/");
    ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    if (ModelState.IsValid)
    {
      //Create new company
      Company company = new()
      {
        Name = Input.CompanyName,
        Description = Input.CompanyDescription
      };
      await _context.AddAsync(company);
      await _context.SaveChangesAsync();

      //Create new user
      var user = CreateUser(company.Id);

      await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
      await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
      var result = await _userManager.CreateAsync(user, Input.Password);

      if (result.Succeeded)
      {
        _logger.LogInformation("User created a new account with password.");

        await _userManager.AddToRoleAsync(user, nameof(BTRoles.Admin));

        var userId      = await _userManager.GetUserIdAsync(user);
        var code        = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        code            = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = Url.Page( "/Account/ConfirmEmail", null, new { area = "Identity", userId, code, returnUrl }, Request.Scheme);

        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email", $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

        if (_userManager.Options.SignIn.RequireConfirmedAccount)
        {
          return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
        }

        await _signInManager.SignInAsync(user, false);
        
        return LocalRedirect(returnUrl);
      }

      foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
    }

    // If we got this far, something failed, redisplay form
    return Page();
  }

  private BTUser CreateUser(int companyId)
  {
    try
    {
      var btUser       = Activator.CreateInstance<BTUser>();
      btUser.FirstName = Input.FirstName;
      btUser.LastName  = Input.LastName;
      btUser.CompanyId = companyId;

      return btUser;
    }
    catch
    {
      throw new InvalidOperationException($"Can't create an instance of '{nameof(BTUser)}'. " +
                                          $"Ensure that '{nameof(BTUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                                          "override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
    }
  }

  private IUserEmailStore<BTUser> GetEmailStore()
  {
    if (!_userManager.SupportsUserEmail)
      throw new NotSupportedException("The default UI requires a user store with email support.");
    
      return (IUserEmailStore<BTUser>)_userStore;
  }

  /// <summary>
  ///   This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
  ///   directly from your code. This API may change or be removed in future releases.
  /// </summary>
  public class InputModel
  {
    [Required]
    [Display(Name = "First Name")]
    public string FirstName { get; set; }

    /// <summary>
    ///   This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///   directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [Required]
    [Display(Name = "Last Name")]
    public string LastName { get; set; }

    /// <summary>
    ///   This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///   directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [Required]
    [Display(Name = "Company Name")]
    public string CompanyName { get; set; }

    /// <summary>
    ///   This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///   directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [Display(Name = "Company Description")]
    public string CompanyDescription { get; set; }

    /// <summary>
    ///   This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///   directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; }

    /// <summary>
    ///   This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///   directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
      MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }

    /// <summary>
    ///   This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///   directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }
  }
}