[System.Serializable]
public class CharacterDebugInfo
{
    public bool toggle;
    public bool currentMove = true;
    public bool position = true;
    public bool lifePoints = true;
    public bool gaugePoints = true;
    public bool currentState;
    public bool currentSubState;
    public bool stunTime = true;
    public bool comboHits = true;
    public bool comboDamage = true;
    public bool inputs = true;
    public bool buttonSequence;
    public bool aiWeightList;

    public CharacterDebugInfo() { }

    public CharacterDebugInfo(CharacterDebugInfo other)
    {
        this.toggle = other.toggle;
        this.currentMove = other.currentMove;
        this.position = other.position;
        this.lifePoints = other.lifePoints;
        this.gaugePoints = other.gaugePoints;
        this.currentState = other.currentState;
        this.currentSubState = other.currentSubState;
        this.stunTime = other.stunTime;
        this.comboHits = other.comboHits;
        this.comboDamage = other.comboDamage;
        this.inputs = other.inputs;
        this.buttonSequence = other.buttonSequence;
        this.aiWeightList = other.aiWeightList;
    }
}