# Architecture

## Non-negotiable game rules

- The flight body and velocity point in the same direction whenever speed is above a small threshold.
- At zero speed, the body may rotate in place; movement resumes along the body's forward direction.
- Aim and camera direction are independent from the flight body. The crosshair remains centered while the camera/weapon aim rotates.
- Movement supports pitch, yaw, roll, full stop/hover, airbrake, boost, spins, and high-G hook turns.
- There is no grapple or wall-grab mechanic.
- The primary weapon is precise and repeatable. The secondary weapon is stronger, limited, and fired with right mouse/left trigger by default.
- Shields are part of the combat model, but exact damage and regeneration rules are intentionally undecided.
- Character-wing and small-aircraft presentations must share a comparable central silhouette and hit-volume layout.

## Runtime boundaries for later implementation

Keep the simulation independent from presentation so the browser demo, native build, replays, bots, and eventual server authority use the same rules.

1. **Input** converts devices into a device-neutral movement intent and aim intent.
2. **Core simulation** advances a fixed 60 Hz tick and owns authoritative time/state.
3. **Flight** turns movement intent into body orientation, speed, boost, airbrake, and maneuver state.
4. **Combat** resolves aim rays, primary/secondary resources, hit regions, shields, and damage.
5. **Camera** follows aim intent and reads simulation state without controlling velocity.
6. **Presentation** interpolates visuals, animation, audio, trails, and effects from simulation state.
7. **UI** reads public state and emits menu/loadout commands; it does not mutate flight or combat directly.
8. **Networking** will later serialize input/state at these boundaries rather than networking scene objects ad hoc.

Use data assets for tunable flight, weapon, shield, camera, and input curves. Avoid burying balance constants in scene objects. Keep gameplay state out of animation and visual-effect components.

## Asset ownership

`Assets/Aeronauts2` is the first-party namespace:

- `Art/Characters`: pilots, bodies, rigs, and character presentation
- `Art/Vehicles`: aircraft-form presentation sharing the common gameplay frame
- `Art/Environments`: arena geometry and background set dressing
- `Art/Materials` and `Art/VFX`: rendering assets
- `Audio/Music` and `Audio/SFX`: authored audio
- `Input`: action maps and device-specific tuning
- `Prefabs`: composed gameplay and presentation prefabs by domain
- `Scenes/Bootstrap`: startup and persistent systems
- `Scenes/FlightLab`: isolated movement/combat experimentation
- `Scenes/Tests`: integration test scenes only
- `Settings`: game-owned configuration assets
- `Scripts`: future runtime modules matching the boundaries above
- `Tests`: future EditMode and PlayMode tests
- `UI`: menus, HUD, reticles, and accessibility presentation

Third-party content stays in `Assets/ThirdParty/<publisher-or-package>` and must include its license/provenance. Do not edit third-party sources in place; wrap or derive them under `Assets/Aeronauts2`.

## Platform strategy

The first build target is WebGL for easy sharing. The native target is Windows IL2CPP for a later Steam release. Keep core input, simulation, combat, and presentation portable across both.

Browser constraints to preserve from the start:

- no dependency on native plugins in the core game;
- avoid large startup downloads and excessive texture/audio memory;
- treat browser pause, focus loss, and controller reconnection as normal;
- networking must use a browser-compatible transport such as secure WebSockets or WebRTC;
- quality tiers must allow cheaper shadows, post-processing, particles, and render scale.

The first online architecture decision happens only after the offline flight lab feels good. At that checkpoint, evaluate server-authoritative prediction/reconciliation and browser-compatible relay hosting. The combat design should assume lag compensation will be required for precise hitscan.
