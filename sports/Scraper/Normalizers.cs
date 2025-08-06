using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scraper
{
    internal class Normalizers
    {
        public static string NormalizeFromMapping(string input, Dictionary<string, List<string>> mapping)
        {
            input = input.Trim().ToLowerInvariant();

            foreach (var kvp in mapping)
            {
                if (kvp.Value.Any(alias => alias.Trim().ToLowerInvariant() == input))
                {
                    return kvp.Key;
                }
            }

            return input;
        }
    }
}
