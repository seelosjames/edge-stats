-- LEAGUE Table
CREATE TABLE LEAGUE (
    league_id SERIAL PRIMARY KEY,
    league_name VARCHAR(255) NOT NULL UNIQUE,
    abbreviation VARCHAR(50) UNIQUE,
    sport_type VARCHAR(100) NOT NULL -- Example: Basketball, Soccer, etc.
);

-- TEAM Table
CREATE TABLE TEAM (
    team_id SERIAL PRIMARY KEY,
    team_name VARCHAR(255) NOT NULL,
    location VARCHAR(255),
    league_id INT NOT NULL REFERENCES LEAGUE(league_id)
);

-- GAMES Table
CREATE TABLE GAMES (
    game_id SERIAL PRIMARY KEY,
    game_uuid UUID NOT NULL UNIQUE,
    league_id INT NOT NULL REFERENCES LEAGUE(league_id),
    team1 INT NOT NULL REFERENCES TEAM(team_id),
    team2 INT NOT NULL REFERENCES TEAM(team_id),
    game_date DATE NOT NULL
);

-- PROP Table
CREATE TABLE PROP (
    prop_id SERIAL PRIMARY KEY,
    prop_uuid UUID NOT NULL UNIQUE,
    game_id INT NOT NULL REFERENCES GAMES(game_id) ON DELETE CASCADE,
    prop_name VARCHAR(255) NOT NULL,
    prop_type VARCHAR(100)
);

-- LINE Table
CREATE TABLE LINE (
    line_id SERIAL PRIMARY KEY,
    prop_id INT NOT NULL REFERENCES PROP(prop_id) ON DELETE CASCADE,
    line DECIMAL(10,2) NOT NULL,
    description TEXT,
    odd DECIMAL(4,4) NOT NULL
);

-- SPORTSBOOK Table
CREATE TABLE SPORTSBOOK (
    sportsbook_id SERIAL PRIMARY KEY,
    sportsbook_name VARCHAR(255) NOT NULL
);

-- SPORTSBOOK_LEAGUE_URL Table
CREATE TABLE SPORTSBOOK_LEAGUE_URL (
    url_id SERIAL PRIMARY KEY,
    sportsbook_id INT NOT NULL REFERENCES SPORTSBOOK(sportsbook_id) ON DELETE CASCADE,
    league_id INT NOT NULL REFERENCES LEAGUE(league_id),
    url TEXT NOT NULL
);

-- GAMEURL Table
CREATE TABLE GAMEURL (
    game_url_id SERIAL PRIMARY KEY,
    game_id INT NOT NULL REFERENCES GAMES(game_id) ON DELETE CASCADE,
    sportsbook_id INT NOT NULL REFERENCES SPORTSBOOK(sportsbook_id) ON DELETE CASCADE,
    url TEXT NOT NULL
);

-- Indexes for performance
CREATE INDEX idx_prop_game_id ON PROP (game_id);
CREATE INDEX idx_gameurl_game_id ON GAMEURL (game_id);
CREATE INDEX idx_gameurl_sportsbook_id ON GAMEURL (sportsbook_id);
CREATE INDEX idx_team_league_id ON TEAM (league_id);
CREATE INDEX idx_games_league_id ON GAMES (league_id);
CREATE INDEX idx_sportsbook_league_url_league_id ON SPORTSBOOK_LEAGUE_URL (league_id);
