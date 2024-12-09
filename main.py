import json

# Load the data from your JSON file
with open('data.json', 'r') as file:
    data = json.load(file)

def calculate_implied_probability(odds):
    """Calculate the implied probability from American odds."""
    odds = float(odds)
    if odds < 0:  # Negative odds
        return -odds / (-odds + 100)
    else:  # Positive odds
        return 100 / (odds + 100)

def find_value_bets(sportsbook=None):
    """Find value bets where odds are at least 5% in favor."""
    value_bets = []
    
    for game, props in data.items():
        for prop_name, odds_data in props.items():
            if sportsbook:
                # If a sportsbook is selected, compare only for that one
                if sportsbook in odds_data:
                    selected_odds = odds_data[sportsbook]
                    for other_sportsbook, other_odds_data in odds_data.items():
                        if other_sportsbook != sportsbook:
                            # Compare with other sportsbooks
                            selected_prob = calculate_implied_probability(selected_odds['over_odds'])
                            other_prob = calculate_implied_probability(other_odds_data['over_odds'])
                            
                            # Check if the odds are at least 5% better for the selected sportsbook
                            if (selected_prob - other_prob) >= 0.05:
                                value_bets.append({
                                    'game': game,
                                    'prop': prop_name,
                                    'selected_sportsbook': sportsbook,
                                    'selected_odds': selected_odds['over_odds'],
                                    'other_sportsbook': other_sportsbook,
                                    'other_odds': other_odds_data['over_odds'],
                                    'selected_prob': selected_prob,
                                    'other_prob': other_prob,
                                })
            else:
                # Compare across all sportsbooks
                for sportsbook_name, odds in odds_data.items():
                    for other_sportsbook, other_odds in odds_data.items():
                        if sportsbook_name != other_sportsbook:
                            selected_prob = calculate_implied_probability(odds['over_odds'])
                            other_prob = calculate_implied_probability(other_odds['over_odds'])
                            
                            # Check if the odds are at least 5% better for the selected sportsbook
                            if (selected_prob - other_prob) >= 0.05:
                                value_bets.append({
                                    'game': game,
                                    'prop': prop_name,
                                    'selected_sportsbook': sportsbook_name,
                                    'selected_odds': odds['over_odds'],
                                    'other_sportsbook': other_sportsbook,
                                    'other_odds': other_odds['over_odds'],
                                    'selected_prob': selected_prob,
                                    'other_prob': other_prob,
                                })
    
    return value_bets

# Example usage
sportsbook = input("Enter sportsbook name (or press enter to search across all): ").strip()
value_bets = find_value_bets(sportsbook if sportsbook else None)

# Print out the value bets
if value_bets:
    for bet in value_bets:
        print(f"Game: {bet['game']}, Prop: {bet['prop']}")
        print(f"Selected sportsbook: {bet['selected_sportsbook']} - Odds: {bet['selected_odds']} (Implied probability: {bet['selected_prob'] * 100:.2f}%)")
        print(f"Other sportsbook: {bet['other_sportsbook']} - Odds: {bet['other_odds']} (Implied probability: {bet['other_prob'] * 100:.2f}%)")
        print("-" * 80)
else:
    print("No value bets found.")
