using UnityEngine;
using FPLibrary;

namespace UFE3D
{
    [System.Serializable]
    public class MoveInputs
    {
        public bool chargeMove;
        public Fix64 _chargeTiming = .7;
        public bool allowInputLeniency;
        public bool allowNegativeEdge = false;
        public bool forceAxisPrecision = false;
        public int leniencyBuffer = 3;
        public bool onReleaseExecution;
        public bool requireButtonPress = true;
        public bool onPressExecution = true;
        public ButtonPress[] buttonSequence = new ButtonPress[0];
        public ButtonPress[] buttonExecution = new ButtonPress[0];

        [HideInInspector] public bool editorToggle = false;
        [HideInInspector] public bool buttonSequenceToggle = false;
        [HideInInspector] public bool buttonExecutionToggle = false;
    }
}