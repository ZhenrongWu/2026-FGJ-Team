using FGJ.Flow;
using FGJ.LiarDice;
using UnityEngine;

namespace FGJ.Exploration
{
    [CreateAssetMenu(fileName = "Building", menuName = "FGJ/Exploration/Building Definition")]
    public sealed class BuildingDefinition : ScriptableObject
    {
        [SerializeField] private string buildingId = "Building";
        [SerializeField] private Sprite exterior;
        [SerializeField] private Vector2 exteriorOffset;
        [SerializeField] private int exteriorSortingOrder = -5;
        [SerializeField] private string enterPrompt = "按 E 進入";
        [SerializeField] private string clearedPrompt = "這裡已經安靜了";
        [Min(0f)] [SerializeField] private float interactRange = 1.5f;
        [SerializeField] private string gameplayScene = SceneNames.Gameplay;
        [SerializeField] private LiarDiceConfig gameplayConfig;

        public string BuildingId => buildingId;
        public Sprite Exterior => exterior;
        public Vector2 ExteriorOffset => exteriorOffset;
        public int ExteriorSortingOrder => exteriorSortingOrder;
        public string EnterPrompt => enterPrompt;
        public string ClearedPrompt => clearedPrompt;
        public float InteractRange => interactRange;
        public string GameplayScene => gameplayScene;
        public LiarDiceConfig GameplayConfig => gameplayConfig;

        public void Configure(string id, string enter, string cleared, float range, LiarDiceConfig config)
        {
            buildingId = id;
            enterPrompt = enter;
            clearedPrompt = cleared;
            interactRange = range;
            gameplayConfig = config;
        }
    }
}
