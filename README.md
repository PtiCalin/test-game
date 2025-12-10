# Test Game (Unity)

Unity prototype living in `temp-test/`, with a Menu scene (intro video + menu music) and a castle maze scene using a procedural maze generator. Uses URP and the Input System.

## Project status

- Early-stage setup: imported Unity project files and configured ignores for generated folders.
- Scenes: `SampleScene` (baseline), `Menu` (intro video + menu music on a UI Canvas), and the castle scene that uses the maze generator script.
- Media folders: `Images`, `Musique`, `Videos`.
- No gameplay loop yet; scene is suitable for experimenting with rendering, input, and asset import.
-- Castle scene assets: imported materials for coins, chests, ground, and a maze-generation script for layout and gameplay objects.

## Quick start

1) Clone the repo.
2) Open Unity **6000.2.7f2** (project version) and load the project at `temp-test/`.
3) Open `Assets/Scenes/Menu.unity` to view the intro video and menu music playback (Canvas with VideoPlayer + AudioSource that start on Awake). For the baseline environment, open `Assets/Scenes/SampleScene.unity`. For the castle prototype, use the maze generator (`Assets/Scripts/maze-generator.cs`) and the castle materials in `Assets/Materials/`.
4) Add assets or scripts under `Assets/` as you iterate. Generated folders (`Library`, `Temp`, `Logs`, `Builds`, etc.) are ignored by git.

## Project layout

- `temp-test/Assets/Scenes/Menu.unity`: menu scene with a Canvas-based media player (intro video + menu music on Awake).
- `temp-test/Assets/Scenes/SampleScene.unity`: baseline sample scene.
- `temp-test/Assets/Scripts/maze-generator.cs`: procedural maze generator used for the castle scene (spawns maze, player, and collectibles; config via inspector).
- `temp-test/Assets/Settings/`: URP assets and renderer configs.
- `temp-test/Assets/InputSystem_Actions.inputactions`: Input System actions asset (not yet wired to gameplay).
- `temp-test/Assets/Musique/Menu Theme.mp3`: menu music referenced by the Menu scene audio source.
- `temp-test/Assets/Videos/Intro.mp4`: intro video referenced by the Menu scene video player.
- `temp-test/Assets/Materials/`: imported castle materials (e.g., coins, chests, ground) used by scenes and prefabs.
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

## Contributors

- Lead Developer — Charlie Bouchard (PtiCalin)

## Credits (Assets & Algorithms)

| Item | Author | License | Location | Notes |
| --- | --- | --- | --- | --- |
| Low Poly 3D Treasure Items Game Assets | mehrasaur | CC0 | `Assets/Models/Collectibles` | FBX models for coins, treasures, gems, chests; materials created in Unity. |
| Character Model (Visual Novel Series) | styloo | CC0 | `Assets/Models/Characters` | Published Oct 23, 2024 (updated Apr 22, 2025). Unity/Unreal/Godot compatible. |

- Maze generation: implementation based on the "Recursive Backtracker" depth-first algorithm popularized by Jamis Buck, *Maze Generation: Recursive Backtracking* (2010). Reference gist `recursive-backtracker.rb` used as structural guide.
- Third-person camera: inspired by the open-source project *3rd Person Camera And Movement System* by SunnyValleyStudio (MIT).

## License

MIT License. See `LICENSE`.
