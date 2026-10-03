using System.Collections;
using UnityEngine;

namespace FGJ.LiarDice.Table
{
    public sealed class DiceCupView : MonoBehaviour
    {
        [SerializeField] private Transform dome;
        [SerializeField] private Transform diceRoot;
        [Min(0.01f)] [SerializeField] private float trayRadius = 0.2f;
        [SerializeField] private Vector3 liftedOffset = new Vector3(0.3f, 0.12f, 0.05f);
        [SerializeField] private Vector3 liftedEuler = new Vector3(0f, 0f, -28f);
        [SerializeField] private float shakeAngle = 10f;
        [Min(0.1f)] [SerializeField] private float shakeFrequency = 9f;

        private Vector3 _closedPosition;
        private Quaternion _closedRotation;
        private bool _closedPoseCaptured;

        public Transform DiceRoot => diceRoot;
        public float TrayRadius => trayRadius;
        public bool IsLifted { get; private set; }

        public void Configure(Transform cupDome, Transform cupDiceRoot, float radius)
        {
            dome = cupDome;
            diceRoot = cupDiceRoot;
            trayRadius = radius;
            _closedPoseCaptured = false;
        }

        public void SetLiftedImmediate(bool lifted)
        {
            CaptureClosedPose();
            IsLifted = lifted;
            ApplyLift(lifted ? 1f : 0f);
        }

        public IEnumerator AnimateLift(bool lifted, float duration)
        {
            CaptureClosedPose();
            var from = IsLifted ? 1f : 0f;
            var to = lifted ? 1f : 0f;
            IsLifted = lifted;

            var elapsed = 0f;
            while (elapsed < duration)
            {
                ApplyLift(Mathf.SmoothStep(from, to, elapsed / duration));
                elapsed += Time.deltaTime;
                yield return null;
            }
            ApplyLift(to);
        }

        public IEnumerator Shake(float duration)
        {
            CaptureClosedPose();
            IsLifted = false;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                var wobble = Mathf.Sin(elapsed * shakeFrequency * Mathf.PI * 2f) * shakeAngle * (1f - elapsed / duration);
                dome.localPosition = _closedPosition + Vector3.up * Mathf.Abs(wobble) * 0.004f;
                dome.localRotation = _closedRotation * Quaternion.Euler(wobble * 0.5f, 0f, wobble);
                elapsed += Time.deltaTime;
                yield return null;
            }
            ApplyLift(0f);
        }

        private void CaptureClosedPose()
        {
            if (_closedPoseCaptured)
                return;
            _closedPosition = dome.localPosition;
            _closedRotation = dome.localRotation;
            _closedPoseCaptured = true;
        }

        private void ApplyLift(float amount)
        {
            dome.localPosition = _closedPosition + liftedOffset * amount;
            dome.localRotation = _closedRotation * Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(liftedEuler), amount);
        }
    }
}
