import argparse
import logging
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


def scrape_games(sportsbook, url, sport):
    """
    Dispatches the scraping task to the appropriate function based on the sportsbook.
    """
    scraper = get_games_dispatch.get(sportsbook)
    if scraper:
        return scraper(url, sport)
    else:
        raise ValueError(f"No scraper implemented for sportsbook: {sportsbook}")


def scrape_odds(sportsbook, sport):
    """
    Dispatches the scraping task to the appropriate function based on the sportsbook.
    """
    scraper = get_odds_dispatch.get(sportsbook)
    if scraper:
        return scraper(sport)
    else:
        raise ValueError(f"No scraper implemented for sportsbook: {sportsbook}")


def get_odds(leagues, books):
    conn = get_db_connection()
    try:
        with conn:
            # Dictionary to group URLs by sportsbook
            sportsbook_urls = defaultdict(list)

            # Collect URLs grouped by sportsbook
            for league in leagues:
                for book in books:
                    if league in league_urls and book in league_urls[league]:
                        current_datetime = datetime.now()
                        data = (
                            book,
                            current_datetime,
                            league,
                        )  # (sportsbook, datetime, league)
                        urls = get_game_url_by_sb_and_game(conn, data)

                        for game_id, game_url, sportsbook_name in urls:
                            sportsbook_urls[sportsbook_name].append(
                                (game_id, game_url, league)
                            )
                    else:
                        logging.warning(f"No link found for {league} at {book}")

            # Process each sportsbook one at a time
            for sportsbook, url_list in sportsbook_urls.items():
                logging.info(f"Processing odds for sportsbook: {sportsbook}")
                scraper = get_odds_dispatch.get(sportsbook)
                if scraper:
                    try:
                        pass
                        scraper(
                            url_list
                        )  # Scrape odds using the specific sportsbook scraper
                    except Exception as e:
                        logging.error(
                            f"Error scraping odds at {sportsbook}: {e}"
                        )
                else:
                    logging.warning(
                        f"No scraper implemented for sportsbook: {sportsbook}"
                    )

    except Exception as e:
        logging.error(f"Error processing odds: {e}")
    finally:
        conn.close()


def get_games(leagues, books):
    conn = get_db_connection()
    try:
        with conn:
            for league in leagues:
                for book in books:
                    if league in league_urls and book in league_urls[league]:
                        url = league_urls[league][book]
                        try:
                            # Use the unified scraper
                            games = scrape_games(book, url, league)
                            for game in games:
                                team_1_data = (game[0][2], league)
                                team_2_data = (game[0][3], league)

                                insert_team(conn, team_1_data)
                                insert_team(conn, team_2_data)

                                insert_game(conn, game[0])
                                insert_game_url(conn, game[1])
                        except Exception as scrape_error:
                            logging.error(
                                f"Error scraping {league} at {book}: {scrape_error}"
                            )
                    else:
                        logging.warning(f"No link found for {league} at {book}")
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
        get_odds(args.leagues, args.books)  # Pass selected leagues and books
    elif args.get_value:
        print("Getting value:")
        print("Leagues:", args.leagues)
        print("Books:", args.books)
        # Add value bet logic here


# Example usage
if __name__ == "__main__":
    main()
