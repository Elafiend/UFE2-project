using FPLibrary;

namespace UFE3D
{
    [System.Serializable]
    public class ComboOptions
    {
        public ComboDisplayMode comboDisplayMode;
        public Sizes hitStunDeterioration;
        public Sizes damageDeterioration;
        public Sizes airJuggleDeterioration;
        public int _minHitStun;
        public Fix64 _minDamage;
        public Fix64 _minPushForce;
        public int maxConsecutiveCrumple = 1;
        public AirJuggleDeteriorationType airJuggleDeteriorationType;
        public bool neverAirRecover = false;
        public AirRecoveryType airRecoveryType = AirRecoveryType.CantMove;
        public bool resetFallingForceOnHit = true;
        public int maxCombo = 99;
        public Fix64 _knockBackMinForce;
        public bool neverCornerPush;
        public bool fixJuggleWeight = true;
        public Fix64 _juggleWeight;
    }
}