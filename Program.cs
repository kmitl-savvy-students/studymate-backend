using Microsoft.EntityFrameworkCore;
using studymate_backend.Contexts;
using studymate_backend.Services;
using studymate_backend.Services.FrontendUrl;
using studymate_backend.Services.GoogleOAuthUrl;

var builder = WebApplication.CreateBuilder(args);

// Add PostgreSQL database context using NpgSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
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

// Resolve IFrontendUrlService using DI, and configure CORS dynamically
var frontendUrlService = app.Services.GetRequiredService<IFrontendUrlService>();
var frontendUrl = frontendUrlService.GetFrontendUrl();

app.UseCors(policyBuilder =>
{
    if (frontendUrl != null)
        policyBuilder.WithOrigins(frontendUrl)
            .AllowAnyHeader()
            .AllowAnyMethod();
});

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();