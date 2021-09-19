using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using FPLibrary;
using UFE3D;

public class ControlsScript : MonoBehaviour {

    #region trackable definitions
    public Fix64 afkTimer;
    public int airJuggleHits;
    public AirRecoveryType airRecoveryType;
    public bool applyRootMotion;
    public bool blockStunned;
    public Fix64 currentLifePoints;
    public Fix64[] currentGaugesPoints;
    public Fix64 comboDamage;
    public Fix64 comboHitDamage;
    public int comboHits;
    public int consecutiveCrumple;
    public BasicMoveReference currentBasicMove;
    public CombatStances currentCombatStance;
    public Fix64 currentDrained;
    public string currentHitAnimation;
    public PossibleStates currentState;
    public SubStates currentSubState;

    public MoveInfo DCMove;
    public CombatStances DCStance;
    public MoveInfo enterMove;
    public MoveInfo exitMove;
    public bool firstHit;
    public Fix64 gaugeDPS;
    public GaugeId gaugeDrainId;
    public bool hitDetected;
    public Fix64 hitAnimationSpeed;
    public bool inhibitGainWhileDraining;
    public bool isAirRecovering;
    public bool isAssist;
    public bool isBlocking;
    public bool isCrouching;
    public bool isDead;
    public bool ignoreCollisionMass;
    public bool introPlayed;
    public bool lit;
    public ControlsScript target;
    public bool lockXMotion = false;
    public bool lockYMotion = false;
    public bool lockZMotion = false;
    public UFE3D.CharacterInfo myInfo;
    public int mirror;
    public Fix64 normalizedDistance;
    public Fix64 normalizedJumpArc;
    public bool outroPlayed;
    public bool potentialBlock;
    public Fix64 potentialParry;
    public bool roundMsgCasted;
    public int roundsWon;
    public bool shakeCamera;
    public bool shakeCharacter;
    public Fix64 shakeDensity;
    public Fix64 shakeCameraDensity;
    public StandUpOptions standUpOverride;
    public Fix64 standardYRotation;
    public Fix64 storedMoveTime;
    public Fix64 stunTime;
    public Fix64 totalDrain;

    public PullIn activePullIn;
    public Hit currentHit;
    public MoveInfo currentMove;
    public MoveInfo storedMove;

    public PhysicsScript Physics { get { return this.myPhysicsScript; } set { myPhysicsScript = value; } }
    public MoveSetScript MoveSet { get { return this.myMoveSetScript; } set { myMoveSetScript = value; } }
    public HitBoxesScript HitBoxes { get { return this.myHitBoxesScript; } set { myHitBoxesScript = value; } }

    public Dictionary<ButtonPress, Fix64> inputHeldDown = new Dictionary<ButtonPress, Fix64>();
    public List<ProjectileMoveScript> projectiles = new List<ProjectileMoveScript>();
    public List<ControlsScript> assists = new List<ControlsScript>();

    public FPTransform worldTransform;
    public FPTransform localTransform;
    #endregion


    public Shader[] normalShaders;
    public Color[] normalColors;

    public HeadLookScript headLookScript;
	public GameObject emulatedCam;
	public CameraScript cameraScript;

    public Text debugger;
    public string aiDebugger { get; set; }
    public CharacterDebugInfo debugInfo;
    public int playerNum;
    public bool isAlt;
    public int selectedCostume = 0;

    [HideInInspector] public GameObject character;
    [HideInInspector] public UFE3D.CharacterInfo opInfo;
    [HideInInspector] public ChallengeMode challengeMode;
    [HideInInspector] public ControlsScript opControlsScript;
    [HideInInspector] public MoveSetData[] loadedMoves;
    [HideInInspector] public ControlsScript owner { get { return isAssist? _owner : this; } set { _owner = value; } }


    private PhysicsScript myPhysicsScript;
    private MoveSetScript myMoveSetScript;
    private HitBoxesScript myHitBoxesScript;
    public SpriteRenderer mySpriteRenderer;
    private ControlsScript _owner;

    public void Init() {
        // Set Input Recording
        foreach (ButtonPress bp in System.Enum.GetValues(typeof(ButtonPress))) {
            inputHeldDown.Add(bp, 0);
        }

        // Set Alternative Costume
        if (!isAssist)
        {
            if (isAlt)
            {
                if (myInfo.alternativeCostumes[selectedCostume].enableColorMask)
                {
                    Renderer[] charRenders = character.GetComponentsInChildren<Renderer>();
                    foreach (Renderer charRender in charRenders)
                    {
                        charRender.material.color = myInfo.alternativeCostumes[selectedCostume].colorMask;
                    }
                }
            }

            Renderer[] charRenderers = character.GetComponentsInChildren<Renderer>();
            List<Shader> shaderList = new List<Shader>();
            List<Color> colorList = new List<Color>();
            foreach (Renderer char_rend in charRenderers)
            {
                shaderList.Add(char_rend.material.shader);
                if (char_rend.material.HasProperty("_Color")) colorList.Add(char_rend.material.color);
            }
            normalShaders = shaderList.ToArray();
            normalColors = colorList.ToArray();
        }


        // Head Movement
        if (myInfo.headLook.enabled) {
			character.AddComponent<HeadLookScript>();
			headLookScript = character.GetComponent<HeadLookScript>();
			headLookScript.segments = myInfo.headLook.segments;
			headLookScript.nonAffectedJoints = myInfo.headLook.nonAffectedJoints;
			headLookScript.effect = myInfo.headLook.effect;
			headLookScript.overrideAnimation = !myInfo.headLook.overrideAnimation;
			
			foreach(BendingSegment segment in headLookScript.segments) {
				segment.firstTransform = myHitBoxesScript.GetTransform(segment.bodyPart).parent.transform;
				segment.lastTransform = myHitBoxesScript.GetTransform(segment.bodyPart);
			}
			
			foreach(NonAffectedJoints nonAffectedJoint in headLookScript.nonAffectedJoints) 
				nonAffectedJoint.joint = myHitBoxesScript.GetTransform(nonAffectedJoint.bodyPart);
		}


        // Challenge Mode
        if (!isAssist && playerNum == 1 && UFE.gameMode == GameMode.ChallengeMode) {
            challengeMode = gameObject.AddComponent<ChallengeMode>();
            challengeMode.cScript = this;
        }


        // Rotate or Lock-on target
        target = opControlsScript;
#if !UFE_LITE && !UFE_BASIC
        if (UFE.config.gameplayType == GameplayType._3DFighter)
        {
            validateRotation(true);

            if (playerNum == 2)
            {
                standardYRotation = -standardYRotation;
                if (UFE.config.characterRotationOptions.autoMirror)
                {
                    ForceMirror(true);
                    myHitBoxesScript.inverted = true;
                }
            }
        }
        /*else if (UFE.config.gameplayType == GameplayType._3DArena)
        {
            target = null;

            if (playerNum == 1)
                worldTransform.rotation = FPQuaternion.Euler(UFE.config.roundOptions._p1XRotation);
            else if (playerNum == 2)
                worldTransform.rotation = FPQuaternion.Euler(UFE.config.roundOptions._p2XRotation);
        }*/
        else if (playerNum == 2)
        {
            testCharacterRotation(0, true);
        }
#else
        if (playerNum == 2)
        {
            testCharacterRotation(0, true);
        }
#endif


        // Play Idle Animation
        myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.idle);

    }

	public void ForceMirror(bool toggle)
    {
        //if (UFE.config.characterRotationOptions.alwaysLookAtOpponent) return;
        if (myInfo.animationType == AnimationType.Legacy)
        {
            scaleFlip(toggle);
        }
        else if (myInfo.animationType == AnimationType.Mecanim2D)
        {
            if (myInfo.useScaleFlip)
            {
                scaleFlip(toggle);
            }
            else
            {
                if (myMoveSetScript.SpriteRenderer != null) myMoveSetScript.SpriteRenderer.flipX = toggle;
            }
        }
        else
        {
            if (myInfo.useScaleFlip)
            {
                scaleFlip(toggle);
            }
            else
            {
                myMoveSetScript.SetMecanimMirror(toggle);
                if (!myInfo.useAnimationMaps) myHitBoxesScript.InvertHitBoxes(toggle);
            }
        }
    }

    private void scaleFlip(bool toggle)
    {
        float xScale = Mathf.Abs(character.transform.localScale.x) * (toggle ? -1 : 1);
        character.transform.localScale = new Vector3(xScale, character.transform.localScale.y, character.transform.localScale.z);
    }

    public void InvertRotation(int newMirror = 0){
        mirror = newMirror != 0? newMirror : mirror *= -1;
        standardYRotation = FPMath.Abs(standardYRotation) * -mirror;
	}

    private void testCharacterRotation(){
        testCharacterRotation(0, false);
    }

	private void testCharacterRotation(Fix64 rotationSpeed, bool forceMirror = false)
    {
        Fix64 myX = worldTransform.position.x;
        Fix64 opX = opControlsScript.worldTransform.position.x;

#if !UFE_LITE && !UFE_BASIC
        if (UFE.config.gameplayType == GameplayType._3DFighter && IsSideSwitchEnabled())
        {
            myX = Camera.main.transform.InverseTransformDirection(worldTransform.position.ToVector() - Camera.main.transform.position).x;
            opX = Camera.main.transform.InverseTransformDirection(opControlsScript.worldTransform.position.ToVector() - Camera.main.transform.position).x;
        }
        else if (UFE.config.gameplayType != GameplayType._2DFighter) 
        {
            return;
        }
#endif

        if ((mirror == -1 || forceMirror) && myX > opX) {
            potentialBlock = false;
			InvertRotation(1);
            if (UFE.config.characterRotationOptions.autoMirror) ForceMirror(true);
            myHitBoxesScript.inverted = true;
            UFE.FireSideSwitch(mirror, this);

        } else if ((mirror == 1 || forceMirror) && myX < opX) {
			potentialBlock = false;
            InvertRotation(-1);
            if (UFE.config.characterRotationOptions.autoMirror) ForceMirror(false);
            myHitBoxesScript.inverted = false;
            UFE.FireSideSwitch(mirror, this);
		}

        if (rotationSpeed == 0 || UFE.config.characterRotationOptions.smoothRotation || 
            (UFE.config.networkOptions.disableRotationBlend && (UFE.isConnected || UFE.config.debugOptions.emulateNetwork))) {
            fixCharacterRotation();

        } else {
            FPQuaternion newRotation = FPQuaternion.Slerp(
                localTransform.rotation,
                FPQuaternion.AngleAxis(standardYRotation, FPVector.up),
                UFE.fixedDeltaTime * rotationSpeed
            );

            if (newRotation.ToString() != new FPQuaternion(0, 0, 0, 0).ToString()) localTransform.rotation = newRotation;
        }
	}
	
	private void fixCharacterRotation(){
		if (currentState == PossibleStates.Down) return;

        FPQuaternion fixedRotation = FPQuaternion.AngleAxis(standardYRotation, FPVector.up);
        localTransform.rotation = fixedRotation;
	}

	private void validateRotation(bool byPass = false){
		if (!myPhysicsScript.IsGrounded() || myPhysicsScript.freeze || currentMove != null) fixCharacterRotation();

        if (!byPass)
        {
            if (myPhysicsScript.freeze) return;
            if (currentState == PossibleStates.Down) return;
            if (currentMove != null) return;
            if (myPhysicsScript.IsJumping() && !UFE.config.characterRotationOptions.rotateWhileJumping) return;
            if (currentSubState == SubStates.Stunned && !UFE.config.characterRotationOptions.fixRotationWhenStunned) return;
            if (isBlocking && !UFE.config.characterRotationOptions.fixRotationWhenBlocking) return;
            if (UFE.config.characterRotationOptions.rotateOnMoveOnly && myMoveSetScript.IsBasicMovePlaying(myMoveSetScript.basicMoves.idle)) return;
        }

        if (UFE.config.gameplayType == GameplayType._2DFighter)
        {
            testCharacterRotation(UFE.config.characterRotationOptions._rotationSpeed);
        }
#if !UFE_LITE && !UFE_BASIC
        else
        {
            LookAtTarget();
            if (UFE.config.gameplayType == GameplayType._3DFighter && IsSideSwitchEnabled()) 
                testCharacterRotation(UFE.config.characterRotationOptions._rotationSpeed);
        }
#endif
    }

    private bool IsSideSwitchEnabled()
    {
        if (UFE.config.characterRotationOptions.allowAirBorneSideSwitch && (Physics.IsGrounded() || opControlsScript.Physics.IsGrounded())) return true;
        return false;
    }

    public void LookAtTarget(ControlsScript newTarget, bool save = false) {
        target = newTarget;
        LookAtTarget();
        if (!save) target = null;
    }

    public void LookAtTarget()
    {
        if (target == null) return;
        FPVector lookPos = target.worldTransform.position;
        lookPos.y = worldTransform.position.y;
        worldTransform.LookAt(lookPos);
    }

	public void DoFixedUpdate(
		IDictionary<InputReferences, InputEvents> previousInputs,
		IDictionary<InputReferences, InputEvents> currentInputs
	)
    {
        // Update opControlsScript Reference if Needed
        if (!opControlsScript.gameObject.activeInHierarchy) opControlsScript = UFE.GetControlsScript(playerNum == 1 ? 2 : 1);

        // Apply Training / Challenge Mode Options
        if (!isAssist)
        {
            if ((UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.ChallengeMode)
                && ((playerNum == 1 && UFE.config.trainingModeOptions.p1Life == LifeBarTrainingMode.Refill)
                || (playerNum == 2 && UFE.config.trainingModeOptions.p2Life == LifeBarTrainingMode.Refill)))
            {
                if (!UFE.FindDelaySynchronizedAction(this.RefillLife))
                    UFE.DelaySynchronizedAction(this.RefillLife, UFE.config.trainingModeOptions.refillTime);
            }

            if ((UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.ChallengeMode)
                && ((playerNum == 1 && UFE.config.trainingModeOptions.p1Gauge == LifeBarTrainingMode.Refill)
                || (playerNum == 2 && UFE.config.trainingModeOptions.p2Gauge == LifeBarTrainingMode.Refill)))
            {
                if (!UFE.FindDelaySynchronizedAction(this.RefillGauge))
                    UFE.DelaySynchronizedAction(this.RefillGauge, UFE.config.trainingModeOptions.refillTime);
            }

            if ((UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.ChallengeMode)
                && currentGaugesPoints[0] < myInfo.maxGaugePoints
                && ((playerNum == 1 && UFE.config.trainingModeOptions.p1Gauge == LifeBarTrainingMode.Infinite)
                || (playerNum == 2 && UFE.config.trainingModeOptions.p2Gauge == LifeBarTrainingMode.Infinite))) RefillGauge();

            if ((UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.ChallengeMode)
                && currentLifePoints < myInfo.lifePoints
                && ((playerNum == 1 && UFE.config.trainingModeOptions.p1Life == LifeBarTrainingMode.Infinite)
                || (playerNum == 2 && UFE.config.trainingModeOptions.p2Life == LifeBarTrainingMode.Infinite))) RefillLife();
        }


        //Update Hitboxes Position Map
        //myHitBoxesScript.UpdateMap(myMoveSetScript.GetCurrentClipFrame(myHitBoxesScript.bakeSpeed));


        // Resolve move
        resolveMove();


		// Check inputs
		if (!isAssist && (UFE.p1ControlsScript == this || UFE.p2ControlsScript == this)) translateInputs(previousInputs, currentInputs);


        // Gauge Drain
        if (gaugeDPS != 0)
        {
            owner.currentGaugesPoints[(int)gaugeDrainId] -= (owner.myInfo.maxGaugePoints * (gaugeDPS / 100) / UFE.config.fps) * UFE.timeScale;
            currentDrained += (gaugeDPS / UFE.config.fps) * UFE.timeScale;
            if (totalDrain != 0 && (owner.currentGaugesPoints[(int)gaugeDrainId] <= 0 || currentDrained >= totalDrain))
            {
                ResetDrainStatus(false, (int)gaugeDrainId);
            }
        }


        // Input Viewer
        string inputDebugger = "";
        if (!isAssist && (UFE.p1ControlsScript == this || UFE.p2ControlsScript == this))
        {
            List<InputReferences> inputList = new List<InputReferences>();
            Texture2D lastIconAdded = null;
            foreach (InputReferences inputRef in currentInputs.Keys)
            {
                if (debugger != null && UFE.config.debugOptions.debugMode && debugInfo.inputs)
                {
                    inputDebugger += inputRef.inputButtonName + " - " + inputHeldDown[inputRef.engineRelatedButton] + " (" + currentInputs[inputRef].axisRaw + ")\n";
                }
                if (inputHeldDown[inputRef.engineRelatedButton] == UFE.fixedDeltaTime)
                {
                    if (lastIconAdded != inputRef.activeIcon)
                    {
                        inputList.Add(inputRef);
                        UFE.FireButton(inputRef.engineRelatedButton, this);
                        lastIconAdded = inputRef.activeIcon;
                    }
                }
            }
            UFE.CastInput(inputList.ToArray(), playerNum);
        }


        // Apply Root Motion
        if (applyRootMotion)
        {
            FPVector newPosition = worldTransform.position;
            Fix64 speedModifier = myMoveSetScript.animationPaused ? myMoveSetScript.GetNormalizedSpeed() : 1;
            if (UFE.config.gameplayType == GameplayType._2DFighter)
            {
                if (!lockXMotion) newPosition.x += myHitBoxesScript.GetDeltaPosition().x * speedModifier * UFE.timeScale;
                if (!lockYMotion) newPosition.y += myHitBoxesScript.GetDeltaPosition().y * speedModifier * UFE.timeScale;
                if (!lockZMotion) newPosition.z += myHitBoxesScript.GetDeltaPosition().z * speedModifier * UFE.timeScale;
            }
#if !UFE_LITE && !UFE_BASIC
            else
            {
                FPVector newDelta = myHitBoxesScript.GetDeltaPosition() * speedModifier * UFE.timeScale;
                newDelta.x *= -mirror;
                newPosition = (FPQuaternion.Euler(0, transform.rotation.eulerAngles.y - 90, 0) * newDelta) + worldTransform.position;
            }
#endif

            worldTransform.position = newPosition;
        }
        else
        {
            if (UFE.config.lockZAxis && !isAssist)
            {
                FPVector newPosition = worldTransform.position;
                newPosition.z = 0;
                worldTransform.position = newPosition;
            }
            localTransform.position = new FPVector(0, 0, 0);
        }


        // Force stand state
        if (!myPhysicsScript.freeze
			&& !isDead
            && currentSubState != SubStates.Stunned
			&& introPlayed
			&& myPhysicsScript.IsGrounded()
			&& !myPhysicsScript.IsMoving()
		    && currentMove == null
            && !myMoveSetScript.IsBasicMovePlaying(myMoveSetScript.basicMoves.idle)
			&& !myMoveSetScript.IsAnimationPlaying("fallStraight")
			&& !myPhysicsScript.isTakingOff
			&& !myPhysicsScript.isLanding
			&& !blockStunned
			&& currentState != PossibleStates.Crouch
			&& !isBlocking
		    )
        {
            if (myMoveSetScript.IsAnimationPlaying("crouching")) {
                myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.crouching, "crouching_3", false);
                myMoveSetScript.OverrideWrapMode(WrapMode.ClampForever);
            }

            if (!myMoveSetScript.IsAnimationPlaying("crouching_3") || myMoveSetScript.AnimationTimesPlayed("crouching_3") >= 1)
                myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.idle);

			currentState = PossibleStates.Stand;
			currentSubState = SubStates.Resting;
			if (UFE.config.blockOptions.blockType == BlockType.AutoBlock 
                && myMoveSetScript.basicMoves.blockEnabled) potentialBlock = true;
		}


        // Play resting clip when idle for too long
        if (myMoveSetScript.IsAnimationPlaying("idle")
            && !UFE.config.lockInputs 
		    && !UFE.config.lockMovements
            && !myPhysicsScript.freeze) {
            afkTimer += UFE.fixedDeltaTime;
            if (afkTimer >= myMoveSetScript.basicMoves.idle._restingClipInterval) {
                afkTimer = 0;
                int clipNum = FPRandom.Range(2, 6);
                if (myMoveSetScript.AnimationExists("idle_" + clipNum)) {
                    myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.idle, "idle_" + clipNum, false);
                }
            }
        } else {
            afkTimer = 0;
        }


        // Shake character
        if (shakeDensity > 0) {
			shakeDensity -= UFE.fixedDeltaTime;
			if (myHitBoxesScript.isHit && myPhysicsScript.freeze){
				if (shakeCharacter) shake();
			}
		}else if (shakeDensity < 0) {
			shakeDensity = 0;
			shakeCharacter = false;
		}


        // Shake camera
        if (!isAssist)
        {
            if (shakeCameraDensity > 0) {
                shakeCameraDensity -= UFE.fixedDeltaTime * 3;
                if (shakeCamera) shakeCam();
                if (UFE.config.groundBounceOptions.shakeCamOnBounce && myPhysicsScript.isGroundBouncing) shakeCam();
                if (UFE.config.wallBounceOptions.shakeCamOnBounce && myPhysicsScript.isWallBouncing) shakeCam();
            } else if (shakeCameraDensity < 0) {
                shakeCameraDensity = 0;
                shakeCamera = false;
            }
        }


        // Validate Parry
        if (!isAssist && potentialParry > 0) {
            potentialParry -= UFE.fixedDeltaTime;
            if (potentialParry <= 0) potentialParry = 0;
        }


        // Update head movement
        if (!myPhysicsScript.freeze && !cameraScript.cinematicFreeze && headLookScript != null && opControlsScript.HitBoxes != null) {
            headLookScript.target = opControlsScript.HitBoxes.GetTransformPosition(myInfo.headLook.target);
            headLookScript.DoFixedUpdate();
        }


        // Run UI Debugger
        runDebugger(inputDebugger);


        // Execute Move
        if (currentMove != null)
        {
            ReadMove(currentMove);
        }
        else
        {
            target = opControlsScript;
            //if (UFE.config.gameplayType == GameplayType._3DArena) target = null;
        }


        // Validate rotation
        validateRotation();


        // Apply Stun
        if ((currentSubState == SubStates.Stunned || blockStunned) && stunTime > 0 && !myPhysicsScript.freeze && !isDead) {
            ApplyStun(previousInputs, currentInputs);
        }


        // Apply Forces
        if (GetActive()) myPhysicsScript.ApplyForces(currentMove);


        // Character colliders based on collision mass and body colliders
        normalizedDistance = FPMath.Clamp(FPVector.Distance(opControlsScript.worldTransform.position, worldTransform.position) / UFE.config.cameraOptions._maxDistance, 0, 1);
        if (UFE.config.selectedMatchType != MatchType.Singles)
        {
            foreach (ControlsScript cScript in UFE.GetControlsScriptTeam(opControlsScript.playerNum))
            {
                pushOpponentsAway(cScript, currentInputs);
                if (!isAssist) foreach (ControlsScript assist in cScript.assists) pushOpponentsAway(assist, currentInputs);
            }
        }
        else
        {
            pushOpponentsAway(opControlsScript, currentInputs);
            if (!isAssist) foreach (ControlsScript assist in opControlsScript.assists) pushOpponentsAway(assist, currentInputs);
        }


        // Intro and Enter Moves
        if (!introPlayed && (isAssist || myMoveSetScript.intro == null))
        {
            introPlayed = true;
            if (!isAssist && playerNum == 2 && opControlsScript.introPlayed) {
                UFE.CastNewRound(2);
            }else if (isAssist && enterMove != null) {
                CastMove(enterMove, true);
                enterMove = null;
            }
        } else if (currentMove == null && !introPlayed) {
            if (UFE.config.roundOptions.playIntrosAtSameTime) {
                CastMove(myMoveSetScript.intro, true, true, false);
            } else if (playerNum == 1 || opControlsScript.introPlayed) {
                CastMove(myMoveSetScript.intro, true, true, false);
            }
        }



        // Assist - Play Exit Move after Enter Move
        if (stunTime == 0 && currentState == PossibleStates.Stand && currentMove == null && enterMove == null && exitMove != null) {
            CastMove(exitMove, true);
        }


        // Test Current Challenge
        if (!isAssist && challengeMode != null && challengeMode.complete) {
            UFE.FireAlert("Success", this); // TODO
            if (challengeMode.moveToNext) {
                UFE.DelaySynchronizedAction(this.StartNextChallenge, .6);
            } else {
                UFE.DelaySynchronizedAction(UFE.fluxCapacitor.EndRound, (Fix64)5);
            }
            challengeMode.Stop();
        }


        // Update Unity Transforms with Fixed Point Transforms
        transform.position = worldTransform.position.ToVector();
        character.transform.localPosition = localTransform.position.ToVector();

        if (UFE.config.gameplayType == GameplayType._2DFighter)
        {
            character.transform.rotation = localTransform.rotation.ToQuaternion();
        }
        else
        {
            transform.rotation = worldTransform.rotation.ToQuaternion();
        }

        //Update Hitboxes Position Map
        //myHitBoxesScript.UpdateMap(myMoveSetScript.GetCurrentClipFrame(myHitBoxesScript.bakeSpeed));
    }


    public bool IsAxisRested(IDictionary<InputReferences, InputEvents> currentInputs)
    {
        if (currentState == PossibleStates.Down) return true;
        if (UFE.config.lockMovements) return true;
        if (UFE.config.lockInputs) return true;
        if (myMoveSetScript.IsBasicMovePlaying(myMoveSetScript.basicMoves.idle)) return true;
        foreach (InputReferences inputRef in currentInputs.Keys)
        {
            if (inputRef.inputType == InputType.Button) continue;
            if (currentInputs[inputRef].axisRaw != 0) return false;
        }
        return true;
    }

    private void runDebugger(string inputDebugger)
    {
        // Run Debugger
        if (!isAssist && debugger != null && UFE.config.debugOptions.debugMode)
        {
            debugger.text = "";
            if (UFE.config.debugOptions.debugMode &&
                (!UFE.config.debugOptions.trainingModeDebugger || UFE.gameMode == GameMode.TrainingRoom))
            {
                debugger.text += "FPS: " + (1.0f / UFE.fixedDeltaTime) + "\n";
                debugger.text += "-----Character Info-----\n";
                if (debugInfo.lifePoints) debugger.text += "Life Points: " + currentLifePoints + "\n";
                if (debugInfo.gaugePoints) debugger.text += "Gauge Points: " + currentGaugesPoints[0] + "\n";
                if (debugInfo.position) debugger.text += "Position: " + worldTransform.position + "\n";
                if (debugInfo.currentState) debugger.text += "State: " + currentState + "\n";
                if (debugInfo.currentSubState) debugger.text += "Sub State: " + currentSubState + "\n";
                if (debugInfo.currentState) debugger.text += "Potential Block: " + potentialBlock + "\n";
                if (debugInfo.currentState) debugger.text += "Taking Off: " + myPhysicsScript.isTakingOff + "\n";
                if (debugInfo.currentState) debugger.text += "Is Blocking: " + isBlocking + "\n";
                if (debugInfo.currentState) debugger.text += "Is Crouching: " + isCrouching + "\n";
                if (debugInfo.stunTime && stunTime > 0) debugger.text += "Stun Time: " + stunTime + "\n";
                if (opControlsScript != null && opControlsScript.comboHits > 0)
                {
                    debugger.text += "Current Combo\n";
                    if (debugInfo.comboHits) debugger.text += "- Total Hits: " + opControlsScript.comboHits + "\n";
                    if (debugInfo.comboDamage)
                    {
                        debugger.text += "- Total Damage: " + opControlsScript.comboDamage + "\n";
                        debugger.text += "- Hit Damage: " + opControlsScript.comboHitDamage + "\n";
                    }
                }

                // Other uses
                if (potentialParry > 0) debugger.text += "Parry Window: " + potentialParry + "\n";
                //debugger.text += "Air Jumps: "+ myPhysicsScript.currentAirJumps + "\n";
                //debugger.text += "Horizontal Force: "+ myPhysicsScript.horizontalForce + "\n";
                //debugger.text += "Vertical Force: "+ myPhysicsScript.verticalForce + "\n";

                if (UFE.config.debugOptions.p1DebugInfo.currentMove && currentMove != null)
                {
                    debugger.text += "-----Move Info-----\n";
                    debugger.text += "Move: " + currentMove.name + "\n";
                    debugger.text += "Frames: " + currentMove.currentFrame + "/" + (currentMove.totalFrames -1) + "\n";
                    debugger.text += "Tick: " + currentMove.currentTick.ToString() + "\n";
                    debugger.text += "Animation Speed: " + myMoveSetScript.GetAnimationSpeed() + "\n";
                }
            }
            if (inputDebugger != "") debugger.text += inputDebugger;
            if (aiDebugger != null && debugInfo.aiWeightList) debugger.text += aiDebugger;
        }
    }

    private void pushOpponentsAway(ControlsScript opControlsScript, IDictionary<InputReferences, InputEvents> currentInputs)
    {
        if (opControlsScript == null
            || !opControlsScript.GetActive()
            || opControlsScript.HitBoxes == null
            || ignoreCollisionMass
            || opControlsScript.ignoreCollisionMass) return;


        // Set target in case its a 3D fighter
        FPVector target;
        if (!myPhysicsScript.IsMoving() || IsAxisRested(currentInputs))
            target = opControlsScript.worldTransform.position + opControlsScript.worldTransform.forward * -1;
        else
            target = worldTransform.position + worldTransform.forward * -1;


        // Test collision between hitboxes
        Fix64 pushForce = CollisionManager.TestCollision(myHitBoxesScript.hitBoxes, opControlsScript.HitBoxes.hitBoxes, false, false);
        if (pushForce > 0)
        {
            if (UFE.config.gameplayType == GameplayType._2DFighter)
            {
                if (worldTransform.position.x < opControlsScript.worldTransform.position.x)
                {
                    worldTransform.Translate(new FPVector(.1 * -pushForce, 0, 0));
                }
                else if (worldTransform.position.x > UFE.config.selectedStage._leftBoundary)
                {
                    worldTransform.Translate(new FPVector(.1 * pushForce, 0, 0));
                }

                if (opControlsScript.worldTransform.position.x >= UFE.config.selectedStage._rightBoundary)
                    opControlsScript.worldTransform.Translate(new FPVector(.1 * -pushForce, 0, 0));
                else if (opControlsScript.worldTransform.position.x <= UFE.config.selectedStage._leftBoundary)
                    opControlsScript.worldTransform.Translate(new FPVector(.1 * pushForce, 0, 0));
            }
#if !UFE_LITE && !UFE_BASIC
            else
            {
                if (!myPhysicsScript.IsMoving() || IsAxisRested(currentInputs)) pushForce *= -1;
                worldTransform.position = FPVector.MoveTowards(worldTransform.position, target, .1 * pushForce);
            }
#endif
        }

        pushForce = myInfo.physics._groundCollisionMass - FPVector.Distance(opControlsScript.worldTransform.position, worldTransform.position);
        if (pushForce > 0)
        {
            if (UFE.config.gameplayType == GameplayType._2DFighter)
            {
                if (worldTransform.position.x < opControlsScript.worldTransform.position.x)
                {
                    worldTransform.Translate(new FPVector(.5 * -pushForce, 0, 0));
                }
                else if (worldTransform.position.x > UFE.config.selectedStage._leftBoundary)
                {
                    worldTransform.Translate(new FPVector(.5 * pushForce, 0, 0));
                }

                if (opControlsScript.worldTransform.position.x >= UFE.config.selectedStage._rightBoundary)
                    opControlsScript.worldTransform.Translate(new FPVector(.5 * -pushForce, 0, 0));
                else if (opControlsScript.worldTransform.position.x <= UFE.config.selectedStage._leftBoundary)
                    opControlsScript.worldTransform.Translate(new FPVector(.5 * pushForce, 0, 0));
            }
            else
            {
                if (!myPhysicsScript.IsMoving() || IsAxisRested(currentInputs)) pushForce *= -1;
                worldTransform.position = FPVector.MoveTowards(worldTransform.position, target, .5 * pushForce);
            }
        }
    }

    private bool testMoveExecution(ButtonPress buttonPress){
		return testMoveExecution(new ButtonPress[]{buttonPress});
	}
	
	private bool testMoveExecution(ButtonPress[] buttonPresses){
        MoveInfo tempMove = myMoveSetScript.GetMove(buttonPresses, 0, currentMove, false);
        if (tempMove != null) {
            storedMove = tempMove;
			storedMoveTime = (UFE.config.executionBufferTime / (Fix64)UFE.config.fps);
			return true;
		}
		return false;
	}
	
	private void resolveMove(){
		if (myPhysicsScript.freeze) return;
        if (storedMoveTime > 0) storedMoveTime -= UFE.fixedDeltaTime;
		if (storedMoveTime <= 0 && storedMove != null){
			storedMoveTime = 0;
			if (UFE.config.executionBufferType != ExecutionBufferType.NoBuffer) storedMove = null;
		}

        if (currentMove != null && !opControlsScript.isDead)
            myMoveSetScript.GetNextMove(currentMove, ref storedMove);

        if (storedMove != null && (currentMove == null || myMoveSetScript.SearchMove(storedMove.moveName, currentMove.frameLinks))) {
			bool confirmQueue = false;
			bool ignoreConditions = false;
            if (currentMove != null && UFE.config.executionBufferType == ExecutionBufferType.OnlyMoveLinks) {
                foreach (FrameLink frameLink in currentMove.frameLinks) {
                    if (frameLink.cancelable) {
                        confirmQueue = true;
                    }

                    if (frameLink.ignorePlayerConditions) {
                        ignoreConditions = true;
                    }

                    if (confirmQueue) {
                        foreach (MoveInfo move in frameLink.linkableMoves) {
                            if (move == null) Debug.LogError("Null reference found under move " + currentMove.name + "'s chain moves.");

                            if (storedMove.name == move.name) {
                                storedMove.overrideStartupFrame = frameLink.nextMoveStartupFrame - 1;
                            }
                        }
                    }
                }
            } else if (UFE.config.executionBufferType == ExecutionBufferType.AnyMove
                      || (currentMove == null
                          && storedMoveTime >= ((UFE.config.executionBufferTime - 2) / (Fix64)UFE.config.fps))) {
				confirmQueue = true;
            }

			if (confirmQueue && (ignoreConditions || myMoveSetScript.ValidateMoveStances(storedMove.selfConditions, this))) {
				KillCurrentMove();
                this.SetMove(storedMove);

                storedMove = null;
                storedMoveTime = 0;
			}
		}
	}

	private void translateInputs(
		IDictionary<InputReferences, InputEvents> previousInputs,
		IDictionary<InputReferences, InputEvents> currentInputs
	){
		if (!introPlayed || !opControlsScript.introPlayed) return;
		if (UFE.config.lockInputs && !UFE.config.roundOptions.allowMovementStart) return;
		if (UFE.config.lockMovements) return;

        FPVector lookDirection = FPVector.zero;
        foreach (InputReferences inputRef in currentInputs.Keys) {
			InputEvents ev = currentInputs[inputRef];

            /*if (((inputRef.engineRelatedButton == ButtonPress.Down && ev.axisRaw >= 0)
				|| (inputRef.engineRelatedButton == ButtonPress.Up && ev.axisRaw <= 0))
			    && myPhysicsScript.IsGrounded() 
			    && !myHitBoxesScript.isHit 
			    && currentSubState != SubStates.Stunned){
				currentState = PossibleStates.Stand;
			}*/

            if (myPhysicsScript.IsGrounded()
                && (myInfo.customControls.disableCrouch || inputHeldDown[myInfo.customControls.crouchButton] == 0)
                && !myHitBoxesScript.isHit
                && currentSubState != SubStates.Stunned)
            {
                currentState = PossibleStates.Stand;
            }

            // On Axis Release
            bool axisRelease = false;
			if (inputRef.inputType != InputType.Button && inputHeldDown[inputRef.engineRelatedButton] > 0 && ev.axisRaw == 0)
            {
                axisRelease = true;

                if (inputRef.engineRelatedButton == ButtonPress.Back && UFE.config.blockOptions.blockType == BlockType.HoldBack)
					potentialBlock = false;

                // Move Execution
                MoveInfo tempMove = myMoveSetScript.GetMove(new ButtonPress[] { inputRef.engineRelatedButton }, inputHeldDown[inputRef.engineRelatedButton], currentMove, true);

                // Pressure Sensitive Jump
                if (!myInfo.customControls.disableJump && tempMove == null && myInfo.physics.pressureSensitiveJump && inputRef.engineRelatedButton == myInfo.customControls.jumpButton)
                    ExecuteJump(inputRef.engineRelatedButton, true);

                inputHeldDown[inputRef.engineRelatedButton] = 0;
                if (tempMove != null) {
                    storedMove = tempMove;
					storedMoveTime = ((Fix64)UFE.config.executionBufferTime / UFE.config.fps);
					return;
				}

                // Cast Axis to input viewer if diagonal was released
                foreach (InputReferences inputRef2 in currentInputs.Keys)
                {
                    InputEvents ev2 = currentInputs[inputRef2];
                    if (ev2.axisRaw != 0 && inputHeldDown[inputRef2.engineRelatedButton] > UFE.fixedDeltaTime && inputRef2.inputType != InputType.Button && inputRef.inputType != inputRef2.inputType)
                    {
                        inputRef2.activeIcon = ev2.axisRaw > 0 ? inputRef2.inputViewerIcon1 : inputRef2.inputViewerIcon2;
                        UFE.CastInput(new InputReferences[] { inputRef2 }, playerNum);
                        break;
                    }
                }
            }

            // Set Active Icon for axis
			if (ev.axisRaw != 0 && inputRef.inputType != InputType.Button)
            {
                inputRef.activeIcon = ev.axisRaw > 0 ? inputRef.inputViewerIcon1 : inputRef.inputViewerIcon2;

                if (!axisRelease)
                {
                    foreach (InputReferences inputRef2 in currentInputs.Keys)
                    {
                        InputEvents ev2 = currentInputs[inputRef2];
                        if (ev2.axisRaw != 0 && inputRef2.inputType != InputType.Button && inputRef.inputType != inputRef2.inputType)
                        {
                            if (inputRef.inputType == InputType.VerticalAxis)
                            {
                                // Vertical POV - Pulls the references from the Horizontal definitions.
                                inputRef.activeIcon = ev.axisRaw > 0 ? (ev2.axisRaw > 0 ? inputRef2.inputViewerIcon3 : inputRef2.inputViewerIcon4) : (ev2.axisRaw > 0 ? inputRef2.inputViewerIcon5 : inputRef2.inputViewerIcon6);
                            }
                            else
                            {
                                // Horizontal POV
                                inputRef.activeIcon = ev.axisRaw > 0 ? (ev2.axisRaw > 0 ? inputRef.inputViewerIcon3 : inputRef.inputViewerIcon5) : (ev2.axisRaw > 0 ? inputRef.inputViewerIcon4 : inputRef.inputViewerIcon6);
                            }
                            break;
                        }
                    }
                }
            }
            else if (inputHeldDown[inputRef.engineRelatedButton] == 0)
            {
                inputRef.activeIcon = inputRef.inputViewerIcon1;
            }
			
			// On Axis Press
			if (inputRef.inputType != InputType.Button && ev.axisRaw != 0)
            {
                if (inputRef.inputType == InputType.HorizontalAxis)
                {
                    // Horizontal Movements
                    lookDirection.x = ev.axisRaw;

                    if (ev.axisRaw > 0) // Right
                    {
						if (mirror == 1){
                            inputHeldDown[ButtonPress.Forward] = 0;
                            inputRef.engineRelatedButton = ButtonPress.Back;
                        } else {
                            inputHeldDown[ButtonPress.Back] = 0;
                            inputRef.engineRelatedButton = ButtonPress.Forward;
                        }

						inputHeldDown[inputRef.engineRelatedButton] += UFE.fixedDeltaTime;
						if (inputHeldDown[inputRef.engineRelatedButton] == UFE.fixedDeltaTime && testMoveExecution(inputRef.engineRelatedButton)) return;

#if !UFE_LITE && !UFE_BASIC
                        if (/*UFE.config.gameplayType != GameplayType._3DArena &&*/ CanWalk())
                            myPhysicsScript.MoveX(-mirror, ev.axisRaw);
#else
                        if (CanWalk()) myPhysicsScript.MoveX(-mirror, ev.axisRaw);
#endif
                    }
                    else if (ev.axisRaw < 0) // Left
                    {
                        if (mirror == 1) {
                            inputHeldDown[ButtonPress.Back] = 0;
                            inputRef.engineRelatedButton = ButtonPress.Forward;
                        } else {
                            inputHeldDown[ButtonPress.Forward] = 0;
                            inputRef.engineRelatedButton = ButtonPress.Back;
                        }

						inputHeldDown[inputRef.engineRelatedButton] += UFE.fixedDeltaTime;
						if (inputHeldDown[inputRef.engineRelatedButton] == UFE.fixedDeltaTime && testMoveExecution(inputRef.engineRelatedButton)) return;

#if !UFE_LITE && !UFE_BASIC
                        if (/*UFE.config.gameplayType != GameplayType._3DArena &&*/ CanWalk())
                            myPhysicsScript.MoveX(mirror, ev.axisRaw);
#else
                        if (CanWalk()) myPhysicsScript.MoveX(mirror, ev.axisRaw);
#endif
                    }

                    // Check for potential blocking
                    if (inputRef.engineRelatedButton == ButtonPress.Back 
					    && UFE.config.blockOptions.blockType == BlockType.HoldBack
					    && !myPhysicsScript.isTakingOff
                        && myMoveSetScript.basicMoves.blockEnabled)
                    {
						potentialBlock = true;
					}
					
					// Check for potential parry
					if (((inputRef.engineRelatedButton == ButtonPress.Back && UFE.config.blockOptions.parryType == ParryType.TapBack) ||
					     (inputRef.engineRelatedButton == ButtonPress.Forward && UFE.config.blockOptions.parryType == ParryType.TapForward))
					    && (potentialParry == 0 || UFE.config.blockOptions.easyParry)
					    && inputHeldDown[inputRef.engineRelatedButton] == UFE.fixedDeltaTime
					    && currentMove == null
					    && !isBlocking 
					    && !myPhysicsScript.isTakingOff
					    && currentSubState != SubStates.Stunned
                        && !blockStunned
                        && myMoveSetScript.basicMoves.parryEnabled)
                    {
						potentialParry = UFE.config.blockOptions._parryTiming;
					}
				}
                else
                {
                    // Vertical Movements
                    lookDirection.z = ev.axisRaw;

                    if (ev.axisRaw > 0) // Up
                    {
                        inputRef.engineRelatedButton = ButtonPress.Up;
                        inputHeldDown[ButtonPress.Down] = 0;
                        inputHeldDown[inputRef.engineRelatedButton] += UFE.fixedDeltaTime;

                        if (inputHeldDown[inputRef.engineRelatedButton] == UFE.fixedDeltaTime && testMoveExecution(inputRef.engineRelatedButton)) return;

#if !UFE_LITE && !UFE_BASIC
                        if (UFE.config.gameplayType == GameplayType._3DFighter && myInfo.customControls.zAxisMovement && CanWalk())
                            if (inputRef.engineRelatedButton != myInfo.customControls.jumpButton || !myPhysicsScript.IsMoving())
                                myPhysicsScript.MoveZ(mirror, ev.axisRaw);
#endif
                    }
                    else if (ev.axisRaw < 0) // Down
                    { 
                        inputRef.engineRelatedButton = ButtonPress.Down;
                        inputHeldDown[ButtonPress.Up] = 0;
                        inputHeldDown[inputRef.engineRelatedButton] += UFE.fixedDeltaTime;

                        if (inputHeldDown[inputRef.engineRelatedButton] == UFE.fixedDeltaTime && testMoveExecution(inputRef.engineRelatedButton)) return;

#if !UFE_LITE && !UFE_BASIC
                        if (UFE.config.gameplayType == GameplayType._3DFighter && myInfo.customControls.zAxisMovement && CanWalk())
                            if (inputRef.engineRelatedButton != myInfo.customControls.jumpButton || !myPhysicsScript.IsMoving())
                                myPhysicsScript.MoveZ(mirror, ev.axisRaw);
#endif
                    }
				}

                // Diagonal Input Injection
				foreach (InputReferences inputRef2 in currentInputs.Keys) {
					InputEvents ev2 = currentInputs[inputRef2];
					InputEvents p2;
					if (!previousInputs.TryGetValue(inputRef2, out p2)){
						p2 = InputEvents.Default;
					}
					bool button2Down = ev2.button && !p2.button;

                    if (inputRef2 != inputRef && button2Down)
                    {
                        // Check if there is another axis being held
                        if (inputRef2.inputType != InputType.Button)
                        {
                            ButtonPress newInputRefValue = inputRef.engineRelatedButton;
                            if (inputRef2.inputType == InputType.HorizontalAxis)
                            {
                                ButtonPress b2Press = ButtonPress.Back;
                                if ((ev2.axisRaw > 0 && mirror == -1) || (ev2.axisRaw < 0 && mirror == 1))
                                {
                                    b2Press = ButtonPress.Forward;
                                }
                                else if ((ev2.axisRaw < 0 && mirror == -1) || (ev2.axisRaw > 0 && mirror == 1))
                                {
                                    b2Press = ButtonPress.Back;
                                }

                                if (inputRef.engineRelatedButton == ButtonPress.Down && b2Press == ButtonPress.Back)
                                {
                                    newInputRefValue = ButtonPress.DownBack;
                                }
                                else if (inputRef.engineRelatedButton == ButtonPress.Up && b2Press == ButtonPress.Back)
                                {
                                    newInputRefValue = ButtonPress.UpBack;
                                }
                                else if (inputRef.engineRelatedButton == ButtonPress.Down && b2Press == ButtonPress.Forward)
                                {
                                    newInputRefValue = ButtonPress.DownForward;
                                }
                                else if (inputRef.engineRelatedButton == ButtonPress.Up && b2Press == ButtonPress.Forward)
                                {
                                    newInputRefValue = ButtonPress.UpForward;
                                }
                            }
                            else if (inputRef2.inputType == InputType.VerticalAxis)
                            {
                                ButtonPress b2Press = ev2.axisRaw > 0 ? ButtonPress.Up : ButtonPress.Down;

                                if (inputRef.engineRelatedButton == ButtonPress.Back && b2Press == ButtonPress.Down)
                                {
                                    newInputRefValue = ButtonPress.DownBack;
                                }
                                else if (inputRef.engineRelatedButton == ButtonPress.Forward && b2Press == ButtonPress.Down)
                                {
                                    newInputRefValue = ButtonPress.DownForward;
                                }
                                else if (inputRef.engineRelatedButton == ButtonPress.Back && b2Press == ButtonPress.Up)
                                {
                                    newInputRefValue = ButtonPress.UpBack;
                                }
                                else if (inputRef.engineRelatedButton == ButtonPress.Forward && b2Press == ButtonPress.Up)
                                {
                                    newInputRefValue = ButtonPress.UpForward;
                                }
                            }

                            // If the value has changed, send the new axis input
                            if (newInputRefValue != inputRef.engineRelatedButton)
                            {
                                MoveInfo tempMove = myMoveSetScript.GetMove(
                                    new ButtonPress[] { newInputRefValue }, 0, currentMove, false, false);

                                if (tempMove != null)
                                {
                                    storedMove = tempMove;
                                    storedMoveTime = ((Fix64)UFE.config.executionBufferTime / UFE.config.fps);
                                    return;
                                }
                            }
                        }
                    }
				}

                // Test regular jump if axis is the same as jump button
                if (!myInfo.customControls.disableJump && inputRef.engineRelatedButton == myInfo.customControls.jumpButton)
                {
                    ExecuteJump(inputRef.engineRelatedButton, false);
                }
                // Test crouch if axis is the same as crouch button
                else if (!myInfo.customControls.disableCrouch && inputRef.engineRelatedButton == myInfo.customControls.crouchButton)
                {
                    ExecuteCrouch();
                }
            }

            // Button Press
            if (inputRef.inputType == InputType.Button && !UFE.config.lockInputs)
            {
				if (!previousInputs.TryGetValue(inputRef, out InputEvents p)){
					p = InputEvents.Default;
				}
				bool buttonDown = ev.button && !p.button;
				bool buttonUp = !ev.button && p.button;

				if (ev.button) {
					if (myMoveSetScript.CompareBlockButtons(inputRef.engineRelatedButton) 
					    && currentSubState != SubStates.Stunned 
					    && !myPhysicsScript.isTakingOff
					    && !blockStunned
                        && myMoveSetScript.basicMoves.blockEnabled) {
						potentialBlock = true;
						CheckBlocking(true);
					}

					if (myMoveSetScript.CompareParryButtons(inputRef.engineRelatedButton) 
					    && inputHeldDown[inputRef.engineRelatedButton] == 0 
					    && potentialParry == 0 
					    && currentMove == null 
					    && !isBlocking 
					    && currentSubState != SubStates.Stunned 
					    && !myPhysicsScript.isTakingOff
					    && !blockStunned
                        && myMoveSetScript.basicMoves.parryEnabled) {
						potentialParry = UFE.config.blockOptions._parryTiming;
					}
					
					inputHeldDown[inputRef.engineRelatedButton] += UFE.fixedDeltaTime;

                    // Plinking
                    if (buttonDown && inputHeldDown[inputRef.engineRelatedButton] <= ((Fix64)UFE.config.plinkingDelay / UFE.config.fps))
                    {
                        foreach (InputReferences inputRef2 in currentInputs.Keys)
                        {
                            InputEvents ev2 = currentInputs[inputRef2];
                            InputEvents p2;
                            if (!previousInputs.TryGetValue(inputRef2, out p2))
                            {
                                p2 = InputEvents.Default;
                            }
                            bool button2Down = ev2.button && !p2.button;

                            if (inputRef2 != inputRef && (inputRef2.inputType == InputType.Button && button2Down || inputHeldDown[inputRef2.engineRelatedButton] > 0))
                            {
                                inputHeldDown[inputRef2.engineRelatedButton] += UFE.fixedDeltaTime;
                                MoveInfo tempMove = myMoveSetScript.GetMove(
                                    new ButtonPress[] { inputRef.engineRelatedButton, inputRef2.engineRelatedButton }, 0, currentMove, false, (inputRef.inputType == InputType.Button && inputRef2.inputType == InputType.Button));

                                if (tempMove != null)
                                {
                                    if (myMoveSetScript.CanPlink(currentMove, tempMove))
                                        KillCurrentMove();

                                    storedMove = tempMove;
                                    storedMoveTime = (Fix64)UFE.config.executionBufferTime / UFE.config.fps;
                                }
                            }
                        }
                    }
				}


                if (buttonDown)
                {
                    MoveInfo tempMove = myMoveSetScript.GetMove(new ButtonPress[] { inputRef.engineRelatedButton }, 0, currentMove, false);
                    if (tempMove != null)
                    {
                        // If plinking occured and input sequence is higher then the plinked move, override it
                        if (storedMove == null || tempMove.defaultInputs.buttonSequence.Length > storedMove.defaultInputs.buttonSequence.Length)
                        {
                            storedMove = tempMove;
                            storedMoveTime = (UFE.config.executionBufferTime / (Fix64)UFE.config.fps);
                        }
                        return;
                    }

                    // Test regular jump if input type is button
                    if (!myInfo.customControls.disableJump && inputRef.engineRelatedButton == myInfo.customControls.jumpButton)
                    {
                        ExecuteJump(inputRef.engineRelatedButton, false);
                    }
                    // Test crouch if input type is button
                    else if (!myInfo.customControls.disableCrouch && inputRef.engineRelatedButton == myInfo.customControls.crouchButton)
                    {
                        ExecuteCrouch();
                    }
                }

                if (buttonUp)
                {
                    MoveInfo tempMove = myMoveSetScript.GetMove(new ButtonPress[] { inputRef.engineRelatedButton }, inputHeldDown[inputRef.engineRelatedButton], currentMove, true);

                    // Test pressure sensitive jump if input type is button
                    if (!myInfo.customControls.disableJump && tempMove == null && myInfo.physics.pressureSensitiveJump && inputRef.engineRelatedButton == myInfo.customControls.jumpButton)
                        ExecuteJump(inputRef.engineRelatedButton, true);

                    inputHeldDown[inputRef.engineRelatedButton] = 0;

                    if (tempMove != null) {
                        storedMove = tempMove;
						storedMoveTime = ((Fix64)UFE.config.executionBufferTime / UFE.config.fps);
						return;
					}

					if (myMoveSetScript.CompareBlockButtons(inputRef.engineRelatedButton) 
					    && !myPhysicsScript.isTakingOff) {
						potentialBlock = false;
						CheckBlocking(false);
					}
                }
            }
#if !UFE_LITE && !UFE_BASIC
            /*if (UFE.config.gameplayType == GameplayType._3DArena && CanWalk() && lookDirection != FPVector.zero)
            {
                FPQuaternion newRotation = FPQuaternion.LookRotation(lookDirection);
                if (UFE.config.characterRotationOptions.smoothRotation && UFE.config.characterRotationOptions._rotationSpeed > 0)
                {
                    FPQuaternion prevRotation = worldTransform.rotation;
                    worldTransform.rotation = FPQuaternion.Slerp(worldTransform.rotation, newRotation, UFE.config.characterRotationOptions._rotationSpeed / 100);

                    if (worldTransform.rotation.ToString() == new FPQuaternion(0, 0, 0, 0).ToString())
                        worldTransform.rotation = prevRotation;
                }
                else
                {
                    worldTransform.rotation = newRotation;
                }

                transform.rotation = worldTransform.rotation.ToQuaternion();
                myPhysicsScript.Move(FPMath.Abs(lookDirection.magnitude));
            }*/
#endif
        }
    }

    public bool CanWalk()
    {
        if (currentState == PossibleStates.Stand
            && !isBlocking
            && !myPhysicsScript.isTakingOff
            && !myPhysicsScript.isLanding
            && currentSubState != SubStates.Stunned
            && !blockStunned
            && currentMove == null
            && myMoveSetScript.basicMoves.moveEnabled)
            return true;
        return false;
    }

    public void ExecuteCrouch()
    {
        if (!myPhysicsScript.freeze
            && myPhysicsScript.IsGrounded()
            && currentMove == null
            && currentSubState != SubStates.Stunned
            && !myPhysicsScript.isTakingOff
            && !blockStunned
            && myMoveSetScript.basicMoves.crouchEnabled)
        {
            if (currentState != PossibleStates.Crouch)
                isCrouching = false;

            currentState = PossibleStates.Crouch;
            if (!isBlocking)
            {
                if (!isCrouching)
                {
                    isCrouching = true;
                    if (myMoveSetScript.AnimationExists("crouching_2"))
                    {
                        myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.crouching, "crouching_2", true);
                        myMoveSetScript.OverrideWrapMode(WrapMode.ClampForever);
                    }
                }

                if (myMoveSetScript.IsAnimationPlaying("blockingCrouchingPose") ||
                    myMoveSetScript.IsAnimationPlaying("blockingCrouchingHit") ||
                    myMoveSetScript.IsAnimationPlaying("parryCrouching") ||
                    myMoveSetScript.IsAnimationPlaying("getHitCrouching") ||
                    (myMoveSetScript.AnimationExists("crouching_2") && myMoveSetScript.AnimationTimesPlayed("crouching_2") >= 1) ||
                    (!myMoveSetScript.AnimationExists("crouching_2") && isCrouching))
                {
                    myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.crouching, false);
                }
            }
            else
            {
                myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.blockingCrouchingPose, false);
            }
        }
    }

    public void ExecuteJump(ButtonPress engineRelatedButton, bool pressureSensitive)
    {
        if (pressureSensitive)
        {
            // Pressure Sensitive Jump
            if (myPhysicsScript.IsGrounded()
                && myPhysicsScript.isTakingOff
                && !myPhysicsScript.IsJumping())
            {
                UFE.FindAndRemoveDelaySynchronizedAction(myPhysicsScript.Jump);

                Fix64 minJumpDelaySeconds = myInfo.physics.minJumpDelay / (Fix64)UFE.config.fps;
                if (inputHeldDown[engineRelatedButton] < minJumpDelaySeconds)
                {
                    Fix64 deltaDelaySeconds = minJumpDelaySeconds - inputHeldDown[engineRelatedButton];
                    UFE.DelaySynchronizedAction(myPhysicsScript.MinJump, deltaDelaySeconds);
                }
                else
                {
                    Fix64 jumpDelaySeconds = (Fix64)myInfo.physics.jumpDelay / UFE.config.fps;
                    Fix64 pressurePercentage = FPMath.Min(inputHeldDown[engineRelatedButton] / jumpDelaySeconds, 1);
                    Fix64 newJumpForce = myInfo.physics._jumpForce * pressurePercentage;
                    if (newJumpForce < myInfo.physics._minJumpForce) newJumpForce = myInfo.physics._minJumpForce;
                    myPhysicsScript.Jump(newJumpForce);
                }
            }
        }
        else
        {
            if (!myPhysicsScript.isTakingOff && !myPhysicsScript.isLanding)
            {
                // Double/Multi Jumps
                if (inputHeldDown[engineRelatedButton] == UFE.fixedDeltaTime && !myPhysicsScript.IsGrounded() && myInfo.physics.canJump && myInfo.physics.multiJumps > 1)
                    myPhysicsScript.Jump();

                // Standard Jump
                if (!myPhysicsScript.freeze
                    && !myPhysicsScript.IsJumping()
                    && storedMove == null
                    && currentMove == null
                    && currentState == PossibleStates.Stand
                    && currentSubState != SubStates.Stunned
                    && !isBlocking
                    && myInfo.physics.canJump
                    && !blockStunned
                    && myMoveSetScript.basicMoves.jumpEnabled)
                {
                    myPhysicsScript.isTakingOff = true;
                    potentialBlock = false;
                    potentialParry = 0;

                    Fix64 jumpDelaySeconds = (Fix64)myInfo.physics.jumpDelay / UFE.config.fps;
                    UFE.DelaySynchronizedAction(myPhysicsScript.Jump, jumpDelaySeconds);

                    if (myMoveSetScript.AnimationExists(myMoveSetScript.basicMoves.takeOff.name))
                    {
                        myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.takeOff);

                        if (myMoveSetScript.basicMoves.takeOff.autoSpeed)
                        {
                            myMoveSetScript.SetAnimationSpeed(
                                myMoveSetScript.basicMoves.takeOff.name,
                                myMoveSetScript.GetAnimationLength(myMoveSetScript.basicMoves.takeOff.name) / jumpDelaySeconds);
                        }
                    }
                }
            }
        }
    }

    public void ResetDrainStatus(bool clearGauge) {
        for (int i = 0; i < currentGaugesPoints.Length; i++) ResetDrainStatus(clearGauge, i);
    }

    public void ResetDrainStatus(bool clearGauge, int targetGauge) {
        storedMove = null;
        storedMoveTime = 0;
        myMoveSetScript.ChangeMoveStances(DCStance);
        if (DCMove != null) CastMove(DCMove, true);

        inhibitGainWhileDraining = false;
        if (gaugeDPS > 0 && (currentGaugesPoints[targetGauge] < 0 || clearGauge)) currentGaugesPoints[targetGauge] = 0;
        gaugeDPS = 0;
        gaugeDrainId = GaugeId.Gauge1;
        currentDrained = 0;
        totalDrain = 0;
        DCMove = null;
    }
	
	public void ApplyStun(
		IDictionary<InputReferences, InputEvents> previousInputs,
		IDictionary<InputReferences, InputEvents> currentInputs
	){

        if (airRecoveryType == AirRecoveryType.DontRecover 
            && !myPhysicsScript.IsGrounded() 
            && currentSubState == SubStates.Stunned 
            && currentState != PossibleStates.Down) {
			stunTime = 1;
		}else{
			stunTime -= UFE.fixedDeltaTime;
		}

        BasicMoveInfo newBasicMove = null;
		Fix64 standUpTime = UFE.config.knockDownOptions.air._standUpTime;
        SubKnockdownOptions knockdownOption = null;
        if (!isDead && currentMove == null && myPhysicsScript.IsGrounded()) {
            // Knocked Down
			if (currentState == PossibleStates.Down){
                if (myMoveSetScript.basicMoves.standUpFromAirHit.animMap[0].clip != null &&
                    (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitAir, 1)
                    || currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.fallingFromAirHit, 1)
                    || currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.fallingFromAirHit, 2)
                    || standUpOverride == StandUpOptions.AirJuggleClip)) {
                    if (stunTime <= UFE.config.knockDownOptions.air._standUpTime)
                    {
                        newBasicMove = myMoveSetScript.basicMoves.standUpFromAirHit;
                        standUpTime = UFE.config.knockDownOptions.air._standUpTime;
                        knockdownOption = UFE.config.knockDownOptions.air;
                    }
                } else if (myMoveSetScript.basicMoves.standUpFromKnockBack.animMap[0].clip != null && 
                    (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitKnockBack, 1)
                    || currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitKnockBack, 2)
                    || standUpOverride == StandUpOptions.KnockBackClip)) {
                    if (stunTime <= UFE.config.knockDownOptions.air._standUpTime)
                    {
                        newBasicMove = myMoveSetScript.basicMoves.standUpFromKnockBack;
                        standUpTime = UFE.config.knockDownOptions.air._standUpTime;
                        knockdownOption = UFE.config.knockDownOptions.air;
                    }
                } else if (myMoveSetScript.basicMoves.standUpFromStandingHighHit.animMap[0].clip != null && 
                    (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitHighKnockdown, 1)
                    || currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitHighKnockdown, 2)
                    || standUpOverride == StandUpOptions.HighKnockdownClip)){
					if (stunTime <= UFE.config.knockDownOptions.high._standUpTime)
                    {
                        newBasicMove = myMoveSetScript.basicMoves.standUpFromStandingHighHit;
                        standUpTime = UFE.config.knockDownOptions.high._standUpTime;
                        knockdownOption = UFE.config.knockDownOptions.high;
					}
                } else if (myMoveSetScript.basicMoves.standUpFromStandingMidHit.animMap[0].clip != null && 
                    (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitMidKnockdown, 1)
                    || currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitMidKnockdown, 2)
                    || standUpOverride == StandUpOptions.LowKnockdownClip)){
					if (stunTime <= UFE.config.knockDownOptions.highLow._standUpTime)
                    {
                        newBasicMove = myMoveSetScript.basicMoves.standUpFromStandingMidHit;
                        standUpTime = UFE.config.knockDownOptions.highLow._standUpTime;
                        knockdownOption = UFE.config.knockDownOptions.highLow;
					}
                } else if (myMoveSetScript.basicMoves.standUpFromSweep.animMap[0].clip != null && 
                    (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitSweep, 1)
                    || currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitSweep, 2)
                    || standUpOverride == StandUpOptions.SweepClip)){
					if (stunTime <= UFE.config.knockDownOptions.sweep._standUpTime)
                    {
                        newBasicMove = myMoveSetScript.basicMoves.standUpFromSweep;
                        standUpTime = UFE.config.knockDownOptions.sweep._standUpTime;
                        knockdownOption = UFE.config.knockDownOptions.sweep;
                    }
                } else if (myMoveSetScript.basicMoves.standUpFromAirWallBounce.animMap[0].clip != null && 
                    (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.airWallBounce, 1)
                    || currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.airWallBounce, 2)
                    || standUpOverride == StandUpOptions.AirWallBounceClip)) {
                    if (stunTime <= UFE.config.knockDownOptions.wallbounce._standUpTime)
                    {
                        newBasicMove = myMoveSetScript.basicMoves.standUpFromAirWallBounce;
                        standUpTime = UFE.config.knockDownOptions.wallbounce._standUpTime;
                        knockdownOption = UFE.config.knockDownOptions.wallbounce;
                    }
                } else if (myMoveSetScript.basicMoves.standUpFromGroundBounce.animMap[0].clip != null && 
                    (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.fallingFromGroundBounce, 1)
                    || currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.groundBounce, 1)
                    || currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.groundBounce, 2)
                    || standUpOverride == StandUpOptions.GroundBounceClip)) {
                    if (stunTime <= UFE.config.knockDownOptions.air._standUpTime)
                    {
                        newBasicMove = myMoveSetScript.basicMoves.standUpFromGroundBounce;
                        standUpTime = UFE.config.knockDownOptions.air._standUpTime;
                        knockdownOption = UFE.config.knockDownOptions.air;
                    }
				} else {
					if (myMoveSetScript.basicMoves.standUp.animMap[0].clip == null)
						Debug.LogError("Stand Up animation not found! Make sure you have it set on Character -> Basic Moves -> Stand Up");
					
					if (stunTime <= UFE.config.knockDownOptions.air._standUpTime)
                    {
                        newBasicMove = myMoveSetScript.basicMoves.standUp;
                        standUpTime = UFE.config.knockDownOptions.air._standUpTime;
                        knockdownOption = UFE.config.knockDownOptions.air;
					}
				}
            } else if (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.getHitCrumple, 1)
                || standUpOverride == StandUpOptions.CrumpleClip){
				if (stunTime <= UFE.config.knockDownOptions.crumple._standUpTime){
                    if (myMoveSetScript.basicMoves.standUpFromCrumple.animMap[0].clip != null)
                    {
                        newBasicMove = myMoveSetScript.basicMoves.standUpFromCrumple;
                    } else {
                        if (myMoveSetScript.basicMoves.standUp.animMap[0].clip == null)
                            Debug.LogError("Stand Up animation not found! Make sure you have it set on Character -> Basic Moves -> Stand Up");

                        newBasicMove = myMoveSetScript.basicMoves.standUp;
                    }
                    standUpTime = UFE.config.knockDownOptions.crumple._standUpTime;
                    knockdownOption = UFE.config.knockDownOptions.crumple;
				}
            } else if (currentHitAnimation == myMoveSetScript.GetAnimationString(myMoveSetScript.basicMoves.standingWallBounceKnockdown, 1)
                || standUpOverride == StandUpOptions.StandingWallBounceClip) {
                if (stunTime <= UFE.config.knockDownOptions.wallbounce._standUpTime) {
                    if (myMoveSetScript.basicMoves.standUpFromStandingWallBounce.animMap[0].clip != null)
                    {
                        newBasicMove = myMoveSetScript.basicMoves.standUpFromStandingWallBounce;
                    } else {
                        if (myMoveSetScript.basicMoves.standUp.animMap[0].clip == null)
                            Debug.LogError("Stand Up animation not found! Make sure you have it set on Character -> Basic Moves -> Stand Up");

                        newBasicMove = myMoveSetScript.basicMoves.standUp;
                    }
                    standUpTime = UFE.config.knockDownOptions.wallbounce._standUpTime;
                    knockdownOption = UFE.config.knockDownOptions.wallbounce;
                }
            }
		}

        string standUpAnimation = newBasicMove != null? myMoveSetScript.GetAnimationString(newBasicMove, 1) : null;
        if (standUpAnimation != null && !myMoveSetScript.IsAnimationPlaying(standUpAnimation)){
			myMoveSetScript.PlayBasicMove(newBasicMove, standUpAnimation);
            if (myMoveSetScript.basicMoves.standUp.autoSpeed) {
                myMoveSetScript.SetAnimationSpeed(standUpAnimation, myMoveSetScript.GetAnimationLength(standUpAnimation) / standUpTime);
            }
            if (knockdownOption != null && knockdownOption.hideHitBoxes) myHitBoxesScript.HideHitBoxes(true);
		}
		
		if (stunTime <= 0) {
			ReleaseStun(previousInputs, currentInputs);
		}
	}
    
    public void CastMove(MoveInfo move, bool overrideCurrentMove = false, bool forceGrounded = false, bool castWarning = false) {
		if (move == null) return;
		if (castWarning && !myMoveSetScript.HasMove(move.moveName)) 
            Debug.LogError("Move '"+ move.name +"' could not be found under this character's move set.");

        if (overrideCurrentMove)
        {
			KillCurrentMove();
            MoveInfo newMove = myMoveSetScript.InstantiateMove(move);
			this.SetMove(newMove);
        }
        else
        {
            storedMove = myMoveSetScript.InstantiateMove(move);
		}

        if (forceGrounded) myPhysicsScript.ForceGrounded();
	}

	public void SetMove(MoveInfo move)
    {
        currentMove = move;
        if (currentMove != null)
        {
            if (currentMove.cooldown)
            {
                if (myMoveSetScript.lastMovesPlayed.ContainsKey(currentMove.moveName))
                {
                    myMoveSetScript.lastMovesPlayed[currentMove.moveName] = UFE.currentFrame;
                }
                else
                {
                    myMoveSetScript.lastMovesPlayed.Add(currentMove.moveName, UFE.currentFrame);
                }
            }

            /*if (!myMoveSetScript.AnimationExists(move.name)) {
                Debug.LogWarning("Animation for move '" + move.name + "' not found!");
            }*/

            if (move.disableHeadLook) ToggleHeadLook(false);

            if (myPhysicsScript.IsGrounded())
            {
                myPhysicsScript.isTakingOff = false;
                myPhysicsScript.isLanding = false;
            }

            if (currentState == PossibleStates.NeutralJump ||
                currentState == PossibleStates.ForwardJump ||
                currentState == PossibleStates.BackJump)
            {
                myMoveSetScript.totalAirMoves++;
            }

            Fix64 normalizedTimeConv = myMoveSetScript.GetAnimationNormalizedTime(move.overrideStartupFrame, move);

            if (move.overrideBlendingIn)
            {
                myMoveSetScript.PlayAnimation(move.name, move._blendingIn, normalizedTimeConv);
            }
            else
            {
                myMoveSetScript.PlayAnimation(move.name, myInfo._blendingTime, normalizedTimeConv);
            }

            if (move.animMap.hitBoxDefinitionType == HitBoxDefinitionType.AutoMap)
            {
                myHitBoxesScript.customHitBoxes = null;
                myHitBoxesScript.bakeSpeed = move.fixedSpeed ? move.animMap.bakeSpeed : false;
                myHitBoxesScript.animationMaps = move.animMap.animationMaps;
                //myHitBoxesScript.UpdateMap(0);
            }
            else
            {
                myHitBoxesScript.customHitBoxes = move.animMap.customHitBoxDefinition;
            }

            if (mirror == -1)
            {
                if (currentMove.invertRotationLeft) InvertRotation();
                if (currentMove.forceMirrorLeft) ForceMirror(true);
            }
            else if (mirror == 1)
            {
                if (currentMove.invertRotationRight) InvertRotation();
                if (currentMove.forceMirrorRight) ForceMirror(false);
            }

            applyRootMotion = move.applyRootMotion;
            lockXMotion = move.lockXMotion;
            lockYMotion = move.lockYMotion;
            lockZMotion = move.lockZMotion;

            move.currentTick = move.overrideStartupFrame + (UFE.fixedDeltaTime * UFE.fps);
            move.currentFrame = (int)move.currentTick;

            myMoveSetScript.SetAnimationSpeed(move.name, move._animationSpeed);
            if (move.overrideBlendingOut) myMoveSetScript.overrideNextBlendingValue = move._blendingOut;

            foreach (GaugeInfo gaugeInfo in move.gauges) RemoveGauge(gaugeInfo._gaugeUsage, (int)gaugeInfo.targetGauge);
        }

        foreach (HitBox hitBox in myHitBoxesScript.hitBoxes)
        {
			if (hitBox != null && hitBox.bodyPart != BodyPart.none && hitBox.position != null)
            {
				bool visible = hitBox.defaultVisibility;

				if (move != null && move.bodyPartVisibilityChanges != null)
                {
					foreach (BodyPartVisibilityChange visibilityChange in move.bodyPartVisibilityChanges)
                    {
						if (visibilityChange.castingFrame == 0 && visibilityChange.bodyPart == hitBox.bodyPart)
                        {
							visible = visibilityChange.visible;
							visibilityChange.casted = true;
						}
					}
				}

				hitBox.position.gameObject.SetActive(visible);
			}
		}

        UFE.FireMove(currentMove, this);
	}

	public void ReadMove(MoveInfo move){
		if (move == null) return;

        potentialParry = 0;
		potentialBlock = false;
        CheckBlocking(false);

        // Test character rotation within frame rotation window
        if (currentMove.autoCorrectRotation && move.currentFrame <= move.frameWindowRotation)
            testCharacterRotation();


        // Assign Current Frame Data Description
        if (move.currentFrame <= move.startUpFrames) {
			move.currentFrameData = CurrentFrameData.StartupFrames;
		}else if (move.currentFrame > move.startUpFrames && move.currentFrame <= move.startUpFrames + move.activeFrames) {
			move.currentFrameData = CurrentFrameData.ActiveFrames;
		}else{
			move.currentFrameData = CurrentFrameData.RecoveryFrames;
		}


        // Check Speed Key Frames
        if (!move.fixedSpeed) {
            foreach (AnimSpeedKeyFrame speedKeyFrame in move.animSpeedKeyFrame) {
                if (move.currentFrame >= speedKeyFrame.castingFrame
                    && !myPhysicsScript.freeze) {
                    myMoveSetScript.SetAnimationSpeed(move.name, speedKeyFrame._speed * move._animationSpeed);
                }
            }
        }


        // Check Projectiles
        int projCount = 0;
		foreach (Projectile projectile in move.projectiles){
			if(
				!projectile.casted && 
				projectile.projectilePrefab != null &&
				move.currentFrame >= projectile.castingFrame
			){
                projCount++;

                projectile.casted = true;
				projectile.gaugeGainOnHit = move.gauges.Length > 0 ? move.gauges[0]._gaugeGainOnHit : 0;
				projectile.gaugeGainOnBlock = move.gauges.Length > 0 ? move.gauges[0]._gaugeGainOnBlock : 0;
				projectile.opGaugeGainOnHit = move.gauges.Length > 0 ? move.gauges[0]._opGaugeGainOnHit : 0;
				projectile.opGaugeGainOnBlock = move.gauges.Length > 0 ? move.gauges[0]._opGaugeGainOnBlock : 0;
				projectile.opGaugeGainOnParry = move.gauges.Length > 0 ? move.gauges[0]._opGaugeGainOnParry : 0;

                FPVector newPos = FPVector.zero;
                if (move.animMap.hitBoxDefinitionType == HitBoxDefinitionType.Custom)
                {
                    CustomHitBox hitboxDef = move.animMap.customHitBoxDefinition.customHitBoxes[projectile.hitBoxDefinitionIndex];
                    newPos = hitboxDef.activeFrames[move.currentFrame].position;
                    if (mirror == 1) newPos.x *= -1;
                    newPos += worldTransform.position;
                }
                else
                {
                    newPos = myHitBoxesScript.GetPosition(projectile.bodyPart);
                }

				if (projectile.fixedZAxis) newPos.z = 0;
                long durationFrames = (int)FPMath.Round(projectile._duration * UFE.config.fps);

                string uniqueId = projectile.projectilePrefab.name + playerNum.ToString() + UFE.currentFrame + projCount;
                GameObject pTemp = UFE.SpawnGameObject(projectile.projectilePrefab, newPos.ToVector(), Quaternion.identity, durationFrames, true, uniqueId);

                Vector3 newRotation = projectile.projectilePrefab.transform.rotation.eulerAngles;
                newRotation.z = projectile.directionAngle;
                pTemp.transform.rotation = Quaternion.Euler(newRotation);

                ProjectileMoveScript projectileMoveScript = (ProjectileMoveScript)pTemp.GetComponent(typeof(ProjectileMoveScript));
                if (projectileMoveScript == null)
                {
                    projectileMoveScript = pTemp.AddComponent<ProjectileMoveScript>();
                    projectileMoveScript.fpTransform = pTemp.AddComponent<FPTransform>();
                }
                if (!projectiles.Contains(projectileMoveScript)) projectiles.Add(projectileMoveScript);
                projectileMoveScript.data = projectile;
                projectileMoveScript.data.moveName = move.moveName;
                projectileMoveScript.myControlsScript = this;
                projectileMoveScript.mirror = mirror;
                projectileMoveScript.fpTransform.position = newPos;
                projectileMoveScript.Init();

                //UFE.instantiatedObjects[UFE.instantiatedObjects.Count - 1].projectileScript = projectileMoveScript;
            }
		}


        // Check Particle Effects
        int partCount = 0;
        foreach (MoveParticleEffect particleEffect in move.particleEffects){
			if (
				!particleEffect.casted
                && particleEffect.particleEffect.prefab != null 
                && move.currentFrame >=  particleEffect.castingFrame
			)
            {
                partCount++;
                particleEffect.casted = true;
                UFE.FireParticleEffects(currentMove, this, particleEffect);
                
                long frames = particleEffect.particleEffect.destroyOnMoveOver? (move.totalFrames - move.currentFrame) : Mathf.RoundToInt(particleEffect.particleEffect.duration * UFE.config.fps);
                Quaternion newRotation = particleEffect.particleEffect.initialRotation != Vector3.zero ? Quaternion.Euler(particleEffect.particleEffect.initialRotation) : Quaternion.identity;
                string uniqueId = particleEffect.particleEffect.prefab.name + playerNum.ToString() + UFE.currentFrame + partCount;
                GameObject pTemp = UFE.SpawnGameObject(particleEffect.particleEffect.prefab, Vector3.zero, newRotation, frames, false, uniqueId);
                if (!particleEffect.particleEffect.overrideRotation)
                    pTemp.transform.rotation = particleEffect.particleEffect.prefab.transform.rotation;

                if (particleEffect.particleEffect.stick) {
                    Transform targetTransform = myHitBoxesScript.GetTransform(particleEffect.particleEffect.bodyPart);
                    pTemp.transform.SetParent(targetTransform);
                    pTemp.transform.position = targetTransform.position;
                    if (particleEffect.particleEffect.followRotation) pTemp.AddComponent<StickyGameObject>();

                } else {
                    pTemp.transform.SetParent(UFE.gameEngine.transform);
                    pTemp.transform.position = myHitBoxesScript.GetPosition(particleEffect.particleEffect.bodyPart).ToVector();
                }

                if (particleEffect.particleEffect.lockLocalPosition) pTemp.transform.localPosition = Vector3.zero;

                Vector3 newPosition = Vector3.zero;
                newPosition.x = particleEffect.particleEffect.positionOffSet.x * -mirror;
                newPosition.y = particleEffect.particleEffect.positionOffSet.y;
                newPosition.z = particleEffect.particleEffect.positionOffSet.z;
                pTemp.transform.localPosition += newPosition;
			}
		}

		
        // Check Gauge Updates
        foreach (GaugeInfo gaugeInfo in move.gauges) {
            if (!gaugeInfo.casted && move.currentFrame >= gaugeInfo.castingFrame) {
                AddGauge(gaugeInfo._gaugeGainOnMiss, (int)gaugeInfo.targetGauge);

                if (gaugeInfo.startDrainingGauge)
                {
                    gaugeDPS = gaugeInfo._gaugeDPS;
                    gaugeDrainId = gaugeInfo.targetGauge;
                    totalDrain = gaugeInfo._totalDrain;
                    DCMove = gaugeInfo.DCMove;
                    DCStance = gaugeInfo.DCStance;
                    inhibitGainWhileDraining = gaugeInfo.inhibitGainWhileDraining;
                }
                if (gaugeInfo.stopDrainingGauge)
                {
                    gaugeDPS = 0;
                    inhibitGainWhileDraining = false;
                }

                gaugeInfo.casted = true;
            }
        }


		// Check Applied Forces
		foreach (AppliedForce addedForce in move.appliedForces){
			if (!addedForce.casted && move.currentFrame >= addedForce.castingFrame){
				myPhysicsScript.ResetForces(addedForce.resetPreviousHorizontal, addedForce.resetPreviousVertical);
				myPhysicsScript.AddForce(addedForce._force, GetDirection(), true);
				addedForce.casted = true;
			}
		}


        // Check Body Part Visibility Changes
        foreach (BodyPartVisibilityChange visibilityChange in move.bodyPartVisibilityChanges) {
            if (!visibilityChange.casted && move.currentFrame >= visibilityChange.castingFrame) {
				foreach (HitBox hitBox in myHitBoxesScript.hitBoxes) {
					if (visibilityChange.bodyPart == hitBox.bodyPart && 
                        ((mirror == - 1 && visibilityChange.left) || (mirror == 1 && visibilityChange.right))) {

                        UFE.FireBodyVisibilityChange(currentMove, this, visibilityChange, hitBox);
						hitBox.position.gameObject.SetActive(visibilityChange.visible);
						visibilityChange.casted = true;
					}
				}
            }
        }


		// Check SlowMo Effects
		foreach (SlowMoEffect slowMoEffect in move.slowMoEffects){
			if (!slowMoEffect.casted && move.currentFrame >= slowMoEffect.castingFrame){
				UFE.timeScale = (slowMoEffect._percentage / 100) * UFE.config._gameSpeed;
				UFE.DelaySynchronizedAction(UFE.fluxCapacitor.ReturnTimeScale, slowMoEffect._duration);
				slowMoEffect.casted = true;
			}
		}

		
		// Check Sound Effects
		foreach (SoundEffect soundEffect in move.soundEffects){
			if (!soundEffect.casted && move.currentFrame >= soundEffect.castingFrame){
				UFE.PlaySound(soundEffect.sounds);
				soundEffect.casted = true;
			}
		}

		
		// Check In Game Alert
		foreach (InGameAlert inGameAlert in move.inGameAlert){
			if (!inGameAlert.casted && move.currentFrame >= inGameAlert.castingFrame){
				UFE.FireAlert(inGameAlert.alert, this);
				inGameAlert.casted = true;
			}
		}


        // Check for Sort Order Overrides
        foreach (MoveSortOrder sortOrder in move.sortOrder) {
            if (mySpriteRenderer != null && move.currentFrame == sortOrder.castingFrame) {
                mySpriteRenderer.sortingOrder = sortOrder.value;
            }
        }

		
		// Change Stances
		foreach (StanceChange stanceChange in move.stanceChanges){
			if (!stanceChange.casted && move.currentFrame >= stanceChange.castingFrame){
				myMoveSetScript.ChangeMoveStances(stanceChange.newStance);
				stanceChange.casted = true;
			}
        }


        // Check Opponent Override
		foreach (OpponentOverride opponentOverride in move.opponentOverride){
			if (!opponentOverride.casted && move.currentFrame >= opponentOverride.castingFrame){
				if (opponentOverride.stun){
					opControlsScript.stunTime = opponentOverride._stunTime/UFE.config.fps;
					if (opControlsScript.stunTime > 0) opControlsScript.currentSubState = SubStates.Stunned;
				}
				
				opControlsScript.KillCurrentMove();
				foreach(CharacterSpecificMoves csMove in opponentOverride.characterSpecificMoves){
					if (opInfo.characterName == csMove.characterName) {
						opControlsScript.CastMove(csMove.move, true);
						if (opponentOverride.stun) opControlsScript.currentMove.standUpOptions = opponentOverride.standUpOptions;
						opControlsScript.currentMove.hitAnimationOverride = opponentOverride.overrideHitAnimations;
					}
				}
				if (opControlsScript.currentMove == null && opponentOverride.move != null){
					opControlsScript.CastMove(opponentOverride.move, true);
					if (opponentOverride.stun) opControlsScript.currentMove.standUpOptions = opponentOverride.standUpOptions;
					opControlsScript.currentMove.hitAnimationOverride = opponentOverride.overrideHitAnimations;
				}

                if (opponentOverride._position != FPVector.zero)
                {
                    opControlsScript.activePullIn = new PullIn();
                    FPVector newPos = opponentOverride._position;
                    if (UFE.config.gameplayType == GameplayType._2DFighter)
                    {
                        newPos.x *= -mirror;
                        opControlsScript.activePullIn.position = worldTransform.position + newPos;
                    }
#if !UFE_LITE && !UFE_BASIC
                    else
                    {
                        opControlsScript.activePullIn.position = (FPQuaternion.Euler(0, transform.rotation.eulerAngles.y - 90, 0) * newPos) + worldTransform.position;
                    }
#endif
                    opControlsScript.activePullIn.speed = opponentOverride.blendSpeed;
                    opControlsScript.activePullIn._targetDistance = FPVector.Distance(worldTransform.position, opControlsScript.activePullIn.position);
                    opControlsScript.activePullIn.forceGrounded = false;
                }

                if (opponentOverride.resetAppliedForces){
					opControlsScript.Physics.ResetForces(true, true);
					myPhysicsScript.ResetForces(true, true);
				}
				
				opponentOverride.casted = true;
			}
		}

#if !UFE_LITE && !UFE_BASIC
        // Character Assist
        foreach (CharacterAssist charAssist in move.characterAssist){
			if (!charAssist.casted && move.currentFrame >= charAssist.castingFrame){

                FPVector offSet = charAssist._spawnPosition;
                offSet.x *= -mirror;
                ControlsScript assistCharacter = UFE.SpawnCharacter(charAssist.characterInfo, playerNum, -mirror, (worldTransform.position + offSet), true, charAssist.enterMove, charAssist.exitMove);

#if !UFE_LITE && !UFE_BASIC
                if (UFE.config.gameplayType != GameplayType._2DFighter)
                    assistCharacter.LookAtTarget();
#endif

                charAssist.casted = true;
			}
        }
#endif

        // Check Camera Movements (cinematics)
        foreach (CameraMovement cameraMovement in move.cameraMovements){
			if (cameraMovement.over) continue;
			if (cameraMovement.casted && !cameraMovement.over && cameraMovement.time >= cameraMovement._duration && UFE.freeCamera){
				cameraMovement.over = true;
				ReleaseCam();
			}
			if (move.currentFrame >= cameraMovement.castingFrame){
				cameraMovement.time += UFE.fixedDeltaTime;
				if (cameraMovement.casted) continue;
				cameraMovement.casted = true;

                UFE.freezePhysics = cameraMovement.freezePhysics;
                cameraScript.cinematicFreeze = cameraMovement.freezePhysics;

                PausePlayAnimation(true, cameraMovement._myAnimationSpeed * .01);
                myPhysicsScript.freeze = cameraMovement.freezePhysics;


                if (UFE.config.selectedMatchType != MatchType.Singles)
                {
                    foreach (ControlsScript cScript in UFE.GetAllControlsScripts())
                    {
                        if (cScript == this) continue;
                        cScript.PausePlayAnimation(true, cameraMovement._opAnimationSpeed * .01);
                        cScript.Physics.freeze = cameraMovement.freezePhysics;
                    }
                }
                else
                {
                    foreach (ControlsScript assist in owner.assists) {
                        assist.PausePlayAnimation(true, cameraMovement._opAnimationSpeed * .01);
                        assist.Physics.freeze = cameraMovement.freezePhysics;
                    }
                    foreach (ControlsScript assist in opControlsScript.assists) {
                        assist.PausePlayAnimation(true, cameraMovement._opAnimationSpeed * .01);
                        assist.Physics.freeze = cameraMovement.freezePhysics;
                    }
                }

                opControlsScript.PausePlayAnimation(true, cameraMovement._opAnimationSpeed * .01);
                opControlsScript.Physics.freeze = cameraMovement.freezePhysics;

                if (isAssist) {
                    owner.PausePlayAnimation(true, cameraMovement._opAnimationSpeed * .01);
                    owner.Physics.freeze = cameraMovement.freezePhysics;
                }


                if (cameraMovement.cinematicType == CinematicType.CameraEditor){
					cameraMovement.position.x *= -mirror;
					Vector3 targetPosition = transform.TransformPoint(cameraMovement.position);
					Vector3 targetRotation = cameraMovement.rotation;
					targetRotation.y *= -mirror;
					targetRotation.z *= -mirror;
					cameraScript.MoveCameraToLocation(targetPosition,
					                                  targetRotation,
					                                  cameraMovement.fieldOfView,
					                                  cameraMovement.camSpeed, gameObject);
					
				}else if (cameraMovement.cinematicType == CinematicType.Prefab){
					cameraScript.SetCameraOwner(gameObject);
                    emulatedCam = UFE.SpawnGameObject(cameraMovement.prefab, transform.position, Quaternion.identity, false, cameraMovement._duration);
					
				}else if (cameraMovement.cinematicType == CinematicType.AnimationFile){
					emulatedCam = new GameObject();
					emulatedCam.name = "Camera Parent";
					emulatedCam.transform.parent = transform;
					emulatedCam.transform.localPosition = cameraMovement.gameObjectPosition;
					emulatedCam.AddComponent(typeof(Animation));
					emulatedCam.GetComponent<Animation>().AddClip(cameraMovement.animationClip, "cam");
					emulatedCam.GetComponent<Animation>()["cam"].speed = cameraMovement.camAnimationSpeed;
					emulatedCam.GetComponent<Animation>().Play("cam");
					
					Camera.main.transform.parent = emulatedCam.transform;
					cameraScript.MoveCameraToLocation(cameraMovement.position,
					                                  cameraMovement.rotation,
					                                  cameraMovement.fieldOfView,
					                                  cameraMovement.blendSpeed, gameObject);
					
				}
			}
		}


#if !UFE_LITE && !UFE_BASIC
        // Check Lock-On Targets
        if (UFE.config.gameplayType != GameplayType._2DFighter)
        {
            foreach (MoveLockOnOptions lockOnTarget in move.lockOnTargets)
            {
                if (move.currentFrame >= lockOnTarget.activeFramesBegin &&
                    move.currentFrame < lockOnTarget.activeFramesEnds)
                {
                    if (lockOnTarget.targetSwitchType == TargetSwitchType.CurrentTarget)
                    {
                        LookAtTarget(opControlsScript);
                    }
                    else if (lockOnTarget.targetSwitchType == TargetSwitchType.NearestTarget)
                    {
                        Fix64 closestDistance = Fix64.MaxValue;
                        List<ControlsScript> opTeam = UFE.GetControlsScriptTeam(opControlsScript.playerNum);
                        for (int i = 0; i < opTeam.Count; i++)
                        {
                            if (opTeam[i] != this)
                            {
                                Fix64 newDistance = FPVector.Distance(opTeam[i].worldTransform.position, worldTransform.position);
                                if (closestDistance > newDistance)
                                {
                                    closestDistance = newDistance;
                                    LookAtTarget(opTeam[i]);
                                }
                            }
                        }
                    }
                    else if (lockOnTarget.targetSwitchType == TargetSwitchType.NextTarget)
                    {
                        int targetNum = 0;
                        List<ControlsScript> opTeam = UFE.GetControlsScriptTeam(opControlsScript.playerNum);
                        for (int i = 0; i < opTeam.Count; i++)
                        {
                            if (opTeam[i] == opControlsScript)
                            {
                                if (i == opTeam.Count - 1)
                                    targetNum = 0;
                                else
                                    targetNum = i + 1;

                                opControlsScript = opTeam[targetNum];
                                LookAtTarget(opControlsScript);
                                break;
                            }
                        }
                    }
                    //TODO: Update option to also deal with target rotation and rotation speed
                }
            }
        }


        // Switch Characters
        foreach (SwitchCharacterOptions switchCharOption in move.switchCharacterOptions)
        {
            if (move.currentFrame >= switchCharOption.castingFrame)
            {
                if (switchCharOption.characterSwitchType == CharacterSwitchType.NearestCharacter)
                {
                    Fix64 closestDistance = Fix64.MaxValue;
                    List<ControlsScript> myTeam = UFE.GetControlsScriptTeam(playerNum);
                    for (int i = 0; i < myTeam.Count; i++)
                    {
                        if (myTeam[i] != this)
                        {
                            Fix64 newDistance = FPVector.Distance(myTeam[i].worldTransform.position, worldTransform.position);
                            if (closestDistance > newDistance)
                            {
                                closestDistance = newDistance;
                                UFE.SetMainControlsScript(playerNum, i);
                            }
                        }
                    }
                }
                else if (switchCharOption.characterSwitchType == CharacterSwitchType.NextCharacter)
                {
                    int target = 0;
                    List<ControlsScript> myTeam = UFE.GetControlsScriptTeam(playerNum);
                    for (int i = 0; i < myTeam.Count; i++)
                    {
                        if (myTeam[i] == this)
                        {
                            if (i == myTeam.Count - 1) 
                                target = 0;
                            else
                                target = i + 1;

                            UFE.SetMainControlsScript(playerNum, target);
                            break;
                        }
                    }
                }
                else
                {
                    UFE.SetMainControlsScript(playerNum, (int)switchCharOption.characterSwitchType);
                }
            }
        }
#endif


        // Check State Override
        foreach (StateOverride stateOverride in move.stateOverride)
        {
            if (move.currentFrame >= stateOverride.castingFrame && move.currentFrame <= stateOverride.endFrame)
            {
                currentState = stateOverride.state;
            }
        }


        // Check Invincible Body Parts
        if (move.invincibleBodyParts.Length > 0) {
			foreach (InvincibleBodyParts invBodyPart in move.invincibleBodyParts){
				if (move.currentFrame >= invBodyPart.activeFramesBegin &&
				    move.currentFrame < invBodyPart.activeFramesEnds) {
					if (invBodyPart.completelyInvincible){
						myHitBoxesScript.HideHitBoxes(true);
					}else{
						myHitBoxesScript.HideHitBoxes(invBodyPart.hitBoxes, true);
					}
					ignoreCollisionMass = invBodyPart.ignoreBodyColliders;
				}
				if (move.currentFrame >= invBodyPart.activeFramesEnds) {
					if (invBodyPart.completelyInvincible){
						myHitBoxesScript.HideHitBoxes(false);
					}else{
						myHitBoxesScript.HideHitBoxes(invBodyPart.hitBoxes, false);
					}
					ignoreCollisionMass = false;
				}
			}
		}


        // Check Blockable Area
        if (move.blockableArea.enabled)
        {
            if (move.currentFrame >= move.blockableArea.activeFramesBegin &&
                move.currentFrame < move.blockableArea.activeFramesEnds)
            {
                if (move.animMap.hitBoxDefinitionType == HitBoxDefinitionType.AutoMap && move.blockableArea.bodyPart != BodyPart.none)
                {
                    myHitBoxesScript.blockableArea = move.blockableArea;
                    myHitBoxesScript.blockableArea.position = myHitBoxesScript.GetPosition(myHitBoxesScript.blockableArea.bodyPart);
                }
                else if (move.animMap.hitBoxDefinitionType == HitBoxDefinitionType.Custom)
                {
                    CustomHitBox hitboxDef = move.animMap.customHitBoxDefinition.customHitBoxes[move.blockableArea.hitBoxDefinitionIndex];

                    myHitBoxesScript.blockableArea = move.blockableArea;
                    myHitBoxesScript.blockableArea.shape = hitboxDef.shape;
                    myHitBoxesScript.blockableArea._radius = hitboxDef.activeFrames[move.currentFrame].radius;

                    myHitBoxesScript.blockableArea._rect = new FPRect(0, 0, hitboxDef.activeFrames[move.currentFrame].cubeWidth, hitboxDef.activeFrames[move.currentFrame].cubeHeight);
                    myHitBoxesScript.blockableArea.rect = move.blockableArea._rect.ToRect();

                    FPVector newPosition = hitboxDef.activeFrames[move.currentFrame].position;
                    if (mirror == 1) newPosition.x *= -1;
                    myHitBoxesScript.blockableArea.position = newPosition + worldTransform.position;
                }

                if (!opControlsScript.isBlocking
                    && !opControlsScript.blockStunned
                    && opControlsScript.currentSubState != SubStates.Stunned
                    && CollisionManager.TestCollision(opControlsScript.HitBoxes.hitBoxes, myHitBoxesScript.blockableArea, opControlsScript.mirror > 0, mirror > 0).Length > 0)
                {
                    opControlsScript.CheckBlocking(true);
                }
            }
            else if (move.currentFrame >= move.blockableArea.activeFramesEnds)
            {
                if (UFE.config.blockOptions.blockType == BlockType.HoldBack ||
                    UFE.config.blockOptions.blockType == BlockType.AutoBlock) opControlsScript.CheckBlocking(false);
            }
        }


        // Check Frame Links
        foreach (FrameLink frameLink in move.frameLinks){
            if (move.currentFrame >= frameLink.activeFramesBegins &&
                move.currentFrame <= frameLink.activeFramesEnds) {
                if (frameLink.linkType == LinkType.NoConditions ||
                    (frameLink.linkType == LinkType.HitConfirm &&
                    ((currentMove.hitConfirmOnStrike && frameLink.onStrike) ||
                    (currentMove.hitConfirmOnBlock && frameLink.onBlock) ||
                    (currentMove.hitConfirmOnParry && frameLink.onParry)))) {
                        frameLink.cancelable = true;
                }
            } else {
                frameLink.cancelable = false;
            }
		}


        // Check Hits
        if (!UFE.freezePhysics) {

            if (UFE.config.selectedMatchType != MatchType.Singles)
            {
                foreach (ControlsScript cScript in UFE.GetAllControlsScriptsByPlayer(opControlsScript.playerNum))
                {
                    CheckHits(move, cScript);
                }
            }
            else
            {
                foreach (ControlsScript assist in opControlsScript.assists) CheckHits(move, assist);
                CheckHits(move, opControlsScript);
            }
        }


        // Force Map Update
        //myHitBoxesScript.UpdateMap(move.currentFrame);

        // Kill Move
        if (move.currentFrame >= move.totalFrames - 1) {
            if (move.name == "Intro") {
                introPlayed = true;
                if (opControlsScript.introPlayed) UFE.CastNewRound(2);
            }
            if (move.armorOptions.hitsTaken > 0) comboHits = 0;
			KillCurrentMove();
            
            // Assist - Despawn after Exit Move
            if (exitMove != null && move.name == exitMove.name) {
                exitMove = null;
                ResetData(true);
                SetActive(false);

                worldTransform.position = new FPVector(-999, -999, 0);
                transform.position = worldTransform.position.ToVector();
            }
        }

        // Next Tick
        if (myMoveSetScript.animationPaused){
            move.currentTick += UFE.fixedDeltaTime * UFE.config.fps * myMoveSetScript.GetAnimationSpeed();
        }else{
            move.currentTick += UFE.fixedDeltaTime * UFE.config.fps;
        }


        // Next Frame
        move.currentFrame = (int)move.currentTick;
        //move.currentFrame = (int)FPMath.Floor(move.currentTick) + 2;

        // Get current animation values
        //move.currentTick = MoveSet.GetCurrentClipTick();
        //move.currentFrame = MoveSet.GetCurrentClipFrame();
    }

    public void CheckHits(MoveInfo move, ControlsScript opControlsScript) {
        if (!GetActive() || !opControlsScript.GetActive()) return;

        HurtBox[] activeHurtBoxes = null;
        for (int i = 0; i < move.hits.Length; i ++)
        {
            Hit hit = move.hits[i];
            if (move.currentFrame >= hit.activeFramesBegin &&
                move.currentFrame <= hit.activeFramesEnds)
            {
                if (hit.hurtBoxes.Length > 0)
                {
                    activeHurtBoxes = hit.hurtBoxes;
                    if (hit.impactList == null) hit.impactList = new List<ControlsScript>();

                    if ((opControlsScript.HitBoxes == null)
                    || (!hit.allowMultiHit && opControlsScript.HitBoxes.isHit)
                    || (hit.continuousHit && (opControlsScript.HitBoxes.isHit || move.currentTick < move.currentFrame))
                    || (!hit.continuousHit && hit.impactList.Contains(opControlsScript))
                    || (!opControlsScript.ValidateHit(hit))) continue;

                    foreach (HurtBox hurtBox in activeHurtBoxes)
                    {
                        if (move.animMap.hitBoxDefinitionType == HitBoxDefinitionType.Custom)
                        {
                            CustomHitBox hitboxDef = move.animMap.customHitBoxDefinition.customHitBoxes[hurtBox.hitBoxDefinitionIndex];

                            hurtBox.shape = hitboxDef.shape;
                            hurtBox._radius = hitboxDef.activeFrames[move.currentFrame].radius;
                            hurtBox._rect = new FPRect(0, 0, hitboxDef.activeFrames[move.currentFrame].cubeWidth, hitboxDef.activeFrames[move.currentFrame].cubeHeight);
                            FPVector newPosition = hitboxDef.activeFrames[move.currentFrame].position;
                            if (mirror == 1) newPosition.x *= -1;
                            hurtBox.position = newPosition + worldTransform.position;
                        }
                        else
                        {
                            if (UFE.config.gameplayType == GameplayType._2DFighter)
                            {
                                hurtBox.position = myHitBoxesScript.GetPosition(hurtBox.bodyPart);
                                if (hurtBox.shape == HitBoxShape.circle) {
                                    if (mirror == 1) hurtBox._offSet.x *= -1;
                                    hurtBox.position += hurtBox._offSet;
                                }
                            }
#if !UFE_LITE && !UFE_BASIC
                            else
                            {
                                hurtBox.position = myHitBoxesScript.GetPosition(hurtBox.bodyPart, true);
                                hurtBox.position += hurtBox._offSet;
                                hurtBox.position = (FPQuaternion.Euler(0, transform.rotation.eulerAngles.y - 90, 0) * hurtBox.position) + worldTransform.position;
                            }
#endif

                            hurtBox.rendererBounds = myHitBoxesScript.GetBounds();
                        }
                    }

                    FPVector[] collisionVectors = CollisionManager.TestCollision(opControlsScript.HitBoxes.hitBoxes, activeHurtBoxes, hit.hitConfirmType, false, mirror > 0);
                    if (collisionVectors.Length > 0)
                    { // HURTBOX TEST
                        Fix64 newAnimSpeed = GetHitAnimationSpeed(hit.hitStrength);
                        Fix64 freezingTime = GetHitFreezingTime(hit.hitStrength);

                        // Tech Throw
                        if (hit.hitConfirmType == HitConfirmType.Throw
                            && hit.techable
                            && opControlsScript.currentMove != null
                            && opControlsScript.currentMove.IsThrow(true)
                            )
                        {
                            CastMove(hit.techMove, true);
                            opControlsScript.CastMove(opControlsScript.currentMove.GetTechMove(), true);
                            return;

                        // Throw
                        }
                        else if (hit.hitConfirmType == HitConfirmType.Throw)
                        {
                            CastMove(hit.throwMove, true);
                            return;

                        // Parry
                        }
                        else if (opControlsScript.potentialParry > 0
                            && opControlsScript.currentMove == null
                            && hit.hitConfirmType != HitConfirmType.Throw
                            && opControlsScript.TestParryStances(hit.hitType))
                        {
                            opControlsScript.GetHitParry(hit, move.totalFrames - move.currentFrame, collisionVectors, this);
                            foreach (GaugeInfo gaugeInfo in move.gauges)
                            {
                                opControlsScript.AddGauge(gaugeInfo._opGaugeGainOnParry, (int)gaugeInfo.targetGauge);
                            }
                            move.hitConfirmOnParry = true;

                            myPhysicsScript.ResetForces(hit.resetPreviousHorizontal, hit.resetPreviousVertical, hit.resetPreviousSideways);
                            if (hit.applyDifferentSelfBlockForce)
                                myPhysicsScript.AddForce(hit._appliedForceBlock, GetDirection(), true);
                            else
                                myPhysicsScript.AddForce(hit._appliedForce, GetDirection(), true);
                        }
                        // Block
                        else if (opControlsScript.currentSubState != SubStates.Stunned
                            && opControlsScript.currentMove == null
                            && opControlsScript.isBlocking
                            && opControlsScript.TestBlockStances(hit.hitType)
                            && !hit.unblockable)
                        {
                            opControlsScript.GetHitBlocking(hit, move.totalFrames - move.currentFrame, collisionVectors, (UFE.config.gameplayType != GameplayType._2DFighter), this);
                            foreach (GaugeInfo gaugeInfo in move.gauges)
                            {
                                AddGauge(gaugeInfo._gaugeGainOnBlock, (int)gaugeInfo.targetGauge);
                                opControlsScript.AddGauge(gaugeInfo._opGaugeGainOnBlock, (int)gaugeInfo.targetGauge);
                            }
                            move.hitConfirmOnBlock = true;

                            if (hit.overrideHitEffectsBlock)
                            {
                                newAnimSpeed = hit.hitEffectsBlock._animationSpeed;
                                freezingTime = hit.hitEffectsBlock._freezingTime;
                            }

                            myPhysicsScript.ResetForces(hit.resetPreviousHorizontal, hit.resetPreviousVertical, hit.resetPreviousSideways);
                            if (hit.applyDifferentSelfBlockForce)
                                myPhysicsScript.AddForce(hit._appliedForceBlock, GetDirection(), true);
                            else
                                myPhysicsScript.AddForce(hit._appliedForce, GetDirection(), true);
                        }
                        // Hit
                        else
                        {
                            opControlsScript.GetHit(hit, move.totalFrames - move.currentFrame, collisionVectors, false, this);
                            foreach (GaugeInfo gaugeInfo in move.gauges)
                            {
                                AddGauge(gaugeInfo._gaugeGainOnHit, (int)gaugeInfo.targetGauge);
                                opControlsScript.AddGauge(gaugeInfo._opGaugeGainOnHit, (int)gaugeInfo.targetGauge);
                            }

                            if (hit.pullSelfIn.position != FPVector.zero)
                            {
                                activePullIn = new PullIn();
                                FPVector newPos = hit.pullSelfIn.position;
                                newPos.x *= -mirror;
                                activePullIn.position = opControlsScript.worldTransform.position + newPos;
                                activePullIn.speed = hit.pullSelfIn.speed;
                                activePullIn._targetDistance = FPVector.Distance(worldTransform.position, activePullIn.position);
                                activePullIn.forceGrounded = hit.pullSelfIn.forceGrounded;

                                if (hit.pullSelfIn.forceGrounded)
                                {
                                    activePullIn.position.y = 0;
                                    myPhysicsScript.ForceGrounded();
                                }
                            }

                            move.hitConfirmOnStrike = true;

                            if (hit.overrideHitEffects)
                            {
                                newAnimSpeed = hit.hitEffects._animationSpeed;
                                freezingTime = hit.hitEffects._freezingTime;
                            }

                            myPhysicsScript.ResetForces(hit.resetPreviousHorizontal, hit.resetPreviousVertical, hit.resetPreviousSideways);
                            if (hit.applyDifferentSelfAirForce && !opControlsScript.Physics.IsGrounded())
                                myPhysicsScript.AddForce(hit._appliedForceAir, GetDirection(), true);
                            else
                                myPhysicsScript.AddForce(hit._appliedForce, GetDirection(), true);
                        }

                        // Test position boundaries
                        if (UFE.config.gameplayType == GameplayType._2DFighter)
                        {
                            if ((opControlsScript.worldTransform.position.x >= UFE.config.selectedStage.position.x + UFE.config.selectedStage._rightBoundary - 2 
                                || opControlsScript.worldTransform.position.x <= UFE.config.selectedStage.position.x + UFE.config.selectedStage._leftBoundary + 2)
                                && myPhysicsScript.IsGrounded() && !UFE.config.comboOptions.neverCornerPush && hit.cornerPush)
                            {
                                myPhysicsScript.ResetForces(hit.resetPreviousHorizontalPush, false);
                                myPhysicsScript.AddForce(
                                    new FPVector(hit._pushForce.x + (opControlsScript.Physics.airTime * opInfo.physics._friction), 0, 0), -GetDirection(), true);
                            }
                        }

                        // Apply freezing effect
                        if (opControlsScript.Physics.freeze)
                        {
                            HitPause(newAnimSpeed * .01);
                            UFE.DelaySynchronizedAction(this.HitUnpause, freezingTime);
                        }
                        hit.impactList.Add(opControlsScript);
                    }
                }
                myHitBoxesScript.activeHurtBoxes = activeHurtBoxes;
            }
            else if (move.currentFrame > hit.activeFramesEnds && i >= move.hits.Length - 1)
            {
                myHitBoxesScript.activeHurtBoxes = null;
            }
        }
    }

    public int GetDirection()
    {
#if !UFE_LITE && !UFE_BASIC
        //if (UFE.config.gameplayType == GameplayType._3DArena) return 1;
        //else 
        return -mirror;
#else
        return -mirror;
#endif
    }


    // Imediately cancels any move being executed
    public void KillCurrentMove(){
		if (currentMove == null) return;
		currentMove.currentFrame = 0;
		currentMove.currentTick = 0;

		myHitBoxesScript.activeHurtBoxes = null;
		myHitBoxesScript.blockableArea = null;

		ignoreCollisionMass = false;
		if (UFE.config.blockOptions.blockType == BlockType.HoldBack ||
		    UFE.config.blockOptions.blockType == BlockType.AutoBlock) opControlsScript.CheckBlocking(false);
        
		if (currentMove.disableHeadLook) ToggleHeadLook(true);

        if (mirror == -1) {
            if (currentMove.invertRotationLeft) InvertRotation();
            if (currentMove.forceMirrorLeft) ForceMirror(false);
        }
        else if (mirror == 1) {
            if (currentMove.invertRotationRight) InvertRotation();
            if (currentMove.forceMirrorRight) ForceMirror(true);
        }

        fixCharacterRotation();

        if (stunTime > 0){
			standUpOverride = currentMove.standUpOptions;
			if (standUpOverride != StandUpOptions.None) currentState = PossibleStates.Down;
		}

		this.SetMove(null);
		ReleaseCam();
	}

	// Release character to be playable again
	private void ReleaseStun(
		IDictionary<InputReferences, InputEvents> previousInputs,
		IDictionary<InputReferences, InputEvents> currentInputs
	){
		if (currentSubState != SubStates.Stunned && !blockStunned) return;
		if (!isBlocking && comboHits > 1 && UFE.config.comboOptions.comboDisplayMode == ComboDisplayMode.ShowAfterComboExecution){
			UFE.FireAlert(UFE.config.selectedLanguage.combo, opControlsScript);
		}
        currentHit = null;
		currentSubState = SubStates.Resting;
		blockStunned = false;
		stunTime = 0;
		comboHits = 0;
		comboDamage = 0;
		comboHitDamage = 0;
		airJuggleHits = 0;
        consecutiveCrumple = 0;
        CheckBlocking(false);
        activePullIn = null;

        standUpOverride = StandUpOptions.None;

        myPhysicsScript.ResetWeight();
        myPhysicsScript.isWallBouncing = false;
        myPhysicsScript.wallBounceTimes = 0;
        myPhysicsScript.overrideStunAnimation = null;
        myPhysicsScript.overrideAirAnimation = false;

        if (!myPhysicsScript.IsGrounded()) isAirRecovering = true;

		if (!isDead) ToggleHeadLook(true);

        if (myPhysicsScript.IsGrounded())
        {
            if (isCrouching)
                currentState = PossibleStates.Crouch;
            else
                currentState = PossibleStates.Stand;
        }
		if (!isAssist && (UFE.p1ControlsScript == this || UFE.p2ControlsScript == this)) translateInputs(previousInputs, currentInputs);
	}

	private void ReleaseCam(){
		if (cameraScript.GetCameraOwner() != gameObject) return;
		if (outroPlayed && UFE.config.roundOptions.freezeCamAfterOutro) return;
		Camera.main.transform.parent = null;

		if (emulatedCam != null){
			UFE.DestroyGameObject(emulatedCam);
		}

        UFE.freezePhysics = false;
        cameraScript.ReleaseCam();

        PausePlayAnimation(false);
		myPhysicsScript.freeze = false;

        foreach (ControlsScript assist in owner.assists) {
            assist.PausePlayAnimation(false);
            assist.Physics.freeze = false;
        }

        foreach (ControlsScript assist in opControlsScript.assists) {
            assist.PausePlayAnimation(false);
            assist.Physics.freeze = false;
        }

        opControlsScript.PausePlayAnimation(false);
        opControlsScript.Physics.freeze = false;

        if (isAssist) {
            owner.PausePlayAnimation(false);
            owner.Physics.freeze = false;
        }
    }

	public bool TestBlockStances(HitType hitType){
		if (UFE.config.blockOptions.blockType == BlockType.None) return false;
		if ((hitType == HitType.Mid || hitType == HitType.MidKnockdown || hitType == HitType.Launcher) && myPhysicsScript.IsGrounded()) return true;
		if ((hitType == HitType.Overhead || hitType == HitType.HighKnockdown) && currentState == PossibleStates.Crouch) return false;
		if ((hitType == HitType.Sweep || hitType == HitType.Low) && currentState != PossibleStates.Crouch) return false;
		if (!UFE.config.blockOptions.allowAirBlock && !myPhysicsScript.IsGrounded()) return false;
		return true;
	}
	
	public bool TestParryStances(HitType hitType){
		if (UFE.config.blockOptions.parryType == ParryType.None) return false;
		if ((hitType == HitType.Mid || hitType == HitType.MidKnockdown || hitType == HitType.Launcher) && myPhysicsScript.IsGrounded()) return true;
		if ((hitType == HitType.Overhead || hitType == HitType.HighKnockdown) && currentState == PossibleStates.Crouch) return false;
		if ((hitType == HitType.Sweep || hitType == HitType.Low) && currentState != PossibleStates.Crouch) return false;
		if (!UFE.config.blockOptions.allowAirParry && !myPhysicsScript.IsGrounded()) return false;
		return true;
	}
	
	public void CheckBlocking(bool flag){
		if (myPhysicsScript.freeze) return;
		if (myPhysicsScript.isTakingOff) return;
		if (flag){
			if (potentialBlock){
				if (currentMove != null) {
					potentialBlock = false;
					return;
				}
				if (currentState == PossibleStates.Crouch) {
					if (myMoveSetScript.basicMoves.blockingCrouchingPose.animMap[0].clip == null)
						Debug.LogError("Blocking Crouching Pose animation not found! Make sure you have it set on Character -> Basic Moves -> Blocking Crouching Pose");
					myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.blockingCrouchingPose, false);
					isBlocking = true;
				}else if (currentState == PossibleStates.Stand) {
					if (myMoveSetScript.basicMoves.blockingHighPose.animMap[0].clip == null)
						Debug.LogError("Blocking High Pose animation not found! Make sure you have it set on Character -> Basic Moves -> Blocking High Pose");
					myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.blockingHighPose, false);
					isBlocking = true;
				}else if (!myPhysicsScript.IsGrounded() && UFE.config.blockOptions.allowAirBlock) {
					if (myMoveSetScript.basicMoves.blockingAirPose.animMap[0].clip == null)
						Debug.LogError("Blocking Air Pose animation not found! Make sure you have it set on Character -> Basic Moves -> Blocking Air Pose");
					myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.blockingAirPose, false);
					isBlocking = true;
				}
			}
		}else if (!blockStunned){
			isBlocking = false;
		}
	}
	
	private void HighlightOn(GameObject target, bool flag){
		Renderer[] charRenders = target.GetComponentsInChildren<Renderer>();
		if (flag && !lit){
			lit = true;
			foreach(Renderer charRender in charRenders){
				charRender.material.shader = Shader.Find("VertexLit");
                if (charRender.material.HasProperty("_Color")) charRender.material.color = UFE.config.blockOptions.parryColor;
			}
		}else if (lit){
			lit = false;
			for(int i = 0; i < charRenders.Length; i ++){
				charRenders[i].material.shader = normalShaders[i];
                if (charRenders[i].material.HasProperty("_Color")) charRenders[i].material.color = normalColors[i];
			}
		}
	}
	
	private void HighlightOff(){
		HighlightOn(character, false);
	}

	public bool ValidateHit(Hit hit){
		if (comboHits >= UFE.config.comboOptions.maxCombo) return false;
		if (!hit.groundHit && myPhysicsScript.IsGrounded()) return false;
        if (!hit.crouchingHit && currentState == PossibleStates.Crouch) return false;
        if (!hit.airHit && currentState != PossibleStates.Stand && currentState != PossibleStates.Crouch && !myPhysicsScript.IsGrounded()) return false;
		if (!hit.stunHit && currentSubState == SubStates.Stunned) return false;
		if (!hit.downHit && currentState == PossibleStates.Down) return false;
        if (myMoveSetScript != null && !myMoveSetScript.ValidadeBasicMove(hit.opponentConditions, this)) return false;
        if (myMoveSetScript != null && !myMoveSetScript.ValidateMoveStances(hit.opponentConditions, this)) return false;

		return true;
	}

	public void GetHitParry(Hit hit, int remainingFrames, FPVector[] location, ControlsScript attacker){
		UFE.FireAlert(UFE.config.selectedLanguage.parry, this);

		BasicMoveInfo currentHitInfo = myMoveSetScript.basicMoves.parryHigh;
		blockStunned = true;
		currentSubState = SubStates.Blocking;

		myHitBoxesScript.isHit = true;

        if (!UFE.config.blockOptions.easyParry) {
            potentialParry = 0;
        }

		if (UFE.config.blockOptions.resetButtonSequence){
			myMoveSetScript.ClearLastButtonSequence();
		}

		if (UFE.config.blockOptions.parryStunType == ParryStunType.Fixed){
			stunTime = (Fix64)UFE.config.blockOptions.parryStunFrames/UFE.config.fps;
		}else{
			int stunFrames = 0;
			if (hit.hitStunType == HitStunType.FrameAdvantage) {
				stunFrames = hit.frameAdvantageOnBlock + remainingFrames;
				stunFrames *= (UFE.config.blockOptions.parryStunFrames/100);
				if (stunFrames < 1) stunFrames = 1;
				stunTime = (Fix64)stunFrames/UFE.config.fps;
			}else if (hit.hitStunType == HitStunType.Frames) {
				stunFrames = (int) hit._hitStunOnBlock;
				stunFrames = (int)FPMath.Round((stunFrames * UFE.config.blockOptions.parryStunFrames)/(Fix64)100);
				if (stunFrames < 1) stunFrames = 1;
				stunTime = (Fix64)stunFrames/UFE.config.fps;
			}else{
				stunTime = hit._hitStunOnBlock * ((Fix64)UFE.config.blockOptions.parryStunFrames/ 100);
			}
		}

        UFE.FireParry(myHitBoxesScript.GetStrokeHitBox(), attacker.currentMove, this);

		// Create hit parry effect
		GameObject particle = UFE.config.blockOptions.parryHitEffects.hitParticle;
        Fix64 killTime = UFE.config.blockOptions.parryHitEffects.killTime;
		AudioClip soundEffect = UFE.config.blockOptions.parryHitEffects.hitSound;
		if (location.Length > 0 && particle != null){
            HitEffectSpawnPoint spawnPoint = UFE.config.blockOptions.parryHitEffects.spawnPoint;
            if (hit.overrideEffectSpawnPoint) spawnPoint = hit.spawnPoint;

            long frames = (long)FPMath.Round(killTime * UFE.config.fps);
            GameObject pTemp = UFE.SpawnGameObject(particle, GetParticleSpawnPoint(spawnPoint, location), Quaternion.identity, frames);
            pTemp.transform.rotation = particle.transform.rotation;

            if (UFE.config.blockOptions.parryHitEffects.mirrorOn2PSide && mirror > 0) {
                pTemp.transform.localEulerAngles = new Vector3(pTemp.transform.localEulerAngles.x, pTemp.transform.localEulerAngles.y + 180, pTemp.transform.localEulerAngles.z);
            }

			//pTemp.transform.localScale = new Vector3(-mirror, 1, 1);
            pTemp.transform.parent = UFE.gameEngine.transform;
		}
		UFE.PlaySound(soundEffect);
		
		// Shake Options
		shakeCamera = UFE.config.blockOptions.parryHitEffects.shakeCameraOnHit;
		shakeCharacter = UFE.config.blockOptions.parryHitEffects.shakeCharacterOnHit;
		shakeDensity = UFE.config.blockOptions.parryHitEffects._shakeDensity;
        shakeCameraDensity = UFE.config.blockOptions.parryHitEffects._shakeCameraDensity;


		
		// Get correct animation according to stance
        if (currentState == PossibleStates.Crouch) {
            currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.parryCrouching, hit);
			currentHitInfo = myMoveSetScript.basicMoves.parryCrouching;
			if (!myMoveSetScript.AnimationExists(currentHitAnimation))
                Debug.LogError("Parry Crouching animation not found! Make sure you have it set on Character -> Basic Moves -> Parry Animations -> Crouching");
		}else if (currentState == PossibleStates.Stand){
            HitBox strokeHit = myHitBoxesScript.GetStrokeHitBox();
            if (strokeHit.type == HitBoxType.low && myMoveSetScript.basicMoves.parryLow.animMap[0].clip != null) {
                currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.parryLow, hit);
                currentHitInfo = myMoveSetScript.basicMoves.parryLow;

            } else {
                currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.parryHigh, hit);
                currentHitInfo = myMoveSetScript.basicMoves.parryHigh;
                if (!myMoveSetScript.AnimationExists(currentHitAnimation))
                    Debug.LogError("Parry High animation not found! Make sure you have it set on Character -> Basic Moves -> Parry Animations -> Standing");

            }
        } else if (!myPhysicsScript.IsGrounded()) {
            currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.parryAir, hit);
			currentHitInfo = myMoveSetScript.basicMoves.parryAir;
			if (!myMoveSetScript.AnimationExists(currentHitAnimation))
                Debug.LogError("Parry Air animation not found! Make sure you have it set on Character -> Basic Moves -> Parry Animations -> Air");
		}

		myMoveSetScript.PlayBasicMove(currentHitInfo, currentHitAnimation);
        if (currentHitInfo.autoSpeed) {
            myMoveSetScript.SetAnimationSpeed(currentHitAnimation, (myMoveSetScript.GetAnimationLength(currentHitAnimation) / stunTime));
        }
		
		// Highlight effect when parry
		if (UFE.config.blockOptions.highlightWhenParry){
			HighlightOn(gameObject, true);
			UFE.DelaySynchronizedAction(this.HighlightOff, 0.2);
		}
		
		// Freeze screen depending on how strong the hit was
		HitPause(GetHitAnimationSpeed(hit.hitStrength) * .01);
		UFE.DelaySynchronizedAction(this.HitUnpause, GetHitFreezingTime(hit.hitStrength));

        // Reset hit to allow for another hit while the character is still stunned
        Fix64 spaceBetweenHits = 1;
		if (hit.spaceBetweenHits == Sizes.VerySmall){
			spaceBetweenHits = 1.05;
		}else if (hit.spaceBetweenHits == Sizes.Small){
			spaceBetweenHits = 1.15;
		}else if (hit.spaceBetweenHits == Sizes.Medium){
			spaceBetweenHits = 1.3;
		}else if (hit.spaceBetweenHits == Sizes.High){
			spaceBetweenHits = 1.7;
		}else if (hit.spaceBetweenHits == Sizes.VeryHigh){
			spaceBetweenHits = 2;
		}

        if (UFE.config.blockOptions.parryHitEffects.autoHitStop) {
            UFE.DelaySynchronizedAction(myHitBoxesScript.ResetHit, GetHitFreezingTime(hit.hitStrength) * spaceBetweenHits);
        } else {
            UFE.DelaySynchronizedAction(myHitBoxesScript.ResetHit, UFE.config.blockOptions.parryHitEffects._hitStop * spaceBetweenHits);
        }
		
		// Add force to the move
		myPhysicsScript.ResetForces(hit.resetPreviousHorizontalPush, hit.resetPreviousVerticalPush, hit.resetPreviousSidewaysPush);

		if (!UFE.config.blockOptions.ignoreAppliedForceParry)
            if (hit.applyDifferentBlockForce) {
                myPhysicsScript.AddForce(hit._pushForceBlock, opControlsScript.GetDirection(), true);
            } else {
                myPhysicsScript.AddForce(hit._pushForce, opControlsScript.GetDirection(), true);
            }
	}

	public void GetHitBlocking(Hit hit, int remainingFrames, FPVector[] location, bool ignoreDirection, ControlsScript attacker){
		// Lose life
		if (hit._damageOnBlock >= currentLifePoints){
			GetHit(hit, remainingFrames, location, ignoreDirection, attacker);
			return;
		}else{
            Fix64 damage = hit._damageOnBlock;
            if (hit.damageType == DamageType.Percentage) damage = myInfo.lifePoints * (damage / 100);
            DamageMe(damage);
		}

		blockStunned = true;
		currentSubState = SubStates.Blocking;
        myHitBoxesScript.isHit = true;

		int stunFrames = 0;
		BasicMoveInfo currentHitInfo = myMoveSetScript.basicMoves.blockingHighHit;

		if (hit.hitStunType == HitStunType.FrameAdvantage) {
			stunFrames = hit.frameAdvantageOnBlock + remainingFrames;
			if (stunFrames < 1) stunFrames = 1;
			stunTime = (Fix64)stunFrames/UFE.config.fps;
		}else if (hit.hitStunType == HitStunType.Frames) {
			stunFrames = (int) hit._hitStunOnBlock;
			if (stunFrames < 1) stunFrames = 1;
			stunTime = (Fix64)stunFrames/UFE.config.fps;
		}else{
			stunTime = hit._hitStunOnBlock;
		}

        UFE.FireBlock(myHitBoxesScript.GetStrokeHitBox(), opControlsScript.currentMove, this);

        HitTypeOptions hitEffects = UFE.config.blockOptions.blockHitEffects;
        Fix64 freezingTime = GetHitFreezingTime(hit.hitStrength);
        if (hit.overrideHitEffectsBlock) {
            hitEffects = hit.hitEffectsBlock;
            freezingTime = hitEffects._freezingTime;
        }

        // Create hit effect
        GameObject particle = hitEffects.hitParticle;
        Fix64 killTime = hitEffects.killTime;
		AudioClip soundEffect = hitEffects.hitSound;
		if (location.Length > 0 && particle != null){
            HitEffectSpawnPoint spawnPoint = hitEffects.spawnPoint;
            if (hit.overrideEffectSpawnPoint) spawnPoint = hit.spawnPoint;
            
            long frames = (long)FPMath.Round(killTime * UFE.config.fps);
            GameObject pTemp = UFE.SpawnGameObject(particle, GetParticleSpawnPoint(spawnPoint, location), Quaternion.identity, frames);
            pTemp.transform.rotation = particle.transform.rotation;

            if (hitEffects.mirrorOn2PSide && mirror > 0) {
                pTemp.transform.localEulerAngles = new Vector3(pTemp.transform.localEulerAngles.x, pTemp.transform.localEulerAngles.y + 180, pTemp.transform.localEulerAngles.z);
            }

		}
		UFE.PlaySound(soundEffect);

		// Shake Options
		shakeCamera = hitEffects.shakeCameraOnHit;
		shakeCharacter = hitEffects.shakeCharacterOnHit;
		shakeDensity = hitEffects._shakeDensity;
        shakeCameraDensity = hitEffects._shakeCameraDensity;


        if (currentState == PossibleStates.Crouch){
			currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.blockingCrouchingHit, hit);
			currentHitInfo = myMoveSetScript.basicMoves.blockingCrouchingHit;

			if (!myMoveSetScript.AnimationExists(currentHitAnimation))
                Debug.LogError("Blocking Crouching Hit animation not found! Make sure you have it set on Character -> Basic Moves -> Blocking Animations");
		}else if (currentState == PossibleStates.Stand){
			HitBox strokeHit = myHitBoxesScript.GetStrokeHitBox();
			if (strokeHit.type == HitBoxType.low && myMoveSetScript.basicMoves.blockingLowHit.animMap[0].clip != null){
				currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.blockingLowHit, hit);
				currentHitInfo = myMoveSetScript.basicMoves.blockingLowHit;

			}else{
				currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.blockingHighHit, hit);
				currentHitInfo = myMoveSetScript.basicMoves.blockingHighHit;
				if (!myMoveSetScript.AnimationExists(currentHitAnimation))
                    Debug.LogError("Blocking High Hit animation not found! Make sure you have it set on Character -> Basic Moves -> Blocking Animations");

			}

		}else if (!myPhysicsScript.IsGrounded()){
			currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.blockingAirHit, hit);
			currentHitInfo = myMoveSetScript.basicMoves.blockingAirHit;
			if (!myMoveSetScript.AnimationExists(currentHitAnimation))
				Debug.LogError("Blocking Air Hit animation not found! Make sure you have it set on Character -> Basic Moves -> Blocking Animations");
		}


        myMoveSetScript.PlayBasicMove(currentHitInfo, currentHitAnimation);
        hitAnimationSpeed = myMoveSetScript.GetAnimationLength(currentHitAnimation) / stunTime;

        if (currentHitInfo.autoSpeed) {
            myMoveSetScript.SetAnimationSpeed(currentHitAnimation, hitAnimationSpeed);
        }

        // Freeze screen depending on how strong the hit was
        HitPause(GetHitAnimationSpeed(hit.hitStrength) * .01);
        UFE.DelaySynchronizedAction(this.HitUnpause, freezingTime);

        // Reset hit to allow for another hit while the character is still stunned
        Fix64 spaceBetweenHits = 1;
		if (hit.spaceBetweenHits == Sizes.VerySmall){
			spaceBetweenHits = 1.05;
		}else if (hit.spaceBetweenHits == Sizes.Small){
			spaceBetweenHits = 1.15;
		}else if (hit.spaceBetweenHits == Sizes.Medium){
			spaceBetweenHits = 1.3;
		}else if (hit.spaceBetweenHits == Sizes.High){
			spaceBetweenHits = 1.7;
		}else if (hit.spaceBetweenHits == Sizes.VeryHigh){
			spaceBetweenHits = 2;
		}

        if (hitEffects.autoHitStop) {
            UFE.DelaySynchronizedAction(myHitBoxesScript.ResetHit, freezingTime * spaceBetweenHits);
        } else {
            UFE.DelaySynchronizedAction(myHitBoxesScript.ResetHit, hitEffects._hitStop * spaceBetweenHits);
        }
		
		// Add force to the move
		myPhysicsScript.ResetForces(hit.resetPreviousHorizontalPush, hit.resetPreviousVerticalPush, hit.resetPreviousSidewaysPush);

        if (!UFE.config.blockOptions.ignoreAppliedForceBlock)
            if (hit.applyDifferentBlockForce) {
                myPhysicsScript.AddForce(hit._pushForceBlock, ignoreDirection ? -GetDirection() : opControlsScript.GetDirection());
            } else {
                myPhysicsScript.AddForce(hit._pushForce, ignoreDirection ? -GetDirection() : opControlsScript.GetDirection());
            }
	}

    public void TestRotationOnHit(ControlsScript attacker, Hit hit, bool overrideConditions = false)
    {
        if (UFE.config.gameplayType != GameplayType._2DFighter &&
            (overrideConditions || UFE.config.characterRotationOptions.fixRotationWhenStunned || UFE.config.characterRotationOptions.fixRotationOnHit || hit.fixRotation))
            LookAtTarget(attacker, false);

        if (UFE.config.gameplayType == GameplayType._2DFighter && 
            (overrideConditions || UFE.config.characterRotationOptions.fixRotationOnHit || hit.fixRotation))
            testCharacterRotation();
    }
	
	public void GetHit(Hit hit, int remainingFrames, FPVector[] location, bool ignoreDirection, ControlsScript attacker){
        // Get what animation should be played depending on the character's state
        bool airHit = false;
		bool armored = false;
		bool isKnockDown = false;
        Fix64 damageModifier = 1;
        Fix64 hitStunModifier = 1;
		BasicMoveInfo currentHitInfo;

        currentHit = hit;

        if (!attacker.isAssist) opControlsScript = attacker;

        myHitBoxesScript.isHit = true;

		if (myInfo.headLook.disableOnHit) ToggleHeadLook(false);

        TestRotationOnHit(attacker, hit);

        if (currentMove != null && currentMove.frameLinks.Length > 0){
			foreach (FrameLink frameLink in currentMove.frameLinks){
				if (currentMove.currentFrame >= frameLink.activeFramesBegins &&
				    currentMove.currentFrame <= frameLink.activeFramesEnds) {
					if (frameLink.linkType == LinkType.CounterMove){
						bool cancelable = false;
						if (frameLink.counterMoveType == CounterMoveType.SpecificMove)
                        {
							if (frameLink.counterMoveFilter == currentMove) cancelable = true;
						}
                        else
                        {
							HitBox strokeHitBox = myHitBoxesScript.GetStrokeHitBox();
							if ((frameLink.anyHitStrength || frameLink.hitStrength == hit.hitStrength) &&
							    (frameLink.anyStrokeHitBox || frameLink.hitBoxType == strokeHitBox.type) &&
							    (frameLink.anyHitType || frameLink.hitType == hit.hitType)){
								cancelable = true;
							}
						}

                        if (cancelable)
                        {
                            frameLink.cancelable = true;
							if (frameLink.disableHitImpact) {
                                Fix64 timeLeft = (Fix64)(currentMove.totalFrames - currentMove.currentFrame)/UFE.config.fps;

								myHitBoxesScript.ResetHit();
								UFE.DelaySynchronizedAction(myHitBoxesScript.ResetHit, timeLeft);
								return;
							}
						}
					}
				}
			}
		}
		
		// Set position in case of pull enemy in
        if (hit.pullEnemyIn.position != FPVector.zero)
        {
            activePullIn = new PullIn();
            FPVector newPos = hit.pullEnemyIn.position;
            newPos.x *= mirror;
            activePullIn.position = opControlsScript.worldTransform.position + newPos;
            activePullIn.speed = hit.pullEnemyIn.speed;
            activePullIn._targetDistance = FPVector.Distance(worldTransform.position, activePullIn.position);
            activePullIn.forceGrounded = hit.pullEnemyIn.forceGrounded;

            if (hit.pullEnemyIn.forceGrounded)
            {
                activePullIn.position.y = 0;
                myPhysicsScript.ForceGrounded();
            }
        }

        if (hit.resetCrumples) consecutiveCrumple = 0;

        // Obtain animation depending on HitType
		if (myPhysicsScript.IsGrounded()) {
            if (hit.hitStrength == HitStrengh.Crumple && hit.hitType != HitType.Launcher) {
				if (myMoveSetScript.basicMoves.getHitCrumple.animMap[0].clip == null)
					Debug.LogError("("+ myInfo.characterName +") Crumple animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
				currentHitAnimation = myMoveSetScript.basicMoves.getHitCrumple.name;
				currentHitInfo = myMoveSetScript.basicMoves.getHitCrumple;
                consecutiveCrumple ++;

			}else if (hit.hitType == HitType.Launcher){
				if (myMoveSetScript.basicMoves.getHitAir.animMap[0].clip == null)
                    Debug.LogError("(" + myInfo.characterName + ") Air Juggle animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
				currentHitAnimation = myMoveSetScript.basicMoves.getHitAir.name;
				currentHitInfo = myMoveSetScript.basicMoves.getHitAir;

				airHit = true;
			}else if (hit.hitType == HitType.KnockBack){
                if (myMoveSetScript.basicMoves.getHitKnockBack.animMap[0].clip == null) {
                    if (myMoveSetScript.basicMoves.getHitAir.animMap[0].clip == null)
                        Debug.LogError("(" + myInfo.characterName + ") Air Juggle & Knock Back animations not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
                    currentHitAnimation = myMoveSetScript.basicMoves.getHitAir.name;
                    currentHitInfo = myMoveSetScript.basicMoves.getHitAir;
                } else {
                    currentHitAnimation = myMoveSetScript.basicMoves.getHitKnockBack.name;
                    currentHitInfo = myMoveSetScript.basicMoves.getHitKnockBack;
                }

				airHit = true;
			}else if (hit.hitType == HitType.HighKnockdown){
				if (myMoveSetScript.basicMoves.getHitHighKnockdown.animMap[0].clip == null)
                    Debug.LogError("(" + myInfo.characterName + ") Standing High Hit [Knockdown] animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
				currentHitAnimation = myMoveSetScript.basicMoves.getHitHighKnockdown.name;
				currentHitInfo = myMoveSetScript.basicMoves.getHitHighKnockdown;

				isKnockDown = true;
			}else if (hit.hitType == HitType.MidKnockdown){
				if (myMoveSetScript.basicMoves.getHitMidKnockdown.animMap[0].clip == null)
                    Debug.LogError("(" + myInfo.characterName + ") Standing Mid Hit [Knockdown] animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
				currentHitAnimation = myMoveSetScript.basicMoves.getHitMidKnockdown.name;
				currentHitInfo = myMoveSetScript.basicMoves.getHitMidKnockdown;

				isKnockDown = true;
			}else if (hit.hitType == HitType.Sweep){
				if (myMoveSetScript.basicMoves.getHitSweep.animMap[0].clip == null)
                    Debug.LogError("(" + myInfo.characterName + ") Sweep [Knockdown] animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
				currentHitAnimation = myMoveSetScript.basicMoves.getHitSweep.name;
				currentHitInfo = myMoveSetScript.basicMoves.getHitSweep;

				isKnockDown = true;
			}else if (currentState == PossibleStates.Crouch && !hit.forceStand){
				if (myMoveSetScript.basicMoves.getHitCrouching.animMap[0].clip == null)
                    Debug.LogError("(" + myInfo.characterName + ") Crouching Hit animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
				currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.getHitCrouching, hit);
				currentHitInfo = myMoveSetScript.basicMoves.getHitCrouching;

			}else{
				HitBox strokeHit = myHitBoxesScript.GetStrokeHitBox();
                if (strokeHit.type == HitBoxType.low && myMoveSetScript.basicMoves.getHitLow.animMap[0].clip != null) {
                    if (myMoveSetScript.basicMoves.getHitHigh.animMap[0].clip == null)
                        Debug.LogError("(" + myInfo.characterName + ") Standing Low Hit animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
					currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.getHitLow, hit);
					currentHitInfo = myMoveSetScript.basicMoves.getHitLow;

				}else{
					if (myMoveSetScript.basicMoves.getHitHigh.animMap[0].clip == null)
                        Debug.LogError("(" + myInfo.characterName + ") Standing High Hit animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
					currentHitAnimation = GetHitAnimation(myMoveSetScript.basicMoves.getHitHigh, hit);
					currentHitInfo = myMoveSetScript.basicMoves.getHitHigh;

				}
			}
		}else{
			if (hit.hitStrength == HitStrengh.Crumple && myMoveSetScript.basicMoves.getHitKnockBack.animMap[0].clip != null){
				currentHitAnimation = myMoveSetScript.basicMoves.getHitKnockBack.name;
				currentHitInfo = myMoveSetScript.basicMoves.getHitKnockBack;
			}else{
				if (myMoveSetScript.basicMoves.getHitAir.animMap[0].clip == null)
                    Debug.LogError("(" + myInfo.characterName + ") Air Juggle animation not found! Make sure you have it set on Character -> Basic Moves -> Hit Reactions");
				currentHitAnimation = myMoveSetScript.basicMoves.getHitAir.name;
				currentHitInfo = myMoveSetScript.basicMoves.getHitAir;
			}
			airHit = true;
		}
        
        // Override Hit Animation
        myPhysicsScript.overrideStunAnimation = null;
        if (hit.overrideHitAnimation) {
            BasicMoveInfo basicMoveOverride = myMoveSetScript.GetBasicAnimationInfo(hit.newHitAnimation);
            if (basicMoveOverride != null) {
                currentHitInfo = basicMoveOverride;
                currentHitAnimation = currentHitInfo.name;
                myPhysicsScript.overrideStunAnimation = currentHitInfo;
            } else {
                Debug.LogWarning("(" + myInfo.characterName + ") " + currentHitAnimation + " animation not found! Override not applied.");
            }
        }
		
		// Obtain hit effects
		HitTypeOptions hitEffects = hit.hitEffects;
		if (!hit.overrideHitEffects) {
			if (hit.hitStrength == HitStrengh.Weak) hitEffects = UFE.config.hitOptions.weakHit;
			if (hit.hitStrength == HitStrengh.Medium) hitEffects = UFE.config.hitOptions.mediumHit;
			if (hit.hitStrength == HitStrengh.Heavy) hitEffects = UFE.config.hitOptions.heavyHit;
			if (hit.hitStrength == HitStrengh.Crumple) hitEffects = UFE.config.hitOptions.crumpleHit;
			if (hit.hitStrength == HitStrengh.Custom1) hitEffects = UFE.config.hitOptions.customHit1;
			if (hit.hitStrength == HitStrengh.Custom2) hitEffects = UFE.config.hitOptions.customHit2;
			if (hit.hitStrength == HitStrengh.Custom3) hitEffects = UFE.config.hitOptions.customHit3;
			if (hit.hitStrength == HitStrengh.Custom4) hitEffects = UFE.config.hitOptions.customHit4;
			if (hit.hitStrength == HitStrengh.Custom5) hitEffects = UFE.config.hitOptions.customHit5;
			if (hit.hitStrength == HitStrengh.Custom6) hitEffects = UFE.config.hitOptions.customHit6;
		}

		// Cancel current move if any
        if (!hit.armorBreaker && currentMove != null &&
            currentMove.armorOptions.hitsTaken < currentMove.armorOptions.hitAbsorption &&
		    currentMove.currentFrame >= currentMove.armorOptions.activeFramesBegin && 
		    currentMove.currentFrame <= currentMove.armorOptions.activeFramesEnds){
			armored = true;
			currentMove.armorOptions.hitsTaken ++;
			damageModifier -= currentMove.armorOptions.damageAbsorption * .01;
			if (currentMove.armorOptions.overrideHitEffects) 
				hitEffects = currentMove.armorOptions.hitEffects;

		}else if (currentMove != null && !currentMove.hitAnimationOverride){
			if ((UFE.config.counterHitOptions.startUpFrames && currentMove.currentFrameData == CurrentFrameData.StartupFrames) ||
			    (UFE.config.counterHitOptions.activeFrames && currentMove.currentFrameData == CurrentFrameData.ActiveFrames) ||
			    (UFE.config.counterHitOptions.recoveryFrames && currentMove.currentFrameData == CurrentFrameData.RecoveryFrames)){
				UFE.FireAlert(UFE.config.selectedLanguage.counterHit, opControlsScript);
				damageModifier += UFE.config.counterHitOptions._damageIncrease * .01;
				hitStunModifier += UFE.config.counterHitOptions._hitStunIncrease * .01;
			}

            // Run another hit check for possible trades
            if (!opControlsScript.HitBoxes.isHit) CheckHits(currentMove, opControlsScript);
            foreach (ControlsScript assist in opControlsScript.assists)
                if (!assist.HitBoxes.isHit) CheckHits(currentMove, assist);

            storedMove = null;
			KillCurrentMove();
		}
		
		// Create hit effect
		if (location.Length > 0 && hitEffects.hitParticle != null){
            HitEffectSpawnPoint spawnPoint = hitEffects.spawnPoint;
            if (hit.overrideEffectSpawnPoint) spawnPoint = hit.spawnPoint;
            Vector3 newLocation = GetParticleSpawnPoint(spawnPoint, location);

            long frames = Mathf.RoundToInt(hitEffects.killTime * UFE.config.fps);
            string uniqueId = hitEffects.hitParticle.name + playerNum.ToString() + UFE.currentFrame;
            GameObject pTemp = UFE.SpawnGameObject(hitEffects.hitParticle, newLocation, Quaternion.identity, frames, false, uniqueId);
            
            if (hitEffects.mirrorOn2PSide && mirror > 0) {
                pTemp.transform.localEulerAngles = new Vector3(pTemp.transform.localEulerAngles.x, pTemp.transform.localEulerAngles.y + 180, pTemp.transform.localEulerAngles.z);
            }
		}

        // Set the Sort Order for each character
        if (UFE.config.sortCharacterOnHit) {
            if (myInfo.animationType == AnimationType.Mecanim2D && mySpriteRenderer != null) {
                mySpriteRenderer.sortingOrder = UFE.config.backgroundSortLayer;
            }

            if (attacker.myInfo.animationType == AnimationType.Mecanim2D && attacker.mySpriteRenderer != null) {
                attacker.mySpriteRenderer.sortingOrder = UFE.config.foregroundSortLayer;
            }
        }

		// Play sound
		UFE.PlaySound(hitEffects.hitSound);

		// Shake Options
		shakeCamera = hitEffects.shakeCameraOnHit;
		shakeCharacter = hitEffects.shakeCharacterOnHit;
		shakeDensity = hitEffects._shakeDensity;
        shakeCameraDensity = hitEffects._shakeCameraDensity;

        // Cast First Hit if true
        if (!firstHit && !opControlsScript.firstHit){
			opControlsScript.firstHit = true;
			UFE.FireAlert(UFE.config.selectedLanguage.firstHit, opControlsScript);
		}
		UFE.FireHit(myHitBoxesScript.GetStrokeHitBox(), opControlsScript.currentMove, opControlsScript);

        // Convert to percentage in case of DamageType
        Fix64 damage = hit._damageOnHit;
        if (hit.damageType == DamageType.Percentage) damage = myInfo.lifePoints * (damage / 100);

        // Damage deterioration
        if (hit.damageScaling) {
            if (UFE.config.comboOptions.damageDeterioration == Sizes.VerySmall)
            {
                damage = damage - (damage * comboHits * .05);
            }
            else if (UFE.config.comboOptions.damageDeterioration == Sizes.Small)
            {
                damage = damage - (damage * comboHits * .1);
            }
            else if (UFE.config.comboOptions.damageDeterioration == Sizes.Medium)
            {
                damage = damage - (damage * comboHits * .2);
            }
            else if (UFE.config.comboOptions.damageDeterioration == Sizes.High)
            {
                damage = damage - (damage * comboHits * .3);
            }
            else if (UFE.config.comboOptions.damageDeterioration == Sizes.VeryHigh)
            {
                damage = damage - (damage * comboHits * .4);
            }
        }

		if (damage < UFE.config.comboOptions._minDamage) damage = UFE.config.comboOptions._minDamage;
		damage *= damageModifier;
        comboHitDamage = damage;
        comboDamage += damage;

        owner.comboHits++;

		if (comboHits > 1 && UFE.config.comboOptions.comboDisplayMode == ComboDisplayMode.ShowDuringComboExecution){
			UFE.FireAlert(UFE.config.selectedLanguage.combo, opControlsScript);
		}

		// Lose life
		isDead = DamageMe(damage, hit.doesntKill);

        // Reset hit to allow for another hit while the character is still stunned
        Fix64 spaceBetweenHits = 1;
		if (hit.spaceBetweenHits == Sizes.VerySmall){
			spaceBetweenHits = 1.05;
		}else if (hit.spaceBetweenHits == Sizes.Small){
			spaceBetweenHits = 1.15;
		}else if (hit.spaceBetweenHits == Sizes.Medium){
			spaceBetweenHits = 1.3;
		}else if (hit.spaceBetweenHits == Sizes.High){
			spaceBetweenHits = 1.7;
		}else if (hit.spaceBetweenHits == Sizes.VeryHigh){
			spaceBetweenHits = 2;
		}

        if (hitEffects.autoHitStop) {
            UFE.DelaySynchronizedAction(myHitBoxesScript.ResetHit, hitEffects._freezingTime * spaceBetweenHits);
        } else {
            UFE.DelaySynchronizedAction(myHitBoxesScript.ResetHit, hitEffects._hitStop * spaceBetweenHits);
        }


        // Override Camera Speed
        if (hit.overrideCameraSpeed) {
            cameraScript.OverrideSpeed((float)hit._newMovementSpeed, (float)hit._newRotationSpeed);
            UFE.DelaySynchronizedAction(cameraScript.RestoreSpeed, hit._cameraSpeedDuration);
        }


        // Stun
        int stunFrames = 0;
		if ((currentMove == null || !currentMove.hitAnimationOverride) && (!armored || isDead)) {
			// Hit stun deterioration (the longer the combo gets, the harder it is to combo)
			currentSubState = SubStates.Stunned;
			if (hit.hitStunType == HitStunType.FrameAdvantage) {
				stunFrames = hit.frameAdvantageOnHit + remainingFrames;
				if (stunFrames < 1) stunFrames = 1;
				if (stunFrames < UFE.config.comboOptions._minHitStun) stunFrames = UFE.config.comboOptions._minHitStun;
				stunTime = (Fix64)stunFrames/UFE.config.fps;
			}else if (hit.hitStunType == HitStunType.Frames) {
				stunFrames = (int) hit._hitStunOnHit;
				if (stunFrames < 1) stunFrames = 1;
				if (stunFrames < UFE.config.comboOptions._minHitStun) stunFrames = UFE.config.comboOptions._minHitStun;
                stunTime = (Fix64)stunFrames/UFE.config.fps;
            } else {
                stunFrames = (int)FPMath.Round(hit._hitStunOnHit * UFE.config.fps);
				stunTime = hit._hitStunOnHit;
			}

			if (!hit.resetPreviousHitStun){
				if (UFE.config.comboOptions.hitStunDeterioration == Sizes.VerySmall){
					stunTime -= (Fix64)comboHits * .005;
				}else if (UFE.config.comboOptions.hitStunDeterioration == Sizes.Small){
					stunTime -= (Fix64)comboHits * .01;
				}else if (UFE.config.comboOptions.hitStunDeterioration == Sizes.Medium){
					stunTime -= (Fix64)comboHits * .02;
				}else if (UFE.config.comboOptions.hitStunDeterioration == Sizes.High){
					stunTime -= (Fix64)comboHits * .03;
				}else if (UFE.config.comboOptions.hitStunDeterioration == Sizes.VeryHigh){
					stunTime -= (Fix64)comboHits * .045;
				}
			}
			stunTime *= hitStunModifier;

            FPVector pushForce = new FPVector();
            if (!myPhysicsScript.IsGrounded() && hit.applyDifferentAirForce) {
                pushForce.x = hit._pushForceAir.x;
                pushForce.y = hit._pushForceAir.y;
            } else {
                pushForce.x = hit._pushForce.x;
                pushForce.y = hit._pushForce.y;
            }

            if (consecutiveCrumple > UFE.config.comboOptions.maxConsecutiveCrumple) {
                isKnockDown = true;
                airHit = true;
                pushForce.y = 1;
            }

            if (hit.overrideAirRecoveryType) {
                airRecoveryType = hit.newAirRecoveryType;
            } else {
                airRecoveryType = UFE.config.comboOptions.airRecoveryType;
            }

            // Add force to the move		
            // Air juggle deterioration (the longer the combo, the harder it is to push the opponent higher)
            if (pushForce.y > 0 || (isDead && !isKnockDown)) {

				if (UFE.config.comboOptions.airJuggleDeteriorationType == AirJuggleDeteriorationType.ComboHits){
					airJuggleHits = comboHits - 1;
				}
                if (UFE.config.comboOptions.airJuggleDeterioration == Sizes.VerySmall){
                    pushForce.y -= pushForce.y * airJuggleHits * .02;
				}else if (UFE.config.comboOptions.airJuggleDeterioration == Sizes.Small){
                    pushForce.y -= pushForce.y * airJuggleHits * .04;
				}else if (UFE.config.comboOptions.airJuggleDeterioration == Sizes.Medium){
                    pushForce.y -= pushForce.y * airJuggleHits * .1;
				}else if (UFE.config.comboOptions.airJuggleDeterioration == Sizes.High){
                    pushForce.y -= pushForce.y * airJuggleHits * .3;
				}else if (UFE.config.comboOptions.airJuggleDeterioration == Sizes.VeryHigh){
                    pushForce.y -= pushForce.y * airJuggleHits * .4;
				}
                if (pushForce.y < UFE.config.comboOptions._minPushForce) pushForce.y = UFE.config.comboOptions._minPushForce;
				airJuggleHits ++;
			}

            // Force a standard weight so the same air combo works on all characters
			if (UFE.config.comboOptions.fixJuggleWeight){
				myPhysicsScript.ApplyNewWeight(UFE.config.comboOptions._juggleWeight);
			}
            if (hit.overrideJuggleWeight) {
                myPhysicsScript.ApplyNewWeight(hit._newJuggleWeight);
            }
			
            // Restand the opponent (or juggle) if its an OTG
            if (currentState == PossibleStates.Down) {
                if (pushForce.y > 0) {
                    currentState = PossibleStates.NeutralJump;
                } else {
                    currentState = PossibleStates.Stand;
                }
            }

            if (airHit && airRecoveryType == AirRecoveryType.CantMove && hit.instantAirRecovery) 
                stunTime = 0.001;

            if (isDead) stunTime = 99999;

            if ((airHit || (!myPhysicsScript.IsGrounded() && airRecoveryType == AirRecoveryType.DontRecover)) && pushForce.y > 0)
            {
				if (myMoveSetScript.basicMoves.getHitAir.animMap[0].clip == null)
					Debug.LogError("Get Hit Air animation not found! Make sure you have it set on Character -> Basic Moves -> Get Hit Air");

				myPhysicsScript.ResetForces(hit.resetPreviousHorizontalPush, hit.resetPreviousVerticalPush, hit.resetPreviousSidewaysPush);
                myPhysicsScript.AddForce(pushForce, ignoreDirection || target == null? -GetDirection() : attacker.GetDirection());

                if (myMoveSetScript.basicMoves.getHitKnockBack.animMap[0].clip != null && 
                    pushForce.x > UFE.config.comboOptions._knockBackMinForce)
                {
                    currentHitAnimation = myMoveSetScript.basicMoves.getHitKnockBack.name;
                    currentHitInfo = myMoveSetScript.basicMoves.getHitKnockBack;
				}
                else
                {
					currentHitAnimation = myMoveSetScript.basicMoves.getHitAir.name;
					currentHitInfo = myMoveSetScript.basicMoves.getHitAir;
				}

                if (hit.overrideHitAnimationBlend)
                {
                    myMoveSetScript.PlayBasicMove(currentHitInfo, currentHitAnimation, hit._newHitBlendingIn, hit.resetHitAnimations);
                }
                else
                {
                    myMoveSetScript.PlayBasicMove(currentHitInfo, currentHitAnimation, hit.resetHitAnimations);
                }

                if (currentHitInfo.autoSpeed)
                {
                    // if the hit was in the air, calculate the time it will take for the character to hit the ground
                    Fix64 airTime = myPhysicsScript.GetPossibleAirTime(pushForce.y);

                    if (myMoveSetScript.basicMoves.fallingFromAirHit.animMap[0].clip == null) airTime *= 2;

                    if (stunTime > airTime || airRecoveryType == AirRecoveryType.DontRecover) { 
                        stunTime = airTime;
                    }

                    myMoveSetScript.SetAnimationNormalizedSpeed(currentHitAnimation, (myMoveSetScript.GetAnimationLength(currentHitAnimation) / stunTime));
                }
			}
            else
            {
                hitAnimationSpeed = 0;

				if (hit.hitType == HitType.HighKnockdown){
                    applyKnockdownForces(UFE.config.knockDownOptions.high, attacker);
                    myPhysicsScript.overrideAirAnimation = true;
                    airRecoveryType = AirRecoveryType.DontRecover;
                    if (!hit.customStunValues) stunTime =
                        UFE.config.knockDownOptions.high._knockedOutTime + UFE.config.knockDownOptions.high._standUpTime;

				}else if (hit.hitType == HitType.MidKnockdown){
                    applyKnockdownForces(UFE.config.knockDownOptions.highLow, attacker);
                    myPhysicsScript.overrideAirAnimation = true;
                    airRecoveryType = AirRecoveryType.DontRecover;
                    if (!hit.customStunValues) stunTime =
                        UFE.config.knockDownOptions.highLow._knockedOutTime + UFE.config.knockDownOptions.highLow._standUpTime;

				}else if (hit.hitType == HitType.Sweep){
                    applyKnockdownForces(UFE.config.knockDownOptions.sweep, attacker);
                    myPhysicsScript.overrideAirAnimation = true;
                    airRecoveryType = AirRecoveryType.DontRecover;
                    if (!hit.customStunValues) stunTime =
                        UFE.config.knockDownOptions.sweep._knockedOutTime + UFE.config.knockDownOptions.sweep._standUpTime;

				}

				hitAnimationSpeed = myMoveSetScript.GetAnimationLength(currentHitAnimation)/stunTime;

                if (hit.hitStrength == HitStrengh.Crumple) {
                    stunTime += UFE.config.knockDownOptions.crumple._knockedOutTime;
                }

                if (!myPhysicsScript.overrideAirAnimation) {
                    myPhysicsScript.ResetForces(hit.resetPreviousHorizontalPush, hit.resetPreviousVerticalPush, hit.resetPreviousSidewaysPush);
                    myPhysicsScript.AddForce(pushForce, ignoreDirection || target == null ? -GetDirection() : attacker.GetDirection());
                }

                // Set deceleration of hit stun animation so it can look more natural (deprecated)
                /*if (hit.overrideHitAcceleration) {
                    hitStunDeceleration = hitAnimationSpeed / 3;
                }*/

                if (hit.overrideHitAnimationBlend){
                    myMoveSetScript.PlayBasicMove(currentHitInfo, currentHitAnimation, hit._newHitBlendingIn, hit.resetHitAnimations);
                }else{
                    myMoveSetScript.PlayBasicMove(currentHitInfo, currentHitAnimation, hit.resetHitAnimations);
                }

                if (currentHitInfo.autoSpeed && hitAnimationSpeed > 0) {
                    myMoveSetScript.SetAnimationSpeed(currentHitAnimation, hitAnimationSpeed);
                }
			}
		}
		
		// Freeze screen depending on how strong the hit was
		HitPause(GetHitAnimationSpeed(hit.hitStrength) * .01);
		UFE.DelaySynchronizedAction(this.HitUnpause, hitEffects._freezingTime);
    }

    private Vector3 GetParticleSpawnPoint(HitEffectSpawnPoint spawnPoint, FPVector[] locations) {
        if (spawnPoint == HitEffectSpawnPoint.StrikingHurtBox) {
            return locations[0].ToVector();
        } else if (spawnPoint == HitEffectSpawnPoint.StrokeHitBox) {
            return locations[1].ToVector();
        } else {
            return locations[2].ToVector();
        }
    }

	private void applyKnockdownForces(SubKnockdownOptions knockdownOptions, ControlsScript attacker){
		myPhysicsScript.ResetForces(true, true);
		myPhysicsScript.AddForce(knockdownOptions._predefinedPushForce, attacker.GetDirection());
	}

	private string GetHitAnimation(BasicMoveInfo hitMove, Hit hit){
		if (hit.hitStrength == HitStrengh.Weak) return hitMove.name;
		if (hitMove.animMap[1].clip != null && hit.hitStrength == HitStrengh.Medium) return myMoveSetScript.GetAnimationString(hitMove, 2);
		if (hitMove.animMap[2].clip != null && hit.hitStrength == HitStrengh.Heavy) return myMoveSetScript.GetAnimationString(hitMove, 3);
		if (hitMove.animMap[3].clip != null && hit.hitStrength == HitStrengh.Custom1) return myMoveSetScript.GetAnimationString(hitMove, 4);
		if (hitMove.animMap[4].clip != null && hit.hitStrength == HitStrengh.Custom2) return myMoveSetScript.GetAnimationString(hitMove, 5);
		if (hitMove.animMap[5].clip != null && hit.hitStrength == HitStrengh.Custom3) return myMoveSetScript.GetAnimationString(hitMove, 6);
		if (hitMove.animMap.Length > 6 && hitMove.animMap[6].clip != null && hit.hitStrength == HitStrengh.Custom4) return myMoveSetScript.GetAnimationString(hitMove, 7);
		if (hitMove.animMap.Length > 7 && hitMove.animMap[7].clip != null && hit.hitStrength == HitStrengh.Custom5) return myMoveSetScript.GetAnimationString(hitMove, 8);
		if (hitMove.animMap.Length > 8 && hitMove.animMap[8].clip != null && hit.hitStrength == HitStrengh.Custom6) return myMoveSetScript.GetAnimationString(hitMove, 9);
		return hitMove.name;
	}

	public void ToggleHeadLook(bool flag){
		if (headLookScript != null && myInfo.headLook.enabled) headLookScript.enabled = flag;
	}

	// Pause animations and physics to create a sense of impact
	public void HitPause(){
		HitPause(0);
	}

	public void HitPause(Fix64 animSpeed){
		if (shakeCamera) Camera.main.transform.position += Vector3.forward/2;
		myPhysicsScript.freeze = true;
		
		PausePlayAnimation(true, animSpeed);
	}
	
	// Unpauses the pause
	public void HitUnpause(){
        if (cameraScript.cinematicFreeze) return;
        myPhysicsScript.freeze = false;

		PausePlayAnimation(false);
	}

	// Method to pause animations and return them to their prior speed accordly
	private void PausePlayAnimation(bool pause){
		PausePlayAnimation(pause, 0);
	}

	private void PausePlayAnimation(bool pause, Fix64 animSpeed){
		if (animSpeed < 0) animSpeed = 0;
		if (pause){
			myMoveSetScript.SetAnimationSpeed(myMoveSetScript.GetAnimationSpeed() * animSpeed);
		}else {
			myMoveSetScript.RestoreAnimationSpeed();
		}
	}

    public void AddGauge(Fix64 gaugeGain, int targetGauge) {
        if ((isDead || opControlsScript.isDead) && UFE.config.roundOptions.inhibitGaugeGain) return;
		if (!UFE.config.gameGUI.hasGauge) return;
        if (inhibitGainWhileDraining) return;

		currentGaugesPoints[targetGauge] += (myInfo.maxGaugePoints * (gaugeGain/100));
        if (currentGaugesPoints[targetGauge] > myInfo.maxGaugePoints) currentGaugesPoints[targetGauge] = myInfo.maxGaugePoints;
    }

    private void RemoveGauge(Fix64 gaugeLoss, int targetGauge) {
        if ((isDead || opControlsScript.isDead) && UFE.config.roundOptions.inhibitGaugeGain) return;
		if (!UFE.config.gameGUI.hasGauge) return;
        if ((UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.ChallengeMode)
            && playerNum == 1 && UFE.config.trainingModeOptions.p1Gauge == LifeBarTrainingMode.Infinite) return;
        if ((UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.ChallengeMode)
            && playerNum == 2 && UFE.config.trainingModeOptions.p2Gauge == LifeBarTrainingMode.Infinite) return;
        currentGaugesPoints[targetGauge] -= (myInfo.maxGaugePoints * (gaugeLoss / 100));
		if (currentGaugesPoints[targetGauge] < 0) currentGaugesPoints[targetGauge] = 0;

        if ((UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.ChallengeMode)
            && ((playerNum == 1 && UFE.config.trainingModeOptions.p1Gauge == LifeBarTrainingMode.Refill)
            || (playerNum == 2 && UFE.config.trainingModeOptions.p2Gauge == LifeBarTrainingMode.Refill))) {
                if (!UFE.FindAndUpdateDelaySynchronizedAction(this.RefillGauge, UFE.config.trainingModeOptions.refillTime)) 
				UFE.DelaySynchronizedAction(this.RefillGauge, UFE.config.trainingModeOptions.refillTime);
		}
	}
	
	public bool DamageMe(Fix64 damage, bool doesntKill){
		if (doesntKill && damage >= currentLifePoints) damage = currentLifePoints - 1;
		return DamageMe(damage);
	}
	
	private void RefillLife(){
		currentLifePoints = myInfo.lifePoints;
		UFE.FireLifePoints(myInfo.lifePoints, opControlsScript);
	}
	
	private void RefillGauge(){
        for (int i = 0; i < currentGaugesPoints.Length; i++) AddGauge(myInfo.maxGaugePoints, i);
	}

	private bool DamageMe(Fix64 damage){
        if ((UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.ChallengeMode)
            && playerNum == 1 && UFE.config.trainingModeOptions.p1Life == LifeBarTrainingMode.Infinite) return false;
        if ((UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.ChallengeMode)
            && playerNum == 2 && UFE.config.trainingModeOptions.p2Life == LifeBarTrainingMode.Infinite) return false;
		if (currentLifePoints <= 0) return true;
		if (UFE.GetTimer() <= 0 && UFE.config.roundOptions.hasTimer) return true;

		currentLifePoints -= damage;
		if (currentLifePoints < 0) currentLifePoints = 0;
		UFE.FireLifePoints(currentLifePoints, opControlsScript);

        if ((UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.ChallengeMode)
            && ((playerNum == 1 && UFE.config.trainingModeOptions.p1Life == LifeBarTrainingMode.Refill) 
            || (playerNum == 2 && UFE.config.trainingModeOptions.p2Life == LifeBarTrainingMode.Refill))) {
                if (currentLifePoints == 0) currentLifePoints = myInfo.lifePoints;
                if (!UFE.FindAndUpdateDelaySynchronizedAction(this.RefillLife, UFE.config.trainingModeOptions.refillTime)) {
                    UFE.DelaySynchronizedAction(this.RefillLife, UFE.config.trainingModeOptions.refillTime);
                }
		}

        if ((UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.ChallengeMode)
            && playerNum == 1 && UFE.config.trainingModeOptions.p1Life != LifeBarTrainingMode.Normal) return false;
        if ((UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.ChallengeMode)
            && playerNum == 2 && UFE.config.trainingModeOptions.p2Life != LifeBarTrainingMode.Normal) return false;

		if (currentLifePoints == 0) return true;
		return false;
	}

    private void StartNextChallenge() {
        UFE.config.lockInputs = true;
        UFE.config.lockMovements = true;
        UFE.DelaySynchronizedAction(UFE.StartFight, (Fix64)2);

        if (challengeMode.resetRound) {
            UFE.ResetTimer();

            ResetData(true);
            opControlsScript.ResetData(false);
        }

        challengeMode.Run();
    }

	public void SetMoveToOutro(){
        KillCurrentMove();
        this.SetMove(myMoveSetScript.GetOutro());

		outroPlayed = true;
	}

	public void ResetData(bool resetLife)
    {
		if (UFE.config.roundOptions.resetPositions)
        {
            myMoveSetScript.PlayBasicMove(myMoveSetScript.basicMoves.idle, myMoveSetScript.basicMoves.idle.name, 0);
			myPhysicsScript.ForceGrounded();

            currentState = PossibleStates.Stand;
            currentSubState = SubStates.Resting;
            stunTime = 0;
        }
        else if (currentState == PossibleStates.Down && myPhysicsScript.IsGrounded())
        {
            stunTime = 1;
        }

		if (resetLife || UFE.config.roundOptions.resetLifePoints)
        {
            if (playerNum == 1 && (UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.ChallengeMode)) {
                currentLifePoints = (Fix64)myInfo.lifePoints * (UFE.config.trainingModeOptions.p1StartingLife / 100);
            } else if (playerNum == 2 && (UFE.gameMode == GameMode.TrainingRoom || UFE.gameMode == GameMode.ChallengeMode)) {
                currentLifePoints = (Fix64)myInfo.lifePoints * (UFE.config.trainingModeOptions.p2StartingLife / 100);
            } else {
                currentLifePoints = myInfo.lifePoints;
            }
		}

		blockStunned = false;
		comboHits = 0;
		comboDamage = 0;
		comboHitDamage = 0;
		airJuggleHits = 0;
		CheckBlocking(false);
		isDead = false;
		myPhysicsScript.isTakingOff = false;
		myPhysicsScript.isLanding = false;
		
		myPhysicsScript.ResetWeight();
		ToggleHeadLook(true);
	}

	// Get amount of freezing time depending on the Strengtht of the move
	public Fix64 GetHitAnimationSpeed(HitStrengh hitStrength){
		if (hitStrength == HitStrengh.Weak){
			return UFE.config.hitOptions.weakHit._animationSpeed;
		} else if (hitStrength == HitStrengh.Medium){
			return UFE.config.hitOptions.mediumHit._animationSpeed;
		}else if (hitStrength == HitStrengh.Heavy){
			return UFE.config.hitOptions.heavyHit._animationSpeed;
		}else if (hitStrength == HitStrengh.Crumple){
			return UFE.config.hitOptions.crumpleHit._animationSpeed;
        } else if (hitStrength == HitStrengh.Custom1) {
            return UFE.config.hitOptions.customHit1._animationSpeed;
        } else if (hitStrength == HitStrengh.Custom2) {
            return UFE.config.hitOptions.customHit2._animationSpeed;
        } else if (hitStrength == HitStrengh.Custom3) {
            return UFE.config.hitOptions.customHit3._animationSpeed;
        } else if (hitStrength == HitStrengh.Custom4) {
            return UFE.config.hitOptions.customHit4._animationSpeed;
        } else if (hitStrength == HitStrengh.Custom5) {
            return UFE.config.hitOptions.customHit5._animationSpeed;
        } else if (hitStrength == HitStrengh.Custom6) {
            return UFE.config.hitOptions.customHit6._animationSpeed;
		}
		return 0;
	}

	// Get amount of freezing time depending on the Strengtht of the move
	public Fix64 GetHitFreezingTime(HitStrengh hitStrength){
		if (hitStrength == HitStrengh.Weak){
			return UFE.config.hitOptions.weakHit._freezingTime;
		} else if (hitStrength == HitStrengh.Medium){
			return UFE.config.hitOptions.mediumHit._freezingTime;
		}else if (hitStrength == HitStrengh.Heavy){
			return UFE.config.hitOptions.heavyHit._freezingTime;
		}else if (hitStrength == HitStrengh.Crumple){
            return UFE.config.hitOptions.crumpleHit._freezingTime;
        } else if (hitStrength == HitStrengh.Custom1) {
            return UFE.config.hitOptions.customHit1._freezingTime;
        } else if (hitStrength == HitStrengh.Custom2) {
            return UFE.config.hitOptions.customHit2._freezingTime;
        } else if (hitStrength == HitStrengh.Custom3) {
            return UFE.config.hitOptions.customHit3._freezingTime;
        } else if (hitStrength == HitStrengh.Custom4) {
            return UFE.config.hitOptions.customHit4._freezingTime;
        } else if (hitStrength == HitStrengh.Custom5) {
            return UFE.config.hitOptions.customHit5._freezingTime;
        } else if (hitStrength == HitStrengh.Custom6) {
            return UFE.config.hitOptions.customHit6._freezingTime;
		}
		return 0;
	}

    // Shake Camera
    void shakeCam(){
        float rnd = Random.Range((float)-shakeCameraDensity * .1f, (float)shakeCameraDensity * .1f);
        Camera.main.transform.position += new Vector3(rnd, rnd, 0);
	}

    // Shake Character while being hit and in freezing mode
    void shake() {
        Fix64 rnd = FPRandom.Range(-shakeDensity * .1, shakeDensity * .1);
        localTransform.position = new FPVector(localTransform.position.x + rnd, localTransform.position.y, localTransform.position.z);
    }
    
    public void SetActive(bool value) {
        gameObject.SetActive(value);
    }
    
    public bool GetActive() {
        return gameObject.activeInHierarchy;
    }
}
