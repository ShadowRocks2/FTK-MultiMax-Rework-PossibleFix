using BepInEx.Configuration;
using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                                               "The max number of players");

            if (!File.Exists(configFilePath)) {
                configFile.Save();
            }
        }

        public static void InitializeMaxPlayers() {
            if (ConfigHandler.MaxPlayersConfig != null) {
                GameFlowMC.gMaxPlayers = ConfigHandler.MaxPlayersConfig.Value;
                GameFlowMC.gMaxEnemies = GameFlowMC.gMaxPlayers;
                uiQuickPlayerCreate.Default_Classes = Enumerable.Repeat(0, GameFlowMC.gMaxPlayers).ToArray(); // Or any valid default class ID
            } else {
                Debug.LogError("maxPlayersConfig is not initialized!");
            }
        }
    }
}
