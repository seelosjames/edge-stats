using EdgeStats;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddControllers(); // Needed if using app.MapControllers()
builder.Services.AddDbContext<EdgeStatsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

// Middleware
app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

// Minimal APIs
app.MapGet("/lines", async (EdgeStatsDbContext db) => 
    await db.Lines
        .Include(l => l.Prop)
        .Include(l => l.Sportsbook)
        .Include(l => l.Prop.Game)
        .Include(l => l.Prop.Game.League)
        .Include(l => l.Prop.Game.Team1)
        .Include(l => l.Prop.Game.Team2)
        .ToListAsync());

app.MapGet("/props", async (EdgeStatsDbContext db) => await db.Props.ToListAsync());
app.MapGet("/", () => "Backend is running!");

app.MapGet("/filters/sports", async (EdgeStatsDbContext db) =>
    await db.Leagues
        .Select(l => l.LeagueName)
        .Distinct()
        .ToListAsync()
);

app.MapGet("/filters/sportsbooks", async (EdgeStatsDbContext db) =>
    await db.Sportsbooks
        .Select(s => s.SportsbookName)
        .Distinct()
        .ToListAsync()
);

app.MapGet("/filters/prop-types", async (EdgeStatsDbContext db) =>
    await db.Props
        .Select(p => p.PropType)
        .Distinct()
        .ToListAsync()
);

app.Run();
