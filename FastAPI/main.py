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


class GuideResponse(BaseModel):
    instruction: str
    model: str
    status: str


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

    prompt = f"""
You are a Unity 3D navigation assistant for a meeting room.

Your task is to generate one short movement instruction from the player's current position to the target object.

Use the coordinates strictly to decide the direction.

Coordinate rule:
- This is a Unity world coordinate system, not an image coordinate system.
- x means left and right.
- z means forward and backward.
- y means height and should be ignored for basic navigation.
- Larger x means more to the right.
- Smaller x means more to the left.
- Larger z means more forward in the room.
- Smaller z means more backward in the room.

Direction rule:
- If target_x > player_x, tell the player to turn or move right.
- If target_x < player_x, tell the player to turn or move left.
- If target_z > player_z, tell the player to move forward.
- If target_z < player_z, tell the player to move back.
- If both x and z are different, combine the directions.
- Example: player (0, 4), target (2, -2) means move back and turn right.
- Example: player (-1, -4), target (2, -2) means move forward and turn right.
- Example: player (2, -4), target (-2, 3) means move forward and turn left.

Player position:
x = {request.player_position.x}
y = {request.player_position.y}
z = {request.player_position.z}

Current task:
{request.current_task}

Scene objects:
{objects_text}

Output requirements:
- Return only one short instruction sentence.
- Mention the target object by name.
- Use simple words such as "move forward", "move back", "turn left", "turn right".
- Do not mention coordinates.
- Do not use Markdown.
- Do not explain your reasoning.
"""

    try:
        response = client.chat.completions.create(
            model=MODEL_NAME,
            messages=[
                {
                    "role": "system",
                    "content": "You are a strict Unity 3D navigation assistant. Follow the coordinate rules exactly."
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
            status="success"
        )

    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"Model API call failed: {str(e)}"
        )