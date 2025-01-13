from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from webdriver_manager.chrome import ChromeDriverManager
from datetime import datetime, timedelta
import time
import re

from .helper import *
from betting.betting import *


# XPaths for Pinnacle elements
XPATH_DATE = '//*[@id="root"]/div[1]/div[2]/main/div[1]/div[2]/div[2]/div/span'
XPATH_TEAM_1 = (
    '//*[@id="root"]/div[1]/div[2]/main/div[1]/div[2]/div[3]/div[2]/div/label'
)
XPATH_TEAM_2 = (
    '//*[@id="root"]/div[1]/div[2]/main/div[1]/div[2]/div[4]/div[2]/div/label'
)


def pinnacle_parse_prop_string(input_string):
    """
    Parses the input string based on its format and returns relevant information.

    For strings in the format `Player (Prop)`, it returns:
        - player: The part before the parentheses
        - prop: The part inside the parentheses
        - prop_type: A fixed string "player_prop"

    Other formats will be handled in the future.
    """
    # Check for the format with parentheses
    match = re.match(r"^(.*?)\s*\((.*?)\)$", input_string)
    if match:
        player = match.group(1).strip()
        prop = match.group(2).strip()
        prop_type = "player_prop"
        return {"player": player, "prop": prop, "prop_type": prop_type}

    # Placeholder for other formats
    return {"message": "Format not yet supported"}


def get_pinnacle_games(url, sport):
    # Configure Chrome options
    chrome_options = Options()
    chrome_options.add_argument("--disable-gpu")
    chrome_options.add_argument("--no-sandbox")
    chrome_options.add_argument("--disable-dev-shm-usage")
    chrome_options.add_argument("--headless=new")
    chrome_options.add_argument("--window-size=1920,1080")

    # Initialize the WebDriver
    service = Service(ChromeDriverManager().install())
    driver = webdriver.Chrome(service=service, options=chrome_options)

    sports_book_name = "pinnacle"
    data = []

    try:
        driver.get(url)

        # Wait for the games container to load
        games = WebDriverWait(driver, 10).until(
            EC.presence_of_all_elements_located(
                (
                    By.XPATH,
                    '//*[@id="root"]/div[1]/div[2]/main/div/div[4]/div[2]/div/div',
                )
            )
        )

        game_urls = []

        # Loop through each game element to extract url
        for game in games:
            if game.get_attribute("class") == "row-u9F3b9WCM3 row-k9ktBvvTsJ":
                link_container = game.find_element(By.TAG_NAME, "a")
                game_urls.append(link_container.get_attribute("href"))

        for game_url in game_urls:
            driver.get(game_url)

            # Extract details
            date_time_str = (
                WebDriverWait(driver, 10)
                .until(EC.presence_of_element_located((By.XPATH, XPATH_DATE)))
                .text
            )

            team_1 = (
                WebDriverWait(driver, 10)
                .until(EC.presence_of_element_located((By.XPATH, XPATH_TEAM_1)))
                .text
            )

            team_2 = (
                WebDriverWait(driver, 10)
                .until(EC.presence_of_element_located((By.XPATH, XPATH_TEAM_2)))
                .text
            )

            # Parse datetime for date
            date_time_obj = datetime.strptime(date_time_str, "%A, %B %d, %Y at %H:%M")

            # Parse datetime for id
            date_str = date_time_str.split(" at")[0]
            date_obj = datetime.strptime(date_str, "%A, %B %d, %Y")

            game_id = generate_id(team_1, team_2, date_obj.strftime("%Y-%m-%d"))

            # Create new game object
            new_game = {
                "id": game_id,
                "game": {"teams": [team_1, team_2], "date": date_time_obj},
                "links": {sports_book_name: game_url},
                "league": sport,
            }

            data.append(new_game)

        return data

    except Exception as e:
        print(f"Error while processing link {url}: {e}")
    finally:
        driver.quit()

def get_pinnacle_odds(driver, data, link):
    try:
        driver.get(link)
        time.sleep(4)

        # Wait for content to load
        show_all_button = WebDriverWait(driver, 10).until(
            EC.presence_of_element_located(
                (
                    By.XPATH,
                    '//*[@id="root"]/div[1]/div[2]/main/div[3]/div[1]/div[1]/div[1]/div/button',
                )
            )
        )
        show_all_button.click()

        # Prop divs
        prop_category_divs = WebDriverWait(driver, 10).until(
            EC.presence_of_all_elements_located(
                (
                    By.XPATH,
                    '//*[@id="root"]/div[1]/div[2]/main/div[3]/div',
                )
            )
        )

        for prop_category in prop_category_divs:
            prop_divs = prop_category.find_elements(By.XPATH, "div")
            for prop_div in prop_divs:
                # //*[@id="root"]/div[1]/div[2]/main/div[3]/div[1]/div[1]/div[1]/span[1]
                title = prop_div.find_element(By.XPATH, "div[1]/span[1]").text

            # //*[@id="root"]/div[1]/div[2]/main/div[3]/div[1]/div[1]

        # Get the matchup to use as the data key
        away_team = driver.find_element(
            By.XPATH,
            '//*[@id="root"]/div[1]/div[2]/main/div[1]/div[2]/div[3]/div[2]/div/label',
        ).text
        home_team = driver.find_element(
            By.XPATH,
            '//*[@id="root"]/div[1]/div[2]/main/div[1]/div[2]/div[4]/div[2]/div/label',
        ).text
        game = f"{away_team} vs {home_team}"  # Key for data dict

        # Get player prop div # and number of player props available
        lenth = len(
            driver.find_elements(
                By.XPATH, '//*[@id="root"]/div[1]/div[2]/main/div[3]/div'
            )
        )
        player_props = f'//*[@id="root"]/div[1]/div[2]/main/div[3]/div[{str(lenth)}]'
        num_props = len(driver.find_elements(By.XPATH, f"{player_props}/div"))

        # Initialize Game Data
        game_data = {}

        # Loop through player props
        for i in range(1, num_props + 1):
            # Get and format Prop info
            info = driver.find_element(
                By.XPATH, f"{player_props}/div[{str(i)}]/div[1]/span[1]"
            ).text
            name = info.split("(")[0].strip()
            item = info.split("(")[1].rstrip(")")
            over_number = re.search(
                r"\d+(\.\d+)?",
                driver.find_element(
                    By.XPATH,
                    f"{player_props}/div[{str(i)}]/div[2]/div/div/div[1]/button/span[1]",
                ).text,
            ).group()
            over_odds = driver.find_element(
                By.XPATH,
                f"{player_props}/div[{str(i)}]/div[2]/div/div/div[1]/button/span[2]",
            ).text
            under_number = re.search(
                r"\d+(\.\d+)?",
                driver.find_element(
                    By.XPATH,
                    f"{player_props}/div[{str(i)}]/div[2]/div/div/div[2]/button/span[1]",
                ).text,
            ).group()
            under_odds = driver.find_element(
                By.XPATH,
                f"{player_props}/div[{str(i)}]/div[2]/div/div/div[2]/button/span[2]",
            ).text

            # Initialize player and prop type
            player = f"{name} {item}"

            # Save info to game_data
            game_data[player] = {
                "Pinnacle": {
                    "over_number": float(over_number),
                    "over_odds": decimal_to_american(float(over_odds)),
                    "under_number": float(under_number),
                    "under_odds": decimal_to_american(float(under_odds)),
                    "link": link,
                }
            }

        data[game] = game_data
        return data
    except Exception as e:
        print("An error occurred while fetching matchups:", e)
