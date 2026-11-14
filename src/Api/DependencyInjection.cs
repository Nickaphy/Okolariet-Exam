using Application.InBoundPort.INicknameUseCases;
using Application.UseCases.Nickname;

namespace Api
{
    // Dependency Injection specifically for the applications layers Use Cases
    public static class APIDependencyInjection
    {
        public static IServiceCollection AddAPIService(this IServiceCollection services)
        {
            services.AddScoped<IAddWordAsync, AddWordUseCase>();
            services.AddScoped<IDeleteWordAsync, DeleteWordUseCase>();

            return services;
        }
    }
}
