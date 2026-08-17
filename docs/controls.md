# Controls

These are provisional bindings for the flight-lab prototype. All bindings and sensitivity curves must be remappable.

## Shared model

Movement and aiming are separate channels. Body turn rate is limited by flight tuning, not mouse sensitivity. Mouse or right-stick input rotates the aim camera; the crosshair stays centered and shots travel through it. This prevents a high-sensitivity mouse from making the vehicle turn faster.

There are two gameplay views. **Aim View** uses the independent aim direction for the camera, centered crosshair, and weapons, so orbiting aim shows the aircraft from different angles. **Forward View** freezes aim input and points all three along body forward for high-speed flight and rolls. Toggling back to Aim View starts at current body forward instead of restoring an old off-screen direction.

Aim limits are measured in the aircraft's rolled local frame but do not drag the camera while it remains in bounds. The default constrained profile is `110` degrees left/right and `78` degrees up/down. The comparison profile is full horizontal rotation (`180` degrees per side) with `89` degrees up/down. `V`/controller Select switches profiles as a FlightLab experiment; the HUD shows the active view, profile, and degrees.

At non-zero speed, body forward and velocity stay aligned. At zero speed, pitch, yaw, and roll remain available, allowing the player to orient before accelerating.

Pitch is deliberately faster than yaw. Rolling into the desired turning plane and then pitching is the most efficient way to change direction. Strong steering bleeds more speed than a smooth arc. Airbrake sharply increases pitch/yaw authority at speed while rapidly reducing momentum; it suppresses acceleration and boost until released.

## Mouse and keyboard

| Action | Default |
| --- | --- |
| Aim camera | Mouse |
| Pitch body | `W` / `S` |
| Yaw body | `A` / `D` |
| Manual roll | Hold `Q` / `E` |
| Automatic 360-degree roll | Double-tap `Q` / `E` |
| Accelerate | `Space` |
| Airbrake / decelerate toward zero | `Left Ctrl` |
| Boost | `Left Shift` |
| Finite-speed primary projectile | Hold left mouse |
| Instant railgun | Press right mouse |
| Toggle Aim/Forward View | `C` |
| Test constrained/full-360 profile | `V` |

Mouse input uses raw delta plus user sensitivity for aim only. Body rotation remains governed by pitch/yaw/roll rates and maneuver state.

## Controller

| Action | Default |
| --- | --- |
| Pitch/yaw body | Left stick |
| Aim camera | Right stick |
| Manual roll left/right | Hold left/right bumper |
| Automatic 360-degree roll | Double-tap left/right bumper |
| Accelerate | South face button |
| Airbrake / decelerate toward zero | West face button |
| Boost | East face button |
| Finite-speed primary projectile | Hold right trigger |
| Instant railgun | Press left trigger |
| Toggle Aim/Forward View | Press right stick |
| Test constrained/full-360 profile | Select/Back button |

Controller aim needs separate horizontal/vertical sensitivity, dead-zone, response curve, acceleration, and optional aim assistance. Competitive cross-input play may eventually require input-based matchmaking or independently balanced assistance; do not hide that choice inside the base movement controller.

## Skill maneuvers

The core hook turn is an emergent sequence: roll to select the plane, pitch while holding airbrake, release at the desired heading, then make a fresh boost press. An airbrake held for at least `0.18 s` from an entry speed of at least `18` arms a `0.15 s` perfect-exit window. Hitting it gives `0.22 s` of extra boost acceleration and displays `SLINGSHOT` in the debug HUD. Tapping airbrake for one simulation tick cannot earn the bonus.

The same controls can produce partial hooks, near-stop reversals, and boost exits; Q+E has no special chord and there is no automatic 180-degree action. The same-side double-tap window for the automatic roll is currently `0.25 s`.

## Weapon model

Both weapons use the centered camera ray to choose a target point with no random spread, but the actual shot starts at the center of the shared head/cockpit region. The query ignores the player's own colliders. A second obstruction query prevents third-person camera peeking from shooting through cover; `HEAD ORIGIN BLOCKED` appears near the crosshair and the shot impacts the obstruction normally. This deliberately exposes the critical head/cockpit center whenever the player has a clear firing line. The primary is an automatic projectile weapon: bolts travel at `120` units/second without gravity, making lead necessary against moving targets. It currently fires `8` bolts/second. The railgun is hitscan, so its damage resolves in the firing tick; it has a three-shot reserve and `0.85 s` cooldown. Pressing `R` resets the reserve and combat diagnostics together with flight.

Cockpit regions apply the provisional `2.5x` critical multiplier. Body hits, critical hits, accuracy, railgun reserve, and the last target's shield/hull appear in the combat HUD. These damage/resource values exist only to make the lab measurable.

## Still to decide through playtesting

- whether acceleration is hold, toggle, or speed-step based;
- exact turn momentum loss, airbrake authority, and perfect-exit timing after hands-on feel tests;
- whether constrained or full-360 aim becomes the final competitive default, and the final profile angles;
- roll auto-level behavior and accessibility options;
- controller aim-assist rules and cross-input matchmaking;
- exact primary speed/fire rate and railgun reserve/cooldown after combat playtests;
- whether the railgun pierces targets or remains first-hit only;
- final shield, hull, critical, and weapon damage rules.
