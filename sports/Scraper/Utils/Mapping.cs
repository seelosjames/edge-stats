namespace Scraper.Utils
{
	internal class Mapping
	{
		public static readonly Dictionary<string, List<string>> LineMapping = new()
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

		public static readonly Dictionary<string, List<string>> PropTypeMapping = new()
		{
			{ "Game", new() { "OT Included" } },
			{ "First Half", new() { "1ST HALF", "1st Half" } },
			{ "Second Half", new() { "2ND HALF", "2nd Half" } },
			{ "First Quarter", new() { "1ST QUARTER", "1st Quarter" } },
			{ "Second Quarter", new() { "2ND QUARTER", "2nd Quarter" } },
			{ "Third Quarter", new() { "3RD QUARTER", "3rd Quarter" } },
			{ "Fourth Quarter", new() { "4TH QUARTER", "4th Quarter" } },
			{ "First Period", new() { "1ST PERIOD", "1st Period" } },
			{ "Second Period", new() { "2ND PERIOD", "2nd Period" } },
			{ "Third Period", new() { "3RD PERIOD", "3rd Period" } }
		};

		public static readonly Dictionary<string, List<string>> TeamMapping = new()
		{
            //NBA Teams
            { "Atlanta Hawks", new() },
			{ "Boston Celtics", new() },
			{ "Brooklyn Nets", new() },
			{ "Charlotte Hornets", new() },
			{ "Chicago Bulls", new() },
			{ "Cleveland Cavaliers", new() },
			{ "Dallas Mavericks", new() },
			{ "Denver Nuggets", new() },
			{ "Detroit Pistons", new() },
			{ "Golden State Warriors", new() },
			{ "Houston Rockets", new() },
			{ "Indiana Pacers", new() },
			{ "Los Angeles Clippers", new()  },
			{ "Los Angeles Lakers", new() { "L.A. Lakers" } },
			{ "Memphis Grizzlies", new() },
			{ "Miami Heat", new() },
			{ "Milwaukee Bucks", new() },
			{ "Minnesota Timberwolves", new() },
			{ "New Orleans Pelicans", new()  },
			{ "New York Knicks", new()  },
			{ "Oklahoma City Thunder", new() },
			{ "Orlando Magic", new() },
			{ "Philadelphia 76ers", new() },
			{ "Phoenix Suns", new() },
			{ "Portland Trail Blazers", new() },
			{ "Sacramento Kings", new() },
			{ "San Antonio Spurs", new()  },
			{ "Toronto Raptors", new() },
			{ "Utah Jazz", new() },
			{ "Washington Wizards", new() },

            // NFL Teams
			{ "Arizona Cardinals", new() },
			{ "Atlanta Falcons", new() },
			{ "Baltimore Ravens", new() },
			{ "Buffalo Bills", new() },
			{ "Carolina Panthers", new() },
			{ "Chicago Bears", new() },
			{ "Cincinnati Bengals", new() },
			{ "Cleveland Browns", new() },
			{ "Dallas Cowboys", new() },
			{ "Denver Broncos", new() },
			{ "Detroit Lions", new() },
			{ "Green Bay Packers", new() },
			{ "Houston Texans", new() },
			{ "Indianapolis Colts", new() },
			{ "Jacksonville Jaguars", new() },
			{ "Kansas City Chiefs", new() },
			{ "Las Vegas Raiders", new() },
			{ "Los Angeles Chargers", new() },
			{ "Los Angeles Rams", new() },
			{ "Miami Dolphins", new() },
			{ "Minnesota Vikings", new() },
			{ "New England Patriots", new() },
			{ "New Orleans Saints", new() },
			{ "New York Giants", new() },
			{ "New York Jets", new() },
			{ "Philadelphia Eagles", new() },
			{ "Pittsburgh Steelers", new() },
			{ "San Francisco 49ers", new() },
			{ "Seattle Seahawks", new() },
			{ "Tampa Bay Buccaneers", new() },
			{ "Tennessee Titans", new() },
			{ "Washington Commanders", new() }
		};

		public static readonly Dictionary<string, List<string>> PropNameMapping = new()
		{
			{ "Spread", new()
				{
					"Handicap", "Point Spread", "Alternative Point Spread", "POINT SPREAD",
					"ALTERNATIVE POINT SPREAD", "Puck Line", "PUCK LINE", "Alternative Puck Line",
					"ALTERNATIVE PUCK LINE"
				}
			},
			{ "Total", new()
				{
					"TOTAL SCORE", "Alternative Total Score", "TOTAL", "ALTERNATIVE TOTAL SCORE"
				}
			},
			{ "Money Line", new()
				{
					"MONEYLINE", "ALTERNATIVE MONEYLINE"
				}
			},
			{ "Regulation Money Line", new()
				{
					"3-Way Moneyline", "3-WAY MONEYLINE"
				}
			},
			{ "Home Team Score", new()
				{
					"HOME TEAM SCORE"
				}
			},
			{ "Away Team Score", new()
				{
					"AWAY TEAM SCORE"
				}
			},
			{ "Both Teams To Score", new()
				{
					"BOTH TEAMS TO SCORE"
				}
			}
		};
	}
}
