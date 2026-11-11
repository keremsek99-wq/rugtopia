# Rugtopia

Hybrid-casual puzzle: conveyor sorting plus bottom-up rug weaving. The shuttle runs boustrophedon and only the glowing tip cell can be woven (Weft Path / Atkı Yolu).

There is no Mac in the build loop. Core tests run on Ubuntu. The iOS Xcode project is exported on Ubuntu with [GameCI](https://game.ci/docs/github/builder), then a macOS GitHub-hosted runner signs it and uploads to TestFlight. Setup steps are in [docs/CI_SETUP.md](docs/CI_SETUP.md) (Turkish).

This repository is **public** so macOS runner minutes stay free. Do not commit licenses, certificates, `.p8` keys, provisioning profiles, or passwords.

## Layout

| Path | What |
|---|---|
| `Assets/Scripts/Core/` | Rules. No `UnityEngine` references. |
| `Assets/Scripts/Meta/`, `View/`, `Editor/` | Save/economy, placeholder view, editor menu and the CI build entry point |
| `Assets/Data/Levels/`, `Assets/Resources/levels.json` | 30 levels exported from the prototype |
| `Assets/Tests/EditMode/` | One EditMode smoke test for GameCI |
| `CoreTests/` | .NET 8 xUnit suite (260 tests). This is the suite CI runs without Unity |
| `prototype/` | HTML5 prototype, source of truth for the rules. Live: https://ilmek-prototype.ilmek-oyun.workers.dev/ |
| `fastlane/` | TestFlight signing and upload |
| `.github/workflows/` | `core-tests`, `unity-build`, `ios-testflight` |

## Core tests

.NET 8 SDK:

```bash
dotnet test CoreTests
```

Expected: 260 passed. `bash Tools/ci/self-check.sh` checks that the CI secret gate skips when secrets are absent and refuses to point match at this public repo.

Regenerate level JSON from the prototype (Node 18+ and .NET 8):

```bash
Tools/regenerate.sh
```

## CI

| Workflow | When | Without secrets |
|---|---|---|
| `core-tests` | every push and pull request | runs `dotnet test` |
| `unity-build` | pull requests, pushes to `main`, manual | skips with a notice |
| `ios-testflight` | manual, and tags `v*` | skips with a notice |

`unity-build` uses `game-ci/unity-test-runner@v4` (EditMode) and `game-ci/unity-builder@v6` (iOS Xcode project). Editor version comes from `ProjectSettings/ProjectVersion.txt` (`6000.3.0f1`). `ios-testflight` signs with fastlane match and an App Store Connect API key. The match git repo must be a **different private repository**.

Bundle id defaults to `com.rugtopia.game`. Override it with the Actions variable `IOS_BUNDLE_ID`, or with the bundle id field on a manual TestFlight run.

Secrets to add are listed in [docs/CI_SETUP.md](docs/CI_SETUP.md).

## Verification status

| Item | Status |
|---|---|
| Core C# and the 260 xUnit tests | Run locally with the .NET 8 SDK and on `core-tests` |
| Unity Editor compile, EditMode test inside Unity, iOS Xcode export | Not run yet. Those steps need the Unity license secrets. The scripts were written without a Unity Editor |
| TestFlight upload | Not run yet. Needs the Apple secrets and a paid Apple Developer account |

Command-line build, once a license is available: `Unity -batchmode -nographics -quit -projectPath . -buildTarget iOS -executeMethod Rugtopia.EditorTools.RugtopiaMenu.BuildIOSBatch` (the workflow uses the same method and passes `-customBuildPath`, `-bundleId`, `-marketingVersion`, and `-iosBuildNumber`).

## Rules (prototype v3.1)

- Spools wait in lanes. A tap sends the front spool onto a belt with a fixed number of slots.
- The rug is woven bottom-up, one stitch at a time, boustrophedon. Only the shuttle tip can be woven. A matching belt spool fires, oldest first. Other colours wait and keep their slot. Empty spools leave.
- Hidden `?` spools reveal at the front of a lane. Covered rows stay veiled until that row is active. A knotted spool fires only after one more row is finished.
- Boosters: +1 Slot, Comb (any of the top 4 spools in a lane), Shuttle (next 5 stitches of the active row).
- Win: every stitch woven. Fail: nothing can fire and the belt is full or the lanes are empty.
- Core resolves weaving instantly. The prototype's per-stitch timing belongs in the view.

More prototype notes: [prototype/README.md](prototype/README.md).
