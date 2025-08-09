using System.Security.Cryptography;
using System.Text;

namespace Scraper.Utils
{
	internal class IdHelper
	{

		public static Guid GenerateGameUuid(string team1, string team2, DateTime actualTime)
		{
			DateTime normalizedTime = Normalizer.NormalizeTime(actualTime);
			string keyString = $"{team1}_{team2}_{normalizedTime:yyyyMMdd_HHmm}";
			return CreateUuidFromString(keyString);
		}

		public static Guid GeneratePropUuid(string propName, string propType, int gameId)
		{
			string keyString = $"{propName}_{propType}_{gameId}";
			return CreateUuidFromString(keyString);
		}

		public static Guid GenerateLineUuid(Guid propUuid, string description, string sportsbook)
		{
			string keyString = $"{propUuid}_{description}_{sportsbook}";
			return CreateUuidFromString(keyString);
		}

		private static Guid CreateUuidFromString(string input)
		{
			using (var md5 = MD5.Create())
			{
				byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
				return new Guid(hash);
			}
		}
	}
}
