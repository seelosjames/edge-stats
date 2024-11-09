# config.py

# Define target URLs for scraping
TARGET_URLS = {
    "example_sportsbook": "https://www.example.com/odds",
}

# Default headers for requests
DEFAULT_HEADERS = {
    "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36"
}

# Time delay between requests
REQUEST_DELAY = 1  # in seconds, adjust later for rate limiting
