using System;

namespace UFE3D
{
    [System.Serializable]
    public class PossibleMoveStates : ICloneable
    {
        public PossibleStates possibleState;
        public JumpArc jumpArc;
        public int jumpArcBegins = 0;
        public int jumpArcEnds = 100;

        public CharacterDistance opponentDistance;
        public int proximityRangeBegins = 0;
        public int proximityRangeEnds = 100;

        public bool movingForward = true;
        public bool movingBack = true;

        public bool standBy = true;
        public bool blocking;
        public bool stunned;

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}