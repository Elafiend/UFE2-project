using UnityEngine;
using System;
using FPLibrary;

namespace UFE3D
{
    [System.Serializable]
    public class StageOptions : ICloneable
    {
        public string stageName;
        public StorageMode stageLoadingMethod;
        public string stagePath;
        public Texture2D screenshot;
        public GameObject prefab;
        public AudioClip music;
        public Fix64 _groundFriction = 100;
        public Fix64 _leftBoundary = -38;
        public Fix64 _rightBoundary = 38;
        public FPVector position;

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}