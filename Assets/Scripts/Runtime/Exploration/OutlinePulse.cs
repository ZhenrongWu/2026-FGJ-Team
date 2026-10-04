using System;
using UnityEngine;

namespace FGJ.Exploration
{
    [Serializable]
    public sealed class OutlinePulse
    {
        [Min(0.05f)] [SerializeField] private float period = 0.8f;
        [Range(0f, 1f)] [SerializeField] private float minAlpha = 0.15f;
        [Range(0f, 1f)] [SerializeField] private float maxAlpha = 1f;

        public float Period => period;
        public float MinAlpha => minAlpha;
        public float MaxAlpha => maxAlpha;

        public OutlinePulse()
        {
        }

        public OutlinePulse(float period, float minAlpha, float maxAlpha)
        {
            this.period = Mathf.Max(0.05f, period);
            this.minAlpha = Mathf.Clamp01(minAlpha);
            this.maxAlpha = Mathf.Clamp01(maxAlpha);
        }

        public float AlphaAt(float elapsedSeconds)
        {
            var wave = 0.5f - 0.5f * Mathf.Cos(elapsedSeconds / period * Mathf.PI * 2f);
            return Mathf.Lerp(minAlpha, maxAlpha, wave);
        }
    }
}
