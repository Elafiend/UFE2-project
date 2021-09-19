using System;

namespace UFE3D
{
    [System.Serializable]
    public class BodyPartVisibilityChange : ICloneable
    {
        public int castingFrame;
        public BodyPart bodyPart;
        public bool visible;
        public bool left;
        public bool right;

        public bool casted { get; set; }

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}