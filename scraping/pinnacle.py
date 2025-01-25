from selenium import webdriver
from selenium.common.exceptions import NoSuchElementException
from selenium.webdriver.common.by import By
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from webdriver_manager.chrome import ChromeDriverManager
from datetime import datetime, timedelta
import time
import re


from scraping.helper import *
from betting.betting import *


# XPaths for Pinnacle elements
XPATH_DATE = '//*[@id="root"]/div[1]/div[2]/main/div[1]/div[2]/div[2]/div/span'
XPATH_TEAM_1 = (
    '//*[@id="root"]/div[1]/div[2]/main/div[1]/div[2]/div[3]/div[2]/div/label'
)
XPATH_TEAM_2 = (
    '//*[@id="root"]/div[1]/div[2]/main/div[1]/div[2]/div[4]/div[2]/div/label'
)

SPORTSBOOK_NAME = "pinnacle"


import re


def pinnacle_parse_line_info(line_divs, prop_uuid, team_1=None, team_2=None):
    results = []
    for i, line_div in enumerate(line_divs):
        line_info = line_div.find_elements(By.XPATH, "button/span")
        description = line_info[0].text
        odd = line_info[1].text
        if team_1 or team_2:
            if i % 2 == 0:
                description = f"{team_1} {description}" if team_1 else description
            else:
                description = f"{team_2} {description}" if team_2 else description
        results.append({"description": description, "odd": odd, 'line_uuid': generate_line_uuid(prop_uuid, description, odd)})
    return results


def pinnacle_parse_prop_string(input_string):
    """
    Parses the input string based on its format and returns relevant information.

    Supported formats:
    1. "Player (Prop)" - Returns player, prop, and prop_type as "player_prop".
    2. "Prop Name - Prop Type" - Returns prop_name and prop_type.

    For unsupported formats, returns a message indicating the format is not supported.
    """

    # Check for the format with parentheses (Player (Prop))
    match = re.match(r"^(.*?)\s*\((.*?)\)$", input_string)
    if match:
        player = match.group(1).strip()
        prop = match.group(2).strip()
        return {
            "prop_name": player + " " + prop,
            "prop_type": "player_prop",
        }

    # Check for the format with a dash (Prop Name - Prop Type)
    match = re.match(r"^(.*?)\s*[-–]\s*(.*?)$", input_string)
    if match:
        prop_name = match.group(1).strip()
        prop_type = match.group(2).strip()
        return {"prop_name": normalize_prop_name(prop_name), "prop_type": prop_type}

    # Placeholder for other formats
    return {"message": "Format not yet supported"}


def get_pinnacle_games(url, league):
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

            team_1 = normalize_team_name(
                WebDriverWait(driver, 10)
                .until(EC.presence_of_element_located((By.XPATH, XPATH_TEAM_1)))
                .text
            )

            team_2 = normalize_team_name(
                WebDriverWait(driver, 10)
                .until(EC.presence_of_element_located((By.XPATH, XPATH_TEAM_2)))
                .text
            )

            # Parse datetime for date
            date_time_obj = datetime.strptime(date_time_str, "%A, %B %d, %Y at %H:%M")

            # Parse datetime for id
            date_str = date_time_str.split(" at")[0]
            date_obj = datetime.strptime(date_str, "%A, %B %d, %Y")

            game_uuid = generate_game_uuid(
                team_1, team_2, date_obj.strftime("%Y-%m-%d")
            )

            # Data to Update/Create Game
            # (game_uuid, league_id, team_1, team_2, game_date)
            game_data = (game_uuid, league, team_1, team_2, date_time_obj)

            # Data to Update/Create Game URL
            # (game_id, sportsbook_id, url)
            game_url_data = (game_uuid, SPORTSBOOK_NAME, game_url)

            data.append([game_data, game_url_data])

        return data

    except Exception as e:
        print(f"Error while processing link {url}: {e}")
    finally:
        driver.quit()


def get_pinnacle_odds(url_list):

    # url_list: (game_id, game_url, league_name)

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

    for url in url_list:
        get_pinnacle_nba_odds(driver, url)


def get_pinnacle_nba_odds(driver, url_data):
    game_id = url_data[0]
    url = url_data[1]
    league_name = url_data[2]
    data = []

    try:
        driver.get(url)

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
                title = pinnacle_parse_prop_string(
                    prop_div.find_element(By.XPATH, "div[1]/span[1]").text
                )
                prop_name = title["prop_name"]
                prop_type = title["prop_type"]
                prop_uuid = generate_prop_uuid(prop_name, prop_type, game_id)

                try:
                    button = prop_div.find_element(By.XPATH, "div[2]/button")
                    button.click()
                except NoSuchElementException:
                    print(f"No 'More lines' button found for {prop_name}.")
                # //*[@id="root"]/div[1]/div[2]/main/div[3]/div[1]/div[2]/div[2]/ul/li[1]
                try:
                    team_1 = prop_div.find_element(
                        By.XPATH, "div[2]/ul/li[1]"
                    ).text
                    team_2 = prop_div.find_element(
                        By.XPATH, "div[2]/ul/li[2]"
                    ).text
                except NoSuchElementException:
                    team_1, team_2 = None, None
                    print(
                        f"Teams not found for {prop_name}, proceeding without team names."
                    )

                line_divs = prop_div.find_elements(
                    By.CLASS_NAME, "button-wrapper-Z7pE7Fol_T"
                )
                results = pinnacle_parse_line_info(line_divs, prop_uuid, team_1, team_2)
                print(f"Parsed {len(results)} lines for {prop_name}: {results}")
                print()
    except Exception as e:
        print("An error occurred while fetching matchups:", e)
