using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public static IEnumerable<T> Mask<T>(this IEnumerable<T> self, IEnumerable<bool> mask)
        {
            return self.Zip(mask).Where(zip => zip.Second).Select(zip => zip.First);
        }

        public static T Copy<T>(this T self) where T : class, ICloneable
        {
            return self.Clone() as T;
        }
    }
}
