using Microsoft.EntityFrameworkCore;
using studymate_backend.Contexts;
using studymate_backend.Services;
using studymate_backend.Services.FrontendUrl;
using studymate_backend.Services.GoogleOAuthUrl;

var builder = WebApplication.CreateBuilder(args);

// add environment variables
var server = Environment.GetEnvironmentVariable("DB_SERVER");
var database = Environment.GetEnvironmentVariable("DB_NAME");
var userId = Environment.GetEnvironmentVariable("DB_USER"); 
var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

var connectionString = $"Host={server};Database={database};Username={userId};Password={password};";

// Add PostgreSQL database context using NpgSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
);

// Frontend URL Service
builder.Services.Configure<FrontendConfig>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddSingleton<IFrontendUrlService, FrontendUrlService>();

// Google OAuth Endpoint URL Service
builder.Services.Configure<GoogleOAuthConfig>(builder.Configuration.GetSection("GoogleOAuth"));
builder.Services.AddSingleton<IGoogleOAuthUrlService, GoogleOAuthUrlService>();

// Add services to the container
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<UserTokenService>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS but configure it inside app after DI services are available
builder.Services.AddCors();

var app = builder.Build();

app.UseCors(policyBuilder =>
{
    policyBuilder.WithOrigins(
            "http://localhost:4200",
            "https://preprod.savvystudymate.com")
        .AllowAnyHeader()
        .AllowAnyMethod();
});

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

await app.RunAsync();