from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from webdriver_manager.chrome import ChromeDriverManager
from betting_functions import *
import time
from datetime import datetime
import hashlib
import json

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
    },
    "ncaaf": {"pinnacle": "https://www.pinnacle.com/en/football/ncaa/matchups/#all"},
    "ncaab": {"pinnacle": "https://www.pinnacle.com/en/basketball/ncaa/matchups/#all"},
    "epl": {
        "pinnacle": "https://www.pinnacle.com/en/soccer/england-premier-league/matchups/#all"
    },
    "bundesliga": {
        "pinnacle": "https://www.pinnacle.com/en/soccer/germany-bundesliga/matchups/#all"
    },
    "ligue_1": {
        "pinnacle": "https://www.pinnacle.com/en/soccer/france-ligue-1/matchups/#all"
    },
    "serie_a": {
        "pinnacle": "https://www.pinnacle.com/en/soccer/italy-serie-a/matchups/#all"
    },
    "laliga": {
        "pinnacle": "https://www.pinnacle.com/en/soccer/spain-la-liga/matchups/#all"
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


# Function to create a unique ID
def generate_id(team_1, team_2, date_time_str):
    combined_string = f"{team_1}_{team_2}_{date_time_str}"
    return hashlib.md5(combined_string.encode()).hexdigest()


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
    data = []

    for sport in sports:
        get_pinnacle_games(sports_links[sport]["pinnacle"], data, sport)
        # get_fliff_games(sports_links[sport]["fliff"])

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
            game_id = generate_id(
                team_1, team_2, date_time_obj.strftime("%Y-%m-%d %H:%M:%S")
            )

            # Add to data if new
            if game_id not in {entry["id"] for entry in data}:
                data.append(
                    {
                        "id": game_id,
                        "game": {
                            "teams": [team_1, team_2],
                            "date": date_time_obj,
                            "league": sport,
                        },
                        "links": {sports_book_name: game_url},
                    }
                )

    except Exception as e:
        print(f"Error while processing link {url}: {e}")
    finally:
        driver.quit()


# Example usage
if __name__ == "__main__":
    get_games(
        [
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

        print("INDEXES", today_index)
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
