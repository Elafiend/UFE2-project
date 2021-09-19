using UnityEngine;
using System;
using FPLibrary;
using UFE3D;

[System.Serializable]
public class GaugeInfo : ICloneable
{
    public int castingFrame;
    public GaugeId targetGauge;
    public bool startDrainingGauge;
    public bool inhibitGainWhileDraining;
    public bool stopDrainingGauge;
    public Fix64 _gaugeDPS;
    public Fix64 _totalDrain;
    public Fix64 _gaugeRequired;
    public Fix64 _gaugeUsage;
    public Fix64 _gaugeGainOnMiss;
    public Fix64 _gaugeGainOnHit;
    public Fix64 _gaugeGainOnBlock;
    public Fix64 _opGaugeGainOnBlock;
    public Fix64 _opGaugeGainOnParry;
    public Fix64 _opGaugeGainOnHit;
    public MoveInfo DCMove;
    public CombatStances DCStance;

    [HideInInspector] public bool editorToggle = false;

    #region trackable definitions
    public bool casted { get; set; }
    #endregion

    public object Clone()
    {
        return CloneObject.Clone(this);
    }
}