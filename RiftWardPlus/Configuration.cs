using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Vintagestory.API.Common;

namespace RiftWardPlus;

#pragma warning disable CA2211
public static class Configuration
{
    private static Dictionary<string, object> LoadConfigurationByDirectoryAndName(ICoreAPI api, string directory, string name, string defaultDirectory)
    {
        string directoryPath = Path.Combine(api.DataBasePath, directory);
        string configPath = Path.Combine(api.DataBasePath, directory, $"{name}.json");
        Dictionary<string, object> loadedConfig;
        try
        {
            // Load Configurations
            string jsonConfig = File.ReadAllText(configPath);
            loadedConfig = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonConfig);
        }
        catch (DirectoryNotFoundException)
        {
            Debug.LogWarn($"WARNING: Configurations directory does not exist creating {name}.json and directory...");
            try
            {
                Directory.CreateDirectory(directoryPath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"ERROR: Cannot create directory: {ex.Message}");
            }
            Debug.Log("Loading default configurations...");
            // Load default configurations
            loadedConfig = api.Assets.Get(new AssetLocation(defaultDirectory)).ToObject<Dictionary<string, object>>();

            Debug.Log($"Configurations loaded, saving configs in: {configPath}");
            try
            {
                // Saving default configurations
                string defaultJson = JsonConvert.SerializeObject(loadedConfig, Formatting.Indented);
                File.WriteAllText(configPath, defaultJson);
                return LoadConfigurationByDirectoryAndName(api, directory, name, defaultDirectory);
            }
            catch (Exception ex)
            {
                Debug.LogError($"ERROR: Cannot save default files to {configPath}, reason: {ex.Message}");
            }
        }
        catch (FileNotFoundException)
        {
            Debug.LogWarn($"WARNING: Configurations {name}.json cannot be found, recreating file from default");
            Debug.Log("Loading default configurations...");
            // Load default configurations
            loadedConfig = api.Assets.Get(new AssetLocation(defaultDirectory)).ToObject<Dictionary<string, object>>();

            Debug.Log($"Configurations loaded, saving configs in: {configPath}");
            try
            {
                // Saving default configurations
                string defaultJson = JsonConvert.SerializeObject(loadedConfig, Formatting.Indented);
                File.WriteAllText(configPath, defaultJson);
                return LoadConfigurationByDirectoryAndName(api, directory, name, defaultDirectory);
            }
            catch (Exception ex)
            {
                Debug.LogError($"ERROR: Cannot save default files to {configPath}, reason: {ex.Message}");
            }

        }
        catch (Exception ex)
        {
            Debug.LogError($"ERROR: Cannot read the Configurations: {ex.Message}");
            Debug.Log("Loading default values from mod assets...");
            // Load default configurations
            loadedConfig = api.Assets.Get(new AssetLocation(defaultDirectory)).ToObject<Dictionary<string, object>>();
        }
        return loadedConfig;
    }


    #region baseconfigs
    public static int detectorTickrate = 5000;
    public static int detectorRadius = 20;
    public static int detectorYRadius = 5;
    public static int detectorMillisecondsSleepBetweenBlockCheck = 5;
    public static int detectorMaxScannedRiftWards = 1;
    public static bool detectorOnlyActiveRiftWards = false;
    public static bool pingerOnlyActiveRiftWards = false;
    public static bool compassRose = true;
    public static bool ENABLERADIUSBLOCKCHECK = false;
    public static bool enableExtendedLogs = false;

    public static void UpdateBaseConfigurations(ICoreAPI api)
    {
        Dictionary<string, object> baseConfigs = LoadConfigurationByDirectoryAndName(
            api,
            "ModConfig/RiftWardPlus/config",
            "base",
            "riftwardplus:config/base.json"
        );

        { //detectorTickrate
            if (baseConfigs.TryGetValue("detectorTickrate", out object value))
                if (value is null) Debug.LogError("CONFIGURATION ERROR: detectorTickrate is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: detectorTickrate is not int is {value.GetType()}");
                else detectorTickrate = (int)(long)value;
            else Debug.LogError("CONFIGURATION ERROR: detectorTickrate not set");
        }
        { //detectorRadius
            if (baseConfigs.TryGetValue("detectorRadius", out object value))
                if (value is null) Debug.LogError("CONFIGURATION ERROR: detectorRadius is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: detectorRadius is not int is {value.GetType()}");
                else detectorRadius = (int)(long)value;
            else Debug.LogError("CONFIGURATION ERROR: detectorRadius not set");
        }
        { //detectorMillisecondsSleepBetweenBlockCheck
            if (baseConfigs.TryGetValue("detectorMillisecondsSleepBetweenBlockCheck", out object value))
                if (value is null) Debug.LogError("CONFIGURATION ERROR: detectorMillisecondsSleepBetweenBlockCheck is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: detectorMillisecondsSleepBetweenBlockCheck is not int is {value.GetType()}");
                else detectorMillisecondsSleepBetweenBlockCheck = (int)(long)value;
            else Debug.LogError("CONFIGURATION ERROR: detectorMillisecondsSleepBetweenBlockCheck not set");
        }
        { //detectorMaxScannedRiftWards
            if (baseConfigs.TryGetValue("detectorMaxScannedRiftWards", out object value))
                if (value is null) Debug.LogError("CONFIGURATION ERROR: detectorMaxScannedRiftWards is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: detectorMaxScannedRiftWards is not int is {value.GetType()}");
                else detectorMaxScannedRiftWards = (int)(long)value;
            else Debug.LogError("CONFIGURATION ERROR: detectorMaxScannedRiftWards not set");
        }
        { //detectorOnlyActiveRiftWards
            if (baseConfigs.TryGetValue("detectorOnlyActiveRiftWards", out object value))
                if (value is null) Debug.LogError("CONFIGURATION ERROR: detectorOnlyActiveRiftWards is null");
                else if (value is not bool) Debug.LogError($"CONFIGURATION ERROR: detectorOnlyActiveRiftWards is not boolean is {value.GetType()}");
                else detectorOnlyActiveRiftWards = (bool)value;
            else Debug.LogError("CONFIGURATION ERROR: detectorOnlyActiveRiftWards not set");
        }
        { //pingerOnlyActiveRiftWards
            if (baseConfigs.TryGetValue("pingerOnlyActiveRiftWards", out object value))
                if (value is null) Debug.LogError("CONFIGURATION ERROR: pingerOnlyActiveRiftWards is null");
                else if (value is not bool) Debug.LogError($"CONFIGURATION ERROR: pingerOnlyActiveRiftWards is not boolean is {value.GetType()}");
                else pingerOnlyActiveRiftWards = (bool)value;
            else Debug.LogError("CONFIGURATION ERROR: pingerOnlyActiveRiftWards not set");
        }
        { //compassRose
            if (baseConfigs.TryGetValue("compassRose", out object value))
                if (value is null) Debug.LogError("CONFIGURATION ERROR: compassRose is null");
                else if (value is not bool) Debug.LogError($"CONFIGURATION ERROR: compassRose is not boolean is {value.GetType()}");
                else compassRose = (bool)value;
            else Debug.LogError("CONFIGURATION ERROR: compassRose not set");
        }
        { //ENABLERADIUSBLOCKCHECK
            if (baseConfigs.TryGetValue("ENABLERADIUSBLOCKCHECK", out object value))
                if (value is null) Debug.LogError("CONFIGURATION ERROR: ENABLERADIUSBLOCKCHECK is null");
                else if (value is not bool) Debug.LogError($"CONFIGURATION ERROR: ENABLERADIUSBLOCKCHECK is not boolean is {value.GetType()}");
                else ENABLERADIUSBLOCKCHECK = (bool)value;
            else Debug.LogError("CONFIGURATION ERROR: ENABLERADIUSBLOCKCHECK not set");
        }
        { //enableExtendedLogs
            if (baseConfigs.TryGetValue("enableExtendedLogs", out object value))
                if (value is null) Debug.LogError("CONFIGURATION ERROR: enableExtendedLogs is null");
                else if (value is not bool) Debug.LogError($"CONFIGURATION ERROR: enableExtendedLogs is not boolean is {value.GetType()}");
                else enableExtendedLogs = (bool)value;
            else Debug.LogError("CONFIGURATION ERROR: enableExtendedLogs not set");
        }
    }
    #endregion
}