from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.common.action_chains import ActionChains
from webdriver_manager.chrome import ChromeDriverManager
from betting_functions import *
from datetime import datetime, timedelta
import time
import hashlib
import json
import os

sports_links = {
    "nba": {
        "pinnacle": "https://www.pinnacle.com/en/basketball/nba/matchups/#all",
        "fliff": "https://sports.getfliff.com/channels?channelId=461",
    },
    "nhl": {
        "pinnacle": "https://www.pinnacle.com/en/hockey/nhl/matchups/#period:0",
        "fliff": "https://sports.getfliff.com/channels?channelId=481",
    },
    "nfl": {
        "pinnacle": "https://www.pinnacle.com/en/football/nfl/matchups/#all",
        "fliff": "https://sports.getfliff.com/channels?channelId=451",
    },
    "ncaaf": {
        "pinnacle": "https://www.pinnacle.com/en/football/ncaa/matchups/#all",
        "fliff": "https://sports.getfliff.com/channels?channelId=452",
    },
    "ncaab": {
        "pinnacle": "https://www.pinnacle.com/en/basketball/ncaa/matchups/#all",
        "fliff": "https://sports.getfliff.com/channels?channelId=462",
    },
    "epl": {
        "pinnacle": "https://www.pinnacle.com/en/soccer/england-premier-league/matchups/#all",
        "fliff": "https://sports.getfliff.com/channels?channelId=4385",
    },
    "bundesliga": {
        "pinnacle": "https://www.pinnacle.com/en/soccer/germany-bundesliga/matchups/#all",
        "fliff": "https://sports.getfliff.com/channels?channelId=4382",
    },
    "ligue_1": {
        "pinnacle": "https://www.pinnacle.com/en/soccer/france-ligue-1/matchups/#all",
        "fliff": "https://sports.getfliff.com/channels?channelId=4384",
    },
    "serie_a": {
        "pinnacle": "https://www.pinnacle.com/en/soccer/italy-serie-a/matchups/#all",
        "fliff": "https://sports.getfliff.com/channels?channelId=4388",
    },
    "laliga": {
        "pinnacle": "https://www.pinnacle.com/en/soccer/spain-la-liga/matchups/#all",
        "fliff": "https://sports.getfliff.com/channels?channelId=4395",
    },
}

# XPaths for Pinnacle elements
XPATH_DATE = '//*[@id="root"]/div[1]/div[2]/main/div[1]/div[2]/div[2]/div/span'
XPATH_TEAM_1 = (
    '//*[@id="root"]/div[1]/div[2]/main/div[1]/div[2]/div[3]/div[2]/div/label'
)
XPATH_TEAM_2 = (
    '//*[@id="root"]/div[1]/div[2]/main/div[1]/div[2]/div[4]/div[2]/div/label'
)


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


# Function to create a unique ID
def generate_id(team_1, team_2, date_time_str):
    combined_string = f"{team_1}_{team_2}_{date_time_str}"
    return hashlib.md5(combined_string.encode()).hexdigest()


# Function to load existing data from data.json
def load_existing_data(file_path="data.json"):
    if os.path.exists(file_path):
        try:
            with open(file_path, "r") as file:
                return json.load(file)
        except json.JSONDecodeError:
            # If the file is empty or invalid JSON, return an empty list
            print(f"{file_path} is empty or corrupted. Starting with an empty dataset.")
            return []
    return []


# Function to save data back to data.json
def save_data(data, file_path="data.json"):
    with open(file_path, "w") as file:
        json.dump(data, file, indent=4, default=str)


# Function to update or append game data
def update_or_add_game(data, new_game, book):
    new_game_link = new_game["links"][book]
    # Look for existing game by ID
    for existing_game in data:
        if existing_game["id"] == new_game["id"]:
            existing_game["links"][book] = new_game_link
            return  # Exit after updating the existing game
    # If no match, add the new game
    data.append(new_game)
    print(f"Added new game with ID: {new_game['id']}")


def get_games(
    sports=[
        "nba",
        "nfl",
        "nhl",
        "ncaaf",
        "ncaab",
        "epl",
        "bundesliga",
        "ligue_1",
        "serie_a",
        "laliga",
    ]
):
    # Dictionary to store matchup data
    data = load_existing_data()

    for sport in sports:
        get_pinnacle_games(sports_links[sport]["pinnacle"], data, sport)
        # get_fliff_games(sports_links[sport]["fliff"], data, sport)

    with open("data.json", "w") as f:
        json.dump(data, f, default=str, indent=4)


def get_pinnacle_games(url, data, sport):
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

            # Parse datetime
            date_time_obj = datetime.strptime(date_time_str, "%A, %B %d, %Y at %H:%M")
            if sport == "nba":
                date_time_obj = date_time_obj - timedelta(minutes=10)
            elif sport == "nhl":
                date_time_obj = date_time_obj - timedelta(minutes=7)

            game_id = generate_id(
                team_1, team_2, date_time_obj.strftime("%Y-%m-%d %H:%M:%S")
            )

            # Create new game object
            new_game = {
                "id": game_id,
                "game": {"teams": [team_1, team_2], "date": date_time_obj},
                "links": {sports_book_name: game_url},
                "league": sport,
            }

            # Update or add the game
            update_or_add_game(data, new_game, sports_book_name)

    except Exception as e:
        print(f"Error while processing link {url}: {e}")
    finally:
        driver.quit()


def get_fliff_games(url, data, sport):
    # Configure Chrome options
    mobile_emulation = {"deviceName": "iPhone 12 Pro"}
    chrome_options = webdriver.ChromeOptions()
    chrome_options.add_experimental_option("mobileEmulation", mobile_emulation)

    # Initialize the WebDriver
    driver = webdriver.Chrome(options=chrome_options)

    sports_book_name = "fliff"

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

                game_id = generate_id(
                    team_1, team_2, date_time_obj.strftime("%Y-%m-%d %H:%M:%S")
                )

                # Create new game object
                new_game = {
                    "id": game_id,
                    "game": {"teams": [team_1, team_2], "date": date_time_obj},
                    "league": sport,
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
            new_game["links"] = {sports_book_name: driver.current_url}

            # Update or add the game
            update_or_add_game(data, new_game, sports_book_name)

        driver.quit()

    except Exception as e:
        print("An error occurred while fetching matchups:", e)


# Example usage
if __name__ == "__main__":
    get_games(
        [
            # "nba",
            "nfl",
            # "nhl",
            # "ncaaf",
            # "ncaab",
            # "epl",
            # "bundesliga",
            # "ligue_1",
            # "serie_a",
            # "laliga",
        ]
    )


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


def get_pinnacle_odds(driver, data, link):
    try:
        driver.get(link)
        time.sleep(4)

        # Show all props
        driver.find_element(
            By.XPATH,
            '//*[@id="root"]/div[1]/div[2]/main/div[3]/div[1]/div[1]/div[1]/div/button',
        ).click()

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
        print(f"Error while processing link {link}: {e}")
