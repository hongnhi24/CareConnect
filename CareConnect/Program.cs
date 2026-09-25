using CareConnect.Data;
using CareConnect.Hubs;
using CareConnect.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddScoped<
    ICaregiverMatchingService,
    CaregiverMatchingService>();

builder.Services.AddScoped<
    IPasswordSecurityService,
    PasswordSecurityService>();

builder.Services.AddHostedService<
    LegacyPasswordMigrationService>();
builder.Services.AddHostedService<
   MedicationReminderService>();
builder.Services.AddHostedService<
   AppointmentReminderService>();

builder.Services.AddDbContext<CareConnectDbContext>(options =>
{
    string connectionString =
        builder.Configuration.GetConnectionString(
            "CareConnectConnection")
        ?? throw new InvalidOperationException(
            "Không tìm thấy chuỗi kết nối CareConnectConnection.");

    options.UseSqlServer(connectionString);
});

builder.Services.AddScoped<
    ICaregiverProfileCompletenessService,
    CaregiverProfileCompletenessService>();

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";

        options.Cookie.Name = "CareConnect.Authentication";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy =
            CookieSecurePolicy.SameAsRequest;

        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<NotificationHub>(
    "/hubs/notifications");
app.Run();
