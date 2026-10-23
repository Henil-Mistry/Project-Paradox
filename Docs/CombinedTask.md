# Phased expansion and five-level greybox

## Phase 0 handover
Inspected the clean committed V1.1 implementation plus an uncommitted eight-line geometry-definition change in LevelBuilder. Level01 was the only scene; no prefabs or progression system existed. Previous evidence recorded 17 passing tests for the original puzzle and carry fix. The saved scene had not received the proposed expansion. An earlier Unity launch was blocked by approval-review usage limits; the renewed authorized launch succeeded.

Acceptance: expand existing geometry without changing gameplay, verify original completion, then add Garden → Library → Mirror Lake → Observatory → Heart greyboxes using existing systems. Next action after inspection was the eight-transform scene migration, not regeneration.

## Phase 1 — completed
Files: LevelBuilder.cs, Level01.unity, LevelTests.cs, ExpansionTestResults.xml.
- Start floor: width 14→15, length 10→11, center Z 0→-0.5. Chasm-side edge remains Z=5.
- Far floor: width 14→15; center and length unchanged.
- Side walls: X ±7.25→±7.75, Z 10→9.5, length 30→31.
- Start wall: Z -5.25→-6.25; both end walls width 15→16.
- Exit partitions: center X ±4.25→±4.5, width 5.5→6; inner edges remain ±1.5.
- Floor area 336→375 square units (+11.6%). Shared geometry keeps both dimensions aligned.

Unity edited the existing scene, preserving every serialized object ID and reference. Audit confirmed only eight transforms changed; all puzzle transforms stayed identical. Unity also normalized three single-burst repeat intervals and an unused best-fit font maximum when saving; no effective gameplay changes.

Unity 6000.6.7f1 compiled and passed 18/18 graphics-enabled Play Mode tests in 24.71 seconds. New tests exercised both dimensions' added floor strips, back extension, wall collision, exit-partition joins and absence of a chasm bypass. Existing tests verified initial scene, interaction, transfer, completion, restart, recovery and carry stability. Level01 starts, plays and completes in these automated scenarios. Manual keyboard/mouse playthrough was not performed. No unresolved Phase 1 failures.

## Phase 2 architecture and layouts
Level01 remains the entry point and becomes Garden through additive primitive scenery. Its original puzzle transforms, collider sizes and serialized references remain in place. Four NEW scenes reuse its configured components. No movement, camera, transfer, dimension, pressure-plate or door implementation was replaced.

- **Garden / Level01:** original crossing and transfer puzzle with perimeter planters, geometric flowers and a bench. One flower briefly flickers every nine seconds.
- **Library / Level02_Library:** reading table, shelf aisles, books and a far-side aisle divider. The original central crossing leads to a route around the divider. Symbols above the table shift intermittently as placeholders for escaping letters.
- **Mirror Lake / Level03_MirrorLake:** the Black crossing is offset left with broad ice landings; the Cube pedestal starts on the right. A non-solid lake surface occupies the chasm below the walking route. Mirrored monolith fragments visibly misalign as a cheap reflection placeholder. The right lakeside obstacle creates an approach around its outside.
- **Observatory / Level04_Observatory:** a right-hand crossing and approach barrier lead into a ring of pillars. A large clock face, skipping hand and eclipse disc provide stronger intermittent glitches. There is no player-operated time mechanic.
- **Heart / Level05_Heart:** entrance fins and a core chamber surround an abstract three-part heart. Players route around the core to the existing transfer/plate/exit puzzle. Core segments and echoes flicker, shift and stutter most frequently. On normal puzzle completion, all VisualGlitch components restore their authored local pose and visibility and remain still.

Progression is explicit: the existing completion panel and Restart remain; a Next button advances Garden → Library → Mirror Lake → Observatory → Heart only after completion. Heart has no Next button. R and Restart reload the active scene. Each level begins in White with an unopened door and Black Cube. Glitches affect decoration only, never collision geometry, camera or puzzle state.

Source changes: GameFlow adds guarded next-scene loading; GreyboxLevels is an editor-only creation tool that refuses to overwrite existing new scenes; VisualGlitch is an isolated per-object effect; BuildPlayer exports all enabled build scenes. HUD labels and Next button wiring are authored into the scenes, with no change to HUD runtime code. Test additions cover expansion and the complete five-level sequence. The old Build Level 01 command is still a destructive regeneration tool with a confirmation; do not use it to apply these changes or it will remove Garden dressing. Edit the saved scenes instead.

Visual assets use existing materials, Unity primitives and TextMesh only. No packages were added. Physics support and recovery bounds are reused. The lake surface is deliberately non-solid: falling into the original chasm still recovers the player.


## Validation evidence and limits
- Phase 1: 18/18 Play Mode tests passed, including added-space/perimeter checks and original completion/restart. Evidence: ExpansionTestResults.xml.
- Initial Phase 2 route suite: 19/19 passed in 84.97 seconds. It walks the CharacterController through every scene, uses camera targeting for pickup/transfer/plate placement, triggers each exit, restarts every scene, checks incomplete-level Next rejection, and invokes every saved Next button. Heart remains stable for 1.3 seconds after completion.
- Reference audit: all five scenes opened and 1,935 components checked without missing scripts or broken serialized references.
- Identity audit: all 277 original Level01 serialized object IDs retained. Phase 1 modifies eight non-critical transforms; Phase 2 preserves every original gameplay Transform and collider. Existing player, input, camera, interaction, dimension, transfer, and puzzle-state source files compare byte-for-byte with the committed version.
- Preview capture initially exposed reversed letters, submerged reflection fragments and weak clock-hand contrast; these were corrected in decoration only. A temporary audit helper initially used an obsolete Unity API; it was corrected to the installed Unity 6 API before the successful audit. Temporary migration/audit helpers are not shipped.
- Source whitespace checks pass; Unity's generated scene serialization contains its normal trailing spaces on empty fields. These were left as generated rather than rewriting scene text.

**Not performed:** a human keyboard/mouse playthrough of all five levels; subjective difficulty, timing and comfort review; every possible traversal or drop location; exports on other operating systems. Automated walks supply controller movement and invoke the same E interaction operation, rather than hardware key events. The lake reflections and flying letters are deliberately lightweight visual placeholders. No required mechanics were deferred.


Final gameplay suite: **20/20 passed, 0 failed/skipped**, in **87.21 seconds**, including the five-layout render test. Evidence: FiveLevelTestResults.xml. After that run, the clock hand and reflection fragments received material-only dimension-tint corrections; their final compilation, reference audit and Linux build succeeded. The final reference audit covers **1,937 components**. No transforms, colliders, gameplay code or routes changed after the full suite. A targeted graphics test recaptures these final material corrections separately.

The Linux build includes all five scenes and is available locally in `Builds/Linux/ProjectParadox.x86_64`. It is generated output excluded from Git. A manual standalone playthrough of this five-level build has not been performed. No remaining execution blockers; the earlier approval-review block cleared on the renewed request.

Final material-only visual regression: **1/1 passed** (all five scenes rendered), 2.45 seconds. Updated Library, Mirror Lake and Observatory captures were inspected. Evidence: GreyboxVisualTestResults.xml; images in GreyboxPreviews/.
