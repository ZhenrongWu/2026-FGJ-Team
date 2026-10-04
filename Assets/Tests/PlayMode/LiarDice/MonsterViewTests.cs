using System.Collections.Generic;
using FGJ.LiarDice;
using FGJ.LiarDice.Table;
using NUnit.Framework;
using UnityEngine;

namespace FGJ.Tests.PlayMode.LiarDice
{
    public class MonsterViewTests
    {
        private readonly List<Object> _created = new List<Object>();
        private MonsterView _view;
        private Sprite _placeholder;

        [SetUp]
        public void SetUp()
        {
            var root = Track(new GameObject("Monster"));
            var renderer = root.AddComponent<SpriteRenderer>();
            _placeholder = CreateSprite();
            _view = root.AddComponent<MonsterView>();
            _view.Configure(renderer, _placeholder);
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

        private Sprite CreateSprite()
        {
            var texture = Track(new Texture2D(4, 4));
            return Track(Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0f)));
        }

        [Test]
        public void Show_MonsterWithSprite_UsesItsSprite()
        {
            var monster = Track(ScriptableObject.CreateInstance<MonsterProfile>());
            var sprite = CreateSprite();
            monster.SetSprite(sprite);

            _view.Show(monster);

            Assert.AreSame(sprite, _view.CurrentSprite);
            Assert.IsFalse(_view.IsShowingPlaceholder);
        }

        [Test]
        public void ShowDead_MonsterWithDeadSprite_SwapsToDeadSprite()
        {
            var monster = Track(ScriptableObject.CreateInstance<MonsterProfile>());
            var dead = CreateSprite();
            monster.SetSprite(CreateSprite());
            monster.SetDeadSprite(dead);
            _view.Show(monster);

            _view.ShowDead(monster);

            Assert.AreSame(dead, _view.CurrentSprite);
            Assert.IsTrue(_view.IsShowingDead);
        }

        [Test]
        public void ShowDead_MonsterWithoutDeadSprite_KeepsAliveSprite()
        {
            var monster = Track(ScriptableObject.CreateInstance<MonsterProfile>());
            var alive = CreateSprite();
            monster.SetSprite(alive);
            _view.Show(monster);

            _view.ShowDead(monster);

            Assert.AreSame(alive, _view.CurrentSprite);
            Assert.IsFalse(_view.IsShowingDead);
        }

        [Test]
        public void Show_AfterDeath_RestoresAliveSprite()
        {
            var monster = Track(ScriptableObject.CreateInstance<MonsterProfile>());
            var alive = CreateSprite();
            monster.SetSprite(alive);
            monster.SetDeadSprite(CreateSprite());
            _view.ShowDead(monster);

            _view.Show(monster);

            Assert.AreSame(alive, _view.CurrentSprite);
            Assert.IsFalse(_view.IsShowingDead);
        }

        [Test]
        public void Show_MonsterWithoutSprite_FallsBackToPlaceholder()
        {
            _view.Show(Track(ScriptableObject.CreateInstance<MonsterProfile>()));

            Assert.AreSame(_placeholder, _view.CurrentSprite);
            Assert.IsTrue(_view.IsShowingPlaceholder);
        }
    }
}
