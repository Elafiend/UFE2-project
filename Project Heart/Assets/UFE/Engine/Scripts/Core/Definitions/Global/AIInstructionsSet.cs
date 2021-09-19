using UnityEngine;
using System;

namespace UFE3D
{
    [System.Serializable]
    public class AIInstructionsSet : ICloneable
    {
        public ScriptableObject aiInfo;
        public AIBehavior behavior;

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}