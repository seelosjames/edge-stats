import psycopg2
from psycopg2.extras import execute_values
import os


def get_db_connection():
    return psycopg2.connect(
        dbname="odds_insights",
        user="postgres",
        password="12345678",
        host="localhost",
        port="5433",
    )


def insert_game(conn, game_data):
    query = """
    INSERT INTO game (game_uuid, league_id, team1, team2, game_date)
    VALUES (
        %s,
        (SELECT league_id FROM league WHERE abbreviation = %s),
        %s,
        %s,
        %s
    )
    ON CONFLICT (game_uuid) DO NOTHING
    """
    with conn.cursor() as cursor:
        cursor.execute(query, game_data)


def insert_game_url(conn, game_url_data):
    query = """
    INSERT INTO game_links (game_id, sportsbook_id, url)
    VALUES (
        (SELECT game_id FROM game WHERE game_uuid = %s),
        (SELECT sportsbook_id FROM sportsbook WHERE sportsbook_name = %s),
        %s
    )
    ON CONFLICT DO NOTHING;
    """
    with conn.cursor() as cursor:
        cursor.execute(query, game_url_data)
