using FinTrack.Core.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Host.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", Register);
        group.MapPost("/login", Login);
        group.MapPost("/logout", Logout).RequireAuthorization();
        group.MapGet("/me", GetCurrentUser).RequireAuthorization();
    }

    private static async Task<IResult> Register(
        [FromBody] RegisterRequest request,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName ?? request.Email.Split('@')[0]
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return Results.ValidationProblem(
                result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description }));
        }

        await signInManager.SignInAsync(user, isPersistent: true);

        return Results.Created($"/api/auth/me", new UserResponse(user.Id, user.Email!, user.DisplayName));
    }

    private static async Task<IResult> Login(
        [FromBody] LoginRequest request,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        var result = await signInManager.PasswordSignInAsync(
            request.Email,
            request.Password,
            request.RememberMe,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                return Results.Problem("Account locked out", statusCode: 423);
            }
            return Results.Problem("Invalid credentials", statusCode: 401);
        }

        var user = await userManager.FindByEmailAsync(request.Email);
        return Results.Ok(new UserResponse(user!.Id, user.Email!, user.DisplayName));
    }

    private static async Task<IResult> Logout(SignInManager<ApplicationUser> signInManager)
    {
        await signInManager.SignOutAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> GetCurrentUser(
        HttpContext context,
        UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.GetUserAsync(context.User);
        if (user == null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new UserResponse(user.Id, user.Email!, user.DisplayName));
    }
}

public record RegisterRequest(string Email, string Password, string? DisplayName);
public record LoginRequest(string Email, string Password, bool RememberMe = false);
public record UserResponse(Guid Id, string Email, string DisplayName);
