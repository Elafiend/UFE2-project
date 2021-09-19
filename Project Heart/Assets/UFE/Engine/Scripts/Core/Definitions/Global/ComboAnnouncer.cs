using System;
using UnityEngine;

namespace UFE3D
{
    [System.Serializable]
    public class ComboAnnouncer : ICloneable
    {
        public AudioClip audio;
        public int hits;

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}