using UnityEngine;

namespace MarsOutpost.Radiation
{
    /// <summary>An approved place for the simulated alert to begin.</summary>
    public sealed class RadiationSpawnPoint : MonoBehaviour
    {
        public string id = "spawn";
        [Min(0f)] public float clearance = 0.35f;

        public Vector2 Position => transform.position;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.35f, 0.2f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, Mathf.Max(0.1f, clearance));
        }
    }
}
