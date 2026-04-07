using Microsoft.AspNetCore.Authentication.Cookies;
using SV22T1020607.BusinessLayers;
using SV22T1020607.Shop;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews()
    .AddMvcOptions(option =>
    {
        option.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    });

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.Cookie.Name = "AuthenticationCookieShop";
        option.LoginPath = "/Account/Login";
        option.AccessDeniedPath = "/Account/AccessDenied";
        option.ExpireTimeSpan = TimeSpan.FromDays(30);
    });

builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromMinutes(60);
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
});

var app = builder.Build();

// Khởi tạo tầng dịch vụ dữ liệu
string connectionString = builder.Configuration.GetConnectionString("LiteCommerceDB") ?? "";
Configuration.Initialize(connectionString);

// Khởi tạo ApplicationContext
ApplicationContext.Configure(
    app.Services.GetRequiredService<IHttpContextAccessor>(),
    app.Services.GetRequiredService<IWebHostEnvironment>(),
    app.Configuration
);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
