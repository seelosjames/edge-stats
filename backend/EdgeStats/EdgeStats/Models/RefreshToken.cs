namespace EdgeStats.Models
{
	public class RefreshToken
	{
		public int Id { get; set; }
		public string Token { get; set; } = null!;
		public string UserId { get; set; } = null!;
		public DateTime Expires { get; set; }
		public bool IsRevoked { get; set; } = false;

		public virtual ApplicationUser User { get; set; } = null!;
	}
}
