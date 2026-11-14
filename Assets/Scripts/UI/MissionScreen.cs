using MarsOutpost.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MarsOutpost.UI
{
    /// <summary>
    /// Base screen controller. Handles the Android back button (the new Input System
    /// reports it as Escape) and exposes an on-screen back button hook.
    /// </summary>
    public abstract class MissionScreen : MonoBehaviour
    {
        private bool backEnabled = true;

        protected MissionDirector Director => MissionDirector.Instance;

        protected virtual void Update()
        {
            if (!backEnabled)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                RequestBack();
            }
        }

        public void SetBackEnabled(bool value)
        {
            backEnabled = value;
        }

        public void RequestBack()
        {
            if (backEnabled)
            {
                HandleBack();
            }
        }

        protected abstract void HandleBack();
    }
}
