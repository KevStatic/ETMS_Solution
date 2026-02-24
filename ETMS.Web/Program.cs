using ETMS.Application.Interfaces;
using ETMS.Application.Services;
using ETMS.Infrastructure.Context;
using ETMS.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<DapperContext>();

// Infrastructure Layer
builder.Services.AddScoped<IUserAccountRepository, UserAccountRepository>();

// Application Layer
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<ITransferRequestRepository, TransferRequestRepository>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // This tells the app exactly where to redirect unauthorized users!
        // Since Sattvik used [Route("login")], we point it here:
        options.LoginPath = "/login";

        // Optional but good practice
        options.AccessDeniedPath = "/login/accessdenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Log them out after 8 hours
    });

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

app.UseRouting();

// ✅ ADD THIS LINE (Must be exactly here, before Authorization)
app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

// Explicit route for /login
app.MapControllerRoute(
    name: "login",
    pattern: "login",
    defaults: new { controller = "Account", action = "Login" })
    .WithStaticAssets();

// Default landing route -> /login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();
