using Microsoft.EntityFrameworkCore;
using Npgsql;
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

if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<StoreDbContext>(options =>
        options.UseInMemoryDatabase("willovate-store-tests"));
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("Store")
        ?? throw new InvalidOperationException("Connection string 'Store' is not configured.");

    var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
    dataSourceBuilder.EnableDynamicJson();
    var dataSource = dataSourceBuilder.Build();

    builder.Services.AddDbContext<StoreDbContext>(options =>
        options.UseNpgsql(dataSource));
}
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IWebsiteService, WebsiteService>();
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddScoped<IPageService, PageService>();
builder.Services.AddScoped<IPageElementService, PageElementService>();
builder.Services.AddScoped<IAiService, AiService>();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:5173"];

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
