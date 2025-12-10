# Test Game (Unity)

Unity prototype living in `temp-test/`, with a Menu scene that plays an intro video and menu music via a UI Canvas, using URP and the Input System.

## Project status

- Early-stage setup: imported Unity project files and configured ignores for generated folders.
- Scenes: `SampleScene` (baseline) and `Menu` (intro video + menu music on a UI Canvas).
- Media folders: `Images`, `Musique`, `Videos`.
- No gameplay loop yet; scene is suitable for experimenting with rendering, input, and asset import.

## Quick start

1) Clone the repo.
2) Open Unity **6000.2.7f2** (project version) and load the project at `temp-test/`.
3) Open `Assets/Scenes/Menu.unity` to view the intro video and menu music playback (Canvas with VideoPlayer + AudioSource that start on Awake). For the baseline environment, open `Assets/Scenes/SampleScene.unity`.
4) Add assets or scripts under `Assets/` as you iterate. Generated folders (`Library`, `Temp`, `Logs`, `Builds`, etc.) are ignored by git.

## Project layout

- `temp-test/Assets/Scenes/Menu.unity`: menu scene with a Canvas-based media player (intro video + menu music on Awake).
- `temp-test/Assets/Scenes/SampleScene.unity`: baseline sample scene.
- `temp-test/Assets/Settings/`: URP assets and renderer configs.
- `temp-test/Assets/InputSystem_Actions.inputactions`: Input System actions asset (not yet wired to gameplay).
- `temp-test/Assets/Musique/Menu Theme.mp3`: menu music referenced by the Menu scene audio source.
- `temp-test/Assets/Videos/Intro.mp4`: intro video referenced by the Menu scene video player.
- `temp-test/ProjectSettings/ProjectVersion.txt`: Unity editor version pin (6000.2.7f2).
- `CHANGELOG.md`: Release notes following Keep a Changelog.
- `PROJECT_FILES.md`: Inventory of notable files.
- `TESTING.md`: How to run checks (general guidance; no automated tests yet).
- `CONTRIBUTION.md`: Contribution guidelines.
- `.gitignore`: Excludes Unity-generated and other development artifacts.

## Development flow

- Work on a feature branch off `main` (or the active integration branch).
- Keep changes small; update docs alongside code.
- Run in-Editor playtests when changing scenes, assets, or scripts.
- Open a pull request with a clear summary and links to related issues.

## License

MIT License. See `LICENSE`.
