using Microsoft.AspNetCore.Authorization;
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
        public const string ActiveRole = "Active";

        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public IActionResult SignIn() => View();

        [HttpPost]
        public async Task<IActionResult> SignIn(SignInInfoViewModel model, string returnUrl)
        {
            if (ModelState.IsValid && CheckUserAccountExists(model, out User user) && IsActive(user) && await TrySignInAsync(user, model.Password))
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction(nameof(HomeController.Index), nameof(HomeController).GetControllerName());
            }
            
            return View(model);
        }

        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegistrationInfoViewModel model)
        {
            if (ModelState.IsValid && CheckNameIsUnique(model) && CheckEmailIsUnique(model) && CheckPasswordMatch(model) && await TryRegisterAsync(model))
            {
                return RedirectToAction(nameof(HomeController.Index), nameof(HomeController).GetControllerName());
            }

            return View(model);
        }

        public async new Task<IActionResult> SignOut()
        {
            await this.signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), nameof(HomeController).GetControllerName());
        }

        #region NonActions

        [NonAction]
        private bool MakeQuery(out User user, Func<string, Task<User>> selector, string argument, Predicate<User> errorCondition, string errorMessage)
        {
            user = selector.Invoke(argument).Result;
            if (errorCondition.Invoke(user))
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                return false;
            }
            return true;
        }

        [NonAction]
        private bool CheckUserAccountExists(SignInInfoViewModel model, out User user)
            => MakeQuery(out user,
                         selector: this.userManager.FindByEmailAsync,
                         argument: model.Email,
                         errorCondition: user => user is null,
                         errorMessage: "Couldn't find user with this email");

        [NonAction]
        private bool IsActive(User user)
        {
            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "You are blocked and cannot access user account");
                return false;
            }
            return true;
        }

        [NonAction]
        private async Task<bool> TrySignInAsync(User user, string password)
        {
            SignInResult result = await this.signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Wrong password");
                return false;
            }
            user.LastLoginDate = DateTime.Now;
            await this.userManager.UpdateAsync(user);
            return true;
        }

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
        private bool CheckPasswordMatch(RegistrationInfoViewModel model)
        {
            if (model.Password != model.PasswordConfirmation)
            {
                ModelState.AddModelError(string.Empty, "Password did not match");
                return false;
            }

            return true;
        }

        [NonAction]
        private async Task<bool> TryRegisterAsync(RegistrationInfoViewModel model)
        {
            User user = CreateUser(model);
            IdentityResult result = await this.userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Something went wrong. Failed to create user");
                return false;
            }
            await ActivateAsync(user);
            await this.signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: false);
            return true;
        }

        [NonAction]
        private User CreateUser(RegistrationInfoViewModel model) => new User
        {
            UserName = model.UserName,
            Email = model.Email,
            RegistrationDate = DateTime.Now,
            LastLoginDate = DateTime.Now,
            IsActive = true
        };

        [NonAction]
        private async Task ActivateAsync(User user)
        {
            await this.userManager.AddToRoleAsync(user, ActiveRole);
        }

        #endregion

        public async Task<IActionResult> AccessDenied()
        {
            await this.signInManager.SignOutAsync();
            return RedirectToAction(nameof(SignIn), nameof(AccountController).GetControllerName());
        }
    }
}
