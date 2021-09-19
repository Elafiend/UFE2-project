using UnityEngine;
using System;
using FPLibrary;

namespace UFE3D
{
    [System.Serializable]
    public class PullIn : ICloneable
    {
        public int speed = 50;
        public bool forceGrounded = false;
        public FPVector position;

        public Fix64 _targetDistance { get; set; }

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}