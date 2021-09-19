using UnityEngine;
using System;
using FPLibrary;
using UFE3D;

[System.Serializable]
public class StateOverride : ICloneable
{
    public int castingFrame;
    public int endFrame;
    public PossibleStates state;

    public object Clone()
    {
        return CloneObject.Clone(this);
    }
}