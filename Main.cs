using BepInEx;
using Google2u;
using HarmonyLib;
using Rewired;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace FTK_MultiMax_Rework
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Main : BaseUnityPlugin
    {
        private const string pluginGuid = "fortheking.edm.multimaxrework";
        private const string pluginName = "MultiMaxRework";
        private const string pluginVersion = "1.4";

        private static Harmony Harmony { get; set; } = new Harmony(pluginGuid);

        public IEnumerator Start()
        {
            Debug.Log("MultiMax Rework - Initializing...");
            ConfigHandler.InitializeConfig();
            ConfigHandler.InitializeMaxPlayers();
            Debug.Log("MultiMax Rework - Patching...");

            try
            {
                PatchMethods();

                typeof(uiQuickPlayerCreate)
                    .GetField("guiQuickPlayerCreates", BindingFlags.Static | BindingFlags.NonPublic)
                    ?.SetValue(null, new uiQuickPlayerCreate[GameFlowMC.gMaxPlayers]);

                uiQuickPlayerCreate.Default_Classes = new int[GameFlowMC.gMaxPlayers];

                Harmony.Patch(AccessTools.Method(typeof(uiCharacterCreateRoot), "Start"), postfix: new HarmonyMethod(typeof(Main), nameof(AddMorePlayerSlotsInMenu)));
                Harmony.Patch(AccessTools.Method(typeof(ReInput.PlayerHelper), "GetPlayer", new[] { typeof(int) }), prefix: new HarmonyMethod(typeof(Main), nameof(FixRewire)));
                Harmony.Patch(AccessTools.Method(typeof(uiPortraitHolderManager), "Create", new[] { typeof(HexLand) }), postfix: new HarmonyMethod(typeof(Main), nameof(AddMorePlayersToUI)));
                Harmony.Patch(AccessTools.Method(typeof(uiPortraitHolder), "UpdateDisplay"), prefix: new HarmonyMethod(typeof(Main), nameof(UpdateDisplayPatch)));
                Harmony.Patch(AccessTools.Method(typeof(uiPlayerMainHud), "Update"), prefix: new HarmonyMethod(typeof(Main), nameof(PlaceUI)));
                Harmony.Patch(AccessTools.Method(typeof(uiHudScroller), "Init"), prefix: new HarmonyMethod(typeof(Main), nameof(InitHUD)));
                Harmony.Patch(AccessTools.Method(typeof(Diorama), "_resetTargetQueue"), prefix: new HarmonyMethod(typeof(Main), nameof(DummySlide)));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MultiMax Rework] Error while patching: {ex}");
            }

            while (FTKHub.Instance == null)
                yield return null;

            DummiesHandler.CreateDummies();
            Debug.Log("MultiMax Rework - Done");
        }

        private void PatchMethods()
        {
            PatchMethod<uiGoldMenu>("Awake", typeof(uiGoldMenuPatches), "GoldAwake");
            PatchMethod<uiPopupMenu>("Awake", typeof(uiPopupMenuPatches), "PopupAwake");
            PatchMethod<EncounterSession>("GiveOutLootXPGold", typeof(EncounterSessionPatches), "XPModifierPatch");
        }

        private void PatchMethod<T>(string originalMethodName, Type patchClass, string patchMethodName, Type[] parameterTypes = null)
        {
            var original = AccessTools.Method(typeof(T), originalMethodName, parameterTypes);
            var patch = AccessTools.Method(patchClass, patchMethodName);

            if (original != null && patch != null)
            {
                Harmony.Patch(original, new HarmonyMethod(patch));
            }
            else
            {
                Debug.LogWarning($"[MultiMax Rework] Failed to patch method: {typeof(T).Name}.{originalMethodName}");
            }
        }

        public static bool InitHUD(ref uiHudScroller __instance, uiPlayerMainHud _playerHud, ref int ___m_Index, ref Dictionary<uiPlayerMainHud, int> ___m_TargetIndex, ref List<uiPlayerMainHud> ___m_Huds, ref float ___m_HudWidth, ref float[] ___m_Positions)
        {
            int num = GameLogic.Instance.IsSinglePlayer() ? _playerHud.m_Cow.m_FTKPlayerID.TurnIndex + 1 : _playerHud.m_Cow.m_FTKPlayerID.TurnIndex + 1;
            ___m_Index = GameLogic.Instance.IsSinglePlayer() ? 0 : _playerHud.m_Cow.m_FTKPlayerID.TurnIndex;

            ___m_TargetIndex[_playerHud] = num;
            ___m_Huds.Add(_playerHud);
            RectTransform component = _playerHud.GetComponent<RectTransform>();
            ___m_HudWidth = component.rect.width;
            Vector3 localPosition = component.localPosition;
            localPosition.y = 0f - component.anchoredPosition.y;

            if (num >= ___m_Positions.Length)
            {
                float[] newArray = new float[num + 1];
                Array.Copy(___m_Positions, newArray, ___m_Positions.Length);
                ___m_Positions = newArray;
            }

            localPosition.x = ___m_Positions[num];
            component.localPosition = localPosition;
            return false;
        }

        public static void PlaceUI(ref uiPlayerMainHud __instance)
        {
            int turnIndex = __instance.m_Cow.m_FTKPlayerID.TurnIndex;
            float startX = -725f;
            float endX = 725f;
            float width = __instance.GetComponent<RectTransform>().rect.width - 220f;
            float spacing = (endX - startX) / GameFlowMC.gMaxPlayers;
            float scaleFactor = Mathf.Min(1f, spacing / width);

            __instance.GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Lerp(startX, endX, turnIndex / (float)(GameFlowMC.gMaxPlayers - 1)), __instance.GetComponent<RectTransform>().anchoredPosition.y);
            __instance.GetComponent<RectTransform>().localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        }

        public static void AddMorePlayersToUI(ref uiPortraitHolder __result)
        {
            int currentCount = __result.m_PortraitActionPoints.Count;

            for (int i = currentCount; i < GameFlowMC.gMaxPlayers; i++)
            {
                uiPortraitActionPoint clone = UnityEngine.Object.Instantiate(
                    __result.m_PortraitActionPoints[currentCount - 1],
                    __result.m_PortraitActionPoints[currentCount - 1].transform.parent);
                __result.m_PortraitActionPoints.Add(clone);
            }
        }

        public static bool UpdateDisplayPatch(uiPortraitHolder __instance, ref bool __result)
        {
            if (__instance.m_PortraitActionPoints == null)
            {
                __result = false;
                return false;
            }

            var followCarrierField = typeof(uiPortraitHolder).GetField("m_FollowCarrier", BindingFlags.NonPublic | BindingFlags.Instance);
            var carrierPassengersField = typeof(uiPortraitHolder).GetField("m_CarrierPassengers", BindingFlags.NonPublic | BindingFlags.Instance);

            var followCarrier = (MiniHexInfo)followCarrierField?.GetValue(__instance);
            var carrierPassengers = (List<CharacterOverworld>)carrierPassengersField?.GetValue(__instance);

            if (followCarrier == null && (__instance.m_HexLand?.m_PlayersInHex?.Count ?? 0) == 0)
            {
                __instance.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(__instance.gameObject);
                __result = false;
                return false;
            }

            __instance.gameObject.SetActive(true);
            __instance.m_PortraitAndName.Hide();
            __instance.m_PortraitRoot.gameObject.SetActive(true);

            foreach (var actionPoint in __instance.m_PortraitActionPoints)
                actionPoint.ResetShouldShow();

            var playersToShow = followCarrier != null ? carrierPassengers : __instance.m_HexLand.m_PlayersInHex;
            int index = 0;
            foreach (var player in playersToShow)
            {
                if (index < __instance.m_PortraitActionPoints.Count)
                    __instance.m_PortraitActionPoints[index].CalculateShouldShow(player, followCarrier != null);
                index++;
            }

            foreach (var actionPoint in __instance.m_PortraitActionPoints)
                actionPoint.UpdateShow();

            __result = true;
            return false;
        }

        public static bool FixRewire(int playerId, ref Player __result)
        {
            if (playerId < ReInput.players.playerCount)
                return true;

            __result = ReInput.players.GetPlayer(Mathf.Clamp(ReInput.players.playerCount - 1, 0, ReInput.players.playerCount - 1));
            return false;
        }

        public static void AddMorePlayerSlotsInMenu(ref uiCharacterCreateRoot __instance)
        {
            if (__instance.m_CreateUITargets.Length >= GameFlowMC.gMaxPlayers)
                return;

            Transform[] uiTargets = new Transform[GameFlowMC.gMaxPlayers];
            Transform[] camTargets = new Transform[GameFlowMC.gMaxPlayers];

            Vector3 posStart = SelectScreenCamera.Instance.m_PlayerTargets[0].position;
            Vector3 posEnd = SelectScreenCamera.Instance.m_PlayerTargets[2].position;

            for (int i = 0; i < GameFlowMC.gMaxPlayers; i++)
            {
                if (i < __instance.m_CreateUITargets.Length)
                {
                    uiTargets[i] = __instance.m_CreateUITargets[i];
                    camTargets[i] = SelectScreenCamera.Instance.m_PlayerTargets[i];
                }
                else
                {
                    uiTargets[i] = UnityEngine.Object.Instantiate(uiTargets[i - 1], uiTargets[i - 1].parent);
                    camTargets[i] = UnityEngine.Object.Instantiate(camTargets[i - 1], camTargets[i - 1].parent);
                }
            }

            __instance.m_CreateUITargets = uiTargets;
            SelectScreenCamera.Instance.m_PlayerTargets = camTargets;

            for (int j = 0; j < uiTargets.Length; j++)
                uiTargets[j].GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Lerp(-550f, 550f, j / (float)(uiTargets.Length - 1)), 129f);

            for (int k = 0; k < camTargets.Length; k++)
                camTargets[k].position = Vector3.Lerp(posStart, posEnd, k / (float)(camTargets.Length - 1));

            Debug.Log("[MultiMax Rework] : SLOT COUNT " + uiTargets.Length);
        }

        public static void DummySlide()
        {
            foreach (var dummy in UnityEngine.Object.FindObjectsOfType<DummyAttackSlide>())
            {
                if (dummy.m_Distances.Length < 1000)
                {
                    float[] expanded = new float[1000];
                    Array.Copy(dummy.m_Distances, expanded, dummy.m_Distances.Length);
                    dummy.m_Distances = expanded;
                    Debug.Log(dummy.m_Distances);
                }
            }
        }
    }
}
