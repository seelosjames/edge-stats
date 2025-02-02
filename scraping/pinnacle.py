from selenium import webdriver
from selenium.common.exceptions import NoSuchElementException
from selenium.webdriver.common.by import By
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from webdriver_manager.chrome import ChromeDriverManager
from datetime import datetime
import re

from database.db import *
from scraping.helper import *
from betting.betting import *


def pinnacle_parse_line_info(conn, line_divs, prop_uuid, prop_id, prop_type, sportsbook, team_1=None, team_2=None):
    for i, line_div in enumerate(line_divs):
        line_info = line_div.find_elements(By.XPATH, "button/span")
        if len(line_info) == 0:
            return
        if prop_type == "Player Prop":
            description = " ".join(line_info[0].text.split()[:2])
        else:
            description = line_info[0].text
        if team_1 or team_2:
            if i % 2 == 0:
                description = f"{team_1} {description}" if team_1 else description
            else:
                description = f"{team_2} {description}" if team_2 else description
        
        odd = decimal_to_percentage(float(line_info[1].text))
        line_uuid = generate_line_uuid(prop_uuid, description, odd, sportsbook)
        
        print(f'Found line: {description} at {odd}')
        
        # Insert line into DB
        line_data = (prop_id, line_uuid, odd, convert_number_to_float(description), sportsbook)
        insert_line(conn, line_data)
    print()
        

def pinnacle_parse_prop_string(input_string):
    """
    Parses the input string based on its format and returns relevant information.

    Supported formats:
    1. "Player (Prop)" - Returns player, prop, and prop_type as "player_prop".
    2. "Prop Name - Prop Type" - Returns prop_name and prop_type.

    For unsupported formats, returns a message indicating the format is not supported.
    """
    
    match = re.match(r"^(.*?)\s*\((.*?)\)", input_string)
    if match:
        player = match.group(1).strip()
        prop = normalize_prop(match.group(2).strip())
        print({
            "prop_name": player + " " + prop,
            "prop_type": "Player Prop",
        })

    # Check for the format with parentheses (Player (Prop))
    match = re.match(r"^(.*?)\s*\((.*?)\)$", input_string)
    if match:
        player = match.group(1).strip()
        prop = normalize_prop(match.group(2).strip())
        return {
            "prop_name": player + " " + prop,
            "prop_type": "Player Prop",
        }

    # Check for the format with a dash (Prop Name - Prop Type)
    match = re.match(r"^(.*?)\s*[-–]\s*(.*?)$", input_string)
    if match:
        prop_name = match.group(1).strip()
        prop_type = match.group(2).strip()
        return {
            "prop_name": normalize_prop_name(prop_name),
            "prop_type": prop_type,
        }

    # Placeholder for other formats
    return {"message": "Prop not yet supported"}


def get_pinnacle_games(conn, url, sportsbook, league):
    # Configure Chrome options
    chrome_options = Options()
    chrome_options.add_argument("--no-sandbox")
    chrome_options.add_argument("--disable-dev-shm-usage")
    chrome_options.add_argument("--headless=new")
    chrome_options.add_argument("--window-size=1920,1080")

    # Initialize the WebDriver
    service = Service(ChromeDriverManager().install())
    driver = webdriver.Chrome(service=service, options=chrome_options)

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
                .until(
                    EC.presence_of_element_located(
                        (
                            By.XPATH,
                            '//*[@id="root"]/div[1]/div[2]/main/div[1]/div[2]/div[2]/div/span',
                        )
                    )
                )
                .text
            )

            team_1 = normalize_team_name(
                WebDriverWait(driver, 10)
                .until(
                    EC.presence_of_element_located(
                        (
                            By.XPATH,
                            '//*[@id="root"]/div[1]/div[2]/main/div[1]/div[2]/div[3]/div[2]/div/label',
                        )
                    )
                )
                .text
            )

            team_2 = normalize_team_name(
                WebDriverWait(driver, 10)
                .until(
                    EC.presence_of_element_located(
                        (
                            By.XPATH,
                            '//*[@id="root"]/div[1]/div[2]/main/div[1]/div[2]/div[4]/div[2]/div/label',
                        )
                    )
                )
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

            print(f'Found {league} game: {team_1} vs. {team_2} on {date_time_obj.strftime("%B %d, %Y, %I:%M %p")} on {sportsbook}')

            # Insert teams into DB
            team_1_data = (team_1, league)
            team_2_data = (team_2, league)

            insert_team(conn, team_1_data)
            insert_team(conn, team_2_data)

            # Insert game into DB
            game_data = (game_uuid, league, team_1, team_2, date_time_obj)
            insert_game(conn, game_data)

            # Insert game_url into DB
            game_url_data = (game_uuid, sportsbook, game_url)
            insert_game_url(conn, game_url_data)

    except Exception as e:
        print(f"Error while processing link {url}: {e}")
    finally:
        driver.quit()


def get_pinnacle_odds(conn, urls, sportsbook, league):

    # Configure Chrome options
    chrome_options = Options()
    chrome_options.add_argument("--no-sandbox")
    chrome_options.add_argument("--disable-dev-shm-usage")
    chrome_options.add_argument("--headless=new")
    chrome_options.add_argument("--window-size=1920,1080")

    # Initialize the WebDriver
    service = Service(ChromeDriverManager().install())
    driver = webdriver.Chrome(service=service, options=chrome_options)


    for url_data in urls:
        game_id = url_data[0]
        url = url_data[1]
        
        if league == 'nba':
            get_pinnacle_nba_odds(driver, conn, game_id, url, sportsbook, league)
        elif league == 'nhl':
            get_pinnacle_nhl_odds(driver, conn, game_id, url, sportsbook, league)
            

def get_pinnacle_nba_odds(driver, conn, game_id, url, sportsbook, league):
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

                if "message" in title and title["message"] == "Prop not yet supported":
                    continue

                prop_name = normalize_prop_name(title["prop_name"])
                prop_type = title["prop_type"]
                prop_uuid = generate_prop_uuid(prop_name, prop_type, game_id)

                try:
                    button = prop_div.find_element(By.XPATH, "div[2]/button")
                    button.click()
                except NoSuchElementException:
                    pass

                try:
                    team_1 = prop_div.find_element(By.XPATH, "div[2]/ul/li[1]").text
                    team_2 = prop_div.find_element(By.XPATH, "div[2]/ul/li[2]").text
                except NoSuchElementException:
                    team_1, team_2 = None, None

                line_divs = prop_div.find_elements(
                    By.CLASS_NAME, "button-wrapper-Z7pE7Fol_T"
                )

                print(f'Found {prop_type} prop: {prop_name} at {sportsbook}')
                
                # Insert prop into DB
                prop_data = (prop_uuid, game_id, prop_type, prop_name)
                prop_id = insert_prop(conn, prop_data)
                
                pinnacle_parse_line_info(conn, line_divs, prop_uuid, prop_id, prop_type, sportsbook, team_1, team_2)

    except Exception as e:
        print("An error occurred while fetching matchups:", e)


def get_pinnacle_nhl_odds(driver, conn, game_id, url, sportsbook, league):
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

                if "message" in title and title["message"] == "Prop not yet supported":
                    continue

                prop_name = normalize_prop_name(title["prop_name"])
                prop_type = title["prop_type"]
                prop_uuid = generate_prop_uuid(prop_name, prop_type, game_id)

                try:
                    button = prop_div.find_element(By.XPATH, "div[2]/button")
                    button.click()
                except NoSuchElementException:
                    pass

                try:
                    team_1 = prop_div.find_element(By.XPATH, "div[2]/ul/li[1]").text
                    team_2 = prop_div.find_element(By.XPATH, "div[2]/ul/li[2]").text
                except NoSuchElementException:
                    team_1, team_2 = None, None

                line_divs = prop_div.find_elements(
                    By.CLASS_NAME, "button-wrapper-Z7pE7Fol_T"
                )

                print(f'Found {prop_type} prop: {prop_name} at {sportsbook}')
                
                # Insert prop into DB
                prop_data = (prop_uuid, game_id, prop_type, prop_name)
                prop_id = insert_prop(conn, prop_data)
                
                pinnacle_parse_line_info(conn, line_divs, prop_uuid, prop_id, prop_type, sportsbook, team_1, team_2)

    except Exception as e:
        print("An error occurred while fetching matchups:", e)