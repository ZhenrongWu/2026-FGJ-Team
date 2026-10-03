using System;
using System.Collections.Generic;
using UnityEngine;

namespace FGJ.Exploration
{
    [Serializable]
    public sealed class SpriteClip
    {
        public string name;
        public Sprite[] frames = Array.Empty<Sprite>();
        [Min(0.1f)] public float framesPerSecond = 6f;
        public bool loop = true;

        public SpriteClip()
        {
        }

        public SpriteClip(string name, Sprite[] frames, float framesPerSecond, bool loop)
        {
            this.name = name;
            this.frames = frames;
            this.framesPerSecond = framesPerSecond;
            this.loop = loop;
        }

        public int FrameAt(float elapsedSeconds)
        {
            if (frames.Length == 0)
                return -1;
            var index = Mathf.FloorToInt(elapsedSeconds * framesPerSecond);
            return loop ? index % frames.Length : Mathf.Min(index, frames.Length - 1);
        }

        public bool IsFinishedAt(float elapsedSeconds)
        {
            return !loop && elapsedSeconds * framesPerSecond >= frames.Length;
        }
    }

    public sealed class SpriteFrameAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer target;
        [SerializeField] private List<SpriteClip> clips = new List<SpriteClip>();

        private SpriteClip _current;
        private float _elapsed;

        public string CurrentClipName => _current?.name;
        public int CurrentFrame { get; private set; } = -1;
        public bool IsFinished => _current != null && _current.IsFinishedAt(_elapsed);

        public void Configure(SpriteRenderer renderer, IEnumerable<SpriteClip> spriteClips)
        {
            target = renderer;
            clips = new List<SpriteClip>(spriteClips);
            _current = null;
        }

        public void Play(string clipName)
        {
            if (_current != null && _current.name == clipName)
                return;

            _current = clips.Find(clip => clip.name == clipName);
            _elapsed = 0f;
            ApplyFrame();
        }

        public void Advance(float deltaTime)
        {
            if (_current == null)
                return;
            _elapsed += deltaTime;
            ApplyFrame();
        }

        private void Update()
        {
            Advance(Time.deltaTime);
        }

        private void ApplyFrame()
        {
            CurrentFrame = _current?.FrameAt(_elapsed) ?? -1;
            if (target != null && CurrentFrame >= 0)
                target.sprite = _current.frames[CurrentFrame];
        }
    }
}
