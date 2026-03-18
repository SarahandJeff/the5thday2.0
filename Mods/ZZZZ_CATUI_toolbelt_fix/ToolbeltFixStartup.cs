using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

public class ToolbeltFixStartup : IModApi
{
    public static Harmony HarmonyInstance;

    public void InitMod(Mod modInstance)
    {
        try
        {
            Debug.Log("[CATUI_ToolbeltFix] Initializing toolbelt server fix...");

            HarmonyInstance = new Harmony("com.asylum.catui.toolbeltfix");

            // Manually patch the property getter with correct MethodType
            var targetType = typeof(Inventory);
            var targetProperty = targetType.GetProperty("PUBLIC_SLOTS_PLAYMODE");

            if (targetProperty == null)
            {
                Debug.LogError("[CATUI_ToolbeltFix] Could not find PUBLIC_SLOTS_PLAYMODE property!");
                return;
            }

            var targetMethod = targetProperty.GetGetMethod();
            if (targetMethod == null)
            {
                Debug.LogError("[CATUI_ToolbeltFix] Could not find PUBLIC_SLOTS_PLAYMODE getter!");
                return;
            }

            // Remove any existing patches on this method (from the broken original mod)
            try
            {
                var patches = Harmony.GetPatchInfo(targetMethod);
                if (patches != null && patches.Prefixes != null)
                {
                    foreach (var prefix in patches.Prefixes)
                    {
                        Debug.Log($"[CATUI_ToolbeltFix] Found existing prefix patch from: {prefix.owner}");

                        // Unpatch the original mod's broken patch
                        if (prefix.owner == "CATUI_toolbelt_more_slot")
                        {
                            Debug.Log($"[CATUI_ToolbeltFix] Removing broken patch from {prefix.owner}...");
                            HarmonyInstance.Unpatch(targetMethod, prefix.PatchMethod);
                            Debug.Log($"[CATUI_ToolbeltFix] Successfully removed broken patch!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[CATUI_ToolbeltFix] Could not check/remove existing patches: {ex.Message}");
            }

            var prefixMethod = typeof(InventoryPatchFix).GetMethod("Prefix", BindingFlags.Static | BindingFlags.NonPublic);
            if (prefixMethod == null)
            {
                Debug.LogError("[CATUI_ToolbeltFix] Could not find Prefix method!");
                return;
            }

            HarmonyInstance.Patch(targetMethod, prefix: new HarmonyMethod(prefixMethod));

            Debug.Log("[CATUI_ToolbeltFix] Successfully patched PUBLIC_SLOTS_PLAYMODE!");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CATUI_ToolbeltFix] Error during initialization: {ex.Message}");
            Debug.LogError(ex.StackTrace);
        }
    }
}

