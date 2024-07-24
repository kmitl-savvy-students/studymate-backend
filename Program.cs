using Microsoft.EntityFrameworkCore;
using StudyMate.Data;
using StudyMate.Services;

var builder = WebApplication.CreateBuilder(args);

// My Services
builder.Services.AddScoped<UserService>();

builder.Services.AddControllers();

// My Database Contexts
builder.Services.AddDbContext<UserManagementContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
