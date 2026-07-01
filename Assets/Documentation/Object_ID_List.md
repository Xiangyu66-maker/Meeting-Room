# ConferenceRoom_before_blockout_sync Object ID List

Scene: `Assets/Scenes/ConferenceRoom_before_blockout_sync.unity`

Notes:
- Existing conference-room object IDs are preserved.
- New escape-puzzle objects are added under `Escape Puzzle Additions` and use `ObjectIdentity.object_id`.
- `screen_01` contains an inactive child named `Screen Symbol Number Mapping Clue`; activate it from `remote_01` interaction.
- Do not display the final password directly in the room; derive it from whiteboard, seat cards, and screen mapping.

| object_id | category | current GameObject | GameObject fileID | Transform fileID | local position | note |
|---|---|---|---:|---:|---|---|
| `room_shell_01` | room | Conference Room | 1650224853 | 1650224854 | `(-15.2728815, 5.3232884, -12.534252)` | Existing room shell / imported room root. |
| `environment_01` | room | Environment | 952334433 | 952334434 | `(0, 0, 0)` | Existing environment root. |
| `furniture_group_01` | group | Generated Furniture | 1557114090 | 1557114091 | `(0, 0, -6)` | Existing generated furniture root. |
| `meeting_table_01` | furniture | Large Conference Table | 1519642791 | 1519642792 | `(0, 0, 0)` | Main conference table. |
| `table_target_area` | task_area | Meeting Table Accessories | 8389489 | 8389490 | `(0, 0, 0)` | Existing centered table accessory group used as the table target area. |
| `cabinet_01` | furniture | Openable Cabinet | 7600000000 | 7600000001 | `(12.87, 0, 7.33)` | Openable side cabinet; optional exploration object. |
| `cabinet_door_01` | interactive_part | Cabinet Sliding Door | 7600000100 | 7600000101 | `(0, 0.92, 0.34)` | Sliding cabinet door moved by OpenableCabinet. |
| `cabinet_handle_01` | interactive_part | Cabinet Door Handle | 7600000120 | 7600000121 | `(-0.42, 0, 0.07)` | Visible handle on the sliding cabinet door. |
| `locked_door_01` | puzzle_exit | Locked Exit Door | 7800000100 | 7800000101 | `(5.88, 1.15, -4.45)` | Puzzle exit door; trigger: door inspected. |
| `keypad_01` | puzzle_input | Door Keypad | 7800000200 | 7800000201 | `(5.72, 1.18, -5.18)` | Final password entry object; trigger: password input. |
| `whiteboard_01` | puzzle_clue | Puzzle Whiteboard | 7800000300 | 7800000301 | `(-3.15, 2.05, -3.18)` | First clue object; trigger: whiteboard observed. |
| `seat_card_01` | puzzle_clue | Seat Card 01 | 7800000400 | 7800000401 | `(-1.85, 1.08, -7.02)` | Seat order clue: Seat 1 = circle; contributes first password symbol. |
| `seat_card_02` | puzzle_clue | Seat Card 02 | 7800000500 | 7800000501 | `(-0.62, 1.08, -7.02)` | Seat order clue: Seat 2 = star; contributes second password symbol. |
| `seat_card_03` | puzzle_clue | Seat Card 03 | 7800000600 | 7800000601 | `(0.62, 1.08, -7.02)` | Seat order clue: Seat 3 = triangle; contributes third password symbol. |
| `seat_card_04` | puzzle_clue | Seat Card 04 | 7800000700 | 7800000701 | `(1.85, 1.08, -7.02)` | Seat order clue: Seat 4 = square; contributes fourth password symbol. |
| `player_start` | spawn_marker | Player Start Marker | 7800000800 | 7800000801 | `(0, 0.08, -3.85)` | Recommended first-person spawn marker inside the meeting room. |
| `victory_exit_point` | exit_marker | Victory Exit Point | 7800000900 | 7800000901 | `(6.35, 0.08, -4.45)` | Recommended win/exit marker beyond the locked door. |
| `chair_01` | furniture | Conference Chair | 119337048 | 119337049 | `(-2.25, 0, -1.85)` | Front row, left 1. |
| `chair_02` | furniture | Conference Chair | 1575176793 | 1575176794 | `(-0.75, 0, -1.85)` | Front row, left 2. |
| `chair_03` | furniture | Conference Chair | 326599794 | 326599795 | `(0.75, 0, -1.85)` | Front row, right 2. |
| `chair_04` | furniture | Conference Chair | 717476028 | 717476029 | `(2.25, 0, -1.85)` | Front row, right 1. |
| `chair_05` | furniture | Conference Chair | 1341993764 | 1341993765 | `(-3.65, 0, 0)` | Left side chair. |
| `chair_06` | furniture | Conference Chair | 265705567 | 265705568 | `(3.65, 0, 0)` | Right side chair. |
| `chair_07` | furniture | Conference Chair | 754601251 | 754601252 | `(-2.25, 0, 1.85)` | Rear row, left 1. |
| `chair_08` | furniture | Conference Chair | 299238179 | 299238180 | `(-0.75, 0, 1.85)` | Rear row, left 2. |
| `chair_09` | furniture | Conference Chair | 682710445 | 682710446 | `(0.75, 0, 1.85)` | Rear row, right 2. |
| `chair_10` | furniture | Conference Chair | 1982990310 | 1982990311 | `(2.25, 0, 1.85)` | Rear row, right 1. |
| `computer_desk_01` | computer | Computer Desk | 1001751372 | 1001751373 | `(-4.9, 0, -0.25)` | Existing side computer desk. |
| `desktop_computer_01` | computer | Desktop Computer | 1870863660 | 1870863661 | `(0, 0, 0)` | Environmental context/distractor; not main puzzle target. |
| `monitor_01` | computer | Monitor Bezel | 1603007954 | 1603007955 | `(0, 1.43, -0.44)` | Monitor frame. |
| `monitor_screen_01` | computer | Blue Screen | 1492978973 | 1492978974 | `(0, 1.43, -0.505)` | Visible monitor screen. |
| `keyboard_01` | computer | Keyboard | 725937463 | 725937464 | `(-0.12, 0.86, 0.18)` | Keyboard on the computer desk. |
| `mouse_01` | computer | Mouse | 2085283713 | 2085283714 | `(0.72, 0.86, 0.2)` | Mouse on the computer desk. |
| `computer_tower_01` | computer | Computer Tower | 1652992944 | 1652992945 | `(0.98, 1.04, -0.18)` | Desktop tower. |
| `cable_panel_01` | computer | Cable Access Panel | 284349722 | 284349723 | `(0, 0.915, 0)` | Cable access panel on the main table. |
| `presentation_setup_01` | presentation | Presentation Setup | 1349389628 | 1349389629 | `(0, 0, 0)` | Existing presentation setup root. |
| `screen_01` | presentation | Projection Screen Surface | 1035640280 | 1035640281 | `(0, 2.75, -10.85)` | Main projection screen; hidden mapping clue child should activate after remote_01. |
| `screen_pull_cord_01` | presentation | Screen Pull Cord | 1239897209 | 1239897210 | `(2.72, 3.38, -10.7)` | Screen pull cord. |
| `projector_01` | presentation | Ceiling Projector | 46138338 | 46138339 | `(0, 6.409, -0.26)` | Ceiling mounted projector root. |
| `projector_beam_01` | presentation | Visible Projector Beam | 935997709 | 935997710 | `(0, 4.35, -7.99)` | Visible projector beam. |
| `projector_cable_01` | presentation | Projector Ceiling Cable | 2005354396 | 2005354397 | `(0, 0.42, 0)` | Existing projector ceiling cable. |
| `remote_01` | task_object | Presentation Remote | 450274123 | 450274124 | `(0.72, 1.002, -0.4)` | Existing table remote; trigger: screen activated. |
| `document_01` | distractor | Open Notebook Left | 357638901 | 357638902 | `(-1.55, 0.995, -0.42)` | Environmental distractor or optional note. |
| `document_02` | distractor | Open Notebook Right | 1295910833 | 1295910834 | `(1.42, 0.995, 0.38)` | Environmental distractor or optional note. |
| `conference_speaker_01` | table_object | Conference Speaker | 1891694450 | 1891694451 | `(0, 1.01, 0)` | Conference speaker on the table. |
| `plant_01` | plant | Left Back Floor Plant | 160433997 | 160433998 | `(-5.6, 0, -9.55)` | Left rear floor plant. |
| `plant_02` | plant | Right Back Floor Plant | 1157177702 | 1157177703 | `(5.35, 0, -9.45)` | Right rear floor plant. |
| `plant_03` | plant | Entry Side Floor Plant | 882623609 | 882623610 | `(5.55, 0, 2.4)` | Entry side floor plant. |
| `plant_04` | distractor | Small Table Plant | 942075663 | 942075664 | `(2.42, 0.9, 0.48)` | Small plant on table / distractor. |
| `window_01` | window | Left Window Front | 626372400 | 626372401 | `(-20.044, 2.48, -1.7)` | Left side front window. |
| `window_02` | window | Left Window Rear | 1552555601 | 1552555602 | `(-20.046, 2.48, -6.1)` | Left side rear window. |
| `window_03` | window | Right Window Front | 2056751108 | 2056751109 | `(13.84, 2.48, -1.7)` | Right side front window. |
| `window_04` | window | Right Window Rear | 1355144015 | 1355144016 | `(13.85, 2.48, -6.1)` | Right side rear window. |

