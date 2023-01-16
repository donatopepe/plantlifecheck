using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Areas.User;

namespace Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<CustomUser> _userManager;
        private readonly SignInManager<CustomUser> _signInManager;
        private readonly SMSSender _allarmiSMS;

        public IndexModel(
            UserManager<CustomUser> userManager,
            SignInManager<CustomUser> signInManager, SMSSender allarmiSMS)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _allarmiSMS = allarmiSMS;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            
            [Display(Name = "Phone number Confirmied")]
            public bool PhoneNumberConfirmed { get; set; }

            [Display(Name = "Phone number Code")]
            public int PhoneNumberCode { get; set; }
        }

        private async Task LoadAsync(CustomUser user)
        {
            
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var phoneNumberConfirmed = user.PhoneNumberConfirmed;

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                PhoneNumberConfirmed = phoneNumberConfirmed
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            //Console.WriteLine(">>>>>>>>>>>>>>>>"+user.Email+" "+user.Id);
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

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                user.CodePhone = new Random(DateTime.Now.Millisecond).Next(111111, 999999);
                user.PhoneNumberConfirmed = false;
                

                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {

                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
                else
                {
                    string v = _allarmiSMS.ComponiInviaSMS($"PlantLifeCheck: Codice di verifica numero di telefono {user.CodePhone}, da inserire in profilo account", Input.PhoneNumber);
                    
                }
            }

            if (Input.PhoneNumberCode == user.CodePhone)
            {
                user.PhoneNumberConfirmed = true;
                user.CodePhone = 0;
                var setPhoneResult = await _userManager.UpdateAsync(user);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
