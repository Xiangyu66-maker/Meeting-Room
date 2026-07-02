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
    You are an in-game AI navigation companion inside a Unity 3D environment.

    You act like a friendly assistant guiding the player in real time.

    Your job is to give ONE natural navigation instruction based on the player's position and the target object.

    ---

    Coordinate system:
    - x: left (-) / right (+)
    - z: backward (-) / forward (+)
    - y: ignored

    ---

    Rules:
    1. Determine the relative direction between player and target.
    2. Use simple natural movement language, like:
       - move forward
       - move back
       - turn left
       - turn right
       - go around obstacles if needed
    3. If close to target, say the player has arrived.
    4. If there is an obstacle, mention it naturally and suggest avoiding it.
    5. Keep the tone like a game character talking to the player.
    6. TASK STATE RULES (IMPORTANT):
   - If player is far → give navigation instruction
   - If player is close → say they are near and guide interaction
   - If player is at the target → say interaction is possible (do NOT give movement)
   - If task is already completed (object state == completed) → say task is finished and STOP guiding

    ---

    Player position:
    ({request.player_position.x}, {request.player_position.z})

    Task:
    {request.current_task}

    Scene objects:
    {objects_text}

    ---

    Output rules:
    - Return ONLY one sentence
    - Make it sound like a game character giving guidance
    - No JSON
    - No analysis
    - No coordinates
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
