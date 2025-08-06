namespace Scraper.Utils
{
	internal class Normalizer
	{
		public static string NormalizeFromMapping(string input, Dictionary<string, List<string>> mapping)
		{
			input = input.Trim();

			foreach (var kvp in mapping)
			{
				if (kvp.Value.Any(alias => alias == input))
				{
					return kvp.Key;
				}
			}

			return input;
		}

		public static DateTime NormalizeTime(DateTime actualTime, int roundToMinutes = 5)
		{
			int roundedMinutes = actualTime.Minute / roundToMinutes * roundToMinutes;
			return new DateTime(
				actualTime.Year,
				actualTime.Month,
				actualTime.Day,
				actualTime.Hour,
				roundedMinutes,
				0
			);
		}
	}
}
