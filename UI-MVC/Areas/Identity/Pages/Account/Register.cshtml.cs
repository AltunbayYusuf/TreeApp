// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using IntegratieProject.BL.Domain.project;
using IntegratieProject.BL.Domain.users;
using IntegratieProject.DAL.Ef;
using IntegratieProject.DAL.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace IntegratieProject.UI.MVC.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly TreeDbContext _context;
        public List<SelectListItem> SubPlatforms { get; set; }

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            TreeDbContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            [Required(ErrorMessage = "De naam is verplicht.")]
            public string Name { get; set; }

            [Required(ErrorMessage = "Het e-mailadres is verplicht.")]
            [EmailAddress(ErrorMessage = "Vul een geldig e-mailadres in.")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Het wachtwoord is verplicht.")]
            [StringLength(100, ErrorMessage = "Het wachtwoord moet minstens {2} karakters lang zijn.",
                MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Required(ErrorMessage = "Je moet het wachtwoord bevestigen.")]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "De wachtwoorden komen niet overeen.")]
            public string ConfirmPassword { get; set; }

            [Required(ErrorMessage = "Selecteer een subplatform.")]
            public int? SubPlatformId { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;

            ExternalLogins = (await _signInManager
                .GetExternalAuthenticationSchemesAsync()).ToList();

            SubPlatforms = _context.SubPlatforms
                .Select(sp => new SelectListItem
                {
                    Value = sp.Id.ToString(),
                    Text = sp.CompanyName
                }).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                // 1. Haal het subplatform op VOORDAT we de user aanmaken in Identity
                var subPlatform = _context.SubPlatforms
                    .FirstOrDefault(sp => sp.Id == Input.SubPlatformId);

                if (subPlatform != null)
                {
                    // Belangrijk voor LoginModel: Koppel de slug aan de ApplicationUser!
                    user.SubPlatformSlug = subPlatform.Slug;
                }

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, CustomIdentityConstants.SubAdminRoleName);

                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await _userManager.ConfirmEmailAsync(user, token);

                    var generalAdmin = _context.GeneralAdmins.FirstOrDefault();
                    var subPlatformd = _context.SubPlatforms.FirstOrDefault(sp => sp.Id == Input.SubPlatformId);

                    var subAdmin = new SubAdmin
                    {
                        Name = Input.Name,
                        IdentityUserId = user.Id,
                        GeneralAdmin = generalAdmin,
                        SubPlatform = subPlatformd,
                        Projects = new List<Project>()
                    };

                    _context.SubAdmins.Add(subAdmin);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("User created a new account with password.");

                    StatusMessage = $"Account voor {Input.Name} is succesvol aangemaakt en direct geactiveerd!";
                    return RedirectToPage();
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // 2. HERLAAD de SubPlatforms lijst als we hier terecht komen (als formulier validatie faalt)
            SubPlatforms = _context.SubPlatforms
                .Select(sp => new SelectListItem
                {
                    Value = sp.Id.ToString(),
                    Text = sp.CompanyName
                }).ToList();

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                                                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                                                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }

            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}