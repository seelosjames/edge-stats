import json
from functions.betting import *


def find_value_bets(
    json_data, target_sportsbook="Fliff", reference_sportsbook="Pinnacle", edge=0.04
):
    """
    Find value bets for a specific sportsbook compared to a reference sportsbook.

    Args:
        json_data (dict): The JSON data containing sportsbook odds.
        target_sportsbook (str): The sportsbook for which to find value bets.
        reference_sportsbook (str): The reference sportsbook for fair odds.
        edge (float): The minimum percentage edge to consider a value bet (default 5%).

    Returns:
        list: A list of value bets with details.
    """
    value_bets = []

    for game, game_data in json_data.items():
        for player_prop, prop_data in game_data.items():
            if target_sportsbook in prop_data and reference_sportsbook in prop_data:
                if (
                    prop_data[target_sportsbook]["over_number"]
                    == prop_data[reference_sportsbook]["over_number"]
                ):
                    target_over = prop_data[target_sportsbook]["over_odds"]
                    target_under = prop_data[target_sportsbook]["under_odds"]
                    reference_over = prop_data[reference_sportsbook]["over_odds"]
                    reference_under = prop_data[reference_sportsbook]["under_odds"]

                    if (
                        american_to_percentage(reference_over) / 100
                        - american_to_percentage(target_over) / 100
                        > edge
                    ):
                        value_bets.append(f"Take the over for {player_prop}")
                    elif (
                        american_to_percentage(reference_under) / 100
                        - american_to_percentage(target_under) / 100
                        > edge
                    ):
                        value_bets.append(f"Take the under for {player_prop}")

    return value_bets


# Example usage with JSON data
def main():
    # Load your JSON data here
    with open("data.json", "r") as file:
        json_data = json.load(file)

    value_bets = find_value_bets(json_data)

    for bet in value_bets:
        print(bet)


if __name__ == "__main__":
    main()
