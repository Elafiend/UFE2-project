using FPLibrary;

namespace UFE3D
{
    [System.Serializable]
    public class SubKnockdownOptions
    {
        public Fix64 _knockedOutTime = 2;
        public Fix64 _standUpTime = .6;
        public int hideHitBoxesOnFrame = 10;
        public bool hideHitBoxes;
        public bool editorToggle;
        public bool hasQuickStand;
        public FPVector _predefinedPushForce;
        public ButtonPress[] quickStandButtons = new ButtonPress[0]; // TODO
        public Fix64 minQuickStandTime; // TODO
        public bool hasDelayedStand;
        public ButtonPress[] delayedStandButtons = new ButtonPress[0]; // TODO
        public Fix64 maxDelayedStandTime; // TODO
    }
}