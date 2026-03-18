============================================================
 		BIOME & TIER LOCKPICK SYSTEM      
============================================================

DESCRIPTION
-----------
This mod is an **extension for the Left 2 Die and extension for SPHEREII LOCK MINI-GAME  ** project.
Work also as separet mod if you dont like both mods mentioned above!

It adds a complete lock difficulty & attempt system based on:
- biome difficulty ranges
- POI / building tier (Tier 0–5)
- a “Master” difficulty level (highest tier)

It is designed to **extend and complement SphereII’s Lockpicking MiniGame**,
but it also **works without SphereII installed** (vanilla lockpicking / radial
flow is supported).

MAIN FEATURES
-------------

1) BIOME + POI TIER DIFFICULTY SYSTEM
- Every lock gets a difficulty calculated from:
  * biome difficulty range (min/max)
  * POI / building tier
- Result is clamped into the difficulty scale:
  * Very Easy → Easy → Medium → Hard → Very Hard → Master

2) LOCK DIFFICULTY LABELS / CONSISTENT TIERING
- Locks are categorized into clear tiers that match the gameplay scale.
- The system ensures difficulty is predictable and consistent across the map:
  * Forest can stay lower difficulty
  * Wasteland / Radiated zones can roll higher difficulty including Master
  * Higher POI tiers push locks into harder tiers

3) ATTEMPT LIMIT + LOCKED STATE (ANTI-SPAM PROTECTION)
- Manual lockpicking has a configurable failure limit (default: 5 broken picks).
- After reaching the limit the lock becomes **LOCKED**:
  * further lockpicking is blocked
  * the player is forced to break the lock/container instead
- This prevents infinite lockpick spam and adds real risk/reward.

4) FORCE LOCK OPTION (FALLOUT-LIKE)
- A “Force” action can be shown in the interaction radial menu (if enabled).
- If the lock is already jammed (max failures reached), **Force is also blocked**
  to keep the system consistent and prevent bypassing the jam mechanic.

5) SPHEREII LOCKPICKING MINI-GAME COMPATIBILITY (OPTIONAL)
If SphereII’s Lockpicking MiniGame mod is installed:
- this mod injects the **same computed difficulty tier** into the mini-game
- your biome/tier/master difficulty is applied to mini-game parameters
- the attempt/fail tracking remains consistent with the global lock state

If SphereII is NOT installed:
- the mod still enforces tier difficulty, attempt limits, jam state, and Force Lock
  using vanilla-compatible hooks.

CONFIGURATION (XML)
-------------------
Gameplay values can be tuned in XML without recompiling the DLL.

Main config file:
- Lockpicksystem.xml

Supported settings:
- max manual failures before JAMMED (Pick maxFails)
- Force Lock chances (Default + per-block overrides)
- biome difficulty ranges (min/max difficulty per biome/region)

NOTE:
- POI / building tier influence is part of the DLL logic (not an XML setting).
- Mini-game scaling values are applied automatically when SphereII is detected.

INSTALLATION
------------
1. Extract the mod into:
   Mods/<YourModFolder>/
2. (Optional) Install SphereII Lockpicking MiniGame if you want the mini-game UI.
3. Configure settings in:
   Lockpicksystem.xml
4. Start a game / server and test locks across different biomes and POI tiers.

REQUIREMENTS
------------
- 7 Days to Die 2.5
- EAC OFF: depends on your server policy (Harmony patches are used)

OPTIONAL DEPENDENCIES
---------------------
- SphereII/ Xyth Lockpicking MiniGame
  (This mod will automatically enhance it when detected.)

COMPATIBILITY NOTES
-------------------
- Works standalone (vanilla flow)
- Adds extra behavior when SphereII MiniGame is present
- Designed to be modular with XML tuning where supported

CREDITS
-------
- SphereII/ Xyths (Lockpicking MiniGame concept / base mod)
- Community modding tools & Harmony ecosystem

LICENSE
-------
Free for personal use and modification.
Do not redistribute without permission.
============================================================
