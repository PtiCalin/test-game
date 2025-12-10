# Project Files

Snapshot of notable files tracked in the repository. Update this document as the project grows.

## Unity project (`temp-test/`)
- `Assets/Scenes/SampleScene.unity`: Current main scene.
- `Assets/Settings/`: URP render pipeline assets and renderer configurations.
- `Assets/InputSystem_Actions.inputactions`: Input System actions asset (not yet wired to gameplay code).
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
