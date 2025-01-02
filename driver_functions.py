import json
import os

from functions.betting import *
from functions.pinnacle import *
from functions.fliff import *

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


# Function to load existing data from data.json
def load_existing_data(file_path="data.json"):
    if os.path.exists(file_path):
        try:
            with open(file_path, "r") as file:
                return json.load(file)
        except json.JSONDecodeError:
            # If the file is empty or invalid JSON, return an empty list
            print(f"{file_path} is empty or corrupted. Starting with an empty dataset.")
            return []
    return []


# Function to save data back to data.json
def save_data(data, file_path="data.json"):
    with open(file_path, "w") as file:
        json.dump(data, file, indent=4, default=str)


def get_games(
    sports=[
        "nba",
        "nfl",
        "nhl",
        "ncaaf",
        "ncaab",
        "epl",
        "bundesliga",
        "ligue_1",
        "serie_a",
        "laliga",
    ]
):
    # Dictionary to store matchup data
    data = load_existing_data()

    for sport in sports:
        # get_pinnacle_games(sports_links[sport]["pinnacle"], data, sport)
        get_fliff_games(sports_links[sport]["fliff"], data, sport)

    with open("data.json", "w") as f:
        json.dump(data, f, default=str, indent=4)


# Example usage
if __name__ == "__main__":
    get_games(
        [
            "nba",
            # "nfl",
            # "nhl",
            # "ncaaf",
            # "ncaab",
            # "epl",
            # "bundesliga",
            # "ligue_1",
            # "serie_a",
            # "laliga",
        ]
    )
