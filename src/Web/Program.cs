using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
using Refit;
using Web;
using Web.Adapters.Answers;
using Web.Adapters.Assets;
using Web.Adapters.Items;
using Web.Adapters.ModdedTemplates;
using Web.Adapters.Nickname;
using Web.Adapters.Projects;
using Web.Adapters.Questions;
using Web.Adapters.Sessions;
using Web.Adapters.Staff;
using Web.Adapters.Statistics;
using Web.Adapters.Templates;
using Web.Adapters.Users;
using Web.Adapters.Versions;
using Web.Api;
using Web.Auth;
using Web.Helpers;
using Web.Ports.Answers;
using Web.Ports.Assets;
using Web.Ports.Items;
using Web.Ports.ModdedTemplates;
using Web.Ports.Nickname;
using Web.Ports.Projects;
using Web.Ports.Questions;
using Web.Ports.Sessions;
using Web.Ports.Staff;
using Web.Ports.Statistics;
using Web.Ports.Templates;
using Web.Ports.Users;
using Web.Ports.Versions;
using Web.Services.Editor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// wwwroot/appsettings.json → "Api:BaseUrl".
// Prism mock: http://localhost:4010   Real Host: http://localhost:5101/api/v1
var apiBaseUrl = new Uri(builder.Configuration["Api:BaseUrl"]
    ?? throw new InvalidOperationException("Api:BaseUrl is missing in wwwroot/appsettings.json."));

// The shared JSON settings (enums as camelCase text, see ApiJson), plus: leave null values out of requests
// (Prism rejects null for fields that aren't nullable).
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
// (See the auth plan: AuthService still talks to Refit directly; it gets a gateway when auth is reworked.)
builder.Services.AddRefitClient<IAuthApi>(refitSettings)
    .ConfigureHttpClient(c => c.BaseAddress = apiBaseUrl)
    .AddHttpMessageHandler<AttachAccessTokenHandler>();

// ---- Gateways: the ONLY way pages and services reach the API (hexagonal architecture).
// Each line registers the generated Refit client (the HTTP part) and the gateway on top of it:
//   port (Ports/..., what the page asks for)  ←  adapter (Adapters/..., calls Refit and turns errors into GatewayException).
AddGateway<IProjectsApi, IProjectGateway, ProjectGateway>();
AddGateway<IVersionsApi, IVersionGateway, VersionGateway>();
AddGateway<ITemplatesApi, ITemplateGateway, TemplateGateway>();
AddGateway<IModdedTemplatesApi, IModdedTemplateGateway, ModdedTemplateGateway>();
AddGateway<IItemsApi, IItemGateway, ItemGateway>();
AddGateway<IQuestionsApi, IQuestionGateway, QuestionGateway>();
AddGateway<IAnswersApi, IAnswerGateway, AnswerGateway>();
AddGateway<IAssetsApi, IAssetGateway, AssetGateway>();
AddGateway<IStaffApi, IStaffGateway, StaffGateway>();
AddGateway<INicknamesApi, INicknameGateway, NicknameGateway>();
AddGateway<IStatisticsApi, IStatisticsGateway, StatisticsGateway>();
AddGateway<IUsersApi, IUserGateway, UserGateway>();
AddGateway<ISessionsApi, ISessionGateway, SessionGateway>();

// ---- Services built on the gateways
builder.Services.AddScoped<ExhibitionLoader>();     // loads an exhibition for the editor
builder.Services.AddScoped<ExhibitionEditor>();     // every change the editor makes
builder.Services.AddSingleton(new AssetUrls(apiBaseUrl));   // where uploaded files are loaded from

// Radzen UI components (colour picker, drop-downs, icons).
builder.Services.AddRadzenComponents();

await builder.Build().RunAsync();

// Registers one Refit client (with the API address and the login-token handler) and the gateway that uses it.
void AddGateway<TApi, TPort, TAdapter>()
    where TApi : class
    where TPort : class
    where TAdapter : class, TPort
{
    builder.Services.AddRefitClient<TApi>(refitSettings)
        .ConfigureHttpClient(c => c.BaseAddress = apiBaseUrl)
        .AddHttpMessageHandler<BearerTokenHandler>();
    builder.Services.AddScoped<TPort, TAdapter>();
}
