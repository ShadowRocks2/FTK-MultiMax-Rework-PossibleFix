using FTKItemName;
using Google2u;
using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace FTK_MultiMax_Rework {
    public static class uiPopupMenuPatches {
        public static void PopupAwake(uiPopupMenu __instance) {
            if (__instance == null || __instance.m_Popups == null) {
                Debug.LogWarning("[MultiMax Rework] uiPopupMenu or its popups list is null.");
                return;
            }

            PopupButton givePopup = __instance.m_Popups.FirstOrDefault(popup => popup.m_Action == uiPopupMenu.Action.Give);

            if (givePopup != null) {
                givePopup.m_Count = Mathf.Max(0, GameFlowMC.gMaxPlayers - 1);
                Debug.Log($"[MultiMax Rework] uiPopupMenu Give popup count set to {givePopup.m_Count}");
            }
            else {
                Debug.LogWarning("[MultiMax Rework] Give popup button not found.");
            }
        }
    }
}
