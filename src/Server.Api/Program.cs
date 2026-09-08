using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Server.Api.Data;
using Server.Api.Data.Entities;
using Server.Api.Options;
using Server.Api.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseWindowsService(options =>
{
    options.ServiceName = "GearLatheFeederPreXLN.Server";
});

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler(_ => { });
builder.Services.AddOpenApi();

builder.Services.Configure<FileStorageOptions>(
    builder.Configuration.GetSection(FileStorageOptions.SectionName));
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<SeedDataOptions>(
    builder.Configuration.GetSection(SeedDataOptions.SectionName));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebClient", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (origins is { Length: > 0 })
        {
            policy.WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod();
            return;
        }

        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>() ?? new JwtOptions();
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IMachineService, MachineService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUploadedFileService, UploadedFileService>();
builder.Services.AddScoped<IModelProfileService, ModelProfileService>();
builder.Services.AddScoped<IPasswordHasher<UserEntity>, PasswordHasher<UserEntity>>();
builder.Services.AddScoped<DatabaseInitializer>();
builder.Services.AddScoped<IShelfDeclarationService, ShelfDeclarationService>();
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddSingleton<IFileStorageService>(serviceProvider =>
{
    var environment = serviceProvider.GetRequiredService<IHostEnvironment>();
    var options = serviceProvider.GetRequiredService<IOptions<FileStorageOptions>>().Value;

    return new LocalFileStorageService(environment.ContentRootPath, options);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/openapi/{documentName}.json");
}

app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        if (ShouldDisableCaching(context))
        {
            ApplyNoCacheHeaders(context.Response.Headers);
        }

        return Task.CompletedTask;
    });

    await next();
});

app.UseExceptionHandler();
app.UseCors("WebClient");
app.UseAuthentication();
app.UseAuthorization();

var indexFilePath = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "index.html");
if (File.Exists(indexFilePath))
{
    app.UseDefaultFiles();
    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = staticFileContext =>
        {
            if (ShouldDisableCaching(staticFileContext.Context))
            {
                ApplyNoCacheHeaders(staticFileContext.Context.Response.Headers);
            }
        }
    });
}

app.MapControllers();

if (File.Exists(indexFilePath))
{
    app.MapFallbackToFile("index.html");
}

app.Run();

static bool ShouldDisableCaching(HttpContext context)
{
    if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
    {
        return false;
    }

    var requestPath = context.Request.Path.Value ?? string.Empty;
    if (requestPath.Equals("/config.json", StringComparison.OrdinalIgnoreCase))
    {
        return true;
    }

    var responseContentType = context.Response.ContentType;
    if (!string.IsNullOrWhiteSpace(responseContentType) &&
        responseContentType.StartsWith("text/html", StringComparison.OrdinalIgnoreCase))
    {
        return true;
    }

    return requestPath.Equals("/", StringComparison.OrdinalIgnoreCase) ||
           requestPath.Equals("/index.html", StringComparison.OrdinalIgnoreCase);
}

static void ApplyNoCacheHeaders(IHeaderDictionary headers)
{
    headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
    headers["Pragma"] = "no-cache";
    headers["Expires"] = "0";
}

public partial class Program;
