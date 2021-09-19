using UnityEngine;
using System;
using FPLibrary;

namespace UFE3D
{
    [Serializable]
    public class BasicMoveInfo : ICloneable
    {
        public SerializedAnimationMap[] animMap = new SerializedAnimationMap[9];
        public Fix64 _animationSpeed = 1;
        public WrapMode wrapMode;

        public UFE3D.MoveInfo moveInfo;
        public bool useMoveFile = false;

        public bool autoSpeed = true;
        public Fix64 _restingClipInterval = 6;
        public bool overrideBlendingIn = false;
        public bool overrideBlendingOut = false;
        public Fix64 _blendingIn = 0;
        public Fix64 _blendingOut = 0;
        public bool invincible;
        public bool disableHeadLook;
        public bool applyRootMotion;
        public bool lockXMotion = false;
        public bool lockYMotion = false;
        public bool lockZMotion = false;
        public bool loopDownClip;
        public AudioClip[] soundEffects = new AudioClip[0];
        public bool continuousSound;
        public ParticleInfo particleEffect = new ParticleInfo();
        public BasicMoveReference reference;


        [HideInInspector] public string name;
        [HideInInspector] public bool editorToggle;
        [HideInInspector] public bool soundEffectsToggle;

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}