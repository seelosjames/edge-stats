# scraper.py

import requests
import time
from bs4 import BeautifulSoup
from config import TARGET_URLS, DEFAULT_HEADERS, REQUEST_DELAY
from parsers import parse_odds_page

def fetch_page(url):
    """Fetch the HTML content of a given URL with basic error handling."""
    try:
        response = requests.get(url, headers=DEFAULT_HEADERS)
        response.raise_for_status()
        return response.text
    except requests.exceptions.RequestException as e:
        print(f"Error fetching {url}: {e}")
        return None

def scrape_sportsbook():
    """Main function to scrape sportsbook odds from defined URLs."""
    for name, url in TARGET_URLS.items():
        print(f"Scraping {name} from {url}")
        html_content = fetch_page(url)
        if html_content:
            data = parse_odds_page(html_content)
            print(f"Scraped data from {name}: {data}")
        time.sleep(REQUEST_DELAY)  # Delay between requests
