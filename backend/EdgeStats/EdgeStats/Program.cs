using EdgeStats;
using EdgeStats.Models;
using EdgeStats.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

// =============================
// 1️⃣ CORS
// =============================
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

// =============================
// 2️⃣ Controllers + JSON
// =============================
builder.Services.AddControllers()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.WriteIndented = true;
	});

// =============================
// 3️⃣ PostgreSQL + Identity
// =============================
builder.Services.AddDbContext<EdgeStatsDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
	.AddEntityFrameworkStores<EdgeStatsDbContext>()
	.AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
	options.Password.RequiredLength = 10;
	options.Password.RequireDigit = true;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireUppercase = true;
	options.Password.RequireLowercase = true;
});

// =============================
// 4️⃣ JWT Authentication
// =============================
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
	// Provide a dummy key to avoid crash during migrations
	jwtKey = "ThisIsADevelopmentJwtKeyForDesignTimeOnly!";
}
builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
	};
});

builder.Services.AddScoped<TokenService>();

// =============================
// 5️⃣ Authorization
// =============================
builder.Services.AddAuthorization();

// =============================
// 6️⃣ Developer DB exception filter
// =============================
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// =============================
// 7️⃣ Custom Services
// =============================
builder.Services.AddScoped<ScraperService>();

var app = builder.Build();

// =============================
// 8️⃣ Middleware
// =============================
app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication(); // ⬅ must come before UseAuthorization
app.UseAuthorization();

app.MapControllers();

// =============================
// 9️⃣ OPTIONAL: Reset DB
// =============================
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<EdgeStatsDbContext>();
//    db.Database.EnsureDeleted();
//    db.Database.EnsureCreated();
//}


// =============================
// 10 OPTIONAL: Prepopulate Database
// =============================
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<EdgeStatsDbContext>();

//    // Seed Leagues
//    if (!db.Leagues.Any())
//    {
//        db.Leagues.AddRange(
//			new League { LeagueCode = "NFL", LeagueName = "National Football League", Sport = "Football"},
//			new League{LeagueCode = "NCAAF", LeagueName = "National Collegiate Athletic Association Football", Sport = "Football"}
//		);
//        db.SaveChanges();
//    }

//    // Seed Sportsbooks
//    if (!db.Sportsbooks.Any())
//    {
//        db.Sportsbooks.AddRange(
//            new Sportsbook { SportsbookName = "Pinnacle", SportsbookUrl = "https://www.pinnacle.com/en/" },
//            new Sportsbook { SportsbookName = "Fliff", SportsbookUrl = "https://sports.getfliff.com/sports?channelId=-333" },
//            new Sportsbook { SportsbookName = "Underdog", SportsbookUrl = "https://underdogfantasy.com/pick-em/higher-lower/all/home" },
//            new Sportsbook { SportsbookName = "PrizePicks", SportsbookUrl = "https://app.prizepicks.com/board" },
//            new Sportsbook { SportsbookName = "Betr", SportsbookUrl = "https://picks.betr.app/picks/home/lobby" }
//        );
//        db.SaveChanges();
//    }

//    // Seed Sportsbook URLs
//    if (!db.SportsbookUrls.Any())
//    {
//        var nfl = db.Leagues.First(l => l.LeagueCode == "NFL");
//        var ncaaf = db.Leagues.First(l => l.LeagueCode == "NCAAF");

//        var pinnacle = db.Sportsbooks.First(s => s.SportsbookName == "Pinnacle");
//        var fliff = db.Sportsbooks.First(s => s.SportsbookName == "Fliff");
//        var underdog = db.Sportsbooks.First(s => s.SportsbookName == "Underdog");
//        var prizepicks = db.Sportsbooks.First(s => s.SportsbookName == "PrizePicks");
//        var betr = db.Sportsbooks.First(s => s.SportsbookName == "Betr");

//        db.SportsbookUrls.AddRange(
//            new SportsbookUrl
//            {
//                SportsbookId = pinnacle.SportsbookId,
//                LeagueId = nfl.LeagueId,
//                Url = "https://www.pinnacle.com/en/football/nfl/matchups/#all"
//            },
//            new SportsbookUrl
//            {
//                SportsbookId = pinnacle.SportsbookId,
//                LeagueId = ncaaf.LeagueId,
//                Url = "https://www.pinnacle.com/en/football/ncaa/matchups/#all"
//            },
//            new SportsbookUrl
//            {
//                SportsbookId = fliff.SportsbookId,
//                LeagueId = nfl.LeagueId,
//                Url = "https://sports.getfliff.com/sports?channelId=451"
//            },
//            new SportsbookUrl
//            {
//                SportsbookId = fliff.SportsbookId,
//                LeagueId = ncaaf.LeagueId,
//                Url = "https://sports.getfliff.com/sports?channelId=452"
//            },
//            new SportsbookUrl
//            {
//                SportsbookId = underdog.SportsbookId,
//                LeagueId = nfl.LeagueId,
//                Url = "https://underdogfantasy.com/pick-em/higher-lower/all/nfl"
//            },
//            new SportsbookUrl
//            {
//                SportsbookId = underdog.SportsbookId,
//                LeagueId = ncaaf.LeagueId,
//                Url = "https://underdogfantasy.com/pick-em/higher-lower/all/cfb"
//            },
//            new SportsbookUrl
//            {
//                SportsbookId = prizepicks.SportsbookId,
//                LeagueId = nfl.LeagueId,
//                Url = "https://app.prizepicks.com/board"
//            },
//            new SportsbookUrl
//            {
//                SportsbookId = prizepicks.SportsbookId,
//                LeagueId = ncaaf.LeagueId,
//                Url = "https://app.prizepicks.com/board"
//            },
//            new SportsbookUrl
//            {
//                SportsbookId = betr.SportsbookId,
//                LeagueId = nfl.LeagueId,
//                Url = "https://picks.betr.app/picks/home/NFL"
//            },
//            new SportsbookUrl
//            {
//                SportsbookId = betr.SportsbookId,
//                LeagueId = ncaaf.LeagueId,
//                Url = "https://picks.betr.app/picks/home/CFB"
//            }
//        );
//        db.SaveChanges();
//    }

//}

app.Run();