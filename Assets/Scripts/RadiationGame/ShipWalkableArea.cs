using UnityEngine;

namespace MarsOutpost.Radiation
{
    /// <summary>
    /// A hand-authored set of walkable rectangles for the illustrated ship. It keeps the
    /// player inside rooms and corridors without needing pixel-perfect wall colliders.
    /// </summary>
    public sealed class ShipWalkableArea : MonoBehaviour
    {
        [SerializeField] private Rect[] zones = System.Array.Empty<Rect>();

        public Rect[] Zones => zones;

        public void SetZones(Rect[] newZones)
        {
            zones = newZones ?? System.Array.Empty<Rect>();
        }

        public bool IsInside(Vector2 point, float radius)
        {
            if (zones == null || zones.Length == 0)
            {
                return false;
            }

            float safeRadius = Mathf.Max(0f, radius);
            for (int i = 0; i < zones.Length; i++)
            {
                Rect zone = zones[i];
                if (point.x - safeRadius >= zone.xMin &&
                    point.x + safeRadius <= zone.xMax &&
                    point.y - safeRadius >= zone.yMin &&
                    point.y + safeRadius <= zone.yMax)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Try the full move, then slide on one axis, then stay put.</summary>
        public Vector2 Resolve(Vector2 current, Vector2 target, float radius)
        {
            if (IsInside(target, radius))
            {
                return target;
            }

            var horizontal = new Vector2(target.x, current.y);
            if (IsInside(horizontal, radius))
            {
                return horizontal;
            }

            var vertical = new Vector2(current.x, target.y);
            if (IsInside(vertical, radius))
            {
                return vertical;
            }

            return current;
        }

        private void OnDrawGizmosSelected()
        {
            if (zones == null)
            {
                return;
            }

            Gizmos.color = new Color(0.2f, 0.9f, 0.6f, 0.35f);
            foreach (Rect zone in zones)
            {
                Gizmos.DrawWireCube(new Vector3(zone.center.x, zone.center.y, 0f), new Vector3(zone.width, zone.height, 0f));
            }
        }
    }
}
