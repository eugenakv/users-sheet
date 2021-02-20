using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSheet.Entities;
using UsersSheet.Models;
using UsersSheet.Utility;

namespace UsersSheet.Controllers
{
    [Authorize(Roles = AccountController.ActiveRole)]
    public class SheetController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;

        public SheetController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public IActionResult Users()
        {
            SelectedUser[] users = this.userManager.Users.Select(user => new SelectedUser { Selected = false, User = user }).ToArray();

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> Block(IEnumerable<SelectedUser> model)
        {
            IEnumerable<User> users = GetSelectedUsers(model);
            await SelfSignOutAsync(users);
            foreach (var user in users)
            {
                await this.userManager.RemoveFromRoleAsync(user, AccountController.ActiveRole);
                user.IsActive = false;
                await this.userManager.UpdateAsync(user);
            }

            return RedirectToAction(nameof(SheetController.Users), nameof(SheetController).GetControllerName());
        }

        [HttpPost]
        public async Task<IActionResult> Unblock(IEnumerable<SelectedUser> model)
        {
            IEnumerable<User> users = GetSelectedUsers(model);
            foreach (var user in users)
            {
                await this.userManager.AddToRoleAsync(user, AccountController.ActiveRole);
                user.IsActive = true;
                await this.userManager.UpdateAsync(user);
            }

            return RedirectToAction(nameof(SheetController.Users), nameof(SheetController).GetControllerName());
        }

        public async Task<IActionResult> Delete(IEnumerable<SelectedUser> model)
        {
            IEnumerable<User> users = GetSelectedUsers(model);
            await SelfSignOutAsync(users);
            foreach (var user in users)
            {
                await this.userManager.DeleteAsync(user);
            }

            return RedirectToAction(nameof(SheetController.Users), nameof(SheetController).GetControllerName());
        }

        #region NonActions

        [NonAction]
        private async Task SelfSignOutAsync(IEnumerable<User> users)
        {
            bool shouldSelfSignOut = users.Select(user => user.UserName).Contains(TempData["currentUser"] as string);
            if (shouldSelfSignOut)
            {
                await this.signInManager.SignOutAsync();
            }
        }

        [NonAction]
        private User[] GetSelectedUsers(IEnumerable<SelectedUser> model)
        {
            IEnumerable<string> userNames = (TempData["users"] as string[]).Mask(model.Select(user => user.Selected));
            return this.userManager.Users.Where(user => userNames.Contains(user.UserName)).ToArray();
        }

        #endregion
    }
}
