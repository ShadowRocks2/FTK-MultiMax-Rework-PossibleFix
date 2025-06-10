using HarmonyLib;
using UnityEngine;

namespace FTK_MultiMax_Rework {
    public static class EncounterSessionPatches {
        public static void XPModifierPatch(ref FTKPlayerID _recvPlayer, ref int _xp, ref int _gold) {
            var characterOverworldByFid = FTKHub.Instance.GetCharacterOverworldByFID(_recvPlayer);

            if (characterOverworldByFid == null) {
                Debug.LogWarning($"[MultiMaxRework] No CharacterOverworld found for player {_recvPlayer}. XP/GOLD patch skipped.");
                return;
            }

            float xpMod = Mathf.Max(1f, characterOverworldByFid.m_CharacterStats.XpModifier);
            float goldMod = Mathf.Max(1f, characterOverworldByFid.m_CharacterStats.GoldModifier);

            if (GameFlowMC.gMaxPlayers > 3) {
                _xp = Mathf.RoundToInt((_xp * xpMod) * 1.5f);
                _gold = Mathf.RoundToInt((_gold * goldMod) * 1.5f);

                Debug.Log($"[MultiMaxRework] XP modified to {_xp}, GOLD modified to {_gold} for player {_recvPlayer}.");
            }
        }
    }
}
