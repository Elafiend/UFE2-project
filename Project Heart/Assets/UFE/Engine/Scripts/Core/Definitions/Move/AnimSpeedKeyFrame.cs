using System;
using FPLibrary;

namespace UFE3D
{
    [Serializable]
    public class AnimSpeedKeyFrame : ICloneable
    {
        public int castingFrame = 0;
        public Fix64 _speed = 1;

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}