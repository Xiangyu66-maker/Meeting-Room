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

if not API_KEY:
    raise RuntimeError("KKRICH_API_KEY is missing. Please set it in the .env file.")

client = OpenAI(
    api_key=API_KEY,
    base_url=BASE_URL
)

app = FastAPI(
    title="Unity AI Guidance Backend",
    description="FastAPI backend for AI Agent guidance in an immersive Unity puzzle game.",
    version="0.2"
)


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


class GuideResponse(BaseModel):
    instruction: str
    model: str
    status: str
    vision_text: Optional[str] = None
    latency_ms: Optional[int] = None


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


def analyze_image_with_vlm(image_base64: Optional[str], objects_text: str) -> str:
    if not image_base64:
        return "No visual input provided."

    try:
        response = client.chat.completions.create(
            model=VLM_MODEL_NAME,
            messages=[
                {
                    "role": "system",
                    "content": (
                        "You are a vision perception module for a Unity 3D puzzle game. "
                        "Your job is to describe task-relevant visual information, "
                        "not to give final player instructions."
                    )
                },
                {
                    "role": "user",
                    "content": [
                        {
                            "type": "text",
                            "text": f"""
Analyze this Unity game camera image for puzzle-game task guidance.

Unity structured object data:
{objects_text}

Your job is to generate a concise task-relevant visual summary.
Do not give the final player instruction.

Focus only on:
1. Visible task-related objects
2. Readable text, signs, symbols, numbers, or UI prompts
3. Interactable objects
4. Visible object states, such as locked, closed, opened, available
5. Relationships between important objects
6. Whether the image matches Unity object states
7. Possible clues related to the current task
8. Whether the Unity recommended target object is visible in the current camera image

Important rules:
- Unity object state is the ground truth.
- Do not override Unity state based only on the image.
- Do not claim an object is visible unless it is clearly visible in the image.
- If the recommended target is not visible, say it is not visible or uncertain.
- Do not solve the puzzle directly.
- Do not give the final player instruction.
- Do not give movement directions.
- Do not describe irrelevant background details.
- Keep the answer short and factual.

Return using this format:

Visible objects:
- ...

Readable text / symbols:
- ...

Interactable elements:
- ...

Task-related clues:
- ...

Unity state alignment:
- ...

Recommended target visibility:
- target_object_id: visible / not visible / uncertain
- evidence: ...
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

        return response.choices[0].message.content.strip()

    except Exception as e:
        print("VLM error:", str(e))
        return "Visual analysis unavailable."


def build_guidance_prompt(
    request: GuideRequest,
    objects_text: str,
    vision_text: str
) -> str:
    current_task = (
        request.current_task.strip()
        if request.current_task
        else "Explore the room and look for useful clues."
    )

    return f"""
You are an in-game AI assistant inside a Unity 3D immersive puzzle game.

You act like a friendly game companion helping the player understand the scene and decide the next useful action.

Your job is to give ONE natural game-style guidance sentence based on:
- the current task
- Unity structured objects
- visual information from the camera image
- object states

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

---

Player position:
({request.player_position.x}, {request.player_position.z})

Current task:
{current_task}

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


@app.get("/health")
def health_check():
    return {
        "status": "ok",
        "message": "Unity AI guidance backend is running.",
        "model": MODEL_NAME,
        "vlm_model": VLM_MODEL_NAME
    }


@app.post("/guide", response_model=GuideResponse)
def generate_guidance(request: GuideRequest):
    start_time = time.time()

    objects_text = build_objects_text(request.objects)
    vision_text = analyze_image_with_vlm(request.image_base64, objects_text)
    prompt = build_guidance_prompt(request, objects_text, vision_text)

    try:
        response = client.chat.completions.create(
            model=MODEL_NAME,
            messages=[
                {
                    "role": "system",
                    "content": (
                        "You are a Unity 3D in-game assistant. "
                        "Help the player understand the scene, notice clues, "
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

        return GuideResponse(
            instruction=instruction,
            model=MODEL_NAME,
            status="success",
            vision_text=vision_text,
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
