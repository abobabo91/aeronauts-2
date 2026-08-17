# Aeronauts 2

Foundation for a high-skill competitive aerial arena shooter. The current playable is an offline greybox Flight Lab with fixed-tick flight, keyboard/controller input, independent configurable aim, a centered reticle, bank-to-turn movement, momentum management, timed airbrake exits, finite-speed primary projectiles, an instant railgun, precision target regions, and live movement/combat diagnostics.

`Aeronauts 2` is an internal working title. Before any public release, confirm naming rights and replace all references if necessary. Do not reuse assets, characters, audio, branding, or code from the 1999 PlayStation game *Aironauts*.

## Current stack

- Unity `6000.0.81f1` (Unity 6.0 LTS)
- Universal Render Pipeline `17.0.4` (builtin, with SRP Core `17.0.4`)
- Unity Input System `1.20.0`
- Unity Test Framework `1.6.0`
- Web Build Support and Windows IL2CPP Build Support — both verified directly under the editor's `PlaybackEngines` folder
- Blender `5.2.0 LTS`
- Git LFS `3.5.1`
- Visual Studio Code `1.133.0` with Microsoft C#, C# Dev Kit, and Unity extensions

The browser demo and later Windows/Steam build are intended to share one Unity project. Multiplayer packages are deliberately not installed yet; movement and aiming must be proven in an offline flight lab before the networking model is locked.

A native Windows build additionally requires Visual Studio 2019 or later with C++ Tools and Windows SDK `10.0.19041.0` or newer. That administrator-level prerequisite is deferred until native/Steam builds begin; it is not needed for the browser-first prototype.

### Do not move this project to Unity 6.3 LTS

Unity `6000.3.22f1` cannot compile a URP project at all, including the URP template Unity itself ships inside that editor:

```
ReadonlyMaterialConverter.cs(159,18): error CS0246:
The type or namespace name 'MaterialReferenceChanger' could not be found
```

`MaterialReferenceChanger` is referenced twice in `com.unity.render-pipelines.universal` `17.3.0` (`Editor/Tools/Converters/ReadonlyMaterialConverter/`, plus its test) and is defined nowhere — not in URP, not in SRP Core `17.3.0`, not in the editor's managed assemblies. The shipped packages are byte-intact (each `BuiltInPackages/<pkg>.sha1` matches its `Library/PackageCache/<pkg>@<hash>` folder), so this is not a broken install, and `6000.3.22f1` is the newest 6.3 LTS available. Every other type in that file resolves, so it is a single dangling reference in a Built-in→URP migration tool.

Patching it locally would mean deleting `ReadonlyMaterialConverter` *and* dropping the `typeof(ReadonlyMaterialConverter)` registration at `Editor/Converter/Converters.cs:115` inside the Unity install — machine-local, lost on reinstall, and invisible to a fresh clone. The project targets 6.0 LTS instead. Re-test 6.3 only when a patch newer than `6000.3.22f1` ships.

Input System `1.12.0` — the version Unity's own 6.3 template pins — is separately broken on modern editors: it references `BuildTarget.ReservedCFE`, which no longer exists (`error CS0117`). Fixed at or before `1.14.2`; this project uses `1.20.0`, the first release declaring `unity: 6000.0` as its floor.

## First open

The project imports clean on `6000.0.81f1`: batch import exits `0` with an empty Console — no compiler errors, no warnings. A headless import is the fastest way to confirm that after any package or editor change:

```powershell
& "$env:LOCALAPPDATA\Programs\Unity\Hub\Editor\6000.0.81f1\Editor\Unity.exe" `
  -batchmode -quit -nographics -projectPath . -logFile .\Logs\import.log
```

Exit code `0` means the project compiled. Exit code `1` prints `Aborting batchmode due to failure: Scripts have compiler errors.`, and the offending lines are in the log as `error CS####`.

To open the editor normally, use `unity open --project <this folder>`, or add the folder in the Hub and open it with `6000.0.81f1`.

Licensing is per-machine, not per-project. A fresh workstation needs:

```powershell
& "$env:LOCALAPPDATA\Unity\bin\unity.exe" auth login                          # opens a browser; complete it within ~2 min or it exits "Sign-in timed out"
& "$env:LOCALAPPDATA\Unity\bin\unity.exe" license activate --personal --accept-eula
```

Without a license the editor still starts, resolves packages and then exits before importing; `Unity.Licensing.Client.log` shows `Failed to update license file. [Code: 401] Token not found in cache` followed by `No ULF license found`.

Use the `unity` CLI, not the Hub's old CLI. Hub `3.20.1` no longer accepts the documented `-- --headless` form — it fails with `Cannot find module '--headless'`, and the bare `--headless` flag fails with `bad option`. The `unity` CLI (`1.0.0-beta.5`) covers `auth`, `license`, `install`, `open`, `run`, `build` and `test`.

Two `unity` CLI behaviours worth knowing, both observed here: `install --dry-run` prints nothing at all, and `unity editors` reported `modules=Web, Windows` for an editor whose Web module had actually failed to install. Confirm modules against `Editor/Data/PlaybackEngines/` on disk rather than trusting the CLI.

Installed tool locations on this workstation:

- Unity CLI: `C:\Users\abele\AppData\Local\Unity\bin\unity.exe` (`1.0.0-beta.5`)
- Unity Editor: `C:\Users\abele\AppData\Local\Programs\Unity\Hub\Editor\6000.0.81f1\Editor\Unity.exe`
- Unity Hub: `C:\Program Files\Unity Hub\Unity Hub.exe` (`3.20.1`)
- Blender: `C:\Users\abele\AppData\Local\Programs\Blender\5.2.0\blender.exe`

`6000.3.22f1` was tested, found unusable for this project, and removed from this workstation. Do not reinstall or retarget to Unity 6.3 until a patch newer than `6000.3.22f1` has been verified against a clean URP project.

### Build support verification

`6000.0.81f1/Editor/Data/PlaybackEngines/` contains both `WebGLSupport` (4.05 GB) and `windowsstandalonesupport` (0.79 GB). A complete WebGL build now succeeds through `Aeronauts2.Editor.Build.BuildPipeline.BuildWebGL`; the first cold-cache build took 10m37s and produced a 12.3 MB local output including a 7.6 MB Brotli-compressed `.wasm.br`. Build output stays ignored under `Build/WebGL`.

Do not trust `unity editors` or `install-modules --list` for module verification. Both have reported modules as installed when their playback-engine folders were absent.

Blender portable archive SHA-256: `2D184B626C001692C362291911293B6A297179D618D95E9E9192C3A80318ADC4`.

## Repository layout

- `Assets/Aeronauts2/`: all first-party game content
- `Assets/ThirdParty/`: imported third-party assets, each with its license
- `Packages/`: pinned Unity packages
- `ProjectSettings/`: shared Unity configuration
- `docs/`: durable gameplay and technical decisions

## Flight Lab development

Open `Assets/Aeronauts2/Scenes/FlightLab/FlightLab.unity` and iterate in Editor Play Mode. The current controls and tuning are documented in `docs/controls.md`; the editable `FlightTuning`, `AimTuning`, `InputTuning`, `WeaponTuning`, and `TargetTuning` assets are under `Assets/Aeronauts2/Settings/`.

The current movement skill loop is roll to choose a turning plane, pitch through the turn, use airbrake when a tighter hook is worth the speed loss, then tap boost shortly after release for an earned slingshot burst. Airbrake overrides acceleration and boost while held. There is no scripted 180-degree action: a reversal is performed through the same airbrake-and-steering physics. Hold Q/E (controller bumpers) for manual roll or double-tap one side for a committed 360-degree roll.

Combat deliberately separates two aim skills. Left mouse/right trigger fires exact, zero-gravity primary bolts at `120` world units/second, so moving targets require lead. Right mouse/left trigger fires an instantaneous hitscan railgun with a three-shot reserve. The centered camera ray selects the intended point, but both authoritative shots originate at the center of the shared head/cockpit region. Player-owned colliders are ignored by the shot query; cover between that origin and the point absorbs the shot even when the third-person camera can see around it, and the HUD warns `HEAD ORIGIN BLOCKED`. Body and small cockpit regions report separate hits; shield, hull, damage, cooldown, and reserve values are placeholders in data assets for later playtesting.

Aim View and Forward View are gameplay camera states. Aim View orbits the camera around the aircraft with independent weapon aim, allowing side/front views of the shared frame while the centered crosshair remains the firing direction. Forward View freezes independent aim and aligns camera, crosshair, and weapons with body forward. Press `C` or the controller right stick to toggle; returning to Aim View begins from the current forward direction rather than snapping to a stale angle.

Aim limits are body-relative and live in `AimTuning.asset`. The default constrained profile permits `110` degrees to either side and `78` degrees up/down; the full-360 profile permits `180` degrees to either side and `89` degrees up/down. The FlightLab-only comparison switch is `V` or controller Select. Turning the body does not drag an in-bounds Aim View camera, while crossing a configured edge clamps it; resetting flight recenters aim on the body.

Rebuild the generated greybox scene after changing its editor utility:

```powershell
& "$env:LOCALAPPDATA\Programs\Unity\Hub\Editor\6000.0.81f1\Editor\Unity.exe" `
  -batchmode -quit -nographics -projectPath . `
  -executeMethod Aeronauts2.Editor.FlightLabSceneBuilder.Build `
  -logFile .\Logs\flightlab-build.log
```

Run the fixed-tick movement, input-gesture, weapon-state, damage-region, and structure tests:

```powershell
& "$env:LOCALAPPDATA\Programs\Unity\Hub\Editor\6000.0.81f1\Editor\Unity.exe" `
  -batchmode -nographics -projectPath . -runTests -testPlatform EditMode `
  -testResults .\Logs\editmode-results.xml -logFile .\Logs\editmode-tests.log
```

Run the scene startup smoke test by changing `EditMode` to `PlayMode` and the two output filenames to `playmode-results.xml` and `playmode-tests.log`.

### WebGL build and real-browser smoke test

Produce the Brotli-compressed browser build:

```powershell
& "$env:LOCALAPPDATA\Programs\Unity\Hub\Editor\6000.0.81f1\Editor\Unity.exe" `
  -batchmode -quit -nographics -projectPath . `
  -executeMethod Aeronauts2.Editor.Build.BuildPipeline.BuildWebGL `
  -logFile .\Logs\webgl-build.log
```

Unity's Brotli output must be served with matching `Content-Encoding` and WebAssembly MIME headers; generic `python -m http.server` is insufficient. Start the project server in one terminal:

```powershell
& "C:\WPy64-31180\python-3.11.8.amd64\python.exe" `
  .\tools\serve_webgl.py --directory .\Build\WebGL --port 8008
```

Browser automation follows this workstation's existing Claude Code pattern: attach Playwright to a normally launched Chrome over CDP. Ports `9222`-`9229` belong to persistent automation profiles; use a scratch port at `9230+` and an isolated profile:

```powershell
Start-Process "C:\Program Files\Google\Chrome\Application\chrome.exe" -ArgumentList @(
  '--remote-debugging-port=9230',
  "--user-data-dir=$PWD\Logs\browser\chrome-profile",
  '--remote-allow-origins=*',
  '--no-first-run',
  '--no-default-browser-check')

& "C:\WPy64-31180\python-3.11.8.amd64\python.exe" `
  .\tools\browser_smoke.py --cdp http://127.0.0.1:9230 `
  --url http://127.0.0.1:8008/ --output-dir .\Logs\browser
```

The smoke test waits for the real Unity canvas, checks WebGL/GPU state, obtains pointer lock, switches constrained → full-360 → constrained aim profiles, creates off-body Aim View flight, toggles Forward View and back, resets, fires both weapons, records console/page/network failures, measures 180 rendered frames, and saves four screenshots plus `report.json`. A WebGL-only telemetry bridge exposes read-only gameplay state to the test, so the gate fails unless exact aim limits, a non-zero independent aim angle, 0-degree Forward View alignment, return-to-aim behavior, reset recentering, clear head-origin state, movement, weapon counts, hits, and railgun reserve are observed. Verified on Chrome `143.0.7499.147` with AMD D3D11/ANGLE: `5.39 s` load, `59.9 FPS`, `17.3 ms` p95, six of six head-origin shots registered, and zero runtime/page/real network errors. Unity logs two visually harmless unsupported optional shaders (`Hidden/CoreSRP/CoreCopy` and `Hidden/Universal/HDRDebugView`) plus six Chromium `getInternalformatParameter` warnings; the rendered scene contains no missing/magenta materials.

Generated folders such as `Library`, `Temp`, `Logs`, `Build`, and `UserSettings` are intentionally ignored. Large source art, 3D files, audio, and video are tracked through Git LFS.

See [architecture](docs/architecture.md), [controls](docs/controls.md), and [demo scope](docs/demo-scope.md).
