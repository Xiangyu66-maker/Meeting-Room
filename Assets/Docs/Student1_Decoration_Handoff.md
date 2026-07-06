# Student 1 Decoration Handoff

Scene: `Assets/Scenes/ConferenceRoom_before_blockout_sync.unity`

This update adds visual decoration objects to the existing meeting room scene. These objects are decoration-only for now. They do not implement interaction logic, inventory logic, AI hints, VLM/API logic, or password behavior.

## Added Decoration Object IDs

| Object ID | Location | Purpose |
| --- | --- | --- |
| `wall_poster_01` | Visible wall near the whiteboard side of the room. | Office poster with `Innovation Meeting` text and simple graphic elements. Future clue carrier. |
| `clock_01` | Wall above the meeting area, visible from the table. | Static wall clock with simple hands and hour marks. Future number clue carrier. |
| `meeting_agenda_01` | Wall near the whiteboard/screen clue area. | Agenda board with Opening, Discussion, Decision, Exit. Future meeting-order hint carrier. |
| `sticky_note_01` | Side wall near the cabinet area. | Small yellow sticky note with `Check later`. Future note clue carrier. |
| `company_logo_wall_01` | Upper visible wall area. | Fictional `SURF LAB` logo wall decoration. Future symbol carrier. |
| `whiteboard_marker_01` | Near the lower edge of `whiteboard_01`. | Whiteboard marker decoration and possible distractor. |
| `whiteboard_eraser_01` | Near the lower edge of `whiteboard_01`. | Whiteboard eraser decoration and possible distractor. |
| `access_notice_01` | Near `locked_door_01` and `keypad_01`. | Door access notice reading `4-digit access code required`. |

## ObjectIdentity Metadata

Each added decoration root object has an `ObjectIdentity` component:

| Object ID | Category | Description |
| --- | --- | --- |
| `wall_poster_01` | `decoration` | Wall poster for meeting room decoration and possible future clue carrier. |
| `clock_01` | `decoration` | Static wall clock decoration and possible future number clue carrier. |
| `meeting_agenda_01` | `decoration` | Meeting agenda board used as environmental decoration and possible order hint. |
| `sticky_note_01` | `decoration` | Small sticky note decoration and possible future backpack clue. |
| `company_logo_wall_01` | `decoration` | Company logo wall decoration for the meeting room. |
| `whiteboard_marker_01` | `decoration` | Whiteboard marker decoration. |
| `whiteboard_eraser_01` | `decoration` | Whiteboard eraser decoration. |
| `access_notice_01` | `decoration` | Door access notice sign near the keypad. |

## Notes For Teammates

- These objects are visual decorations only.
- No `InteractableObject` component was added to these decoration objects.
- Colliders were added where appropriate so the objects behave like normal physical scene props.
- Future puzzle, guidance, or VLM systems can reference these object IDs if they become clues later.
- Existing puzzle object IDs and password logic were preserved.
