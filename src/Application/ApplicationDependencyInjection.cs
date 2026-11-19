using Application.Helpers;
using Application.Helpers.Interfaces;
using Application.InBoundPort;
using Application.InBoundPort.AuthUseCases;
using Application.InBoundPort.INicknameUseCases;
using Application.InBoundPort.IStaffUseCases;
using Application.InBoundPort.IAnswerUseCases;
using Application.InBoundPort.IQuestionUseCases;
using Application.InBoundPort.IAssetUseCases;
using Application.InBoundPort.IQuizResultUseCases;
using Application.InBoundPort.IUserUsecases;
using Application.InBoundPort.ISessionUseCase;
using Application.UseCases.Auth;
using Application.UseCases.Nickname;
using Application.UseCases.Staff;
using Application.UseCases.Answers;
using Application.UseCases.Questions;
using Application.UseCases.Assets;
using Application.UseCases.QuizResults;
using Application.UseCases.Users;
using Application.UseCases.Sessions;
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

            // Session
            services.AddScoped<IPostSessionAsync, AddSessionUseCase>();
            services.AddScoped<IGetSessionUseCase, GetSessionUseCase>();


            // User
            services.AddScoped<ICreateUserUseCase, CreateUserUseCase>();

            // Quiz
            services.AddScoped<ISubmitQuizResultUseCase, SubmitQuizResultUseCase>();

            
            

            return services;
        }
    }
}
