using System.Linq;
using FGJ.Editor;
using FGJ.Exploration;
using NUnit.Framework;
using UnityEditor;

namespace FGJ.Tests.EditMode.Exploration
{
    public class ExplorationRouteAssetTests
    {
        private ExplorationRoute _route;

        [SetUp]
        public void SetUp()
        {
            _route = AssetDatabase.LoadAssetAtPath<ExplorationRoute>(ExplorationSceneMenu.RoutePath);
            Assert.IsNotNull(_route, $"找不到 {ExplorationSceneMenu.RoutePath}");
        }

        [Test]
        public void Route_HasOneBuildingPerLevel()
        {
            Assert.AreEqual(ExplorationSceneMenu.LevelCount, _route.Placements.Count);
            Assert.IsTrue(_route.Placements.All(placement => placement.building != null));
        }

        [Test]
        public void Route_LoopsBackToFirstBuildingAfterLastLevel()
        {
            var placements = _route.Placements;
            var spacing = placements[1].x - placements[0].x;

            Assert.AreEqual(spacing * placements.Count, _route.LoopLength, 1e-4f);
        }

        [Test]
        public void Route_BuildingsAreNamedAfterTheirLevelInOrder()
        {
            for (var i = 0; i < _route.Placements.Count; i++)
            {
                var building = _route.Placements[i].building;
                var level = (i + 1).ToString("00");
                Assert.AreEqual($"Building_{level}", building.name);
                Assert.AreEqual($"Building{level}", building.BuildingId);
            }
        }

        [Test]
        public void Route_BuildingsShareTheSameSettings()
        {
            var first = _route.Placements[0].building;
            Assert.IsNotNull(first.Exterior);
            Assert.IsNotNull(first.Outline);

            foreach (var building in _route.Placements.Select(placement => placement.building))
            {
                Assert.AreSame(first.Exterior, building.Exterior, building.name);
                Assert.AreSame(first.Outline, building.Outline, building.name);
                Assert.AreEqual(first.ExteriorOffset, building.ExteriorOffset, building.name);
                Assert.AreEqual(first.ExteriorScale, building.ExteriorScale, building.name);
                Assert.AreEqual(first.ExteriorSortingOrder, building.ExteriorSortingOrder, building.name);
                Assert.AreEqual(first.EnterPrompt, building.EnterPrompt, building.name);
                Assert.AreEqual(first.ClearedPrompt, building.ClearedPrompt, building.name);
                Assert.AreEqual(first.InteractRange, building.InteractRange, building.name);
                Assert.AreSame(first.GameplayConfig, building.GameplayConfig, building.name);
                Assert.AreSame(first.Monster, building.Monster, building.name);
            }
        }

        [Test]
        public void Route_BuildingIdsAreUnique()
        {
            var ids = _route.Placements.Select(placement => placement.building.BuildingId).ToList();

            CollectionAssert.AllItemsAreUnique(ids);
        }
    }
}
