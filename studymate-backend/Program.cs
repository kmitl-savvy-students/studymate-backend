using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using studymate_backend.Libraries.Core;
using studymate_backend.Libraries.Helper;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(option => { option.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower; });

builder.Services.AddAuthentication("StudyMateToken")
    .AddScheme<AuthenticationSchemeOptions, SdmTokenHandler>("StudyMateToken", _ => { });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors();

var app = builder.Build();

app.UseMiddleware<SdmGlobalMiddlewareCacheManagement>();

app.UseCors(policyBuilder =>
{
    policyBuilder.WithOrigins(
        "http://localhost:4200",
        "https://preprod.savvystudymate.com",
        "https://kmitl.savvystudymate.com",
        "https://ce67-27.cloud.ce.kmitl.ac.th",
        "https://gcp.savvystudymate.com"
    ).AllowAnyHeader().AllowAnyMethod();
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();