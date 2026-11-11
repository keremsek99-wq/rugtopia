# Main scene

No `.unity` file is committed: scene YAML written by hand without the Editor is fragile.

**Option A (recommended):** menu **Rugtopia → 2. Create Main Scene**. It creates `Assets/Scenes/Main.unity` containing:

| GameObject | Components | Notes |
|---|---|---|
| Main Camera (tag MainCamera) | Camera, orthographic, size 10, position (0,0,-10), solid background #F3E6D3 | portrait 9:19.5 layout |
| GameController | `Rugtopia.View.GameController` | builds Board, Belt, Lanes and HUD children at runtime |

It also adds the scene to Build Settings.

**Option B:** press Play in any scene, even an empty one. `GameController.Bootstrap` ([RuntimeInitializeOnLoadMethod]) spawns the controller and camera when none exist.

World layout (orthographic units, y up), all built in code:

| Area | Position |
|---|---|
| Rug | rect (-4.3, 1.2, 8.6 × 7.4) |
| NEXT strip | y = 0.4 |
| Belt | y = -1.0 |
| Booster buttons | screen-space, 36.5–41 % height |
| Lanes | rect (-4.3, -8.6, 8.6 × 5.4) |
| Top bar | screen-space, top 7 % |
