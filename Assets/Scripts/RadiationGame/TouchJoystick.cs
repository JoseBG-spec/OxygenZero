using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MarsOutpost.Radiation
{
    /// <summary>
    /// Large mobile joystick. It works with the project's Input System UI module and
    /// also accepts a mouse drag in the Editor for quick testing.
    /// </summary>
    public sealed class TouchJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform background;
        public RectTransform knob;
        [Range(0f, 0.9f)] public float deadZone = 0.08f;

        public Vector2 Value { get; private set; }

        private bool dragging;
        private bool interactable = true;

        public void SetInteractable(bool value)
        {
            interactable = value;
            if (!value)
            {
                ResetKnob();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!interactable)
            {
                return;
            }

            dragging = true;
            UpdateFromPointer(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragging)
            {
                UpdateFromPointer(eventData);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            dragging = false;
            ResetKnob();
        }

        private void OnDisable()
        {
            dragging = false;
            ResetKnob();
        }

        private void UpdateFromPointer(PointerEventData eventData)
        {
            if (background == null) return;

            Camera camera = eventData.pressEventCamera;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    background,
                    eventData.position,
                    camera,
                    out Vector2 localPoint))
            {
                return;
            }

            float radius = Mathf.Max(1f, Mathf.Min(background.rect.width, background.rect.height) * 0.5f);
            Vector2 clamped = Vector2.ClampMagnitude(localPoint / radius, 1f);
            Value = clamped.magnitude < deadZone ? Vector2.zero : clamped;
            if (knob != null)
            {
                knob.anchoredPosition = clamped * (background.rect.width * 0.31f);
            }
        }

        private void ResetKnob()
        {
            Value = Vector2.zero;
            if (knob != null)
            {
                knob.anchoredPosition = Vector2.zero;
            }
        }
    }
}
