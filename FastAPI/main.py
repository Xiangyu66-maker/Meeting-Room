import os
from typing import List, Optional

from dotenv import load_dotenv
from fastapi import FastAPI, HTTPException
from openai import OpenAI
from pydantic import BaseModel, Field

load_dotenv()

API_KEY = os.getenv("KKRICH_API_KEY")
MODEL_NAME = os.getenv("KKRICH_MODEL", "gpt-5.4-mini")
BASE_URL = os.getenv("KKRICH_BASE_URL", "https://api.kkrich.ltd/v1")
VLM_MODEL_NAME = os.getenv("KKRICH_VLM_MODEL", "gpt-5.4-mini")

client = OpenAI(
    api_key=API_KEY,
    base_url=BASE_URL
)

app = FastAPI(
    title="Unity AI Guidance Backend",
    description="FastAPI backend for AI Agent guidance in an immersive game.",
    version="0.1"
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

def analyze_image_with_vlm(image_base64: Optional[str]) -> str:
    if not image_base64:
        return "No visual input provided."

    try:
        response = client.chat.completions.create(
            model=VLM_MODEL_NAME,
            messages=[
                {
                    "role": "system",
                    "content": "You are a vision perception module for a Unity 3D puzzle game. Your job is to describe what is visible, not to give final instructions."
                },
                {
                    "role": "user",
                    "content": [
                        {
                            "type": "text",
                            "text": """
Analyze this Unity game camera image for task guidance and puzzle understanding.

Focus on:
- visible important objects
- possible clues or hints
- readable symbols, notes, signs, numbers, or text
- interactable objects
- task-related items
- object relationships, such as an item on a table or near a door
- anything that may help the player decide the next useful interaction

Do NOT give movement directions.
Do NOT solve the puzzle directly.
Do NOT describe irrelevant background details.

Return a short plain-text scene understanding summary.
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

    except Exception:
        return "Visual analysis unavailable."

@app.get("/")
def home():
    return {
        "message": "FastAPI backend is running.",
        "model": MODEL_NAME,
        "base_url": BASE_URL
    }


@app.post("/guide", response_model=GuideResponse)
def generate_guidance(request: GuideRequest):
    objects_text = ""

    if request.objects:
        for obj in request.objects:
            objects_text += (
                f"- {obj.name}, id={obj.id}, "
                f"type={obj.type}, tag={obj.tag}, "
                f"position=({obj.position.x}, {obj.position.y}, {obj.position.z}), "
                f"role={obj.role}, state={obj.state}\n"
            )
    else:
        objects_text = "No scene objects."
    vision_text = analyze_image_with_vlm(request.image_base64)


    prompt = f"""
    You are an in-game AI assistant inside a Unity 3D immersive game.

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

    ---

    Player position:
    ({request.player_position.x}, {request.player_position.z})

    Current task:
    {request.current_task}

    Unity structured objects:
    {objects_text}

    Visual analysis from camera:
    {vision_text}

    ---

    Output rules:
    - Return ONLY one sentence
    - Make it sound like a game character speaking to the player
    - No JSON
    - No analysis
    - No coordinates
    - No step-by-step explanation
    """

    try:
        response = client.chat.completions.create(
            model=MODEL_NAME,
            messages=[
                {
                    "role": "system",
                    "content": "You are a Unity 3D in-game assistant. Help the player understand the scene, notice clues, and decide the next useful interaction."
                },
                {
                    "role": "user",
                    "content": prompt
                }
            ],
            stream=False
        )

        instruction = response.choices[0].message.content.strip()

        return GuideResponse(
            instruction=instruction,
            model=MODEL_NAME,
            status="success",
            vision_text = vision_text
        )

    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"Model API call failed: {str(e)}"
        )
