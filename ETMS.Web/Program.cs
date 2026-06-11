using ETMS.Application.Interfaces;
using ETMS.Application.Services;
using ETMS.Domain.Interfaces;
using ETMS.Infrastructure.Context;
using ETMS.Infrastructure.Repositories;
using ETMS.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
DotNetEnv.Env.Load();

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddControllersWithViews();
    builder.Services.AddSingleton<DapperContext>();
    builder.Services.AddMemoryCache();

    // ── Repository Layer ───────────────────────────────────────────────────
    builder.Services.AddScoped<IApprovalDashboardRepository, ApprovalDashboardRepository>();
    builder.Services.AddScoped<IApprovalService, ApprovalService>();
    builder.Services.AddScoped<IUrlEncryptionService, UrlEncryptionService>();

    // ── Infrastructure Layer ──────────────────────────────────────────────────
    builder.Services.AddScoped<IUserAccountRepository, UserAccountRepository>();
    builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
    builder.Services.AddScoped<ILocationRepository, LocationRepository>();
    builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
    builder.Services.AddScoped<ITransferRequestRepository, TransferRequestRepository>();
    builder.Services.AddScoped<IOtpRepository, OtpRepository>();
    // Repositories
    builder.Services.AddScoped<IUserSettingsRepository, UserSettingsRepository>();

    // Services
    builder.Services.AddScoped<IProfileService, ProfileService>();
    builder.Services.AddScoped<IUserSettingsService, UserSettingsService>();
    builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

    // ── Email Service ─────────────────────────────────────────────────────────
    // Secrets come from ETMS.Web/.env (Smtp__Username etc.) loaded by DotNetEnv above;
    // non-secret defaults (Host/Port) live in appsettings.json.
    builder.Services.AddScoped<IEmailService>(_ =>
    {
        var smtp = builder.Configuration.GetSection("Smtp");

        string Require(string key)
        {
            var value = smtp[key];
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException(
                    $"Smtp:{key} is not configured. Set Smtp__{key} in ETMS.Web/.env (see .env.example).");
            return value;
        }

        return new EmailService(
            host: Require("Host"),
            port: int.TryParse(smtp["Port"], out var port) ? port : 587,
            username: Require("Username"),
            password: Require("Password"),
            fromEmail: Require("FromEmail")
        );
    });

    // ── Application Layer ─────────────────────────────────────────────────────
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IForgotPasswordService, ForgotPasswordService>();
    builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();

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

    // Ensure letters directory exists on startup
    var lettersPath = Path.Combine(app.Environment.WebRootPath, "letters");
    Directory.CreateDirectory(lettersPath); // does nothing if already exists

    app.Run();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("=== STARTUP CRASH ===");
    Console.WriteLine(ex.Message);
    Console.WriteLine(ex.InnerException?.Message);
    Console.ResetColor();

    // Keep the window open only when running interactively; ReadKey throws
    // when console input is redirected (e.g. hosted under IIS / a service).
    if (!Console.IsInputRedirected)
        Console.ReadKey();
}