// Licensed to the .NET Foundation under one or more agreements.

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IntegratieProject.DAL.Identity;
using IntegratieProject.BL.interfaces;
using IntegratieProject.UI.MVC.Services;

namespace IntegratieProject.UI.MVC.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ISubplatformManager _subplatformManager;
        private readonly IGoogleCloudStorageService _googleCloudStorageService;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ISubplatformManager subplatformManager,
            IGoogleCloudStorageService googleCloudStorageService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _subplatformManager = subplatformManager;
            _googleCloudStorageService = googleCloudStorageService;
        }

        public string Username { get; set; }

        [TempData] public string StatusMessage { get; set; }

        [BindProperty] public InputModel Input { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Subplatform Logo")] public IFormFile LogoFile { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            if (Input.LogoFile != null && Input.LogoFile.Length > 0)
            {
                // VERDACHTE 1: Is de slug wel ingevuld voor deze gebruiker?
                if (string.IsNullOrEmpty(user.SubPlatformSlug))
                {
                    ModelState.AddModelError(string.Empty,
                        "Fout: Je account is niet gekoppeld aan een subplatform (SubPlatformSlug is leeg in de database).");
                    await LoadAsync(user);
                    return Page();
                }

                try
                {
                    string logoUri = await _googleCloudStorageService.UploadProjectMediaAsync(
                        Input.LogoFile,
                        user.SubPlatformSlug
                    );

                    _subplatformManager.UpdateSubPlatformLogo(user.SubPlatformSlug, logoUri);

                    StatusMessage = "Je profiel en logo zijn succesvol bijgewerkt!";
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"GCS Fout: {ex.Message}");
                    await LoadAsync(user);
                    return Page();
                }
            }
            else
            {
                StatusMessage = "Je profiel is bijgewerkt (geen nieuw logo geselecteerd).";
            }

            await _signInManager.RefreshSignInAsync(user);
            return RedirectToPage();
        }
    }
}