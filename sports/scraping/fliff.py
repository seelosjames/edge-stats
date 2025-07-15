from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.common.exceptions import NoSuchElementException
from selenium.common.exceptions import TimeoutException
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.common.action_chains import ActionChains
from datetime import datetime, timedelta
import time

from database.db import *
from scraping.helper import *
from betting.betting import *


def fliff_parse_string(term):
    parts = term.split(" - ", 1)  # Split at the first " - "
    return parts if len(parts) == 2 else (term, None)


def get_fliff_odds(conn, urls, sportsbook, league):
    # Set up driver
    mobile_emulation = {"deviceName": "iPhone 12 Pro"}
    chrome_options = webdriver.ChromeOptions()
    chrome_options.add_experimental_option("mobileEmulation", mobile_emulation)
    driver = webdriver.Chrome(options=chrome_options)


    for url_data in urls:
        game_id = url_data[0]
        url = url_data[1]
        
        print('Scraping Odds for game')
        
        if league == 'nba':
            get_fliff_nba_odds(driver, conn, game_id, url, sportsbook, league)
        elif league == 'nhl':
            get_fliff_nhl_odds(driver, conn, game_id, url, sportsbook, league)



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


def get_fliff_nhl_odds(driver, conn, game_id, url, sportsbook, league):
    try:
        driver.get(url)

        # Wait for tabs to load
        tabs = WebDriverWait(driver, 15).until(
            EC.presence_of_all_elements_located((By.CLASS_NAME, "tab-filter"))
        )

        for i in range(1, len(tabs)):
            tab_title = tabs[i].text
            
            if tab_title == "SUMMARY" or tab_title == 'BOOSTED' or tab_title == 'GAME PROPS' or tab_title == 'TEAM PROPS':
                continue
            tabs[i].click()
            time.sleep(1)
            driver.execute_script("window.scrollTo(0, 0);")

            try:
                # Wait for closed divs to load (if they exist)
                closed_divs = WebDriverWait(driver, 10).until(
                    EC.presence_of_all_elements_located(
                        (By.CLASS_NAME, "toggled-opened")
                    )
                )
                for div in closed_divs:
                    actions = ActionChains(driver)
                    actions.move_to_element(div).perform()
                    div.click()
            except TimeoutException:
                print("No closed divs found for this tab.")

            main_div = driver.find_element(By.CLASS_NAME, "more-markets")
            prop_divs = main_div.find_elements(By.XPATH, "div")
            
            

            for prop_div in prop_divs:
                if prop_div.get_attribute("class") == "more-markets-note":
                    continue
                elif prop_div.get_attribute("class") == "more-markets-title":
                    
                    if tab_title == "GAME LINES":
                        prop_name = normalize_prop_name(prop_div.find_element(By.XPATH, 'span').text)
                        prop_type = "Game"
                        prop_uuid = generate_prop_uuid(prop_name, prop_type, game_id)
                        
                        # print(f'Found {prop_type} prop: {prop_name} at {sportsbook}')
                        
                        # Insert prop into DB
                        prop_data = (prop_uuid, game_id, prop_type, prop_name)
                        prop_id = insert_prop(conn, prop_data)
                        
                    elif tab_title == "PLAYER PROPS":
                        parts = prop_div.find_element(By.XPATH, 'span').text.split("PLAYER ", 1)
                        prop = normalize_prop(parts[1] if len(parts) > 1 else parts[0])
                        prop_type = "Player Prop"
                        
                    elif tab_title == "PERIODS":
                        parts = prop_div.find_element(By.XPATH, 'span').text.split("PERIOD ", 1)
                        parts[0] += "PERIOD" if len(parts) > 1 else ""
                        prop_name = normalize_prop_name(parts[-1])
                        prop_type = normalize_prop_type(parts[0])
                        
                        prop_uuid = generate_prop_uuid(prop_name, prop_type, game_id)
                        
                        # print(f'Found {prop_type} prop: {prop_name} at {sportsbook}')
                        
                        # Insert prop into DB
                        prop_data = (prop_uuid, game_id, prop_type, prop_name)
                        prop_id = insert_prop(conn, prop_data)
                          
                else:
                    if tab_title == "PLAYER PROPS":
                        player_name = prop_div.find_element(By.XPATH, "p").text + " "
                        prop_name = player_name + prop
                        prop_uuid = generate_prop_uuid(prop_name, prop_type, game_id)
                        
                        # print(f'Found {prop_type} prop: {prop_name} at {sportsbook}')
                        
                        # Insert prop into DB
                        prop_data = (prop_uuid, game_id, prop_type, prop_name)
                        prop_id = insert_prop(conn, prop_data)
                
                    
                    odds_divs = prop_div.find_elements(By.XPATH, 'div')

                    for odds_div in odds_divs:
                        locked = False
                        try:
                            odds_div.find_element(By.CLASS_NAME, 'icon-lock')
                            locked = True
                        except:
                            pass
                        
                        if not locked:
                            line_info = odds_div.find_elements(By.TAG_NAME, 'span')
                            description = convert_number_to_float(line_info[0].text)
                            odd = american_to_percentage(float(line_info[1].text.replace("+", "").strip()))
                        
                            line_uuid = generate_line_uuid(prop_uuid, description, odd, sportsbook)
                            
                            # print(f'Found line: {description} at {odd}')
                    
                            # Insert line into DB
                            line_data = (prop_id, line_uuid, odd, description, sportsbook)
                            insert_line(conn, line_data)

    except Exception as e:
        print(f"An error occurred: {e}")

def get_fliff_nba_odds(driver, conn, game_id, url, sportsbook, league):
    try:
        driver.get(url)

        # Wait for tabs to load
        tabs = WebDriverWait(driver, 15).until(
            EC.presence_of_all_elements_located((By.CLASS_NAME, "tab-filter"))
        )

        for i in range(1, len(tabs)):
            if tabs[i].text == "SUMMARY" or tabs[i].text == 'BOOSTED' or tabs[i] == 'GAME PROPS':
                continue
            tabs[i].click()
            time.sleep(1)
            driver.execute_script("window.scrollTo(0, 0);")

            try:
                # Wait for closed divs to load (if they exist)
                closed_divs = WebDriverWait(driver, 10).until(
                    EC.presence_of_all_elements_located(
                        (By.CLASS_NAME, "toggled-opened")
                    )
                )
                for div in closed_divs:
                    actions = ActionChains(driver)
                    actions.move_to_element(div).perform()
                    div.click()
            except TimeoutException:
                print("No closed divs found for this tab.")

            main_div = driver.find_element(By.CLASS_NAME, "more-markets")
            prop_divs = main_div.find_elements(By.XPATH, "div")
            
            tab_title = tabs[i].text

            for prop_div in prop_divs:
                if prop_div.get_attribute("class") == "more-markets-title":
                    if tab_title == "GAME LINES":
                        prop_name = normalize_prop_name(prop_div.find_element(By.XPATH, 'span').text)
                        prop_type = "Game"
                        prop_uuid = generate_prop_uuid(prop_name, prop_type, game_id)
                        
                        # print(f'Found {prop_type} prop: {prop_name} at {sportsbook}')
                        
                        # Insert prop into DB
                        prop_data = (prop_uuid, game_id, prop_type, prop_name)
                        prop_id = insert_prop(conn, prop_data)
                        
                    elif tab_title == "PLAYER PROPS":
                        prop = normalize_prop(prop_div.find_element(By.XPATH, 'span').text.split(" ", 1)[1])
                        prop_type = "Player Prop"
                        
                    elif tab_title == "HALVES" or tab_title == "QUARTERS":
                        prop_title = fliff_parse_string(prop_div.find_element(By.XPATH, 'span').text)
                        prop_name = normalize_prop_name(prop_title[0])
                        prop_type = normalize_prop_type(prop_title[1])
                        prop_uuid = generate_prop_uuid(prop_name, prop_type, game_id)
                        
                        # print(f'Found {prop_type} prop: {prop_name} at {sportsbook}')
                        
                        # Insert prop into DB
                        prop_data = (prop_uuid, game_id, prop_type, prop_name)
                        prop_id = insert_prop(conn, prop_data)
                          
                else:
                    if tab_title == "PLAYER PROPS":
                        player_name = prop_div.find_element(By.XPATH, "p").text + " "
                        if player_name == "James Harden":
                            print()
                        prop_name = player_name + prop
                        prop_uuid = generate_prop_uuid(prop_name, prop_type, game_id)
                        
                        # print(f'Found {prop_type} prop: {prop_name} at {sportsbook}')
                        
                        # Insert prop into DB
                        prop_data = (prop_uuid, game_id, prop_type, prop_name)
                        prop_id = insert_prop(conn, prop_data)
                    
                    odds_divs = prop_div.find_elements(By.XPATH, 'div')

                    for odds_div in odds_divs:
                        locked = False
                        try:
                            odds_div.find_element(By.CLASS_NAME, 'icon-lock')
                            locked = True
                        except:
                            pass
                        
                        if not locked:
                            line_info = odds_div.find_elements(By.TAG_NAME, 'span')
                            description = convert_number_to_float(line_info[0].text)
                            odd = american_to_percentage(float(line_info[1].text.replace("+", "").strip()))
                        
                            line_uuid = generate_line_uuid(prop_uuid, description, odd, sportsbook)
                            
                            # print(f'Found line: {description} at {odd}')
                    
                            # Insert line into DB
                            line_data = (prop_id, line_uuid, odd, description, sportsbook)
                            insert_line(conn, line_data)

    except Exception as e:
        print(f"An error occurred: {e}")

def get_fliff_games(conn, url, sportsbook, league):
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
        games = WebDriverWait(driver, 15).until(
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

                team_1 = normalize_team_name(
                    game.find_element(By.XPATH, "div[2]/div[2]/div[1]/span").text
                )
                team_2 = normalize_team_name(
                    game.find_element(By.XPATH, "div[2]/div[3]/div[1]/span").text
                )

                game_uuid = generate_game_uuid(
                    team_1, team_2, date_time_obj.strftime("%Y-%m-%d")
                )

                # (game_uuid, league_id, team_1, team_2, game_date)
                game_data = (game_uuid, league, team_1, team_2, date_time_obj)

                new_game_data["game_data"] = game_data

                not_live.append(new_game_data)

        for game in not_live:
            i = game["index"]

            driver.get(url)

            # Wait for the game containers to be loaded
            games = WebDriverWait(driver, 15).until(
                EC.presence_of_all_elements_located(
                    (By.CLASS_NAME, "card-shared-container")
                )
            )

            # Find the specific game card header for the game
            card_header = games[i].find_element(By.CLASS_NAME, "card-row-header")
            card_footer = games[i].find_element(By.CLASS_NAME, "card-regular-footer")

            # Wait for the card header to be clickable
            WebDriverWait(driver, 15).until(EC.element_to_be_clickable(card_header))

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

            game_data = game["game_data"]

            print(f'Found {league} game: {game_data[2]} vs. {game_data[3]} on {game_data[4].strftime("%B %d, %Y, %I:%M %p")} on {sportsbook}')

            # Insert teams into DB
            team_1_data = (game_data[2], league)
            team_2_data = (game_data[3], league)

            insert_team(conn, team_1_data)
            insert_team(conn, team_2_data)

            # Insert game into DB
            game_data = (game_data[0], league, game_data[2], game_data[3], game_data[4])
            insert_game(conn, game_data)

            # Insert game_url into DB
            game_url_data = (game_data[0], sportsbook, game_url)
            insert_game_url(conn, game_url_data)

        return data

    except Exception as e:
        print("An error occurred while fetching matchups:", e)
    finally:
        driver.quit()
