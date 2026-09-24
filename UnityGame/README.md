# Bagging Order — Unity + Blender

Unity 6.0.39f1 project. Open `Assets/Scenes/Checkout.unity` and press Play. The `Bagging Order` editor menu builds a Windows preview or WebGL player.

## Gameplay

Drag actual 3D groceries off the conveyor and release over a landing spot. R or right-click turns horizontally; F or the mouse wheel tips vertically. You can also tap a grocery, use the floating Turn / Tip buttons beside it, then drag or tap a landing spot. Tap empty board space or press Escape to cancel; Z or Undo reverses a placement. The original twelve levels use a 3-wide, 2-deep, 4-high bag; experiments 13-15 vary the dimensions or fill objective. Choose level opens all teaching levels; Shopping list shows remaining groceries.

Rotations snap to distinct occupied shapes. Flowers must stay upright. Banana bunches occupy a simple two-cell footprint and bruise under heavy groceries. Weight and support capacity stay unchanged when rotated. Every exposed underside cell needs support. Cold objectives require all cold groceries to connect through faces in one bag; small snowflake seals identify them. There is no cold timer. Stars: one for completion, one for no damage, one for the displayed objective.

## Art

`../art/blender/` contains thirteen editable Blender source files. `../art/create_groceries.py` regenerates them and their FBX exports in `Assets/Resources/Groceries/`. Optional arguments after `--` select model names to regenerate. Unity imports the model materials and normalizes each visual to its grid dimensions. The environment and bag are constructed by the Unity runtime.

## Validation

`BuildGame.Windows` checks weight transfer, height, row independence, rotations, upright restrictions, the banana footprint and cold adjacency. A search using the actual placement rules verifies a damage-free, three-star solution within the advertised bag target for every level, selecting only from the first three conveyor items. It writes `VERIFIED-SOLUTIONS.md`. These checks establish solvability; playtesting is still needed for difficulty tuning.

Launch the Windows build with `-baggingSmokeTest` to exercise conveyor replenishment, drag targets, hover fading, completion, undo, tipping, upright restrictions, the natural banana model and cold markers. It writes preview images and exits. Latest build: `Builds/WindowsJourneyMap/BaggingOrder.exe`.

## Arkadium delivery

This is a local prototype. Install the matching Unity WebGL Build Support module to build for Arkadium. The live SDK, ads, and leaderboard are not integrated. Replace prototype PlayerPrefs saving with Arkadium persistence and connect lifecycle pause/resume before submission. Mobile browser performance and loading require testing in the WebGL build.

Typography uses bundled Lato Regular and Bold (SIL Open Font License, included beside the fonts). The conveyor advances when groceries need replenishing, moves surviving products forward, and feeds the next product from the right. Picking is paused during delivery; undo and restart cancel and rebuild delivery safely. Pickup, rotation, placement, and the completion panel are animated.

Product-name captions are removed from the conveyor. Every bag is four cells high. Front-row groceries turn translucent while hovering behind them and return to normal when moving away, cancelling, or dropping.


## Neighborhood grocery interface

The market UI uses a generated transparent bag emblem, cream paper cards, colorful sticker buttons with press animation, a progress meter, leaf decorations and a receipt result panel with sequential star animations. Text remains editable. UI code is in Assets/Scripts/BaggingGame.UI.cs; art direction and generation prompts are in ../art/ui/ART-DIRECTION.md. The concept is a style reference; the existing 3D shop and grocery models are retained.


## Painted reference revision

Turn and Tip now use painted square artwork side by side, with live Kalam labels. The logo uses the illustrated bag sign. A warm illustrated shop sits behind the playable 3D board; paper and wood grain, folded seams, and a more frontal camera connect the scene to the concept. See ../art/ui/PAINTED-REVISION.md for asset paths, prompts and font licensing. Rotation buttons stay in a stable dock beneath the selected product.


## Grocery detail and drag visibility

The bag stays opaque while dragging. A dedicated camera keeps the held grocery visible, including invalid positions behind the bag. Placement requires the pointer to be over a valid supported footprint; invalid drops do not change the queue. Only packed front-row groceries fade when needed to see a rear placement.

All thirteen Blender groceries have refined geometry, packaging details and exported grain textures. The detail pass lives in ../art/grocery_detail.py, called by create_groceries.py. Texture colors use a neutral material tint to prevent double tinting in Unity. Editable sources remain in ../art/blender/. Run ../art/render_product_gallery.py in Blender to refresh ../art/product-gallery.png.


## Interaction polish

Groceries remain full size on the widened conveyor, while dragging, and when packed. Pickup lifts gently; parked Turn/Tip rotates around the grocery center. Dragging eases toward supported positions, hovering slightly above the landing surface before settling on release without a bounce. Invalid releases return to the conveyor.

Soft shadows baked from the actual rotated product silhouette show available landing positions. The active shadow is emphasized; red indicates crushing. The bag remains opaque, while front-row groceries fade when needed to see a rear placement. Model bounds fill packing dimensions, and damage uses a warm tint rather than shortening the support below other products.

All interface text uses the bundled Kalam font, including instructions, counters, shopping lists, level selection and results. Illustrated logo and product packaging remain artwork.

Automated checks cover full-size pickup, visible shadow previews, tap targets, opaque bag, invalid drops, return animation, rotation, checkout, undo and level mechanics.

## UI finish

Rounded UI surfaces use Unity�s rounded texture drawing with subtle shared paper grain, soft layered shadows and animated button feedback. Decorative stamps use supersampled edge coverage. The grocery counter includes an eased gold progress bar and remaining quantity; its input exclusion matches the larger card. Kalam remains the interface font.

## Bag navigation

The open bag is highlighted in numbered tabs with per-bag item counts. Previous/Next moves between existing bags; New bag reuses an empty bag or creates one. Bags and contents slide together, with packing disabled during the transition. Navigation can be undone. Grocery progress remains checkout-wide. Level 6 introduces multiple bags explicitly; LEVEL-DESIGN.md describes the curriculum and generator requirements.

## Teaching curriculum

Twelve groceries and eight authored lessons now introduce packing, protection, turning, tipping, upright restrictions, multiple bags and cold grouping. Bananas have a natural four-finger model with a simple two-cell footprint; rice adds a sturdy pantry box. The editor solver verifies the first-three conveyor constraint across multiple bags. See LEVEL-DESIGN.md and VERIFIED-SOLUTIONS.md. This is the validated foundation for a future generator. The curriculum has its own progression save key, so it starts at lesson one.

## Puzzle interaction pass

Bag limits are enforced. Tap or drag an uncovered packed grocery to rearrange it within the open bag; supporting groceries must be uncovered first. Cancel restores the original position, and Undo reverses a completed move. Product tags explain weight, crushability, cold and upright properties beside the rotation controls. Three new challenges require rotation geometrically. The build writes DIFFICULTY-AUDIT.md alongside the verified solutions.

## Family cornflakes

A separate 2-wide x 3-high x 1-deep cornflakes carton joins the smaller oats box. It is light (weight 1), supports up to 3 load units, and can turn and tip. Checkout 12, Family size, packs three family boxes plus milk, rice and soup into one bag. The custom cream, red and golden-yellow package is authored in Blender.

## Drag polish

The property tag and rotation controls use a stable viewport dock. Landing shadows are more visible and accept a small targeting margin, with sticky selection to reduce flicker between neighbors. Held groceries lift before crossing packed stacks and then lower toward the landing shadow. This preview was built with -baggingSkipChecks at the player's request; gameplay testing is left to the player.

## Pointer-following drag

The held grocery now follows the pointer without snapping, lifting over stacks, or moving toward a candidate. Only the landing shadow selects a grid position. Groceries visually overlapped by the held object fade and restore when clear; the bag stays opaque. Releasing over a valid candidate uses the short settle animation. Controls and property tags keep their stable dock. Built without automated tests at the player's request.

## Neighborhood map and bag experiments

The preview opens on an illustrated, scrollable neighborhood map. All fifteen checkouts are unlocked. Select a numbered stop to preview its bag dimensions, objective and best stars, then Play checkout. Map is available from gameplay and the results receipt. Best stars and tried status are saved locally by level name; previous builds did not store individual stars, so those earlier results cannot be recovered.

- **13 / Breakfast to go:** 3 wide x 2 deep x 2 high. Two large cornflakes cartons must fill every cell.
- **14 / Narrow aisles:** 2 wide x 2 deep x 4 high. Tip the milk below tall cornflakes cartons.
- **15 / Ready to seal:** standard bag, level top at height 3. Fill every column through the gold line with full support. A paper flap folds closed on completion.

Perfect-fit and seal objectives are required to complete their experiments. If all groceries are packed but the target is not met, players can keep rearranging. Weight balance is not added. The new preview is compiled with automated checks skipped, and has not been playtested by the assistant.

## Journey map

The map is now a vertically scrolling trail starting with checkout one at the bottom. Large round numbered stops show best stars, the current level has a marker, and experiments carry NEW badges. Shop-themed districts decorate the path. Tap a stop for a modal bag/objective preview and Play; there is no permanent side panel. Drag or scroll to explore; Current level and New stops jump along the trail. All levels remain unlocked. Compiled only, without automated tests or gameplay testing.

## Cozy map and audio preview

Latest preview: `Builds/WindowsCozy/BaggingOrder.exe`.

The journey now uses a generated painted neighborhood backdrop with readable paper level labels. Wins hold the board for three seconds with drifting stars and leaves, a rising musical phrase, and then a separate receipt/star reveal. An original 50-second looping score combines soft harmonic keys and plucked tones, with gentle pickup, placement, rotation, paper, and button sounds. Sound on/off mutes both music and effects and persists between sessions.

Audio source: `../art/create_cozy_audio.py` (original synthesis, no sampled recordings). Map provenance: `../art/ui/COZY-MAP.md`.

Compiled with `-baggingSkipChecks`; no automated tests or playtesting, as requested. Visual timing and audio balance are ready for user evaluation.

## Cozy UI fixes

Latest preview: `Builds/WindowsCozyPolish/BaggingOrder.exe`.
Turn and Tip use the same orientation rules as gameplay to disable unavailable actions. The receipt is a single antialiased paper silhouette, including its torn edges. The journey path is a cached antialiased continuous texture instead of overlapping rectangular segments. Build checks and playtesting remain skipped at the user's request.

## Softer interaction audio

Latest preview: `Builds/WindowsSoftSounds/BaggingOrder.exe`.
Replaced pitched click, pickup, turn, and placement notes with short filtered paper/cloth contact effects, rounded attacks, and lower levels. Music and win chimes are unchanged. Regenerate these four effects with `python art/create_cozy_audio.py --effects-only`. Compile only; no listening tests or playtesting were run.

## Simplified play screen

Latest preview: `Builds/WindowsSimpleUI/BaggingOrder.exe`.
Disabled rotation buttons retain the painted artwork instead of switching to rough line icons. Main HUD retains the goal and a single packed counter/progress bar. Undo stays accessible; multi-bag levels show numbered tabs and a contextual add button. Menu contains resume, shopping list, map, restart, sound, and bag dimensions/allowance. Removed duplicate bag counts, item counts, repeated progress text, oversized logo and permanent instruction strip. Product traits remain contextual, with weight labels instead of numerical weight data. Input blocking follows the new panels and menu. No gameplay or automated tests run.

## Clean results and aligned stars

Latest preview: `Builds/WindowsCleanResults/BaggingOrder.exe`.
Map nodes, level previews, and completion tickets share centered, evenly spaced, unrotated star rows. The completion ticket now shows only checkout number, stars, a short message, Next level, Replay, and Map. Map stops have additional vertical space so district headings sit between level labels and the current-level marker. Compile only, with checks and playtesting skipped.

## Crushing feedback

Latest preview: `Builds/WindowsCrushFeedback/BaggingOrder.exe`.
A risky landing pulses the affected groceries amber and labels the victim "Too heavy". New damage adds a short squash, a paper crumple sound and a small paper puff. Damaged groceries retain a side crease and muted bruising; their contextual tag names the grocery that caused the damage. Damage provenance survives rearrangement and snapshot-based undo. Mesh deformation is visual only and preserves the logical support grid. No automated checks or playtesting run.

## Gentle damage and star-based results

Latest preview: `Builds/WindowsGentleResults/BaggingOrder.exe`.
Fixed the logged MaterialPropertyBlock constructor error by creating it in Initialize on the main thread. Damage now uses a 3.5% side crease, 2.5% brief compression, a faint tint, and a quieter paper sound; removed the puff and normal recalculation that exaggerated shading changes. Three stars get "Perfectly packed!", the full chime and celebration; two stars get "Checkout complete", a short faded chime and fewer particles; one star gets "Room to improve" with no confetti or victory chime. Results give one short improvement hint when below three stars. Compile only; no gameplay or automated tests run.

## Centered celebration

Latest preview: `Builds/WindowsCenteredCelebration/BaggingOrder.exe`.
Completion message and particles share the viewport center. Particles emit in mirrored pairs so both two-star and three-star celebrations remain balanced regardless of bag size or camera position. Compile only; no playtesting or automated checks run.

## Clean rotation controls and balanced celebration

Latest preview: `Builds/WindowsCleanControls/BaggingOrder.exe`.
Turn/Tip now use supersampled code-drawn icons and the shared rounded paper buttons, removing the speckled sprite edges and atlas sampling from these controls. Disabled states use the same geometry. Celebration particles emit in matching groups of four with identical timing and mirrored positions on both axes, centered on the message with no gravity/drift. Compile only, no playtesting or automated checks.

## Six discovery puzzles

Latest preview: `Builds/WindowsDiscoveries/BaggingOrder.exe`.
Open Map → New puzzles for levels 16–21 in Little discoveries: The narrow strip, The breakfast lid, Soft landing, Save the headroom, A chilled corner, Two different jobs. Existing levels and scores remain intact. All six are exact-fill designs; cold grouping is introduced after shape, ordering and headroom. AHA-LEVELS.md records intended discoveries and hand-authored constructions, not executed solver results. The map supports the longer chapter and caps the trail texture to the device size limit. Compile with checks skipped; no automated solver or gameplay tests run.

## Bag button alignment

Bag tabs and the add-bag button now use one shared renderer, a consistent 17-point font, centered single-line text, and 76-unit widths. The selector centers only the visible buttons, removing the empty add-button space when the bag limit is reached. Web build compiled with checks skipped; no playtesting run.
