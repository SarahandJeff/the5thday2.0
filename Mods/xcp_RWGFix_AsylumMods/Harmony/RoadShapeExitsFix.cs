using HarmonyLib;
using System.Reflection;
using WorldGenerationEngineFinal;
using System.Collections.Generic;

namespace RoadShapeFix
{
    /// <summary>
    /// Fixes the bug where only T-junctions and intersections spawn in world generation.
    /// The root cause is in TownPlanner.cs where it calculates exits based on neighbors.
    /// The algorithm creates overly dense road networks where every tile has 3-4 neighbors.
    ///
    /// This mod fixes the exit calculation to properly generate caps, corners, and straights
    /// by correcting the neighbor checking logic to match the game's original intent.
    /// </summary>
    public class RoadShapeExitsFix
    {
        public class RoadShapeExitsFixInit : IModApi
        {
            public void InitMod(Mod _modInstance)
            {
                Log.Out("[RoadShapeFix] ========================================");
                Log.Out("[RoadShapeFix] Initializing Road Shape Bug Fix");
                Log.Out("[RoadShapeFix] Fixing township generation to properly create all road shapes");
                Log.Out("[RoadShapeFix] ========================================");

                var harmony = new Harmony("com.asylum.roadshapefix");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                Log.Out("[RoadShapeFix] Harmony patches applied successfully!");
            }
        }

        // Shared helper methods used by both patches
        static class SharedHelpers
        {
            public static int GetTileExits(StreetTile tile)
            {
                var field = typeof(StreetTile).GetField("ConnectedExits",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                return field != null ? (int)field.GetValue(tile) : 0;
            }

            public static string GetShapeName(StreetTile tile)
            {
                var field = typeof(StreetTile).GetField("RoadShape",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                if (field == null) return "invalid";

                int roadShape = (int)field.GetValue(tile);

                // Use if-else instead of switch expression for C# 7.3 compatibility
                if (roadShape == 0) return "straight";
                if (roadShape == 1) return "t";
                if (roadShape == 2) return "intersection";
                if (roadShape == 3) return "cap";
                if (roadShape == 4) return "corner";
                return "invalid";
            }

            public static string GetShapeNameFromExits(int exits)
            {
                // Exit patterns: straight=5, t=7, intersection=15, cap=4, corner=6
                if (exits == 5) return "straight";
                if (exits == 7) return "t";
                if (exits == 15) return "intersection";
                if (exits == 4) return "cap";
                if (exits == 6) return "corner";
                if (exits == 0) return "isolated";
                return "unknown(" + exits + ")";
            }

            public static int FixTileExits(StreetTile tile, int currentExits, Township township, bool verbose = false)
            {
                int newExits = 0;
                string[] dirNames = { "North", "East", "South", "West" };

                if (verbose)
                {
                    Log.Out($"[RoadShapeFix] === Analyzing tile at {tile.GridPosition} in {tile.District.name} ===");
                    Log.Out($"[RoadShapeFix]   Current exits: {currentExits} ({GetShapeNameFromExits(currentExits)})");
                }

                // Check all 4 directions
                for (int dir = 0; dir < 4; dir++)
                {
                    StreetTile neighbor = tile.GetNeighbor(dir);

                    if (verbose)
                    {
                        if (neighbor == null)
                        {
                            Log.Out($"[RoadShapeFix]   {dirNames[dir]}: No neighbor");
                        }
                        else
                        {
                            bool sameTownship = neighbor.Township == township;
                            bool inStreets = township.Streets.ContainsKey(neighbor.GridPosition);
                            Log.Out($"[RoadShapeFix]   {dirNames[dir]}: Neighbor at {neighbor.GridPosition}, SameTownship={sameTownship}, InStreets={inStreets}, District={neighbor.District?.name ?? "null"}");
                        }
                    }

                    // Only create exit if neighbor exists AND is in the township's Streets
                    if (neighbor != null &&
                        neighbor.Township == township &&
                        township.Streets.ContainsKey(neighbor.GridPosition))
                    {
                        newExits |= 1 << dir;
                        if (verbose)
                        {
                            Log.Out($"[RoadShapeFix]   {dirNames[dir]}: EXIT CREATED");
                        }
                    }
                }

                // Special case: Single-tile settlement (no neighbors in Streets)
                // These should be caps (dead-ends) since custom districts only have cap tile prefabs
                // The original game sets exits=15 (intersection) but we need to find the correct cap orientation
                if (newExits == 0)
                {
                    // Use the ORIGINAL exits value to determine which direction(s) had connections
                    // The HighwayPlanner has already set exit bits for highway connections
                    int originalExits = currentExits;

                    if (originalExits == 0)
                    {
                        // No exits set yet, default to South
                        newExits = 4;
                        if (verbose)
                            Log.Out($"[RoadShapeFix] Single-tile settlement at {tile.GridPosition}, no original exits, defaulting to South cap (exits=4)");
                    }
                    else
                    {
                        // Find the first set bit in originalExits to use as the cap direction
                        // This preserves the orientation from the highway connection
                        for (int dir = 0; dir < 4; dir++)
                        {
                            if ((originalExits & (1 << dir)) != 0)
                            {
                                newExits = 1 << dir;
                                if (verbose)
                                    Log.Out($"[RoadShapeFix] Single-tile settlement at {tile.GridPosition}, using original exit direction {dirNames[dir]} for cap (exits={newExits})");
                                break;
                            }
                        }
                    }
                }

                if (verbose)
                {
                    Log.Out($"[RoadShapeFix]   New exits: {newExits} ({GetShapeNameFromExits(newExits)})");
                    if (newExits != currentExits)
                    {
                        Log.Out($"[RoadShapeFix]   CHANGE DETECTED: {GetShapeNameFromExits(currentExits)} -> {GetShapeNameFromExits(newExits)}");
                    }
                }

                return newExits;
            }
        }

        /// <summary>
        /// Patch the TownPlanner.Plan method where it calculates exits for street tiles.
        /// The bug is in the section that does:
        ///   for (int num15 = 0; num15 < 4; num15++)
        ///   {
        ///       StreetTile neighbor = value5.GetNeighbor(num15);
        ///       if (neighbor != null && neighbor.Township == value5.Township)
        ///       {
        ///           num14 |= 1 << num15;
        ///       }
        ///   }
        ///   value5.SetExits(num14);
        ///
        /// The issue: This creates exits for ALL neighbors in the same township,
        /// resulting in overly dense networks. We need to be more selective.
        /// </summary>
        [HarmonyPatch(typeof(TownPlanner))]
        [HarmonyPatch("Plan")]
        public class TownPlanner_Plan_Patch
        {
            static System.Collections.IEnumerator Postfix(System.Collections.IEnumerator __result, TownPlanner __instance)
            {
                // Let the original method run
                while (__result.MoveNext())
                {
                    yield return __result.Current;
                }

                // After town planning, fix the exit calculations
                Log.Out("[RoadShapeFix] ========================================");
                Log.Out("[RoadShapeFix] PLAN POSTFIX: Town planning complete, fixing road shape exits...");
                Log.Out("[RoadShapeFix] ========================================");

                var worldBuilderField = typeof(TownPlanner).GetField("worldBuilder",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                if (worldBuilderField == null)
                {
                    Log.Error("[RoadShapeFix] Could not find worldBuilder field!");
                    yield break;
                }

                WorldBuilder worldBuilder = (WorldBuilder)worldBuilderField.GetValue(__instance);

                if (worldBuilder == null || worldBuilder.Townships == null)
                {
                    Log.Error("[RoadShapeFix] WorldBuilder or Townships is null!");
                    yield break;
                }

                Log.Out($"[RoadShapeFix] Found {worldBuilder.Townships.Count} townships to process");

                int fixedCount = 0;
                int totalStreets = 0;
                int intersectionToOtherCount = 0;
                Dictionary<string, int> shapeStats = new Dictionary<string, int>
                {
                    {"straight", 0}, {"t", 0}, {"intersection", 0}, {"cap", 0}, {"corner", 0}, {"invalid", 0}
                };

                // Iterate through all townships and fix their road exits
                foreach (Township township in worldBuilder.Townships)
                {
                    if (township == null || township.Streets == null)
                        continue;

                    List<StreetTile> streetTiles = new List<StreetTile>(township.Streets.Values);
                    totalStreets += streetTiles.Count;

                    // Fix exits for edge tiles and create proper variety
                    foreach (StreetTile tile in streetTiles)
                    {
                        if (tile == null || tile.District == null)
                            continue;

                        // Skip gateway districts - they need all their connections
                        if (tile.District.name == "gateway")
                            continue;

                        // Recalculate exits with proper logic
                        int originalExits = SharedHelpers.GetTileExits(tile);
                        int fixedExits = SharedHelpers.FixTileExits(tile, originalExits, township);

                        if (fixedExits != originalExits)
                        {
                            // Log if we're changing an intersection to something else
                            if (originalExits == 15 && fixedExits != 15)
                            {
                                intersectionToOtherCount++;
                                Log.Out($"[RoadShapeFix] Changed intersection to {SharedHelpers.GetShapeNameFromExits(fixedExits)} at {tile.GridPosition} in {tile.District.name}");
                            }

                            tile.SetExits(fixedExits);
                            fixedCount++;
                        }

                        // Track statistics
                        string shapeName = SharedHelpers.GetShapeName(tile);
                        if (shapeStats.ContainsKey(shapeName))
                            shapeStats[shapeName]++;
                    }
                }

                Log.Out("[RoadShapeFix] ========================================");
                Log.Out("[RoadShapeFix] PLAN POSTFIX SUMMARY:");
                Log.Out($"[RoadShapeFix] Fixed {fixedCount} out of {totalStreets} street tiles");
                Log.Out($"[RoadShapeFix] Intersections changed to other shapes: {intersectionToOtherCount}");
                Log.Out($"[RoadShapeFix] Shape distribution:");
                Log.Out($"[RoadShapeFix]   Straights: {shapeStats["straight"]}");
                Log.Out($"[RoadShapeFix]   T-junctions: {shapeStats["t"]}");
                Log.Out($"[RoadShapeFix]   Intersections: {shapeStats["intersection"]}");
                Log.Out($"[RoadShapeFix]   Caps: {shapeStats["cap"]}");
                Log.Out($"[RoadShapeFix]   Corners: {shapeStats["corner"]}");
                Log.Out("[RoadShapeFix] Plan postfix complete!");
                Log.Out("[RoadShapeFix] ========================================");
            }
        }

        /// <summary>
        /// Patch HighwayPlanner.Plan to fix exits AFTER highways are created.
        /// This runs after HighwayPlanner modifies exits but before SpawnPrefabs loads the tiles.
        /// </summary>
        [HarmonyPatch(typeof(HighwayPlanner))]
        [HarmonyPatch("Plan")]
        public class HighwayPlanner_Plan_Patch
        {
            static System.Collections.IEnumerator Postfix(System.Collections.IEnumerator __result, HighwayPlanner __instance)
            {
                // Let the original method run first
                while (__result.MoveNext())
                {
                    yield return __result.Current;
                }

                // Now fix exits AFTER HighwayPlanner has modified them
                Log.Out("[RoadShapeFix] ========================================");
                Log.Out("[RoadShapeFix] HIGHWAYPLANNER POSTFIX: Highway planning complete");
                Log.Out("[RoadShapeFix] Fixing exits after HighwayPlanner modifications...");
                Log.Out("[RoadShapeFix] ========================================");

                var worldBuilderField = typeof(HighwayPlanner).GetField("worldBuilder",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                if (worldBuilderField != null)
                {
                    WorldBuilder worldBuilder = (WorldBuilder)worldBuilderField.GetValue(__instance);

                    if (worldBuilder != null && worldBuilder.Townships != null)
                    {
                        int fixedCount = 0;
                        int townshipCount = 0;
                        int totalTiles = 0;

                        Log.Out($"[RoadShapeFix] === POST-HIGHWAY ANALYSIS ===");
                        Log.Out($"[RoadShapeFix] Total townships: {worldBuilder.Townships.Count}");

                        foreach (Township township in worldBuilder.Townships)
                        {
                            if (township == null || township.Streets == null)
                                continue;

                            townshipCount++;
                            int townshipTiles = township.Streets.Count;
                            totalTiles += townshipTiles;

                            Log.Out($"[RoadShapeFix] --- Township {townshipCount}: {townshipTiles} tiles ---");

                            // Show first few tiles for debugging
                            int tileIndex = 0;
                            foreach (StreetTile tile in township.Streets.Values)
                            {
                                if (tile == null || tile.District == null)
                                    continue;

                                tileIndex++;
                                bool isVerbose = (tileIndex <= 3 || townshipTiles == 1); // Verbose for first 3 tiles or single-tile townships

                                // Skip gateway districts
                                if (tile.District.name == "gateway")
                                {
                                    if (isVerbose)
                                        Log.Out($"[RoadShapeFix] Tile {tileIndex}/{townshipTiles}: Skipping gateway tile at {tile.GridPosition}");
                                    continue;
                                }

                                // Recalculate exits
                                int originalExits = SharedHelpers.GetTileExits(tile);
                                int fixedExits = SharedHelpers.FixTileExits(tile, originalExits, township, isVerbose);

                                if (fixedExits != originalExits)
                                {
                                    tile.SetExits(fixedExits);
                                    fixedCount++;
                                    Log.Out($"[RoadShapeFix] *** FIXED TILE {tileIndex}/{townshipTiles}: {SharedHelpers.GetShapeNameFromExits(originalExits)} -> {SharedHelpers.GetShapeNameFromExits(fixedExits)} at {tile.GridPosition} in {tile.District.name} ***");
                                }
                                else if (isVerbose)
                                {
                                    Log.Out($"[RoadShapeFix] Tile {tileIndex}/{townshipTiles}: No change needed at {tile.GridPosition} in {tile.District.name}");
                                }
                            }
                        }

                        Log.Out($"[RoadShapeFix] ========================================");
                        Log.Out($"[RoadShapeFix] POST-HIGHWAY SUMMARY:");
                        Log.Out($"[RoadShapeFix] Processed {townshipCount} townships with {totalTiles} total tiles");
                        Log.Out($"[RoadShapeFix] Fixed {fixedCount} tiles after highway planning");
                        Log.Out($"[RoadShapeFix] ========================================");
                    }
                }
            }
        }
    }
}

