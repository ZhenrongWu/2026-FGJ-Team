using FGJ.Exploration;
using NUnit.Framework;
using UnityEngine;

namespace FGJ.Tests.EditMode.Exploration
{
    public class BuildingRowLayoutTests
    {
        private readonly BuildingRowLayout _layout = new BuildingRowLayout(44f, 10f);

        [Test]
        public void Positions_FirstBuilding_StartsAtFirstX()
        {
            var positions = _layout.Positions(new[] { new Vector2(3f, 5f) });

            Assert.AreEqual(44f, positions[0], 1e-4f);
        }

        [Test]
        public void Positions_NeighbouringBuildings_KeepTheSameGapBetweenEdges()
        {
            var spans = new[] { new Vector2(12f, 8f), new Vector2(3f, 6f), new Vector2(17f, 17f) };

            var positions = _layout.Positions(spans);

            Assert.AreEqual(44f + 8f + 10f + 3f, positions[1], 1e-4f);
            Assert.AreEqual(positions[1] + 6f + 10f + 17f, positions[2], 1e-4f);
        }

        [Test]
        public void LoopLength_LeavesTheSameGapBeforeTheFirstBuildingRepeats()
        {
            var spans = new[] { new Vector2(12f, 8f), new Vector2(3f, 6f) };
            var positions = _layout.Positions(spans);

            var loop = _layout.LoopLength(spans);

            var lastRight = positions[1] + 6f;
            var repeatedFirstLeft = positions[0] + loop - 12f;
            Assert.AreEqual(10f, repeatedFirstLeft - lastRight, 1e-4f);
        }

        [Test]
        public void LoopLength_NoBuildings_IsZero()
        {
            Assert.AreEqual(0f, _layout.LoopLength(new Vector2[0]));
        }
    }
}
