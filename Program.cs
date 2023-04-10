using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RecomField.Data;
using RecomField.Hubs;
using RecomField.Models;

namespace RecomField
{
    public class Program
    {
        public static string Environment { get; set; } = "Development";
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.User.AllowedUserNameCharacters = null;
            }).AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
            builder.Services.AddControllersWithViews().AddViewLocalization();

            builder.Services.AddAuthentication().AddGoogle(options =>
            {
                options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ??
                throw new Exception("Authentication:Google:ClientId is not in configuration");
                options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ??
                throw new Exception("Authentication:Google:ClientId is not in configuration");
                options.AccessDeniedPath = "/Identity/Account/Login";})
            .AddFacebook(options =>
            {
                options.AppId = builder.Configuration["Authentication:Facebook:AppId"] ??
                throw new Exception("Authentication:Facebook:AppId is not in configuration");
                options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"] ??
                throw new Exception("Authentication:Facebook:AppSecret is not in configuration");
                options.AccessDeniedPath = "/Identity/Account/Login";
            });

            builder.Services.AddSignalR();

            var app = builder.Build();

            var supportedCultures = new[] { "en-US", "ru" };
            var localizationOptions = new RequestLocalizationOptions().SetDefaultCulture(supportedCultures[0])
                .AddSupportedCultures(supportedCultures).AddSupportedUICultures(supportedCultures);
            app.UseRequestLocalization(localizationOptions);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                Environment = app.Environment.EnvironmentName;
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();
            app.MapHub<MainHub>("/mainHub");

            app.Run();
        }
    }
}