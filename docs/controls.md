# Controls

These are provisional bindings for the flight-lab prototype. All bindings and sensitivity curves must be remappable.

## Shared model

Movement and aiming are separate channels. Body turn rate is limited by flight tuning, not mouse sensitivity. Mouse or right-stick input rotates the aim camera; the crosshair stays centered and shots travel through it. This prevents a high-sensitivity mouse from making the vehicle turn faster.

At non-zero speed, body forward and velocity stay aligned. At zero speed, pitch, yaw, and roll remain available, allowing the player to orient before accelerating.

## Mouse and keyboard

| Action | Default |
| --- | --- |
| Aim camera | Mouse |
| Pitch body | `W` / `S` |
| Yaw body | `A` / `D` |
| Roll body | `Q` / `E` |
| Accelerate | `Space` |
| Airbrake / decelerate toward zero | `Left Ctrl` |
| Boost | `Left Shift` |
| Primary weapon | Left mouse |
| Secondary weapon | Right mouse |

Mouse input uses raw delta plus user sensitivity for aim only. Body rotation remains governed by pitch/yaw/roll rates and maneuver state.

## Controller

| Action | Default |
| --- | --- |
| Pitch/yaw body | Left stick |
| Aim camera | Right stick |
| Roll left/right | Left/right bumper |
| Accelerate | South face button |
| Airbrake / decelerate toward zero | West face button |
| Boost | East face button |
| Primary weapon | Right trigger |
| Secondary weapon | Left trigger |

Controller aim needs separate horizontal/vertical sensitivity, dead-zone, response curve, acceleration, and optional aim assistance. Competitive cross-input play may eventually require input-based matchmaking or independently balanced assistance; do not hide that choice inside the base movement controller.

## Still to decide through playtesting

- whether acceleration is hold, toggle, or speed-step based;
- how hook turns are triggered: emergent airbrake/boost timing or a dedicated maneuver button;
- whether aim has a hard angular cone relative to the body;
- roll auto-level behavior and accessibility options;
- controller aim-assist rules and cross-input matchmaking;
- whether the secondary uses ammunition, charges, cooldown, or a combination.
