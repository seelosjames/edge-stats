using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scraper
{
    internal class Mappings
    {
        public static readonly Dictionary<string, List<string>> PropMapping = new()
        {
            { "Double-Double", new() { "Double+Double", "DoubleDouble", "DOUBLE DOUBLE" } },
            { "Triple-Double", new() { "Triple+Double", "TripleDouble", "TRIPLE DOUBLE" } },
            { "Points-Rebounds-Assists", new() { "Pts+Rebs+Asts", "PointsReboundsAssist", "POINTS AND ASSISTS AND REBOUNDS", "Points Rebounds Assists" } },
            { "Three-Point-FG", new() { "ThreePointFieldGoals", "THREE POINTS MADE", "3 Point FG" } },
            { "Points", new() { "PTS", "POINTS" } },
            { "Assists", new() { "AST", "ASSISTS" } },
            { "Turnovers", new() { "TO", "TURNOVERS" } },
            { "Steals", new() { "STL", "STEALS" } },
            { "Blocks", new() { "BLK", "BLOCKS" } },
            { "Rebounds", new() { "REB", "REBOUNDS" } },
            { "Points-Assists", new() { "Pts+Asts", "POINTS AND ASSISTS" } },
            { "Points-Rebounds", new() { "Pts+Rebs", "POINTS AND REBOUNDS" } },
            { "Assists-Rebounds", new() { "Asts+Rebs", "ASSISTS AND REBOUNDS" } },
            { "Steals-Blocks", new() { "Stls+Blks", "STEALS AND BLOCKS" } },
            { "First-Quarter-Points", new() { "1Q-Pts", "FIRST QUARTER POINTS" } },
            { "First-Quarter-Assists", new() { "1Q-Asts", "FIRST QUARTER ASSISTS" } },
            { "First-Quarter-Rebounds", new() { "1Q-Rebs", "FIRST QUARTER REBOUNDS" } },
            { "Shots", new() { "SHOTS", "ShotsOnGoal", "Shots On Goal" } },
            { "Goals", new() { "GOALS" } },
            { "Blocked-Shots", new() { "BLOCKED SHOTS" } },
            { "Goaltender-Goals-Against", new() { "GOALTENDER GOALS AGAINST" } },
            { "Goaltender-Saves", new() { "GOALTENDER SAVES" } },
            { "Goaltender-Shutouts", new() { "GOALTENDER SHUTOUTS" } },
            { "Powerplay-Points", new() { "POWERPLAY POINTS" } },
            { "Goaltender-Plus-Minus", new() { "GOALTENDER PLUS MINUS" } },
            { "First-Goal-Scorer", new() { "FIRST GOAL SCORER" } },
            { "Last-Goal-Scorer", new() { "LAST GOAL SCORER" } },
        };

        public static readonly Dictionary<string, List<string>> TeamMapping = new()
        {
            { "Atlanta Hawks", new() },
            { "Boston Celtics", new() },
            { "Brooklyn Nets", new() },
            { "Charlotte Hornets", new() },
            { "Chicago Bulls", new() },
            { "Cleveland Cavaliers", new() },
            { "Dallas Mavericks", new() },
            { "Denver Nuggets", new() },
            { "Detroit Pistons", new() },
            { "Golden State Warriors", new() { "" } },
            { "Houston Rockets", new() },
            { "Indiana Pacers", new() },
            { "Los Angeles Clippers", new() { "" } },
            { "Los Angeles Lakers", new() { "L.A. Lakers" } },
            { "Memphis Grizzlies", new() },
            { "Miami Heat", new() },
            { "Milwaukee Bucks", new() },
            { "Minnesota Timberwolves", new() },
            { "New Orleans Pelicans", new() { "" } },
            { "New York Knicks", new() { "" } },
            { "Oklahoma City Thunder", new() },
            { "Orlando Magic", new() },
            { "Philadelphia 76ers", new() },
            { "Phoenix Suns", new() },
            { "Portland Trail Blazers", new() },
            { "Sacramento Kings", new() },
            { "San Antonio Spurs", new() { "" } },
            { "Toronto Raptors", new() },
            { "Utah Jazz", new() },
            { "Washington Wizards", new() },
        };
    }
}
