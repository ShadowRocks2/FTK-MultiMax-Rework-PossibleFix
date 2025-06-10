using System;
using Photon; // depending on FTK's Photon version
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTK_MultiMax_Rework {
    public static class RoomHandler {
        public static bool CreateRoom(ref GameLogic __instance, string _roomName, bool _isOpen) {
            PhotonNetwork.offlineMode = false;

            if (!PhotonNetwork.connectedAndReady) {
                Debug.LogError("[MultiMax Rework] PhotonNetwork is not connected!");
                return false; // skip original method to avoid errors
            }

            RoomOptions roomOptions = new RoomOptions {
                IsOpen = _isOpen,
                IsVisible = _isOpen,
                MaxPlayers = (__instance.m_GameMode == GameLogic.GameMode.SinglePlayer)
                             ? (byte)1
                             : (byte)Mathf.Clamp(GameFlowMC.gMaxPlayers, 1, 255)
            };

            TypedLobby typedLobby = new TypedLobby {
                Type = LobbyType.Default
            };

            Debug.Log($"[MultiMax Rework] Creating room '{_roomName}' | MaxPlayers: {roomOptions.MaxPlayers} | Open: {_isOpen}");

            PhotonNetwork.CreateRoom(_roomName, roomOptions, typedLobby);
            return false; // prevent original method from running
        }
    }
}
