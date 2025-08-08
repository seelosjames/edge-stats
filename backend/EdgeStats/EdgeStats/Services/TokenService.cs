using EdgeStats.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EdgeStats.Services
{
	public class TokenService
	{

		private readonly EdgeStatsDbContext _context;
		private readonly IConfiguration _configuration;

		public TokenService(EdgeStatsDbContext context, IConfiguration configuration)
		{
			_context = context;
			_configuration = configuration;
		}

		public string GenerateJwtToken(ApplicationUser user)
		{
			var jwtSettings = _configuration.GetSection("Jwt");

			var claims = new[]
			{
			new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? ""),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			new Claim(ClaimTypes.NameIdentifier, user.Id)
		};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: jwtSettings["Issuer"],
				audience: jwtSettings["Audience"],
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpireMinutes"])),
				signingCredentials: creds);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		public async Task<string> GenerateNewRefreshToken(string userId)
		{
			var refreshToken = new RefreshToken
			{
				Token = Guid.NewGuid().ToString(),
				UserId = userId,
				Expires = DateTime.UtcNow.AddDays(7),
				IsRevoked = false
			};

			// Optional: revoke old tokens
			var existingTokens = _context.RefreshTokens.Where(rt => rt.UserId == userId && !rt.IsRevoked);
			_context.RefreshTokens.RemoveRange(existingTokens);

			await _context.RefreshTokens.AddAsync(refreshToken);
			await _context.SaveChangesAsync();

			return refreshToken.Token;
		}

		public async Task<bool> ValidateRefreshToken(string token)
		{
			var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsRevoked);
			if (refreshToken == null || refreshToken.Expires < DateTime.UtcNow)
				return false;

			return true;
		}

		public async Task<ApplicationUser?> GetUserFromRefreshToken(string token)
		{
			var refreshToken = await _context.RefreshTokens.Include(rt => rt.User)
				.FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsRevoked);

			if (refreshToken == null || refreshToken.Expires < DateTime.UtcNow)
				return null;

			return refreshToken.User;
		}
	}
}
