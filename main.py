from driver_functions import *
import json
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.chrome.service import Service
from selenium.webdriver.chrome.options import Options
from webdriver_manager.chrome import ChromeDriverManager
from functions.betting import *
import time
import re




# Fetch and print matchup data
data = get_games()


# Specify the filename
filename = "data.json"

# # Write the dictionary to a JSON file
# with open(filename, "w") as json_file:
#     json.dump(
#         data, json_file, indent=4
#     )

# print(f"Dictionary has been saved to {filename}")
