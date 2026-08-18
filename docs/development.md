# Development guide

## Version pins

- Unity Editor: `6000.0.81f1` (Unity 6.0 LTS)
- Universal Render Pipeline and SRP Core: `17.0.4`
- Unity Input System: `1.20.0`
- Unity Test Framework: `1.6.0`
- Fixed simulation timestep: 60 Hz
- Active input handling: new Input System only

Do not casually retarget the project. Flight, input, rendering, and WebGL builds have been verified together on these versions.

## Known editor and package traps

### Unity 6.3 test failure

Unity `6000.3.22f1` was tested locally with its bundled URP `17.3.0`. The bundled package referenced `MaterialReferenceChanger` from `ReadonlyMaterialConverter`, but that type was not present, so a clean URP project could not compile before any Aeronauts 2 code was involved.

The installed package hashes matched their shipped `.sha1` files, which ruled out a corrupt download. The project therefore remains on Unity `6000.0.81f1`. Re-evaluate 6.3 only against a newer patch and a clean URP project first.

### Input System 1.12.0

Input System `1.12.0` failed on the tested modern editor because it referenced the unavailable `BuildTarget.ReservedCFE` member. The project uses `1.20.0`; do not downgrade without compiling a clean test project first.

### Unity command line

For automation, call the editor executable directly or use the separate `unity` CLI. Unity Hub `3.20.1` did not accept its older documented `-- --headless` invocation during setup.

Some Unity CLI module-listing commands reported Web/Windows support even when the playback-engine folder was absent. Verify modules on disk under:

```text
<Unity editor>/Editor/Data/PlaybackEngines/
```

The expected folders are `WebGLSupport` and `windowsstandalonesupport`.

## Rebuild the generated Flight Lab

`Assets/Aeronauts2/Editor/FlightLabSceneBuilder.cs` owns the reproducible greybox scene. Rebuild after changing its composition code:

```powershell
$unity = "$env:LOCALAPPDATA\Programs\Unity\Hub\Editor\6000.0.81f1\Editor\Unity.exe"

& $unity -batchmode -quit -nographics -projectPath . `
  -executeMethod Aeronauts2.Editor.FlightLabSceneBuilder.Build `
  -logFile .\Logs\flightlab-build.log
```

The builder also ensures the Flight Lab is the enabled build scene and creates missing tuning/material assets.

## Automated tests

```powershell
$unity = "$env:LOCALAPPDATA\Programs\Unity\Hub\Editor\6000.0.81f1\Editor\Unity.exe"

& $unity -batchmode -nographics -projectPath . `
  -runTests -testPlatform EditMode `
  -testResults .\Logs\editmode-results.xml `
  -logFile .\Logs\editmode-tests.log

& $unity -batchmode -nographics -projectPath . `
  -runTests -testPlatform PlayMode `
  -testResults .\Logs\playmode-results.xml `
  -logFile .\Logs\playmode-tests.log
```

The current verified baseline is 30 passing EditMode tests and 7 passing PlayMode tests.

## WebGL build and browser gate

Build with the project-owned editor method:

```powershell
& $unity -batchmode -quit -nographics -projectPath . `
  -executeMethod Aeronauts2.Editor.Build.BuildPipeline.BuildWebGL `
  -logFile .\Logs\webgl-build.log
```

Serve the Brotli-compressed output with the supplied header-aware server:

```powershell
python .\tools\serve_webgl.py --directory .\Build\WebGL --port 8008
```

The browser smoke test attaches Playwright to a dedicated Chrome DevTools Protocol port. Ports `9222` through `9226` are reserved for persistent automation on the original development workstation; use `9230` or higher for a scratch profile.

```powershell
Start-Process "C:\Program Files\Google\Chrome\Application\chrome.exe" -ArgumentList @(
  '--remote-debugging-port=9230',
  "--user-data-dir=$PWD\Logs\browser\chrome-profile",
  '--remote-allow-origins=*',
  '--no-first-run',
  '--no-default-browser-check')

python .\tools\browser_smoke.py `
  --cdp http://127.0.0.1:9230 `
  --url http://127.0.0.1:8008/ `
  --output-dir .\Logs\browser
```

The gate checks the rendered Unity canvas, pointer lock, keyboard/mouse input, both aim profiles, both camera modes, flight alignment, player weapons, shield gating, bot flight/fire, duel telemetry, browser errors, network failures, screenshots, and frame timing.

## Repository hygiene

Unity-generated `Library`, `Temp`, `Logs`, `Build`, and `UserSettings` folders are ignored. IDE project files are ignored. Large authored 3D, image-source, audio, and video formats are configured for Git LFS in `.gitattributes`.
