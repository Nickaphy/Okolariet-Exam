using Application.Helpers;
using Application.Helpers.Interfaces;
using Application.InBoundPort.AuthUseCases;
using Application.InBoundPort.INicknameUseCases;
using Application.InBoundPort.IProjectUseCases;
using Application.InBoundPort.IStaffUseCases;
using Application.UseCases.Auth;
using Application.UseCases.Nickname;
using Application.UseCases.Projects;
using Application.UseCases.Staff;
using Domain.Services;
using Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Application.InBoundPort.IVersionUseCases;
using Application.UseCases.Versions;
namespace Application
{
    // Dependency Injection specifically for the applications layers Use Cases
    public static class ApplicationDependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            //General purpose:
            services.TryAddSingleton(TimeProvider.System);

            // Nickname use cases
            services.AddScoped<IAddWordAsync, AddWordUseCase>();
            services.AddScoped<IDeleteWordAsync, DeleteWordUseCase>();

            // Staff use cases
            services.AddScoped<IListStaffUseCase, ListStaffUseCase>();
            services.AddScoped<IGetStaffUseCase, GetStaffUseCase>();
            services.AddScoped<ICreateStaffUseCase, CreateStaffUseCase>();
            services.AddScoped<IUpdateStaffUseCase, UpdateStaffUseCase>();
            services.AddScoped<IDeleteStaffUseCase, DeleteStaffUseCase>();
            services.AddScoped<ISetStaffPasswordUseCase, SetStaffPasswordUseCase>();

            // Auth
            services.AddSingleton(new PasswordPolicy());
            services.AddSingleton<IPasswordValidatorService, PasswordValidatorService>();
            services.AddScoped<ITokenIssuer, TokenIssuer>();
            services.AddScoped<ILoginUseCase, LoginUseCase>();
            services.AddScoped<IRefreshTokensUseCase, RefreshTokensUseCase>();
            services.AddScoped<ILogoutUseCase, LogoutUseCase>();

            //Project
            services.AddScoped<IListProjectsUseCase, ListProjectsUseCase>();
            services.AddScoped<IGetProjectUseCase, GetProjectUseCase>();
            services.AddScoped<ICreateProjectUseCase, CreateProjectUseCase>();
            services.AddScoped<IUpdateProjectUseCase, UpdateProjectUseCase>();
            services.AddScoped<IDeleteProjectUseCase, DeleteProjectUseCase>();

            //PublishedVersionProject
            services.AddScoped<IListVersionsUseCase, ListVersionsUseCase>();
            services.AddScoped<IPublishVersionUseCase, PublishVersionUseCase>();
            services.AddScoped<IGetLatestVersionUseCase, GetLatestVersionUseCase>();
            services.AddScoped<IGetDraftVersionUseCase, GetDraftVersionUseCase>();

            return services;
        }
    }
}
