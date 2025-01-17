from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.common.action_chains import ActionChains
from datetime import datetime, timedelta
import time

from .helper import *
from betting.betting import *


SPORTSBOOK_NAME = "Fliff"


def fliff_parse_datetime(input_str):
    now = datetime.now()

    if "at" in input_str:
        if "Today" in input_str:
            # Handle "Today at X"
            time_str = input_str.replace("Today at ", "")
            return datetime.strptime(time_str, "%I:%M %p").replace(
                year=now.year, month=now.month, day=now.day
            )
        elif "Tomorrow" in input_str:
            # Handle "Tomorrow at X"
            time_str = input_str.replace("Tomorrow at ", "")
            today_time = datetime.strptime(time_str, "%I:%M %p").replace(
                year=now.year, month=now.month, day=now.day
            )
            return today_time + timedelta(days=1)
        else:
            # Handle "Jan 5, 2025 at X"
            return datetime.strptime(input_str, "%b %d, %Y at %I:%M %p")

    raise ValueError(f"Unsupported date string format: {input_str}")


def get_fliff_games(url, league):
    # Configure Chrome options
    mobile_emulation = {"deviceName": "iPhone 12 Pro"}
    chrome_options = webdriver.ChromeOptions()
    chrome_options.add_experimental_option("mobileEmulation", mobile_emulation)

    # Initialize the WebDriver
    driver = webdriver.Chrome(options=chrome_options)

    data = []

    try:
        driver.get(url)

        # Wait for the games container to load
        games = WebDriverWait(driver, 10).until(
            EC.presence_of_all_elements_located(
                (By.CLASS_NAME, "card-shared-container")
            )
        )

        not_live = []

        for i in range(len(games)):
            game = games[i]
            live = bool(game.find_elements(By.CLASS_NAME, "live-bar-state"))

            if not live:
                new_game_data = {}

                new_game_data["index"] = i

                date = game.find_element(
                    By.XPATH, "div[2]/div[1]/div[1]/div/span[1]"
                ).text
                times = game.find_element(
                    By.XPATH, "div[2]/div[1]/div[1]/div/span[2]"
                ).text
                date_time_obj = fliff_parse_datetime(date + " " + times)

                team_1 = game.find_element(By.XPATH, "div[2]/div[2]/div[1]/span").text
                team_2 = game.find_element(By.XPATH, "div[2]/div[3]/div[1]/span").text

                game_uuid = generate_id(
                    team_1, team_2, date_time_obj.strftime("%Y-%m-%d %H:%M:%S")
                )

                # Create new game object
                new_game = {
                    "id": game_uuid,
                    "game": {"teams": [team_1, team_2], "date": date_time_obj},
                    "league": league,
                }

                new_game_data["data"] = new_game

                not_live.append(new_game_data)

        for game in not_live:
            i = game["index"]
            new_game = game["data"]

            driver.get(url)

            # Wait for the game containers to be loaded
            games = WebDriverWait(driver, 10).until(
                EC.presence_of_all_elements_located(
                    (By.CLASS_NAME, "card-shared-container")
                )
            )

            # Find the specific game card header for the game
            card_header = games[i].find_element(By.CLASS_NAME, "card-row-header")
            card_footer = games[i].find_element(By.CLASS_NAME, "card-regular-footer")

            # Wait for the card header to be clickable
            WebDriverWait(driver, 10).until(EC.element_to_be_clickable(card_header))

            # Scroll the element into view using ActionChains
            actions = ActionChains(driver)
            actions.move_to_element(card_footer).perform()

            # Attempt a regular click
            try:
                card_header.click()
            except Exception as e:
                # print(f"Click failed, trying JavaScript click: {e}")
                # Use JavaScript to click the element if regular click fails
                driver.execute_script("arguments[0].click();", card_header)

            WebDriverWait(driver, 10).until(
                EC.presence_of_all_elements_located(
                    (By.XPATH, '//*[@id="root"]/div[1]')
                )
            )

            game_url = driver.current_url
            

            # Data to Update/Create Game
            # (game_uuid, league_id, team_1, team_2, game_date)
            game_data = (game_uuid, league, team_1, team_2, date_time_obj)

            # Data to Update/Create Game URL
            # (game_id, sportsbook_id, url)
            game_url_data = (game_uuid, SPORTSBOOK_NAME, game_url)

            data.append([game_data, game_url_data])

        return data

    except Exception as e:
        print("An error occurred while fetching matchups:", e)
    finally:
        driver.quit()

def get_fliff_odds(data):

    # Set up driver
    mobile_emulation = {"deviceName": "iPhone 12 Pro"}
    chrome_options = webdriver.ChromeOptions()
    chrome_options.add_experimental_option("mobileEmulation", mobile_emulation)
    driver = webdriver.Chrome(options=chrome_options)

    try:
        url = "https://sports.getfliff.com/channels?channelId=461"
        driver.get(url)
        time.sleep(7)

        matchups = driver.find_elements(By.CLASS_NAME, "card-shared-container")
        today_index = []

        for i in range(len(matchups)):
            matchup = matchups[i]
            label = matchup.find_element(By.CLASS_NAME, "card-top-left-info").text
            if "Today" in label:
                today_index.append(i)

        time.sleep(5)

        for i in today_index:
            driver.get(url)
            time.sleep(7)
            matchups = driver.find_elements(By.CLASS_NAME, "card-shared-container")

            time.sleep(5)

            matchups[i].find_element(By.CLASS_NAME, "card-row-header").click()
            time.sleep(5)

            driver.find_element(
                By.XPATH, '//*[@id="root"]/div[1]/div[2]/div[3]'
            ).click()
            time.sleep(5)
            prop_types = driver.find_elements(
                By.XPATH, '//*[@id="root"]/div[1]/div[3]/div'
            )

            game = driver.find_element(
                By.XPATH, '//*[@id="root"]/div[1]/div[1]/div/div[1]/div/div[1]/span'
            ).text

            for j in range(1, len(prop_types) + 1):
                driver.find_element(
                    By.XPATH, f'//*[@id="root"]/div[1]/div[3]/div[{str(j)}]'
                ).click()

                prop_name = (
                    driver.find_element(
                        By.XPATH, f'//*[@id="root"]/div[1]/div[3]/div[{str(j)}]/span'
                    )
                    .text.replace("PLAYER ", "", 1)
                    .replace("POINTS AND ASSISTS AND REBOUNDS", "Pts+Rebs+Asts", 1)
                    .replace("DOUBLE DOUBLE", "Double+Double", 1)
                    .replace("TRIPLE DOUBLE", "Triple+Double", 1)
                    .title()
                    .replace("Three Points Made", "3 Point FG", 1)
                )

                time.sleep(1)
                for prop in driver.find_elements(
                    By.CLASS_NAME, "more-markets-item-option-multiple"
                ):
                    player = prop.find_element(By.XPATH, ".//p").text
                    key = f"{player} {prop_name}"
                    over_number = (
                        prop.find_element(By.XPATH, ".//div[1]/div/div[2]/span")
                        .text.replace("Over ", "", 1)
                        .title()
                    )
                    over_odds = prop.find_element(
                        By.XPATH, ".//div[1]/div/div[3]/span"
                    ).text.replace("+ ", "", 1)
                    under_number = (
                        prop.find_element(By.XPATH, ".//div[2]/div/div[2]/span")
                        .text.replace("Under ", "", 1)
                        .title()
                    )
                    under_odds = prop.find_element(
                        By.XPATH, ".//div[2]/div/div[3]/span"
                    ).text.replace("+ ", "", 1)

                    if game in data:
                        if key in data[game]:
                            data[game][key]["Fliff"] = {
                                "over_number": float(over_number),
                                "over_odds": int(over_odds),
                                "under_number": float(under_number),
                                "under_odds": int(under_odds),
                                "link": driver.current_url,
                            }
                        else:
                            data[game][key] = {
                                "Fliff": {
                                    "over_number": float(over_number),
                                    "over_odds": int(over_odds),
                                    "under_number": float(under_number),
                                    "under_odds": int(under_odds),
                                    "link": driver.current_url,
                                }
                            }
                    else:
                        print("GAME NOT FOUND")

                driver.find_element(
                    By.XPATH, f'//*[@id="root"]/div[1]/div[3]/div[{str(j)}]'
                ).click()
                time.sleep(1)

        driver.quit()
        return data

    except Exception as e:
        print("An error occurred while fetching matchups:", e)
