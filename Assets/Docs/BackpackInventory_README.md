# Backpack Inventory Loop

Scene: `Assets/Scenes/ConferenceRoom_before_blockout_sync.unity`

This task adds a simple backpack / inventory loop for the meeting room escape puzzle.

## Controls

- Press `Tab` to open or close the backpack.
- Press `E` while looking at `cabinet_01` / `Openable Cabinet` to collect the cabinet note.
- The red dot on the `Bag` icon means there is an unread clue note.
- Opening the backpack and viewing the note clears the red dot.

## Cabinet Note

The cabinet note is collected only once.

Item:

- `itemId`: `note_first_digit`
- `itemName`: `Cabinet Note`
- `itemType`: `note`
- `content`: `The first digit of the password is 3.`

This note intentionally reveals only the first digit clue. It does not display the full password.

## Setup

`BackpackSetupHelper` runs automatically after the scene loads. It creates the runtime backpack manager/UI/input objects if missing and attaches `ClueNotePickup` to the cabinet object found by `ObjectIdentity.object_id == "cabinet_01"` or by the GameObject name `Openable Cabinet`.

No OpenAI, Gemini, VLM, AI hints, or PuzzleGraph logic is included in this backpack task.
