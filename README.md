# FastAPI Backend for Embodied AI Guidance in Immersive Games

本文件夹包含项目 **Adaptive Task Planning and Procedural Guidance via Embodied AI Agents in Immersive Games** 的 FastAPI 后端代码。

当前原型基于 Unity 会议室逃脱解密场景。后端接收 Unity 发送的玩家位置、当前任务、场景物体状态和可选摄像机截图，并通过 VLM + LLM 流程生成自适应 AI 引导提示。

---

## 1. 功能概述

当前后端支持：

- 接收 Unity 运行时游戏状态数据
- 接收 Base64 格式的 Unity 相机截图
- 使用 VLM 分析当前画面
- 使用 LLM 生成一句自然的游戏提示
- 结合 Unity 状态、视觉分析和当前任务阶段
- 判断 Unity 推荐目标是否在当前画面中可见
- 根据玩家是否卡住调整提示强度
- 保存当前阶段最近两次 AI 提示，避免重复
- 返回 `instruction`、`vision_text`、`status`、`model` 和 `latency_ms`

---

## 2. 文件结构

```text
FastAPI/
├── main.py
├── README.md
├── requirements.txt
├── .env.example
└── test_main.http
```

---

## 3. 系统流程

```text
Unity 玩家 / 摄像机
        ↓
Unity 任务状态与物体状态
        ↓
POST /guide
        ↓
FastAPI 后端
        ↓
VLM 分析截图 → vision_text
        ↓
短期记忆：最近两次 AI 提示 + 当前阶段提示次数
        ↓
LLM 生成最终 instruction
        ↓
Unity 显示提示
```

系统使用三类信息：

```text
Unity State = 游戏真实状态
VLM = 当前画面视觉理解
LLM = 最终任务引导生成
```

Unity 的结构化状态是 ground truth。VLM 只负责补充当前画面中的物体、文字、交互提示和线索。LLM 最后生成玩家可以看到的一句话提示。

---

## 4. 环境配置

在 `FastAPI/` 文件夹下创建 `.env` 文件：

```env
KKRICH_API_KEY=your_api_key_here
KKRICH_MODEL=gpt-5.5
KKRICH_VLM_MODEL=gpt-5.5
KKRICH_BASE_URL=https://api.kkrich.ltd/v1
```

真实 `.env` 不要上传到 GitHub，可以上传 `.env.example`。

---

## 5. 安装依赖

```bash
pip install -r requirements.txt
```

推荐 `requirements.txt`：

```txt
fastapi
uvicorn
python-dotenv
openai
pydantic
```

---

## 6. 启动后端

```bash
uvicorn main:app --reload
```

打开 Swagger 测试页面：

```text
http://127.0.0.1:8000/docs
```

---

## 7. API 接口

### `GET /`

检查后端是否运行。

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

### `POST /guide`

Unity 调用的主要接口，用于请求 AI 引导。

请求示例：

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

响应示例：

```json
{
  "instruction": "Focus on the next unobserved seat card; it may contain part of the keypad clue.",
  "model": "gpt-5.5",
  "status": "success",
  "vision_text": "Visible objects:\n- A seat card is visible near the chair.\n\nRecommended target visibility:\n- seat_card_02: visible\n- evidence: A card-like object is visible near the chair.",
  "latency_ms": 4200
}
```

字段说明：

| 字段 | 说明 |
|---|---|
| `instruction` | 最终显示给玩家的一句话提示 |
| `model` | 使用的 LLM 模型 |
| `status` | 请求状态 |
| `vision_text` | VLM 对截图的分析结果 |
| `latency_ms` | 本次请求耗时，单位毫秒 |

---

## 8. VLM 视觉分析

VLM 负责分析 Unity 截图，但不直接给玩家最终提示。

它主要识别：

- 可见任务物体
- 可读文字、符号、UI 提示
- 可交互元素
- 物体状态，例如 locked、closed、opened、available
- 当前任务相关线索
- 推荐目标是否可见

示例输出：

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
- The visible keypad matches Unity object state.

Recommended target visibility:
- keypad_01: visible
- evidence: A keypad is clearly visible beside the locked door.
```

---

## 9. 推荐目标可见性判断

Unity 会告诉后端当前推荐目标，例如：

```text
recommended_target_object: keypad_01
```

VLM 会判断该目标是否出现在当前截图中：

```text
Recommended target visibility:
- keypad_01: visible / not visible / uncertain
```

LLM 根据这个结果调整提示：

| 目标状态 | AI 提示方式 |
|---|---|
| `visible` | 直接提示玩家检查或交互目标 |
| `not visible` | 提示玩家先寻找目标或相关区域 |
| `uncertain` | 给出更谨慎的提示 |

这样可以避免 AI 在目标不可见时错误地说“你面前的物体”。

---

## 10. 自适应提示强度

后端 prompt 包含提示强度策略。

AI 会根据以下信息调整提示：

- 当前任务阶段
- 玩家停留在当前阶段的时间
- 密码输入失败次数
- 当前阶段已经提示的次数
- 最近两次 AI 提示

基本策略：

| 情况 | 提示方式 |
|---|---|
| 刚进入阶段 | 轻提示 |
| 正常推进 | 普通提示 |
| 停留较久 | 更具体提示 |
| 密码错误 | 纠错提示 |
| 任务完成 | 完成提示 |

---

## 11. 短期记忆 Short-term Memory

后端包含轻量级短期记忆模块。

当前版本不需要 Unity 发送 `session_id`。后端默认所有请求都来自同一个 demo 玩家：

```text
default_player
```

短期记忆保存：

| 字段 | 说明 |
|---|---|
| `last_stage` | 上一次任务阶段 |
| `hint_count` | 当前阶段提示次数 |
| `recent_instructions` | 最近两次 AI 提示 |

当玩家停留在同一阶段时，`hint_count` 会增加，最近两次 AI 提示会加入下一次 prompt。这样 AI 可以避免重复提示，并逐渐给出更具体的提示。

当任务阶段变化时，后端会自动清空上一阶段的提示记录。

当前 memory 存在 Python 内存中，后端重启后会清空。对于 prototype 来说已经足够。正式系统中可以改用 Redis 或数据库。

---

## 12. Unity 集成说明

Unity 向 `/guide` 发送 POST 请求。

请求内容包括：

- 玩家位置
- 当前任务上下文
- 结构化场景物体
- 可选 Base64 截图

后端返回 JSON。Unity 主要显示 `instruction` 字段。`vision_text` 和 `latency_ms` 主要用于调试和 API 性能测试。

当前版本不需要 Unity 发送 `session_id`。

---

## 13. 当前版本

当前后端版本：`v0.3`

主要更新：

- 增加结构化 VLM 视觉分析
- 增加推荐目标可见性判断
- 增加自适应提示强度策略
- 增加轻量级后端短期记忆
- 保存最近两次 AI 提示
- 增加 `latency_ms` 用于性能测试

---

## 14. 当前限制

- 当前 memory 存在 Python 内存中，后端重启后会清空
- 当前使用 `default_player`，更适合单人 demo
- 如果未来支持多人，需要增加 `session_id`
- 当前 VLM 输出是文本格式，后续可以改成 JSON
- 当前后端只生成提示，不直接控制 Unity 行为

---

## 15. 后续改进方向

- 增加 `session_id` 支持多玩家或多测试会话
- 使用 Redis 或数据库保存 memory
- 让 VLM 输出 JSON，方便后端解析
- 在 response 中增加 `hint_level`、`guidance_type` 和 `target_object_id`
- 对比不同模型的准确性、自然度、延迟和成本
