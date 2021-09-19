using UnityEngine;
using System;

namespace UFE3D
{
    [System.Serializable]
    public class InvincibleBodyParts : ICloneable
    {
        public BodyPart[] bodyParts = new BodyPart[0];
        public bool completelyInvincible = true;
        public bool ignoreBodyColliders = false;
        public int activeFramesBegin;
        public int activeFramesEnds;

        [HideInInspector] public HitBox[] hitBoxes;

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}