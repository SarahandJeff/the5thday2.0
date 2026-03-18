================================================================================
                        COMPODENSITYFIX MOD
                    POI Density Bug Fix for 7 Days to Die
================================================================================

VERSION: 1.0.0
AUTHOR: AsylumMods

================================================================================
WHAT THIS MOD FIXES
================================================================================

PROBLEM:
The vanilla game has a bug where POI density constraints override your 
DuplicateRepeatDistance settings. This causes the same POI to spawn multiple 
times right next to itself, even if you set DuplicateRepeatDistance = 16000.

ROOT CAUSE:
During world generation, the game uses a 3-tier retry system when placing POIs:
  1st attempt: Full distance checking (100% of DuplicateRepeatDistance)
  2nd attempt: Reduced distance (30% of DuplicateRepeatDistance)
  3rd attempt: NO DISTANCE CHECKING (distanceScale = 0.0)

When tile density is low, the density filter eliminates most POI candidates 
BEFORE the distance check runs. This leaves only 1-2 small POIs as options.
On the 3rd retry, distance checking is completely disabled, allowing duplicates.

SOLUTION:
This mod uses Harmony patches to actively check every POI placement and REJECT 
any duplicate that would violate a minimum distance of 25% of your 
DuplicateRepeatDistance setting (4,000 blocks for 16k settings).

================================================================================
HOW IT WORKS
================================================================================

The mod patches two methods in the game:
  - PrefabManager.GetPrefabWithDistrict (town POIs)
  - PrefabManager.GetWildernessPrefab (wilderness POIs)

After the game selects a POI, the mod:
  1. Checks if distanceScale < 1.0 (any retry scenario)
  2. Calls the game's isNameValid() method with 25% minimum distance
  3. If the POI would be a duplicate within 4,000 blocks → REJECT IT
  4. If the POI passes the distance check → ALLOW IT

When a POI is rejected, the marker is left empty instead of forcing a duplicate.

================================================================================
WHAT YOU'LL SEE IN LOGS
================================================================================

SUCCESSFUL PLACEMENTS:
[compodensityfix] Allowed POI 'house_modern_02' at 9995, 7595 - passed distance check (minDist=4000, distanceScale=0.3)

REJECTED DUPLICATES:
[compodensityfix] REJECTED duplicate POI 'xcpv_filler_deverezieaux' at 9806, 7032 - would violate minimum distance of 4000 blocks (25% of 16000), distanceScale was 0

FAILED MARKER (when no valid POI exists):
WRN SpawnMarkerPartsAndPrefabs failed commercial, tags , size 25, 25 25, 25, totalDensityLeft 1507.5

================================================================================
INSTALLATION
================================================================================

1. Copy the entire "compodensityfix" folder to: 7 Days To Die\Mods\
2. The mod will load automatically when you start the game
3. Generate a NEW world to see the fix in action (existing worlds won't change)

================================================================================
TECHNICAL DETAILS
================================================================================

HARMONY LIBRARY:
This mod uses Harmony (0Harmony.dll) to patch game methods at runtime without 
modifying base game files. This is the standard modding framework for 7DTD.

PATCHED METHODS:
  - PrefabManager.GetPrefabWithDistrict (Postfix patch)
  - PrefabManager.GetWildernessPrefab (Postfix patch)

MINIMUM DISTANCE ENFORCEMENT:
  - Always enforces at least 25% of DuplicateRepeatDistance
  - For 16k settings: 4,000 block minimum
  - For 1k settings: 250 block minimum
  - Adjusts automatically based on each POI's settings

NO REFLECTION NEEDED:
The mod calls the public isNameValid() method directly - no performance impact.

================================================================================
COMPATIBILITY
================================================================================

COMPATIBLE WITH:
  - All POI mods (Compopack, etc.)
  - Custom world generation mods
  - Any DuplicateRepeatDistance settings

NOT COMPATIBLE WITH:
  - Other mods that patch the same methods (rare)

PERFORMANCE:
  - Only runs during world generation
  - Zero performance impact during gameplay
  - Minimal overhead during generation (one method call per retry)

================================================================================
TROUBLESHOOTING
================================================================================

IF DUPLICATES STILL APPEAR:
  - Make sure you generated a NEW world (mod only affects generation)
  - Check the log for [compodensityfix] messages to verify mod is active
  - Some POIs may have very low DuplicateRepeatDistance settings (check XML)

IF MOD DOESN'T LOAD:
  - Verify the mod folder is in: 7 Days To Die\Mods\compodensityfix\
  - Check that compodensityfix.dll exists in the mod folder
  - Look for errors in the game log during mod loading

================================================================================
UNRELATED BUGS
================================================================================

AVATARNPCCONTROLLER NULLREFERENCEEXCEPTION:
If you see this error, it's NOT caused by this mod. It's a vanilla game bug 
where NPC entities are missing skeleton bones. The game's assignParts() method 
doesn't check for null before accessing bone transforms.

This happens with some modded NPCs or corrupted entity data. It's unrelated to 
POI density or world generation.

================================================================================

