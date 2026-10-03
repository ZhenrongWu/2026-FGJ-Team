using NUnit.Framework;
using UnityEngine;

namespace FGJ.Tests.PlayMode
{
    internal static class TestPrefabs
    {
        public const string LiarDiceRoomPath = "Assets/Prefabs/Gameplay/LiarDiceRoom.prefab";
        public const string DiceTablePath = "Assets/Prefabs/Gameplay/DiceTable.prefab";
        public const string PlayerPath = "Assets/Prefabs/Exploration/Player.prefab";

        public static T Instantiate<T>(string path) where T : Component
        {
#if UNITY_EDITOR
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.IsNotNull(prefab, $"找不到 {path}，請先執行 FGJ/Prefabs/Create Missing Default Prefabs");
            return Object.Instantiate(prefab).GetComponent<T>();
#else
            Assert.Ignore("Prefab 測試需要在編輯器中執行。");
            return null;
#endif
        }
    }
}
