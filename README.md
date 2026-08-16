# Aeronauts 2

Foundation for a high-skill competitive aerial arena shooter. This repository currently contains a Unity URP project, tooling configuration, project structure, and design constraints. It contains no custom gameplay code.

`Aeronauts 2` is an internal working title. Before any public release, confirm naming rights and replace all references if necessary. Do not reuse assets, characters, audio, branding, or code from the 1999 PlayStation game *Aironauts*.

## Current stack

- Unity `6000.3.22f1` (Unity 6.3 LTS)
- Universal Render Pipeline `17.0.1`
- Unity Input System `1.12.0`
- Unity Test Framework `1.4.2`
- Web Build Support and Windows IL2CPP Build Support
- Blender `5.2.0 LTS`
- Git LFS `3.5.1`
- Visual Studio Code `1.133.0` with Microsoft C#, C# Dev Kit, and Unity extensions

The browser demo and later Windows/Steam build are intended to share one Unity project. Multiplayer packages are deliberately not installed yet; movement and aiming must be proven in an offline flight lab before the networking model is locked.

Windows IL2CPP support is installed, but a native Windows build also requires Visual Studio 2019 or later with C++ Tools and Windows SDK `10.0.19041.0` or newer. That administrator-level prerequisite is deferred until native/Steam builds begin; it is not needed for the browser-first prototype.

## First open

1. Start Unity Hub and sign in with a Unity account.
2. Activate a Unity Personal license if the account is eligible.
3. Add this folder as an existing project and open it with Unity `6000.3.22f1`.
4. Allow the initial package restore and asset import to finish.

The command-line template copy completed, but its first import is currently blocked by the missing Unity license. Unity will generate `.meta` files for the new architecture folders during the first licensed import.

Installed tool locations on this workstation:

- Unity Hub: `C:\Program Files\Unity Hub\Unity Hub.exe`
- Unity Editor: `C:\Users\abele\AppData\Local\Programs\Unity\Hub\Editor\6000.3.22f1\Editor\Unity.exe`
- Unity CLI: `C:\Users\abele\AppData\Local\Unity\bin\unity.exe`
- Blender: `C:\Users\abele\AppData\Local\Programs\Blender\5.2.0\blender.exe`

Blender portable archive SHA-256: `2D184B626C001692C362291911293B6A297179D618D95E9E9192C3A80318ADC4`.

## Repository layout

- `Assets/Aeronauts2/`: all first-party game content
- `Assets/ThirdParty/`: imported third-party assets, each with its license
- `Packages/`: pinned Unity packages
- `ProjectSettings/`: shared Unity configuration
- `docs/`: durable gameplay and technical decisions

Generated folders such as `Library`, `Temp`, `Logs`, `Build`, and `UserSettings` are intentionally ignored. Large source art, 3D files, audio, and video are tracked through Git LFS.

See [architecture](docs/architecture.md), [controls](docs/controls.md), and [demo scope](docs/demo-scope.md).
