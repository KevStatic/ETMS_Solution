<<<<<<< HEAD
//using ETMS.Application.Services;
=======
>>>>>>> f0ee524405ff6582eb67b8a17f4e922eeb967f9e
using ETMS.Application.Interfaces;
using ETMS.Infrastructure.Context;
using ETMS.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<DapperContext>();

<<<<<<< HEAD
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserAccountRepository, UserAccountRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ITransferRequestRepository, TransferRequestRepository>();

//// Infrastructure Layer
//builder.Services.AddScoped<IUserAccountRepository, UserAccountRepository>();

//// Application Layer
//builder.Services.AddScoped<IAuthService, AuthService>();
=======
builder.Services.AddScoped<ITransferRequestRepository, TransferRequestRepository>();

builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
>>>>>>> f0ee524405ff6582eb67b8a17f4e922eeb967f9e

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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
