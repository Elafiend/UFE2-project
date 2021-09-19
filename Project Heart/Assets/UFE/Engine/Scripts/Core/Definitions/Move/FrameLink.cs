using UnityEngine;
using System;
using UFE3D;

[System.Serializable]
public class FrameLink : ICloneable
{
    public LinkType linkType = LinkType.NoConditions;
    public bool allowBuffer = true;
    public bool onStrike = true;
    public bool onBlock = true;
    public bool onParry = true;
    public int activeFramesBegins;
    public int activeFramesEnds;
    public CounterMoveType counterMoveType;
    public MoveInfo counterMoveFilter;
    public bool disableHitImpact = true;
    public bool anyHitStrength = true;
    public HitStrengh hitStrength;
    public bool anyStrokeHitBox = true;
    public HitBoxType hitBoxType;
    public bool anyHitType = true;
    public HitType hitType;
    public bool ignoreInputs;
    public bool ignorePlayerConditions;
    public bool ignoreGauge;
    public int nextMoveStartupFrame = 1;
    public MoveInfo[] linkableMoves = new MoveInfo[0];


    #region trackable definitions
    public bool cancelable { get; set; }
    #endregion


    [HideInInspector] public bool linkableMovesToggle;
    [HideInInspector] public bool hitConfirmToggle;
    [HideInInspector] public bool counterMoveToggle;

    public object Clone()
    {
        return CloneObject.Clone(this);
    }
}