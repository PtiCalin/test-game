# Project Files

Snapshot of notable files tracked in the repository. Update this document as the project grows.

## Unity project (`temp-test/`)

- `Assets/Scenes/Menu.unity`: Menu scene with UI Canvas hosting a media player (AudioSource + VideoPlayer) that auto-plays menu music and the intro video.
- `Assets/Scenes/SampleScene.unity`: Baseline sample scene.
- `Assets/Scenes/Castle.unity`: Castle prototype scene (corridor 30x10x4, shared ground, maze generator, player controller, toggleable camera).
- `Assets/Scripts/maze-generator.cs`: Procedural maze generator for the castle scene (builds maze, spawns player, places collectibles).
- `Assets/Scripts/character.cs`: Rigidbody player controller (accel/speed cap, air control, double jump) using Input System actions.
- `Assets/Scripts/camera-settings.cs`: Third-person orbit / bird's-eye camera controller with Tab toggle and cursor lock.
- `Assets/Materials/`: Imported and authored materials (coins, chests, ground, Poliigon brick/metal/wood) applied to scene objects and prefabs.
- `Assets/Settings/`: URP render pipeline assets and renderer configurations.
- `Assets/InputSystem_Actions.inputactions`: Input System actions asset used by the player controller.
- `Assets/Musique/Menu Theme.mp3`: Menu music used by `Menu.unity`.
- `Assets/Videos/Intro.mp4`: Intro video played in `Menu.unity`.
- `ProjectSettings/ProjectVersion.txt`: Unity editor version pin (6000.2.7f2).
- `Packages/manifest.json`: Unity package references (URP, Input System, etc.).
- Generated folders (e.g., `Library`, `Temp`, `Logs`, `Builds`, `DerivedDataCache`) are ignored by git.

## Repo-level docs and config

- `README.md`: Project overview, quick start, and links.
- `LICENSE`: Licensing terms (MIT).
- `CHANGELOG.md`: Release history in Keep a Changelog format.
- `PROJECT_FILES.md`: This inventory of key files.
- `TESTING.md`: How to run tests, linting, and coverage guidance.
- `CONTRIBUTION.md`: Contribution and pull request guidelines.
- `.gitignore`: Excludes Unity-generated and other development artifacts.
