import math
import random
import time
import numpy as np


def american_to_percentage(odds):
    if odds >= 100:
        return 100 / (odds + 100)
    elif odds <= -100:
        return abs(odds) / (abs(odds) + 100)
    else:
        print("Invalid Number, cannot be between -100 and 100")
        return None


def percentage_to_american(probability):
    if probability == 0:
        return None
    elif probability > 0.5:
        return round((probability * 100) / (1 - probability) * -1)
    else:
        return round((100 / probability) - 100)


def calculate_winnings(odds, bet_amount):
    if odds > 0:
        winnings = (odds / 100) * bet_amount
    else:
        winnings = (100 / abs(odds)) * bet_amount

    return round(winnings, 2)


def parlay(odds_list):
    parlay_probability = 1
    for odds in odds_list:
        parlay_probability *= american_to_percentage(odds)
    return parlay_probability


# Simulate the batch betting strategy
def simulate_batch_betting(
    odds=-110,
    paid_odds=-110,
    goal=200,
    initial_balance=100,
    bet_percentage=0.025,
    bets_per_batch=10,
    iterations=10_000,
    max_bets=1_000_000,
):
    win = 0
    lost = 0
    bets_to_goal = []

    for _ in range(iterations):
        current_balance = initial_balance
        bets = 0

        while 0 < current_balance < goal and bets < max_bets:
            bets += 1
            bet_amount = (
                current_balance * bet_percentage
            )  # Bet a percentage of the current balance
            bet_amount = min(
                bet_amount, current_balance
            )  # Ensure bet doesn't exceed balance

            # Perform a batch of bets
            batch_winnings = 0
            for _ in range(bets_per_batch):
                if random.random() < american_to_percentage(odds):
                    batch_winnings += calculate_winnings(paid_odds, bet_amount) + bet_amount

            # Update balance with winnings (or losses)
            current_balance += batch_winnings - bet_amount * bets_per_batch

            # Break if the goal is reached
            if current_balance >= goal:
                bets_to_goal.append(bets)
                break

        # Track wins and losses
        if current_balance >= goal:
            win += 1
        else:
            lost += 1

    # Calculate average bets to reach the goal for successful sessions
    average_bets_to_goal = sum(bets_to_goal) / len(bets_to_goal) if bets_to_goal else 0

    # Return results as a tuple
    return win / iterations, average_bets_to_goal


simulate_batch_betting(
    odds=-135,
    paid_odds=-110,
    goal=200,
    initial_balance=100,
    bet_percentage=0.025,
    bets_per_batch=10,
    iterations=1,
    max_bets=1_000_000,
)
