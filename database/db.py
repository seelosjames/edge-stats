import psycopg2
from psycopg2.extras import execute_values
import os


def get_db_connection():
    return psycopg2.connect(
        dbname=os.getenv("odds_insights"),
        user=os.getenv("postgres"),
        password=os.getenv("12345678"),
        host=os.getenv("localhost"),
        port=os.getenv("5433"),
    )


def insert_game(conn, game_data):
    query = """
    INSERT INTO game (game_uuid, league, home_team, away_team, game_date)
    VALUES (%s, %s, %s, %s, %s)
    ON CONFLICT (game_uuid) DO NOTHING
    RETURNING game_id;
    """
    with conn.cursor() as cursor:
        cursor.execute(query, game_data)
        return (
            cursor.fetchone()
        )  # Returns game_id if inserted, or None if already exists


def insert_game_link(conn, link_data):
    query = """
    INSERT INTO game_links (game_id, url, sportsbook_id)
    VALUES (%s, %s, %s)
    ON CONFLICT DO NOTHING;
    """
    with conn.cursor() as cursor:
        cursor.execute(query, link_data)
