# V1 validation

Date: **23 October 2026**. Final repository test run: **13:01:54–13:02:14 IST** (07:31:54–07:32:14 UTC in the XML).

## Verified

- **Unity compilation:** the installed **6000.6.7f1** editor imported the final repository and compiled the runtime, editor and test assemblies. No C# compile errors were reported.
- **Scene generation:** Unity executed `Paradox.Editor.LevelBuilder.Build`, saved Level01 and its material assets, assigned scene references and added Level01 to the build scene list. Generated scene and project settings are included with Unity-generated metadata.
- **Play Mode integration:** **15 passed, 0 failed, 0 skipped**, in **20.48 seconds**, running from the final repository with graphics enabled. The test runner exited with code 0. Raw evidence: [TestResults.xml](TestResults.xml).
- **Rendered visuals:** Unity captured White, Black, solved-door and completion frames. White/Black and completion previews were inspected. Initial White brightness and HUD contrast problems were corrected and recaptured. See [White](Previews/White.png), [Black](Previews/Black.png), [Solved](Previews/Solved.png), and [Complete](Previews/Complete.png).
- **Standalone build:** Unity BuildPipeline successfully produced the Linux x86_64 player in `Builds/Linux/ProjectParadox.x86_64`. The local output is deliberately excluded from Git because it contains generated platform binaries.
- **Standalone startup:** the Linux player ran for 10 seconds in batch/headless mode, initialized the scene and PhysX, and produced no script exceptions or missing-reference warnings. The test intentionally stopped it with `timeout` (exit 124). Unity emitted its first-run PlayerPrefs message because no prior preferences file existed. This verifies startup, not interactive standalone play.
- **Supporting checks:** `bash -n Tools/validate.sh` and `git diff --check` passed.

## Final test coverage

| Scenario | Result |
| --- | --- |
| Initial White world; opposite dimension visibility and solidity | Passed |
| Held-switch rejection; transfer changes Cube membership/location but not player dimension or position | Passed |
| Plate requirement, latched door, dimension persistence, exit trigger and completion UI | Passed |
| Black Cube cannot activate the White plate | Passed |
| Switching into hidden solid geometry is blocked | Passed |
| Current-dimension Cube recovery; held fall recovery; progress preservation | Passed |
| Switching off the bridge falls and respawns | Passed |
| Repeated switch, pickup and transfer requests do not corrupt state | Passed |
| Restart clears completion, door and Cube state; repeated requests reload only once | Passed |
| Cube left behind or dropped at start stays reachable through Black | Passed |
| Real CharacterController crosses bridge and collides with closed exit | Passed |
| Camera ray targeting drives pickup, pad transfer and plate placement through the E interaction operation | Passed |
| Normal drop succeeds; obstructed drop at a wall is blocked | Passed |
| Reverse transfer preserves player dimension; the saved Restart button listener reloads correctly | Passed |
| White, Black, solved and completed frames render with a real graphics device | Passed |

## Problems found and fixed

1. Early scene-generation review found that caching the initial disabled state would keep objects hidden after scene reload. DimensionObject now owns the enabled state of all renderers/colliders under its root.
2. The first runtime suite passed 9/11. Gravity was gated by cursor locking, so the bridge-fall test could not fall when the batch runner had no locked cursor. Gravity now continues while cursor capture is released.
3. The intermediate suite passed 10/11. A penetration query against colliders saved disabled did not detect hidden geometry. The query now temporarily enables its shapes synchronously and restores both enabled states before returning; no physics simulation runs during that query.
4. The next suite passed 11/11. Adding camera interaction and graphics capture passed 13/13. Final visual corrections and the two additional interaction/restart scenarios passed 15/15 in the repository.

## Reproduce

In the editor: **Window > General > Test Runner**, choose **PlayMode**, then **Run All** with Level01 available in the build scene list.

From a terminal, set `UNITY_EDITOR` to your editor executable and run:

```bash
UNITY_EDITOR='/path/to/Unity' PARADOX_GRAPHICS=1 ./Tools/validate.sh
```

This runs with a graphics device and writes the XML, log and preview PNGs under Docs. Close another Unity instance using this project before launching batch validation. Without `PARADOX_GRAPHICS=1`, the script uses `-nographics`; the preview test is intentionally skipped, while the remaining tests run.

For a Linux build, use **Paradox > Build Linux Player**, or run Unity with `-batchmode -nographics -quit -projectPath <repository> -executeMethod Paradox.Editor.BuildPlayer.BuildLinux`.

## Limits — not verified

- No human keyboard/mouse playthrough was performed. Movement feel, mouse sensitivity, actual Space input and the visible timing of effects need the manual checklist in ProjectParadox_V1.md.
- Tests use real Unity scene components and physics, but reposition the player for some scenarios. Camera interaction tests call the same operation as E; they do not send hardware key events. The CharacterController crossing test supplies movement directly.
- Restart button wiring was exercised by invoking the button's saved event. A physical mouse click on that button was not exercised.
- Pixel previews verify still-frame layout and world differences, not perceived animation smoothness or every display size.
- Windows/macOS exports, other Unity versions and new-Input-System-only projects were not tested.
- The first sandboxed Unity attempt failed to start Package Manager IPC. Running the installed editor outside that sandbox succeeded. A nonfatal license-token refresh message appeared during editor startup; compilation, tests and build all completed successfully with the available license.

## Carrying drift fix — 23 October 2026

Inspected the supplied 29.52-second recording: while carrying, the Cube visibly moved away from its camera-relative hold position during turns. A regression test reproduced the bug in the original code: **2.694 units of grip-position error on its first sampled frame**.

The Cube's interpolated Rigidbody was writing an older world-space pose over the camera-parented Transform. `TransferableObject` now disables interpolation while held, initializes the physics pose during release/transfer/recovery, and restores its original interpolation setting after synchronizing the new pose. Teleports also suspend interpolation before changing position, preventing stale arrival/recovery poses.

Validation used a separate project copy in `/tmp/ParadoxValidation` to avoid closing the user's open editor. Its Assets, Packages and ProjectSettings were copied from this repository. Unity **6000.6.7f1**, graphics enabled: **17 passed, 0 failed, 0 skipped**, in **21.51 seconds** (13:38:48–13:39:09 IST). Evidence: [CarryFixTestResults.xml](CarryFixTestResults.xml).

New coverage checks position error below 0.001 units and rotation error below 0.05 degrees through 50 frames of movement/turning with a slower physics timestep. It also checks repeated pickup/drop, transfer, and respawn restore normal physics. The original 15 scenarios still pass. The first fix run exposed a teleport/recovery interpolation issue; that was corrected before the final passing run.

A fresh manual playthrough of the patched version remains to be done. Stop Play mode, let Unity compile, then press Play again. No scene regeneration is needed.

The Linux player was rebuilt successfully from the patched sources and copied to `Builds/Linux`. The editor scene was not regenerated or replaced.
