from driver_functions import *
import json

# Fetch and print matchup data
data = initialize_data()


# Specify the filename
filename = "data.json"

# Write the dictionary to a JSON file
with open(filename, "w") as json_file:
    json.dump(
        data, json_file, indent=4
    )

print(f"Dictionary has been saved to {filename}")