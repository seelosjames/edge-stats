using EdgeStats.Dtos;
using EdgeStats.Models;
using EdgeStats.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("authentication")]
public class AuthController : ControllerBase
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly IConfiguration _configuration;
	private readonly TokenService _tokenService;

	public AuthController(
		UserManager<ApplicationUser> userManager,
		SignInManager<ApplicationUser> signInManager,
		IConfiguration configuration,
		TokenService tokenService)
	{
		_userManager = userManager;
		_signInManager = signInManager;
		_configuration = configuration;
		_tokenService = tokenService;
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
	{
		var user = await _userManager.FindByNameAsync(model.Username);
		if (user == null)
			return Unauthorized("Invalid username or password");

		var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
		if (!result.Succeeded)
			return Unauthorized("Invalid username or password");

		var accessToken = _tokenService.GenerateJwtToken(user);
		var refreshToken = await _tokenService.GenerateNewRefreshToken(user.Id);

		return Ok(new
		{
			access = accessToken,
			refresh = refreshToken
		});
	}

	[HttpPost("token")]
	public async Task<IActionResult> Register([FromBody] RegisterRequestDto model)
	{
		var userExists = await _userManager.FindByNameAsync(model.Username);
		if (userExists != null)
			return BadRequest("User already exists!");

		var user = new ApplicationUser
		{
			UserName = model.Username,
			Email = model.Email
		};

		var result = await _userManager.CreateAsync(user, model.Password);

		if (!result.Succeeded)
			return BadRequest(result.Errors);

		return Ok("User created successfully!");
	}

	[HttpPost("token/refresh")]
	public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request)
	{
		Console.WriteLine($"Refresh token received: {request?.RefreshToken ?? "null"}");

		if (string.IsNullOrWhiteSpace(request?.RefreshToken))
			return BadRequest("Missing refresh token");

		var isValid = await _tokenService.ValidateRefreshToken(request.RefreshToken);
		if (!isValid)
			return Unauthorized("Invalid refresh token");

		var user = await _tokenService.GetUserFromRefreshToken(request.RefreshToken);
		if (user == null)
			return Unauthorized("Invalid refresh token");

		var newAccessToken = _tokenService.GenerateJwtToken(user);
		var newRefreshToken = await _tokenService.GenerateNewRefreshToken(user.Id);

		return Ok(new
		{
			access = newAccessToken,
			refresh = newRefreshToken
		});
	}
}
