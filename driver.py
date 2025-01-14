from database.db import *
from scraping.pinnacle import *
import uuid
import hashlib
import logging
import os


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
def save_games_to_db(sports):
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


# Example usage
if __name__ == "__main__":
    save_games_to_db(["NBA"])
