using UnityEngine;
using FPLibrary;

[System.Serializable]
public class BlockArea
{
    public bool enabled = true;
    public int activeFramesBegin;
    public int activeFramesEnds;

    public BodyPart bodyPart;
    public int hitBoxDefinitionIndex;

    public HitBoxShape shape;
    public Rect rect = new Rect(0, 0, 4, 4);
    public FPRect _rect = new FPRect();
    public bool followXBounds;
    public bool followYBounds;
    public Fix64 _radius = .5;
    public FPVector _offSet;

    public FPVector cuboidMin;
    public FPVector cuboidMax;

    [HideInInspector] public FPVector position;
}