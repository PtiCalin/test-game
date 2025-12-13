# Changelog

All notable changes to this project will be documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.6] - 2025-11-31

### Added

- Applied Poliigon wood floor (`Poliigon_WoodFloorAsh_4186_Preview1`) to corridor and maze floors; applied Poliigon brick wall (`Poliigon_BrickWallReclaimed_8320_Preview1`) to corridor and maze walls.
- Added dedicated corridor floor plane for cleaner texturing.

### Changed

- Widened castle corridor to ~35x10x6 for better third-person camera clearance.
- Updated maze defaults: cell size 6, wall height 3, wall thickness 0.6.
- Documentation refreshed (`README.md`, `PROJECT_FILES.md`, `TESTING.md`).

## [0.1.5] - 2025-11-30

### Added

- Implemented Rigidbody-based player controller (`character.cs`) with Input System actions, air control, and double jump.
- Added third-person orbit / bird's-eye camera (`camera-settings.cs`) with Tab toggle and cursor lock.
- Created Poliigon-based materials (brick, bronze, wood) and configured import settings (linear metallic/roughness/AO, normal maps marked correctly).
- Wired castle scene character to the imported model and shared ground material across corridor and maze room.

### Changed

- Castle corridor scaled to 30x10x4 with player spawn inside and ground alignment.
- `README.md` and `PROJECT_FILES.md` updated with castle scene details, controls, new scripts, and materials.

## [0.1.4] - 2025-11-30

### Added

- Imported castle materials assets and documented their location.
- Documented the procedural maze-generation script for the castle scene in `README.md` and `PROJECT_FILES.md`.
- Added asset and algorithm attributions (contributors, imported models, maze algorithm inspiration) to `README.md`.

## [0.1.3] - 2025-11-29

### Changed

- Documented the Menu scene setup (Canvas with media player for intro video and menu music) in `README.md` and `PROJECT_FILES.md`.

## [0.1.2] - 2025-11-28

### Changed

- Updated project documentation (`README.md`, `PROJECT_FILES.md`) to reflect the current Unity project state and ignored artifacts.

## [0.1.1] - 2025-11-28

- Imported Unity project files into `temp-test/` and excluded generated/voluminous project materials (Library, Temp, Logs, Builds, etc.) from version control.

## [0.1.0] - 2025-11-28

### Added

- Initial documentation scaffold (`README.md`, `LICENSE`, `CHANGELOG.md`, `PROJECT_FILES.md`, `TESTING.md`, `CONTRIBUTION.md`, `.gitignore`).
