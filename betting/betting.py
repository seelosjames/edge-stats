def american_to_decimal(american_odds):
    if american_odds > 0:
        return american_odds / 100 + 1
    elif american_odds < 0:
        return 1 - 100 / american_odds
    else:
        raise ValueError("American odds cannot be zero.")


def decimal_to_american(decimal_odds):
    if decimal_odds >= 2.0:
        return (decimal_odds - 1) * 100
    elif 1 < decimal_odds < 2.0:
        return -100 / (decimal_odds - 1)
    else:
        raise ValueError("Decimal odds must be greater than 1.")


def decimal_to_percentage(decimal_odds):
    if decimal_odds > 1:
        return round((100 / decimal_odds), 4)
    else:
        raise ValueError("Decimal odds must be greater than 1.")


def percentage_to_decimal(percentage):
    if 0 < percentage <= 100:
        return 100 / percentage
    else:
        raise ValueError("Percentage must be between 0 and 100.")


def decimal_to_fraction(decimal_odds):
    from fractions import Fraction

    if decimal_odds > 1:
        fractional_odds = Fraction(decimal_odds - 1).limit_denominator()
        return fractional_odds
    else:
        raise ValueError("Decimal odds must be greater than 1.")


def fraction_to_decimal(fraction):
    if fraction > 0:
        return fraction + 1
    else:
        raise ValueError("Fractional odds must be greater than 0.")


def american_to_percentage(american_odds):
    decimal_odds = american_to_decimal(american_odds)
    return decimal_to_percentage(decimal_odds)


def percentage_to_american(percentage):
    decimal_odds = percentage_to_decimal(percentage)
    return decimal_to_american(decimal_odds)


def fraction_to_american(fraction):
    decimal_odds = fraction_to_decimal(fraction)
    return decimal_to_american(decimal_odds)


def american_to_fraction(american_odds):
    decimal_odds = american_to_decimal(american_odds)
    return decimal_to_fraction(decimal_odds)


def percentage_to_fraction(percentage):
    decimal_odds = percentage_to_decimal(percentage)
    return decimal_to_fraction(decimal_odds)


def fraction_to_percentage(fraction):
    decimal_odds = fraction_to_decimal(fraction)
    return decimal_to_percentage(decimal_odds)
