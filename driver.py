from database.db import *
from scraping.pinnacle import *
import uuid
import hashlib
import logging
import os



sports_links = {
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


# Function to save a single game and its links
def save_game_to_db(conn, game, league, sportsbook_id, url):

    # Insert game into the database
    game_data = (
        "game_uuid",
        league,
        game["team1"],
        game["team2"],
        game["game_date"],
    )
    game_id = insert_game(conn, game_data)

    # Insert game link into the database
    if game_id:
        link_data = (game_id, url, sportsbook_id)
        insert_game_link(conn, link_data)


# Function to process and save all games
def save_games_to_db(sports):
    conn = get_db_connection()
    try:
        with conn:
            for sport in sports:
                # Scrape data for each sport
                games = get_pinnacle_games(
                    sports_links[sport]["pinnacle"], sport
                )  # Replace [] with the correct list
                for game in games:
                    print(game)
                    # save_game_to_db(
                    #     conn, game, sport, 1, sports_links[sport]["pinnacle"]
                    # )  # Assuming sportsbook_id=1 for now
    except Exception as e:
        logging.error(f"Error saving games to DB: {e}")
    finally:
        conn.close()


# Example usage
if __name__ == "__main__":
    
    save_games_to_db(["nba"])
