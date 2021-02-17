using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using UsersSheet.Entities;
using UsersSheet.Models;
using UsersSheet.Utility;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace UsersSheet.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly string ActiveRole = "Active";

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(SignInInfoViewModel model)
        {
            if (!(ModelState.IsValid && CheckUserAccountExists(model, out User user) && await IsActiveAsync(user)))
            {
                return View(model);
            }

            SignInResult result = await this.signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Wrong password");
                return View(model);
            }

            return RedirectToAction(nameof(HomeController.Index), nameof(HomeController).GetControllerName());
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegistrationInfoViewModel model)
        {
            if (!(ModelState.IsValid && CheckNameIsUnique(model) && CheckEmailIsUnique(model)))
            {
                return View(model);
            }

            User user = CreateUser(model);
            IdentityResult result = await this.userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Something went wrong. Failed to create user");
                return View(model);
            }
            await ActivateAsync(user);
            await this.signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: false);

            return RedirectToAction(nameof(HomeController.Index), nameof(HomeController).GetControllerName());
        }

        #region NonActions

        [NonAction]
        private User CreateUser(RegistrationInfoViewModel model) => new User
        {
            UserName = model.UserName,
            Email = model.Email,
            RegistrationDate = DateTime.UtcNow,
            LastLoginDate = DateTime.UtcNow
        };

        [NonAction]
        private async Task<bool> IsActiveAsync(User user)
        {
            bool isActive = await this.userManager.IsInRoleAsync(user, ActiveRole);
            if (!isActive)
            {
                ModelState.AddModelError(string.Empty, "You are blocked and cannot access user account");
            }

            return isActive;
        }

        [NonAction]
        private async Task ActivateAsync(User user)
        {
            await this.userManager.AddToRoleAsync(user, this.ActiveRole);
        }

        [NonAction]
        private bool CheckUserAccountExists(SignInInfoViewModel model, out User user)
            => MakeQuery(out user,
                         selector: this.userManager.FindByEmailAsync,
                         argument: model.Email,
                         errorCondition: user => user is null,
                         errorMessage: "Couldn't find user with this email");


        [NonAction]
        private bool CheckNameIsUnique(RegistrationInfoViewModel model) 
            => MakeQuery(out User _,
                         selector: this.userManager.FindByNameAsync,
                         argument: model.UserName,
                         errorCondition: user => user is not null,
                         errorMessage: "User with same name already exists");

        [NonAction]
        private bool CheckEmailIsUnique(RegistrationInfoViewModel model) 
            => MakeQuery(out User _,
                         selector: this.userManager.FindByEmailAsync,
                         argument: model.Email,
                         errorCondition: user => user is not null,
                         errorMessage: "User with this email already exists");

        [NonAction]
        private bool MakeQuery(out User user,
                                Func<string, Task<User>> selector,
                                string argument,
                                Predicate<User> errorCondition,
                                string errorMessage)
        {
            user = selector.Invoke(argument).Result;
            if (errorCondition.Invoke(user))
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                return false;
            }
            return true;
        }

        #endregion

        public async Task<IActionResult> AccessDenied()
        {
            await this.signInManager.SignOutAsync();
            return RedirectToAction(nameof(SignIn), nameof(AccountController).GetControllerName());
        }
    }
}
