namespace UFE3D
{
    [System.Serializable]
    public class MoveClassification
    {
        public AttackType attackType;
        public HitType hitType;
        public FrameSpeed startupSpeed;
        public FrameSpeed recoverySpeed;
        public HitConfirmType hitConfirmType;
        public CharacterDistance preferableDistance;
        public GaugeUsage gaugeUsage;
        public bool anyAttackType = true;
        public bool anyHitType = true;
        public bool anyHitConfirmType = true;
    }
}