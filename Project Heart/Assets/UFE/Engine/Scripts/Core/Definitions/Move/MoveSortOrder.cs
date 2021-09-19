using UnityEngine;
using System;
using FPLibrary;
using UFE3D;

[System.Serializable]
public class MoveSortOrder : ICloneable
{
    public int castingFrame;
    public int value;

    [HideInInspector] public bool editorToggle = false;

    public object Clone()
    {
        return CloneObject.Clone(this);
    }
}