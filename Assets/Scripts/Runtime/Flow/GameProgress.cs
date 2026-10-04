using System;
using System.Collections.Generic;
using FGJ.Exploration;
using UnityEngine;

namespace FGJ.Flow
{
    [CreateAssetMenu(fileName = "GameProgress", menuName = "FGJ/Flow/Game Progress")]
    public sealed class GameProgress : ScriptableObject
    {
        [NonSerialized] private readonly HashSet<string> _clearedBuildingIds = new HashSet<string>();
        [NonSerialized] private BuildingDefinition _currentBuilding;
        [NonSerialized] private string _returnBuildingId;
        [NonSerialized] private float _enteredX;
        [NonSerialized] private float _returnX;
        [NonSerialized] private int _playerOxygen;

        public BuildingDefinition CurrentBuilding => _currentBuilding;
        public string ReturnBuildingId => _returnBuildingId;
        public float ReturnX => _returnX;

        public void ResetRun()
        {
            _clearedBuildingIds.Clear();
            _currentBuilding = null;
            _returnBuildingId = null;
            _enteredX = 0f;
            _returnX = 0f;
            _playerOxygen = 0;
        }

        public void EnterBuilding(BuildingDefinition building, float entranceX = 0f)
        {
            _currentBuilding = building;
            _returnBuildingId = null;
            _enteredX = entranceX;
        }

        public int PlayerOxygenFor(int maxOxygen)
        {
            return _playerOxygen > 0 ? Mathf.Min(_playerOxygen, maxOxygen) : maxOxygen;
        }

        public void CompleteBuilding(bool playerWon, int playerOxygenAfter = 0)
        {
            if (!playerWon)
                return;

            if (playerOxygenAfter > 0)
                _playerOxygen = playerOxygenAfter;

            if (_currentBuilding != null)
            {
                _clearedBuildingIds.Add(_currentBuilding.BuildingId);
                _returnBuildingId = _currentBuilding.BuildingId;
                _returnX = _enteredX;
            }
            _currentBuilding = null;
        }

        public bool IsCleared(string buildingId) => buildingId != null && _clearedBuildingIds.Contains(buildingId);

        public string ConsumeReturnBuilding()
        {
            var buildingId = _returnBuildingId;
            _returnBuildingId = null;
            return buildingId;
        }
    }
}
