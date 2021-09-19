using System;

namespace UFE3D
{
    [Serializable]
    public class InGameAlert : ICloneable
    {
        public int castingFrame;
        public string alert;

        #region trackable definitions
        public bool casted { get; set; }
        #endregion

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}