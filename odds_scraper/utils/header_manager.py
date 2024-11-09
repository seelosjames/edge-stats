# utils/header_manager.py

from fake_useragent import UserAgent

def get_random_headers():
    """Return a randomized User-Agent header."""
    ua = UserAgent()
    return {
        "User-Agent": ua.random
    }
