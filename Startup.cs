using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UsersSheet.Data;
using UsersSheet.Entities;
using UsersSheet.Utility;

namespace UsersSheet
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(config =>
            {
                config.UseSqlServer(Configuration.GetConnectionString("ApplicationDbContext"));
            });
            services.AddIdentity<User, Role>(options => options.UseNoPasswordRequirments())
                    .AddEntityFrameworkStores<ApplicationDbContext>();

            services.ConfigureApplicationCookie(config =>
            {
                config.LoginPath = "/Account/SignIn";
                config.AccessDeniedPath = "/Account/AccessDenied";
                config.ReturnUrlParameter = "returnUrl";
            });

            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
