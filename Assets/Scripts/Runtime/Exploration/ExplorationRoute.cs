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

        public IReadOnlyList<BuildingPlacement> Placements => placements;

        public void SetPlacements(IEnumerable<BuildingPlacement> buildingPlacements)
        {
            placements = new List<BuildingPlacement>(buildingPlacements);
        }
    }
}
