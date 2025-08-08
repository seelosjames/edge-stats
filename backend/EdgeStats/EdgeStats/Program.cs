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

app.Run();