import argparse
import logging
from database.db import *
from scraping.pinnacle import *
from scraping.fliff import *


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


# Dispatcher to select the appropriate scraper
scraper_dispatch = {
    "pinnacle": get_pinnacle_games,
    "fliff": get_fliff_games,  # Implement this for Fliff scraping
    # Add other sportsbooks here
}


def scrape_games(sportsbook, url, sport):
    """
    Dispatches the scraping task to the appropriate function based on the sportsbook.
    """
    scraper = scraper_dispatch.get(sportsbook)
    if scraper:
        return scraper(url, sport)
    else:
        raise ValueError(f"No scraper implemented for sportsbook: {sportsbook}")


def get_games(sports, books):
    conn = get_db_connection()
    try:
        with conn:
            for sport in sports:
                for book in books:
                    if sport in sports_links and book in sports_links[sport]:
                        url = sports_links[sport][book]
                        try:
                            # Use the unified scraper
                            games = scrape_games(book, url, sport)
                            for game in games:
                                team_1_data = (game[0][2], sport)
                                team_2_data = (game[0][3], sport)

                                insert_team(conn, team_1_data)
                                insert_team(conn, team_2_data)

                                insert_game(conn, game[0])
                                insert_game_url(conn, game[1])
                        except Exception as scrape_error:
                            logging.error(
                                f"Error scraping {sport} at {book}: {scrape_error}"
                            )
                    else:
                        logging.warning(f"No link found for {sport} at {book}")
    except Exception as e:
        logging.error(f"Error saving games to DB: {e}")
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
        # Add odds scraping logic here
    elif args.get_value:
        print("Getting value:")
        print("Leagues:", args.leagues)
        print("Books:", args.books)
        # Add value bet logic here


# Example usage
if __name__ == "__main__":
    main()
