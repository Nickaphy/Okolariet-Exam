using Application.Auth;
using Application.OutBoundPort.AuthRepository.Lockouts;
using Application.OutBoundPort.AuthRepository.Logins;
using Application.OutBoundPort.AuthRepository.RefreshToken;
using Application.OutBoundPort.AuthRepository.Tokens;
using Application.OutBoundPort.IPasswordHash;
using Application.OutBoundPort.IStaffRepository;
using Infrastructure.Adapters.Auth;
using Infrastructure.Adapters.Staff;
using Infrastructure.Database;
using Infrastructure.Security;
using Infrastructure.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    // Dependency Injection for Infrastructure -> Seeddata, Database, Security (JWT) and E/F Core configuration
    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IHash, AspNetPasswordHasher>();
            services.AddSingleton<IVerify, AspNetPasswordHasher>();
            services.AddSingleton<IVerifyDummy, AspNetPasswordHasher>();

            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddSingleton<IGenerateRefreshToken, GenerateRefreshTokenAdapter>();
            services.AddSingleton<IHashToken, HashTokenAdapter>();

            // Staff DI
            services.AddScoped<IGetByEmailAsync, GetStaffByEmailAdapter>();

            // TEMPORARY: one shared instance behind all nine ports (singleton, so data survives between requests)
            services.AddSingleton<InMemoryAuthStore>();
            services.AddSingleton<IGetLoginLockoutAsync>(sp => sp.GetRequiredService<InMemoryAuthStore>());
            services.AddSingleton<ISaveLoginLockoutAsync>(sp => sp.GetRequiredService<InMemoryAuthStore>());
            services.AddSingleton<IAddLoginAsync>(sp => sp.GetRequiredService<InMemoryAuthStore>());
            services.AddSingleton<IGetLoginByIdAsync>(sp => sp.GetRequiredService<InMemoryAuthStore>());
            services.AddSingleton<IUpdateLoginAsync>(sp => sp.GetRequiredService<InMemoryAuthStore>());
            services.AddSingleton<IRevokeAllLoginsForStaffAsync>(sp => sp.GetRequiredService<InMemoryAuthStore>());
            services.AddSingleton<IAddRefreshTokenAsync>(sp => sp.GetRequiredService<InMemoryAuthStore>());
            services.AddSingleton<IGetRefreshTokenByHashAsync>(sp => sp.GetRequiredService<InMemoryAuthStore>());
            services.AddSingleton<IUpdateRefreshTokenAsync>(sp => sp.GetRequiredService<InMemoryAuthStore>());
            return services;
        }

        public static async Task InitializeInfrastructureAsync(this IServiceProvider services, IConfiguration config)
        {
            using var scope = services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<OkolarietDbContext>().Database.MigrateAsync();
           // await scope.ServiceProvider.GetRequiredService<StaffSeeder>()
           //  .SeedAsync(config["SeedAdmin:Email"], config["SeedAdmin:Password"]);
        }
    }
}
