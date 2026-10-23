# Project Paradox V1

**It's all in your perspective.** Project Paradox is a calm, single-player 3D puzzle game. White is pale and structured; Black is deep blue and violet. These worlds share architecture but have different solid objects and mechanisms. There is one compact level, with no time pressure.

## What is implemented

A first-person CharacterController, camera, jumping and gravity; two dimensions with persistent state, atmosphere and material changes; a Black bridge and Cube; paired warp pads that transfer the object; a White pressure plate; a latched, animated door; a completion screen; scene restart; contextual UI; particle feedback; player and Cube recovery.

All geometry is built from Unity primitives. Materials use the built-in Standard shader. UI uses uGUI and the built-in LegacyRuntime font. No external art or custom shaders are used.

## Open and run

1. In Unity Hub, choose Add/Open project and select the repository folder containing `Assets`, `Packages`, and `ProjectSettings`.
2. Use **6000.6.7f1**, the installed version used for generation and validation. Other editor versions have not been tested; the scripts use Unity 6's `Rigidbody.linearVelocity`.
3. Allow Unity to import. The project uses **Built-in rendering**, confirmed by the empty custom render pipeline setting, and **legacy Input Manager**, confirmed by `activeInputHandler: 0`. Keep those settings.
4. Double-click `Assets/Scenes/Level01.unity`. Press Play and click inside the Game view.
5. If the scene is missing, use **Paradox > Build Level 01**. The builder creates the primitive scene, materials, UI, event system, references and build-list entry. No manual tag or layer configuration is required. Only the camera uses the standard `MainCamera` tag; objects use the Default layer.
6. To export locally, use **Paradox > Build Linux Player**. Linux Build Support must be installed. The output is `Builds/Linux/ProjectParadox.x86_64`; keep its accompanying data folder and libraries together.

For a fresh source-kit setup, create a **3D Built-in** project in this version, copy `Assets` into it, ensure **Unity UI 2.6.0** and the built-in **Test Framework 1.8.0** are present in Package Manager, set Active Input Handling to **Input Manager (Old)** or **Both**, and run the scene builder. New-Input-System-only mode is not supported by this V1. Do not import this Built-in material set into a different rendering pipeline without adapting it.

The repository started empty. Unity generated its project settings and scene in a temporary project, which were copied into the repository with their `.meta` files. No existing game work was overwritten. Unity UI is needed for the specified interface; the Test Framework is needed for the included tests. Other default project dependencies come from Unity's generated project.

## Controls

| Input | Action |
| --- | --- |
| WASD | Move |
| Mouse | Look |
| Space | Jump |
| E | Pick up the looked-at Cube; while holding, transfer near a pad, place near the plate, or drop |
| Q | Switch dimension |
| R | Reload Level01 at any time |
| Escape / click | Release / recapture the cursor |

E uses a 2.7-unit reach and a short cooldown. Nearby pads take priority while carrying. Plate placement deliberately centers the Cube to avoid fiddly precision. Move away from a pad to drop normally. Normal drops check both the destination and the path so objects cannot be pushed through a wall. When the cursor is released, movement and interaction stop; gravity continues. This is cursor release, not a pause menu.

## Solution

1. Start in White. See the far plate and door; the chasm has no bridge and the pedestal has no Cube.
2. Press Q. Black reveals both the bridge and the Cube on the left pedestal.
3. Look at the Cube and press E. Carry it along the bridge to the far platform.
4. Stand near the violet warp pad on the left. Press E when “Transfer object” appears. The Cube leaves your hands and moves to the White pad. You remain in Black.
5. Press Q. The Cube and plate are now visible in White, while the bridge disappears.
6. Pick up the Cube, approach the square plate on the right, and press E when “Place on plate” appears.
7. The door lifts and stays open. Its small cube-over-bar glyph connects the Cube and plate visually. Walk through to see “Puzzle complete”. Use Restart or R to begin again.

## Layout and scene references

Coordinates are in Unity units; Y is height and +Z is forward from spawn.

| Object | Center / placement | Size / role |
| --- | --- | --- |
| Player Spawn | (0, 0.12, -2.5) | Controller 1.8 high, radius 0.32; camera 1.6 above root |
| Start Floor | (0, -0.5, 0) | 14 × 1 × 10; top Y=0, ends Z=5 |
| Chasm | Z=5 through Z=11 | 6-unit gap; recovery trigger centered (0, -4, 8) |
| Black Bridge | (0, -0.2, 8) | 3 × 0.4 × 6 |
| Pedestal / Black home | (-3, 0.35, 1) / (-3, 1.15, 1) | Pedestal 1.5 × 0.7 × 1.5; Cube 0.8 per side |
| Far Floor | (0, -0.5, 18) | 14 × 1 × 14 |
| Matching warp pads | (-3, 0.12, 14) | White and Black pads; shared arrival (-3, 0.74, 14) |
| White plate | (3, 0, 17) | 2 × 2 base; placement (3, 0.66, 17) |
| Door | (0, 1.6, 21) | 3 × 3.2 × 0.4; lifts 4.5 units in 1 second |
| Exit zone | (0, 1.5, 23.5) | Enclosed behind the full-width exit partition |

`Systems` holds DimensionManager, PuzzleManager and GameFlow. Player references its camera, spawn, HUD, puzzle, two pads and plate. The dimension manager references the sun, camera, HUD, hands, puzzle and disabled capsule probe. The Cube references both recovery homes and its DimensionObject. Pads reference each other and their arrival markers. The plate references the puzzle; the puzzle references the plate, door and HUD. The builder assigns all of these.

## Files and scripts

| File under Assets | Responsibility |
| --- | --- |
| Scenes/Level01.unity | Editor-generated, ready-to-play level |
| Scripts/Core/DimensionManager.cs | Owns current dimension, switch guards/event and atmosphere |
| Scripts/Core/DimensionObject.cs | Toggles renderers/colliders and freezes absent/held Cube physics |
| Scripts/Core/DimensionTint.cs | Swaps shared architecture materials |
| Scripts/Core/PuzzleManager.cs | Owns the persistent door-open and completed flags |
| Scripts/Core/GameFlow.cs | Handles R and button restart through scene reload |
| Scripts/Player/PlayerInput.cs | Reads the legacy input controls in one place |
| Scripts/Player/FirstPersonController.cs | Moves, looks, jumps, applies gravity and recovers falls |
| Scripts/Player/PlayerInteraction.cs | Targets pickup, pad transfer, plate placement and safe drops |
| Scripts/Puzzle/TransferableObject.cs | Cube membership, held state and per-dimension recovery homes |
| Scripts/Puzzle/WarpPad.cs | Transfers a held Cube to its paired opposite-dimension pad |
| Scripts/Puzzle/PressurePlate.cs | Checks current physical Cube occupancy only in White |
| Scripts/Puzzle/Door.cs | Animates the latched door slab |
| Scripts/Puzzle/ExitZone.cs | Completes the puzzle when the player enters after unlocking |
| Scripts/Puzzle/RespawnZone.cs | Recovers the player or Cube from the chasm |
| Scripts/UI/HUD.cs | Labels, prompts, messages, transition flash and completion |
| Scripts/Editor/LevelBuilder.cs | Builds primitives and assigns every scene reference |
| Scripts/Editor/BuildPlayer.cs | Exports a Linux player using Unity BuildPipeline |
| Tests/PlayMode/LevelTests.cs | Integration scenarios using the generated scene and real physics |
| Materials/*.mat | Pale, dark, trim, Cube, plate, door and glow materials |
| Scripts/*.asmdef and nested definitions | Separate runtime, editor and test code |

`Assets/Prefabs` is reserved for later reuse; V1 does not need prefabs. The builder reuses existing materials, so rebuilding preserves your material edits. It replaces Level01 only after a dialog; save a customized level under another name before rebuilding. It preserves other build-scene entries.

## Add a dimension-specific object

1. Create a primitive and place it on a safe floor. Give it a material and leave its GameObject active.
2. Add **DimensionObject** to its root. Assign `Systems`' **DimensionManager** to Manager.
3. Choose White, Black, or Both in Membership. All child renderers and colliders are owned by this component; do not put unrelated objects or permanently disabled colliders under that root.
4. Play and switch both ways. Check rendering, collisions and switching while overlapping its target position.
5. For shared material changes, add **DimensionTint**, assign Manager, White, Black, and Surface (the primitive's Renderer).

DimensionObject never deactivates its GameObject. Any additional dimension-specific behavior you write should check `IsPresent` before acting; disabling a trigger collider does not disable unrelated custom script updates.

## Add another transferable object

V1 contains exactly one Cube; these steps explain how to extend it later.

1. Duplicate the existing Cube, preserving its BoxCollider, Rigidbody, DimensionObject and TransferableObject components. Keep scale 0.8 per side for the current hold and clearance settings.
2. Create separate Black and White home markers at safe positions, at least 0.4 above their supporting surfaces. Assign both homes in TransferableObject.
3. Assign its Manager, its own DimensionObject and a recovery particle effect. Set Dimension and membership consistently; V1 starts in Black.
4. Use the same pads; they accept any TransferableObject. Check that each arrival has enough space. The current level is designed and tested for one Cube, so simultaneous arrivals of multiple objects are not part of V1's guarantee.
5. Test drops, transfer in both directions, falls, and restart. The plate accepts any released White TransferableObject; change that rule explicitly if a future puzzle needs a particular object.

## Assumptions and recovery rules

- First-person, single-scene play with the specified controls. No confirmation is needed for these reversible defaults.
- Switching while holding is blocked; transfer releases the Cube first.
- Switching into newly solid geometry is blocked using a capsule penetration query. The probe and candidate collider are temporarily enabled and immediately restored within a synchronous query because never-enabled colliders can lack native shapes. No physics step occurs while testing.
- Switching on the bridge is allowed. It disappears in White; the player falls and returns to the start. Switch to Black to return across the bridge.
- The Cube's Black recovery home is the starting pedestal; its White home is the far warp pad. Transfer arrival and recovery home are separate for a return transfer to Black.
- Falling with the Cube releases it to its current dimension's home. A placed Cube, current dimension and puzzle flags survive player respawn.
- Hidden Cube physics is suspended. Switching preserves its position, membership and puzzle meaning, but discards physical velocity; momentum is not a mechanic.
- The door latches open until R/restart. Removing the Cube and switching dimensions never closes it.
- No persistent mutable static game state is used. Restart reloads the scene once even if requested repeatedly in a frame.
- The whole UI completion panel may be deactivated; dimension-specific scene objects are never deactivated.

## Manual playthrough checklist

These are **manual checks still to perform**. Automated counterparts and their results are listed separately in Validation.md; a simulated-input test is not a human keyboard/mouse playthrough.

- [ ] Start in White: no Cube or bridge, visible plate, chasm cannot be jumped across.
- [ ] WASD, mouse look and Space feel comfortable; walls and closed door block passage.
- [ ] Q reveals the Black bridge and Cube; look at Cube and E picks it up.
- [ ] Carry the Cube across; Q while holding gives feedback and does not switch.
- [ ] E near pad releases/transfers it with particles; player remains in Black.
- [ ] Q reveals White Cube; E pickup and plate placement open the door.
- [ ] Walk through the door: completion appears and cursor unlocks; click Restart.
- [ ] R during play and after completion restores spawn, White, Black Cube at pedestal, closed door and hidden completion.
- [ ] Drop Cube at start, leave it in Black while across, and return over the Black bridge to retrieve it.
- [ ] Drop Cube into the gap in each dimension; verify its current-dimension recovery home.
- [ ] Fall while holding in both dimensions; verify release/recovery and retained progress.
- [ ] Switch on bridge to White and verify fall recovery.
- [ ] Stand where hidden geometry will appear; verify switch is blocked with feedback.
- [ ] Try Q/E/R rapidly; verify no duplicated Cube or broken state.
- [ ] Place Black Cube where the White plate will be; verify door remains closed.
- [ ] Remove the Cube after opening; switch repeatedly and fall; verify door remains open.
- [ ] Inspect White/Black contrast, transition and transfer feedback, and HUD on your display.

## Known limits and deferred work

The automated tests cover scene state and physics plus the camera-targeting interaction path. They reposition the player in some scenarios and call the same interaction operation as E; physical keyboard input, mouse sensitivity, jump feel and a human end-to-end session remain unverified. Visual previews are rendered by Unity, but they are still frames rather than confirmation of animation feel. See Validation.md for current run evidence.

This is a primitive prototype. There are no added audio assets, pause menu, custom models, extra levels, level selection, save/load or advanced animation. These remain outside V1. The chosen support target is Unity 6000.6.7f1, Built-in rendering and legacy input; other platforms and editor versions need their own verification.
