# First Playable Interaction Loop

Scene:

`Assets/Scenes/ConferenceRoom_before_blockout_sync.unity`

## How It Works

`InteractionSetupHelper` is attached to the scene helper object named `Scene Object ID Validator`. On Play Mode start it:

- Finds GameObjects with `ObjectIdentity`.
- Adds `InteractableObject` to important puzzle objects when missing.
- Adds safe BoxColliders when an important object has no collider.
- Adds `DoorController` to `locked_door_01`.
- Adds `KeypadController` to `keypad_01`.
- Adds `FirstPersonInteractor` to the active camera.

## Controls

- Look at an interactable object.
- Press `E` to interact.
- The Console logs `Interacted with: object_id | description`.

## Keypad

Interact with `keypad_01` to start password input mode.

- Number keys `0` to `9`: enter digits.
- `Backspace`: delete the last digit.
- `Enter`: submit.
- `Escape`: cancel input mode.

Correct password: `3142`

When the correct password is submitted, `DoorController.UnlockDoor()` is called on `locked_door_01`, and the door slides open.

## Notes

This is only the first playable interaction loop. It does not implement OpenAI, Gemini, VLM diagnosis, or the full PuzzleGraphManager.

`remote_01` currently logs that the remote was used. Later work should connect it to activating the inactive child clue on `screen_01` named `Screen Symbol Number Mapping Clue`.
