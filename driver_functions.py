from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.chrome.options import Options
from webdriver_manager.chrome import ChromeDriverManager
from betting_functions import *
import time
import re


def get_games():
    """
    Fetches today's NBA matchups and their associated links from Pinnacle's website.
    """

    # Configure Chrome options
    chrome_options = Options()
    chrome_options.add_argument("--disable-gpu")
    chrome_options.add_argument("--no-sandbox")
    # Uncomment the line below to run the browser in headless mode
    # chrome_options.add_argument("--headless")

    # Initialize the WebDriver
    service = Service(ChromeDriverManager().install())
    driver = webdriver.Chrome(service=service, options=chrome_options)

    # Dictionary to store matchup data
    data = {}
    pinnacle_links = []

    try:
        url = "https://www.pinnacle.com/en/basketball/nba/matchups/#all"
        driver.get(url)
        time.sleep(1)  # Allow the page to load

        # Locate all game containers
        games = driver.find_elements(
            By.XPATH, '//*[@id="root"]/div[1]/div[2]/main/div/div[4]/div[2]/div/div'
        )

        today = False

        # Loop through each game container to extract relevant data
        for i in range(1, len(games) + 1):
            game_element = driver.find_element(
                By.XPATH,
                f'//*[@id="root"]/div[1]/div[2]/main/div/div[4]/div[2]/div/div[{i}]',
            )

            game_class = game_element.get_attribute("class")

            if "TODAY" not in game_element.text and "dateBar" in game_class:
                today = False

            if "TODAY" in game_element.text:
                today = True

            if today and game_class == "row-u9F3b9WCM3 row-k9ktBvvTsJ":
                # Extract hyperlink and team names for the game
                link_container = game_element.find_element(By.TAG_NAME, "a")
                teams = link_container.find_elements(By.TAG_NAME, "span")
                matchup = f"{teams[0].text} vs {teams[1].text}"

                # Initialize matchup in data and add save the Pinnacle link
                data[matchup] = {}
                pinnacle_links.append(link_container.get_attribute("href"))

        # Pinnacle
        for link in pinnacle_links:
            get_pinnacle_odds(driver, data, link)
        driver.quit()

        # Fliff
        data = get_fliff_odds(data)

        return data

    except Exception as e:
        print("An error occurred while fetching matchups:", e)


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
