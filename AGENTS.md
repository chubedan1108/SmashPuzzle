# Repository Guidelines

## Project Structure & Module Organization

This is a Unity 6 (`6000.0.73f1`) 3D slingshot-puzzle prototype. Game-owned content lives under `Assets/00SmashPuzzle/`: put runtime C# in `Scripts/`, reusable objects in `Prefabs/`, materials in `Materials/`, and playable scenes in `Scenes/`. Shared infrastructure is under `Assets/Framework/`. `Assets/JMO Assets/` and `Assets/AssetClone/` contain imported third-party art or packages; avoid modifying them unless the change specifically concerns vendor content. Unity package versions are recorded in `Packages/manifest.json`, while global engine configuration belongs in `ProjectSettings/`.

Do not commit generated directories such as `Library/`, `Temp/`, `Logs/`, or `obj/`. Keep every Unity asset together with its `.meta` file so GUID references remain stable.

## Build, Test, and Development Commands

Open the project through Unity Hub with Unity `6000.0.73f1`, then load `Assets/00SmashPuzzle/Scenes/Game.unity` and press Play.

From PowerShell, with `Unity.exe` replaced by the local Editor path:

```powershell
Unity.exe -projectPath .
Unity.exe -batchmode -projectPath . -runTests -testPlatform editmode -testResults TestResults.xml -quit
Unity.exe -batchmode -projectPath . -runTests -testPlatform playmode -testResults PlayResults.xml -quit
dotnet build SmashPuzzle.sln
```

The final command is a quick C# compile check; Unity remains authoritative for scenes, serialized references, and builds. Create player builds from **File > Build Profiles** because no repository build script currently exists.

## Coding Style & Naming Conventions

Use four-space indentation and one type per C# file. Use `PascalCase` for types, methods, properties, and events; use `camelCase` for parameters and serialized private fields. Prefix non-serialized private backing fields with `_` when helpful. Prefer `[SerializeField] private` over public mutable fields, unsubscribe event handlers in `OnDestroy`, and keep Unity lifecycle methods grouped near the top of a component. Match filenames to their primary class, for example `SlingshotController.cs`.

## Testing Guidelines

The Unity Test Framework is installed, but project tests are not yet present. Add Edit Mode tests under `Assets/00SmashPuzzle/Tests/EditMode/` and physics or scene tests under `Tests/PlayMode/`; name fixtures `FeatureNameTests.cs` and methods `Method_Condition_ExpectedResult`. Test launch calculations, pooling resets, and input-to-event behavior when changing those systems.

## Commit & Pull Request Guidelines

Existing history uses generic messages such as `commit code`; use concise imperative subjects instead, such as `Fix unreachable slingshot trajectory`. Keep commits focused and include related `.meta` files. Pull requests should explain gameplay impact, list test results, link the issue when available, and include a screenshot or short capture for scene, prefab, UI, or visual changes.
