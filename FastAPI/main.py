import json
import os
import time
from typing import List, Optional

from dotenv import load_dotenv
from fastapi import FastAPI, HTTPException
from openai import OpenAI
from pydantic import BaseModel, Field

load_dotenv()

API_KEY = os.getenv("KKRICH_API_KEY")
MODEL_NAME = os.getenv("KKRICH_MODEL", "gpt-5.5")
BASE_URL = os.getenv("KKRICH_BASE_URL", "https://api.kkrich.ltd/v1")
VLM_MODEL_NAME = os.getenv("KKRICH_VLM_MODEL", "gpt-5.5")

client = OpenAI(
    api_key=API_KEY,
    base_url=BASE_URL
)

app = FastAPI(
    title="Unity AI Guidance Backend",
    description="FastAPI backend for AI Agent guidance in an immersive Unity puzzle game.",
    version="0.3"
)

session_memory = {}

class Position(BaseModel):
    x: float
    y: Optional[float] = 0
    z: float


class SceneObject(BaseModel):
    id: str
    name: str
    type: Optional[str] = None
    tag: Optional[str] = None
    position: Position
    role: Optional[str] = None
    state: Optional[str] = None


class GuideRequest(BaseModel):
    player_position: Position
    current_task: str
    objects: List[SceneObject] = Field(default_factory=list)
    image_base64: Optional[str] = None


class VisionDiagnosis(BaseModel):
    visible_objects: List[str] = Field(default_factory=list)
    likely_current_focus: Optional[str] = None
    target_visible: bool = False
    target_confidence: float = 0.0
    visual_stage_hint: Optional[str] = None
    evidence: Optional[str] = None


class GuideResponse(BaseModel):
    instruction: str
    model: str
    status: str
    vision_text: Optional[str] = None
    vision_diagnosis: Optional[VisionDiagnosis] = None
    latency_ms: Optional[int] = None


def get_stage_from_task(current_task: str) -> str:
    if not current_task:
        return "unknown"

    task_lower = current_task.lower()

    if "current_stage:" in task_lower:
        try:
            return task_lower.split("current_stage:")[1].splitlines()[0].strip()
        except Exception:
            return "unknown"

    return "unknown"


def get_memory(session_id: str) -> dict:
    """
    Get memory for the current demo player.
    If memory does not exist, initialize it.
    """
    if session_id not in session_memory:
        session_memory[session_id] = {
            "last_stage": "unknown",
            "hint_count": 0,
            "recent_instructions": []
        }

    return session_memory[session_id]


def update_memory(
    session_id: str,
    current_stage: str,
    instruction: str
) -> None:

    memory = get_memory(session_id)

    if memory["last_stage"] != current_stage:
        memory["last_stage"] = current_stage
        memory["hint_count"] = 0
        memory["recent_instructions"] = []
    else:
        memory["hint_count"] += 1

    memory["recent_instructions"].append(instruction)
    memory["recent_instructions"] = memory["recent_instructions"][-2:]


def build_objects_text(objects: List[SceneObject]) -> str:
    if not objects:
        return "No structured Unity objects provided."

    lines = []

    for obj in objects:
        lines.append(
            f"- {obj.name}, id={obj.id}, "
            f"type={obj.type}, tag={obj.tag}, "
            f"position=({obj.position.x}, {obj.position.y}, {obj.position.z}), "
            f"role={obj.role}, state={obj.state}"
        )

    return "\n".join(lines)


def get_recommended_target_id(request: GuideRequest) -> str:
    for obj in request.objects:
        if obj.state == "next_recommended_target":
            return obj.id

    for line in request.current_task.splitlines():
        if "recommended_target_object:" in line:
            return line.split("recommended_target_object:", 1)[1].strip()

    return request.objects[0].id if request.objects else ""


def parse_json_object(text: str) -> dict:
    if not text:
        return {}

    cleaned = text.strip()
    if cleaned.startswith("```"):
        cleaned = cleaned.strip("`")
        if cleaned.lower().startswith("json"):
            cleaned = cleaned[4:].strip()

    start = cleaned.find("{")
    end = cleaned.rfind("}")
    if start >= 0 and end > start:
        cleaned = cleaned[start:end + 1]

    try:
        return json.loads(cleaned)
    except json.JSONDecodeError:
        return {}


def normalize_vision_diagnosis(data: dict) -> VisionDiagnosis:
    visible_objects = data.get("visible_objects") or []
    if isinstance(visible_objects, str):
        visible_objects = [item.strip() for item in visible_objects.split(",") if item.strip()]
    if not isinstance(visible_objects, list):
        visible_objects = []

    raw_target_visible = data.get("target_visible", False)
    if isinstance(raw_target_visible, str):
        target_visible = raw_target_visible.strip().lower() in {"true", "1", "yes", "visible"}
    else:
        target_visible = bool(raw_target_visible)

    try:
        target_confidence = float(data.get("target_confidence", 0.0))
    except (TypeError, ValueError):
        target_confidence = 0.0

    target_confidence = max(0.0, min(1.0, target_confidence))

    return VisionDiagnosis(
        visible_objects=[str(item) for item in visible_objects],
        likely_current_focus=data.get("likely_current_focus") or None,
        target_visible=target_visible,
        target_confidence=target_confidence,
        visual_stage_hint=data.get("visual_stage_hint") or None,
        evidence=data.get("evidence") or None,
    )


def format_vision_diagnosis(diagnosis: VisionDiagnosis) -> str:
    visible = ", ".join(diagnosis.visible_objects) if diagnosis.visible_objects else "none"
    focus = diagnosis.likely_current_focus or "unknown"
    stage = diagnosis.visual_stage_hint or "Unknown"
    evidence = diagnosis.evidence or "No visual evidence."
    return (
        f"visible_objects={visible}; "
        f"likely_current_focus={focus}; "
        f"target_visible={diagnosis.target_visible}; "
        f"target_confidence={diagnosis.target_confidence:.2f}; "
        f"visual_stage_hint={stage}; "
        f"evidence={evidence}"
    )


def analyze_image_with_vlm(image_base64: Optional[str], request: GuideRequest) -> VisionDiagnosis:
    if not image_base64:
        return VisionDiagnosis(evidence="No visual input provided.")

    target_id = get_recommended_target_id(request)
    known_object_ids = [obj.id for obj in request.objects]
    objects_text = build_objects_text(request.objects)

    try:
        response = client.chat.completions.create(
            model=VLM_MODEL_NAME,
            messages=[
                {
                    "role": "system",
                    "content": (
                        "You are a vision perception module for a Unity 3D puzzle game. "
                        "Return reliable structured JSON only. Do not solve the puzzle."
                    )
                },
                {
                    "role": "user",
                    "content": [
                        {
                            "type": "text",
                            "text": f"""
Analyze this Unity game camera image for adaptive task guidance.

Known Unity task context:
{request.current_task}

Known Unity objects:
{objects_text}

Current recommended target object id from Unity:
{target_id}

Known object ids:
{known_object_ids}

Return ONLY valid JSON with this exact schema:
{{
  "visible_objects": ["object_id_if_visible"],
  "likely_current_focus": "object_id_or_empty_string",
  "target_visible": true,
  "target_confidence": 0.0,
  "visual_stage_hint": "FindWhiteboard | SearchSeatClues | UseKeypad | EnterPassword | Completed | Unknown",
  "evidence": "short visual evidence"
}}

Rules:
- visible_objects should use known Unity object ids when possible.
- target_visible means the current recommended target appears clearly visible in the camera view.
- target_confidence must be between 0 and 1.
- If unsure, set target_visible=false and use low confidence.
- Do not reveal the final password.
- Do not output Markdown, comments, or extra text.
"""
                        },
                        {
                            "type": "image_url",
                            "image_url": {
                                "url": f"data:image/jpeg;base64,{image_base64}"
                            }
                        }
                    ]
                }
            ],
            stream=False
        )

        content = response.choices[0].message.content.strip()
        return normalize_vision_diagnosis(parse_json_object(content))

    except Exception:
        return VisionDiagnosis(evidence="Visual analysis unavailable.")


def build_guidance_prompt(
    request: GuideRequest,
    objects_text: str,
    vision_text: str,
    memory: dict
) -> str:
    current_task = (
        request.current_task.strip()
        if request.current_task
        else "Explore the room and look for useful clues."
    )

    recent_instructions = memory.get("recent_instructions", [])

    recent_instructions_text = (
        "\n".join([f"- {item}" for item in recent_instructions])
        if recent_instructions
        else "No recent AI instructions."
    )

    hint_count = memory.get("hint_count", 0)

    return f"""
You are an in-game AI assistant inside a Unity 3D immersive puzzle game.

You act like a friendly game companion helping the player understand the scene and decide the next useful action.

Your job is to give ONE natural game-style guidance sentence based on:
- the current task
- Unity structured objects
- visual information from the camera image
- object states
- short-term memory from previous AI instructions

---

Main goal:
Do NOT focus on route navigation.
Focus on task guidance, scene understanding, clues, interactable objects, and what the player should do next.

---

Information priority:
1. Unity object state is the ground truth for task progress.
2. Unity structured objects are the ground truth for known game objects.
3. Visual analysis is used to add scene context, such as visible clues, obstacles, objects, and interactable items.
4. If Unity data and visual analysis conflict, trust Unity object state first.
5. Short-term memory is used only to avoid repeated hints and adjust hint strength.

---

Task guidance rules:
1. If an object state is completed, say the task is complete and do not give further action for that object.
2. If the player needs to interact with an object, suggest the interaction naturally.
3. If the visual analysis shows a clue, mention it as a hint.
4. If the scene contains an important object related to the task, guide the player to inspect or use it.
5. If there is not enough information, suggest what the player should check next.
6. Do not give exact puzzle solutions unless the task is already completed.
7. Do not output movement directions unless clearly necessary.
8. If Unity provides a recommended target or current stage in current_task, follow that diagnosis.

Recommended target visibility rules:
1. If the visual analysis says the recommended target is visible, suggest inspecting or interacting with that visible target directly.
2. If the visual analysis says the recommended target is not visible, do not claim the target is visible or in front of the player.
3. If the recommended target is not visible, tell the player to look for that target or check the related area.
4. If the recommended target visibility is uncertain, give a cautious hint and mention the likely area instead of a precise visible object.
5. If Unity state and visual analysis conflict, trust Unity state as the task ground truth, but phrase the instruction carefully.

Hint strength policy:
1. Use a gentle hint when the player has just entered the current stage.
2. Use a normal hint when the player is progressing normally.
3. Use a stronger hint when time_in_current_stage_seconds is high or the player appears stuck.
4. Use a correction hint when failed_keypad_attempts is greater than 0.
5. A gentle hint should mention the relevant area or clue type without directly naming every answer.
6. A stronger hint may mention the recommended target object more directly.
7. A correction hint should help the player recover from a wrong action, such as re-checking the seat-card clues after a wrong keypad attempt.
8. Do not reveal the final password directly.
9. Do not repeat the exact same wording as a previous hint if previous guidance is available.

Short-term memory rules:
1. Use recent AI instructions to avoid repeating the same guidance.
2. Do not repeat the same wording as the previous two instructions.
3. If hint_count is 0, give a gentle and indirect hint.
4. If hint_count is 1, give a clearer hint.
5. If hint_count is 2 or more, give a stronger and more direct hint.
6. If the player remains in the same stage, gradually increase specificity.
7. If the previous instruction already mentioned the same target, do not repeat it exactly; add a more specific clue or next interaction.
8. Do not reveal the final password directly.

---

Player position:
({request.player_position.x}, {request.player_position.z})

Current task:
{current_task}

Short-term memory:
Hint count in current stage:
{hint_count}

Recent AI instructions:
{recent_instructions_text}

Unity structured objects:
{objects_text}

Visual analysis from camera:
{vision_text}

---

Output rules:
- Return ONLY one sentence.
- Make it sound like a game character speaking to the player.
- No JSON.
- No analysis.
- No coordinates.
- No step-by-step explanation.
"""


@app.get("/")
def home():
    return {
        "message": "FastAPI backend is running.",
        "model": MODEL_NAME,
        "vlm_model": VLM_MODEL_NAME,
        "base_url": BASE_URL
    }


@app.post("/guide", response_model=GuideResponse)
def generate_guidance(request: GuideRequest):
    start_time = time.time()
    session_id = "default_player"

    current_stage = get_stage_from_task(request.current_task)
    memory = get_memory(session_id)

    objects_text = build_objects_text(request.objects)
    vision_diagnosis = analyze_image_with_vlm(request.image_base64, request)
    vision_text = format_vision_diagnosis(vision_diagnosis)
    prompt = build_guidance_prompt(request, objects_text, vision_text, memory)

    try:
        response = client.chat.completions.create(
            model=MODEL_NAME,
            messages=[
                {
                    "role": "system",
                    "content": (
                        "You are a Unity 3D in-game assistant. "
                        "Help the player understand the scene, notice clues, "
                        "avoid repeating previous hints, "
                        "and decide the next useful interaction."
                    )
                },
                {
                    "role": "user",
                    "content": prompt
                }
            ],
            stream=False
        )

        instruction = response.choices[0].message.content.strip()
        latency_ms = int((time.time() - start_time) * 1000)

        update_memory(
            session_id=session_id,
            current_stage=current_stage,
            instruction=instruction
        )

        return GuideResponse(
            instruction=instruction,
            model=MODEL_NAME,
            status="success",
            vision_text=vision_text,
            vision_diagnosis=vision_diagnosis,
            latency_ms=latency_ms
        )

    except Exception as e:
        print("LLM error:", str(e))
        latency_ms = int((time.time() - start_time) * 1000)

        raise HTTPException(
            status_code=500,
            detail={
                "message": "Model API call failed.",
                "error": str(e),
                "latency_ms": latency_ms,
                "vision_text": vision_text
            }
        )
