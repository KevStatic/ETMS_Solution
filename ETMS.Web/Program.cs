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
builder.Services.AddMemoryCache();

// ── Infrastructure Layer ──────────────────────────────────────────────────────
builder.Services.AddScoped<IUserAccountRepository, UserAccountRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
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

// ── Authentication (only once) ────────────────────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login/accessdenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();           // ✅ only once
app.UseAuthentication();   // ✅ must be before UseAuthorization
app.UseAuthorization();

app.MapStaticAssets();

// ── Routes ────────────────────────────────────────────────────────────────────
app.MapControllerRoute(
    name: "login",
    pattern: "login",
    defaults: new { controller = "Account", action = "Login" })
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();