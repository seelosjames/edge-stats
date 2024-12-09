def decimal_to_percentage(decimal_odds):
    """
    Convert decimal odds to implied probability as a percentage.

    Args:
        decimal_odds (float): The decimal odds.

    Returns:
        float: Implied probability as a percentage.
    """
    if decimal_odds <= 0:
        raise ValueError("Decimal odds must be greater than 0")
    
    implied_probability = (1 / decimal_odds) * 100
    return round(implied_probability, 2)


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
