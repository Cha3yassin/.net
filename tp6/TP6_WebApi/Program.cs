using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TP6_WebApi.Models;
using TP6_WebApi.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(o =>
    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Inscription de la classe de contexte
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection")));

// Inscription de la classe repository
builder.Services.AddScoped<ICompteRepository, CompteRepository>();

// Swagger (documentation et test de l'API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
