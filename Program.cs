using System.Text;
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
using CustomerService.API.Data.context;
using HealthChecks.ApplicationStatus.DependencyInjection;
using HealthChecks.SqlServer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using CustomerService.API.Delegations;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using CustomerService.API.Pipelines.Implementations;
using CustomerService.API.Pipelines.Interfaces;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSignalR();

//builder.Services.AddHttpClient<IWhatsAppService, WhatsAppService>();
//builder.Services.AddScoped<IMessagePipeline, MessagePipeline>();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddDbContext<CustomerSupportContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(o => o.AddPolicy("CorsPolicy", p =>
    p.WithOrigins(origins!)
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()
));


builder.Services.Configure<GeminiClient>(builder.Configuration.GetSection(("Gemini")));
builder.Services.AddTransient<GeminiDelegatingHandler>();
builder.Services.AddHttpClient<GeminiClient>(
    (serviceProvider, httpClient) =>
    {
        var geminiOptions = serviceProvider.GetRequiredService<IOptions<GeminiOptions>>().Value;

        httpClient.BaseAddress = new Uri(geminiOptions.Url);
    }).AddHttpMessageHandler<GeminiDelegatingHandler>();

var jwtSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSection);


builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IAuthTokenRepository, AuthTokenRepository>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGeminiClient, GeminiClient>();
builder.Services.AddScoped<IMessagePipeline, MessagePipeline>();
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();

builder.Services.AddMapster();

builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();
builder.Services.AddResponseCompression();

builder.Services.AddOptions();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();


builder.Services.AddHealthChecksUI().AddInMemoryStorage();

var jwt = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

builder.Services
    .AddAuthentication(options =>
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
            IssuerSigningKey = new SymmetricSecurityKey(key),
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("AdminPolicy", p => p.RequireRole("db_admin"));
    opts.AddPolicy("AgentPolicy", p => p.RequireRole("db_agent"));
    opts.AddPolicy("ClientPolicy", p => p.RequireRole("db_client"));
});

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
        Description = "Enter 'Bearer {token}'"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services
    .AddHealthChecks()
    .AddApplicationStatus(name: "api_status", tags: new[] { "api" })
    .AddSqlServer(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "sql",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "db", "sql", "sqlserver" });

builder.Services
    .AddHealthChecksUI()
    .AddInMemoryStorage();

builder.Services.AddControllers();

var app = builder.Build();

app.MapGet("/", () => "¡Hola, mundo!");
app.MapHealthChecks("/healthz", new HealthCheckOptions()
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecksUI();

app.UseSerilogRequestLogging();
app.UseCors("CorsPolicy");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1"));
}

app.UseHttpsRedirection();

app.UseResponseCompression();
app.UseResponseCaching();
app.UseIpRateLimiting();
app.UseErrorHandling();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/liveness", () => Results.Ok(new { status = "alive" }))
   .AllowAnonymous();

app.MapHub<CustomerService.API.Utils.ChatHub>("/chatHub");


app.MapControllers();

app.Run();