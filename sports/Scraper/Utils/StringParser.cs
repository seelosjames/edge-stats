using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Scraper.Utils
{
	internal class StringParser
	{
		public static Dictionary<string, string> PinnacleParsePropString(string input)
		{
			// Match format: Player (Prop)
			var match = Regex.Match(input, @"^(.*?)\s*\((.*?)\)$");
			if (match.Success)
			{
				string player = match.Groups[1].Value.Trim();
				string prop = Normalizer.NormalizeFromMapping(match.Groups[2].Value.Trim(), Mapping.LineMapping);

				return new Dictionary<string, string>
				{
					{ "prop_name", $"{player} {prop}" },
					{ "prop_type", "Player Prop" }
				};
			}

			// Match format: Prop Name - Prop Type
			match = Regex.Match(input, @"^(.*?)\s*[-–]\s*(.*?)$");
			if (match.Success)
			{
				string propName = Normalizer.NormalizeFromMapping(match.Groups[1].Value.Trim(), Mapping.PropNameMapping);
				string propType = match.Groups[2].Value.Trim();

				return new Dictionary<string, string>
				{
					{ "prop_name", propName },
					{ "prop_type", propType }
				};
			}

			// Unsupported format
			return new Dictionary<string, string>
			{
				{ "message", "Prop not yet supported" }
			};
		}

		public static string FormatTrailingNumber(string input)
		{
			var match = Regex.Match(input, @"(-?\d+(\.\d+)?)$");
			if (match.Success)
			{
				double number = double.Parse(match.Value);
				return input.Substring(0, match.Index) + number.ToString();
			}

			return input;
		}
	}
}
