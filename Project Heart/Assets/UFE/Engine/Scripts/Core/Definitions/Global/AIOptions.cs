using UnityEngine;

namespace UFE3D
{
    [System.Serializable]
    public class AIOptions
    {
        public AIEngine engine;

        // Random AI Engine
        public bool attackWhenEnemyIsDown = false;
        public bool moveWhenEnemyIsDown = false;
        public float inputFrequency = .3f;
        public bool behaviourToggle;
        public AIDistanceBehaviour[] distanceBehaviour = new AIDistanceBehaviour[0];


        // Fuzzy AI Engine
        public bool multiCoreSupport = true;
        public bool persistentBehavior = false;
        public bool difficultyToggle;
        public AIDifficultySettings[] difficultySettings = new AIDifficultySettings[0];

        [HideInInspector] public AIDifficultyLevel selectedDifficultyLevel = AIDifficultyLevel.Normal;
        [HideInInspector] public AIDifficultySettings selectedDifficulty;
    }
}