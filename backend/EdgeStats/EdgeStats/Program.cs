using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using EdgeStats;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<EdgeStatsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();


var app = builder.Build();

app.MapGet("/lines", async (EdgeStatsDbContext db) =>
    await db.Lines.ToListAsync());

app.MapGet("/line/{sportsbookId}", (int sportsbookId) =>
    $"Requesting lines from {sportsbookId}");

app.MapGet("/", () => "Backend is running!");


app.Run();
