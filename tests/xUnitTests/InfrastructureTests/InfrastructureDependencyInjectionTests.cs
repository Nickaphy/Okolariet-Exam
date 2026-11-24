using Application.Auth;                                   // JwtSettings
using Application.OutBoundPort.AuthRepository.Lockouts;
using Application.OutBoundPort.AuthRepository.Logins;
using Application.OutBoundPort.AuthRepository.RefreshToken;
using Application.OutBoundPort.AuthRepository.Tokens;
using Application.OutBoundPort.IPasswordHash;
using Application.OutBoundPort.IStaffRepository;
using Infrastructure;                                     // AddInfrastructure
using Infrastructure.Testing;                             // InMemoryAuthStore
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NicknamePorts = Application.OutBoundPort.INicknameWordRepository;
using Application.InBoundPort.IStaffUseCases;
using Application.UseCases.Staff;

namespace xUnitTests.InfrastructureTests
{
    // Checks that every outbound port Infrastructure promises actually has an adapter behind it.
    // This is the kind of mistake that otherwise only shows up when the Host starts.
    public class InfrastructureDependencyInjectionTests
    {
        private static IConfiguration Config(bool withConnectionString = true)
        {
            var values = new Dictionary<string, string?>
            {
                ["JwtSettings:Secret"] = "unit-test-secret-that-is-longer-than-32-bytes!",
                ["JwtSettings:Issuer"] = "okolariet-api",
                ["JwtSettings:Audience"] = "okolariet-web",
                ["JwtSettings:AccessTokenMinutes"] = "15",
            };
            if (withConnectionString)
                values["ConnectionStrings:okolariet"] = "Server=unused;Database=unused;TrustServerCertificate=True";

            return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        }

        // ValidateOnBuild fails here if any registered adapter is missing one of its own dependencies.
        private static ServiceProvider Build() =>
            new ServiceCollection()
                .AddInfrastructure(Config())
                .AddScoped<ICreateStaffUseCase, CreateStaffUseCase>()
                .AddSingleton(TimeProvider.System)
                .AddLogging()
                .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

        [Theory]
        [InlineData(typeof(IHash))]
        [InlineData(typeof(IVerify))]
        [InlineData(typeof(IVerifyDummy))]
        [InlineData(typeof(IJwtTokenGenerator))]
        [InlineData(typeof(IGenerateRefreshToken))]
        [InlineData(typeof(IHashToken))]
        [InlineData(typeof(IGetByEmailAsync))]
        [InlineData(typeof(IGetLoginLockoutAsync))]
        [InlineData(typeof(ISaveLoginLockoutAsync))]
        [InlineData(typeof(IAddLoginAsync))]
        [InlineData(typeof(IGetLoginByIdAsync))]
        [InlineData(typeof(IUpdateLoginAsync))]
        [InlineData(typeof(IRevokeAllLoginsForStaffAsync))]
        [InlineData(typeof(IAddRefreshTokenAsync))]
        [InlineData(typeof(IGetRefreshTokenByHashAsync))]
        [InlineData(typeof(IUpdateRefreshTokenAsync))]
        [InlineData(typeof(NicknamePorts.IAddAsync))]
        [InlineData(typeof(NicknamePorts.IExistsAsync))]
        [InlineData(typeof(NicknamePorts.IGetAsync))]
        [InlineData(typeof(NicknamePorts.IRemoveAsync))]
        [InlineData(typeof(NicknamePorts.ICountActiveAsync))]
        public void Port_has_an_adapter(Type port)
        {
            using var provider = Build();
            using var scope = provider.CreateScope();

            Assert.NotNull(scope.ServiceProvider.GetRequiredService(port));
        }

        [Fact]
        public void All_auth_storage_ports_share_one_in_memory_store()
        {
            // If they didn't, a refresh token saved at login would be invisible to the refresh use case.
            using var provider = Build();
            var store = provider.GetRequiredService<InMemoryAuthStore>();

            Assert.Same(store, provider.GetRequiredService<IAddRefreshTokenAsync>());
            Assert.Same(store, provider.GetRequiredService<IGetRefreshTokenByHashAsync>());
            Assert.Same(store, provider.GetRequiredService<IAddLoginAsync>());
            Assert.Same(store, provider.GetRequiredService<IGetLoginByIdAsync>());
            Assert.Same(store, provider.GetRequiredService<ISaveLoginLockoutAsync>());
            Assert.Same(store, provider.GetRequiredService<IGetLoginLockoutAsync>());
        }

        [Fact]
        public void JwtSettings_are_read_from_configuration()
        {
            using var provider = Build();

            var settings = provider.GetRequiredService<IOptions<JwtSettings>>().Value;

            Assert.Equal("okolariet-api", settings.Issuer);
            Assert.Equal("okolariet-web", settings.Audience);
            Assert.Equal(15, settings.AccessTokenMinutes);
        }

        [Fact]
        public void Missing_connection_string_stops_startup_with_a_clear_message()
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
                new ServiceCollection().AddInfrastructure(Config(withConnectionString: false)));

            Assert.Contains("okolariet", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
