using UnityEngine;
using System;
using FPLibrary;
using UFE3D;

[System.Serializable]
public class Projectile : ICloneable
{
    public int castingFrame = 1;
    public GameObject projectilePrefab;
    public GameObject impactPrefab;

    public BodyPart bodyPart;
    public int hitBoxDefinitionIndex;

    public FPVector _castingOffSet;
    public int speed = 20;
    public int directionAngle;
    public Fix64 _duration = 5;
    public float impactDuration = 1;
    public bool fixedZAxis;
    public bool projectileCollision;
    public bool unblockable;
    public bool mirrorOn2PSide;
    public bool destroyWhenOffCameraBounds;
    public bool applyGravity;
    public bool limitMultiCasting;
    public int onScreenLimit;
    public bool limitOnlyThis;
    public FPVector forceApplied;
    public Fix64 weight;
    public Fix64 cameraBoundsOffSet;

    public HitBox hitBox;
    public HurtBox hurtBox;
    public BlockArea blockableArea;

    public Sizes spaceBetweenHits;
    public int totalHits = 1;
    public bool resetPreviousHitStun = true;
    public int hitStunOnHit;
    public int hitStunOnBlock;

    public bool overrideHitEffects;
    public bool fixRotation = true;
    public bool armorBreaker;
    public HitTypeOptions hitEffects;

    public bool groundHit;
    public bool airHit;
    public bool downHit;

    public DamageType damageType;
    public Fix64 _damageOnHit;
    public Fix64 _damageOnBlock;
    public bool damageScaling;


    public bool obeyDirectionalHit = true;
    public bool hitEffectsOnHit = true;
    public bool resetPreviousHorizontalPush;
    public bool resetPreviousVerticalPush;
    public bool applyDifferentAirForce;
    public bool applyDifferentBlockForce;
    public FPVector _pushForce;
    public FPVector _pushForceAir;
    public FPVector _pushForceBlock;
    public HitStrengh hitStrength;
    public HitType hitType;

    public MoveInfo moveLinkOnStrike;
    public MoveInfo moveLinkOnBlock;
    public MoveInfo moveLinkOnParry;
    public bool forceGrounded;

    [HideInInspector] public bool moveLinksToggle;
    [HideInInspector] public bool damageOptionsToggle;
    [HideInInspector] public bool hitStunOptionsToggle;
    [HideInInspector] public bool preview;
    [HideInInspector] public string moveName;

    #region trackable definitions
    public bool casted { get; set; }
    public Fix64 gaugeGainOnHit { get; set; }
    public Fix64 gaugeGainOnBlock { get; set; }
    public Fix64 opGaugeGainOnHit { get; set; }
    public Fix64 opGaugeGainOnBlock { get; set; }
    public Fix64 opGaugeGainOnParry { get; set; }
    public Transform position { get; set; }
    #endregion

    public object Clone()
    {
        return CloneObject.Clone(this);
    }
}