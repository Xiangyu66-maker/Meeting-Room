# FastAPI AI Guidance Backend

本文件夹包含 Unity 会议室解密游戏 AI 引导系统的 FastAPI 后端代码。

后端会接收 Unity 运行时发送的数据，包括玩家位置、当前任务、关键场景物体状态，以及可选的相机截图。随后，后端使用 VLM + LLM 流程分析当前场景，并生成符合游戏上下文的 AI 引导提示。

---

## 后端功能概述

后端主要提供 `/guide` API 接口。

Unity 会发送：

- 玩家位置
- 当前任务上下文
- 关键 Unity 物体状态
- 可选的相机截图 base64

FastAPI 会返回：

- AI 生成的游戏提示 `instruction`
- 使用的模型名称
- 请求状态
- 可选的 VLM 视觉分析结果 `vision_text`
- API 响应时间 `latency_ms`

---

## 文件结构

```text
FastAPI/
├── main.py                 FastAPI 后端主程序
├── test_main.http          HTTP 接口测试文件
└── README.md               后端说明文档
```

---

## 环境配置

需要在 `FastAPI` 文件夹中创建 `.env` 文件。

`.env` 文件应该和 `main.py` 在同一层。

```env
KKRICH_API_KEY=your_api_key_here
KKRICH_MODEL=gpt-5.5
KKRICH_VLM_MODEL=gpt-5.5
KKRICH_BASE_URL=https://api.kkrich.ltd/v1
```

说明：

- `KKRICH_API_KEY` 是必须填写的 API key。
- `KKRICH_MODEL` 用于最终任务提示生成。
- `KKRICH_VLM_MODEL` 用于视觉场景理解。
- `KKRICH_VLM_MODEL` 必须是支持图片输入的模型。
- 不要把真实的 `.env` 文件上传到 GitHub。

---

## 安装依赖

在 `FastAPI` 文件夹中打开终端，运行：

```bash
pip install fastapi uvicorn python-dotenv openai pydantic
```

如果之后添加了 `requirements.txt`，也可以使用：

```bash
pip install -r requirements.txt
```

---

## 启动后端

在 `FastAPI` 文件夹中打开终端，运行：

```bash
uvicorn main:app --reload
```

启动成功后，可以在浏览器中打开：

```text
http://127.0.0.1:8000/docs
```

这里是 FastAPI 自动生成的 Swagger UI，可以手动测试 `/guide` 接口。

也可以打开下面这个地址检查后端是否正在运行：

```text
http://127.0.0.1:8000/health
```

---

## API 接口说明

### POST `/guide`

该接口接收 Unity 发送的游戏状态数据，并返回 AI 生成的游戏提示。

---

## 示例 Request

```json
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
```

---

## 示例 Response

```json
{
  "instruction": "Start by checking the whiteboard; it may explain where the password clues are hidden.",
  "model": "gpt-5.5",
  "status": "success",
  "vision_text": "A whiteboard is visible in the meeting room and appears to be related to the current task.",
  "latency_ms": 4200
}
```

---

## Request 字段说明

| 字段 | 类型 | 说明 |
|---|---|---|
| `player_position` | object | 玩家或相机在 Unity 中的位置 |
| `current_task` | string | Unity 生成的当前任务上下文 |
| `objects` | list | Unity 场景中的关键物体及其状态 |
| `image_base64` | string, optional | 可选的相机截图 base64 字符串 |

---

## Response 字段说明

| 字段 | 类型 | 说明 |
|---|---|---|
| `instruction` | string | 最终返回给 Unity 的 AI 游戏提示 |
| `model` | string | 后端使用的模型名称 |
| `status` | string | 请求状态 |
| `vision_text` | string, optional | VLM 对截图的视觉分析结果，主要用于调试 |
| `latency_ms` | integer, optional | API 响应时间，单位为毫秒 |

---

## AI 处理流程

```text
Unity 当前任务 + 物体状态 + 可选截图
↓
FastAPI /guide
↓
VLM 分析相机截图，生成 vision_text
↓
LLM 结合 current_task、Unity objects 和 vision_text
↓
生成最终游戏提示 instruction
↓
以 JSON 格式返回给 Unity
```

---

## 为什么使用 VLM + LLM

直接使用 VLM 也可以从图片中生成提示，但这种方式不容易调试，也可能忽略 Unity 中更可靠的物体状态。

本后端采用模块化结构：

| 模块 | 职责 |
|---|---|
| VLM | 负责视觉场景理解 |
| Unity State | 提供可靠的游戏状态信息 |
| LLM | 结合任务、物体状态和视觉信息生成最终提示 |
| FastAPI | 连接 Unity 和 AI 模型的后端控制层 |

这种设计的优点：

- 更容易调试
- 更容易解释
- 更可控
- 更适合 API 性能评估
- 更适合 Unity 解密游戏的任务引导

---

## VLM 输出设计

VLM 不需要描述画面中的所有细节，而是应该生成与当前任务相关的视觉摘要。

`vision_text` 主要关注：

- 可见的任务相关物体
- 可读文字或 UI 提示
- 可交互元素
- 可能的线索
- 物体之间的关系
- 图片内容是否与 Unity 物体状态一致

示例 `vision_text`：

```text
Visible objects:
- A whiteboard is visible in the meeting room.

Readable text / symbols:
- Some text or clue appears on the whiteboard.

Interactable elements:
- The whiteboard appears to be a task-related clue object.

Task-related clues:
- The whiteboard may explain where the password clues can be found.

Unity state alignment:
- The visible whiteboard matches the Unity object state: unobserved.
```

---

## 返回 JSON 的原因

后端返回 JSON，而不是只返回一段纯文本 `instruction`。

虽然 Unity 最终只需要把 `instruction` 显示给玩家，但完整 JSON 对调试、模型对比和 API 性能测试更有用。

字段用途：

- `instruction`：显示给玩家的最终提示
- `model`：记录使用的模型
- `status`：判断请求是否成功
- `vision_text`：查看 VLM 到底识别到了什么
- `latency_ms`：记录 API 响应时间，用于性能测试

在 Unity 中，玩家界面只需要显示 `instruction`，其他字段可以输出到 Console 用于测试和调试。

---

## 使用 Swagger UI 测试

1. 启动后端：

```bash
uvicorn main:app --reload
```

2. 打开：

```text
http://127.0.0.1:8000/docs
```

3. 找到 `POST /guide`。

4. 点击 `Try it out`。

5. 粘贴 JSON 请求。

6. 点击 `Execute`。

7. 查看返回结果。

成功返回通常包含：

```json
{
  "instruction": "...",
  "model": "...",
  "status": "success",
  "vision_text": "...",
  "latency_ms": 4200
}
```

---

## Unity 集成说明

Unity 端通过 `CameraPromptSender.cs` 向本后端发送请求。

默认接口地址：

```text
http://127.0.0.1:8000/guide
```

Unity 中默认手动发送请求的按键是：

```text
G
```

如果请求成功，Unity Console 中应该能看到 FastAPI 返回的 JSON，包括 `instruction` 和可选的 `vision_text`。

---

## 当前测试场景

| 测试场景 | 场景内容 | 期望结果 |
|---|---|---|
| Locked Exit + Keypad | 锁住的门旁边有 keypad | AI 建议使用 keypad，并寻找密码线索 |
| Cabinet Interaction | 关闭的柜子和交互提示 | AI 建议打开柜子寻找隐藏线索 |
| Meeting Room Puzzle | 白板、座位卡、keypad、锁门 | AI 根据逃脱流程引导玩家完成任务 |

---

## 模型对比

当前初步测试了 GPT-5.4 和 GPT-5.5。

建议对比指标：

| 指标 | 说明 |
|---|---|
| Visual accuracy | VLM 是否正确识别关键物体和文字 |
| Task relevance | 最终提示是否符合当前任务 |
| Naturalness | 输出是否像自然的游戏提示 |
| Stability | 多次测试结果是否稳定 |
| Latency | API 响应时间 |
| Cost | API 使用成本 |

初步测试结果表明，GPT-5.4 和 GPT-5.5 都可以理解简单 Unity 解密场景并生成有用提示。GPT-5.5 的提示通常更稳定、更具体，而 GPT-5.4 也可以用于简单 demo。

---

## 常见问题

### 1. 打不开 FastAPI 页面

确认后端已经启动：

```bash
uvicorn main:app --reload
```

然后打开：

```text
http://127.0.0.1:8000/docs
```

---

### 2. Unity 连接不上后端

检查：

- FastAPI 是否正在运行
- Unity 里的 endpoint 是否是 `http://127.0.0.1:8000/guide`
- Unity 和 FastAPI 是否运行在同一台电脑
- 防火墙或网络设置是否阻止请求

---

### 3. VLM 返回 `No visual input provided.`

说明 Unity 或测试请求没有发送 `image_base64`。

检查：

- Unity 中是否开启 `Include Camera Image`
- JSON 请求中是否包含 `image_base64`

---

### 4. VLM 返回 `Visual analysis unavailable.`

可能原因：

- API key 缺失或错误
- 选择的 VLM 模型不支持图片输入
- base URL 配置错误
- 图片格式或 base64 数据有问题

---

### 5. API key 问题

检查 `.env`：

```env
KKRICH_API_KEY=your_api_key_here
KKRICH_MODEL=gpt-5.5
KKRICH_VLM_MODEL=gpt-5.5
KKRICH_BASE_URL=https://api.kkrich.ltd/v1
```

修改 `.env` 后，需要重启 FastAPI。

---

## 注意事项

不要上传真实的 `.env` 文件到 GitHub。

建议不要上传以下文件：

```text
.env
venv/
__pycache__/
request.json
test.png
```

---

## 当前状态

当前后端已经可以接收 Unity 运行时数据，调用 VLM 进行视觉分析，并结合 Unity 物体状态生成任务相关的 AI 提示。

目前后端仍然是 early prototype，后续可以继续通过更多 Unity 场景、API 测试和模型对比来改进。
