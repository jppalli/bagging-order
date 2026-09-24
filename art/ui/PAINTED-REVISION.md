# Painted reference revision

The first concept (`market-ui-concept.png`) is the target for this revision. Actual generated artwork now supplies the two rotation button faces and the logo sign, instead of approximating those assets with basic rectangles.

## Shipped assets

- `../../UnityGame/Assets/Resources/UI/turn-tip-painted.png`: transparent paired button sheet; Unity selects the left/right halves through UV coordinates. Labels remain live text.
- `../../UnityGame/Assets/Resources/UI/bagging-logo-painted.png`: transparent illustrated wordmark. The second generation removed its background; original alpha is preserved.
- `../../UnityGame/Assets/Resources/UI/shop-backdrop.png`: painted interior on a camera-aligned background plane, behind the playable 3D bag and conveyor.
- `../../UnityGame/Assets/Resources/Fonts/Kalam-Bold.ttf`: Google Fonts Kalam, SIL Open Font License included in `Kalam-OFL.txt`.

Runtime changes: side-by-side square rotation controls; hand-lettered display text; a hanging level tag; textured cream-bordered buttons; grain on paper and counter materials; folded bag seams; a camera closer to the front while preserving visibility of both packing rows. Gameplay rules and four-level bag dimensions are unchanged.

Generation used the built-in image tool with the first concept as style reference. The environment is a 2D background plate; the gameplay groceries and packing geometry remain interactive 3D objects.

## Rotation artwork prompt

Use the supplied image only as STYLE REFERENCE. Generate production UI artwork for the SAME game. A transparent PNG with exactly TWO chunky rounded-square sticker buttons placed SIDE BY SIDE, evenly sized, taking up most of a wide 2:1 canvas with 5% transparent padding. Left button sunny golden yellow; right button warm tomato coral red. Match the two Turn / Tip buttons at the bottom center of the reference VERY closely: imperfect hand-crafted rounded silhouettes, thick warm cream rims, forest-green offset bottom shadows, subtle painted paper grain, golden/coral inner bevels, slight dimensional lift. LEFT upper half: bold forest-green circular turning arrow icon, smooth chunky brush stroke. RIGHT upper half: bold dark terracotta tipping carton/tilting wedge icon with three little motion marks. Both LOWER halves must be completely BLANK for live text 'Turn' and 'Tip' added by Unity. Keep top/side decorative sunburst ticks tiny and near the silhouettes. No text at all, no lettering, no other objects, no scene, no frames. Actual transparent alpha background, no checkerboard printed. Make this a clean usable 2-button game sprite sheet, front-facing, NOT an interface mockup.

## Logo prompt

Using the reference as style reference only, create ONE production logo asset matching the top-left BAGGING ORDER sign in this image as closely as possible. A wide squat kraft paper grocery bag-shaped sign with small handle sticking out the top, cream inner border, dark forest green offset chunky shadow/backing, tiny leafy sprigs peeking behind. Exact text centered in two lines: 'BAGGING' in bold playful forest green hand-lettered chunky capitals, 'ORDER' in warm tomato red chunky capitals, both with fine cream outlines. Slight charming tilt about 5 degrees counterclockwise, warm textured paper-cut dimensional painted illustration, sophisticated cozy neighborhood grocery game. Strong legibility at 230px wide. Only logo, no level tag underneath, no backdrop, no panels, no other text, no watermark. Isolated on genuinely transparent alpha background, tightly composed with safe margin preserving all outline. The subject should fill canvas, landscape approximately 3:2.

Follow-up edit: remove all blurred background outside the logo silhouette and inside the handle, preserve the lettering and bag/leaf shapes, output true transparent PNG.

## Backdrop prompt

Using the supplied image as STYLE reference, create a background plate for the SAME cozy grocery packing game. Landscape 3:2. A beautifully lit neighborhood grocery interior with warm cream plaster, muted forest green window frames, gentle sun through large shop windows, rustic produce crates of tomatoes at far left and lemons at far right, small leafy plants and shelves in the back. Handcrafted paper-textured painterly 3D / illustrated game aesthetic matching the reference, warm golden daylight and soft shadows, pleasantly detailed without visual clutter. The central 55 percent of the image should remain quiet, open and softly out of focus, suitable behind a live 3D packing bag. The lower third is an EMPTY warm wooden checkout worktop with subtle grain extending to the bottom edge. Camera near frontal with a modest elevated view. Only environment: NO bags, NO groceries on the foreground counter, NO conveyor, NO buttons, NO lettering, NO logos, NO numbers, NO interface. Do not reproduce any HUD or center bag from reference. Full opaque background plate, no borders.
