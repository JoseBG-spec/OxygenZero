using System;
using UnityEngine;

namespace MarsOutpost.Core
{
    /// <summary>One authored resource change. Values are game balance only.</summary>
    [Serializable]
    public struct ResourceChange
    {
        public string resourceId;
        public int delta;
        [TextArea(1, 2)] public string reason;

        public ResourceChange(string resourceId, int delta, string reason)
        {
            this.resourceId = resourceId;
            this.delta = delta;
            this.reason = reason;
        }
    }
}
