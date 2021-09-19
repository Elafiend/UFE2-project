using System;
using FPLibrary;

namespace UFE3D
{
    [System.Serializable]
    public class SlowMoEffect : ICloneable
    {
        public int castingFrame;
        public Fix64 _duration;
        public Fix64 _percentage;

        #region trackable definitions
        public bool casted { get; set; }
        #endregion

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}