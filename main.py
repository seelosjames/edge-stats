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


amount = 10
odds = 200
total_cost = 0
bets = 0
win = 0
lost = 0

for _ in range(100_000):
    bank_account = 100
    amount = 10

    while bank_account > 0 and bank_account < 200 and bets < 1_000_000:
        bets += 1
        won = False
        while not won:
            if bank_account <= 0: break
            if bank_account - amount < 0:
                amount = bank_account
            bank_account -= amount
            num = random.random()
            if num < american_to_percentage(odds):
                won = True
                bank_account += calculate_winnings(odds, amount) + amount
                amount = 10
            else:
                amount *= 2
            if bank_account >= 200: break
    if bank_account > 0:
        win += 1
    else:
        lost += 1
    



print("WON:", win)
print("LOST:", lost)

print("Chances of Suceeding:", win/100_000)


