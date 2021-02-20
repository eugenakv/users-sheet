using Microsoft.AspNetCore.Identity;
using System;
using UsersSheet.Entities;
using Microsoft.Extensions.DependencyInjection;
using UsersSheet.Controllers;

namespace UsersSheet.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            RoleManager<Role> roleManager = serviceProvider.GetService<RoleManager<Role>>();
            roleManager.CreateAsync(new Role { Name = AccountController.ActiveRole }).GetAwaiter().GetResult();
        }
    }
}
