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

Both weapons use the centered camera ray to choose a target point with no random spread, but the actual shot starts at the center of the shared head/cockpit region. The query ignores the player's own colliders. A second obstruction query prevents third-person camera peeking from shooting through cover; `HEAD ORIGIN BLOCKED` appears near the crosshair and the shot impacts the obstruction normally. This deliberately exposes the critical head/cockpit center whenever the player has a clear firing line. The primary is an automatic projectile weapon: bolts travel at `120` units/second without gravity, making lead necessary against moving targets. It currently fires `8` bolts/second for `16` damage each. The railgun is hitscan, so its damage resolves in the firing tick; it deals `80` damage, has a three-shot reserve, and has a `0.85 s` cooldown. Pressing `R` resets flight, every target, weapon resources, score, and combat diagnostics together.

Targets currently have `300` shield and `100` hull. Shield never regenerates. As long as even one shield point remains, body and cockpit locations take the same per-weapon shield damage and a shield-breaking shot cannot carry excess damage into hull. The primary deals `16` shield damage and the railgun deals `80`; hit location starts changing damage only on later shots after shield reaches zero.

With shield down, a primary body hit deals `16` hull damage while a cockpit hit deals the full `100` and immediately eliminates the target. A railgun body hit deals `80` hull damage. A railgun cockpit hit is always lethal because it bypasses shield, including against a full `300` shield target. Eliminated targets disappear and respawn after three seconds.

Player and bot share these shield/hull and hit-region rules. The bot currently flies through the same fixed-tick flight simulation and fires only the finite-speed primary; it has independent leading, reaction, aim-error, range, aggression, and burst values in `BotTuning.asset`. The duel is first to five eliminations, with each combatant returning after three seconds. Target flashes and centered markers distinguish shield hits, critical hits, shield breaks, hull hits, railgun bypass kills, and eliminations. The combat HUD shows match score, both combatants' live shield/hull or respawn state, bot speed/fire, player accuracy, criticals, eliminations, shield-down state, and last-target timing. These values are a deliberate first balance hypothesis for playtesting, not final competitive balance.

## Still to decide through playtesting

- whether acceleration is hold, toggle, or speed-step based;
- exact turn momentum loss, airbrake authority, and perfect-exit timing after hands-on feel tests;
- whether constrained or full-360 aim becomes the final competitive default, and the final profile angles;
- roll auto-level behavior and accessibility options;
- controller aim-assist rules and cross-input matchmaking;
- exact primary speed/fire rate and railgun reserve/cooldown after combat playtests;
- whether the railgun pierces targets or remains first-hit only;
- final shield, hull, critical, and weapon damage rules.
