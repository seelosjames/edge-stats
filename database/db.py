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


def get_game_url_by_sb_and_game(conn, data):
    query = """
    SELECT gu.game_id, gu.game_url, sb.sportsbook_name
    FROM game_url gu
    JOIN sportsbook sb ON gu.sportsbook_id = sb.sportsbook_id
    JOIN game g ON gu.game_id = g.game_id
    JOIN league l ON g.league_id = l.league_id
    WHERE sb.sportsbook_name = %s
    AND g.game_datetime > %s
    AND l.league_name = %s;
    """
    try:
        with conn.cursor() as cursor:
            cursor.execute(query, data)
            results = cursor.fetchall()
        return results
    except Exception as e:
        print(f"Error fetching game URLs: {e}")
        return None


def insert_game(conn, game_data):
    query = """
    INSERT INTO game (game_uuid, league_id, team_1, team_2, game_datetime)
    VALUES (
        %s,
        (SELECT league_id FROM league WHERE league_name = %s),
        (SELECT team_id FROM team WHERE team_name = %s),
        (SELECT team_id FROM team WHERE team_name = %s),
        %s
    )
    ON CONFLICT (game_uuid) DO NOTHING
    """
    try:
        with conn.cursor() as cursor:
            cursor.execute(query, game_data)
        conn.commit()
    except Exception as e:
        print(f"Error inserting game: {e}")
        conn.rollback()


def insert_game_url(conn, game_url_data):
    query = """
    INSERT INTO game_url (game_id, sportsbook_id, game_url)
    VALUES (
        (SELECT game_id FROM game WHERE game_uuid = %s),
        (SELECT sportsbook_id FROM sportsbook WHERE sportsbook_name = %s),
        %s
    )
    ON CONFLICT DO NOTHING;
    """
    try:
        with conn.cursor() as cursor:
            cursor.execute(query, game_url_data)
        conn.commit()
    except Exception as e:
        print(f"Error inserting game URL: {e}")
        conn.rollback()


def insert_team(conn, team_data):
    query = """
    INSERT INTO team (team_name, league_id)
    VALUES (
        %s,
        (SELECT league_id FROM league WHERE league_name = %s)
    )
    ON CONFLICT DO NOTHING;
    """
    try:
        with conn.cursor() as cursor:
            cursor.execute(query, team_data)
        conn.commit()
    except Exception as e:
        print(f"Error inserting team: {e}")
        conn.rollback()
