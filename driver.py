import argparse
import logging
from database.db import *
from scraping.pinnacle import *


sports_links = {
    "NBA": {
        "Pinnacle": "https://www.pinnacle.com/en/basketball/nba/matchups/#all",
        "Fliff": "https://sports.getfliff.com/channels?channelId=461",
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


# Function to process and save all games
def get_games(sports):
    conn = get_db_connection()
    try:
        with conn:
            for sport in sports:
                # Scrape data for each sport
                games = get_pinnacle_games(
                    sports_links[sport]["Pinnacle"], sport
                )
                for game in games:
                    insert_game(conn, game[0])
                    insert_game_url(conn, game[1])
    except Exception as e:
        logging.error(f"Error saving games to DB: {e}")
    finally:
        conn.close()


def main():
    parser = argparse.ArgumentParser(description="Sports Betting Data Scraper")
    parser.add_argument("--leagues", type=str, nargs="+", help="Select leagues")
    parser.add_argument("--books", type=str, nargs="+", help="Select Sportsbooks")
    parser.add_argument("--get_games", action="store_true", help="Select leagues")
    parser.add_argument("--get_odds", action="store_true", help="Select leagues")
    parser.add_argument("--get_value", action="store_true", help="Select leagues")
    parser.add_argument("--connect_db", action="connect_db", help="Select leagues")

    args = parser.parse_args()

    if args.connect_db:
        print("Connecting to db and syncing data")
    elif args.get_games:
        print("Getting games:")
        print("Leagues:", args.leagues)
        print("Books:", args.books)
    elif args.get_odds:
        print("Getting odds:")
        print("Leagues:", args.leagues)
        print("Books:", args.books)
    elif args.get_value:
        print("Getting value:")
        print("Leagues:", args.leagues)
        print("Books:", args.books)

    # get_games(["NBA"])


# Example usage
if __name__ == "__main__":
    main()
