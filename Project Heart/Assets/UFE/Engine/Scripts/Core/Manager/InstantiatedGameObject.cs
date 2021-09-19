using UnityEngine;
using UFENetcode;

namespace UFE3D
{
    public class InstantiatedGameObject
    {
        public GameObject gameObject;
        public MrFusion mrFusion;
        public string id;
        public long creationFrame;
        public long? destructionFrame;

        public bool destroyMe;

        public InstantiatedGameObject(
            string id = null,
            GameObject gameObject = null,
            MrFusion mrFusion = null,
            long creationFrame = 0,
            long? destructionFrame = null
        )
        {
            this.gameObject = gameObject;
            this.id = id;
            this.mrFusion = mrFusion;
            this.creationFrame = creationFrame;
            this.destructionFrame = destructionFrame != null ? new long?(destructionFrame.Value) : null;
        }

        public InstantiatedGameObject(InstantiatedGameObject other) : this(
            other.id,
            other.gameObject,
            other.mrFusion,
            other.creationFrame,
            other.destructionFrame
        )
        { }

        public bool IsDestroyed()
        {
            if (this == null) return true;
            return destroyMe;
        }
    }
}