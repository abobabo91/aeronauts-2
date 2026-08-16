# Aeronauts 2

Foundation for a high-skill competitive aerial arena shooter. This repository currently contains a Unity URP project, tooling configuration, project structure, and design constraints. It contains no custom gameplay code.

`Aeronauts 2` is an internal working title. Before any public release, confirm naming rights and replace all references if necessary. Do not reuse assets, characters, audio, branding, or code from the 1999 PlayStation game *Aironauts*.

## Current stack

- Unity `6000.0.81f1` (Unity 6.0 LTS)
- Universal Render Pipeline `17.0.4` (builtin, with SRP Core `17.0.4`)
- Unity Input System `1.20.0`
- Unity Test Framework `1.6.0`
- Windows IL2CPP Build Support — **Web Build Support is not yet installed** (see below)
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

`6000.3.22f1` is also installed but unusable for this project — see above. It can be removed with `unity uninstall 6000.3.22f1`.

### Outstanding: Web Build Support

`Editor/Data/PlaybackEngines/` for `6000.0.81f1` contains only `windowsstandalonesupport`. The Web module install failed:

```
An error occurred trying to start process 'UnitySetup-WebGL-Support-for-Editor-6000.0.81f1.exe' …
The requested operation requires elevation.
```

Unity's module installers need administrator rights. Run this **from an elevated PowerShell** before attempting a browser build:

```powershell
& "$env:LOCALAPPDATA\Unity\bin\unity.exe" install-modules -e 6000.0.81f1 --module webgl -f --accept-eula
```

Three traps in that one line, all hit here:

- The editor version goes in `-e/--editor-version`. Do **not** write `--version` — that is the CLI's own global flag, so the command prints `1.0.0-beta.5` and exits without installing anything.
- `-f` (force) is required. The Hub's state database reports `webgl → Installed, 4.05 GB` even though `PlaybackEngines/` has no `WebGLSupport` folder, so a plain install is a no-op that exits `Error: No modules found to install.`
- Without real elevation it fails ~14 ms after launching `UnitySetup-WebGL-Support-for-Editor-<version>.exe`, reporting only `1 item(s) failed to install`. The first attempt gave the honest message: `The requested operation requires elevation`. `install-modules` does have a UAC helper (`--no-elevate` opts out), but the prompt has to actually be accepted.

Verify against disk afterwards — `Editor/Data/PlaybackEngines/` must contain `WebGLSupport`. Do not trust `unity editors` or `install-modules --list`; both reported Web as installed here when it was not.

`unity editors` reports `6000.3.22f1` with modules `Web, Windows`. The Hub log shows a transient `2 item(s) failed to install` during the WebGL module install; the retry succeeded and `Editor/Data/PlaybackEngines` contains both `WebGLSupport` and `windowsstandalonesupport`.

Blender portable archive SHA-256: `2D184B626C001692C362291911293B6A297179D618D95E9E9192C3A80318ADC4`.

## Repository layout

- `Assets/Aeronauts2/`: all first-party game content
- `Assets/ThirdParty/`: imported third-party assets, each with its license
- `Packages/`: pinned Unity packages
- `ProjectSettings/`: shared Unity configuration
- `docs/`: durable gameplay and technical decisions

Generated folders such as `Library`, `Temp`, `Logs`, `Build`, and `UserSettings` are intentionally ignored. Large source art, 3D files, audio, and video are tracked through Git LFS.

See [architecture](docs/architecture.md), [controls](docs/controls.md), and [demo scope](docs/demo-scope.md).
