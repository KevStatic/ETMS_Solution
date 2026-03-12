using ETMS.Application.Interfaces;
using ETMS.Application.Services;
using ETMS.Domain.Interfaces;
using ETMS.Infrastructure.Context;
using ETMS.Infrastructure.Repositories;
using ETMS.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddControllersWithViews();
    builder.Services.AddSingleton<DapperContext>();
    builder.Services.AddMemoryCache();

    // ── Repository Layer ───────────────────────────────────────────────────
    builder.Services.AddScoped<IApprovalDashboardRepository, ApprovalDashboardRepository>();
    builder.Services.AddScoped<IApprovalService, ApprovalService>();

    // ── Infrastructure Layer ──────────────────────────────────────────────────
    builder.Services.AddScoped<IUserAccountRepository, UserAccountRepository>();
    builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
    builder.Services.AddScoped<ILocationRepository, LocationRepository>();
    builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
    builder.Services.AddScoped<ITransferRequestRepository, TransferRequestRepository>();
    builder.Services.AddScoped<IOtpRepository, OtpRepository>();

    // ── Email Service ─────────────────────────────────────────────────────────
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

    // ── Application Layer ─────────────────────────────────────────────────────
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IForgotPasswordService, ForgotPasswordService>();

    // ── Antiforgery ───────────────────────────────────────────────────────────
    builder.Services.AddAntiforgery(options =>
    {
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

    // ── Authentication ────────────────────────────────────────────────────────
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Account/Login";   // ✅ Fixed login path
            options.AccessDeniedPath = "/Account/Login";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
        });

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();        // ✅ Required for CSS/JS
    app.UseRouting();
    app.UseAuthentication();     // ✅ Must be before UseAuthorization
    app.UseAuthorization();

    // ── Routes ────────────────────────────────────────────────────────────────
    app.MapControllerRoute(
        name: "portal",
        pattern: "portal/{action=Index}/{id?}",
        defaults: new { controller = "Dashboard" });

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Account}/{action=Login}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("=== STARTUP CRASH ===");
    Console.WriteLine(ex.Message);
    Console.WriteLine(ex.InnerException?.Message);
    Console.ResetColor();
    Console.ReadKey();
}