import argparse
import logging
import json
from collections import defaultdict
from database.db import *
from scraping.pinnacle import *
from scraping.fliff import *


league_urls = {
    "nba": {
        "pinnacle": "https://www.pinnacle.com/en/basketball/nba/matchups/#all",
        "fliff": "https://sports.getfliff.com/channels?channelId=461",
    },
    "nhl": {
        "pinnacle": "https://www.pinnacle.com/en/hockey/nhl/matchups/#period:0",
        "fliff": "https://sports.getfliff.com/channels?channelId=481",
    },
    "nfl": {
        "pinnacle": "https://www.pinnacle.com/en/football/nfl/matchups/#all",
        "fliff": "https://sports.getfliff.com/channels?channelId=451",
    },
    "ncaaf": {
        "pinnacle": "https://www.pinnacle.com/en/football/ncaa/matchups/#all",
        "fliff": "https://sports.getfliff.com/channels?channelId=452",
    },
    "ncaab": {
        "pinnacle": "https://www.pinnacle.com/en/basketball/ncaa/matchups/#all",
        "fliff": "https://sports.getfliff.com/channels?channelId=462",
    },
    "epl": {
        "pinnacle": "https://www.pinnacle.com/en/soccer/england-premier-league/matchups/#all",
        "fliff": "https://sports.getfliff.com/channels?channelId=4385",
    },
    "bundesliga": {
        "pinnacle": "https://www.pinnacle.com/en/soccer/germany-bundesliga/matchups/#all",
        "fliff": "https://sports.getfliff.com/channels?channelId=4382",
    },
    "ligue_1": {
        "pinnacle": "https://www.pinnacle.com/en/soccer/france-ligue-1/matchups/#all",
        "fliff": "https://sports.getfliff.com/channels?channelId=4384",
    },
    "serie_a": {
        "pinnacle": "https://www.pinnacle.com/en/soccer/italy-serie-a/matchups/#all",
        "fliff": "https://sports.getfliff.com/channels?channelId=4388",
    },
    "laliga": {
        "pinnacle": "https://www.pinnacle.com/en/soccer/spain-la-liga/matchups/#all",
        "fliff": "https://sports.getfliff.com/channels?channelId=4395",
    },
}

def save_to_json(data, filename="output.json"):
    """Save data to a JSON file."""
    try:
        with open(filename, "w", encoding="utf-8") as f:
            json.dump(data, f, indent=4)
        print(f"Data saved to {filename}")
    except Exception as e:
        print(f"Error saving JSON file: {e}")
        
def find_value_bets(data):
    value_bets = []

    for item in data:
        sportsbook_1_odd = None
        sportsbook_2_odd = None

        # Loop through the lines to find the odds for sportsbook_id 1 and 2
        for line in item['lines']:
            if line['sportsbook_id'] == 1:
                sportsbook_1_odd = line['odd']
            elif line['sportsbook_id'] == 2:
                sportsbook_2_odd = line['odd']

        # Check if both sportsbook odds were found and the value condition is met
        if sportsbook_1_odd and sportsbook_2_odd:
            if sportsbook_1_odd - sportsbook_2_odd >= 3:
                value_bets.append({
                    "game_datetime": item['game_datetime'],
                    "team_1_name": item['team_1_name'],
                    "team_2_name": item['team_2_name'],
                    "prop_name": item['prop_name'],
                    "prop_type": item['prop_type'],
                    "sportsbook_1_odd": sportsbook_1_odd,
                    "sportsbook_2_odd": sportsbook_2_odd,
                    "value_difference": sportsbook_1_odd - sportsbook_2_odd,
                    "sportsbook_1_id": 1,
                    "sportsbook_2_id": 2,
                    "description": item['description'],
                })

    return value_bets

# Dispatcher to select the appropriate scraper
get_games_dispatch = {
    "pinnacle": get_pinnacle_games,
    "fliff": get_fliff_games,
    # Add other sportsbooks here
}

# Dispatcher to select the appropriate odds scraper
get_odds_dispatch = {
    "pinnacle": get_pinnacle_odds,
    "fliff": get_fliff_odds,
    # Add other sportsbooks here
}


def scrape_games(conn, url, sportsbook, league):
    """
    Dispatches the scraping task to the appropriate function based on the sportsbook.
    """
    scraper = get_games_dispatch.get(sportsbook)
    if scraper:
        return scraper(conn, url, sportsbook, league)
    else:
        raise ValueError(f"No scraper implemented for sportsbook: {sportsbook}")


def scrape_odds(conn, urls, sportsbook, league):
    """
    Dispatches the scraping task to the appropriate function based on the sportsbook.
    """
    scraper = get_odds_dispatch.get(sportsbook)
    if scraper:
        return scraper(conn, urls, sportsbook, league)
    else:
        raise ValueError(f"No scraper implemented for sportsbook: {sportsbook}")


def get_odds(leagues, sportsbooks):
    conn = get_db_connection()
    try:
        with conn:
            for league in leagues:
                for sportsbook in sportsbooks:
                    if league in league_urls and sportsbook in league_urls[league]:
                        current_datetime = datetime.now()
                        urls = get_game_url_by_sb_and_game(conn, (sportsbook, current_datetime, league))
                        
                        scrape_odds(conn, urls, sportsbook, league)
                    else:
                        logging.warning(f"No link found for {league} at {sportsbook}")

    except Exception as e:
        logging.error(f"Error processing odds: {e}")
    finally:
        conn.close()


def get_games(leagues, sportsbooks):
    conn = get_db_connection()
    try:
        with conn:
            for league in leagues:
                for sportsbook in sportsbooks:
                    if league in league_urls and sportsbook in league_urls[league]:
                        url = league_urls[league][sportsbook]
                        try:
                            scrape_games(conn, url, sportsbook, league)
                        except Exception as scrape_error:
                            logging.error(
                                f"Error scraping {league} at {sportsbook}: {scrape_error}"
                            )
                    else:
                        logging.warning(f"No link found for {league} at {sportsbook}")
    except Exception as e:
        logging.error(f"Error saving games to DB: {e}")
    finally:
        conn.close()


def get_value():
    conn = get_db_connection()
    try:
        with conn:
            lines = get_lines(conn)
            # if lines:
            #     save_to_json(lines)
            
            value_bets = find_value_bets(lines)
            print()
            print("-" * 50)
            
            
            for bet in value_bets:
                print(f"Game: {bet['team_1_name']} vs {bet['team_2_name']} on {bet['game_datetime']}")
                print(f"Prop: {bet['prop_name']} {({bet['prop_type']})}")
                print(f"Pinnacle 1 Odd: {bet['sportsbook_1_odd']} (Value: {bet['value_difference']})")
                print(f"Fliff 2 Odd: {bet['sportsbook_2_odd']}")
                print(f"Take {bet['description']}")
                print("-" * 50)
                print()
            

    except Exception as e:
        logging.error(f"Error processing odds: {e}")
    finally:
        conn.close()

def main():
    parser = argparse.ArgumentParser(description="Sports Betting Data Scraper")
    parser.add_argument(
        "--leagues", type=str, nargs="+", required=True, help="Select leagues"
    )
    parser.add_argument(
        "--books", type=str, nargs="+", required=True, help="Select sportsbooks"
    )
    parser.add_argument(
        "--get_games",
        action="store_true",
        help="Get games for selected leagues and books",
    )
    parser.add_argument(
        "--get_odds",
        action="store_true",
        help="Get odds for selected leagues and books",
    )
    parser.add_argument(
        "--get_value",
        action="store_true",
        help="Get value bets for selected leagues and books",
    )
    parser.add_argument(
        "--connect_db",
        action="store_true",
        help="Connect to the database and sync data",
    )

    args = parser.parse_args()

    if args.connect_db:
        print("Connecting to db and syncing data")
        # Add your DB connection logic here
    elif args.get_games:
        print("Getting games:")
        print("Leagues:", args.leagues)
        print("Books:", args.books)
        get_games(args.leagues, args.books)  # Pass selected leagues and books
    elif args.get_odds:
        print("Getting odds:")
        print("Leagues:", args.leagues)
        print("Books:", args.books)
        get_odds(args.leagues, args.books)  # Pass selected leagues and books
    elif args.get_value:
        print("Getting value:")
        print("Leagues:", args.leagues)
        print("Books:", args.books)
        get_value()


# Example usage
if __name__ == "__main__":
    main()
    # get_value()
    # get_odds(['nba'], ['fliff'])
