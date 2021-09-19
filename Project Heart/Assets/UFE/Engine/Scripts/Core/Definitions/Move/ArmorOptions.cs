[System.Serializable]
public class ArmorOptions
{
    public int activeFramesBegin;
    public int activeFramesEnds;

    public bool overrideHitEffects;
    public HitTypeOptions hitEffects;

    public int hitAbsorption;
    public int damageAbsorption;
    public BodyPart[] nonAffectedBodyParts = new BodyPart[0];

    #region trackable definitions
    public int hitsTaken { get; set; }
    #endregion
}
