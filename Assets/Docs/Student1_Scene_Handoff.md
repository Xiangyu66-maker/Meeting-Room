# Student 1 Scene Handoff

## 1. Scene

Scene name: `ConferenceRoom_before_blockout_sync`

Scene path: `Assets/Scenes/ConferenceRoom_before_blockout_sync.unity`

This scene adapts the existing conference room into a first-person puzzle-style meeting room escape environment. The player is locked in the meeting room, must inspect clues, infer a four-digit keypad code, and unlock the exit door.

## 2. Puzzle Summary

The puzzle uses three clue layers:

1. `whiteboard_01` gives the symbol set and the instruction: the meeting order decides the exit.
2. `seat_card_01` to `seat_card_04` define the symbol order on the meeting table.
3. `remote_01` should activate the hidden mapping clue on `screen_01`.

The final interaction target is `keypad_01`, which should unlock `locked_door_01` and allow the player to reach `victory_exit_point`.

## 3. Preserved Object IDs

The existing conference room layout and object IDs were preserved:

`room_shell_01`, `environment_01`, `furniture_group_01`, `meeting_table_01`, `table_target_area`, `cabinet_01`, `cabinet_door_01`, `cabinet_handle_01`, `chair_01` to `chair_10`, `computer_desk_01`, `desktop_computer_01`, `monitor_01`, `monitor_screen_01`, `keyboard_01`, `mouse_01`, `computer_tower_01`, `cable_panel_01`, `presentation_setup_01`, `screen_01`, `screen_pull_cord_01`, `projector_01`, `projector_beam_01`, `projector_cable_01`, `remote_01`, `document_01`, `document_02`, `conference_speaker_01`, `plant_01` to `plant_04`, `window_01` to `window_04`.

`remote_01` remains on the meeting table to avoid disrupting the existing table layout. It is documented as the object that should trigger `screen_01` activation.

## 4. Newly Added Object IDs

The following puzzle objects were added under `Escape Puzzle Additions`:

| object_id | Role |
|---|---|
| `locked_door_01` | Locked meeting room exit; should trigger "door inspected". |
| `keypad_01` | Final password input object; should trigger "password input". |
| `whiteboard_01` | First clue; should trigger "whiteboard observed". |
| `seat_card_01` | Seat clue: Seat 1 = circle. |
| `seat_card_02` | Seat clue: Seat 2 = star. |
| `seat_card_03` | Seat clue: Seat 3 = triangle. |
| `seat_card_04` | Seat clue: Seat 4 = square. |
| `player_start` | Recommended spawn marker inside the meeting room. |
| `victory_exit_point` | Recommended win/exit marker beyond the locked door. |

All newly added puzzle objects have `ObjectIdentity.object_id` and BoxCollider components.

## 5. Puzzle Clue Locations

`whiteboard_01` is on the entry-side wall. In the Unity scene TextMesh it displays the four symbols triangle, circle, square, star, followed by:

```text
The meeting order decides the exit.
```

Seat cards are on `meeting_table_01`:

```text
seat_card_01: Seat 1 = circle
seat_card_02: Seat 2 = star
seat_card_03: Seat 3 = triangle
seat_card_04: Seat 4 = square
```

`screen_01` has an inactive child GameObject named `Screen Symbol Number Mapping Clue`. Student 2 or the interaction layer should activate it when `remote_01` is used. It displays:

```text
triangle = 4
circle = 3
square = 2
star = 1
```

`document_01`, `document_02`, `desktop_computer_01`, and `plant_04` are preserved as environmental context or distractors.

## 6. Developer-Only Password Derivation

Do not show this final password directly in the player-facing environment.

Whiteboard order symbols:

```text
triangle, circle, square, star
```

Seat card meeting order:

```text
Seat 1 = circle
Seat 2 = star
Seat 3 = triangle
Seat 4 = square
```

Screen mapping:



Developer-only solution:

```text
circle, star, triangle, square = 3, 1, 4, 2
Password = 3142
```

## 7. Notes For Student 2: HTG / Puzzle Graph

Use these object IDs as primary graph nodes:

`locked_door_01`, `keypad_01`, `whiteboard_01`, `seat_card_01`, `seat_card_02`, `seat_card_03`, `seat_card_04`, `remote_01`, `screen_01`.

Suggested stage progression:

1. Door/keypad inspected.
2. Whiteboard observed.
3. Seat cards observed.
4. Remote used.
5. Screen mapping observed.
6. Correct keypad input.
7. Door opened and victory exit reached.

Suggested interaction events:

```text
locked_door_01 -> door inspected
keypad_01 -> password input
whiteboard_01 -> whiteboard observed
seat_card_01..seat_card_04 -> seat cards observed
remote_01 -> screen activated
screen_01 -> screen mapping observed
```

## 8. Notes For Student 3: Guidance Actions

These object IDs should support guidance actions:

```text
highlight whiteboard_01
highlight seat_card_01
highlight seat_card_02
highlight seat_card_03
highlight seat_card_04
highlight remote_01
highlight screen_01
highlight keypad_01
show arrow to target object
dim irrelevant distractors: document_01, document_02, desktop_computer_01, plant_04
```

Avoid highlighting the final password directly. Hints should guide attention to clue objects rather than revealing `3142`.

## 9. Notes For Student 4: VLM State JSON

Include these high-level state fields in VLM diagnosis output:

```json
{
  "current_stage": "",
  "inspected_objects": [],
  "visible_objects": [],
  "recent_interactions": [],
  "failed_keypad_attempts": 0,
  "time_in_current_stage": 0,
  "candidate_target_objects": []
}
```

Important candidate target IDs:

`locked_door_01`, `keypad_01`, `whiteboard_01`, `seat_card_01`, `seat_card_02`, `seat_card_03`, `seat_card_04`, `remote_01`, `screen_01`, `document_01`, `document_02`, `desktop_computer_01`, `plant_04`.

## 10. Recommended Screenshot Views

Recommended screenshot views for reports and VLM prompts:

1. Top-down object ID map.
2. First-person view from `player_start`.
3. View of `whiteboard_01`.
4. View of `meeting_table_01` and `seat_card_01` to `seat_card_04`.
5. View of `remote_01` and `screen_01`.
6. View of `locked_door_01` and `keypad_01`.

## 11. Validation

`SceneObjectIdValidator.cs` was added at:

`Assets/Scripts/SceneObjectIdValidator.cs`

A `Scene Object ID Validator` GameObject was added under `Escape Puzzle Additions`. It checks for:

`locked_door_01`, `keypad_01`, `whiteboard_01`, `seat_card_01`, `seat_card_02`, `seat_card_03`, `seat_card_04`, `remote_01`, `screen_01`, `meeting_table_01`, `cabinet_01`, `player_start`, `victory_exit_point`.
