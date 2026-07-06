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
