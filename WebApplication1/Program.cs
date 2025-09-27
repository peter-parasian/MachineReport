using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authorization;
using BCrypt.Net;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString")));

builder.Services.AddScoped<ILogger<MaintenanceController>, Logger<MaintenanceController>>();

builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", config =>
    {
        config.LoginPath = "/Home/Login";
        config.LogoutPath = "/Home/Logout";
        config.AccessDeniedPath = "/Home/AccessDenied";
        config.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        config.SlidingExpiration = true;
        config.Cookie.HttpOnly = true;
        config.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        config.Cookie.SameSite = SameSiteMode.Lax;
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!context.Users.Any(u => u.Email == "admin@gmail.com"))
    {
        var adminUser = new User
        {
            Name = "Administrator",
            Email = "admin@gmail.com",
            PhoneNumber = "081234567890",
            Role = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
            IsVerified = true
        };

        context.Users.Add(adminUser);
        context.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    await next();
});

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();