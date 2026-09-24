# Unity browser build

## Build

Use Unity 6000.0.39f1 with **WebGL Build Support** installed. Close any other Unity Editor using this project, then run in PowerShell:

```powershell
./build-web.ps1
```

The script builds the current game into `UnityGame/Builds/WebGL`, with automated checks skipped at the owner's request. The compile log is `UnityGame/build-web.log`.

## Open locally

Serve the whole generated directory over HTTP; do not double-click its HTML file:

```powershell
python -m http.server 8080 --bind 127.0.0.1 --directory UnityGame/Builds/WebGL
```

Open http://127.0.0.1:8080. Browser audio may need a click/tap to start. This Unity build is separate from the old Vite/Three.js prototype on port 5173.

## Host or share

Upload the entire WebGL directory, including `index.html`, `Build`, and `TemplateData`, to a static web host. Gzip compression uses Unity's decompression fallback so a host without custom compression headers can serve the build. Keep the file names and directory layout intact.

Generated web and Windows builds are excluded from source control; rebuild them locally or distribute them as release artifacts. Pushing the repository alone does not publish a playable website. This preview does not yet integrate the Arkadium SDK.

## GitHub

Source repository: https://github.com/jppalli/bagging-order

The repository contains Unity source/settings/packages and `.meta` files, Blender source and scripts, artwork/audio, and the earlier Three.js prototype. It excludes Unity's caches, builds, logs, secrets, dependencies, and Blender backup files. Do not add Unity license files or account tokens.

### Unity 6000.0.39f1 loader workaround

`WebLoaderFix.Apply()` runs after web builds. It guards Unity's decompression-fallback worker download when a single-threaded build has no worker URL. Without it, this version's generated loader throws `Cannot read properties of undefined (reading match)` before loading game data. The workaround checks the expected generated code and stops the build if an engine update changes it, rather than silently producing a broken loader. An already repaired loader is left unchanged.

The local browser startup was inspected after this repair; game content rendered and the loading bar closed. No gameplay tests were performed.
