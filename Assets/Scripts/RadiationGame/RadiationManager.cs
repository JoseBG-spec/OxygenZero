using System;
using UnityEngine;

namespace MarsOutpost.Radiation
{
    /// <summary>
    /// Owns the simulated hazard. It starts at a random approved spawn point, grows
    /// slowly, and shrinks when the control room activates shielding. It never restarts
    /// on its own. All values are game values, not radiation measurements.
    /// </summary>
    public sealed class RadiationManager : MonoBehaviour
    {
        public enum State
        {
            Idle,
            Alert,
            Contained
        }

        public RadiationConfig config;
        public RadiationVisual visual;
        public ShipWalkableArea walkableArea;
        public RadiationSpawnPoint[] spawnPoints;
        public LayerMask obstacleMask;

        public State CurrentState { get; private set; } = State.Idle;
        public bool IsAlertActive => CurrentState == State.Alert;
        public bool IsContained => CurrentState == State.Contained;
        public bool IsActive => CurrentState != State.Idle;
        public bool IsPaused { get; private set; }
        public Vector2 Origin { get; private set; }
        public float Radius { get; private set; }
        public RadiationSpawnPoint ActiveSpawn { get; private set; }
        public RadiationConfig Config => config;

        public event Action AlertStarted;
        public event Action ShieldingActivated;
        public event Action HazardContained;

        private float age;

        private void Awake()
        {
            if (visual != null)
            {
                visual.Hide();
            }
        }

        private void Update()
        {
            if (IsPaused)
            {
                return;
            }

            if (CurrentState == State.Alert)
            {
                age += Time.deltaTime;
                float duration = Mathf.Max(0.01f, config != null ? config.maxRadius / config.spreadSpeed : 1f);
                float t = Mathf.Clamp01(age / duration);
                Radius = Mathf.Lerp(0.35f, config != null ? config.maxRadius : 4.8f, t);
                UpdateVisual();
            }
            else if (CurrentState == State.Contained)
            {
                float speed = config != null ? config.containmentSpeed : 4f;
                Radius = Mathf.MoveTowards(Radius, 0f, speed * Time.deltaTime);
                UpdateVisual();
                if (Radius <= 0.001f)
                {
                    Radius = 0f;
                    CurrentState = State.Idle;
                    if (visual != null)
                    {
                        visual.Hide();
                    }

                    HazardContained?.Invoke();
                }
            }
        }

        /// <summary>Start one alert at a random approved point. Returns false when no valid point exists.</summary>
        public bool BeginAlert()
        {
            if (CurrentState != State.Idle)
            {
                return false;
            }

            RadiationSpawnPoint point = ChooseSpawnPoint();
            if (point == null)
            {
                Debug.LogError("[MarsOutpost] Radiation drill has no valid approved spawn point.");
                return false;
            }

            ActiveSpawn = point;
            Origin = point.Position;
            Radius = 0.35f;
            age = 0f;
            CurrentState = State.Alert;
            if (visual != null)
            {
                visual.transform.position = new Vector3(Origin.x, Origin.y, 0f);
                visual.Show(Origin);
                UpdateVisual();
            }

            AlertStarted?.Invoke();
            return true;
        }

        /// <summary>Activate shielding. This contains the simulated hazard; it does not switch off radiation.</summary>
        public bool ActivateShielding()
        {
            if (CurrentState != State.Alert)
            {
                return false;
            }

            CurrentState = State.Contained;
            ShieldingActivated?.Invoke();
            return true;
        }

        public void ResetDrill()
        {
            CurrentState = State.Idle;
            Radius = 0f;
            age = 0f;
            ActiveSpawn = null;
            if (visual != null)
            {
                visual.Hide();
            }
        }

        public void SetPaused(bool paused)
        {
            IsPaused = paused;
        }

        /// <summary>0 = safe, 1 = at the centre of the warning area.</summary>
        public float GetLevelAt(Vector2 worldPosition)
        {
            if (!IsActive || Radius <= 0.001f)
            {
                return 0f;
            }

            float distance = Vector2.Distance(worldPosition, Origin);
            if (distance > Radius)
            {
                return 0f;
            }

            return Mathf.Clamp01(1f - distance / Radius);
        }

        /// <summary>Simulated 0-100 reading shown by the detector. Never a real radiation value.</summary>
        public float GetReadingAt(Vector2 worldPosition)
        {
            return GetLevelAt(worldPosition) * 100f;
        }

        public float ReadingAtOrigin => CurrentState == State.Idle ? 0f : 100f;

        private void UpdateVisual()
        {
            if (visual == null)
            {
                return;
            }

            visual.SetState(Radius, config != null ? config.maxRadius : 4.8f);
        }

        private RadiationSpawnPoint ChooseSpawnPoint()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                return null;
            }

            int start = UnityEngine.Random.Range(0, spawnPoints.Length);
            for (int offset = 0; offset < spawnPoints.Length; offset++)
            {
                RadiationSpawnPoint point = spawnPoints[(start + offset) % spawnPoints.Length];
                if (IsSpawnPointValid(point))
                {
                    return point;
                }
            }

            return null;
        }

        private bool IsSpawnPointValid(RadiationSpawnPoint point)
        {
            if (point == null)
            {
                return false;
            }

            if (walkableArea != null && !walkableArea.IsInside(point.Position, point.clearance))
            {
                return false;
            }

            if (obstacleMask.value != 0 &&
                Physics2D.OverlapCircle(point.Position, point.clearance, obstacleMask) != null)
            {
                return false;
            }

            return true;
        }
    }
}
