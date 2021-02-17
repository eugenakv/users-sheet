using Microsoft.AspNetCore.Identity;

namespace UsersSheet.Utility
{
    public static class Extensions
    {
        public static string GetControllerName(this string self)
        {
            return self.Replace("Controller", string.Empty);
        }

        public static void UseNoPasswordRequirments(this IdentityOptions self)
        {
            self.Password.RequireDigit = false;
            self.Password.RequireLowercase = false;
            self.Password.RequireNonAlphanumeric = false;
            self.Password.RequireUppercase = false;
            self.Password.RequiredLength = 1;
        }
    }
}
