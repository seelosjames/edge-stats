import hashlib
import re

team_name_mapping = {
    "Atlanta Hawks": [],
    "Boston Celtics": [],
    "Brooklyn Nets": [],
    "Charlotte Hornets": [],
    "Chicago Bulls": [],
    "Cleveland Cavaliers": [],
    "Dallas Mavericks": [],
    "Denver Nuggets": [],
    "Detroit Pistons": [],
    "Golden State Warriors": [""],
    "Houston Rockets": [],
    "Indiana Pacers": [],
    "Los Angeles Clippers": [""],
    "Los Angeles Lakers": ["L.A. Lakers"],
    "Memphis Grizzlies": [],
    "Miami Heat": [],
    "Milwaukee Bucks": [],
    "Minnesota Timberwolves": [],
    "New Orleans Pelicans": [""],
    "New York Knicks": [""],
    "Oklahoma City Thunder": [],
    "Orlando Magic": [],
    "Philadelphia 76ers": [],
    "Phoenix Suns": [],
    "Portland Trail Blazers": [],
    "Sacramento Kings": [],
    "San Antonio Spurs": [""],
    "Toronto Raptors": [],
    "Utah Jazz": [],
    "Washington Wizards": [],
}

prop_name_mapping = {
    "Spread": ["Handicap", "Point Spread", 'Alternative Point Spread', 'POINT SPREAD', 'ALTERNATIVE POINT SPREAD', 'Puck Line', 'PUCK LINE', 'Alternative Puck Line', 'ALTERNATIVE PUCK LINE'],
    "Total": ['TOTAL SCORE', 'Alternative Total Score', 'TOTAL', 'ALTERNATIVE TOTAL SCORE'],
    'Money Line': ['MONEYLINE', 'ALTERNATIVE MONEYLINE'],
    'Regulation Money Line': ['3-Way Moneyline', '3-WAY MONEYLINE'],
    'Home Team Score': ['HOME TEAM SCORE'],
    'Away Team Score': ['AWAY TEAM SCORE'],
    'Both Teams To Score': ['BOTH TEAMS TO SCORE']
}

prop_type_mapping = {
    "Game": ["OT Included"],
    "First Half": ['1ST HALF', "1st Half"],
    "Second Half": ['2ND HALF', "2nd Half"],
    'First Quarter': ['1ST QUARTER', "1st Quarter"],
    'Second Quarter': ['2ND QUARTER', "2nd Quarter"],
    'Third Quarter': ['3RD QUARTER', "3rd Quarter"],
    'Fourth Quarter': ['4TH QUARTER', "4th Quarter"],
    'First Period': ['1ST PERIOD', '1st Period'],
    'Second Period': ['2ND PERIOD', '2nd Period'],
    'Third Period': ['3RD PERIOD', '3rd Period'],
}

prop_mapping = {
    "Double-Double": ["Double+Double", "DoubleDouble", 'DOUBLE DOUBLE'],
    "Triple-Double": ["Triple+Double", "TripleDouble", 'TRIPLE DOUBLE'],
    "Points-Rebounds-Assists": ["Pts+Rebs+Asts", "PointsReboundsAssist", 'POINTS AND ASSISTS AND REBOUNDS', 'Points Rebounds Assists'],
    "Three-Point-FG": ["ThreePointFieldGoals", 'THREE POINTS MADE', '3 Point FG'],
    "Points": ["PTS", 'POINTS'],
    "Assists": ["AST", 'ASSISTS'],
    "Turnovers": ["TO", 'TURNOVERS'],
    "Steals": ["STL", 'STEALS'],
    "Blocks": ["BLK", 'BLOCKS'],
    "Rebounds": ["REB", 'REBOUNDS'],
    "Points-Assists": ["Pts+Asts", 'POINTS AND ASSISTS'],
    "Points-Rebounds": ["Pts+Rebs", 'POINTS AND REBOUNDS'],
    "Assists-Rebounds": ["Asts+Rebs", 'ASSISTS AND REBOUNDS'],
    "Steals-Blocks": ["Stls+Blks", 'STEALS AND BLOCKS'],
    "First-Quarter-Points": ["1Q-Pts", 'FIRST QUARTER POINTS'],
    "First-Quarter-Assists": ["1Q-Asts", 'FIRST QUARTER ASSISTS'],
    "First-Quarter-Rebounds": ["1Q-Rebs", "FIRST QUARTER REBOUNDS"],
    "Shots":['SHOTS', 'ShotsOnGoal', 'Shots On Goal'],
    "Goals":['GOALS'],
    "Blocked-Shots":['BLOCKED SHOTS'],
    "Goaltender-Goals-Against":['GOALTENDER GOALS AGAINST'],
    "Goaltender-Saves":['GOALTENDER SAVES'],
    "Goaltender-Shutouts":['GOALTENDER SHUTOUTS'],
    "Powerplay-Points":['POWERPLAY POINTS'],
    "Goaltender-Plus-Minus":['GOALTENDER PLUS MINUS'],
    "First-Goal-Scorer":['FIRST GOAL SCORER'],
    "Last-Goal-Scorer":['LAST GOAL SCORER'],
}


def normalize_prop_name(name):
    for standard_name, aliases in prop_name_mapping.items():
        if name in aliases:
            return standard_name
    return name  # Return original name if no match is found


def normalize_prop(type):
    for standard_name, aliases in prop_mapping.items():
        if type in aliases:
            return standard_name
    return type  # Return original name if no match is found

def normalize_prop_type(type):
    for standard_name, aliases in prop_type_mapping.items():
        if type in aliases:
            return standard_name
    return type  # Return original name if no match is found


def normalize_team_name(name):
    for standard_name, aliases in team_name_mapping.items():
        if name in aliases:
            return standard_name
    return name  # Return original name if no match is found


def convert_number_to_float(input_string):
    # Use regex to check if the string ends with a number (integer or float)
    match = re.search(r"(-?\d+(\.\d+)?)$", input_string)
    if match:
        # Convert the matched number to float and reconstruct the string
        number = float(match.group(0))
        return input_string[:match.start()] + str(number)
    else:
        # Return the string as is if no number is found at the end
        return input_string


# Function to update or append game data
def update_or_add_game(data, new_game, book):
    new_game_link = new_game["links"][book]
    # Look for existing game by ID
    for existing_game in data:
        if existing_game["id"] == new_game["id"]:
            existing_game["links"][book] = new_game_link
            return  # Exit after updating the existing game
    # If no match, add the new game
    data.append(new_game)
    print(f"Added new game with ID: {new_game['id']}")


# Function to create a unique ID
def generate_game_uuid(team_1, team_2, date_time_str):
    combined_string = f"{team_1}_{team_2}_{date_time_str}"
    return hashlib.md5(combined_string.encode()).hexdigest()


def generate_prop_uuid(prop_name, prop_type, game_id):
    unique_string = f"{prop_name}_{prop_type}_{game_id}"
    return hashlib.md5(unique_string.encode()).hexdigest()


def generate_line_uuid(prop_uuid, description, odd, sportsbook):
    unique_string = f"{prop_uuid}_{description}_{sportsbook}"
    return hashlib.md5(unique_string.encode()).hexdigest()
