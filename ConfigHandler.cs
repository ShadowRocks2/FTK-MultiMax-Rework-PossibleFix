using BepInEx.Configuration;
using BepInEx;
using System.IO;
using System.Linq;
using UnityEngine;

namespace FTK_MultiMax_Rework {
    public static class ConfigHandler {
        public static ConfigEntry<int> MaxPlayersConfig { get; private set; }

        public static void InitializeConfig() {
            string configFilePath = Path.Combine(Paths.ConfigPath, "MultiMaxRework.cfg");
            var configFile = new ConfigFile(configFilePath, true);

            MaxPlayersConfig = configFile.Bind("General",
                                               "MaxPlayers",
                                               5,
                                               "The max number of players (recommended: 2 to 8)");

            // No need to manually save here, ConfigFile handles this.
        }

        public static void InitializeMaxPlayers() {
            if (MaxPlayersConfig != null) {
                // Clamp the value to prevent invalid player counts
                int maxPlayers = Mathf.Clamp(MaxPlayersConfig.Value, 2, 8); // Adjust upper limit based on game limits

                GameFlowMC.gMaxPlayers = maxPlayers;
                GameFlowMC.gMaxEnemies = maxPlayers;

                // Initialize Default_Classes with default class IDs (assuming 0 is valid)
                uiQuickPlayerCreate.Default_Classes = Enumerable.Repeat(0, maxPlayers).ToArray();

                Debug.Log($"[MultiMaxRework] Max players set to: {GameFlowMC.gMaxPlayers}");
            } else {
                Debug.LogError("[MultiMaxRework] MaxPlayersConfig is not initialized!");
            }
        }
    }
}
