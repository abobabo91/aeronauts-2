# Browser demo scope

## First playable: offline flight lab

The first milestone proves feel, not content volume or networking.

Current implementation status: the Flight Lab scene, greybox gates/altitude references, shared player/bot placeholder frame, fixed-tick skill movement, independent Aim View, body-aligned Forward View, live constrained/full-360 profiles, head/cockpit-origin weapons with cover obstruction, finite-speed primary projectiles, instant limited player railgun, stationary/horizontal/vertical drill targets, shared player/bot damage and body/cockpit regions, a fixed-tick non-regenerating shield gate and respawn, a primary-projectile bot driven through the shared flight simulation, a first-to-five duel, target flashes, shield-break/bypass/critical/elimination markers, accuracy/combat diagnostics, coordinated reset, explicit WebGL build pipeline, Brotli-aware local server, and automated real-Chrome smoke test are implemented. Thirty EditMode checks cover movement, aim constraints, automated flight, weapon state, shield gating, damage lifecycle, gestures, duel structure, and bindings; seven PlayMode checks validate the real scene, both camera states, coordinated reset, railgun shield bypass, unshielded primary cockpit elimination, head-origin obstruction, instantaneous railgun versus primary travel time, shared bot flight/projectiles, and five scored cockpit eliminations. The Chrome gate additionally verifies both aim profiles, off-body Aim View, body-aligned Forward View, return-to-aim behavior, reset recentering, clear head-origin shooting, retained high-capacity shield with no hull leakage, bot flight/body alignment and fire, rendered input/combat, speed, shot counts, hits, and railgun reserve through WebGL telemetry. Bot feel, combat timing exercises, hosted distribution, and final balance remain.

- one greybox arena with open space, cover, gates, and altitude references;
- one shared gameplay frame with two swappable visual silhouettes: wing-rig pilot and compact aircraft;
- pitch, yaw, roll, acceleration to a capped speed, full deceleration to zero, airbrake, boost, and hook-turn experimentation;
- independent mouse/controller aim camera with a centered reticle;
- stationary and moving targets with center/cockpit precision hit regions;
- one finite-speed precision primary and one instantaneous limited railgun secondary;
- provisional `300` non-regenerating shield/`100` hull gate, part-specific unshielded damage, railgun critical bypass, elimination score, and target respawn;
- reset, timing, accuracy, and movement-course feedback;
- WebGL and Windows development builds from the same scenes and settings.

## Second playable: combat validation (current)

- first-to-five one-versus-one combat with matching placeholder gameplay frames;
- shared player/bot health, shield, respawn, fixed-tick flight, and primary-projectile rules;
- readable hit, shield-break, elimination, and respawn feedback;
- controller and mouse/keyboard balance tests;
- tune bot flight, leading/error, pressure, and weapon cadence from repeatable playtests;
- choose final health/shield rules from playtest evidence.

## Deferred until the movement is fun

- public matchmaking, accounts, progression, cosmetics, monetization, and Steam integration;
- final character/vehicle art, narrative, and multiple arenas;
- a permanent networking package or hosting provider;
- ranked rules and cross-input policy;
- console platform SDK work and certification.

The browser build is a distribution target, not a throwaway rewrite. Native Windows/Steam work begins only after the same project has a stable flight/combat vertical slice.
