using FGJ.Audio;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FGJ.Exploration
{
    public sealed class SideScrollPlayer : MonoBehaviour
    {
        public const string IdleClip = "Idle";
        public const string WalkClip = "Walk";
        public const string EnterClip = "Enter";

        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private SpriteFrameAnimator animator;
        [Min(0f)] [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float minX = 2f;
        [SerializeField] private float maxX = 54f;
        [SerializeField] private GameAudio gameAudio;

        private SideScrollMotor _motor;
        private float? _moveInputOverride;
        private bool _isEntering;
        private bool _footstepsPlaying;

        public bool InputLocked { get; set; }
        public float X => transform.position.x;
        public int Facing => Motor.Facing;
        public bool IsMoving => Motor.IsMoving;

        private SideScrollMotor Motor => _motor ??= new SideScrollMotor(transform.position.x, minX, maxX, moveSpeed);

        public void Configure(SpriteRenderer renderer, SpriteFrameAnimator frameAnimator, float leftBound,
            float rightBound, float speed)
        {
            spriteRenderer = renderer;
            animator = frameAnimator;
            minX = leftBound;
            maxX = rightBound;
            moveSpeed = speed;
            _motor = null;
        }

        public void SetAudio(GameAudio audio)
        {
            gameAudio = audio;
        }

        public void SetMoveInputOverride(float? input)
        {
            _moveInputOverride = input;
        }

        public void PlaceAt(float x)
        {
            Motor.Teleport(x);
            ApplyPosition();
        }

        public void PlayEnter()
        {
            InputLocked = true;
            _isEntering = true;
            SetFootsteps(false);
            spriteRenderer.flipX = false;
            animator.Play(EnterClip);
        }

        private void Start()
        {
            ApplyPosition();
            animator.Play(IdleClip);
        }

        private void Update()
        {
            if (_isEntering)
                return;

            var input = InputLocked ? 0f : _moveInputOverride ?? ReadKeyboardAxis();
            Motor.Step(input, Time.deltaTime);
            ApplyPosition();
            spriteRenderer.flipX = Motor.Facing < 0;
            animator.Play(Motor.IsMoving ? WalkClip : IdleClip);
            SetFootsteps(Motor.IsMoving);
        }

        private void OnDisable()
        {
            SetFootsteps(false);
        }

        private void SetFootsteps(bool walking)
        {
            if (gameAudio == null || walking == _footstepsPlaying)
                return;
            _footstepsPlaying = walking;
            gameAudio.SetFootsteps(walking);
        }

        private void ApplyPosition()
        {
            var position = transform.position;
            position.x = Motor.X;
            transform.position = position;
        }

        private static float ReadKeyboardAxis()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return 0f;

            var axis = 0f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                axis -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                axis += 1f;
            return axis;
        }
    }
}
