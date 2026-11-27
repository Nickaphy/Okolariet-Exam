using Application.Auth;
using Application.OutBoundPort.AuthRepository.Lockouts;
using Application.OutBoundPort.AuthRepository.Logins;
using Application.OutBoundPort.AuthRepository.RefreshToken;
using Application.OutBoundPort.AuthRepository.Tokens;
using Application.OutBoundPort.IPasswordHash;
using Application.OutBoundPort.ISessionRepository;
using Application.OutBoundPort.IStaffRepository;
using Application.OutBoundPort.IUserRepository;
using Application.OutBoundPort.IAssetRepository;
using Application.OutBoundPort.IFileStorage;
using Application.OutBoundPort.IModdedTemplateRepository;
using Application.OutBoundPort.IQuestionRepository;
using Application.OutBoundPort.IQuizResultRepository;
using Application.OutBoundPort.ITemplateRepository;
using Infrastructure.Adapters.Auth;
using Infrastructure.Adapters.Sessions;
using Infrastructure.Adapters.Staff;
using Infrastructure.Adapters.Users;
using Infrastructure.Adapters.Assets;
using Infrastructure.Adapters.ModdedTemplates;
using Infrastructure.Adapters.Questions;
using Infrastructure.Adapters.QuizResults;
using Infrastructure.Adapters.Templates;
using Infrastructure.Database;
using Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NicknamePorts = Application.OutBoundPort.INicknameWordRepository;
using Infrastructure.Adapters.Nickname;
using Application.OutBoundPort.IProjectRepository;
using Application.OutBoundPort.IVersionRepository;
using Infrastructure.Adapters.Projects;
using Infrastructure.Adapters.Versions;
using Infrastructure.Storage;
using Microsoft.Extensions.Options;

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

            // Asset DI
            services.AddScoped<IAddAssetAsync, AddAssetAdapter>();
            services.AddScoped<IAssetExistsAsync, AssetExistsAdapter>();
            services.AddScoped<IAssetInUseAsync, AssetInUseAdapter>();
            services.AddScoped<IDeleteAssetAsync, DeleteAssetAdapter>();
            services.AddScoped<IGetAssetByIdAsync, GetAssetByIdAdapter>();
            services.AddScoped<IListAssetsAsync, ListAssetsAdapter>();

            // File storage DI (one shared instance behind all three ports)
            services.AddSingleton<LocalFileStorageAdapter>();
            services.AddSingleton<ISaveFileAsync>(sp => sp.GetRequiredService<LocalFileStorageAdapter>());
            services.AddSingleton<IOpenFileAsync>(sp => sp.GetRequiredService<LocalFileStorageAdapter>());
            services.AddSingleton<IDeleteFileAsync>(sp => sp.GetRequiredService<LocalFileStorageAdapter>());

            // Template DI (catalog in code)
            services.AddSingleton<TemplateCatalogAdapter>();
            services.AddSingleton<IGetTemplateByTypeAsync>(sp => sp.GetRequiredService<TemplateCatalogAdapter>());
            services.AddSingleton<IListTemplatesAsync>(sp => sp.GetRequiredService<TemplateCatalogAdapter>());

            // ModdedTemplate DI
            services.AddScoped<IAddModdedTemplateAsync, AddModdedTemplateAdapter>();
            services.AddScoped<IDeleteModdedTemplateAsync, DeleteModdedTemplateAdapter>();
            services.AddScoped<IGetModdedTemplateByIdAsync, GetModdedTemplateByIdAdapter>();
            services.AddScoped<IIsLinkedFromButtonAsync, IsLinkedFromButtonAdapter>();
            services.AddScoped<IListModdedTemplatesByProjectAsync, ListModdedTemplatesByProjectAdapter>();
            services.AddScoped<ITemplateSupportsQuestionsAsync, TemplateSupportsQuestionsAdapter>();
            services.AddScoped<IUpdateModdedTemplateAsync, UpdateModdedTemplateAdapter>();
            services.AddScoped<ITemplateBelongsToProjectAsync, ModdedTemplateBelongsToProjectAdapter>();

            // Question DI (answers are saved through their question)
            services.AddScoped<IAddQuestionAsync, AddQuestionAdapter>();
            services.AddScoped<IGetQuestionByIdAsync, GetQuestionByIdAdapter>();
            services.AddScoped<IGetQuestionByAnswerIdAsync, GetQuestionByAnswerIdAdapter>();
            services.AddScoped<IListQuestionsByTemplateAsync, ListQuestionsByTemplateAdapter>();
            services.AddScoped<IUpdateQuestionAsync, UpdateQuestionAdapter>();

            // QuizResult DI
            services.AddScoped<ITryAddQuizResultAsync, TryAddQuizResultAdapter>();
            services.AddScoped<IListQuizResultsBySessionAsync, ListQuizResultsBySessionAdapter>();

            // Staff DI
            services.AddScoped<IAddAsync, AddStaffAdapter>();
            services.AddScoped<IAnyAsync, AnyStaffAdapter>();
            services.AddScoped<ICountAdminsAsync, CountAdminsAdapter>();
            services.AddScoped<IDeleteAsync, DeleteStaffAdapter>();
            services.AddScoped<IEmailExistsAsync, StaffEmailExistsAdapter>();
            services.AddScoped<IGetByEmailAsync, GetStaffByEmailAdapter>();
            services.AddScoped<IGetByIdAsync, GetStaffByIdAdapter>();
            services.AddScoped<IListAsync, ListStaffAdapter>();
            services.AddScoped<IUpdateAsync, UpdateStaffAdapter>();

            // Nickname DI
            services.AddScoped<NicknamePorts.IAddAsync, AddNicknameWordAdapter>();
            services.AddScoped<NicknamePorts.IExistsAsync, NicknameWordExistsAdapter>();
            services.AddScoped<NicknamePorts.IGetAsync, GetNicknameWordAdapter>();
            services.AddScoped<NicknamePorts.IRemoveAsync, RemoveNicknameWordAdapter>();
            services.AddScoped<NicknamePorts.ICountActiveAsync, CountActiveNicknameWordsAdapter>();

            // Project DI
            services.AddScoped<IAddProjectAsync, AddProjectAdapter>();
            services.AddScoped<IGetProjectByIdAsync, GetProjectByIdAdapter>();
            services.AddScoped<IListProjectsAsync, ListProjectsAdapter>();
            services.AddScoped<IUpdateProjectAsync, UpdateProjectAdapter>();
            services.AddScoped<IDeleteProjectAsync, DeleteProjectAdapter>();
            services.AddScoped<IModdedTemplateBelongsToProjectAsync, ModdedTemplateBelongsToProjectAdapter>();
            services.AddScoped<IProjectExistsAsync, ProjectExistsAdapter>();
            services.AddScoped<IIsMainModdedTemplateAsync, IsMainModdedTemplateAdapter>();

            // Session DI
            services.AddScoped<IProjectExists, ProjectExistsAdapter>();
            services.AddScoped<ICreateSessionAsync, CreateSessionAdapter>();
            services.AddScoped<IGetSessionByIdAsync, GetSessionByIdAdapter>();
            services.AddScoped<IGetSessionSummaryAsync, GetSessionSummaryAdapter>();

            // User DI
            services.AddScoped<ICreateUserAsync, CreateUserAdapter>();
            services.AddScoped<IClearQuizLeaderboardAsync, ClearQuizLeaderboardAdapter>();
            services.AddScoped<IDeleteUserAsync, DeleteUserAdapter>();
            services.AddScoped<IGetUserByIdAsync, GetUserByIdAdapter>();
            services.AddScoped<IListUsersAsync, ListUsersAdapter>();

            // Version DI
            services.AddScoped<IAddVersionAsync, AddVersionAdapter>();
            services.AddScoped<IListVersionsAsync, ListVersionsAdapter>();
            services.AddScoped<IGetLatestVersionAsync, GetLatestVersionAdapter>();
            services.AddScoped<IBuildSnapshotAsync, BuildSnapshotAdapter>();

            // Auth storage DI: logins, refresh tokens and lockouts in the database (StaffLogins, RefreshTokens,
            // LoginLockouts), so a restart of the Host doesn't log everyone out or forget a lockout.
            services.AddScoped<IGetLoginLockoutAsync, GetLoginLockoutAdapter>();
            services.AddScoped<ISaveLoginLockoutAsync, SaveLoginLockoutAdapter>();
            services.AddScoped<IAddLoginAsync, AddLoginAdapter>();
            services.AddScoped<IGetLoginByIdAsync, GetLoginByIdAdapter>();
            services.AddScoped<IUpdateLoginAsync, UpdateLoginAdapter>();
            services.AddScoped<IRevokeAllLoginsForStaffAsync, RevokeAllLoginsForStaffAdapter>();
            services.AddScoped<IAddRefreshTokenAsync, AddRefreshTokenAdapter>();
            services.AddScoped<IGetRefreshTokenByHashAsync, GetRefreshTokenByHashAdapter>();
            services.AddScoped<IUpdateRefreshTokenAsync, UpdateRefreshTokenAdapter>();

            // File storage DI
            // The adapter that reads and writes the files belongs to the Asset feature.
            services.Configure<FileStorageSettings>(configuration.GetSection("FileStorage"));

            // Seed data DI
            services.AddScoped<StaffSeeder>();
            services.AddScoped<NicknameWordSeeder>();

            var connectionString = configuration.GetConnectionString("Okolariet")
                ?? throw new InvalidOperationException("ConnectionStrings__Okolariet is missing. Fill it in in .env.");

            // Give each request a database context connected to SQL Server.
            services.AddDbContext<OkolarietDbContext>(options =>
                options.UseSqlServer(connectionString));

            return services;
        }

        public static async Task InitializeInfrastructureAsync(this IServiceProvider services, IConfiguration config)
        {
            using var scope = services.CreateScope();
            // Creates the database and tables if they don't exist. It never updates an existing database,
            // so after a change to an entity or configuration the database must be dropped (docker compose down -v).
            await scope.ServiceProvider.GetRequiredService<OkolarietDbContext>().Database.EnsureCreatedAsync();

            Directory.CreateDirectory(scope.ServiceProvider.GetRequiredService<IOptions<FileStorageSettings>>().Value.FullRootPath);

            await scope.ServiceProvider.GetRequiredService<StaffSeeder>()
              .SeedAsync(config["SeedAdmin:Email"], config["SeedAdmin:Password"]);
            await scope.ServiceProvider.GetRequiredService<NicknameWordSeeder>().SeedAsync();
        }
    }
}
