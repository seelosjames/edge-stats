from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.chrome.options import Options
from webdriver_manager.chrome import ChromeDriverManager
import time
import re

data = {}


def get_odds(link):
    try:
        driver.get(link)
        time.sleep(1)
        
        driver.find_element(By.XPATH, '//*[@id="root"]/div[1]/div[2]/main/div[3]/div[1]/div[1]/div[1]/div/button').click()
        time.sleep(2)
        
        away_team = driver.find_element(By.XPATH, '//*[@id="root"]/div[1]/div[2]/main/div[1]/div[2]/div[3]/div[2]/div/label').text
        home_team = driver.find_element(By.XPATH, '//*[@id="root"]/div[1]/div[2]/main/div[1]/div[2]/div[4]/div[2]/div/label').text
        
        game = f'{away_team} vs {home_team}'
        
        lenth = len(driver.find_elements(By.XPATH, '//*[@id="root"]/div[1]/div[2]/main/div[3]/div'))
        player_props = f'//*[@id="root"]/div[1]/div[2]/main/div[3]/div[{str(lenth)}]'
        num_props = len(driver.find_elements(By.XPATH, f'{player_props}/div'))
        
        game_data = {}
        
        for i in range(1, num_props + 1):
            info = driver.find_element(By.XPATH, f'{player_props}/div[{str(i)}]/div[1]/span[1]').text
            name = info.split('(')[0].strip()
            item = info.split('(')[1].rstrip(')')
            over_number = re.search(r"\d+(\.\d+)?", driver.find_element(By.XPATH, f'{player_props}/div[{str(i)}]/div[2]/div/div/div[1]/button/span[1]').text).group()
            over_odds = driver.find_element(By.XPATH, f'{player_props}/div[{str(i)}]/div[2]/div/div/div[1]/button/span[2]').text
            under_number = re.search(r"\d+(\.\d+)?", driver.find_element(By.XPATH, f'{player_props}/div[{str(i)}]/div[2]/div/div/div[2]/button/span[1]').text).group()
            under_odds = driver.find_element(By.XPATH, f'{player_props}/div[{str(i)}]/div[2]/div/div/div[2]/button/span[2]').text
            # re.search(r"\d+(\.\d+)?",
            
            player = f'{name} {item}'
            
            game_data[player] = {
                "Pinnacle": {
                    "over_number": over_number,
                    "over_odds": over_odds,
                    "under_number": under_number,
                    "under_odds": under_odds,
                }
            }

            # print("Name:", name)
            # print("Item:", item)
            # print("Over Num:", over_number)
            # print("Over Odds:", over_odds)
            # print("Under Num:", under_number)
            # print("Under Odds:", under_odds)
            # print()
            
            
            
        data[game] = game_data
        
        
        
        # Add data extraction code here
    except Exception as e:
        print(f"Error while processing link {link}: {e}")
        
        
# Configure Chrome options
chrome_options = Options()
# Uncomment the next line to run the browser in headless mode (without UI)
# chrome_options.add_argument("--headless")
chrome_options.add_argument("--disable-gpu")
chrome_options.add_argument("--no-sandbox")

# Initialize the Chrome WebDriver with the configured options
service = Service(ChromeDriverManager().install())
driver = webdriver.Chrome(service=service, options=chrome_options)

try:
    # Navigate to the target website containing matchups
    url = "https://www.pinnacle.com/en/basketball/nba/matchups/#all"
    driver.get(url)
    

    # Wait for the page to load (adjust sleep time based on network conditions)
    time.sleep(1)
    # get_odds('https://www.pinnacle.com/en/basketball/nba/portland-trail-blazers-vs-los-angeles-lakers/1601576438/')
    

    # Locate the container with game matchups
    games = driver.find_elements(By.XPATH, '//*[@id="root"]/div[1]/div[2]/main/div/div[4]/div[2]/div/div')
    links = []

    # Iterate through the matchups starting from the third element (if applicable)
    for i in range(3, len(games) + 1):
        game = driver.find_element(By.XPATH, f'//*[@id="root"]/div[1]/div[2]/main/div/div[4]/div[2]/div/div[{i}]')
        
        # Extract the hyperlink for each game matchup
        link = game.find_element(By.TAG_NAME, 'a').get_attribute("href")
        links.append(link)  # Print the link (can be removed in production)
        print(link)


    # Process the odds from the retrieved link
    for link in links:
        get_odds(link)
    

except Exception as e:
    # Print any errors that occur during the execution
    print("An error occurred:", e)

finally:
    # Ensure the WebDriver quits properly
    driver.quit()


print(data)