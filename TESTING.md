# Testing Guide

Basic guidance for running and adding tests. Adjust commands to match the chosen tech stack.

## Prerequisites
- Install project dependencies for your stack (e.g., `npm install`, `pip install -r requirements.txt`, or `poetry install`).
- Ensure required services (databases, queues) are available if tests depend on them.

## Running tests
- Unity playtest (current): open `Assets/Scenes/Castle.unity`, enter Play Mode, verify third-person/bird's-eye camera toggle (`Tab`), movement (`WASD`), jump (`Space`), collision with maze walls, and collectible pickup.
- Automated unit tests: none yet—add per stack (e.g., `npm test`, `pnpm test`, `yarn test`, or `python -m pytest`).
- Linting/formatting: add and run your preferred tools (e.g., `npm run lint`, `ruff check .`, `black .`).
- Type checks: add commands as applicable (e.g., `mypy .`, `pyright`, `tsc --noEmit`).

## Coverage
- Example: `pytest --cov` for Python or `npm test -- --coverage` for Node.
- Publish or store coverage artifacts in CI as needed.

## Writing tests
- Co-locate tests near the code they validate or keep them under a `tests/` directory.
- Prefer fast, deterministic tests; use fixtures/fakes over live integrations when possible.
- Add regression tests when fixing bugs or altering behavior.
