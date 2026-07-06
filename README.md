下面是你可以**直接复制粘贴到 `FastAPI/README.md`** 的中文版本。这个版本已经按照你现在的后端代码来写，包括：VLM、LLM、目标可见性判断、提示强度、短期记忆、`/guide`、`/health`、`/memory` 等内容。你的代码里已经有 `session_memory`、`get_memory()`、`update_memory()` 和最近两次 instruction 记录逻辑，所以 README 里可以这样写。

````markdown
# FastAPI Backend for Embodied AI Guidance in Immersive Games

本文件夹包含项目 **Adaptive Task Planning and Procedural Guidance via Embodied AI Agents in Immersive Games** 的 FastAPI 后端代码。

后端会接收 Unity 沉浸式解密游戏环境中的运行时数据，包括玩家位置、当前任务阶段、结构化场景物体信息，以及可选的相机截图。随后，后端使用 VLM + LLM 流程分析当前场景，并为玩家生成自适应的程序化任务引导。

当前原型使用 Unity 会议室逃脱解密场景作为测试环境。

---

## 1. 功能概述

当前 FastAPI 后端支持以下功能：

- 通过 `/guide` API 接收 Unity 运行时游戏状态数据。
- 支持 Unity 相机截图的 Base64 图像输入。
- 使用 VLM 分析当前摄像机画面中的任务相关视觉信息。
- 使用 LLM 生成一句自然的游戏风格任务提示。
- 结合 Unity 结构化物体状态、视觉分析结果和当前任务上下文。
- 支持推荐目标可见性判断。
- 支持自适应提示强度策略。
- 支持轻量级后端短期记忆。
- 在当前任务阶段内保存最近两次 AI 提示。
- 当玩家停留在同一阶段时，避免重复提示并逐渐增强提示具体程度。
- 返回 `instruction`、`vision_text`、`status`、`model` 和 `latency_ms`。
- 支持通过 `/memory` 查看后端短期记忆。
- 支持通过 `/memory/reset` 重置后端短期记忆。

---

## 2. 文件结构

```text
FastAPI/
├── main.py              # FastAPI 后端主程序
├── README.md            # 后端说明文档
├── requirements.txt     # Python 依赖文件
├── .env.example         # 环境变量示例文件
└── test_main.http       # 可选 API 测试文件
````

---

## 3. 系统架构

```text
Unity 玩家 / 摄像机
        ↓
Unity 游戏状态追踪
        ↓
POST /guide
        ↓
FastAPI 后端
        ↓
VLM：截图 → vision_text
        ↓
短期记忆：
最近两次 AI 提示 + 当前阶段提示次数
        ↓
LLM：
current_task + objects + vision_text + memory → instruction
        ↓
Unity UI / Console 显示
```

本系统不是简单地让 AI 随机生成提示，而是结合了三个信息来源：

```text
Unity State = 可靠的游戏状态
VLM = 当前画面的视觉理解
LLM = 综合任务状态和视觉信息，生成最终提示
```

其中，Unity 的结构化游戏状态是 ground truth。VLM 主要用于补充当前画面中可见的物体、文字、交互提示和线索。LLM 最后结合这些信息生成玩家可以理解的一句话提示。

---

## 4. 环境配置

请在 `FastAPI/` 文件夹下创建 `.env` 文件。

`.env` 示例：

```env
KKRICH_API_KEY=your_api_key_here
KKRICH_MODEL=gpt-5.5
KKRICH_VLM_MODEL=gpt-5.5
KKRICH_BASE_URL=https://api.kkrich.ltd/v1
```

字段说明：

| 字段                 | 说明                       |
| ------------------ | ------------------------ |
| `KKRICH_API_KEY`   | API key                  |
| `KKRICH_MODEL`     | 用于生成最终 instruction 的 LLM |
| `KKRICH_VLM_MODEL` | 用于分析 Unity 截图的 VLM       |
| `KKRICH_BASE_URL`  | OpenAI-compatible API 地址 |

注意：
真实的 `.env` 文件不要上传到 GitHub。建议上传 `.env.example` 作为示例文件。

---

## 5. 安装依赖

进入 `FastAPI/` 文件夹后，安装依赖：

```bash
pip install -r requirements.txt
```

推荐的 `requirements.txt` 内容：

```txt
fastapi
uvicorn
python-dotenv
openai
pydantic
```

---

## 6. 启动后端

在 `FastAPI/` 文件夹下运行：

```bash
uvicorn main:app --reload
```

启动成功后，可以打开：

```text
http://127.0.0.1:8000/docs
```

这里是 FastAPI 自动生成的 Swagger API 测试页面。

---

## 7. API 接口说明

### 7.1 `GET /`

用于检查后端是否正在运行。

示例响应：

```json
{
  "message": "FastAPI backend is running.",
  "model": "gpt-5.5",
  "vlm_model": "gpt-5.5",
  "base_url": "https://api.kkrich.ltd/v1"
}
```

---

### 7.2 `GET /health`

健康检查接口，用于确认后端状态。

示例响应：

```json
{
  "status": "ok",
  "message": "Unity AI guidance backend is running.",
  "model": "gpt-5.5",
  "vlm_model": "gpt-5.5",
  "memory_sessions": 1
}
```

如果当前代码里还没有 `/health`，可以后续添加。它主要用于快速确认服务是否正常启动。

---

### 7.3 `GET /memory`

调试接口，用于查看当前后端保存的短期记忆。

示例响应：

```json
{
  "default_player": {
    "last_stage": "searchseatclues",
    "hint_count": 2,
    "recent_instructions": [
      "Check the seating area for useful clue cards.",
      "Focus on the next unobserved seat card."
    ]
  }
}
```

该接口主要用于测试和调试，方便查看后端是否成功保存最近两次 AI 提示。

---

### 7.4 `POST /memory/reset`

用于清空后端短期记忆。

示例响应：

```json
{
  "status": "success",
  "message": "Backend short-term memory has been reset."
}
```

建议在测试一个新场景前调用该接口，避免之前的提示记录影响新的测试结果。

---

### 7.5 `POST /guide`

这是 Unity 调用的主要接口。Unity 会向该接口发送当前玩家状态、任务阶段、场景物体和可选的摄像机截图。后端会返回一条 AI 生成的任务提示。

---

## 8. `/guide` 请求示例

```json
{
  "player_position": {
    "x": 1.2,
    "y": 0,
    "z": 3.4
  },
  "current_task": "current_stage: SearchSeatClues\nrecommended_target_object: seat_card_02\ntime_in_current_stage_seconds: 25\nfailed_keypad_attempts: 0",
  "objects": [
    {
      "id": "seat_card_02",
      "name": "Seat Card 02",
      "type": "clue",
      "tag": "interactable",
      "position": {
        "x": 2.1,
        "y": 0.8,
        "z": 4.5
      },
      "role": "password_clue",
      "state": "unobserved_password_clue"
    }
  ],
  "image_base64": "base64_image_string_here"
}
```

当前版本使用方案 B 的短期记忆设计，因此 Unity 不需要发送 `session_id`。后端默认所有请求都来自同一个 demo 玩家：

```text
default_player
```

---

## 9. `/guide` 响应示例

```json
{
  "instruction": "Focus on the next unobserved seat card; it may contain part of the keypad clue.",
  "model": "gpt-5.5",
  "status": "success",
  "vision_text": "Visible objects:\n- A seat card is visible near the chair.\n\nReadable text / symbols:\n- Some clue text may be present on the card.\n\nInteractable elements:\n- The seat card appears to be task-related.\n\nTask-related clues:\n- The card may contain part of the keypad clue.\n\nUnity state alignment:\n- The visible card matches Unity object state.\n\nRecommended target visibility:\n- seat_card_02: visible\n- evidence: A card-like object is visible near the chair.",
  "latency_ms": 4200
}
```

字段说明：

| 字段            | 说明                 |
| ------------- | ------------------ |
| `instruction` | 最终给玩家显示的一句话 AI 提示  |
| `model`       | 当前使用的 LLM 模型       |
| `status`      | 请求状态，例如 `success`  |
| `vision_text` | VLM 对当前截图的视觉分析结果   |
| `latency_ms`  | 后端处理本次请求的总耗时，单位为毫秒 |

Unity 主要使用 `instruction` 显示给玩家。
`vision_text` 和 `latency_ms` 主要用于调试和 API 性能测试。

---

## 10. VLM 视觉分析

后端会先调用 VLM 分析 Unity 相机截图。VLM 不直接生成最终玩家提示，而是生成任务相关的视觉摘要。

VLM 主要关注：

* 当前画面中可见的任务相关物体
* 可读文字、符号、数字、UI 提示
* 可交互物体
* 物体状态，例如 locked、closed、opened、available
* 与当前任务有关的线索
* Unity 状态与画面是否一致
* Unity 推荐目标是否在当前画面中可见

VLM 输出格式示例：

```text
Visible objects:
- A locked door is visible.
- A keypad is mounted beside the door.

Readable text / symbols:
- "LOCKED EXIT"
- "Press E to interact"

Interactable elements:
- The keypad appears interactable.

Task-related clues:
- The keypad may require a code.

Unity state alignment:
- The visible keypad matches Unity object state: available.

Recommended target visibility:
- keypad_01: visible
- evidence: A keypad is clearly visible beside the locked door.
```

---

## 11. 推荐目标可见性判断

推荐目标可见性判断用于判断 Unity 推荐的目标物体是否真的出现在当前玩家摄像机画面中。

例如 Unity 当前任务是：

```text
current_stage: UseKeypad
recommended_target_object: keypad_01
```

如果 VLM 看到 keypad，它会输出：

```text
Recommended target visibility:
- keypad_01: visible
- evidence: A keypad is clearly visible beside the locked door.
```

如果玩家当前视角没有看到 keypad，它可能输出：

```text
Recommended target visibility:
- keypad_01: not visible
- evidence: No keypad is clearly visible in the current camera view.
```

LLM 会根据这个信息调整最终提示：

| 可见性           | AI 提示方式          |
| ------------- | ---------------- |
| `visible`     | 直接建议玩家检查或交互该目标   |
| `not visible` | 提示玩家先寻找目标或检查相关区域 |
| `uncertain`   | 给出更谨慎的提示         |

这样可以避免 AI 在目标并不出现在画面中时，错误地说“你面前的物体”。

---

## 12. 自适应提示强度

后端 prompt 包含自适应提示强度策略。

AI 会根据以下信息调整提示具体程度：

* 当前任务阶段
* 玩家停留在当前阶段的时间
* 密码输入失败次数
* 最近两次 AI 提示
* 当前阶段已经提示的次数

基本策略：

| 情况        | 提示方式            |
| --------- | --------------- |
| 玩家刚进入任务阶段 | 轻提示，比较间接        |
| 玩家正常推进    | 普通提示，指出相关区域或线索  |
| 玩家停留较久    | 更具体地指出目标物体      |
| 密码输入错误    | 给出纠错提示，引导重新检查线索 |
| 任务完成      | 给出完成提示          |

示例：

第一次提示：

```text
The seating area may contain useful clues, so take a closer look around the chairs.
```

第二次提示：

```text
Focus on the seat cards; one of them may contain part of the keypad clue.
```

第三次提示：

```text
Check the next unobserved seat card carefully before returning to the keypad.
```

这个功能使 AI 引导不再是重复的固定提示，而是更符合 procedural guidance 的自适应提示。

---

## 13. 短期记忆 Short-term Memory

后端包含一个轻量级短期记忆模块。

在当前原型中，Unity 不需要发送 `session_id`。后端默认所有请求都来自同一个 demo 玩家：

```text
default_player
```

短期记忆会保存：

| 字段                    | 说明            |
| --------------------- | ------------- |
| `last_stage`          | 上一次任务阶段       |
| `hint_count`          | 当前阶段已经生成提示的次数 |
| `recent_instructions` | 最近两次 AI 提示    |

当玩家停留在同一个任务阶段时，`hint_count` 会增加，并且最近两次 AI 提示会被加入下一次 LLM prompt。这样可以避免 AI 重复相同提示，并在玩家卡住时逐渐增强提示的具体程度。

当任务阶段发生变化时，后端会自动清空上一阶段的提示记录。

当前 memory 存储在 Python 内存中，因此后端重启后会清空。对于当前 prototype 来说已经足够。正式系统中可以进一步使用 Redis 或数据库。

---

## 14. 为什么使用 VLM + Unity State + LLM

本项目没有直接让 VLM 生成最终玩家提示，而是采用分层流程：

```text
VLM = 视觉理解
Unity State = 游戏状态 ground truth
LLM = 最终任务引导生成
```

这样设计有几个优点：

1. **更稳定**
   Unity 的结构化状态比纯视觉判断更可靠。

2. **更容易调试**
   如果提示错误，可以查看是 VLM 视觉识别错误，还是 LLM 推理错误。

3. **更适合解密游戏**
   解密游戏不仅需要看画面，还需要结合任务进度、已交互物体和当前阶段。

4. **更适合 API 性能测试**
   后端可以分别记录 `vision_text`、`instruction` 和 `latency_ms`，方便比较不同模型。

---

## 15. Unity 集成说明

Unity 会向 `/guide` 发送 POST 请求。

请求内容包括：

* 玩家位置
* 当前任务上下文
* 结构化场景物体信息
* 可选的 Base64 摄像机截图

后端返回 JSON。Unity 主要显示其中的 `instruction` 字段，而 `vision_text` 和 `latency_ms` 主要用于调试和 API 性能测试。

当前后端 memory 设计不需要 Unity 发送 `session_id`。所有请求都会被视为同一个 demo 会话：

```text
default_player
```

---

## 16. 当前版本

当前后端版本：`v0.3`

主要更新：

* 增加结构化 VLM 视觉分析。
* 增加推荐目标可见性判断。
* 增加自适应提示强度策略。
* 增加轻量级后端短期记忆。
* 增加最近两次 AI 提示记录。
* 增加 `latency_ms`，用于 API 性能评估。

---

## 17. 当前限制

当前版本仍然是 prototype，存在以下限制：

* 短期记忆保存在 Python 内存中，后端重启后会清空。
* 当前使用 `default_player`，更适合单人 demo。
* 如果未来支持多个玩家，需要让 Unity 发送 `session_id`。
* 当前 VLM 输出仍然是文本格式，后续可以改成 JSON 方便后端解析。
* 当前后端只生成指导建议，不直接控制 Unity 中的玩家或物体。
* 当前系统不会直接透露最终密码，主要用于任务引导和线索提示。

---

## 18. 后续改进方向

后续可以考虑以下改进：

* 增加 `session_id`，支持多个玩家或多个测试会话。
* 使用 Redis 或数据库保存更稳定的短期记忆。
* 让 VLM 输出 JSON 格式，方便后端自动解析目标可见性。
* 在 response 中增加 `hint_level`、`guidance_type` 和 `target_object_id`。
* 增加更多 Unity 测试场景。
* 对比不同模型在视觉识别、任务相关性、自然度、延迟和成本方面的表现。
* 增加 API 测试结果表格和可视化分析。

```

如果你当前代码里还没有 `/health`、`/memory`、`/memory/reset`，要么先把这些接口加回 `main.py`，要么把 README 里对应接口删掉。建议你加回代码，因为对测试很有用。
```
