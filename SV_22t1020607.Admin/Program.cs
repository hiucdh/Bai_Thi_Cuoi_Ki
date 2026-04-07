using SV22T1020607.Admin;
using SV22T1020607.BusinessLayers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using System.Globalization;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews()
                .AddMvcOptions(option =>
                {
                    option.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                    
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                    option.Filters.Add(new AuthorizeFilter(policy));
                });

// Configure Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(option =>
                {
                    option.Cookie.Name = "SV22T1020607.Admin"; 
                    option.LoginPath = "/Account/Login";
                    option.AccessDeniedPath = "/Account/AccessDenied";
                    option.ExpireTimeSpan = TimeSpan.FromDays(7); 
                    option.SlidingExpiration = true;
                    option.Cookie.HttpOnly = true;
                    option.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                });

// Configure Session
builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromHours(2); 
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
});

var app = builder.Build();

// Khởi tạo tầng dịch vụ dữ liệu
string connectionString = builder.Configuration.GetConnectionString("LiteCommerceDB") ?? "";
Configuration.Initialize(connectionString);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// Configure default format
var cultureInfo = new CultureInfo("vi-VN");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Configure Application Context
ApplicationContext.Configure
(
    httpContextAccessor: app.Services.GetRequiredService<IHttpContextAccessor>(),
    webHostEnvironment: app.Services.GetRequiredService<IWebHostEnvironment>(),
    configuration: app.Configuration
);

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
