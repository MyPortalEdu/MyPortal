using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MyPortal.Auth.Cache;
using MyPortal.Auth.Certificates;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Factories;
using MyPortal.Auth.Handlers;
using MyPortal.Auth.Interfaces;
using MyPortal.Auth.Managers;
using MyPortal.Auth.Models;
using MyPortal.Auth.Policies;
using MyPortal.Auth.Providers;
using MyPortal.Auth.Stores;
using MyPortal.Common.Interfaces;
using MyPortal.Common.Options;
using MyPortal.Data.Extensions;
using MyPortal.Data.Factories;
using MyPortal.Data.Security;
using MyPortal.Data.UnitOfWork;
using MyPortal.FileStorage.Extensions;
using MyPortal.Services.Extensions;
using MyPortal.WebApi;
using MyPortal.WebApi.Filters;
using MyPortal.WebApi.Infrastructure.Extensions;
using MyPortal.WebApi.Infrastructure.Middleware;
using MyPortal.WebApi.Providers;
using MyPortal.WebApi.Services;
using MyPortal.WebApi.Services.Background;
using MyPortal.WebApi.Swagger;
using MyPortal.WebApi.Transformers;
using OpenIddict.Validation.AspNetCore;
using QueryKit.Dialects;
using Scalar.AspNetCore;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Sorting;
using PasswordOptions = MyPortal.Common.Options.PasswordOptions;

static bool IsApiRequest(HttpRequest req)
{
    if (req.Path.StartsWithSegments("/connect/authorize") ||
        req.Path.StartsWithSegments("/connect/endsession"))
    {
        return false;
    }

    // treat JSON/XHR or /api paths as API calls
    if (req.Path.StartsWithSegments("/api") || req.Path.StartsWithSegments("/connect"))
    {
        return true;
    }

    var accept = req.Headers["Accept"].ToString();
    if (accept.Contains("application/json", StringComparison.OrdinalIgnoreCase))
    {
        return true;
    }

    var xrw = req.Headers["X-Requested-With"].ToString();
    if (xrw.Equals("XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
    {
        return true;
    }

    return false;
}

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddOptions<DatabaseOptions>()
    .Bind(builder.Configuration.GetSection("Database"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.ConnectionString),
        "Connection string must be provided.")
    .ValidateOnStart();

builder.Services.AddOptions<FileStorageOptions>()
    .Bind(builder.Configuration.GetSection("FileStorage"))
    .ValidateOnStart();

builder.Services.AddFileStorage();

builder.Services.AddOptions<CertificateOptions>()
    .Bind(builder.Configuration.GetSection("Certificates"));

QueryKit.Extensions.ConnectionExtensions.UseDialect(Dialect.SQLServer);

builder.Services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

builder.Services.AddDbContext<AuthDbContext>(o =>
{
    o.UseSqlServer(builder.Configuration.GetSection("Database:ConnectionString").Value);
});

var passwordOpts = new PasswordOptions();
builder.Configuration.GetSection("PasswordRequirements").Bind(passwordOpts);

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = passwordOpts.RequiredLength;
        options.Password.RequireNonAlphanumeric = passwordOpts.RequireNonAlphanumeric;
        options.Password.RequireLowercase = passwordOpts.RequireLowercase;
        options.Password.RequireUppercase = passwordOpts.RequireUppercase;
        options.Password.RequireDigit = passwordOpts.RequireDigit;

        // Lockout: 5 failed attempts → locked for 15 minutes. AllowedForNewUsers
        // means CreateAsync sets LockoutEnabled=true on every new user.
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<ApplicationRole>()
    .AddUserStore<SqlUserStore>()
    .AddRoleStore<SqlRoleStore>()
    .AddSignInManager<ApplicationSignInManager>()
    .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<ISecurityStampValidator, SecurityStampValidator<ApplicationUser>>();
builder.Services.AddScoped<ITwoFactorSecurityStampValidator, TwoFactorSecurityStampValidator<ApplicationUser>>();
builder.Services.Configure<SecurityStampValidatorOptions>(o =>
{
    o.ValidationInterval = TimeSpan.FromMinutes(5);
});

var certOpts = new CertificateOptions();
builder.Configuration.GetSection("Certificates").Bind(certOpts);

builder.Services.AddOpenIddict()
    .AddCore(o =>
    {
        o.UseEntityFrameworkCore()
            .UseDbContext<AuthDbContext>()
            .ReplaceDefaultEntities<Guid>();
    })
    .AddServer(o =>
    {
        o.SetAccessTokenLifetime(TimeSpan.FromMinutes(60));
        o.SetRefreshTokenLifetime(TimeSpan.FromDays(14));

        o.SetTokenEndpointUris("/connect/token");
        o.SetAuthorizationEndpointUris("/connect/authorize");
        o.SetEndSessionEndpointUris("/connect/endsession");
        o.SetUserInfoEndpointUris("/connect/userinfo");

        //o.AcceptAnonymousClients();
        o.AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange();
        o.AllowRefreshTokenFlow();

        o.RegisterScopes("api", "email", "profile", "offline_access");

        if (builder.Environment.IsDevelopment())
        {
            o.AddDevelopmentEncryptionCertificate();
            o.AddDevelopmentSigningCertificate();
        }
        else
        {
            var prodSign = CertLoader.Load(certOpts.Signing);
            var prodEnc = CertLoader.Load(certOpts.Encryption);

            o.AddSigningCertificate(prodSign);
            o.AddEncryptionCertificate(prodEnc);
        }

        o.UseAspNetCore()
            .EnableTokenEndpointPassthrough()
            .EnableAuthorizationEndpointPassthrough()
            .EnableUserInfoEndpointPassthrough()
            .EnableEndSessionEndpointPassthrough();
    })
    .AddValidation(o =>
    {
        o.UseLocalServer();
        o.UseAspNetCore();
    });

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = Schemes.SmartScheme;
        options.DefaultChallengeScheme = Schemes.SmartScheme;
    })
    .AddPolicyScheme(Schemes.SmartScheme, "Cookie or Bearer", o =>
    {
        o.ForwardDefaultSelector = ctx =>
        {
            var auth = ctx.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrWhiteSpace(auth) && auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            }

            return IdentityConstants.ApplicationScheme;
        };
    })
    .AddCookie(IdentityConstants.ApplicationScheme, o =>
    {
        o.LoginPath = "/account/login";
        o.LogoutPath = "/account/logout";
        o.Cookie.HttpOnly = true;
        o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        o.Cookie.SameSite = SameSiteMode.Lax;
        o.ExpireTimeSpan = TimeSpan.FromDays(14);
        o.SlidingExpiration = true;
        // SecurityStampValidator re-validates the stamp every ValidationInterval (5 min,
        // configured above). Disable, password change, role change, or admin force-rotate
        // all trigger a fresh stamp via UpdateSecurityStampAsync, kicking the session out
        // within the interval. No need for a per-request IsEnabled lookup.
        o.Events.OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync;
        o.Events.OnRedirectToLogin = ctx =>
        {
            if (IsApiRequest(ctx.Request))
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }

            ctx.Response.Redirect(ctx.RedirectUri);

            return Task.CompletedTask;
        };
        o.Events.OnRedirectToAccessDenied = ctx =>
        {
            if (IsApiRequest(ctx.Request))
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }
            ctx.Response.Redirect(ctx.RedirectUri);
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy(ScopePolicy.PolicyName, ScopePolicy.ConfigurePolicy);
});

builder.Services.AddAntiforgery(o =>
{
    // Storage cookie keeps its default name (.AspNetCore.Antiforgery.*); the
    // XSRF-TOKEN cookie set by the middleware below is what the SPA reads to
    // populate the X-XSRF-TOKEN header on state-changing requests.
    o.HeaderName = "X-XSRF-TOKEN";
});

builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddScoped<IAuthorizationHandler, UserTypeHandler>();

builder.Services.AddSingleton<IAuthorizationPolicyProvider, ApplicationPolicyProvider>();
builder.Services.AddSingleton<IRolePermissionCache, RolePermissionCache>();
builder.Services.AddSingleton<IUserStatusCache, UserStatusCache>();

builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IRoleAccessor, SqlRoleAccessor>();
builder.Services.AddScoped<IRolePermissionProvider, SqlRolePermissionProvider>();

builder.Services.AddRepositories();
builder.Services.AddMyPortalServices();

builder.Services.AddHostedService<TimetableRunWorker>();

builder.Services.AddScoped<CookieAntiforgeryFilter>();

builder.Services.AddControllers(o =>
{
    o.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));

    o.ModelBinderProviders.Insert(0, new Base64JsonBinderProvider<FilterOptions>());
    o.ModelBinderProviders.Insert(0, new Base64JsonBinderProvider<SortOptions>());

    o.Filters.Add<CookieAntiforgeryFilter>();
});

builder.Services.AddRazorPages();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MyPortal Web API",
        Version = "v1"
    });

    c.OperationFilter<AuthorizeCheckOperationFilter>();
    c.OperationFilter<ResponseTypesOperationFilter>();
    c.DocumentFilter<OAuth2AbsoluteUrlDocumentFilter>();

    // Pull XML doc comments into the OpenAPI document so Scalar can render
    // controller / endpoint summaries and remarks.
    foreach (var xml in Directory.GetFiles(AppContext.BaseDirectory, "MyPortal.*.xml"))
    {
        c.IncludeXmlComments(xml, includeControllerXmlComments: true);
    }

    // OAuth2 Authorization Code + PKCE against the OpenIddict server. URLs here are
    // placeholders; OAuth2AbsoluteUrlDocumentFilter rewrites them to absolute URLs
    // based on the current request host at the moment swagger.json is served, so the
    // same code works for any dev port AND production without hardcoding.
    c.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("https://placeholder.invalid/connect/authorize"),
                TokenUrl = new Uri("https://placeholder.invalid/connect/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "openid",         "OpenID" },
                    { "profile",        "Profile" },
                    { "email",          "Email" },
                    { "offline_access", "Refresh tokens" },
                    { "api",            "MyPortal API access" }
                }
            }
        }
    });
});

builder.Services.AddTransient<ExceptionMiddleware>();

var app = builder.Build();

await AuthSeeder.RunAsync(app.Services);

app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Swashbuckle continues to generate the OpenAPI document at /swagger/v1/swagger.json;
    // Scalar consumes it and renders the modern UI at /scalar/v1.
    app.UseSwagger();
    // MyPortal-branded Scalar. Indigo accent matches the SPA's PrimeNG theme;
    // Scalar already loads Inter as its default font so no font override needed.
    const string scalarBrandingCss = """
        :root, .light-mode {
            /* indigo-600 / indigo-50 — matches the SPA's primary scale */
            --scalar-color-accent: #4f46e5;
            --scalar-background-accent: #eef2ff;
        }
        .dark-mode {
            /* indigo-400 / indigo-950 — readable on dark backgrounds */
            --scalar-color-accent: #818cf8;
            --scalar-background-accent: #1e1b4b;
        }
        """;

    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("MyPortal API")
            .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json")
            .WithFavicon("/favicon.ico")
            .WithCustomCss(scalarBrandingCss)
            .AddPreferredSecuritySchemes(["OAuth2"])
            .AddAuthorizationCodeFlow("OAuth2", flow =>
            {
                flow.ClientId = "myportal-client";
                flow.SelectedScopes = new[] { "openid", "profile", "email", "offline_access", "api" };
                flow.Pkce = Pkce.Sha256;
            });
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (ctx, next) =>
{
    // Mint the XSRF-TOKEN cookie on SPA-shell navigations when it's missing — this
    // covers fresh / deep-link loads. Identity transitions (login/logout) re-mint
    // explicitly so the token is rebound to the new principal; see Login.cshtml.cs.
    // Skipping when the cookie is already present keeps Set-Cookie off every
    // index.html response so CDN/proxy/browser caching isn't defeated.
    // Gated on Accept: text/html so asset requests don't pay the cost.
    if (HttpMethods.IsGet(ctx.Request.Method) &&
        !ctx.Request.Path.StartsWithSegments("/api") &&
        !ctx.Request.Path.StartsWithSegments("/connect") &&
        !ctx.Request.Cookies.ContainsKey("XSRF-TOKEN") &&
        ctx.Request.Headers["Accept"].ToString()
            .Contains("text/html", StringComparison.OrdinalIgnoreCase))
    {
        ctx.RefreshXsrfCookie();
    }
    await next();
});

app.MapControllers();
app.MapRazorPages();

app.MapFallbackToFile("index.html");

app.Run();