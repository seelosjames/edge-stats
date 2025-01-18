import hashlib

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
    "Los Angeles Clippers": [''],
    "Los Angeles Lakers": ["L.A. Lakers"],
    "Memphis Grizzlies": [],
    "Miami Heat": [],
    "Milwaukee Bucks": [],
    "Minnesota Timberwolves": [],
    "New Orleans Pelicans": [''],
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


def normalize_team_name(name):
    for standard_name, aliases in team_name_mapping.items():
        if name in aliases:
            return standard_name
    return name  # Return original name if no match is found


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
def generate_id(team_1, team_2, date_time_str):
    combined_string = f"{team_1}_{team_2}_{date_time_str}"
    return hashlib.md5(combined_string.encode()).hexdigest()
