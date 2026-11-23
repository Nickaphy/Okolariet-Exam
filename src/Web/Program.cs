using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Refit;
using Web;
using Web.Adapters.Answers;
using Web.Adapters.Assets;
using Web.Adapters.Items;
using Web.Adapters.ModdedTemplates;
using Web.Adapters.Nickname;
using Web.Adapters.Projects;
using Web.Adapters.Questions;
using Web.Adapters.Staff;
using Web.Adapters.Templates;
using Web.Adapters.Versions;
using Web.Api;
using Web.Auth;
using Web.Ports.Answers;
using Web.Ports.Assets;
using Web.Ports.Items;
using Web.Ports.ModdedTemplates;
using Web.Ports.Nickname;
using Web.Ports.Projects;
using Web.Ports.Questions;
using Web.Ports.Staff;
using Web.Ports.Templates;
using Web.Ports.Versions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// wwwroot/appsettings.json → "Api:BaseUrl".
// Prism mock: http://localhost:4010   Real Host: http://localhost:5101/api/v1
var apiBaseUrl = new Uri(builder.Configuration["Api:BaseUrl"]
    ?? throw new InvalidOperationException("Api:BaseUrl is missing in wwwroot/appsettings.json."));

// Enum values exactly as in openapi.yaml, and nulls left out (Prism rejects null for non-nullable fields).
var jsonOptions = new JsonSerializerOptions(ApiJson.Options)
{
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};
var refitSettings = new RefitSettings(new SystemTextJsonContentSerializer(jsonOptions));

// ---- Auth: JWT for staff. The kiosk only uses anonymous endpoints.
builder.Services.AddSingleton<TokenStore>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddTransient<BearerTokenHandler>();
builder.Services.AddTransient<AttachAccessTokenHandler>();
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthStateProvider>());
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

// IAuthApi must not use BearerTokenHandler: that handler depends on AuthService, which uses IAuthApi.
builder.Services.AddRefitClient<IAuthApi>(refitSettings)
    .ConfigureHttpClient(c => c.BaseAddress = apiBaseUrl)
    .AddHttpMessageHandler<AttachAccessTokenHandler>();

// ---- API clients + gateways (pages talk to the gateways, not to Refit)
AddApi<INicknamesApi>();
builder.Services.AddScoped<INicknameGateway, NicknameGateway>();

AddApi<IProjectsApi>();
builder.Services.AddScoped<IProjectGateway, ProjectGateway>();

AddApi<IVersionsApi>();
builder.Services.AddScoped<IVersionGateway, VersionGateway>();

AddApi<IStaffApi>();
builder.Services.AddScoped<IStaffGateway, StaffGateway>();

AddApi<IQuestionsApi>();
builder.Services.AddScoped<IQuestionGateway, QuestionGateway>();

AddApi<IAnswersApi>();
builder.Services.AddScoped<IAnswerGateway, AnswerGateway>();

AddApi<IAssetsApi>();
builder.Services.AddScoped<IAssetGateway, AssetGateway>();

AddApi<ITemplatesApi>();
builder.Services.AddScoped<ITemplateGateway, TemplateGateway>();

AddApi<IModdedTemplatesApi>();
builder.Services.AddScoped<IModdedTemplateGateway, ModdedTemplateGateway>();

AddApi<IItemsApi>();
builder.Services.AddScoped<IItemGateway, ItemGateway>();

// No gateways yet
AddApi<ISessionsApi>();
AddApi<IUsersApi>();
AddApi<IStatisticsApi>();

await builder.Build().RunAsync();

void AddApi<T>() where T : class =>
    builder.Services.AddRefitClient<T>(refitSettings)
        .ConfigureHttpClient(c => c.BaseAddress = apiBaseUrl)
        .AddHttpMessageHandler<BearerTokenHandler>();