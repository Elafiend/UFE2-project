using System;

namespace UFE3D
{
    [System.Serializable]
    public class AIDistanceBehaviour : ICloneable
    {
        public CharacterDistance characterDistance;
        public int proximityRangeBegins = 0;
        public int proximityRangeEnds = 100;

        public float movingForwardProbability = .5f;
        public float movingBackProbability = .5f;
        public float jumpingProbability = .5f;
        public float crouchProbability = .5f;
        public float attackProbability = .5f;

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}