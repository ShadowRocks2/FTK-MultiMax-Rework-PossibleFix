using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FTK_MultiMax_Rework {
    public static class DummiesHandler {

        public static void CreateDummies() {
            Debug.Log("MAKING DUMMIES");

            if (FTKHub.Instance.m_Dummies.Length < 6) {
                Debug.LogError("[MultiMaxRework] FTKHub.Instance.m_Dummies does not contain enough dummies!");
                return;
            }

            List<GameObject> dummies = new List<GameObject>();

            for (int j = 0; j < Mathf.Max(3, GameFlowMC.gMaxPlayers); j++) {
                dummies.Add(CreatePlayerDummy(FTKHub.Instance.m_Dummies, j));
            }

            for (int i = 0; i < Mathf.Max(3, GameFlowMC.gMaxEnemies); i++) {
                dummies.Add(CreateEnemyDummy(FTKHub.Instance.m_Dummies, i));
            }

            FTKHub.Instance.m_Dummies = dummies.ToArray();

            Debug.Log("MultiMax - Done");
        }

        private static GameObject CreatePlayerDummy(GameObject[] source, int index) {
            GameObject dummy;
            if (index < 3) {
                dummy = source[index];
            } else {
                dummy = UnityEngine.Object.Instantiate(source[2], source[2].transform.parent);
                dummy.name = $"Player {index + 1} Dummy";
                dummy.GetComponent<PhotonView>().viewID = 10000 + index; // safe dummy range
                Debug.Log($"Created Player Dummy {index + 1}");
            }
            return dummy;
        }

        private static GameObject CreateEnemyDummy(GameObject[] source, int index) {
            GameObject dummy;
            if (index < 3) {
                dummy = source[index + 3];
            } else {
                dummy = UnityEngine.Object.Instantiate(source[5], source[5].transform.parent);
                dummy.name = $"Enemy {index + 1} Dummy";
                dummy.GetComponent<PhotonView>().viewID = 20000 + index; // safe dummy range
                Debug.Log($"Created Enemy Dummy {index + 1}");
            }
            return dummy;
        }
    }
}
