import hashlib

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
