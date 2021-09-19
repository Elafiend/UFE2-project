using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using FPLibrary;
using UFE3D;

public class MoveSetScript : MonoBehaviour {
	public BasicMoves basicMoves;
	public MoveInfo[] attackMoves;
	public MoveInfo[] moves;
	public MoveInfo intro;
	public MoveInfo outro;

    #region trackable definitions
    public MecanimControl MecanimControl { get { return this.mecanimControl; } set { mecanimControl = value; } }
    public LegacyControl LegacyControl { get { return this.legacyControl; } set { legacyControl = value; } }
    public SpriteRenderer SpriteRenderer { get { return this.spriteRenderer; } set { spriteRenderer = value; } }
    public int totalAirMoves;
    public bool animationPaused;
    public Fix64 overrideNextBlendingValue = -1;
    public Fix64 lastTimePress;
    public List<ButtonSequenceRecord> lastButtonPresses = new List<ButtonSequenceRecord>();
    public Dictionary<string, long> lastMovesPlayed = new Dictionary<string, long>();
    #endregion


    public ControlsScript controlsScript;
    public HitBoxesScript hitBoxesScript;
    private MecanimControl mecanimControl;
    private LegacyControl legacyControl;
    private SpriteRenderer spriteRenderer;
    private List<BasicMoveInfo> basicMoveList = new List<BasicMoveInfo>();

    void Awake()
    {
		controlsScript = transform.parent.gameObject.GetComponent<ControlsScript>();
		hitBoxesScript = GetComponent<HitBoxesScript>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        List<MoveSetData> loadedMoveSets = new List<MoveSetData>();
        foreach (MoveSetData moveSetData in controlsScript.myInfo.moves)
        {
            loadedMoveSets.Add(moveSetData);
        }
        foreach (string path in controlsScript.myInfo.stanceResourcePath)
        {
            loadedMoveSets.Add(Resources.Load<StanceInfo>(path).ConvertData());
        }
        controlsScript.loadedMoves = loadedMoveSets.ToArray();

        controlsScript.currentCombatStance = CombatStances.Stance10;
		ChangeMoveStances(CombatStances.Stance1);
	}


    public void ChangeMoveStances(CombatStances newStance)
    {
        if (controlsScript.currentCombatStance == newStance) return;
        foreach (MoveSetData moveSetData in controlsScript.loadedMoves) {
            if (moveSetData.combatStance == newStance) {
                string currentClip = basicMoves != null ? GetCurrentClipName() : null;
                Fix64 currentNormalizedTime = basicMoves != null ? GetCurrentClipPosition() : 0;
                Fix64 currentSpeed = 0;

                if (controlsScript.myInfo.animationType == AnimationType.Legacy && legacyControl != null) {
                    currentSpeed = legacyControl.globalSpeed;
                }
                string currentState = null;
                bool currentMirror = false;
                MecanimAnimationData currentMecanimData = new MecanimAnimationData();
                AnimatorOverrideController overrideController = new AnimatorOverrideController();

                if ((controlsScript.myInfo.animationType == AnimationType.Mecanim3D || controlsScript.myInfo.animationType == AnimationType.Mecanim2D) && mecanimControl != null) {
                    currentState = mecanimControl.currentState;
                    currentMirror = mecanimControl.currentMirror;
                    currentSpeed = mecanimControl.currentSpeed;
                    overrideController = mecanimControl.overrideController;

                    mecanimControl.CopyAnimationData(mecanimControl.currentAnimationData, ref currentMecanimData);
                }
                
                basicMoves = moveSetData.basicMoves;
                attackMoves = moveSetData.attackMoves;
                moves = attackMoves;

                foreach (MoveInfo move1 in moves) {
                    if (move1.defaultInputs.chargeMove && move1.defaultInputs._chargeTiming <= controlsScript.myInfo._executionTiming) {
                        Debug.LogWarning("Warning: " + move1.name + " (" + move1.moveName + ") charge timing must be higher then the character's execution timing.");
                    }

                    foreach (MoveInfo move2 in moves) {
                        if (move2 == null) Debug.LogError("Error: You have an empty move field under " + controlsScript.myInfo.characterName + "'s move set");
                        if (move1.name != move2.name && move1.moveName == move2.moveName) {
                            Debug.LogWarning("Warning: " + move1.name + " (" + move1.moveName + ") has the same name as " + move2.name + " (" + move2.moveName + ")");
                        }
                    }
                }

                // Reset Animation Components
                fillMoves();

                if (moveSetData.cinematicIntro != null) {
                    intro = Instantiate(moveSetData.cinematicIntro) as MoveInfo;
                    intro.name = "Intro";
                    attachAnimation(intro.animMap.clip, intro.name, intro._animationSpeed, intro.wrapMode, intro.animMap.length);
                }
                if (moveSetData.cinematicOutro != null) {
                    outro = Instantiate(moveSetData.cinematicOutro) as MoveInfo;
                    outro.name = "Outro";
                    attachAnimation(outro.animMap.clip, outro.name, outro._animationSpeed, outro.wrapMode, outro.animMap.length);
                }

                controlsScript.currentCombatStance = newStance;

                System.Array.Sort(moves, delegate(MoveInfo move1, MoveInfo move2) {
                    return move1.defaultInputs.buttonExecution.Length.CompareTo(move2.defaultInputs.buttonExecution.Length);
                });

                System.Array.Sort(moves, delegate(MoveInfo move1, MoveInfo move2) {
                    if (move1.defaultInputs.buttonExecution.Length > 1 && move1.defaultInputs.buttonExecution.Contains(ButtonPress.Back)) return 0;
                    if (move1.defaultInputs.buttonExecution.Length > 1 && move1.defaultInputs.buttonExecution.Contains(ButtonPress.Forward)) return 0;
                    if (move1.defaultInputs.buttonExecution.Length > 1) return 1;
                    return 0;
                });

                System.Array.Sort(moves, delegate(MoveInfo move1, MoveInfo move2) {
                    return move1.selfConditions.basicMoveLimitation.Length.CompareTo(move2.selfConditions.basicMoveLimitation.Length);
                });

                System.Array.Sort(moves, delegate(MoveInfo move1, MoveInfo move2) {
                    return move1.opponentConditions.basicMoveLimitation.Length.CompareTo(move2.opponentConditions.basicMoveLimitation.Length);
                });

                System.Array.Sort(moves, delegate(MoveInfo move1, MoveInfo move2) {
                    return move1.opponentConditions.possibleMoveStates.Length.CompareTo(move2.opponentConditions.possibleMoveStates.Length);
                });

                System.Array.Sort(moves, delegate(MoveInfo move1, MoveInfo move2) {
                    return move1.previousMoves.Length.CompareTo(move2.previousMoves.Length);
                });

                System.Array.Sort(moves, delegate(MoveInfo move1, MoveInfo move2) {
                    return move1.defaultInputs.buttonSequence.Length.CompareTo(move2.defaultInputs.buttonSequence.Length);
                });

                System.Array.Reverse(moves);

                if (currentClip != null) {
                    // Restore animation
                    if (controlsScript.myInfo.animationType == AnimationType.Mecanim3D || controlsScript.myInfo.animationType == AnimationType.Mecanim2D) {
                        mecanimControl.currentState = currentState;
                        mecanimControl.currentMirror = currentMirror;
                        mecanimControl.currentSpeed = currentSpeed;
                        mecanimControl.overrideController = overrideController;

                        mecanimControl.currentAnimationData = new MecanimAnimationData();
                        mecanimControl.CopyAnimationData(currentMecanimData, ref mecanimControl.currentAnimationData);

                        mecanimControl.animator.runtimeAnimatorController = overrideController;
                        mecanimControl.animator.Play(currentState, 0, (float)currentNormalizedTime);
                        mecanimControl.animator.applyRootMotion = currentMecanimData.applyRootMotion;
                        mecanimControl.animator.Update(0);
                        mecanimControl.SetSpeed(currentSpeed);

                    } else {
                        legacyControl.globalSpeed = currentSpeed;
                        PlayAnimation(currentClip, 0, currentNormalizedTime);
                    }

                } else {
                    PlayBasicMove(basicMoves.idle);
                    controlsScript.currentState = PossibleStates.Stand;
                    controlsScript.currentSubState = SubStates.Resting;
                }
                return;
            }
        }
    }

    private void fillMoves()
    {
        DestroyImmediate(gameObject.GetComponent(typeof(MecanimControl)));
        DestroyImmediate(gameObject.GetComponent(typeof(LegacyControl)));
		DestroyImmediate(gameObject.GetComponent(typeof(Animation)));
		DestroyImmediate(gameObject.GetComponent(typeof(Animator)));
		DestroyImmediate(gameObject.GetComponent("MecanimControl"));

        if ((UFE.isConnected || UFE.config.debugOptions.emulateNetwork)
            && UFE.config.networkOptions.forceAnimationControl) {
            controlsScript.myInfo.animationFlow = AnimationFlow.UFEEngine;
        }

		if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
			gameObject.AddComponent(typeof(Animation));
            gameObject.GetComponent<Animation>().clip = basicMoves.idle.animMap[0].clip;
            gameObject.GetComponent<Animation>().wrapMode = WrapMode.Once;

            legacyControl = gameObject.AddComponent<LegacyControl>();
            if (controlsScript.myInfo.animationFlow == AnimationFlow.UFEEngine) legacyControl.overrideAnimatorUpdate = true;

		}
        else
        {
            Animator animator = (Animator) gameObject.AddComponent(typeof(Animator));
			animator.avatar = controlsScript.myInfo.avatar;
			//animator.applyRootMotion = true;

            //mecanimControl = gameObject.AddComponent<MC3>();
            mecanimControl = gameObject.AddComponent<MecanimControl>();

            mecanimControl.ApplyBuiltinRootMotion();
            mecanimControl.defaultTransitionDuration = controlsScript.myInfo._blendingTime;
			mecanimControl.SetDefaultClip(basicMoves.idle.animMap[0].clip, "default", basicMoves.idle._animationSpeed, WrapMode.Loop, 
			                              (controlsScript.mirror > 0 && UFE.config.characterRotationOptions.autoMirror));

            mecanimControl.defaultWrapMode = WrapMode.Once;
            if (controlsScript.myInfo.animationFlow == AnimationFlow.UFEEngine) mecanimControl.overrideAnimatorUpdate = true;
            mecanimControl.normalizeFrames = controlsScript.myInfo.normalizeAnimationFrames;
		}


        foreach (MoveInfo move in moves) {
            if (move == null) {
                Debug.LogWarning("You have empty entries in your move list. Check your special moves under Character Editor.");
                continue;
            }
			if (move.animMap.clip != null) {
                attachAnimation(move.animMap.clip, move.name, move._animationSpeed, move.wrapMode, move.animMap.length);
			}
		}

        setBasicMoveAnimation(basicMoves.idle, "idle", BasicMoveReference.Idle);
		setBasicMoveAnimation(basicMoves.moveForward, "moveForward", BasicMoveReference.MoveForward);
		setBasicMoveAnimation(basicMoves.moveBack, "moveBack", BasicMoveReference.MoveBack);
		setBasicMoveAnimation(basicMoves.moveSideways, "moveSideways", BasicMoveReference.MoveSideways);
        setBasicMoveAnimation(basicMoves.crouching, "crouching", BasicMoveReference.Crouching);
        setBasicMoveAnimation(basicMoves.takeOff, "takeOff", BasicMoveReference.TakeOff);
		setBasicMoveAnimation(basicMoves.jumpStraight, "jumpStraight", BasicMoveReference.JumpStraight);
		setBasicMoveAnimation(basicMoves.jumpBack, "jumpBack", BasicMoveReference.JumpBack);
		setBasicMoveAnimation(basicMoves.jumpForward, "jumpForward", BasicMoveReference.JumpForward);
		setBasicMoveAnimation(basicMoves.fallStraight, "fallStraight", BasicMoveReference.FallStraight);
		setBasicMoveAnimation(basicMoves.fallBack, "fallBack", BasicMoveReference.FallBack);
		setBasicMoveAnimation(basicMoves.fallForward, "fallForward", BasicMoveReference.FallForward);
		setBasicMoveAnimation(basicMoves.landing, "landing", BasicMoveReference.Landing);

        setBasicMoveAnimation(basicMoves.blockingCrouchingPose, "blockingCrouchingPose", BasicMoveReference.BlockingCrouchingPose);
        setBasicMoveAnimation(basicMoves.blockingCrouchingHit, "blockingCrouchingHit", BasicMoveReference.BlockingCrouchingHit);
        setBasicMoveAnimation(basicMoves.blockingHighPose, "blockingHighPose", BasicMoveReference.BlockingHighPose);
        setBasicMoveAnimation(basicMoves.blockingHighHit, "blockingHighHit", BasicMoveReference.BlockingHighHit);
        setBasicMoveAnimation(basicMoves.blockingLowHit, "blockingLowHit", BasicMoveReference.BlockingLowHit);
        setBasicMoveAnimation(basicMoves.blockingAirPose, "blockingAirPose", BasicMoveReference.BlockingAirPose);
        setBasicMoveAnimation(basicMoves.blockingAirHit, "blockingAirHit", BasicMoveReference.BlockingAirHit);
        setBasicMoveAnimation(basicMoves.parryCrouching, "parryCrouching", BasicMoveReference.ParryCrouching);
		setBasicMoveAnimation(basicMoves.parryHigh, "parryHigh", BasicMoveReference.ParryHigh);
		setBasicMoveAnimation(basicMoves.parryLow, "parryLow", BasicMoveReference.ParryLow);
		setBasicMoveAnimation(basicMoves.parryAir, "parryAir", BasicMoveReference.ParryAir);

        setBasicMoveAnimation(basicMoves.getHitHigh, "getHitHigh", BasicMoveReference.HitStandingHigh);
        setBasicMoveAnimation(basicMoves.getHitLow, "getHitLow", BasicMoveReference.HitStandingLow);
        setBasicMoveAnimation(basicMoves.getHitCrouching, "getHitCrouching", BasicMoveReference.HitStandingCrouching);
        setBasicMoveAnimation(basicMoves.getHitAir, "getHitAir", BasicMoveReference.HitAirJuggle);
        setBasicMoveAnimation(basicMoves.getHitKnockBack, "getHitKnockBack", BasicMoveReference.HitKnockBack);
		setBasicMoveAnimation(basicMoves.getHitHighKnockdown, "getHitHighKnockdown", BasicMoveReference.HitStandingHighKnockdown);
		setBasicMoveAnimation(basicMoves.getHitMidKnockdown, "getHitMidKnockdown", BasicMoveReference.HitStandingMidKnockdown);
		setBasicMoveAnimation(basicMoves.getHitSweep, "getHitSweep", BasicMoveReference.HitSweep);
		setBasicMoveAnimation(basicMoves.getHitCrumple, "getHitCrumple", BasicMoveReference.HitCrumple);

        setBasicMoveAnimation(basicMoves.groundBounce, "groundBounce", BasicMoveReference.StageGroundBounce);
        setBasicMoveAnimation(basicMoves.standingWallBounce, "standingWallBounce", BasicMoveReference.StageStandingWallBounce);
        setBasicMoveAnimation(basicMoves.standingWallBounceKnockdown, "standingWallBounceKnockdown", BasicMoveReference.StageStandingWallBounceKnockdown);
        setBasicMoveAnimation(basicMoves.airWallBounce, "airWallBounce", BasicMoveReference.StageAirWallBounce);

        setBasicMoveAnimation(basicMoves.fallDown, "fallDown", BasicMoveReference.FallDownDefault);
        setBasicMoveAnimation(basicMoves.fallingFromAirHit, "fallingFromAirHit", BasicMoveReference.FallDownFromAirJuggle);
        setBasicMoveAnimation(basicMoves.fallingFromGroundBounce, "fallingFromBounce", BasicMoveReference.FallDownFromGroundBounce);
        setBasicMoveAnimation(basicMoves.airRecovery, "airRecovery", BasicMoveReference.AirRecovery);

		setBasicMoveAnimation(basicMoves.standUp, "standUp", BasicMoveReference.StandUpDefault);
        setBasicMoveAnimation(basicMoves.standUpFromAirHit, "standUpFromAirHit", BasicMoveReference.StandUpFromAirJuggle);
        setBasicMoveAnimation(basicMoves.standUpFromKnockBack, "standUpFromKnockBack", BasicMoveReference.StandUpFromKnockBack);
        setBasicMoveAnimation(basicMoves.standUpFromStandingHighHit, "standUpFromStandingHighHit", BasicMoveReference.StandUpFromStandingHighHit);
        setBasicMoveAnimation(basicMoves.standUpFromStandingMidHit, "standUpFromStandingMidHit", BasicMoveReference.StandUpFromStandingMidHit);
        setBasicMoveAnimation(basicMoves.standUpFromSweep, "standUpFromSweep", BasicMoveReference.StandUpFromSweep);
        setBasicMoveAnimation(basicMoves.standUpFromCrumple, "standUpFromCrumple", BasicMoveReference.StandUpFromCrumple);
        setBasicMoveAnimation(basicMoves.standUpFromStandingWallBounce, "standUpFromStandingWallBounce", BasicMoveReference.StandUpFromStandingWallBounce);
        setBasicMoveAnimation(basicMoves.standUpFromAirWallBounce, "standUpFromAirWallBounce", BasicMoveReference.StandUpFromAirWallBounce);
        setBasicMoveAnimation(basicMoves.standUpFromGroundBounce, "standUpFromGroundBounce", BasicMoveReference.StandUpFromGroundBounce);
	}
	
	private void setBasicMoveAnimation(BasicMoveInfo basicMove, string animName, BasicMoveReference basicMoveReference)
    {
		if (basicMove.animMap[0].clip == null) {
			return;
		}
		basicMove.name = animName;
		basicMove.reference = basicMoveReference;

        basicMoveList.Add(basicMove);

        attachAnimation(basicMove.animMap[0].clip, animName, basicMove._animationSpeed, basicMove.wrapMode, basicMove.animMap[0].length);
        WrapMode newWrapMode = basicMove.wrapMode;
        if (basicMoveReference == BasicMoveReference.Idle) {
            newWrapMode = WrapMode.Once;
        } else if (basicMove.loopDownClip) {
            newWrapMode = WrapMode.Loop;
        }

        if (basicMove.animMap[1].clip != null) attachAnimation(basicMove.animMap[1].clip, animName + "_2", basicMove._animationSpeed, newWrapMode, basicMove.animMap[1].length);
        if (basicMove.animMap[2].clip != null) attachAnimation(basicMove.animMap[2].clip, animName + "_3", basicMove._animationSpeed, newWrapMode, basicMove.animMap[2].length);
        if (basicMove.animMap[3].clip != null) attachAnimation(basicMove.animMap[3].clip, animName + "_4", basicMove._animationSpeed, newWrapMode, basicMove.animMap[3].length);
        if (basicMove.animMap[4].clip != null) attachAnimation(basicMove.animMap[4].clip, animName + "_5", basicMove._animationSpeed, newWrapMode, basicMove.animMap[4].length);
        if (basicMove.animMap[5].clip != null) attachAnimation(basicMove.animMap[5].clip, animName + "_6", basicMove._animationSpeed, newWrapMode, basicMove.animMap[5].length);
        if (basicMove.animMap.Length > 6 && basicMove.animMap[6].clip != null) attachAnimation(basicMove.animMap[6].clip, animName + "_7", basicMove._animationSpeed, newWrapMode, basicMove.animMap[6].length);
        if (basicMove.animMap.Length > 7 && basicMove.animMap[7].clip != null) attachAnimation(basicMove.animMap[7].clip, animName + "_8", basicMove._animationSpeed, newWrapMode, basicMove.animMap[7].length);
        if (basicMove.animMap.Length > 8 && basicMove.animMap[8].clip != null) attachAnimation(basicMove.animMap[8].clip, animName + "_9", basicMove._animationSpeed, newWrapMode, basicMove.animMap[8].length);
	}

    private void attachAnimation(AnimationClip clip, string animName, Fix64 speed, WrapMode wrapMode, Fix64 length)
    {
        if (!controlsScript.myInfo.useAnimationMaps) length = clip.length;
        if (controlsScript.myInfo.animationType == AnimationType.Legacy) {
            legacyControl.AddClip(clip, animName, speed, wrapMode, length);
        } else {
            mecanimControl.AddClip(clip, animName, speed, wrapMode, length);
        }
    }

    public BasicMoveInfo GetBasicAnimationInfo(BasicMoveReference reference)
    {
        foreach(BasicMoveInfo basicMove in basicMoveList){
            if (basicMove.reference == reference) return basicMove;
        }
        return null;
    }

	public string GetAnimationString(BasicMoveInfo basicMove, int clipNum)
    {
		if (clipNum == 1) return basicMove.name;
		if (clipNum == 2 && basicMove.animMap[1].clip != null) return basicMove.name + "_2";
		if (clipNum == 3 && basicMove.animMap[2].clip != null) return basicMove.name + "_3";
		if (clipNum == 4 && basicMove.animMap[3].clip != null) return basicMove.name + "_4";
		if (clipNum == 5 && basicMove.animMap[4].clip != null) return basicMove.name + "_5";
		if (clipNum == 6 && basicMove.animMap[5].clip != null) return basicMove.name + "_6";
		if (clipNum == 7 && basicMove.animMap.Length > 6 && basicMove.animMap[6].clip != null) return basicMove.name + "_7";
		if (clipNum == 8 && basicMove.animMap.Length > 7 && basicMove.animMap[7].clip != null) return basicMove.name + "_8";
		if (clipNum == 9 && basicMove.animMap.Length > 8 && basicMove.animMap[8].clip != null) return basicMove.name + "_9";
		return basicMove.name;
	}


    public bool IsBasicMovePlaying(BasicMoveInfo basicMove)
    {
        if (basicMove.animMap[0].clip != null && IsAnimationPlaying(basicMove.name)) return true;
        if (basicMove.animMap[1].clip != null && IsAnimationPlaying(basicMove.name + "_2")) return true;
        if (basicMove.animMap[2].clip != null && IsAnimationPlaying(basicMove.name + "_3")) return true;
        if (basicMove.animMap[3].clip != null && IsAnimationPlaying(basicMove.name + "_4")) return true;
        if (basicMove.animMap[4].clip != null && IsAnimationPlaying(basicMove.name + "_5")) return true;
        if (basicMove.animMap[5].clip != null && IsAnimationPlaying(basicMove.name + "_6")) return true;
        if (basicMove.animMap.Length > 6 && basicMove.animMap[6].clip != null && IsAnimationPlaying(basicMove.name + "_7")) return true;
        if (basicMove.animMap.Length > 7 && basicMove.animMap[7].clip != null && IsAnimationPlaying(basicMove.name + "_8")) return true;
        if (basicMove.animMap.Length > 8 && basicMove.animMap[8].clip != null && IsAnimationPlaying(basicMove.name + "_9")) return true;
        return false;
    }
	
	public bool IsAnimationPlaying(string animationName)
    {
		if (controlsScript.myInfo.animationType == AnimationType.Legacy){
            return legacyControl.IsPlaying(animationName);
		}else{
			return mecanimControl.IsPlaying(animationName);
		}
	}
	
	public int AnimationTimesPlayed(string animationName)
    {
		if (controlsScript.myInfo.animationType == AnimationType.Legacy){
            return legacyControl.GetTimesPlayed(animationName);
		}else{
			return mecanimControl.GetTimesPlayed(animationName);
		}
	}

    public void OverrideWrapMode(WrapMode wrap)
    {
		if (controlsScript.myInfo.animationType == AnimationType.Legacy){
            legacyControl.OverrideCurrentWrapMode(wrap);
		}else{
			mecanimControl.OverrideCurrentWrapMode(wrap);
        }
    }

    public Fix64 GetAnimationLength(string animationName)
    {
		if (controlsScript.myInfo.animationType == AnimationType.Legacy){
            return legacyControl.GetAnimationData(animationName).length;
		}else{
			return mecanimControl.GetAnimationData(animationName).length;
		}
	}

	public bool AnimationExists(string animationName)
    {
		if (controlsScript.myInfo.animationType == AnimationType.Legacy){
            return (legacyControl.GetAnimationData(animationName) != null);
		}else{
			return (mecanimControl.GetAnimationData(animationName) != null);
		}
	}

    public void PlayAnimation(string animationName, Fix64 blendingTime)
    {
		PlayAnimation(animationName, blendingTime, 0);
	}

    public void PlayAnimation(string animationName, Fix64 blendingTime, Fix64 normalizedTime)
    {
        if ((UFE.isConnected || UFE.config.debugOptions.emulateNetwork) &&
            UFE.config.networkOptions.disableBlending) blendingTime = 0;

        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            legacyControl.Play(animationName, blendingTime, normalizedTime);
        }
        else if (controlsScript.myInfo.animationType == AnimationType.Mecanim2D)
        {
            mecanimControl.Play(animationName, 0, normalizedTime, false);
        }
        else
        {
			mecanimControl.Play(animationName, blendingTime, normalizedTime, (controlsScript.mirror > 0 && UFE.config.characterRotationOptions.autoMirror));
		}
	}

	public void StopAnimation(string animationName)
    {
		if (controlsScript.myInfo.animationType == AnimationType.Legacy){
            legacyControl.Stop(animationName);
		}else{
			mecanimControl.Stop();
		}
	}

    public void SetAnimationSpeed(Fix64 speed)
    {
        if (speed < 1) animationPaused = true;
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            legacyControl.SetSpeed(speed);
		}
        else
        {
			mecanimControl.SetSpeed(speed);
		}
	}

    public void SetAnimationSpeed(string animationName, Fix64 speed)
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            legacyControl.SetSpeed(animationName, speed);
		}
        else
        {
			mecanimControl.SetSpeed(animationName, speed);
		}
	}

    public void SetAnimationNormalizedSpeed(string animationName, Fix64 normalizedSpeed)
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            legacyControl.SetNormalizedSpeed(animationName, normalizedSpeed);
        }
        else
        {
            mecanimControl.SetNormalizedSpeed(animationName, normalizedSpeed);
        }
    }

    public Fix64 GetAnimationSpeed()
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            return legacyControl.GetSpeed();
        }
        else
        {
            return mecanimControl.GetSpeed();
        }
    }
    public Fix64 GetNormalizedSpeed()
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            return legacyControl.GetNormalizedSpeed();
        }
        else
        {
            return mecanimControl.GetNormalizedSpeed();
        }
    }

    public Fix64 GetAnimationSpeed(string animationName)
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            return legacyControl.GetSpeed(animationName);
        }
        else
        {
            return mecanimControl.GetSpeed(animationName);
        }
    }

    public Fix64 GetOriginalAnimationSpeed(string animationName)
    {
        return mecanimControl.GetOriginalSpeed(animationName);
    }

	public void RestoreAnimationSpeed()
    {
		if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            legacyControl.RestoreSpeed();
		}
        else
        {
			mecanimControl.RestoreSpeed();
		}
		animationPaused = false;
	}
	
	public void PlayBasicMove(BasicMoveInfo basicMove)
    {
		PlayBasicMove(basicMove, basicMove.name);
	}
	
	public void PlayBasicMove(BasicMoveInfo basicMove, bool replay)
    {
		PlayBasicMove(basicMove, basicMove.name, replay);
	}

	public void PlayBasicMove(BasicMoveInfo basicMove, string clipName)
    {
		PlayBasicMove(basicMove, clipName, true);
	}

	public void PlayBasicMove(BasicMoveInfo basicMove, string clipName, bool replay)
    {
		if (overrideNextBlendingValue > -1)
        {
			PlayBasicMove(basicMove, clipName, overrideNextBlendingValue);
			overrideNextBlendingValue = -1;
		}
        else if (basicMove.overrideBlendingIn)
        {
            PlayBasicMove(basicMove, clipName, basicMove._blendingIn, replay, basicMove.invincible);
		}
        else
        {
            PlayBasicMove(basicMove, clipName, controlsScript.myInfo._blendingTime, replay, basicMove.invincible);
		}
		
		if (basicMove.overrideBlendingOut) overrideNextBlendingValue = basicMove._blendingOut;
	}

    public void PlayBasicMove(BasicMoveInfo basicMove, string clipName, Fix64 blendingTime)
    {
        PlayBasicMove(basicMove, clipName, blendingTime, true, basicMove.invincible);
	}

    public void PlayBasicMove(BasicMoveInfo basicMove, string clipName, Fix64 blendingTime, bool replay)
    {
        PlayBasicMove(basicMove, clipName, blendingTime, replay, basicMove.invincible);
    }

    public void PlayBasicMove(BasicMoveInfo basicMove, string clipName, Fix64 blendingTime, bool replay, bool hideHitBoxes)
    {
        if (basicMove.useMoveFile && controlsScript.currentMove == null && controlsScript.storedMove == null)
        {
            controlsScript.CastMove(basicMove.moveInfo, true);
            return;
        }

		if (IsAnimationPlaying(clipName) && !replay) return;

        // Play animation
		PlayAnimation(clipName, blendingTime);

        // Set basic move reference
        controlsScript.currentBasicMove = basicMove.reference;

        // Set root motion options
        controlsScript.applyRootMotion = basicMove.applyRootMotion;
        controlsScript.lockXMotion = basicMove.lockXMotion;
        controlsScript.lockYMotion = basicMove.lockYMotion;
        controlsScript.lockZMotion = basicMove.lockZMotion;

        // Toggle head look
        controlsScript.ToggleHeadLook(!basicMove.disableHeadLook);
        
        // Play sound effects
        UFE.PlaySound(basicMove.soundEffects);

        // Set hit boxes visibility
        hitBoxesScript.HideHitBoxes(hideHitBoxes);

        // Set visibility to nested game objects
        HitBoxesScript hitBoxes = controlsScript.character.GetComponent<HitBoxesScript>();
        if (hitBoxes != null)
        {
            foreach (HitBox hitBox in hitBoxes.hitBoxes)
            {
                if (hitBox != null && hitBox.bodyPart != BodyPart.none && hitBox.position != null)
                {
                    hitBox.position.gameObject.SetActive(hitBox.defaultVisibility);
                }
            }
        }

        // Play particle effects
        if (basicMove.particleEffect.prefab != null)
        {
            Vector3 newPosition = hitBoxesScript.GetPosition(basicMove.particleEffect.bodyPart).ToVector();
            newPosition.x += basicMove.particleEffect.positionOffSet.x * -controlsScript.mirror;
            newPosition.y += basicMove.particleEffect.positionOffSet.y;
            newPosition.z += basicMove.particleEffect.positionOffSet.z;
            GameObject pTemp = UFE.SpawnGameObject(basicMove.particleEffect.prefab, newPosition, Quaternion.identity, Mathf.RoundToInt(basicMove.particleEffect.duration * UFE.config.fps));

            if (basicMove.particleEffect.mirrorOn2PSide && controlsScript.mirror > 0)
            {
                pTemp.transform.localEulerAngles = new Vector3(pTemp.transform.localEulerAngles.x, pTemp.transform.localEulerAngles.y + 180, pTemp.transform.localEulerAngles.z);
            }
            if (basicMove.particleEffect.stick) pTemp.transform.parent = transform;
        }

        // Set animation maps
        for (int i = 0; i < basicMove.animMap.Length; i ++)
        {
            if (clipName == GetAnimationString(basicMove, i + 1))
            {
                if (basicMove.animMap[i].hitBoxDefinitionType == HitBoxDefinitionType.AutoMap)
                {
                    hitBoxesScript.customHitBoxes = null;
                    hitBoxesScript.bakeSpeed = basicMove.autoSpeed ? false : basicMove.animMap[i].bakeSpeed;
                    hitBoxesScript.animationMaps = basicMove.animMap[i].animationMaps;
                    //hitBoxesScript.UpdateMap(0);
                }
                else
                {
                    hitBoxesScript.customHitBoxes = basicMove.animMap[i].customHitBoxDefinition;
                }
                break;
            }
        }

        // Fire basic move event
        UFE.FireBasicMove(basicMove.reference, controlsScript);
    }

	public void SetAnimationPosition(string animationName, Fix64 normalizedTime)
    {
		if (controlsScript.myInfo.animationType == AnimationType.Legacy){
            legacyControl.SetCurrentClipPosition(normalizedTime);
		}else{
			mecanimControl.SetCurrentClipPosition(normalizedTime);
		}
	}

    public Vector3 GetDeltaDisplacement()
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            return legacyControl.GetDeltaDisplacement();
        }
        else
        {
            return mecanimControl.GetDeltaDisplacement();
        }
    }

    public Vector3 GetDeltaPosition()
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            return legacyControl.GetDeltaPosition();
        }
        else
        {
            return mecanimControl.GetDeltaPosition();
        }
    }

    public string GetCurrentClipName()
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            return legacyControl.GetCurrentClipName();
        }
        else
        {
            return mecanimControl.GetCurrentClipName();
        }
    }

    public Fix64 GetCurrentClipPosition()
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            return legacyControl.GetCurrentClipPosition();
		}
        else
        {
			return mecanimControl.GetCurrentClipPosition();
		}
	}

    public Fix64 GetCurrentClipNormalizedTime()
    {
        return mecanimControl.GetCurrentClipNormalizedTime();
    }

    public int GetCurrentClipFrame(bool bakeSpeed = false)
    {
        if (controlsScript.myInfo.animationType == AnimationType.Legacy)
        {
            return legacyControl.GetCurrentClipFrame(bakeSpeed);
		}
        else
        {
            return mecanimControl.GetCurrentClipFrame(bakeSpeed);
        }
    }

    public Fix64 GetAnimationNormalizedTime(int animFrame, MoveInfo move)
    {
		if (move == null) return 0;
		if (move._animationSpeed < 0)
        {
			return (animFrame/ (Fix64)move.totalFrames) + 1;
		}
        else
        {
			return animFrame/ (Fix64)move.totalFrames;
		}
	}
	
	public void SetMecanimMirror(bool toggle)
    {
		mecanimControl.SetMirror(toggle, UFE.config.characterRotationOptions._mirrorBlending, true);
	}

	public bool CompareBlockButtons(ButtonPress button)
    {
		if (button == ButtonPress.Button1 && UFE.config.blockOptions.blockType == BlockType.HoldButton1) return true;
		if (button == ButtonPress.Button2 && UFE.config.blockOptions.blockType == BlockType.HoldButton2) return true;
		if (button == ButtonPress.Button3 && UFE.config.blockOptions.blockType == BlockType.HoldButton3) return true;
		if (button == ButtonPress.Button4 && UFE.config.blockOptions.blockType == BlockType.HoldButton4) return true;
		if (button == ButtonPress.Button5 && UFE.config.blockOptions.blockType == BlockType.HoldButton5) return true;
		if (button == ButtonPress.Button6 && UFE.config.blockOptions.blockType == BlockType.HoldButton6) return true;
		if (button == ButtonPress.Button7 && UFE.config.blockOptions.blockType == BlockType.HoldButton7) return true;
		if (button == ButtonPress.Button8 && UFE.config.blockOptions.blockType == BlockType.HoldButton8) return true;
		if (button == ButtonPress.Button9 && UFE.config.blockOptions.blockType == BlockType.HoldButton9) return true;
		if (button == ButtonPress.Button10 && UFE.config.blockOptions.blockType == BlockType.HoldButton10) return true;
		if (button == ButtonPress.Button11 && UFE.config.blockOptions.blockType == BlockType.HoldButton11) return true;
		if (button == ButtonPress.Button12 && UFE.config.blockOptions.blockType == BlockType.HoldButton12) return true;
		return false;
	}
	
	public bool CompareParryButtons(ButtonPress button)
    {
		if (button == ButtonPress.Button1 && UFE.config.blockOptions.parryType == ParryType.TapButton1) return true;
		if (button == ButtonPress.Button2 && UFE.config.blockOptions.parryType == ParryType.TapButton2) return true;
		if (button == ButtonPress.Button3 && UFE.config.blockOptions.parryType == ParryType.TapButton3) return true;
		if (button == ButtonPress.Button4 && UFE.config.blockOptions.parryType == ParryType.TapButton4) return true;
		if (button == ButtonPress.Button5 && UFE.config.blockOptions.parryType == ParryType.TapButton5) return true;
		if (button == ButtonPress.Button6 && UFE.config.blockOptions.parryType == ParryType.TapButton6) return true;
		if (button == ButtonPress.Button7 && UFE.config.blockOptions.parryType == ParryType.TapButton7) return true;
		if (button == ButtonPress.Button8 && UFE.config.blockOptions.parryType == ParryType.TapButton8) return true;
		if (button == ButtonPress.Button9 && UFE.config.blockOptions.parryType == ParryType.TapButton9) return true;
		if (button == ButtonPress.Button10 && UFE.config.blockOptions.parryType == ParryType.TapButton10) return true;
		if (button == ButtonPress.Button11 && UFE.config.blockOptions.parryType == ParryType.TapButton11) return true;
		if (button == ButtonPress.Button12 && UFE.config.blockOptions.parryType == ParryType.TapButton12) return true;
		return false;
	}

	private bool hasEnoughGauge(Fix64 gaugeNeeded, int targetGauge)
    {
		if (!UFE.config.gameGUI.hasGauge) return true;
        if (controlsScript.currentGaugesPoints[targetGauge] < ((Fix64)controlsScript.myInfo.maxGaugePoints * (gaugeNeeded / 100))) return false;
		return true;
	}

	public MoveInfo GetIntro()
    {
		return InstantiateMove(intro);
	}
	
	public MoveInfo GetOutro()
    {
		return InstantiateMove(outro);
	}
	
	public MoveInfo InstantiateMove(MoveInfo move)
    {
		if (move == null) return null;
		MoveInfo newMove = Instantiate(move) as MoveInfo;
		newMove.name = move.name;
		return newMove;
	}

	public void GetNextMove(MoveInfo currentMove, ref MoveInfo storedMove)
    {
		if (currentMove.frameLinks.Length == 0) return;

		foreach(FrameLink frameLink in currentMove.frameLinks){
			if (frameLink.linkableMoves.Length == 0) continue;
			if (frameLink.cancelable){
                foreach (MoveInfo move in frameLink.linkableMoves) {
                    if (move == null) continue;

                    bool gaugePass = true;
                    if (!frameLink.ignoreGauge)
                        foreach (GaugeInfo gaugeInfo in move.gauges)
                            if (!hasEnoughGauge(gaugeInfo._gaugeRequired, (int)gaugeInfo.targetGauge)) gaugePass = false;

                    if (gaugePass &&
                       (move.defaultInputs.buttonExecution.Length == 0 || frameLink.ignoreInputs ||
                       (move.defaultInputs.onReleaseExecution && !move.defaultInputs.requireButtonPress && controlsScript.inputHeldDown[move.defaultInputs.buttonExecution[0]] == 0)))
                        storedMove = InstantiateMove(move);
				}
			}
		}
	}

	public void ClearLastButtonSequence()
    {
		lastButtonPresses.Clear();
        lastTimePress = 0;
	}

	private bool checkExecutionState(ButtonPress[] buttonPress, bool inputUp)
    {
		if (inputUp 
		    && lastButtonPresses.Count > 0 
			&& buttonPress[0].Equals(lastButtonPresses.ToArray()[lastButtonPresses.Count - 1])) return false;

		return true;
	}

	public MoveInfo GetMove(ButtonPress[] buttonPress, Fix64 charge, MoveInfo currentMove, bool inputUp)
    {
		return GetMove(buttonPress, charge, currentMove, inputUp, false);
	}

    public MoveInfo GetMove(ButtonPress[] buttonPress, Fix64 charge, MoveInfo currentMove, bool inputUp, bool forceExecution)
    {
		if (buttonPress.Length > 0 &&
            (UFE.currentFrame / (Fix64) UFE.config.fps) - lastTimePress <= controlsScript.myInfo._executionTiming) {

			// Attempt first execution
            foreach (MoveInfo move in moves) {
                if (move == null) continue;
				MoveInfo newMove = TestMoveExecution(move, currentMove, buttonPress, inputUp, true);
				if (newMove != null) return newMove;
			}
		}

        // If buttons were pressed, add it to last button presses
		if (buttonPress.Length > 0) {
            // If the last time pressed is over the maximum execution timming, clear recorded sequences
            if ((UFE.currentFrame / (Fix64) UFE.config.fps) - lastTimePress > controlsScript.myInfo._executionTiming) {
				ClearLastButtonSequence();
			}

			if (!forceExecution)
            {
                // If button down event happened on the same frame as last input, merge inputs
                if (!inputUp && charge == 0 && lastButtonPresses.Count > 0 && lastButtonPresses[lastButtonPresses.Count - 1].chargeTime == 0 && lastTimePress == (UFE.currentFrame / (Fix64)UFE.config.fps)) {
                    lastButtonPresses[lastButtonPresses.Count - 1].buttonPresses = lastButtonPresses[lastButtonPresses.Count - 1].buttonPresses.Concat(buttonPress).ToArray();
                }
                // Else, add to sequence list
                else
                {
                    lastButtonPresses.Add(new ButtonSequenceRecord(buttonPress, charge));
                    lastTimePress = UFE.currentFrame / (Fix64)UFE.config.fps;
                }

                if (controlsScript.debugInfo.buttonSequence) {
                    string allbp = "";
                    foreach (ButtonSequenceRecord bpr in lastButtonPresses){
                        allbp += bpr.chargeTime > 0? " (up)" : " (down)";
                        foreach (ButtonPress bp in bpr.buttonPresses) {
                            allbp += " " + bp.ToString();
                        }
                        allbp += " | ";
                    }
                    Debug.Log(allbp);
                }
            }

            // If input sequence failed to cast a move, attempt second execution with current inputs
            foreach (MoveInfo move in moves) {
                MoveInfo newMove = TestMoveExecution(move, currentMove, buttonPress, inputUp, false, forceExecution);
                if (newMove != null) return newMove;
            }
		}

		return null;
	}
    
    private bool searchMoveBuffer(string moveName, FrameLink[] frameLinks, int currentFrame)
    {
        foreach (FrameLink frameLink in frameLinks) {
            if ((currentFrame >= frameLink.activeFramesBegins && currentFrame <= frameLink.activeFramesEnds)
                || (currentFrame >= (frameLink.activeFramesBegins - UFE.config.executionBufferTime)
                && currentFrame <= frameLink.activeFramesEnds) && frameLink.allowBuffer) {

                foreach (MoveInfo move in frameLink.linkableMoves) {
                    if (move == null) continue;
                    if (moveName == move.moveName) return true;
                }
            }
        }

        return false;
    }

	public bool SearchMove(string moveName, FrameLink[] frameLinks, bool ignoreConditions = false)
    {
		foreach(FrameLink frameLink in frameLinks){
			if (frameLink.cancelable){
				if (ignoreConditions && !frameLink.ignorePlayerConditions) continue;

                foreach (MoveInfo move in frameLink.linkableMoves) {
                    if (move == null) continue;
					if (moveName == move.moveName) return true;
				}
			}
		}
		
		return false;
	}

	private bool searchMove(string moveName, MoveInfo[] moves)
    {
        foreach (MoveInfo move in moves) {
            if (move == null) continue;
            if (moveName == move.moveName) return true;
        }
		
		return false;
	}

	public bool HasMove(string moveName)
    {
		foreach(MoveInfo move in this.moves)
			if (moveName == move.moveName) return true;
		
		return false;
	}


    public bool ValidateMoveExecution(MoveInfo move)
    {
        if (!searchMove(move.moveName, attackMoves)) return false;
		if (!ValidateMoveStances(move.selfConditions, controlsScript, true)) return false;
		if (!ValidateMoveStances(move.opponentConditions, controlsScript.opControlsScript)) return false;
		if (!ValidadeBasicMove(move.selfConditions, controlsScript)) return false;
		if (!ValidadeBasicMove(move.opponentConditions, controlsScript.opControlsScript)) return false;

        foreach (GaugeInfo gaugeInfo in move.gauges) {
            if (!hasEnoughGauge(gaugeInfo._gaugeRequired, (int)gaugeInfo.targetGauge)) return false;
        }

		if (move.previousMoves.Length > 0 && controlsScript.currentMove == null) return false;
		if (move.previousMoves.Length > 0 && !searchMove(controlsScript.currentMove.moveName, move.previousMoves)) return false;

		if (controlsScript.currentMove != null && controlsScript.currentMove.frameLinks.Length == 0) return false;
		if (controlsScript.currentMove != null && !SearchMove(move.moveName, controlsScript.currentMove.frameLinks)) return false;
		return true;
	}

	
	public bool ValidateMoveStances(PlayerConditions conditions, ControlsScript cScript)
    {
		return ValidateMoveStances(conditions, cScript, false);
	}

	public bool ValidateMoveStances(PlayerConditions conditions, ControlsScript cScript, bool bypassCrouchStance)
    {
		bool stateCheck = conditions.possibleMoveStates.Length > 0? false : true;
		foreach(PossibleMoveStates possibleMoveState in conditions.possibleMoveStates){

			if (possibleMoveState.possibleState != cScript.currentState
			    && (!bypassCrouchStance || (bypassCrouchStance && cScript.currentState != PossibleStates.Stand))) continue;
			
			if (cScript.normalizedDistance < (Fix64)possibleMoveState.proximityRangeBegins/100) continue;
			if (cScript.normalizedDistance > (Fix64)possibleMoveState.proximityRangeEnds/100) continue;

            if (cScript.currentState == PossibleStates.Stand) {
                //if (cScript.Physics.isTakingOff) continue;
				if (!possibleMoveState.standBy && cScript.currentSubState == SubStates.Resting) continue;
				if (!possibleMoveState.movingBack && cScript.currentSubState == SubStates.MovingBack) continue;
				if (!possibleMoveState.movingForward && cScript.currentSubState == SubStates.MovingForward) continue;

			} else if (cScript.currentState == PossibleStates.NeutralJump
			          || cScript.currentState == PossibleStates.ForwardJump
			          || cScript.currentState == PossibleStates.BackJump){ 
				
				if (cScript.normalizedJumpArc < (Fix64)possibleMoveState.jumpArcBegins/100) continue;
				if (cScript.normalizedJumpArc > (Fix64)possibleMoveState.jumpArcEnds/100) continue;
			}

            if ((!possibleMoveState.blocking && !UFE.config.blockOptions.allowMoveCancel) 
                && (cScript.currentSubState == SubStates.Blocking || cScript.isBlocking)) continue;

			if ((!possibleMoveState.stunned && possibleMoveState.possibleState != PossibleStates.Down) 
			    && cScript.currentSubState == SubStates.Stunned) continue;

			stateCheck = true;
		}
		return stateCheck;
	}

	public bool ValidadeBasicMove(PlayerConditions conditions, ControlsScript cScript)
    {
		if (conditions.basicMoveLimitation.Length == 0) return true;
		if (Array.IndexOf(conditions.basicMoveLimitation, cScript.currentBasicMove) >= 0) return true;
		return false;
	}

    public bool CanPlink(MoveInfo currentMove, MoveInfo tempMove)
    {
        // ignore plink candidate if
        if (currentMove == null) return false; // current move doesnt exist
        if (currentMove.currentFrame > UFE.config.plinkingDelay) return false; // current frame is outside plinking window
        if (currentMove.defaultInputs.buttonExecution.Length == 0) return false; // current move has no button execution
        if (currentMove.defaultInputs.onReleaseExecution) return false; // current move has a release button execution
        if (currentMove.previousMoves.Length > 0) return false; // current move is coming from a previous move chain
        if (tempMove.defaultInputs.buttonSequence.Length < currentMove.defaultInputs.buttonSequence.Length) return false; // plink candidate has less button sequences than current move
        if (tempMove.defaultInputs.buttonExecution.Length <= currentMove.defaultInputs.buttonExecution.Length) return false; // plink candidate has less (or equal) number of button execution than current move
        ButtonPress[] compareExecution = ArrayIntersect<ButtonPress>(currentMove.defaultInputs.buttonExecution, tempMove.defaultInputs.buttonExecution);
        if (!ArraysEqual<ButtonPress>(compareExecution, currentMove.defaultInputs.buttonExecution)) return false; // current move's button executions do not match the button executions from plinking candidate

        return true;
    }
	
	private MoveInfo TestMoveExecution(MoveInfo move, MoveInfo currentMove, ButtonPress[] buttonPress, bool inputUp, bool fromSequence)
    {
		return TestMoveExecution(move, currentMove, buttonPress, inputUp, fromSequence, false);
	}

	private MoveInfo TestMoveExecution(MoveInfo move, MoveInfo currentMove, ButtonPress[] buttonPress, bool inputUp, bool fromSequence, bool forceExecution)
    {
        foreach (GaugeInfo gaugeInfo in move.gauges) {
            if (!hasEnoughGauge(gaugeInfo._gaugeRequired, (int)gaugeInfo.targetGauge)) return null;
        }
		if (move.previousMoves.Length > 0 && currentMove == null) return null;
        if (move.previousMoves.Length > 0 && !searchMove(currentMove.moveName, move.previousMoves)) return null;
        if (controlsScript.isAirRecovering && controlsScript.airRecoveryType == AirRecoveryType.CantMove) return null;
        if (move.cooldown && lastMovesPlayed.ContainsKey(move.moveName) && UFE.currentFrame - lastMovesPlayed[move.moveName] <= move.cooldownFrames) return null;
        

        // Look for Projectiles On Screen
        if (move.projectiles.Length > 0 && controlsScript.projectiles.Count > 0) {
            int totalOnScreen = 0;
            foreach (ProjectileMoveScript pScript in controlsScript.projectiles)
            {
                if (pScript.data.limitMultiCasting)
                {
                    if (pScript.isActiveAndEnabled && pScript.onView) totalOnScreen++;
                    foreach (Projectile proj in move.projectiles)
                    {
                        if (proj.limitMultiCasting && ((!proj.limitOnlyThis && totalOnScreen >= proj.onScreenLimit) ||
                            (pScript.data.moveName == move.moveName && totalOnScreen >= proj.onScreenLimit))) return null;
                    }
                }
            }

        }

        // Look for Assists on Screen
        if (move.characterAssist.Length > 0)
        {
            foreach (ControlsScript cScript in controlsScript.assists)
            {
                if (!cScript.GetActive()) continue;
                foreach (CharacterAssist cAssist in move.characterAssist)
                {
                    if (cScript.myInfo.characterName == cAssist.characterInfo.characterName) return null;
                }
            }

        }

        if (currentMove == null || (currentMove != null && !SearchMove(move.moveName, currentMove.frameLinks, true))){
			if (!ValidateMoveStances(move.selfConditions, controlsScript)) return null;
			if (!ValidateMoveStances(move.opponentConditions, controlsScript.opControlsScript)) return null;
			if (!ValidadeBasicMove(move.selfConditions, controlsScript)) return null;
			if (!ValidadeBasicMove(move.opponentConditions, controlsScript.opControlsScript)) return null;
		}

        if (!CompareSequence(move.defaultInputs, buttonPress, inputUp, fromSequence, true) 
            && !CompareSequence(move.altInputs, buttonPress, inputUp, fromSequence, false)) return null;


		if (controlsScript.storedMove != null && move.moveName == controlsScript.storedMove.moveName)
			return controlsScript.storedMove;

        if (controlsScript.debugInfo.buttonSequence) {
            string allbp4 = "";
            foreach (ButtonPress bp in buttonPress) allbp4 += " " + bp.ToString();
            Debug.Log(move.moveName + ": Button Execution: " + allbp4);
        }

        if (currentMove == null || forceExecution || (searchMoveBuffer(move.moveName, currentMove.frameLinks, currentMove.currentFrame)) || UFE.config.executionBufferType == ExecutionBufferType.AnyMove)
        {
            MoveInfo newMove = InstantiateMove(move);
			
			if ((controlsScript.currentState == PossibleStates.NeutralJump ||
			    controlsScript.currentState == PossibleStates.ForwardJump ||
			    controlsScript.currentState == PossibleStates.BackJump) &&
			    totalAirMoves >= controlsScript.myInfo.possibleAirMoves) return null;

			return newMove;
		}

		return null;
	}
	

    private bool CompareSequence(MoveInputs moveInputs, ButtonPress[] buttonPress, bool inputUp, bool fromSequence, bool allowEmptyExecution)
    {
        if (!allowEmptyExecution && moveInputs.buttonExecution.Length == 0) return false;
        Array.Sort(buttonPress);
		Array.Sort(moveInputs.buttonExecution);

        if (fromSequence)
        {
			if (moveInputs.buttonSequence.Length == 0) return false;
            if (moveInputs.chargeMove) {
                bool charged = false;
                foreach (ButtonSequenceRecord bsr in lastButtonPresses) {
                    if (Array.IndexOf(bsr.buttonPresses, moveInputs.buttonSequence[0]) >= 0 && bsr.chargeTime >= moveInputs._chargeTiming) {
                        charged = true;
                    }
                }

				if (!charged) return false;
			}

            List<ButtonPress[]> buttonPressesList = new List<ButtonPress[]>();
            foreach (ButtonSequenceRecord bsr in lastButtonPresses) {
                if (bsr.chargeTime == 0 || (moveInputs.allowNegativeEdge && Array.IndexOf(bsr.buttonPresses, moveInputs.buttonSequence[0]) >= 0))
                {
                    if (moveInputs.forceAxisPrecision)
                    {
                        List<ButtonPress> filteredBtp = new List<ButtonPress>(bsr.buttonPresses);
                        if (filteredBtp.Contains(ButtonPress.DownBack)
                            || filteredBtp.Contains(ButtonPress.DownForward)
                            || filteredBtp.Contains(ButtonPress.UpBack)
                            || filteredBtp.Contains(ButtonPress.UpForward))
                        {
                            filteredBtp.RemoveAll(item => (int)item <= 3);
                        }

                        buttonPressesList.Add(filteredBtp.ToArray());
                    }
                    else
                    {
                        buttonPressesList.Add(bsr.buttonPresses);
                    }
                }
            }

            if (buttonPressesList.Count >= moveInputs.buttonSequence.Length)
            {
                int compareRange = buttonPressesList.Count - moveInputs.buttonSequence.Length;

                if (moveInputs.allowInputLeniency) compareRange -= moveInputs.leniencyBuffer;
                if (compareRange < 0) compareRange = 0;

                ButtonPress[][] buttonPressesListArray = buttonPressesList.GetRange(compareRange, buttonPressesList.Count - compareRange).ToArray();
                ButtonPress[] compareSequence = ArrayIntersect<ButtonPress>(moveInputs.buttonSequence, buttonPressesListArray);

                if (!ArraysEqual<ButtonPress>(compareSequence, moveInputs.buttonSequence)) return false;
            }
            else
            {
				return false;
			}
        }
        else
        {
			if (moveInputs.buttonSequence.Length > 0) return false;
		}

		if (!inputUp && !moveInputs.onPressExecution) return false;
		if (inputUp && !moveInputs.onReleaseExecution) return false;
		if (!ArraysEqual<ButtonPress>(buttonPress, moveInputs.buttonExecution)) return false;

        return true;
    }

	private T[] ArrayIntersect<T>(T[] a1, T[] a2) {
		if (a1 == null || a2 == null) return null;
		
		EqualityComparer<T> comparer = EqualityComparer<T>.Default;
		List<T> intersection = new List<T>();
		int nextStartingPoint = 0;
		for (int i = 0; i < a1.Length; i++){ // button sequence
			bool added = false;
			for (int k = nextStartingPoint; k < a2.Length; k++){ // button presses
				if (comparer.Equals(a1[i], a2[k])) {
					intersection.Add(a2[k]);
					nextStartingPoint = k;
					added = true;
					break;
				}
			}
			if (!added) return null;
		}

		return intersection.ToArray();
	}

	private T[] ArrayIntersect<T>(T[] a1, T[][] a2) {
		if (a1 == null || a2 == null) return null;

        List<T> intersection = new List<T>();
        int sCount = 0;
        for (int i = 0; i < a2.Length; i++) {
            if (sCount < a1.Length && a2[i].Contains(a1[sCount])) {
                intersection.Add(a1[sCount]);
                sCount++;
            }
        }

		return intersection.ToArray();
	}

    private bool ArraysEqual<T>(T[] a1, T[] a2) {
    	if (ReferenceEquals(a1,a2)) return true;
  		if (a1 == null || a2 == null) return false;
		if (a1.Length != a2.Length) return false;
	    EqualityComparer<T> comparer = EqualityComparer<T>.Default;
		for (int i = 0; i < a1.Length; i++){
        	if (!comparer.Equals(a1[i], a2[i])) return false;
    	}
    	return true;
	}
}
