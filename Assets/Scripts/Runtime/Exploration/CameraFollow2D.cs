using UnityEngine;

namespace FGJ.Exploration
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float levelMinX;
        [SerializeField] private float levelMaxX = 56f;
        [Min(0f)] [SerializeField] private float smoothTime = 0.15f;

        private Camera _camera;
        private float _velocity;

        private Camera Camera => _camera != null ? _camera : _camera = GetComponent<Camera>();
        private float HalfWidth => Camera.orthographicSize * Camera.aspect;

        public void Configure(Transform followTarget, float minX, float maxX)
        {
            target = followTarget;
            levelMinX = minX;
            levelMaxX = maxX;
        }

        public static float ClampX(float targetX, float levelMinX, float levelMaxX, float halfWidth)
        {
            var low = levelMinX + halfWidth;
            var high = levelMaxX - halfWidth;
            if (low > high)
                return (levelMinX + levelMaxX) * 0.5f;
            return Mathf.Clamp(targetX, low, high);
        }

        public void SnapToTarget()
        {
            if (target == null)
                return;
            SetX(ClampX(target.position.x, levelMinX, levelMaxX, HalfWidth));
            _velocity = 0f;
        }

        private void Start()
        {
            SnapToTarget();
        }

        private void LateUpdate()
        {
            if (target == null)
                return;
            var desired = ClampX(target.position.x, levelMinX, levelMaxX, HalfWidth);
            SetX(Mathf.SmoothDamp(transform.position.x, desired, ref _velocity, smoothTime));
        }

        private void SetX(float x)
        {
            var position = transform.position;
            position.x = x;
            transform.position = position;
        }
    }
}
