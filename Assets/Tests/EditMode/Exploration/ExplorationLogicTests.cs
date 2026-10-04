using FGJ.Exploration;
using NUnit.Framework;
using UnityEngine;

namespace FGJ.Tests.EditMode.Exploration
{
    public class SideScrollMotorTests
    {
        [Test]
        public void Step_RightInput_MovesBySpeedTimesDelta()
        {
            var motor = new SideScrollMotor(5f, 0f, 10f, 4f);

            motor.Step(1f, 0.5f);

            Assert.AreEqual(7f, motor.X, 1e-5f);
            Assert.AreEqual(1, motor.Facing);
            Assert.IsTrue(motor.IsMoving);
        }

        [Test]
        public void Step_LeftInput_FacesLeft()
        {
            var motor = new SideScrollMotor(5f, 0f, 10f, 4f);

            motor.Step(-1f, 0.25f);

            Assert.AreEqual(4f, motor.X, 1e-5f);
            Assert.AreEqual(-1, motor.Facing);
        }

        [Test]
        public void Step_InsideDeadZone_StopsAndKeepsFacing()
        {
            var motor = new SideScrollMotor(5f, 0f, 10f, 4f);
            motor.Step(-1f, 0.1f);

            motor.Step(0.05f, 1f);

            Assert.IsFalse(motor.IsMoving);
            Assert.AreEqual(-1, motor.Facing);
        }

        [Test]
        public void Step_AgainstBound_ClampsAndReportsNotMoving()
        {
            var motor = new SideScrollMotor(9.9f, 0f, 10f, 4f);
            motor.Step(1f, 1f);
            Assert.AreEqual(10f, motor.X);

            motor.Step(1f, 1f);

            Assert.AreEqual(10f, motor.X);
            Assert.IsFalse(motor.IsMoving);
        }

        [Test]
        public void Constructor_StartOutsideBounds_IsClamped()
        {
            Assert.AreEqual(0f, new SideScrollMotor(-3f, 0f, 10f, 4f).X);
        }

        [Test]
        public void Teleport_ClampsToBounds()
        {
            var motor = new SideScrollMotor(5f, 0f, 10f, 4f);
            motor.Teleport(42f);
            Assert.AreEqual(10f, motor.X);
        }
    }

    public class SpriteClipTests
    {
        private static SpriteClip Clip(bool loop) => new SpriteClip("Walk", new Sprite[3], 6f, loop);

        [TestCase(0f, ExpectedResult = 0)]
        [TestCase(0.2f, ExpectedResult = 1)]
        [TestCase(0.4f, ExpectedResult = 2)]
        [TestCase(0.5f, ExpectedResult = 0)]
        public int FrameAt_Looping_WrapsAround(float seconds) => Clip(true).FrameAt(seconds);

        [Test]
        public void FrameAt_NotLooping_HoldsLastFrameAndFinishes()
        {
            var clip = Clip(false);

            Assert.AreEqual(2, clip.FrameAt(5f));
            Assert.IsTrue(clip.IsFinishedAt(0.5f));
            Assert.IsFalse(clip.IsFinishedAt(0.4f));
        }

        [Test]
        public void FrameAt_NoFrames_ReturnsMinusOne()
        {
            Assert.AreEqual(-1, new SpriteClip("Empty", new Sprite[0], 6f, true).FrameAt(1f));
        }
    }

    public class CameraAndParallaxTests
    {
        [TestCase(3f, ExpectedResult = 12.5f)]
        [TestCase(30f, ExpectedResult = 30f)]
        [TestCase(55f, ExpectedResult = 43.5f)]
        public float ClampX_KeepsViewInsideLevel(float targetX) => CameraFollow2D.ClampX(targetX, 0f, 56f, 12.5f);

        [Test]
        public void ClampX_LevelNarrowerThanView_CentersCamera()
        {
            Assert.AreEqual(5f, CameraFollow2D.ClampX(9f, 0f, 10f, 12.5f));
        }

        [TestCase(10f, 0f, ExpectedResult = 0f)]
        [TestCase(10f, 0.5f, ExpectedResult = 5f)]
        [TestCase(-20f, 0.8f, ExpectedResult = -16f)]
        public float Offset_FollowsCameraByFactor(float cameraX, float factor)
        {
            return ParallaxLayer.Offset(cameraX, factor);
        }

        [TestCase(0f, 0f, ExpectedResult = 0)]
        [TestCase(55f, 0f, ExpectedResult = 0)]
        [TestCase(56f, 0f, ExpectedResult = 1)]
        [TestCase(130f, 0f, ExpectedResult = 2)]
        [TestCase(-1f, 0f, ExpectedResult = -1)]
        [TestCase(100f, 50f, ExpectedResult = 0)]
        public int FirstTileIndex_FindsTileUnderViewLeftEdge(float viewLeft, float offset)
        {
            return ParallaxLayer.FirstTileIndex(viewLeft, offset, 56f);
        }

        [TestCase(0, ExpectedResult = false)]
        [TestCase(1, ExpectedResult = true)]
        [TestCase(2, ExpectedResult = false)]
        [TestCase(-1, ExpectedResult = true)]
        [TestCase(-2, ExpectedResult = false)]
        public bool IsMirrored_AlternatesSoSeamsMatch(int tileIndex) => ParallaxLayer.IsMirrored(tileIndex);

        [TestCase(0f, 0, false, ExpectedResult = 0f)]
        [TestCase(0f, 1, true, ExpectedResult = 112f)]
        [TestCase(10f, -1, true, ExpectedResult = 10f)]
        [TestCase(10f, 2, false, ExpectedResult = 122f)]
        public float TilePositionX_MirroredTileShiftsByWidthToKeepSameSpan(float offset, int index, bool mirrored)
        {
            return ParallaxLayer.TilePositionX(offset, index, 56f, mirrored);
        }

        [TestCase(-1, 1, ExpectedResult = true)]
        [TestCase(0, 1, ExpectedResult = true)]
        [TestCase(1, 1, ExpectedResult = false)]
        [TestCase(0, 0, ExpectedResult = false)]
        public bool IsOpeningTile_OnlyTilesBeforeOpeningCount(int tileIndex, int openingTileCount)
        {
            return ParallaxLayer.IsOpeningTile(tileIndex, openingTileCount);
        }

        [TestCase(25f, ExpectedResult = 2)]
        [TestCase(56f, ExpectedResult = 2)]
        [TestCase(60f, ExpectedResult = 3)]
        public int TilesNeeded_CoversViewWhileScrolling(float viewWidth)
        {
            return ParallaxLayer.TilesNeeded(viewWidth, 56f);
        }
    }

    public class ExplorationRouteTests
    {
        private ExplorationRoute _route;

        [SetUp]
        public void SetUp()
        {
            _route = ScriptableObject.CreateInstance<ExplorationRoute>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_route);
        }

        [Test]
        public void NearestCopyX_WithoutLoop_ReturnsBaseX()
        {
            Assert.IsFalse(_route.Loops);
            Assert.AreEqual(44f, _route.NearestCopyX(44f, 500f), 1e-4f);
        }

        [Test]
        public void NearestCopyX_PastLastBuilding_ReturnsNextLoopCopy()
        {
            _route.SetLoopLength(120f);

            Assert.AreEqual(164f, _route.NearestCopyX(44f, 150f), 1e-4f);
            Assert.AreEqual(284f, _route.NearestCopyX(44f, 290f), 1e-4f);
        }

        [Test]
        public void NearestCopyX_BeforeFirstLoop_NeverGoesBelowBaseX()
        {
            _route.SetLoopLength(120f);

            Assert.AreEqual(44f, _route.NearestCopyX(44f, -500f), 1e-4f);
        }

        [Test]
        public void SetLoopLength_Negative_DisablesLoop()
        {
            _route.SetLoopLength(-5f);

            Assert.IsFalse(_route.Loops);
        }
    }

    public class OutlinePulseTests
    {
        [Test]
        public void AlphaAt_Start_IsMinAlpha()
        {
            var pulse = new OutlinePulse(1f, 0.2f, 0.9f);

            Assert.AreEqual(0.2f, pulse.AlphaAt(0f), 1e-4f);
        }

        [Test]
        public void AlphaAt_HalfPeriod_IsMaxAlpha()
        {
            var pulse = new OutlinePulse(1f, 0.2f, 0.9f);

            Assert.AreEqual(0.9f, pulse.AlphaAt(0.5f), 1e-4f);
        }

        [Test]
        public void AlphaAt_FullPeriod_RepeatsFromMinAlpha()
        {
            var pulse = new OutlinePulse(1f, 0.2f, 0.9f);

            Assert.AreEqual(pulse.AlphaAt(0.3f), pulse.AlphaAt(1.3f), 1e-4f);
            Assert.AreEqual(0.2f, pulse.AlphaAt(2f), 1e-4f);
        }

        [Test]
        public void Constructor_OutOfRangeValues_AreClamped()
        {
            var pulse = new OutlinePulse(0f, -1f, 2f);

            Assert.Greater(pulse.Period, 0f);
            Assert.AreEqual(0f, pulse.MinAlpha);
            Assert.AreEqual(1f, pulse.MaxAlpha);
        }
    }

    public class BuildingRangeTests
    {
        private BuildingDefinition _building;

        [SetUp]
        public void SetUp()
        {
            _building = ScriptableObject.CreateInstance<BuildingDefinition>();
            _building.Configure("Test", "enter", "cleared", 1.5f, null);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_building);
        }

        [Test]
        public void IsWithinDoor_UsesSymmetricDoorRange()
        {
            Assert.IsTrue(_building.IsWithinDoor(-1.5f));
            Assert.IsTrue(_building.IsWithinDoor(1.5f));
            Assert.IsFalse(_building.IsWithinDoor(-2f));
            Assert.IsFalse(_building.IsWithinDoor(2f));
        }

        [Test]
        public void IsWithinDoor_IgnoresBuildingSpan()
        {
            _building.SetBuildingSpan(8f, 10.5f);

            Assert.IsTrue(_building.IsWithinDoor(1f));
            Assert.IsFalse(_building.IsWithinDoor(5f));
            Assert.IsFalse(_building.IsWithinDoor(-5f));
        }

        [Test]
        public void IsWithinBuilding_WithoutSpan_FallsBackToDoorRange()
        {
            Assert.IsFalse(_building.UsesBuildingSpan);
            Assert.IsTrue(_building.IsWithinBuilding(-1.5f));
            Assert.IsTrue(_building.IsWithinBuilding(1.5f));
            Assert.IsFalse(_building.IsWithinBuilding(2f));
        }

        [Test]
        public void IsWithinBuilding_WithSpan_FollowsBuildingWidthOnEachSide()
        {
            _building.SetBuildingSpan(8f, 10.5f);

            Assert.IsTrue(_building.UsesBuildingSpan);
            Assert.IsTrue(_building.IsWithinBuilding(-8f));
            Assert.IsTrue(_building.IsWithinBuilding(10.5f));
            Assert.IsFalse(_building.IsWithinBuilding(-8.1f));
            Assert.IsFalse(_building.IsWithinBuilding(10.6f));
        }

        [Test]
        public void SetBuildingSpan_Negative_ClampsToZero()
        {
            _building.SetBuildingSpan(-3f, -1f);

            Assert.AreEqual(UnityEngine.Vector2.zero, _building.BuildingSpan);
            Assert.IsFalse(_building.UsesBuildingSpan);
        }
    }
}
