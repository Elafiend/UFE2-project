using UnityEngine;
using System;
using FPLibrary;

namespace UFE3D
{
    [System.Serializable]
    public class CustomHitBox : ICloneable
    {
        public string name = "New Hitbox";
        public FrameDefinition[] activeFrames = new FrameDefinition[0];

        public HitBoxShape shape;
        public CollisionType collisionType;
        public HitBoxType hitBoxType;

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }

    [System.Serializable]
    public class FrameDefinition : ICloneable
    {
        public bool active;
        public FPVector position;
        public Fix64 cubeWidth = .5;
        public Fix64 cubeHeight = .5;
        //public Fix64 cubeVolume = .5; // TODO: Change option to 'Cuboid'
        public Fix64 radius = .5;

        public int range;

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }

    [System.Serializable]
    public class CustomHitBoxesInfo : ScriptableObject
    {
        public AnimationClip clip;
        public Fix64 speed = 1;
        public GameObject preview;
        public int totalFrames = 1;

        public CustomHitBox[] customHitBoxes = new CustomHitBox[0];
    }
}