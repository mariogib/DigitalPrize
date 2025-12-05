using DigitalPrizes.Api.Extensions;
using DigitalPrizes.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with OAuth2 support
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DigitalPrizes API",
        Version = "v1",
        Description = "API for managing digital prizes",
    });

    // Add JWT Bearer authentication to Swagger
    var authSettings = builder.Configuration.GetSection("Authentication");
    var authority = authSettings["Authority"] ?? "https://worldplayauth.ngrok.app/";

    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"{authority}connect/authorize"),
                TokenUrl = new Uri($"{authority}connect/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "openid", "OpenID Connect" },
                    { "profile", "User profile" },
                    { "email", "User email" },
                    { "roles", "User roles" },
                    { "offline_access", "Offline access" },
                },
            },
        },
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2",
                },
            },
            Array.Empty<string>()
        },
    });
});

// Configure JWT Bearer Authentication
var authSettings2 = builder.Configuration.GetSection("Authentication");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authSettings2["Authority"];
        options.Audience = authSettings2["Audience"];
        options.RequireHttpsMetadata = authSettings2.GetValue<bool>("RequireHttpsMetadata", false);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = authSettings2.GetValue<bool>("ValidateIssuer", true),
            ValidateAudience = authSettings2.GetValue<bool>("ValidateAudience", true),
            ValidateLifetime = authSettings2.GetValue<bool>("ValidateLifetime", true),
            ValidIssuer = authSettings2["Authority"],
            ValidAudience = authSettings2["Audience"],
            NameClaimType = "name",
            RoleClaimType = "role",
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("Authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                var userId = context.Principal?.FindFirst("sub")?.Value;
                logger.LogInformation("Token validated for user: {UserId}", userId);
                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Administrator", "System Administrator"));

    options.AddPolicy("Authenticated", policy =>
        policy.RequireAuthenticatedUser());
});

// Add HttpContextAccessor for accessing user claims
builder.Services.AddHttpContextAccessor();

// Add application services
builder.Services.AddApplicationServices(builder.Configuration);

// Configure CORS
var corsSettings = builder.Configuration.GetSection("Cors");
var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "DigitalPrizes API v1");
        options.OAuthClientId(authSettings2["ClientId"]);
        options.OAuthUsePkce();
    });
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
