using UnityEngine;
using FPLibrary;
using UFE3D;

[System.Serializable]
public class SerializedAnimationMap
{
    public AnimationMap[] animationMaps = new AnimationMap[0];
    //public AnimationMap[] customMaps = new AnimationMap[0];
    public AnimationClip clip;
    public CustomHitBoxesInfo customHitBoxDefinition;
    public Fix64 length;
    public bool bakeSpeed = false;
    public HitBoxDefinitionType hitBoxDefinitionType;
}