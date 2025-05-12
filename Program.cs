using System;
using System.Text;
using System.Linq;
using System.Threading;
using AspNetCoreRateLimit;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using HealthChecks.UI.Client;
using CustomerService.API.Middlewares;
using CustomerService.API.Services.Implementations;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using CustomerService.API.Repositories.Implementations;
using CustomerService.API.Repositories.Interfaces;
using HealthChecks.ApplicationStatus.DependencyInjection;
using HealthChecks.SqlServer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using CustomerService.API.Delegations;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using CustomerService.API.Pipelines.Implementations;
using CustomerService.API.Pipelines.Interfaces;
using CustomerService.API.Data.Context;
using MapsterMapper;
using System.Reflection;
using CustomerService.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

// --------------------- Serilog ---------------------
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Host.UseSerilog();

// --------------------- DbContext ---------------------
builder.Services.AddDbContext<CustomerSupportContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --------------------- CORS ---------------------
var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(o => o.AddPolicy("CorsPolicy", p =>
    p.WithOrigins(origins!)
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()
));

// --------------------- HttpClients ---------------------
// 1) Vincula tu sección "Gemini" a GeminiOptions
builder.Services.Configure<GeminiOptions>(
    builder.Configuration.GetSection("Gemini"));

// 2) Registra el DelegatingHandler que inyecta el header
builder.Services.AddTransient<GeminiDelegatingHandler>();

// 3) Configura el HttpClient para IGeminiClient
//    - BaseAddress = host de Google
//    - El handler añadirá el header x-goog-api-key
builder.Services.AddHttpClient<IGeminiClient, GeminiClient>(client =>
{
    client.BaseAddress = new Uri("https://generativelanguage.googleapis.com");
})
.AddHttpMessageHandler<GeminiDelegatingHandler>();


// --------------------- Jwt Settings ---------------------
var jwtSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSection);
var keyBytes = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

// --------------------- Repositories & UoW ---------------------
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IAuthTokenRepository, AuthTokenRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IAttachmentRepository, AttachmentRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// --------------------- Application Services ---------------------
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();
builder.Services.AddScoped<IMessagePipeline, MessagePipeline>();

// --------------------- Mapster ---------------------
var mapsterConfig = TypeAdapterConfig.GlobalSettings;
mapsterConfig.Scan(Assembly.GetExecutingAssembly());
builder.Services.AddSingleton(mapsterConfig);
builder.Services.AddScoped<IMapper, ServiceMapper>();

builder.Services.AddSingleton<IPresenceService, PresenceService>();
builder.Services.AddHostedService<PresenceBackgroundService>();
builder.Services.AddSingleton<ISignalRNotifyService, SignalRNotifyService>();

// --------------------- Response Caching & Compression ---------------------
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();
builder.Services.AddResponseCompression();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();

// --------------------- Rate Limiting ---------------------
builder.Services.AddOptions();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// --------------------- Health Checks & UI ---------------------
builder.Services.AddHealthChecks()
    .AddApplicationStatus("api_status", tags: new[] { "api" })
    .AddSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "sql",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "db", "sql", "sqlserver" }
    );
builder.Services.AddHealthChecksUI().AddInMemoryStorage();

// --------------------- SignalR ---------------------
builder.Services.AddSignalR();

// --------------------- Authentication & Authorization ---------------------
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt =>
{
    opt.RequireHttpsMetadata = true;
    opt.SaveToken = true;
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSection["Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        RoleClaimType = ClaimTypes.Role,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("AdminPolicy", p => p.RequireRole("Admin"));
    opts.AddPolicy("AgentPolicy", p => p.RequireRole("Support"));
    opts.AddPolicy("ClientPolicy", p => p.RequireRole("Customer"));
});

// --------------------- Swagger ---------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CustomerSupport.API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Usa: Bearer {token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// --------------------- Controllers ---------------------
builder.Services.AddControllers();


var app = builder.Build();

// --------------------- Middleware Pipeline ---------------------
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseResponseCaching();
app.UseIpRateLimiting();
app.UseErrorHandling();

app.UseRouting();

app.UseCors("CorsPolicy");


app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1"));

app.UseAuthentication();
app.UseAuthorization();

// Liveness
app.MapGet("/liveness", () => Results.Ok(new { status = "alive" }))
   .AllowAnonymous();

// Health endpoints
app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecksUI();

// SignalR hub
app.MapHub<ChatHub>("/chatHub");
app.MapHub<NotificationsHub>("/hubs/notifications");

// API controllers
app.MapControllers();

app.Run();