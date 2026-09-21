using Microsoft.EntityFrameworkCore;
using Willovate.Store.Api.Data;
using Willovate.Store.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

builder.Services.AddControllers();

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
        ?? throw new InvalidOperationException(
            "Connection string 'Store' is not configured.");

    builder.Services.AddDbContext<StoreDbContext>(options =>
        options.UseNpgsql(connectionString));
}

builder.Services.AddScoped<IProductService, ProductService>();


// =====================================================
// CORS CONFIGURATION
// =====================================================

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>();

// If Cors:AllowedOrigins is not configured,
// allow both Vite development ports.
if (allowedOrigins == null || allowedOrigins.Length == 0)
{
    allowedOrigins =
    [
        "http://localhost:5173",
        "http://localhost:5174"
    ];
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("StoreUi", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// =====================================================
// BUILD APPLICATION
// =====================================================

var app = builder.Build();

app.UseExceptionHandler();


// =====================================================
// SWAGGER
// =====================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// =====================================================
// HTTPS
// =====================================================

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}


// =====================================================
// CORS
// =====================================================

app.UseCors("StoreUi");


// =====================================================
// STATIC FILES
// =====================================================

app.UseStaticFiles();


// =====================================================
// CONTROLLERS
// =====================================================

app.MapControllers();


// =====================================================
// HEALTH CHECK
// =====================================================

app.MapGet(
    "/api/health",
    async (
        StoreDbContext dbContext,
        CancellationToken cancellationToken) =>
    {
        var databaseAvailable =
            await dbContext.Database.CanConnectAsync(cancellationToken);

        return Results.Ok(new
        {
            status = databaseAvailable ? "healthy" : "degraded",
            service = "willovate-store-api",
            timestamp = DateTimeOffset.UtcNow
        });
    })
    .WithName("Health")
    .WithTags("Health");


// =====================================================
// UPLOADS DIRECTORY
// =====================================================

EnsureUploadsDirectory(app.Environment);


// =====================================================
// DATABASE INITIALIZATION
// =====================================================

await InitialiseDatabaseAsync(app);


// =====================================================
// RUN APPLICATION
// =====================================================

await app.RunAsync();


// =====================================================
// DATABASE INITIALIZATION METHOD
// =====================================================

static async Task InitialiseDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();

    var dbContext =
        scope.ServiceProvider.GetRequiredService<StoreDbContext>();

    if (dbContext.Database.IsRelational())
    {
        await dbContext.Database.MigrateAsync();
    }
    else
    {
        await dbContext.Database.EnsureCreatedAsync();
    }

    await SeedData.InitialiseAsync(dbContext);
}


// =====================================================
// UPLOAD DIRECTORY METHOD
// =====================================================

static void EnsureUploadsDirectory(IWebHostEnvironment env)
{
    var webRoot =
        env.WebRootPath ??
        Path.Combine(env.ContentRootPath, "wwwroot");

    var uploadsPath =
        Path.Combine(
            webRoot,
            "uploads",
            "products");

    Directory.CreateDirectory(uploadsPath);
}


public partial class Program;