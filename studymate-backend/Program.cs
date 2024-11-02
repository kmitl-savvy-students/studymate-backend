using System.Text.Json;
using studymate_backend.Libraries.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(option => { option.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower; });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors();

var app = builder.Build();

app.UseCors(policyBuilder =>
{
    policyBuilder.WithOrigins(
        "http://localhost:4200",
        "https://preprod.savvystudymate.com",
        "https://test.savvystudymate.com",
        "https://savvystudymate.com"
    ).AllowAnyHeader().AllowAnyMethod();
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

app.Lifetime.ApplicationStopping.Register(() => SdmDataSource.Dispose());

await app.RunAsync();