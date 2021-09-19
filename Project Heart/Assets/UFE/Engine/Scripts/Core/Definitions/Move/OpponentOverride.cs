using UnityEngine;
using System;
using FPLibrary;
using UFE3D;

[System.Serializable]
public class OpponentOverride : ICloneable
{
    public FPVector _position;
    public int castingFrame;
    public int blendSpeed = 80;
    public bool stun;
    public Fix64 _stunTime;
    public bool overrideHitAnimations;
    public bool resetAppliedForces;

    // End Options
    public StandUpOptions standUpOptions;

    // Options
    public bool characterSpecific;

    // Move
    public MoveInfo move;
    public CharacterSpecificMoves[] characterSpecificMoves = new CharacterSpecificMoves[0]; // Character Specific Moves

    [HideInInspector] public bool animationPreview = false;
    [HideInInspector] public bool movesToggle = false;

    #region trackable definitions
    public bool casted { get; set; }
    #endregion

    public object Clone()
    {
        return CloneObject.Clone(this);
    }
}