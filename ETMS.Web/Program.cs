using ETMS.Application.Interfaces;
using ETMS.Application.Services;
using ETMS.Domain.Interfaces;
using ETMS.Infrastructure.Context;
using ETMS.Infrastructure.Repositories;
using ETMS.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<DapperContext>();

// ── Infrastructure Layer ──────────────────────────────────────────────────────
builder.Services.AddScoped<IUserAccountRepository, UserAccountRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ITransferRequestRepository, TransferRequestRepository>();
builder.Services.AddScoped<IOtpRepository, OtpRepository>();

// ── Email Service ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IEmailService>(_ =>
{
    var smtp = builder.Configuration.GetSection("Smtp");
    return new EmailService(
        host: smtp["Host"]!,
        port: int.Parse(smtp["Port"]!),
        username: smtp["Username"]!,
        password: smtp["Password"]!,
        fromEmail: smtp["FromEmail"]!
    );
});

// ── Application Layer ─────────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IForgotPasswordService, ForgotPasswordService>();
builder.Services.AddMemoryCache();

// ── Authentication ────────────────────────────────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login/accessdenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "login",
    pattern: "login",
    defaults: new { controller = "Account", action = "Login" })
    .WithStaticAssets();

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