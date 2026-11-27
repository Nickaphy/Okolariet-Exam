using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Application.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Api;

public static class ApiDependencyInjection
{
    public static IServiceCollection AddAPI(this IServiceCollection services, IConfiguration config)
    {
        // Controllers live in this assembly, so AddApplicationPart is done here and not in the Host
        services.AddControllers()
            .AddApplicationPart(typeof(ApiDependencyInjection).Assembly)
            .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));   // Admin -> "admin" in JSON
        services.AddProblemDetails();

        // ---- JWT validation ----
        var jwt = config.GetSection("JwtSettings").Get<JwtSettings>();
        if (jwt is null || Encoding.UTF8.GetByteCount(jwt.Secret) < 32) // Stops if it's less than 32 characters
            throw new InvalidOperationException(
                "JwtSettings__Secret is missing or shorter than 32 bytes. Copy .env.example to .env and fill it in.");

        // ---- JWT Bearer authentication -----
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.MapInboundClaims = false;   // keep "sub", "role", "sid" exactly as issued
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)), // same as JwtTokenGenerator
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = "sub",
                    RoleClaimType = "role",
                };
                o.Events = new JwtBearerEvents
                {
                    OnChallenge = async ctx =>
                    {
                        ctx.HandleResponse();
                        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await ctx.Response.WriteAsJsonAsync(new ProblemDetails
                        {
                            Status = 401,
                            Title = "Unauthorized"
                        });
                    },
                    OnForbidden = async ctx =>
                    {
                        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await ctx.Response.WriteAsJsonAsync(new ProblemDetails
                        {
                            Status = 403,
                            Title = "Forbidden"
                        });
                    },
                };
            });

        // ---- Authorization: THE one place the role is enforced (see Application/Auth/AuthRoles).
        // Every endpoint needs a logged-in Admin, unless it says otherwise:
        //   [AllowAnonymous] = the kiosk's public endpoints;   [Authorize] = any logged-in staff member (log out).
        // Controllers don't repeat [Authorize(Roles = "Admin")]: the fallback policy already covers them.
        services.AddAuthorization(o => o.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireRole(AuthRoles.Admin)
            .Build());

        // ---- Rate limit on login/refresh, per client IP ----
        services.AddRateLimiter(o =>
        {
            o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            o.AddPolicy("auth", ctx => RateLimitPartition.GetFixedWindowLimiter(
                ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) })); // limits that only 10 calls are allowed per minute. Anti-botting defense
            o.OnRejected = (ctx, _) =>
            {
                ctx.HttpContext.Response.Headers.RetryAfter = "60";
                return ValueTask.CompletedTask;
            };
        });

        // ---- CORS for the Blazor app ----
        var webOrigin = config["Cors:WebOrigin"]
            ?? throw new InvalidOperationException("Cors:WebOrigin is missing in appsettings.json."); // prevents CORS errors, important since the Web and Host run on different ports.
        services.AddCors(o => o.AddDefaultPolicy(p => p
            .WithOrigins(webOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("ETag", "Location", "Retry-After")));

        return services;
    }
}
