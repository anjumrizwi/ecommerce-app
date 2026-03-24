using Azure.Identity;
using Ecommerce.API.Extensions;
using Ecommerce.API.Security;
using Ecommerce.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/ecommerce-api-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Ecommerce.API")
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog as logging provider
    builder.Host.UseSerilog();

    // Configure Azure Key Vault
    var keyVaultUri = builder.Configuration["KeyVault:VaultUri"];
    if (!string.IsNullOrWhiteSpace(keyVaultUri))
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUri),
            new DefaultAzureCredential());
        Log.Information($"Azure Key Vault configured: {keyVaultUri}");
    }
    else if (!builder.Environment.IsDevelopment())
    {
        throw new InvalidOperationException(
            "KeyVault:VaultUri must be configured in non-Development environments.");
    }

    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    
    // Configure Swagger/OpenAPI
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "Ecommerce API",
            Version = "v1",
            Description = "API for managing products and orders"
        });

        // Include XML comments if available
        var xmlFile = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".xml";
        var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (System.IO.File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    // Add health checks
    builder.Services.AddHealthChecks();

    // Add CORS
    builder.Services.AddCors(options =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:3000", "http://localhost:5173"];

        options.AddPolicy("AllowAll", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });

    // Add Infrastructure services
    builder.Services.AddInfrastructure(builder.Configuration);

    // Configure JWT authentication
    var jwtSection = builder.Configuration.GetSection("Jwt");
    var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = jwtSection["Issuer"],
                ValidAudience = jwtSection["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
    });

    builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Ecommerce API v1");
            options.RoutePrefix = string.Empty; // Serve Swagger UI at root
        });
    }

    // Use custom exception handling middleware
    app.UseCustomExceptionHandler();

    // Use CORS
    app.UseCors("AllowAll");

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    // Map health checks
    app.MapHealthChecks("/health");

    // Map controllers
    app.MapControllers();

    // Enable minimal APIs
    app.MapGet("/", () => Results.Ok("Ecommerce API is running"))
        .WithName("GetRoot")
        .Produces(StatusCodes.Status200OK);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

