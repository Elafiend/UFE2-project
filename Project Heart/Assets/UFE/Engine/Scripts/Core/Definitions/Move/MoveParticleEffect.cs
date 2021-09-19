using System;

namespace UFE3D
{
    [Serializable]
    public class MoveParticleEffect : ICloneable
    {
        public int castingFrame;
        public ParticleInfo particleEffect;

        #region trackable definitions
        public bool casted { get; set; }
        #endregion

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}