# kOS-AtmosphereAutopilot
An interface to allow kOS to control Atmosphere Autopilot

This mod requires a recent version of AA and kOS to be installed.

Access from kOS via ADDONS:AA.

### Supported suffixes, for more details of the effects, please consult the AA documentation
- MODERATEAOA - Boolean - toggles AoA moderation for the craft.
- MAXAOA - Scalar - gets/sets the maximum AoA allowed for the craft when moderation is enabled.
- MODERATEG - Boolean - toggles G-force moderation for the craft.
- MAXG - Scalar - gets/sets the maximum G-force allowed for the craft when moderation is enabled.
- MODERATESIDESLIP - Boolean - toggles sideslip angle moderation for the craft.
- MAXSIDESLIP - Scalar - gets/sets the maximum sideslip angle allowed for the craft when moderation is enabled.
- MODERATESIDEG - Boolean - toggles side G-force moderation for the craft.
- MAXSIDEG - Scalar - gets/sets the maximum side G-force allowed for the craft when moderation is enabled.
---
- PITCHRATELIMIT - Scalar - gets/sets the pitch rate limit for the craft (in radians/s).
- YAWRATELIMIT - Scalar - gets/sets the yaw rate limit for the craft (in radians/s).
- ROLLRATELIMIT - Scalar - gets/sets the roll rate limit for the craft (in radians/s).
---
- WINGLEVELER - Boolean - toggles snapping the wings to level when close to zero bank.
---
- FBW or FLYBYWIRE - Boolean - toggles fly-by-wire mode.
- COORDTURN - Boolean - toggles coordinated turns while in FBW mode.
- ROCKETMODE - Boolean - toggles rocket mode while in FBW mode.
---
- DIRECTOR - Boolean - toggles director mode (note this is a kOS specific director, not the mouse director).
- DIRECTION - Vector - sets the direction vector for the director.
- DIRECTORSTRENGTH - Scalar - sets director strength.
---
- CRUISE - Boolean - toggles cruise mode.
- PSEUDOFLC - Boolean - toggles pseudo FLC mode for cruise climb control.
- HEADING - Scalar - gets/sets heading for cruise mode, use -1 to fly level in the current direction.
- ALTITUDE - Scalar - gets/sets altitude set point for cruise.
- VERTSPEED - Scalar - gets/sets vertical speed for cruise.
- FLCMARGIN - Scalar - gets/sets the speed margin for pseudo FLC.
- MAXCLIMBANGLE - Scalar - gets/sets the maximum climb angle in degrees.
- WAYPOINT - GeoCoordinate - gets/sets the current waypoint for cruise waypoint following.
---
- SPEEDCONTROL - Boolean - toggles speed control.
- SPEED - Scalar - gets/sets the current speed set point.


FBW, DIRECTOR, and CRUISE are mutually exclusive, activating one will deactivate the others. 

In cruise mode, only one of ALTITUDE and VERTSPEED can be active, setting a value to one of them will activate it and deactivate the other mode.

Similarly only one of HEADING and WAYPOINT can be active, setting a value to one will deactivate the other.


The craft settings suffixes will not work without activating the autopilot. Set one of the AA modes before trying to modify them.


### Example usage
```set addons:aa:altitude to reqAltitude.
set addons:aa:heading to reqHeading.
set addons:aa:cruise to true.
```

```set addons:aa:direction to heading(reqHeading, reqPitch):Vector.
set addons:aa:director to true.
```
