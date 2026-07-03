import base64
import json

image_path = "test.png"

with open(image_path, "rb") as f:
    image_base64 = base64.b64encode(f.read()).decode("utf-8")

request_data = {
    "player_position": {
        "x": 0,
        "y": 0,
        "z": 0
    },
    "current_task": "Find a way to unlock the exit door",
    "objects": [
        {
            "id": "locked_exit",
            "name": "Locked Exit",
            "type": "puzzle_object",
            "tag": "Door",
            "position": {
                "x": 2,
                "y": 0,
                "z": 1
            },
            "role": "exit",
            "state": "locked"
        },
        {
            "id": "keypad",
            "name": "Keypad",
            "type": "interactable",
            "tag": "PuzzleInput",
            "position": {
                "x": 1,
                "y": 0,
                "z": 1
            },
            "role": "unlock_device",
            "state": "available"
        }
    ],
    "image_base64": image_base64
}

with open("request.json", "w", encoding="utf-8") as f:
    json.dump(request_data, f, ensure_ascii=False, indent=2)

print("request.json has been created.")