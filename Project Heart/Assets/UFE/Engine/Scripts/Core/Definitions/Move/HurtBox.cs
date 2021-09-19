using UnityEngine;
using System;
using FPLibrary;

namespace UFE3D
{
    [Serializable]
    public class HurtBox : ICloneable
    {
        #region trackable definitions
        public BodyPart bodyPart;
        public bool followXBounds;
        public bool followYBounds;
        public int hitBoxDefinitionIndex;

        public FPVector _offSet;
        public Fix64 _radius = .5;
        public FPRect _rect = new FPRect();
        public HitBoxShape shape;
        public HurtBoxType type = HurtBoxType.physical;

        public bool isBlock { get; set; }
        public FPVector position { get; set; }
        public Rect rendererBounds { get; set; }
        #endregion

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}