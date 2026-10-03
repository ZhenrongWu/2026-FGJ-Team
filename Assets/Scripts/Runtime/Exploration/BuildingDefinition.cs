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
        [SerializeField] private LiarDiceConfig gameplayConfig;
        [SerializeField] private MonsterProfile monster;

        public string BuildingId => buildingId;
        public Sprite Exterior => exterior;
        public Vector2 ExteriorOffset => exteriorOffset;
        public int ExteriorSortingOrder => exteriorSortingOrder;
        public string EnterPrompt => enterPrompt;
        public string ClearedPrompt => clearedPrompt;
        public float InteractRange => interactRange;
        public LiarDiceConfig GameplayConfig => gameplayConfig;
        public MonsterProfile Monster => monster;

        public void Configure(string id, string enter, string cleared, float range, LiarDiceConfig config,
            MonsterProfile buildingMonster = null)
        {
            monster = buildingMonster;
            buildingId = id;
            enterPrompt = enter;
            clearedPrompt = cleared;
            interactRange = range;
            gameplayConfig = config;
        }
    }
}
