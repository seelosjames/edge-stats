odds_data = [
    {
        "game": "Lakers vs Jazz",
        "props": [
            {
                "player": "LeBron James",
                "prop": "Points",
                "line": 27.5,
                "odds": {
                    "Pinnacle": {"over": -110, "under": -110},
                    "Fliff": {"over": -115, "under": -105},
                },
            },
            {
                "player": "Anthony Davis",
                "prop": "Rebounds",
                "line": 12.5,
                "odds": {
                    "Pinnacle": {"over": -120, "under": -110},
                    "Fliff": {"over": -125, "under": -105},
                },
            },
        ],
    },
    {
        "game": "Warriors vs Nuggets",
        "props": [
            {
                "player": "Stephen Curry",
                "prop": "Points",
                "line": 30.5,
                "odds": {
                    "Pinnacle": {"over": -125, "under": 105},
                    "Fliff": {"over": -130, "under": 100},
                },
            },
            {
                "player": "Nikola Jokic",
                "prop": "Points",
                "line": 25.5,
                "odds": {
                    "Pinnacle": {"over": -140, "under": 110},
                    "Fliff": {"over": -105, "under": 115},
                },
            },
        ],
    },
    {
        "game": "Bucks vs Mavericks",
        "props": [
            {
                "player": "Giannis Antetokounmpo",
                "prop": "Points",
                "line": 28.5,
                "odds": {
                    "Pinnacle": {"over": -115, "under": -105},
                    "Fliff": {"over": -120, "under": 100},
                },
            },
            {
                "player": "Luka Doncic",
                "prop": "Points",
                "line": 29.5,
                "odds": {
                    "Pinnacle": {"over": -110, "under": -110},
                    "Fliff": {"over": -105, "under": -115},
                },
            },
        ],
    },
]


def american_to_percentage(odds):
    if odds >= 100:
        return 100 / (odds + 100)
    elif odds <= -100:
        return abs(odds) / (abs(odds) + 100)
    else:
        print("Invalid Number, cannot be between -100 and 100")
        return None


# Function to identify value bets
def find_value_bets(data):
    value_bets = []

    # Loop through each game
    for game in data:
        game_name = game["game"]

        # Loop through each player prop
        for prop in game["props"]:
            player = prop["player"]
            prop_type = prop["prop"]
            line = prop["line"]
            odds = prop["odds"]

            # Compare odds across sportsbooks
            for sportsbook, values in odds.items():
                print(sportsbook, values)
                # for bet_type, odd in values.items():

            print()

                    
    return value_bets

find_value_bets(odds_data)
