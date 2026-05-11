// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using IntegratieProject.DAL.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace IntegratieProject.UI.MVC.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
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
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

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
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            var requestedSubplatform = GetSubplatformFromReturnUrl(returnUrl);

            if (await _userManager.IsInRoleAsync(user, CustomIdentityConstants.SubAdminRoleName))
            {
                if (!string.IsNullOrWhiteSpace(requestedSubplatform) &&
                    !string.Equals(user.SubPlatformSlug, requestedSubplatform, StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(string.Empty,
                        $"Je account hoort bij '{user.SubPlatformSlug}'. Log daarom in via het {user.SubPlatformSlug}, niet via '{requestedSubplatform}'.");
                    return Page();
                }
            }

            var result = await _signInManager.PasswordSignInAsync(
                Input.Email,
                Input.Password,
                Input.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");

                if (await _userManager.IsInRoleAsync(user, CustomIdentityConstants.SubAdminRoleName))
                {
                    if (!string.IsNullOrWhiteSpace(user.SubPlatformSlug))
                    {
                        return Redirect($"/{user.SubPlatformSlug}/SubAdmin");
                    }

                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    await _signInManager.SignOutAsync();
                    return Page();
                }

                if (await _userManager.IsInRoleAsync(user, CustomIdentityConstants.GeneralAdminRoleName))
                {
                    return Redirect("/admin");
                }

                return LocalRedirect(returnUrl);
            }

            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return RedirectToPage("./Lockout");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

        private string GetSubplatformFromReturnUrl(string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return null;
            }

            var cleanUrl = returnUrl.Split('?', '#')[0].Trim('/');

            if (string.IsNullOrWhiteSpace(cleanUrl))
            {
                return null;
            }

            var segments = cleanUrl.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length == 0)
            {
                return null;
            }

            var firstSegment = segments[0];

            if (string.Equals(firstSegment, "Identity", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(firstSegment, "Platform", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return firstSegment;
        }
    }
}