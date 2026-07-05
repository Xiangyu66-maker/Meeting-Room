# Seat Card Guidance Trail

Scene: `Assets/Scenes/ConferenceRoom_before_blockout_sync.unity`

This rule-based guidance system helps the player find the meeting-table seat card clues without revealing the final keypad password.

## Behavior

- Target clue IDs: `seat_card_01`, `seat_card_02`, `seat_card_03`, `seat_card_04`.
- `SeatCardGuidanceManager` starts a 60-second timer when gameplay begins.
- If none of the target seat cards has been inspected after 60 seconds, it shows a world-space guidance line from the player feet to the center of the four seat cards.
- `SimpleObjectHighlighter` highlights the four seat card objects while the guidance is visible.
- Interacting with any seat card hides the trail and highlight.
- Interacting with all four seat cards completes this clue guidance permanently for the current run.

## Setup

`SeatCardGuidanceSetupHelper` runs automatically after scene load. It finds the player via the `Player` tag, then falls back to `Camera.main.transform.root`. It finds target clues by `ObjectIdentity.object_id`, ensures seat cards have colliders and `InteractableObject`, then creates or configures the manager object.

## Future Integration

The current trigger is a simple timeout: 60 seconds and no inspected seat cards. Student 2 or Student 4 can later replace this condition with Puzzle Graph or VLM bottleneck diagnosis while keeping the same guidance output methods: `ShowSeatCardGuidance()`, `HideSeatCardGuidance()`, and `CompleteSeatCardGuidance()`.
