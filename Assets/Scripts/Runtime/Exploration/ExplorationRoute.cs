using System;
using System.Collections.Generic;
using UnityEngine;

namespace FGJ.Exploration
{
    [Serializable]
    public sealed class BuildingPlacement
    {
        public BuildingDefinition building;
        public float x;

        public BuildingPlacement(BuildingDefinition building, float x)
        {
            this.building = building;
            this.x = x;
        }
    }

    [CreateAssetMenu(fileName = "ExplorationRoute", menuName = "FGJ/Exploration/Route")]
    public sealed class ExplorationRoute : ScriptableObject
    {
        [SerializeField] private List<BuildingPlacement> placements = new List<BuildingPlacement>();
        [Tooltip("建築物重複出現的間距，0 表示不循環")]
        [Min(0f)] [SerializeField] private float loopLength;

        public IReadOnlyList<BuildingPlacement> Placements => placements;
        public float LoopLength => loopLength;
        public bool Loops => loopLength > 0f;

        public void SetPlacements(IEnumerable<BuildingPlacement> buildingPlacements)
        {
            placements = new List<BuildingPlacement>(buildingPlacements);
        }

        public void SetLoopLength(float length)
        {
            loopLength = Mathf.Max(0f, length);
        }

        public float NearestCopyX(float baseX, float nearX)
        {
            if (!Loops)
                return baseX;
            var loopIndex = Mathf.Max(0, Mathf.RoundToInt((nearX - baseX) / loopLength));
            return baseX + loopIndex * loopLength;
        }
    }
}
