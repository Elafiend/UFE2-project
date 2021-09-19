using UnityEngine;

namespace UFE3D
{
    [System.Serializable]
    public class PlayerConditions
    {
        public BasicMoveReference[] basicMoveLimitation = new BasicMoveReference[0];
        public PossibleMoveStates[] possibleMoveStates = new PossibleMoveStates[0];

        [HideInInspector] public bool basicMovesToggle = false;
        [HideInInspector] public bool statesToggle = false;
    }
}