using UnityEngine;
using System;
using FPLibrary;

namespace UFE3D
{
    [System.Serializable]
    public class CameraMovement : ICloneable
    {
        public CinematicType cinematicType = CinematicType.CameraEditor;
        public AnimationClip animationClip;
        public GameObject prefab;
        public float camAnimationSpeed = 1;
        public float blendSpeed = 100;
        public Vector3 gameObjectPosition;
        public Vector3 position;
        public Vector3 rotation;
        public int castingFrame;
        public Fix64 _duration;
        public float fieldOfView;
        public float camSpeed = 2;
        public bool freezePhysics;
        public Fix64 _myAnimationSpeed = 100;
        public Fix64 _opAnimationSpeed = 100;
        public bool previewToggle;

        #region trackable definitions
        public bool casted { get; set; }
        public bool over { get; set; }
        public Fix64 time { get; set; }
        #endregion

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}