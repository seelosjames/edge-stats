namespace Scraper.Utils
{
	internal class OddsConverter
	{
		public static double DecimalToPercentage(double decimalOdds)
		{
			if (decimalOdds <= 1)
				throw new ArgumentException("Decimal odds must be greater than 1.");

			return Math.Round(100 / decimalOdds, 4);
		}
	}
}
