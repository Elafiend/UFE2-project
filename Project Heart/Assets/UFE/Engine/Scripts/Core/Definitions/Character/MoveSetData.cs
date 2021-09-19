using UnityEngine;
using System;
using UFE3D;

[System.Serializable]
public class MoveSetData : ICloneable
{
    public CombatStances combatStance = CombatStances.Stance1; // This move set combat stance
    public MoveInfo cinematicIntro;
    public MoveInfo cinematicOutro;

    public BasicMoves basicMoves = new BasicMoves(); // List of basic moves
    public MoveInfo[] attackMoves = new MoveInfo[0]; // List of attack moves

    [HideInInspector] public bool enabledBasicMovesToggle;
    [HideInInspector] public bool basicMovesToggle;
    [HideInInspector] public bool attackMovesToggle;


    public StanceInfo ConvertData()
    {
        StanceInfo stanceData = new StanceInfo();
        stanceData.combatStance = this.combatStance;
        stanceData.cinematicIntro = this.cinematicIntro;
        stanceData.cinematicOutro = this.cinematicOutro;
        stanceData.basicMoves = this.basicMoves;
        stanceData.attackMoves = this.attackMoves;

        return stanceData;
    }

    public object Clone()
    {
        return CloneObject.Clone(this);
    }
}