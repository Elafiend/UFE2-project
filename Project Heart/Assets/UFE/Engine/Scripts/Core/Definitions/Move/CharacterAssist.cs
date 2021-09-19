using UnityEngine;
using System;
using FPLibrary;
using UFE3D;

[System.Serializable]
public class CharacterAssist : ICloneable
{
    public FPVector _spawnPosition;
    public int castingFrame;
    public UFE3D.CharacterInfo characterInfo;
    public MoveInfo enterMove;
    public MoveInfo exitMove;

    [HideInInspector] public bool animationPreview = false;

    #region trackable definitions
    public bool casted { get; set; }
    #endregion

    public object Clone()
    {
        return CloneObject.Clone(this);
    }
}
