using AspnetCoreMvcFull.Filters;
using AspnetCoreMvcFull.Middleware;
using AspnetCoreMvcFull.Services.Auth;
using AspnetCoreMvcFull.Services.Common;
using AspnetCoreMvcFull.Services.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Menambahkan HttpClient untuk digunakan di AuthService
builder.Services.AddHttpClient("AuthClient", client =>
{
  client.Timeout = TimeSpan.FromSeconds(30);
});

// Register memory cache untuk penyimpanan auth-related data
builder.Services.AddMemoryCache();

// Register auth dan security services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICacheService, MemoryCacheService>();
builder.Services.AddScoped<ISecurityService, SecurityService>();
builder.Services.AddScoped<RateLimitFilter>();

// Konfigurasi autentikasi
builder.Services.AddAuthentication(options =>
{
  options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
  options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
  options.Cookie.Name = "CraneBookingAuth";
  options.Cookie.HttpOnly = true;
  options.ExpireTimeSpan = TimeSpan.FromHours(8);
  options.SlidingExpiration = true;
  options.LoginPath = "/Auth/Login";
  options.LogoutPath = "/Auth/Logout";
  options.AccessDeniedPath = "/Auth/AccessDenied";
  options.Cookie.SameSite = SameSiteMode.Strict;
  options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

  // Event untuk validasi cookie dan automatic refresh token
  options.Events = new CookieAuthenticationEvents
  {
    OnValidatePrincipal = async context =>
    {
      // Cek apakah cookie akan segera kedaluwarsa
      if (context.Properties.AllowRefresh != true) return;

      var timeElapsed = DateTimeOffset.Now.Subtract(context.Properties.IssuedUtc ?? DateTimeOffset.Now);
      if (timeElapsed > TimeSpan.FromHours(7)) // Refresh setelah 7 jam (sebelum 8 jam kedaluwarsa)
      {
        var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
        if (context.HttpContext.Request.Cookies.TryGetValue("jwt_token", out var token) &&
                context.HttpContext.Request.Cookies.TryGetValue("refresh_token", out var refreshToken))
        {
          var response = await authService.RefreshTokenAsync(token, refreshToken);
          if (!response.Success)
          {
            context.RejectPrincipal();
            return;
          }

          // Update cookies
          context.HttpContext.Response.Cookies.Append("jwt_token", response.Token, new CookieOptions
          {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = response.TokenExpires
          });

          context.HttpContext.Response.Cookies.Append("refresh_token", response.RefreshToken, new CookieOptions
          {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.Now.AddDays(7)
          });

          // Update properti authentication ticket
          context.Properties.IssuedUtc = DateTimeOffset.Now;
          context.Properties.ExpiresUtc = DateTimeOffset.Now.Add(options.ExpireTimeSpan);
          context.ShouldRenew = true;
        }
        else
        {
          context.RejectPrincipal();
        }
      }
    }
  };
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Home/Error");
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Tambahkan middleware autentikasi SEBELUM otorisasi
app.UseAuthentication();
app.UseAuthorization();

// Tambahkan middleware token refresh
app.UseTokenRefresh();

// Log aktivitas keamanan untuk resource terproteksi
app.Use(async (context, next) =>
{
  // Log semua upaya akses ke resource yang terproteksi
  if (context.Request.Path.StartsWithSegments("/Admin") ||
      context.Request.Path.StartsWithSegments("/Management"))
  {
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation(
        "Upaya akses ke resource terproteksi: {Path} oleh {User} dari {IP}",
        context.Request.Path,
        context.User.Identity?.Name ?? "anonymous",
        context.Connection.RemoteIpAddress);
  }

  await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboards}/{action=Index}/{id?}");

app.Run();
