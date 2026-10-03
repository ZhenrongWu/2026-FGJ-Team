using System.Collections.Generic;
using FGJ.Exploration;
using NUnit.Framework;
using UnityEngine;

namespace FGJ.Tests.PlayMode.Exploration
{
    public class RoomEntranceTests
    {
        private readonly List<Object> _created = new List<Object>();
        private RoomEntrance _entrance;
        private SpriteRenderer _exterior;

        [SetUp]
        public void SetUp()
        {
            var root = Track(new GameObject("Entrance"));
            var exteriorObject = new GameObject("Exterior");
            exteriorObject.transform.SetParent(root.transform, false);
            _exterior = exteriorObject.AddComponent<SpriteRenderer>();
            exteriorObject.SetActive(false);

            _entrance = root.AddComponent<RoomEntrance>();
            _entrance.Configure(_exterior);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var created in _created)
                Object.Destroy(created);
            _created.Clear();
        }

        private T Track<T>(T created) where T : Object
        {
            _created.Add(created);
            return created;
        }

        private BuildingDefinition CreateBuilding(Sprite exterior, Vector2 offset)
        {
            var building = Track(ScriptableObject.CreateInstance<BuildingDefinition>());
            building.Configure("Tavern", "按 ↑ 進入酒館", "酒館裡已經安靜了", 1.5f, null);
            building.SetExterior(exterior, offset);
            return building;
        }

        private Sprite CreateSprite()
        {
            var texture = Track(new Texture2D(4, 4));
            return Track(Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0f)));
        }

        [Test]
        public void Bind_WithExterior_ShowsArtAtDoorOffset()
        {
            var sprite = CreateSprite();

            _entrance.Bind(CreateBuilding(sprite, new Vector2(0.74f, -0.15f)));
            _entrance.SetCleared(false);

            Assert.IsTrue(_entrance.HasExterior);
            Assert.AreSame(sprite, _exterior.sprite);
            Assert.AreEqual(new Vector3(0.74f, -0.15f, 0f), _exterior.transform.localPosition);
            Assert.AreEqual("按 ↑ 進入酒館", _entrance.Prompt);
        }

        [Test]
        public void Bind_WithExteriorScale_ScalesArtAndKeepsDoorAligned()
        {
            var building = CreateBuilding(CreateSprite(), new Vector2(0.74f, -0.15f));
            building.SetExteriorScale(new Vector2(2f, 1.5f));

            _entrance.Bind(building);

            Assert.AreEqual(new Vector3(2f, 1.5f, 1f), _exterior.transform.localScale);
            Assert.AreEqual(1.48f, _exterior.transform.localPosition.x, 1e-5f);
            Assert.AreEqual(-0.225f, _exterior.transform.localPosition.y, 1e-5f);
        }

        [Test]
        public void NewBuildingDefinition_DefaultsToUnitScale()
        {
            var building = Track(ScriptableObject.CreateInstance<BuildingDefinition>());

            Assert.AreEqual(Vector2.one, building.ExteriorScale);
        }

        [Test]
        public void Bind_WithoutExterior_HidesExteriorAndPromptFollowsClearedState()
        {
            _entrance.Bind(CreateBuilding(null, Vector2.zero));

            _entrance.SetCleared(false);
            Assert.IsFalse(_entrance.HasExterior);
            Assert.AreEqual("按 ↑ 進入酒館", _entrance.Prompt);

            _entrance.SetCleared(true);
            Assert.IsTrue(_entrance.IsCleared);
            Assert.AreEqual("酒館裡已經安靜了", _entrance.Prompt);
        }
    }
}
