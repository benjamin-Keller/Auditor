using Auditor.Data;
using Auditor.Services;
using Auditor.Source;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// Optional logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddTransient<SuperHeroService>();
builder.Services.SetupAudit();

var config = builder.Configuration;
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(config.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();