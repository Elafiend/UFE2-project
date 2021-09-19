using FPLibrary;
using System;

namespace UFE3D
{
    public enum LockOnType
    {
        AlwaysLockedOn,
        LockOnButton,
        Disabled
    }

    public enum TargetSwitchType
    {
        CurrentTarget,
        NextTarget,
        NearestTarget,
        NoTarget
    }

    [System.Serializable]
    public class GlobalLockOnOptions
    {
        public LockOnType lockOnType;
        public bool lockOnButtonEnabled = false;
        public ButtonPress lockOnButton;

        public bool lockOutEnabled = false;
        public ButtonPress lockOutButton;

        public TargetSwitchType targetSwitchType;
        public ButtonPress switchButton;

        public Fix64 rotationSpeed; //TODO
    }

    [System.Serializable]
    public class MoveLockOnOptions : ICloneable
    {
        public int activeFramesBegin;
        public int activeFramesEnds;

        public TargetSwitchType targetSwitchType;
        public Fix64 rotationSpeed; //TODO

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}