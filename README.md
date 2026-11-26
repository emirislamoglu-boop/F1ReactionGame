# F1 Reaction Game

A Unity and C# reaction-time game for Android, inspired by motorsport starting lights.

## How to play

1. Tap the game area to start a round.
2. Wait for the red lights to go out. The delay varies each round.
3. Tap as soon as the lights go out. Tapping early counts as a false start and resets the round.

The game displays your most recent reaction time and saves your best and average times locally using Unity's `PlayerPrefs`.

## Run locally

1. Open the repository folder in Unity Hub with Unity **2022.3.62f3**.
2. Open [SampleScene](Assets/Scenes/SampleScene.unity), press **Play**, and click the game area to test a round in the editor.
3. For an Android build, install Unity's **Android Build Support** module and select **Android** in **File → Build Settings**.

The reaction timer, light sequence, and false-start handling are implemented in [ReactionLights.cs](Assets/ReactionLights.cs).
