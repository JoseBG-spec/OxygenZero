using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace MarsOutpost.Radiation
{
    /// <summary>
    /// Keyboard and gamepad fallback for the radiation drill. Touch is handled by the
    /// on-screen joystick and action button; no feature requires the E key.
    /// </summary>
    public static class GameInput
    {
        public static Vector2 Move()
        {
#if ENABLE_INPUT_SYSTEM
            Vector2 value = Vector2.zero;
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) value.x -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) value.x += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) value.y -= 1f;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) value.y += 1f;
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null && gamepad.leftStick.ReadValue().sqrMagnitude > 0.01f)
            {
                value = gamepad.leftStick.ReadValue();
            }

            return Vector2.ClampMagnitude(value, 1f);
#else
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
#endif
        }

        public static bool InteractPressed()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            Gamepad gamepad = Gamepad.current;
            return (keyboard != null && keyboard.eKey.wasPressedThisFrame) ||
                   (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame);
#else
            return Input.GetKeyDown(KeyCode.E);
#endif
        }

        public static bool DialogueAdvancePressed()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            Gamepad gamepad = Gamepad.current;
            return (keyboard != null &&
                    (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame)) ||
                   (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame);
#else
            return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return);
#endif
        }
    }
}
