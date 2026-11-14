using UnityEngine;

namespace MarsOutpost.Radiation
{
    /// <summary>
    /// Top-down player movement with touch as the primary input and keyboard/gamepad
    /// as a fallback. A walkable-area confiner keeps the player inside the illustrated ship.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerController2D : MonoBehaviour
    {
        public TouchJoystick joystick;
        public ShipWalkableArea walkableArea;
        [Min(0.1f)] public float moveSpeed = 2.8f;
        [Min(0.05f)] public float bodyRadius = 0.22f;

        public bool InputEnabled { get; private set; } = true;

        private Rigidbody2D body;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
        }

        private void FixedUpdate()
        {
            if (!InputEnabled)
            {
                body.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 input = joystick != null ? joystick.Value : Vector2.zero;
            if (input.sqrMagnitude < 0.01f)
            {
                input = GameInput.Move();
            }

            Vector2 current = body.position;
            Vector2 target = current + Vector2.ClampMagnitude(input, 1f) * moveSpeed * Time.fixedDeltaTime;
            if (walkableArea != null)
            {
                target = walkableArea.Resolve(current, target, bodyRadius);
            }

            body.MovePosition(target);
        }

        public void SetInputEnabled(bool value)
        {
            InputEnabled = value;
            if (!value && body != null)
            {
                body.linearVelocity = Vector2.zero;
            }
        }

        public void Teleport(Vector2 position)
        {
            if (body == null)
            {
                body = GetComponent<Rigidbody2D>();
            }

            body.position = position;
            transform.position = new Vector3(position.x, position.y, 0f);
            body.linearVelocity = Vector2.zero;
        }
    }
}
