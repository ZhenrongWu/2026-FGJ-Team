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

        [TestCase(25f, ExpectedResult = 2)]
        [TestCase(56f, ExpectedResult = 2)]
        [TestCase(60f, ExpectedResult = 3)]
        public int TilesNeeded_CoversViewWhileScrolling(float viewWidth)
        {
            return ParallaxLayer.TilesNeeded(viewWidth, 56f);
        }
    }
}
