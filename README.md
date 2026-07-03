# Meeting-Room

本项目用于连接 Unity 会议室场景与本地 FastAPI AI 指导后端。

## v0.1

本版本在 Unity 端加入了会议室逃脱任务的自适应 AI 引导。当前主要任务流程为：

```text
观察白板提示
-> 搜索椅子 / 座位卡上的密码线索
-> 前往门旁 keypad_01 输入密码
-> 打开 locked_door_01 完成逃脱
```

新增脚本：

```text
Assets/Scripts/MeetingRoomAdaptiveGuide.cs
```

该脚本负责在 Unity 本地维护任务阶段，并把当前诊断结果交给 `CameraPromptSender.cs` 发送给 FastAPI。它会记录玩家已经交互过的关键物体，例如 `whiteboard_01`、`seat_card_01` 到 `seat_card_04`、`keypad_01`，并根据进度判断下一步目标。

当前自适应阶段包括：

- `FindWhiteboard`：玩家还没有观察白板时，高亮 `whiteboard_01`。
- `SearchSeatClues`：玩家已经观察白板，但还没有看完座位卡时，高亮下一个未观察的 `seat_card`。
- `UseKeypad`：座位卡线索已观察完，引导玩家前往 `keypad_01`。
- `EnterPassword`：玩家正在输入密码；如果输入错误，引导玩家重新检查椅子 / 座位线索。
- `Completed`：门已解锁，任务完成。

`CameraPromptSender.cs` 已更新为支持自适应上下文。发送给 FastAPI 的 `current_task` 不再只是普通任务描述，而会自动加入：

- 已知任务流程。
- 当前任务阶段。
- 推荐目标物体。
- 已观察 / 已交互物体。
- 密码输入失败次数。
- 当前阶段停留时间。
- 上一次 VLM 视觉分析结果。

Unity 端发送给 FastAPI 的 `objects` 也会包含更多关键对象状态，例如：

- `whiteboard_01`
- `seat_card_01` 到 `seat_card_04`
- `chair_01` 到 `chair_10`
- `keypad_01`
- `locked_door_01`
- `victory_exit_point`

相关交互事件已经接入自适应引导：

- `InteractableObject.cs`：玩家交互白板、座位卡、keypad、门时通知引导系统。
- `KeypadController.cs`：开始输入、密码错误、密码正确时通知引导系统。
- `DoorController.cs`：门解锁时通知引导系统。
- `InteractionSetupHelper.cs`：确保主相机上挂载 `FirstPersonInteractor`、`CameraPromptSender` 和 `MeetingRoomAdaptiveGuide`。

当前场景中 `MainCamera` 已挂载自适应引导相关组件。进入 Play 模式后，系统会根据玩家进度自动高亮下一目标，并在阶段变化或玩家停留过久时请求 FastAPI / AI 生成新的指导文本。

## Unity 到 FastAPI 的整体桥接流程

当前链路如下：

```text
Unity Camera / Player 状态
-> CameraPromptSender.cs 收集运行时数据
-> POST http://127.0.0.1:8000/guide
-> FastAPI/main.py 组装提示词模板
-> 调用 AI / VLM 模型
-> 返回 instruction 与可选 vision_text
-> Unity Console 显示返回结果
```

Unity 端发送给 FastAPI 的主要数据包括：

- `player_position`：玩家或相机所在位置。
- `current_task`：当前任务上下文。
- `objects`：Unity 场景中的关键物体信息，例如物体 ID、名称、类型、位置、状态。
- `image_base64`：可选的相机截图，供 FastAPI 调用 VLM 做视觉理解。

FastAPI 端负责：

- 接收 Unity 发来的结构化状态和可选截图。
- 如果存在 `image_base64`，先进行视觉分析。
- 将 Unity 状态、物体信息、视觉分析结果填入后端提示词模板。
- 返回一句适合游戏内提示的 `instruction`。
- 可选返回 `vision_text`，用于调试 VLM 看到的画面内容。

## CameraPromptSender.cs

脚本位置：

```text
Assets/Scripts/CameraPromptSender.cs
```

建议挂载位置：

```text
Player/MainCamera
```

该脚本的用途是把 Unity 运行时状态发送到 FastAPI 的 `/guide` 接口，并接收 AI 返回的指导文本。

主要功能：

- 按键发送请求，默认按 `G`。
- 可在进入 Play 模式时自动发送一次请求。
- 自动读取玩家或相机位置。
- 发送当前任务描述和关键物体信息。
- 可选捕获 Camera 画面，并编码为 JPG base64 发送给后端。
- 解析 FastAPI 返回的 `instruction` 和 `vision_text`。
- 在 Unity Console 中输出请求状态和 AI 返回内容。

### Inspector 参数

`Backend`

- `Endpoint Url`：FastAPI 接口地址，默认应为 `http://127.0.0.1:8000/guide`。

`Task Context`

- `Current Task`：当前任务描述。这里不需要写完整提示词模板，只需要写任务上下文。
- `Send Key`：运行时触发发送的按键，默认是 `G`。
- `Send On Start`：进入 Play 模式后是否自动发送一次请求。

`Player Source`

- `Player Transform`：玩家位置来源。为空时脚本会优先使用相机所在根对象或自身 Transform。

`Camera Image`

- `Capture Camera`：用于截图的 Camera。为空时默认使用挂载该脚本的 Camera。
- `Include Camera Image`：是否把相机截图作为 `image_base64` 发送给 FastAPI。
- `Capture Width`：截图宽度。
- `Capture Height`：截图高度。
- `Jpg Quality`：JPG 压缩质量，数值越高画质越好，但请求体越大。

`Target Object Sent To FastAPI`

- `Target Transform`：目标物体 Transform。填写后会使用该物体的实际世界坐标。
- `Target Id`：目标物体 ID，例如 `whiteboard_01`。
- `Target Name`：目标物体名称，例如 `Whiteboard`。
- `Target Type`：目标类型，例如 `puzzle_clue`。
- `Target Tag`：目标物体标签。
- `Target Position`：未指定 `Target Transform` 时使用的目标位置。
- `Target Role`：目标在任务中的角色，例如 `target`。
- `Target State`：目标当前状态，例如 `idle`、`completed`。

`Debug`

- `Log Request`：是否在 Console 中输出请求日志。
- `Log Response`：是否在 Console 中输出完整后端响应。
- `Log Vision Text`：是否输出 FastAPI 返回的视觉分析文本 `vision_text`。



