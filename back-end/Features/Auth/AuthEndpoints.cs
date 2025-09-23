using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Kanban.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Kanban.Features.Auth;

public static class AuthEndPoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        //POST /api/auth/register
        group.MapPost("/register", async (RegisterRequest req, UserManager<AppUser> userManager, IConfiguration cfg) =>
        {
            var user = new AppUser { UserName = req.Email, Email = req.Email };
            var res = await userManager.CreateAsync(user, req.Password);
            if (!res.Succeeded) return Results.BadRequest(res.Errors);
            return Results.Ok();
        });

        //POST /api/auth/login
        group.MapPost("/login", async (LoginRequest req, UserManager<AppUser> UserManager, IConfiguration cfg) =>
        {
            var user = await UserManager.FindByEmailAsync(req.Email);
            if (user is null || !await UserManager.CheckPasswordAsync(user, req.Password))
                return Results.Unauthorized();

            // Build JWT
            var issuer = cfg["Auth:Issuer"];
            var audience = cfg["Auth:Audience"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Auth:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? "")
            };

            var expires = DateTime.UtcNow.AddHours(1);
            var token = new JwtSecurityToken(issuer, audience, claims, expires: expires, signingCredentials: creds);
            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            return Results.Ok(new
            {
                access_token = accessToken,
                expires_in = (int)(expires - DateTime.UtcNow).TotalSeconds
            });
        });

        //GET /api/auth/me (protected)
        group.MapGet("/me", [Authorize] (ClaimsPrincipal me) =>
        {
            var id = me.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = me.FindFirstValue(ClaimTypes.Email);
            return Results.Ok(new { id, email });
        });

        return group;
    }
}