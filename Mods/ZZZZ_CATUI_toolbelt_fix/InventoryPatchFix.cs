using System;
using System.IO;
using System.Reflection;
using System.Xml;
using UnityEngine;

public static class InventoryPatchFix
{
    private static int _toolbeltSlots = -1;
    private static bool _configLoaded = false;
    
    /// <summary>
    /// Loads the toolbelt slot count from the original mod's config file.
    /// Caches the result so we don't read XML on every call.
    /// </summary>
    private static void LoadConfig()
    {
        if (_configLoaded) return;
        
        _toolbeltSlots = 12; // Default value
        
        try
        {
            // Look for the config in the original mod folder
            string modFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string originalModConfig = Path.Combine(modFolder, "..", "ZZZ_CATUI_toolbelt_more_slot", "Config", "XUi", "windows.xml");
            
            // Normalize the path
            originalModConfig = Path.GetFullPath(originalModConfig);
            
            if (File.Exists(originalModConfig))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(originalModConfig);
                
                XmlNodeList setNodes = xmlDoc.GetElementsByTagName("set");
                if (setNodes.Count > 0)
                {
                    string slotText = setNodes[0].InnerText.Trim();
                    if (int.TryParse(slotText, out int parsedSlots))
                    {
                        // Clamp between 10-20 as per original mod's comments
                        _toolbeltSlots = Math.Max(10, Math.Min(20, parsedSlots));
                        Debug.Log($"[CATUI_ToolbeltFix] Loaded toolbelt slots from config: {_toolbeltSlots}");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[CATUI_ToolbeltFix] Config file not found at: {originalModConfig}, using default: {_toolbeltSlots}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CATUI_ToolbeltFix] Error loading config: {ex.Message}");
        }
        
        _configLoaded = true;
    }
    
    /// <summary>
    /// Harmony Prefix for Inventory.PUBLIC_SLOTS_PLAYMODE getter.
    /// Returns false to skip the original method.
    /// </summary>
    internal static bool Prefix(ref int __result)
    {
        LoadConfig();
        __result = _toolbeltSlots;
        return false; // Skip original method
    }
}

