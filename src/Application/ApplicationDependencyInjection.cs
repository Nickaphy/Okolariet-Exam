using Application.Helpers;
using Application.Helpers.Interfaces;
using Application.InBoundPort.AuthUseCases;
using Application.InBoundPort.INicknameUseCases;
using Application.InBoundPort.IStaffUseCases;
using Application.InBoundPort.IAnswerUseCases;
using Application.InBoundPort.IQuestionUseCases;
using Application.InBoundPort.IAssetUseCases;
using Application.InBoundPort.ITemplateUseCases;
using Application.InBoundPort.IModdedTemplateUseCases;
using Application.UseCases.Auth;
using Application.UseCases.Nickname;
using Application.UseCases.Staff;
using Application.UseCases.Answers;
using Application.UseCases.Questions;
using Application.UseCases.Assets;
using Application.UseCases.Templates;
using Application.UseCases.ModdedTemplates;
using Domain.Services;
using Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

            // Question use cases
            services.AddScoped<IListQuestionsUseCase, ListQuestionsUseCase>();
            services.AddScoped<IGetQuestionUseCase, GetQuestionUseCase>();
            services.AddScoped<ICreateQuestionUseCase, CreateQuestionUseCase>();
            services.AddScoped<IUpdateQuestionUseCase, UpdateQuestionUseCase>();
            services.AddScoped<IDeleteQuestionUseCase, DeleteQuestionUseCase>();

            // Answer use cases
            services.AddScoped<ICreateAnswerUseCase, CreateAnswerUseCase>();
            services.AddScoped<IUpdateAnswerUseCase, UpdateAnswerUseCase>();
            services.AddScoped<IDeleteAnswerUseCase, DeleteAnswerUseCase>();

            // Asset use cases
            services.AddScoped<IListAssetsUseCase, ListAssetsUseCase>();
            services.AddScoped<IGetAssetUseCase, GetAssetUseCase>();
            services.AddScoped<IUploadAssetUseCase, UploadAssetUseCase>();
            services.AddScoped<IDeleteAssetUseCase, DeleteAssetUseCase>();

            // Template use cases
            services.AddScoped<IListTemplatesUseCase, ListTemplatesUseCase>();
            services.AddScoped<IGetTemplateUseCase, GetTemplateUseCase>();

            // ModdedTemplate use cases
            services.AddScoped<IListModdedTemplatesUseCase, ListModdedTemplatesUseCase>();
            services.AddScoped<IGetModdedTemplateUseCase, GetModdedTemplateUseCase>();
            services.AddScoped<ICreateModdedTemplateUseCase, CreateModdedTemplateUseCase>();
            services.AddScoped<IUpdateModdedTemplateUseCase, UpdateModdedTemplateUseCase>();
            services.AddScoped<IDeleteModdedTemplateUseCase, DeleteModdedTemplateUseCase>();
            services.AddScoped<ISetItemUseCase, SetItemUseCase>();
            services.AddScoped<IClearItemUseCase, ClearItemUseCase>();

            return services;
        }
    }
}
