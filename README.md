# FastAPI Backend for Embodied AI Guidance in Immersive Games

This folder contains the FastAPI backend for the project **Adaptive Task Planning and Procedural Guidance via Embodied AI Agents in Immersive Games**.

The backend receives runtime data from the Unity immersive puzzle game, including the player's position, current task stage, structured scene objects, and an optional camera screenshot. It uses a VLM + LLM pipeline to analyze the current scene and generate adaptive procedural guidance for the player.

The current prototype uses a Unity meeting-room escape puzzle scene as the testing environment.
