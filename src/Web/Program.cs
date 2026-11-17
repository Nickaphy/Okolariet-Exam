using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Refit;
using Web;
using Web.Adapters.Nickname;
using Web.Adapters.Answers;
using Web.Adapters.Questions;
using Web.Adapters.Assets;
using Web.Api;
using Web.Api.Contracts;
using Web.Auth;
using Web.Ports.Nickname;
using Web.Ports.Answers;
using Web.Ports.Questions;
using Web.Ports.Assets;
using Web.TestTokens;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
var baseUrl = new Uri(builder.Configuration["Api:BaseUrl"]!);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = baseUrl });
// Turn back to real token when going into production !!!!!!!!!!
builder.Services.AddSingleton<TokenStoreTest>();
builder.Services.AddTransient<AuthHandlerTest>();

// PRISM test URI
var apiUrl = new Uri("http://localhost:4010");

//Access token system, using JWT to ensure that a user doesn't get access to the editor, only staff memebers.
builder.Services.AddRefitClient<IAuthApi>()
    .ConfigureHttpClient(c => c.BaseAddress = apiUrl);

var jsonOptions = SystemTextJsonContentSerializer.GetDefaultJsonSerializerOptions();
jsonOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

var refitSettings = new RefitSettings
{
    ContentSerializer = new SystemTextJsonContentSerializer(jsonOptions)
};

builder.Services.AddRefitClient<INicknamesApi>(refitSettings)
    .ConfigureHttpClient(c => c.BaseAddress = apiUrl)
    .AddHttpMessageHandler<AuthHandlerTest>();

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
builder.Services.AddScoped<IQuestionGateway, QuestionGateway>();

// Answers to a quiz
AddApi<IAnswersApi>();
builder.Services.AddScoped<IAnswerGateway, AnswerGateway>();

// Assets are general items found in a tempalte, such as buttons, textfields, images, etc.
AddApi<IAssetsApi>();
builder.Services.AddScoped<IAssetGateway, AssetGateway>();

// A collections of Assets, Questions or other items. Specifically unaltered by a staff member. A clean slate so to speak.
AddApi<ITemplatesApi>();

// A Template that has been altered by a staff member
AddApi<IModdedTemplatesApi>();

// Different published versions of projects, to ensure that a staff member only publishes a satisfactory whole, and not a half-finished project.
AddApi<IVersionsApi>();

// Statistics about the usage of the application, such as how many users have used it, quiz results etc.
AddApi<IStatisticsApi>();


AddApi<IItemsApi>();

var host = builder.Build();

if (builder.HostEnvironment.IsDevelopment()) // Test, only used in development. 
{
    var auth = host.Services.GetRequiredService<IAuthApi>();
    var response = await auth.CreateToken(new TokenRequest
    {
        GrantType = TokenRequestGrantType.Password,
        Email = "anna@oekolariet.dk",
        Password = "correct horse battery"
    });

    if (response.IsSuccessStatusCode && response.Content is not null)
        host.Services.GetRequiredService<TokenStoreTest>().Set(response.Content.TokenType);
}

await host.RunAsync();

