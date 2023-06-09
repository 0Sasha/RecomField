using CloudinaryDotNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecomField.Data;
using RecomField.Hubs;
using RecomField.Models;
using RecomField.Services;
namespace RecomField;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.User.AllowedUserNameCharacters = null;
        })
            .AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
        builder.Services
            .Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero);
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

        var cloudinary = new Cloudinary(new Account("dvsc8smkg", builder.Configuration["Cloudinary:ApiKey"] ??
            throw new Exception("Cloudinary:ApiKey is not found"), builder.Configuration["Cloudinary:APISecret"] ??
            throw new Exception("Cloudinary:APISecret is not found")));
        builder.Services.AddSingleton(typeof(Cloudinary), cloudinary);

        builder.Services.AddScoped<IUserService<ApplicationUser, IResponseCookies>, UserService>();
        builder.Services.AddScoped<IProductService<Product>, ProductService>();
        builder.Services.AddScoped<IReviewService<Review>, ReviewService>();
        builder.Services.AddScoped<ICloudService<IFormFile>, CloudService>();

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
        app.MapWhen(context => context.Request.Path.HasValue && context.Request.Path.Value.StartsWith("/Identity") &&
        !context.Request.Path.Value.EndsWith("Login") && !context.Request.Path.Value.EndsWith("Logout") &&
        !context.Request.Path.Value.EndsWith("ExternalLogin") && !context.Request.Path.Value.EndsWith("Lockout"),
        appBuilder => appBuilder.Run(async context =>
        {
            context.Response.StatusCode = 404;
            await context.Response.CompleteAsync();
        }));
        app.Run();
    }
}
