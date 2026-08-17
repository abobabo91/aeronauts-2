# Browser demo scope

## First playable: offline flight lab

The first milestone proves feel, not content volume or networking.

Current implementation status: the Flight Lab scene, greybox gates/altitude references, shared placeholder frame, fixed-tick skill movement, independent Aim View, body-aligned Forward View, live constrained/full-360 profiles, head/cockpit-origin weapons with cover obstruction, finite-speed primary projectiles, instant limited railgun, stationary/horizontal/vertical targets, body/cockpit hit regions, placeholder shield/hull/respawn, hit/blocked markers, accuracy/combat diagnostics, reset, explicit WebGL build pipeline, Brotli-aware local server, and automated real-Chrome smoke test are implemented. Twenty-five EditMode checks cover movement, aim constraints, automated flight, weapon state, damage regions, gestures, structure, and bindings; four PlayMode checks validate the real scene, both camera states, head-origin obstruction, and instantaneous railgun versus primary travel time. The Chrome gate additionally verifies both aim profiles, off-body Aim View, body-aligned Forward View, return-to-aim behavior, reset recentering, clear head-origin shooting, rendered input/combat at 60 FPS, speed, shot counts, hits, and railgun reserve through WebGL telemetry. Shield presentation, combat timing exercises, hosted distribution, and final balance remain.

- one greybox arena with open space, cover, gates, and altitude references;
- one shared gameplay frame with two swappable visual silhouettes: wing-rig pilot and compact aircraft;
- pitch, yaw, roll, acceleration to a capped speed, full deceleration to zero, airbrake, boost, and hook-turn experimentation;
- independent mouse/controller aim camera with a centered reticle;
- stationary and moving targets with center/cockpit precision hit regions;
- one finite-speed precision primary and one instantaneous limited railgun secondary;
- placeholder shield/hull state and target respawn;
- reset, timing, accuracy, and movement-course feedback;
- WebGL and Windows development builds from the same scenes and settings.

## Second playable: combat validation

- one-versus-one combat with placeholder rigs;
- health/shield rules chosen from playtest evidence;
- readable hit, shield-break, elimination, and respawn feedback;
- controller and mouse/keyboard balance tests;
- basic bot or recorded ghost only if it improves repeatable testing.

## Deferred until the movement is fun

- public matchmaking, accounts, progression, cosmetics, monetization, and Steam integration;
- final character/vehicle art, narrative, and multiple arenas;
- a permanent networking package or hosting provider;
- ranked rules and cross-input policy;
- console platform SDK work and certification.

The browser build is a distribution target, not a throwaway rewrite. Native Windows/Steam work begins only after the same project has a stable flight/combat vertical slice.
