using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Infrastructure.Data;
using SmartTelehealth.Application;
using SmartTelehealth.Infrastructure;
using Serilog;
using Serilog.Events;
using SmartTelehealth.API;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.API.Hubs;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using SmartTelehealth.API.Filters;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .WriteTo.Console()
    .WriteTo.File("logs/audit-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14, restrictedToMinimumLevel: LogEventLevel.Information)
    .CreateLogger();

builder.Host.UseSerilog();

// Remove or comment out the default logging builder
// builder.Logging.ClearProviders();
// builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<JsonModelActionFilter>();
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.IgnoreObsoleteProperties();
    options.CustomSchemaIds(type => type.FullName);
    options.SupportNonNullableReferenceTypes();
    options.SchemaFilter<IgnoreNavigationPropertiesSchemaFilter>();
});

// Database Configuration - Only register if not in test environment
if (!builder.Environment.IsEnvironment("Test"))
{
    DatabaseRegistration.RegisterDatabaseProvider(builder.Services, builder.Configuration, builder.Environment);
}

// Identity Configuration
builder.Services.AddIdentity<User, Role>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? "default-secret-key-for-development-only");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Application Services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);

// SignalR
builder.Services.AddSignalR();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure model validation error handling
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        var response = new
        {
            success = false,
            message = "Validation failed.",
            errors,
            statusCode = 400
        };
        return new BadRequestObjectResult(response);
    };
});

var app = builder.Build();

// Add global exception handling middleware
app.UseMiddleware<SmartTelehealth.API.GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map SignalR hubs
app.MapHub<ChatHub>("/chatHub");
app.MapHub<VideoCallHub>("/videoCallHub");

// Ensure database is created and seeded (skip in test)
if (!app.Environment.IsEnvironment("Test"))
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        
        context.Database.EnsureCreated();
        // Seed the database
        await SeedData.SeedAsync(context, userManager, roleManager);
    }
}

app.Run();

// Make Program class public for testing
namespace SmartTelehealth.API
{
    public partial class Program { }
}

// Add this class to filter out navigation properties from Swagger
public class IgnoreNavigationPropertiesSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema?.Properties == null || context.Type == null)
            return;

        var navigationProps = context.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p =>
                // Heuristic: navigation properties are usually virtual and reference classes (not primitives/structs)
                p.GetGetMethod()?.IsVirtual == true &&
                !p.PropertyType.IsValueType &&
                p.PropertyType != typeof(string));

        foreach (var prop in navigationProps)
        {
            if (schema.Properties.ContainsKey(prop.Name))
            {
                schema.Properties.Remove(prop.Name);
            }
        }
    }
} 