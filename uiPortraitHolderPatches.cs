using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace FTK_MultiMax_Rework {
    public static class uiPortraitHolderPatches {
        private static FieldInfo m_CarrierPassengersField;

        public static void LoadFix(uiPortraitHolder __instance) {
            Debug.Log("[MultiMax Rework] Before UpdateDisplay method");

            if (m_CarrierPassengersField == null) {
                m_CarrierPassengersField = AccessTools.Field(typeof(uiPortraitHolder), "m_CarrierPassengers");
            }

            if (m_CarrierPassengersField == null) {
                Debug.LogError("[MultiMax Rework] m_CarrierPassengersField is null.");
                return;
            }

            if (__instance.m_PortraitActionPoints == null) {
                Debug.LogError("[MultiMax Rework] m_PortraitActionPoints is null.");
                return;
            }

            if (__instance.m_HexLand == null || __instance.m_HexLand.m_PlayersInHex == null) {
                Debug.LogError("[MultiMax Rework] m_HexLand or m_PlayersInHex is null.");
                return;
            }

            List<CharacterOverworld> carrierPassengers = m_CarrierPassengersField.GetValue(__instance) as List<CharacterOverworld>;

            if (carrierPassengers == null) {
                Debug.LogError("[MultiMax Rework] m_CarrierPassengers is null.");
                return;
            }

            Debug.Log($"[MultiMax Rework] m_CarrierPassengers count: {carrierPassengers.Count}");
            Debug.Log($"[MultiMax Rework] m_PortraitActionPoints count: {__instance.m_PortraitActionPoints.Count}");

            for (int i = 0; i < __instance.m_PortraitActionPoints.Count; i++) {
                if (i < carrierPassengers.Count) {
                    Debug.Log($"[MultiMax Rework] Checking index {i} with CarrierPassenger.");
                    __instance.m_PortraitActionPoints[i].CalculateShouldShow(carrierPassengers[i], _alwaysShowPortrait: true);
                } else {
                    Debug.LogWarning($"[MultiMax Rework] Index {i} exceeds CarrierPassengers count.");
                }
            }

            for (int i = 0; i < __instance.m_PortraitActionPoints.Count; i++) {
                if (i < __instance.m_HexLand.m_PlayersInHex.Count) {
                    Debug.Log($"[MultiMax Rework] Checking index {i} with PlayersInHex.");
                    __instance.m_PortraitActionPoints[i].CalculateShouldShow(__instance.m_HexLand.m_PlayersInHex[i]);
                } else {
                    Debug.LogWarning($"[MultiMax Rework] Index {i} exceeds PlayersInHex count.");
                }
            }
        }
    }
}
