using System.Security.Claims;
using FinTrack.Core.Domain.Entities;
using FinTrack.Core.Services;
using FinTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

namespace FinTrack.Host.Auth;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddFinTrackAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        // Add Authorization
        services.AddAuthorization();

        // Add Identity
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<FinTrackDbContext>()
            .AddDefaultTokenProviders();

        // Configure cookie
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.SlidingExpiration = true;
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = 403;
                return Task.CompletedTask;
            };
        });

        return services;
    }

    public static string? GetUserId(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier);

    public static string? GetUserEmail(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.Email);

    public static string? GetUserDisplayName(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.Name);
}
