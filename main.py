from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.common.keys import Keys
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.chrome.options import Options
from webdriver_manager.chrome import ChromeDriverManager
import time

# Set up Chrome options
chrome_options = Options()
# chrome_options.add_argument("--headless")  # Run in headless mode
chrome_options.add_argument("--disable-gpu")
chrome_options.add_argument("--no-sandbox")

# Initialize the WebDriver
service = Service(ChromeDriverManager().install())
driver = webdriver.Chrome(service=service, options=chrome_options)

try:
    url = "https://www.pinnacle.com/en/basketball/nba/matchups/#all"
    driver.get(url)
    time.sleep(5)

    # today_div = driver.find_element(By.XPATH, '//*[@id="root"]/div[1]/div[2]/main/div/div[4]/div[2]/div')
    games = driver.find_elements(By.XPATH, '//*[@id="root"]/div[1]/div[2]/main/div/div[4]/div[2]/div/div')
    for i in range(3, len(games)):
        game = driver.find_elements(By.XPATH, '//*[@id="root"]/div[1]/div[2]/main/div/div[4]/div[2]/div/div[' + str(i) + ']')
        print('game', game)
        link = game.find_element(By.TAG_NAME, 'a').get_attribute("href")
        print(link)
    # print(len(games))
    
    # //*[@id="root"]/div[1]/div[2]/main/div/div[4]/div[2]/div/div[3]


    # elements = today_div.find_elements(By.TAG_NAME, "a")
    
    # print(len(elements))
    # print("Element Text:", today_div.text)
    # print(elements)

except Exception as e:
    print("An error occurred:", e)

finally:
    # Quit the driver
    driver.quit()
