# Bagging Order — Unity game

The current game lives in **UnityGame/** and uses Unity **6000.0.39f1** with Blender source assets in **art/**. It includes 21 authored grocery-packing levels, drag/drop controls, a journey map, and original cozy audio.

- Open `UnityGame` from Unity Hub to work on the current game.
- Build and web-preview instructions: [WEB-BUILD.md](WEB-BUILD.md).
- Level design: [UnityGame/AHA-LEVELS.md](UnityGame/AHA-LEVELS.md).
- Detailed project history: [UnityGame/README.md](UnityGame/README.md).

Unity caches, generated builds, logs, local preferences, and Blender backup files are excluded from Git. Keep Unity `.meta` files alongside their assets.

## Earlier Three.js prototype

The following describes the earlier browser prototype retained for reference. `npm run dev` runs that prototype, **not the current Unity game**.

Browser prototype for a 3-wide × 2-deep grocery packing puzzle.

## Run

`npm install`, then `npm run dev`. Run `npm test` for rule checks and `npm run build` for production output.

## Controls

Drag a grocery from the bottom conveyor onto a glowing landing spot in the bag and release to pack. Front/back placement follows the nearest spot. Dropping outside cancels. On touch screens the drag preview sits above your finger. You can also tap a grocery, then tap a landing spot. Rotate item (or R) changes orientation; Escape cancels an active drag. Undo restores the previous state including damage and bag changes. Restart retries the current level.

Groceries settle onto the highest overlapping support and require support beneath their complete footprint. Their own weight plus carried load divides equally across supporting cells. Crushing is reversible through undo; damage otherwise persists. Three stars require no damage and the target bag count; two stars allow one crushed item or extra bags; multiple crushed items earn one star.

## Blender assets

Built-in procedural models keep the prototype playable without Blender. Optional Blender GLB assets override them using `public/assets/models/manifest.json`, e.g. `{ "milk": "/assets/models/milk.glb" }`.

Model dimensions use one unit per grid cell, with a small visual margin. Center the object horizontally, put its bottom at zero, and export glTF with Y-up (default export axis conversion). Front faces +Z. Supported keys: can, milk, cereal, eggs, bread, apple, tray. Match dimensions in `src/game.js`. Use simple Principled materials or baked textures; export only the selected asset and apply transforms. The app uses the grid for support, not the visible mesh shape.

## Prototype limits

Five handcrafted levels; local progression only. No live Arkadium SDK, ads, purchases, or leaderboard. Before Arkadium delivery, connect its SDK lifecycle and persistence (replacing localStorage), verify current publisher requirements, and test touch controls and loading on real mobile devices. Generated artwork is placeholder art for the Blender production pass.

Bags start at three cells high in levels 1–2, then grow to four and five cells. Landing spots have no visible row labels; accessible names remain available.
