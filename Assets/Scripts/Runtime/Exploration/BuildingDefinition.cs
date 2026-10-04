using FGJ.LiarDice;
using UnityEngine;

namespace FGJ.Exploration
{
    [CreateAssetMenu(fileName = "Building", menuName = "FGJ/Exploration/Building Definition")]
    public sealed class BuildingDefinition : ScriptableObject
    {
        [SerializeField] private string buildingId = "Building";
        [Tooltip("留空時使用場景預設的入口 Prefab")]
        [SerializeField] private RoomEntrance entrancePrefab;
        [SerializeField] private Sprite exterior;
        [Tooltip("角色靠近時閃爍的外框圖，需與外觀圖同尺寸")]
        [SerializeField] private Sprite outline;
        [Tooltip("以 Scale 為 1 時的位置設定，實際位置會隨 Scale 一起縮放，讓門口維持對齊")]
        [SerializeField] private Vector2 exteriorOffset;
        [SerializeField] private Vector2 exteriorScale = Vector2.one;
        [SerializeField] private int exteriorSortingOrder = -5;
        [SerializeField] private string enterPrompt = "按 ↑ 進入";
        [SerializeField] private string clearedPrompt = "這裡已經安靜了";
        [Min(0f)] [SerializeField] private float interactRange = 1.5f;
        [Tooltip("依建築寬度的觸發範圍：x 為入口往左距離、y 為往右距離；皆為 0 時改用 Interact Range")]
        [SerializeField] private Vector2 interactSpan;
        [SerializeField] private LiarDiceConfig gameplayConfig;
        [SerializeField] private MonsterProfile monster;

        public string BuildingId => buildingId;
        public RoomEntrance EntrancePrefab => entrancePrefab;
        public Sprite Exterior => exterior;
        public Sprite Outline => outline;
        public Vector2 ExteriorOffset => exteriorOffset;
        public Vector2 ExteriorScale => exteriorScale;
        public Vector2 ScaledExteriorOffset => Vector2.Scale(exteriorOffset, exteriorScale);
        public int ExteriorSortingOrder => exteriorSortingOrder;
        public string EnterPrompt => enterPrompt;
        public string ClearedPrompt => clearedPrompt;
        public float InteractRange => interactRange;
        public Vector2 InteractSpan => interactSpan;
        public bool UsesInteractSpan => interactSpan.x > 0f || interactSpan.y > 0f;
        public LiarDiceConfig GameplayConfig => gameplayConfig;
        public MonsterProfile Monster => monster;

        public void SetExterior(Sprite exteriorSprite, Vector2 offset)
        {
            exterior = exteriorSprite;
            exteriorOffset = offset;
        }

        public void SetOutline(Sprite outlineSprite)
        {
            outline = outlineSprite;
        }

        public void SetInteractSpan(float left, float right)
        {
            interactSpan = new Vector2(Mathf.Max(0f, left), Mathf.Max(0f, right));
        }

        public bool IsWithinInteract(float offsetFromEntrance)
        {
            if (!UsesInteractSpan)
                return Mathf.Abs(offsetFromEntrance) <= interactRange;
            return offsetFromEntrance >= -interactSpan.x && offsetFromEntrance <= interactSpan.y;
        }

        public void SetExteriorScale(Vector2 scale)
        {
            exteriorScale = scale;
        }

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
