using System.Linq;
using FGJ.Editor;
using FGJ.Exploration;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace FGJ.Tests.EditMode.Exploration
{
    public class ParallaxOpeningTests
    {
        private GameObject _layerObject;
        private ParallaxLayer _layer;
        private Texture2D _texture;
        private Sprite _flat;
        private Sprite _cave;

        [SetUp]
        public void SetUp()
        {
            _layerObject = new GameObject("Layer");
            _layer = _layerObject.AddComponent<ParallaxLayer>();
            _texture = new Texture2D(4, 4);
            _flat = Sprite.Create(_texture, new Rect(0, 0, 4, 4), Vector2.zero);
            _cave = Sprite.Create(_texture, new Rect(0, 0, 4, 4), Vector2.zero);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_layerObject);
            Object.DestroyImmediate(_flat);
            Object.DestroyImmediate(_cave);
            Object.DestroyImmediate(_texture);
        }

        [Test]
        public void SpriteForTile_StartOfRoute_UsesCaveExit()
        {
            _layer.SetOpening(_flat, _cave, 1);

            Assert.AreSame(_cave, _layer.SpriteForTile(-1));
            Assert.AreSame(_cave, _layer.SpriteForTile(0));
        }

        [Test]
        public void SpriteForTile_AfterCaveExit_UsesFlatGroundForever()
        {
            _layer.SetOpening(_flat, _cave, 1);

            Assert.AreSame(_flat, _layer.SpriteForTile(1));
            Assert.AreSame(_flat, _layer.SpriteForTile(500));
        }

        [Test]
        public void SpriteForTile_WithoutOpeningSprite_UsesTileSprite()
        {
            _layer.SetOpening(_flat, null, 1);

            Assert.AreSame(_flat, _layer.SpriteForTile(0));
        }
    }

    public class CaveBackgroundAssetTests
    {
        private GameObject _background;

        [SetUp]
        public void SetUp()
        {
            _background = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultPrefabs.BackgroundPath);
            Assert.IsNotNull(_background, $"找不到 {DefaultPrefabs.BackgroundPath}");
        }

        [Test]
        public void Background_HasOneLayerPerOceanSpriteInOrder()
        {
            var layerNames = Enumerable.Range(0, _background.transform.childCount)
                .Select(i => _background.transform.GetChild(i).name);

            CollectionAssert.AreEqual(DefaultPrefabs.Layers.Select(layer => layer.name), layerNames);
        }

        [Test]
        public void Background_EveryLayerShowsItsOceanSprite()
        {
            foreach (var layer in DefaultPrefabs.Layers)
            {
                var renderer = _background.transform.Find(layer.name).GetComponentInChildren<SpriteRenderer>();
                Assert.AreEqual(layer.sprite, renderer.sprite?.name, layer.name);
            }
        }

        [Test]
        public void GroundLayer_StartsWithCaveExitThenFlatSeabed()
        {
            var ground = _background.transform.Find(DefaultPrefabs.GroundLayerName).GetComponent<ParallaxLayer>();

            Assert.AreEqual(DefaultPrefabs.CaveExitSprite, ground.SpriteForTile(0)?.name);
            Assert.AreEqual(DefaultPrefabs.GroundSprite, ground.SpriteForTile(DefaultPrefabs.CaveExitTileCount)?.name);
        }
    }
}
