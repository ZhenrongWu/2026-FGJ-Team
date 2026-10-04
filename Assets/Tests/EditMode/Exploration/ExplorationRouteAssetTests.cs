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
        public void Route_BuildingsShareTheSameSettingsExceptArtAndEncounter()
        {
            var first = _route.Placements[0].building;

            foreach (var building in _route.Placements.Select(placement => placement.building))
            {
                Assert.AreEqual(first.ExteriorScale, building.ExteriorScale, building.name);
                Assert.AreEqual(first.ExteriorSortingOrder, building.ExteriorSortingOrder, building.name);
                Assert.AreEqual(first.EnterPrompt, building.EnterPrompt, building.name);
                Assert.AreEqual(first.ClearedPrompt, building.ClearedPrompt, building.name);
                Assert.AreEqual(first.InteractRange, building.InteractRange, building.name);
            }
        }

        [Test]
        public void Route_EachBuildingHasItsOwnLevelConfigAndMonster()
        {
            var buildings = _route.Placements.Select(placement => placement.building).ToList();

            CollectionAssert.AllItemsAreNotNull(buildings.Select(building => building.GameplayConfig));
            CollectionAssert.AllItemsAreNotNull(buildings.Select(building => building.Monster));
            CollectionAssert.AllItemsAreUnique(buildings.Select(building => building.GameplayConfig));
            CollectionAssert.AllItemsAreUnique(buildings.Select(building => building.Monster));
        }

        [Test]
        public void Route_EachBuildingHasMatchingExteriorAndOutline()
        {
            foreach (var building in _route.Placements.Select(placement => placement.building))
            {
                Assert.IsNotNull(building.Exterior, building.name);
                Assert.IsNotNull(building.Outline, building.name);
                StringAssert.EndsWith("_Exterior", building.Exterior.name, building.name);
                Assert.AreEqual(building.Exterior.name.Replace("_Exterior", "_Outline"), building.Outline.name,
                    building.name);
                Assert.AreEqual(building.Exterior.rect.size, building.Outline.rect.size, building.name);
            }
        }

        [TestCase(1, "Tavern_Exterior")]
        [TestCase(2, "Mayor_Exterior")]
        [TestCase(3, "Tavern_Exterior")]
        [TestCase(4, "Tavern_Exterior")]
        public void Route_LevelUsesItsBuildingArt(int level, string exteriorName)
        {
            Assert.AreEqual(exteriorName, _route.Placements[level - 1].building.Exterior.name);
        }

        [Test]
        public void Route_InteractSpanMatchesBuildingWidth()
        {
            var measurer = new BuildingInteractSpanMeasurer();

            foreach (var building in _route.Placements.Select(placement => placement.building))
            {
                var expected = measurer.Measure(building);
                Assert.IsTrue(building.UsesInteractSpan, building.name);
                Assert.AreEqual(expected.x, building.InteractSpan.x, 0.01f, building.name);
                Assert.AreEqual(expected.y, building.InteractSpan.y, 0.01f, building.name);
            }
        }

        [Test]
        public void Route_InteractSpansOfNeighbouringBuildingsDoNotOverlap()
        {
            var placements = _route.Placements;
            for (var i = 0; i < placements.Count; i++)
            {
                var current = placements[i];
                var next = placements[(i + 1) % placements.Count];
                var nextX = i + 1 < placements.Count ? next.x : next.x + _route.LoopLength;
                var currentRight = current.x + current.building.InteractSpan.y;
                var nextLeft = nextX - next.building.InteractSpan.x;
                Assert.Less(currentRight, nextLeft, $"{current.building.name} → {next.building.name}");
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
