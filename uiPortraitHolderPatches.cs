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

            // Resolve field info
            if (m_CarrierPassengersField == null) {
                m_CarrierPassengersField = AccessTools.Field(typeof(uiPortraitHolder), "m_CarrierPassengers");
                if (m_CarrierPassengersField == null) {
                    Debug.LogError("[MultiMax Rework] Failed to find 'm_CarrierPassengers' field via reflection.");
                    return;
                }
            }

            // Safety checks
            if (__instance.m_PortraitActionPoints == null) {
                Debug.LogError("[MultiMax Rework] m_PortraitActionPoints is null.");
                return;
            }

            if (__instance.m_HexLand == null || __instance.m_HexLand.m_PlayersInHex == null) {
                Debug.LogError("[MultiMax Rework] m_HexLand or m_PlayersInHex is null.");
                return;
            }

            // CarrierPassengers fetch and type safety
            object passengersObj = m_CarrierPassengersField.GetValue(__instance);
            if (passengersObj is not List<CharacterOverworld> carrierPassengers) {
                Debug.LogError("[MultiMax Rework] m_CarrierPassengers is not of expected type List<CharacterOverworld>.");
                return;
            }

            Debug.Log($"[MultiMax Rework] m_CarrierPassengers count: {carrierPassengers.Count}");
            Debug.Log($"[MultiMax Rework] m_PortraitActionPoints count: {__instance.m_PortraitActionPoints.Count}");

            // Safely update portraits with CarrierPassengers
            int minCount = Mathf.Min(__instance.m_PortraitActionPoints.Count, carrierPassengers.Count);
            for (int i = 0; i < minCount; i++) {
                Debug.Log($"[MultiMax Rework] Updating PortraitActionPoint {i} with CarrierPassenger.");
                __instance.m_PortraitActionPoints[i].CalculateShouldShow(carrierPassengers[i], _alwaysShowPortrait: true);
            }

            // Safely update portraits with PlayersInHex
            minCount = Mathf.Min(__instance.m_PortraitActionPoints.Count, __instance.m_HexLand.m_PlayersInHex.Count);
            for (int i = 0; i < minCount; i++) {
                Debug.Log($"[MultiMax Rework] Updating PortraitActionPoint {i} with PlayersInHex.");
                __instance.m_PortraitActionPoints[i].CalculateShouldShow(__instance.m_HexLand.m_PlayersInHex[i]);
            }
        }
    }
}
