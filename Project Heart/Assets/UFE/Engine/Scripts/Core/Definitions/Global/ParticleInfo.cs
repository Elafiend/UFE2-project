using UnityEngine;
using System;

namespace UFE3D
{
    [System.Serializable]
    public class ParticleInfo : ICloneable
    {
        public bool editorToggle;
        public GameObject prefab;
        public float duration = 1;
        public bool stick = false;
        public bool destroyOnMoveOver = false;
        public bool followRotation = false;
        public bool lockLocalPosition = false;
        public bool mirrorOn2PSide = false;
        public bool overrideRotation = true;
        public Vector3 initialRotation;
        public Vector3 positionOffSet;
        public BodyPart bodyPart;

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}