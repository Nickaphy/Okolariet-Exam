using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Refit;
using Web;
using Web.Adapters.Nickname;
using Web.Api;
using Web.Auth;
using Web.Ports.Nickname;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
var baseUrl = new Uri(builder.Configuration["Api:BaseUrl"]!);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = baseUrl });
builder.Services.AddSingleton<TokenStore>();
builder.Services.AddTransient<AuthHandler>();

//Access token system, using JWT to ensure that a user doesn't get access to the editor, only staff memebers.
builder.Services.AddRefitClient<IAuthApi>()
    .ConfigureHttpClient(c => c.BaseAddress = baseUrl);

void AddApi<T>() where T : class =>
    builder.Services.AddRefitClient<T>()
        .ConfigureHttpClient(c => c.BaseAddress = baseUrl);
        //.AddHttpMessageHandler<AuthHandler>();

// User nicknames
AddApi<INicknamesApi>();
builder.Services.AddScoped<INicknameGateway, NicknameGateway>();

// The collections of Templates
AddApi<IProjectsApi>();

// A users access to a project
AddApi<ISessionsApi>();

// The guest = user
AddApi<IUsersApi>();

// Staff at Økolariet
AddApi<IStaffApi>();

// The questions to a quiz
AddApi<IQuestionsApi>();

// Answers to a quiz
AddApi<IAnswersApi>();

// Assets are general items found in a tempalte, such as buttons, textfields, images, etc.
AddApi<IAssetsApi>();

// A collections of Assets, Questions or other items. Specifically unaltered by a staff member. A clean slate so to speak.
AddApi<ITemplatesApi>();

// A Template that has been altered by a staff member
AddApi<IModdedTemplatesApi>();

// Different published versions of projects, to ensure that a staff member only publishes a satisfactory whole, and not a half-finished project.
AddApi<IVersionsApi>();

// Statistics about the usage of the application, such as how many users have used it, quiz results etc.
AddApi<IStatisticsApi>();


AddApi<IItemsApi>();


await builder.Build().RunAsync();