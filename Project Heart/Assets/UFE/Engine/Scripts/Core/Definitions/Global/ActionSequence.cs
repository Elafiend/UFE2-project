using UFE3D;

[System.Serializable]
public class ActionSequence
{
    public ActionType actionType;
    public MoveInfo specialMove;
    public BasicMoveReference basicMove;
    public ButtonPress button;
    public bool onlyAllowThisButton;
}