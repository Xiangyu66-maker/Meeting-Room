# FastAPI AI Guidance Backend

This folder contains the FastAPI backend for the Unity meeting room AI guidance system.

The backend receives runtime data from Unity, including the player's position, current task, key scene objects, and optional camera screenshot. It then uses a VLM + LLM pipeline to analyze the scene and generate context-aware game guidance.

## Backend Overview

The backend provides a `/guide` API endpoint.

Unity sends:

- player position
- current task context
- key Unity object states
- optional camera screenshot as base64

FastAPI returns:

- AI-generated instruction
- model name
- request status
- optional VLM visual analysis text

## File Structure

```text
FastAPI/
├── main.py                 Main FastAPI backend
├── test_main.http          HTTP test file
└── README.md
这是环境设置，添加.env到根目录
KKRICH_API_KEY=your_api_key_here
KKRICH_MODEL=gpt-5.5
KKRICH_VLM_MODEL=gpt-5.5
KKRICH_BASE_URL=https://api.kkrich.ltd/v1
要求的依赖
pip install fastapi uvicorn python-dotenv openai pydantic
在终端输入这个开始运行
uvicorn main:app --reload
浏览器输入这个手动操作
http://127.0.0.1:8000/docs
例子request
{
  "player_position": {
    "x": 0,
    "y": 0,
    "z": 0
  },
  "current_task": "Search the room for hidden password clues.",
  "objects": [
    {
      "id": "whiteboard_01",
      "name": "Whiteboard",
      "type": "puzzle_clue",
      "tag": "Clue",
      "position": {
        "x": 1,
        "y": 0,
        "z": 2
      },
      "role": "task_clue",
      "state": "unobserved"
    },
    {
      "id": "keypad_01",
      "name": "Keypad",
      "type": "interactable",
      "tag": "PuzzleInput",
      "position": {
        "x": 4,
        "y": 0,
        "z": 1
      },
      "role": "unlock_device",
      "state": "available"
    }
  ],
  "image_base64": "optional_base64_image_string"
}
例子回复
{
  "instruction": "Start by checking the whiteboard; it may explain where the password clues are hidden.",
  "model": "gpt-5.5",
  "status": "success",
  "vision_text": "A whiteboard is visible in the meeting room and appears to be related to the current task."
}
AI处理过程
Unity task + objects + optional screenshot
↓
FastAPI /guide
↓
VLM analyzes the image and produces vision_text
↓
LLM combines current_task, Unity objects, and vision_text
↓
Final game-style instruction is generated
## 后续改进方向：结构化 VLM 输出

当前后端已经可以让 VLM 返回 `vision_text`，用于描述 Unity 截图中的关键内容。但目前的 `vision_text` 主要是自然语言段落，不同模型或不同请求下格式可能不完全一致。

后续可以将 VLM 输出改为结构化格式，例如：

```text
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
