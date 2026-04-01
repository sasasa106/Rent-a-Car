using Data;
using Microsoft.EntityFrameworkCore;
using Data.Repositories;
using Core.Interfaces;
using Core.Services;
using Core.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Configure EF Core with the connection string from configuration (falls back to env var DEFAULT_CONNECTION)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? Environment.GetEnvironmentVariable("DEFAULT_CONNECTION");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("A database connection string must be provided via configuration (ConnectionStrings:DefaultConnection) or DEFAULT_CONNECTION environment variable.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

builder.Services.AddControllersWithViews();

// Authentication: cookie-based with default login path
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.Cookie.Name = "RentACar.Auth";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

// Register repositories and services from Core.Configuration
builder.Services.RegisterServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
