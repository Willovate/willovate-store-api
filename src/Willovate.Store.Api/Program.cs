using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Text;
using Willovate.Store.Api.Configuration;
using Willovate.Store.Api.Data;
using Willovate.Store.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Willovate Store API",
        Version = "v1",
        Description = "The HTTP API for the Willovate Store product catalog."
    });
});

bool usePostgres = false;
var connectionString = builder.Configuration.GetConnectionString("Store");
if (!string.IsNullOrEmpty(connectionString) && !builder.Environment.IsEnvironment("Testing"))
{
    try
    {
        using var testConn = new NpgsqlConnection(connectionString);
        testConn.Open();
        usePostgres = true;
    }
    catch
    {
        Console.WriteLine("[Database] PostgreSQL server not reachable at localhost:5432. Gracefully falling back to in-memory database with complete seeding.");
    }
}

if (usePostgres)
{
    var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
    dataSourceBuilder.EnableDynamicJson();
    var dataSource = dataSourceBuilder.Build();

    builder.Services.AddDbContext<StoreDbContext>(options =>
        options.UseNpgsql(dataSource)
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
}
else
{
    builder.Services.AddDbContext<StoreDbContext>(options =>
        options.UseInMemoryDatabase(builder.Environment.IsEnvironment("Testing") ? "willovate-store-tests" : "willovate-store-dev"));
}

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IWebsiteService, WebsiteService>();
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddScoped<IPageService, PageService>();
builder.Services.AddScoped<IPageElementService, PageElementService>();
builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ICustomerRegistrationService, CustomerRegistrationService>();
builder.Services.AddScoped<ICustomerLoginService, CustomerLoginService>();
builder.Services.AddScoped<ICustomerGoogleAuthService, CustomerGoogleAuthService>();
builder.Services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
builder.Services.AddScoped<ICustomerMicrosoftAuthService, CustomerMicrosoftAuthService>();
builder.Services.AddScoped<IMicrosoftTokenValidator, MicrosoftTokenValidator>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Configure options from appsettings
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<GoogleOptions>(builder.Configuration.GetSection("Google"));
builder.Services.Configure<MicrosoftOptions>(builder.Configuration.GetSection("Microsoft"));

// Configure JWT authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "willovate-store-api-dev-jwt-secret-key-32chars-minimum!";

var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
    ValidateIssuer = true,
    ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "willovate-store-api",
    ValidateAudience = true,
    ValidAudience = builder.Configuration["Jwt:Audience"] ?? "willovate-store-client",
    ValidateLifetime = true,
    ClockSkew = TimeSpan.Zero
};

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = tokenValidationParameters;
});

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:5173", "http://localhost:5174", "http://127.0.0.1:5173", "http://127.0.0.1:5174"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("StoreUi", policy =>
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseCors("StoreUi");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapGet("/api/health", async (StoreDbContext dbContext, CancellationToken cancellationToken) =>
{
    var databaseAvailable = await dbContext.Database.CanConnectAsync(cancellationToken);

    return Results.Ok(new
    {
        status = databaseAvailable ? "healthy" : "degraded",
        service = "willovate-store-api",
        timestamp = DateTimeOffset.UtcNow
    });
})
.WithName("Health")
.WithTags("Health");

await InitialiseDatabaseAsync(app);
await app.RunAsync();

static async Task InitialiseDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<StoreDbContext>();

    if (dbContext.Database.IsRelational())
    {
        await dbContext.Database.MigrateAsync();
    }
    else
    {
        await dbContext.Database.EnsureCreatedAsync();
    }

    await SeedData.InitialiseAsync(dbContext);
    await TemplateSeeder.SeedAsync(dbContext);
    await CleanupStrayHomePageElementsAsync(dbContext);
}

/// <summary>
/// Removes PageElement rows that were erroneously seeded onto the Home page with
/// element types that are exclusively managed by the frontend's synthetic-element
/// system (hero, heading, text, button, etc.).  These rows show up as duplicate
/// "Main Hero Section" / "Hero Heading" entries in the sidebar Template group.
/// Safe to run on every startup – it is a no-op once the rows are gone.
/// </summary>
static async Task CleanupStrayHomePageElementsAsync(StoreDbContext dbContext)
{
    // Types that the home page should never have as real DB rows
    var syntheticTypes = new HashSet<string>
    {
        "hero", "heading", "text", "button", "announcement", "nav",
        "featured-title", "prod-grid", "coll-list", "img-text",
        "testimonials", "newsletter", "email-signup", "footer",
        "policies", "section-title", "banner_slider"
    };

    // Join via PageId to avoid nullable navigation property dereference (CS8602)
    var homePageIds = await dbContext.Pages
        .Where(p => p.IsHomePage)
        .Select(p => p.Id)
        .ToListAsync();

    var homePageElements = await dbContext.PageElements
        .Where(pe => homePageIds.Contains(pe.PageId) && syntheticTypes.Contains(pe.ElementType))
        .ToListAsync();

    if (homePageElements.Count > 0)
    {
        dbContext.PageElements.RemoveRange(homePageElements);
        await dbContext.SaveChangesAsync();
    }
}

public partial class Program;
