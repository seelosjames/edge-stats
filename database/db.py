import psycopg2

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


def insert_prop(conn, prop_data):
    query = """
        INSERT INTO PROP (prop_uuid, game_id, prop_type, prop_name)
        VALUES (%s, %s, %s, %s)
        ON CONFLICT (prop_uuid) 
        DO UPDATE SET prop_uuid = EXCLUDED.prop_uuid
        RETURNING prop_id;
    """

    try:
        with conn.cursor() as cur:
            cur.execute(query, prop_data)
            result = cur.fetchone()
            prop_id = result[0] if result else None
            conn.commit()

        return prop_id
    except Exception as e:
        print(f"Error inserting prop: {e}")
        conn.rollback()
        return None


def insert_line(conn, line_data):
    query = """
        INSERT INTO LINE (prop_id, line_uuid, odd, description, sportsbook_id)
        VALUES (%s, %s, %s, %s, (SELECT sportsbook_id FROM sportsbook WHERE sportsbook_name = %s))
        ON CONFLICT (line_uuid) DO UPDATE SET
            odd = EXCLUDED.odd,
            description = EXCLUDED.description
        RETURNING line_uuid;
    """

    try:
        with conn.cursor() as cur:
            cur.execute(query, line_data)
            line_uuid = cur.fetchone()[0]
            conn.commit()

        return line_uuid
    except Exception as e:
        print(f"Error inserting or updating line: {e}")
        conn.rollback()
        
def get_lines(conn):
    query = """
        SELECT 
            p.prop_id,
            p.prop_name,
            p.prop_type,
            g.game_id,
            g.game_uuid,
            g.game_datetime,
            t1.team_name AS team_1_name,
            t2.team_name AS team_2_name,
            l.description,
            ARRAY_AGG(
                JSONB_BUILD_OBJECT(
                    'line_id', l.line_id,
                    'line_uuid', l.line_uuid,
                    'sportsbook_id', l.sportsbook_id,
                    'odd', l.odd
                )
            ) AS lines
        FROM LINE l
        JOIN PROP p ON l.prop_id = p.prop_id
        JOIN GAME g ON p.game_id = g.game_id
        JOIN TEAM t1 ON g.team_1 = t1.team_id
        JOIN TEAM t2 ON g.team_2 = t2.team_id
        GROUP BY 
            p.prop_id, p.prop_name, p.prop_type, 
            g.game_id, g.game_uuid, g.game_datetime, 
            t1.team_name, t2.team_name, l.description
        ORDER BY p.prop_id, l.description;
    """

    try:
        
        with conn.cursor() as cur:

            # Execute query
            cur.execute(query)
            results = cur.fetchall()

            grouped_lines = [
                {
                    "prop_id": row[0],
                    "prop_name": row[1],
                    "prop_type": row[2],
                    "game_id": row[3],
                    "game_uuid": row[4],
                    "game_datetime": row[5].strftime('%Y-%m-%d %H:%M:%S') if row[5] else None,
                    "team_1_name": row[6],
                    "team_2_name": row[7],
                    "description": row[8],
                    "lines": row[9]  # Already an array of JSONB objects
                }
                for row in results
            ]

            return grouped_lines

    except Exception as e:
        print(f"Database error: {e}")
        return None
