using UnityEngine;
using FPLibrary;

namespace UFE3D
{
    [System.Serializable]
    public class BlockOptions
    {
        public BlockType blockType;
        public bool allowAirBlock;
        public bool ignoreAppliedForceBlock;
        public bool allowMoveCancel;
        public HitTypeOptions blockHitEffects;

        public ParryType parryType;
        public Fix64 _parryTiming;
        public ParryStunType parryStunType;
        public int parryStunFrames = 10;
        public Color parryColor;
        public bool allowAirParry;
        public bool highlightWhenParry;
        public bool resetButtonSequence;
        public bool easyParry;
        public bool ignoreAppliedForceParry;
        public HitTypeOptions parryHitEffects;

        public Sizes blockPushForce; // TODO
        public ButtonPress[] pushBlockButtons; // TODO
    }
}