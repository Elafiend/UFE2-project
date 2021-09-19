using UnityEngine;
using System;
using FPLibrary;
using UFE3D;
using System.Collections.Generic;

[System.Serializable]
public class Hit : ICloneable
{
    public int activeFramesBegin;
    public int activeFramesEnds;
    public HitConfirmType hitConfirmType;
    public MoveInfo throwMove;
    public MoveInfo techMove;
    public bool techable = true;
    public bool resetHitAnimations = true;
    public bool forceStand;
    public bool fixRotation;
    public bool armorBreaker;
    public bool continuousHit;
    public bool unblockable;
    public Sizes spaceBetweenHits;
    public bool groundHit = true;
    public bool crouchingHit = true;
    public bool airHit = true;
    public bool stunHit = true;
    public bool allowMultiHit = true;
    public PlayerConditions opponentConditions = new PlayerConditions();

    public bool downHit;
    public bool resetPreviousHitStun;
    public bool resetCrumples;
    public bool customStunValues;

    public bool overrideHitEffects;
    public HitTypeOptions hitEffects;
    public bool overrideHitEffectsBlock;
    public HitTypeOptions hitEffectsBlock;
    public bool overrideEffectSpawnPoint;
    public HitEffectSpawnPoint spawnPoint = HitEffectSpawnPoint.StrokeHitBox;
    public bool overrideHitAnimationBlend;
    public Fix64 _newHitBlendingIn;
    public bool overrideJuggleWeight;
    public Fix64 _newJuggleWeight;
    public bool overrideAirRecoveryType;
    public AirRecoveryType newAirRecoveryType = AirRecoveryType.AllowMoves;
    public bool instantAirRecovery;
    public bool overrideHitAnimation;
    public BasicMoveReference newHitAnimation = BasicMoveReference.HitKnockBack;

    public HitStrengh hitStrength;
    public HitStunType hitStunType = HitStunType.Frames;
    public Fix64 _hitStunOnHit;
    public Fix64 _hitStunOnBlock;
    public int frameAdvantageOnHit;
    public int frameAdvantageOnBlock;
    public bool damageScaling;
    public DamageType damageType;
    public Fix64 _damageOnHit;
    public Fix64 _damageOnBlock;
    public bool doesntKill;
    public HitType hitType;

    public bool resetPreviousHorizontalPush;
    public bool resetPreviousVerticalPush;
    public bool resetPreviousSidewaysPush;
    public bool resetJumpForce;
    public bool applyDifferentAirForce;
    public bool applyDifferentBlockForce;
    public bool applyDifferentSelfAirForce;
    public bool applyDifferentSelfBlockForce;
    public FPVector _pushForce;
    public FPVector _pushForceAir;
    public FPVector _pushForceBlock;
    public bool resetPreviousHorizontal;
    public bool resetPreviousVertical;
    public bool resetPreviousSideways;
    public FPVector _appliedForce;
    public FPVector _appliedForceAir;
    public FPVector _appliedForceBlock;

    public bool cornerPush = true;

    public bool groundBounce = true;
    public bool overrideForcesOnGroundBounce;
    public bool resetGroundBounceHorizontalPush;
    public bool resetGroundBounceVerticalPush;
    public FPVector _groundBouncePushForce;

    public bool wallBounce;
    public bool knockOutOnWallBounce;
    public bool overrideForcesOnWallBounce;
    public bool resetWallBounceHorizontalPush;
    public bool resetWallBounceVerticalPush;
    public FPVector _wallBouncePushForce;
    public bool bounceOnCameraEdge;
    public Fix64 cameraEdgeOffSet = 0;
    public bool overrideCameraSpeed;
    public Fix64 _newMovementSpeed;
    public Fix64 _newRotationSpeed;
    public Fix64 _cameraSpeedDuration;

    public PullIn pullEnemyIn;
    public PullIn pullSelfIn;

    [HideInInspector] public bool hitToggle = true;
    [HideInInspector] public bool damageOptionsToggle;
    [HideInInspector] public bool hitStunOptionsToggle;
    [HideInInspector] public bool forceOptionsToggle;
    [HideInInspector] public bool opponentForceToggle;
    [HideInInspector] public bool selfForceToggle;
    [HideInInspector] public bool stageReactionsToggle;
    [HideInInspector] public bool overrideEventsToggle;
    [HideInInspector] public bool hitConditionsToggle;
    [HideInInspector] public bool pullInToggle;
    [HideInInspector] public bool hurtBoxesToggle;
    [HideInInspector] public bool wallBounceToggle;
    [HideInInspector] public bool groundBounceToggle;

    #region trackable definitions
    public HurtBox[] hurtBoxes = new HurtBox[0];
    public List<ControlsScript> impactList { get; set; }
    #endregion

    public object Clone()
    {
        return CloneObject.Clone(this);
    }
}