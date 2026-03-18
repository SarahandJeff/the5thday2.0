using System;
using System.Reflection;
using HarmonyLib;
using WorldGenerationEngineFinal;

namespace compodensityfix.Harmony
{
    /// <summary>
    /// Helper class to check if a POI name matches the target prefixes and get distance multiplier.
    /// </summary>
    public static class POIPrefixHelper
    {
        // POIs with these prefixes get stricter 50% distance enforcement
        private static readonly string[] TargetPrefixes = new string[]
        {
            "xcpv",
            "xcp",
            "xcpnb",
            "xcpnpc"
        };

        /// <summary>
        /// Gets the distance multiplier for the POI based on its prefix.
        /// Returns 0.5f (50%) for xcp* prefixes, 0.25f (25%) for all others.
        /// </summary>
        public static float GetDistanceMultiplier(string poiName)
        {
            if (string.IsNullOrEmpty(poiName))
                return 0.25f;

            string lowerName = poiName.ToLowerInvariant();
            foreach (string prefix in TargetPrefixes)
            {
                if (lowerName.StartsWith(prefix))
                    return 0.5f; // 50% for xcp* POIs
            }
            return 0.25f; // 25% for vanilla/other POIs
        }
    }

    /// <summary>
    /// ModAPI entry point for the POI Density Fix mod.
    /// Fixes the bug where density constraints override DuplicateRepeatDistance settings.
    /// </summary>
    public class POIDensityFixModAPI : IModApi
    {
        public void InitMod(Mod _modInstance)
        {
            Log.Out(" ");
            Log.Out("========================================");
            Log.Out("[compodensityfix] Loading POI Density Fix mod...");
            Log.Out("[compodensityfix] Version: 2.2.0");
            Log.Out("[compodensityfix] This mod fixes duplicate POI spawning caused by density override bug");
            Log.Out("[compodensityfix] xcp* prefixes (xcpv, xcp, xcpnb, xcpnpc): 50% min distance (8000 blocks for 16k)");
            Log.Out("[compodensityfix] All other POIs: 25% min distance (4000 blocks for 16k)");
            Log.Out("========================================");

            var harmony = new HarmonyLib.Harmony("com.asylummods.compodensityfix");

            try
            {
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                Log.Out("[compodensityfix] Harmony patches applied successfully!");

                var patchedMethods = harmony.GetPatchedMethods();
                int count = 0;
                foreach (var patchedMethod in patchedMethods)
                {
                    count++;
                    Log.Out($"[compodensityfix] Patched: {patchedMethod.DeclaringType?.Name}.{patchedMethod.Name}");
                }
                Log.Out($"[compodensityfix] Total methods patched: {count}");
            }
            catch (System.Exception ex)
            {
                Log.Error($"[compodensityfix] ERROR applying patches: {ex.Message}");
                Log.Error($"[compodensityfix] Stack trace: {ex.StackTrace}");
            }

            Log.Out("[compodensityfix] DuplicateRepeatDistance will now be respected even when density is low");
            Log.Out("========================================");
            Log.Out(" ");
        }
    }

    /// <summary>
    /// Patch PrefabManager.GetPrefabWithDistrict to actively check and reject duplicate POIs
    /// This prevents the same POI from spawning right next to itself when tile density is exhausted.
    /// </summary>
    [HarmonyPatch(typeof(PrefabManager))]
    [HarmonyPatch("GetPrefabWithDistrict")]
    public class GetPrefabWithDistrictPatch
    {
        /// <summary>
        /// Postfix patch that actively checks if the selected POI violates minimum distance rules.
        /// If it does, we return null instead, forcing the system to skip placement rather than
        /// place a duplicate POI.
        /// </summary>
        private static void Postfix(ref PrefabData __result, PrefabManager __instance, Vector2i center, float _distanceScale)
        {
            // If no POI was selected, nothing to check
            if (__result == null)
                return;

            // ALWAYS check for duplicates on ALL attempts
            try
            {
                // Get distance multiplier based on POI prefix (50% for xcp*, 25% for others)
                float multiplier = POIPrefixHelper.GetDistanceMultiplier(__result.Name);
                int minDistance = (int)(__result.DuplicateRepeatDistance * multiplier);

                // Call the PUBLIC isNameValid method directly (no reflection needed!)
                bool isValid = __instance.isNameValid(__result, center, __instance.UsedPrefabsWorld, minDistance);

                if (!isValid)
                {
                    int percent = (int)(multiplier * 100);
                    Log.Out($"[compodensityfix] REJECTED duplicate POI '{__result.Name}' at {center} - would violate minimum distance of {minDistance} blocks ({percent}% of {__result.DuplicateRepeatDistance}), distanceScale was {_distanceScale}");
                    __result = null; // Return null instead of the duplicate POI
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[compodensityfix] ERROR in Postfix: {ex.Message}");
                Log.Error($"[compodensityfix] Stack trace: {ex.StackTrace}");
            }
        }
    }

    /// <summary>
    /// Patch PrefabManager.GetWildernessPrefab to enforce distance checks on ALL attempts.
    /// This prevents wilderness POIs from spawning duplicates on both initial and retry attempts.
    /// </summary>
    [HarmonyPatch(typeof(PrefabManager))]
    [HarmonyPatch("GetWildernessPrefab")]
    public class GetWildernessPrefabPatch
    {
        /// <summary>
        /// Postfix patch that returns null if the selected POI would violate minimum distance.
        /// Checks BOTH initial and retry attempts. Uses 50% for xcp* POIs, 25% for others.
        /// </summary>
        private static void Postfix(ref PrefabData __result, PrefabManager __instance, Vector2i center, bool _isRetry)
        {
            // If no POI was selected, nothing to check
            if (__result == null)
                return;

            // ALWAYS check for duplicates on ALL attempts
            try
            {
                // Get distance multiplier based on POI prefix (50% for xcp*, 25% for others)
                float multiplier = POIPrefixHelper.GetDistanceMultiplier(__result.Name);
                int minDistance = (int)(__result.DuplicateRepeatDistance * multiplier);

                // Call the PUBLIC isNameValid method directly (no reflection needed!)
                bool isValid = __instance.isNameValid(__result, center, __instance.UsedPrefabsWorld, minDistance);

                if (!isValid)
                {
                    int percent = (int)(multiplier * 100);
                    Log.Out($"[compodensityfix] REJECTED wilderness POI '{__result.Name}' at {center} - would violate minimum distance of {minDistance} blocks ({percent}% of {__result.DuplicateRepeatDistance}), isRetry={_isRetry}");
                    __result = null;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[compodensityfix] ERROR in GetWildernessPrefab Postfix: {ex.Message}");
                Log.Error($"[compodensityfix] Stack trace: {ex.StackTrace}");
            }
        }
    }
}

