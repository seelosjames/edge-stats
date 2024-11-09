# utils/rate_limiter.py

import time

def rate_limit(delay):
    """Pause for the specified delay to control request rate."""
    time.sleep(delay)
