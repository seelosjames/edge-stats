# parsers.py

from bs4 import BeautifulSoup

def parse_odds_page(html_content):
    """Parse the sportsbook odds page to extract odds data."""
    soup = BeautifulSoup(html_content, 'html.parser')
    odds_data = []

    # Example parsing logic - modify based on the HTML structure of the page
    games = soup.select('.game')  # Adjust selector to the structure of the page
    for game in games:
        team_a = game.select_one('.team-a').text.strip()
        team_b = game.select_one('.team-b').text.strip()
        odds_a = game.select_one('.odds-a').text.strip()
        odds_b = game.select_one('.odds-b').text.strip()

        odds_data.append({
            "team_a": team_a,
            "team_b": team_b,
            "odds_a": odds_a,
            "odds_b": odds_b
        })

    return odds_data