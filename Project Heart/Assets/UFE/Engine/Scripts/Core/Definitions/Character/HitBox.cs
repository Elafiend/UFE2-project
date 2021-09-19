using UnityEngine;
using System;
using FPLibrary;

namespace UFE3D
{
    [Serializable]
    public class HitBox : ICloneable
    {
        public Rect rect = new Rect(0, 0, 4, 4); // TODO remove this variable

        public bool defaultVisibility = true;
        public bool followXBounds;
        public bool followYBounds;

        public Transform position;

        #region trackable definitions
        public int state { get; set; }
        //public Rect rendererBounds { get; set; }
        public bool hide { get; set; }          // Whether the hit box collisions will be detected
        public bool visibility { get; set; }    // Whether the GameObject will be active in the hierarchy

        public BodyPart bodyPart;
        public FPVector mappedPosition;
        public FPVector localPosition;
        public HitBoxShape shape;
        public CollisionType collisionType;
        public HitBoxType type;
        public FPRect _rect = new FPRect();
        public Fix64 _radius = .5;
        public FPVector _offSet;
        #endregion


        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}