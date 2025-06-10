using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace FTK_MultiMax_Rework {
    public class uiGoldMenuPatches {
        public static bool GoldAwake(uiGoldMenu __instance) {
            var m_GoldEntriesField = Traverse.Create(__instance).Field("m_GoldEntries");
            var goldEntries = m_GoldEntriesField.GetValue<List<uiGoldMenuEntry>>();

            __instance.m_InputFocus = __instance.gameObject.GetComponent<FTKInputFocus>();
            __instance.m_InputFocus.m_InputMode = FTKInput.InputMode.InGameUI;
            __instance.m_InputFocus.m_Cancel = __instance.OnButtonCancel;

            if (goldEntries != null) {
                int maxEntries = Mathf.Max(0, GameFlowMC.gMaxPlayers - 2); // safe clamp

                goldEntries.Add(__instance.m_FirstEntry);

                for (int i = 0; i < maxEntries; i++) {
                    uiGoldMenuEntry newEntry = UnityEngine.Object.Instantiate(__instance.m_FirstEntry);
                    newEntry.transform.SetParent(__instance.m_FirstEntry.transform.parent, worldPositionStays: false);
                    goldEntries.Add(newEntry);
                }

                Debug.Log($"[MultiMax Rework] uiGoldMenu expanded to {goldEntries.Count} entries.");
            }
            else {
                Debug.LogError("[MultiMax Rework] Failed to access m_GoldEntries.");
            }

            return false; // skip original Awake()
        }
    }
}
