using UnityEngine;
using System.Collections;
using FPLibrary;
using UFENetcode;
using UFE3D;

public class PhysicsScript : MonoBehaviour {
    #region trackable definitions
    public FPVector activeForces;
    public Fix64 airTime;
    public Fix64 angularDirection;
    public Fix64 appliedGravity;
    public int currentAirJumps;
    public bool freeze;
    public int groundBounceTimes;
    public Fix64 horizontalJumpForce;
    public bool isGroundBouncing;
    public bool isLanding;
    public bool isTakingOff;
    public bool isWallBouncing;
    public Fix64 moveDirection;
    public bool overrideAirAnimation;
    public BasicMoveInfo overrideStunAnimation;
    public Fix64 verticalTotalForce;
    public int wallBounceTimes;
    #endregion

    public ControlsScript controlScript;
    public MoveSetScript moveSetScript;

    private FPTransform worldTransform { get { return controlScript.worldTransform; } set { controlScript.worldTransform = value; } }
    private FPTransform opWorldTransform {
        get
        { return (controlScript.target != null) ? controlScript.target.worldTransform : controlScript.opControlsScript.worldTransform; }
        set
        { if (controlScript.target != null) controlScript.target.worldTransform = value; else controlScript.opControlsScript.worldTransform = value; }
    }

    public void Start()
    {
		appliedGravity = controlScript.myInfo.physics._weight * UFE.config._gravity;
	}

    /// <summary>
    /// (2D Fighter/ 3D Fighter) Makes the character move forward or backwards.
    /// </summary>
    /// <param name="direction">The direction the character is facing.</param>
    /// <param name="axisValue">The moving forward/backwards force percentage being applied (-1 to 1).</param>
    public void MoveX(int direction, Fix64 axisValue)
    {
		if (!IsGrounded()) return;
		if (freeze) return;
		if (isTakingOff) return;
        if (isLanding) return;
        if (activeForces.y != 0) return;

        if (UFE.config.inputOptions.forceDigitalInput) axisValue = axisValue < 0 ? -1 : 1;
        moveDirection = axisValue;

        angularDirection = transform.rotation.eulerAngles.y;

        if (direction == 1)
        {
			controlScript.currentSubState = SubStates.MovingForward;
            if (!IsJumping()) activeForces.x = controlScript.myInfo.physics._moveForwardSpeed * axisValue;
		}
        else
        {
			controlScript.currentSubState = SubStates.MovingBack;
            if (!IsJumping()) activeForces.x = controlScript.myInfo.physics._moveBackSpeed * axisValue;
        }
    }

    /// <summary>
    /// (3D Fighter) Makes the character move sideways.
    /// </summary>
    /// <param name="direction">The direction the character is facing.</param>
    /// <param name="axisValue">The Move Sideways Speed force percentage being applied (-1 to 1).</param>
    public void MoveZ(int direction, Fix64 axisValue)
    {
        if (!IsGrounded()) return;
        if (freeze) return;
        if (isTakingOff) return;
        if (isLanding) return;
        //if (!controlScript.myInfo.customControls.disableJump && moveDirection != 0) return;

        angularDirection = transform.rotation.eulerAngles.y;

        if (UFE.config.inputOptions.forceDigitalInput) axisValue = axisValue < 0 ? -1 : 1;

        controlScript.currentSubState = SubStates.MovingSideways;

        activeForces.z = controlScript.myInfo.physics._moveSidewaysSpeed * axisValue * -direction;
        moveDirection = 1;
    }

    /// <summary>
    /// (3D Arena) Makes the character move towards the direction its facing.
    /// </summary>
    public void Move(Fix64 axisValue)
    {
        if (!IsGrounded()) return;
        if (freeze) return;
        if (isTakingOff) return;
        if (isLanding) return;

        angularDirection = transform.rotation.eulerAngles.y;

        moveDirection = 1;
        controlScript.currentSubState = SubStates.MovingForward;
        activeForces.x = controlScript.myInfo.physics._moveForwardSpeed * axisValue;
    }

    /// <summary>
    /// Makes the character jump using the jump force indicated under Character - Mininum Jump Force.
    /// </summary>
    public void MinJump() {
        Jump(controlScript.myInfo.physics._minJumpForce);
    }

    /// <summary>
    /// Makes the character jump using the jump force indicated under Character - Jump Force.
    /// </summary>
    public void Jump() {
        Jump(controlScript.myInfo.physics._jumpForce);
    }

    /// <summary>
    /// Makes the character jump.
    /// </summary>
    /// <param name="push">Force being applied to the jump.</param>
    public void Jump(Fix64 jumpForce) {
		if (isTakingOff && currentAirJumps > 0) return;
		if (controlScript.currentMove != null) return;

        isTakingOff = false;
		isLanding = false;
		controlScript.storedMove = null;
		controlScript.potentialBlock = false;

		if (controlScript.currentState == PossibleStates.Down) return;
		if (controlScript.currentSubState == SubStates.Stunned || controlScript.currentSubState == SubStates.Blocking) return;
		if (currentAirJumps >= controlScript.myInfo.physics.multiJumps) return;

        if (controlScript.currentSubState == SubStates.MovingBack)
            horizontalJumpForce = controlScript.myInfo.physics._jumpBackDistance;
        else if (controlScript.currentSubState == SubStates.MovingForward)
            horizontalJumpForce = controlScript.myInfo.physics._jumpDistance;
        else
            horizontalJumpForce = 0;

        currentAirJumps++;
        activeForces.x = 0;
        activeForces.y = jumpForce;
		setVerticalData(jumpForce);
    }

    /// <summary>
    /// Is the character jumping?
    /// </summary>
    public bool IsJumping() {
		return (currentAirJumps > 0);
	}

    /// <summary>
    /// Is the character moving forward or backwards?
    /// </summary>
    public bool IsMoving() {
		return (moveDirection != 0);
	}

    public void ResetLanding() {
        isLanding = false;
    }

    /// <summary>
    /// Resets the forces currently in motion.
    /// </summary>
    public void ResetForces(bool resetX, bool resetY, bool resetZ = false) {
        if (resetX) {
            activeForces.x = 0;
            moveDirection = 0;
            horizontalJumpForce = 0;
        }
		if (resetY) activeForces.y = 0;
		if (resetZ) activeForces.z = 0;
	}

    /// <summary>
    /// Apply cumulative forces to the character.
    /// </summary>
    /// <param name="push">Force vector being applied.</param>
    /// <param name="direction">The direction the character is facing. 1 = facing right. -1 = facing left.</param>
    public void AddForce(FPVector push, int direction)
    {
        AddForce(push, direction, false);
    }

    /// <summary>
    /// Apply cumulative forces to the character.
    /// </summary>
    /// <param name="push">Force vector being applied.</param>
    /// <param name="direction">The direction the character is facing. 1 = facing right. -1 = facing left.</param>
    /// <param name="selfApplied">(3D only) Indicates if the force is being applied to the character itself or the target.</param>
    public void AddForce(FPVector push, int direction, bool selfApplied) {
		push.x *= direction;
		push.z *= direction;
        isGroundBouncing = false;
        isWallBouncing = false;

		if (!controlScript.myInfo.physics.cumulativeForce)
        {
			activeForces.x = 0;
            activeForces.y = 0;
            activeForces.z = 0;
		}

        if (activeForces.y < 0 && push.y > 0 && UFE.config.comboOptions.resetFallingForceOnHit) activeForces.y = 0;

        activeForces.y += push.y;

        if (UFE.config.gameplayType == GameplayType._2DFighter)
        {
            activeForces.x += push.x;
        }
        else
        {
            if (selfApplied)
            {
                angularDirection = transform.rotation.eulerAngles.y;
                activeForces.x += push.x;
            }
            else
            {
                angularDirection = controlScript.opControlsScript.transform.rotation.eulerAngles.y;
                activeForces.x -= push.x;
            }
            activeForces.z += push.z;
        }

		setVerticalData(activeForces.y);
	}
	
	private void setVerticalData(Fix64 appliedForce) {
        Fix64 maxHeight = (appliedForce * appliedForce) / (appliedGravity * 2);
		maxHeight += worldTransform.position.y;
        airTime = FPMath.Sqrt(FPMath.Abs(maxHeight * 2 / appliedGravity));
		verticalTotalForce = appliedGravity * airTime;
	}

	public void ApplyNewWeight(Fix64 newWeight) {
		appliedGravity = newWeight * UFE.config._gravity;
	}

	public void ResetWeight(){
		appliedGravity = controlScript.myInfo.physics._weight * UFE.config._gravity;
	}
	
	public Fix64 GetPossibleAirTime(Fix64 appliedForce) {
        Fix64 maxHeight = (appliedForce * appliedForce) / (appliedGravity * 2);
		maxHeight += worldTransform.position.y;
        return FPMath.Sqrt(maxHeight * 2 / appliedGravity);
	}

    /// <summary>
    /// Imediately moves the character to the ground on a stand state.
    /// </summary>
    public void ForceGrounded() {
        activeForces = FPVector.zero;
		setVerticalData(0);
		currentAirJumps = 0;
		isTakingOff = false;
        isLanding = false;
        isGroundBouncing = false;
        isWallBouncing = false;
		if (worldTransform.position.y != 0) worldTransform.Translate(new FPVector(0, -worldTransform.position.y, 0));
        activeForces.y = 0;
        controlScript.currentState = PossibleStates.Stand;
	}

    public void ApplyForces() {
		ApplyForces(null);
	}

	public void ApplyForces(MoveInfo move) {
		if (freeze) return;

        controlScript.normalizedJumpArc = 1 - ((activeForces.y + verticalTotalForce) / (verticalTotalForce * 2));
        
		Fix64 appliedFriction = (moveDirection != 0 || controlScript.myInfo.physics.highMovingFriction) ? UFE.config.selectedStage._groundFriction : controlScript.myInfo.physics._friction;

        if (move != null && move.ignoreFriction) appliedFriction = 0;

        if (controlScript.activePullIn != null)
        {
            bool isPulling = FPVector.Distance(opWorldTransform.position, worldTransform.position) >= controlScript.activePullIn._targetDistance ? true : false;
            bool opIsMoving = false;
            FPVector targetPosition = controlScript.activePullIn.position;

            if (controlScript.activePullIn.forceGrounded && !IsGrounded()) ForceGrounded();

            worldTransform.position = FPVector.Lerp(worldTransform.position,
                                                targetPosition,
                                                UFE.fixedDeltaTime * controlScript.activePullIn.speed);

            if (UFE.config.gameplayType == GameplayType._2DFighter)
            {
                if (worldTransform.position.x <= UFE.config.selectedStage.position.x + UFE.config.selectedStage._leftBoundary ||
                worldTransform.position.x >= UFE.config.selectedStage.position.x + UFE.config.selectedStage._rightBoundary)
                {
                    targetPosition = opWorldTransform.position;
                    targetPosition.x = (worldTransform.position.x <= UFE.config.selectedStage.position.x + UFE.config.selectedStage._leftBoundary) ?
                        worldTransform.position.x + controlScript.activePullIn._targetDistance : worldTransform.position.x - controlScript.activePullIn._targetDistance;
                    opIsMoving = true;
                }
            }
            else
            {
                if (FPVector.Distance(worldTransform.position, FPVector.zero) > UFE.config.selectedStage._rightBoundary) // Distance from center is higher than stage radius
                {
                    targetPosition += opWorldTransform.forward * -1;
                    opIsMoving = true;
                }
            }

            if (opIsMoving) opWorldTransform.position = FPVector.Lerp(opWorldTransform.position, targetPosition, UFE.fixedDeltaTime * controlScript.activePullIn.speed);

            if ((!isPulling && FPVector.Distance(opWorldTransform.position, worldTransform.position) >= controlScript.activePullIn._targetDistance) ||
                (isPulling && FPVector.Distance(opWorldTransform.position, worldTransform.position) <= controlScript.activePullIn._targetDistance))
            {
                controlScript.activePullIn = null;
                if (opIsMoving)
                {
                    opWorldTransform.position = targetPosition;
                }
                else
                {
                    worldTransform.position = targetPosition;
                }
            }
        }
        else
        {
			if (!IsGrounded()) {
				appliedFriction = 0;
				if (activeForces.y == 0) activeForces.y = .1 * -1;
			}

			if ((activeForces.x != 0 || activeForces.z != 0) && !isTakingOff) {
				if (activeForces.x > 0) {
                    activeForces.x -= appliedFriction * UFE.fixedDeltaTime;
                    activeForces.x = FPMath.Max(0, activeForces.x);
				}else if (activeForces.x < 0) {
                    activeForces.x += appliedFriction * UFE.fixedDeltaTime;
                    activeForces.x = FPMath.Min(0, activeForces.x);
				}
				if (activeForces.z > 0) {
                    activeForces.z -= appliedFriction * UFE.fixedDeltaTime;
                    activeForces.z = FPMath.Max(0, activeForces.z);
				}else if (activeForces.z < 0) {
                    activeForces.z += appliedFriction * UFE.fixedDeltaTime;
                    activeForces.z = FPMath.Min(0, activeForces.z);
				}

                bool bouncingOnCamera = false;
                if (controlScript.currentHit != null && controlScript.currentHit.bounceOnCameraEdge)
                {
                    if (UFE.config.gameplayType == GameplayType._2DFighter)
                    {
                        Fix64 leftCameraBounds = opWorldTransform.position.x - UFE.config.cameraOptions._maxDistance;
                        Fix64 rightCameraBounds = opWorldTransform.position.x + UFE.config.cameraOptions._maxDistance;
                        if (worldTransform.position.x <= leftCameraBounds - controlScript.currentHit.cameraEdgeOffSet || worldTransform.position.x >= rightCameraBounds + controlScript.currentHit.cameraEdgeOffSet)
                            bouncingOnCamera = true;
                    }
                    else
                    {
                        if (FPVector.Distance(opWorldTransform.position, worldTransform.position) > UFE.config.cameraOptions._maxDistance - controlScript.currentHit.cameraEdgeOffSet)
                            bouncingOnCamera = true;
                    }
                }

                bool bouncingOnBounds = false;
                if (UFE.config.gameplayType == GameplayType._2DFighter)
                {
                    if (worldTransform.position.x <= UFE.config.selectedStage.position.x + UFE.config.selectedStage._leftBoundary
                    || worldTransform.position.x >= UFE.config.selectedStage.position.x + UFE.config.selectedStage._rightBoundary)
                        bouncingOnBounds = true;
                }
                else
                {
                    if (FPVector.Distance(worldTransform.position, FPVector.zero) > UFE.config.selectedStage._rightBoundary) // Distance from center is higher than stage radius
                        bouncingOnBounds = true;
                }

                if (wallBounceTimes < UFE.config.wallBounceOptions._maximumBounces 
                    && controlScript.currentSubState == SubStates.Stunned
                    && controlScript.currentState != PossibleStates.Down
                    && UFE.config.wallBounceOptions.bounceForce != Sizes.None
                    && FPMath.Abs(activeForces.x) >= UFE.config.wallBounceOptions._minimumBounceForce
                    && (bouncingOnBounds || bouncingOnCamera)
                    && controlScript.currentHit != null && controlScript.currentHit.wallBounce
                    && !isWallBouncing) {
                    
                    if (controlScript.currentHit.overrideForcesOnWallBounce) {
                        if (controlScript.currentHit.resetWallBounceHorizontalPush) activeForces.x = 0;
                        if (controlScript.currentHit.resetWallBounceVerticalPush) activeForces.y = 0;

                        Fix64 addedH = -controlScript.currentHit._wallBouncePushForce.x;
                        Fix64 addedV = controlScript.currentHit._wallBouncePushForce.y;
                        Fix64 addedZ = (UFE.config.gameplayType == GameplayType._2DFighter) ? 0 : controlScript.currentHit._wallBouncePushForce.z;

                        if (controlScript.currentHit != null) controlScript.TestRotationOnHit(controlScript.opControlsScript, controlScript.currentHit);
                        AddForce(new FPVector(addedH, addedV, addedZ), -controlScript.opControlsScript.mirror);

                    } else {
                        if (UFE.config.wallBounceOptions.bounceForce == Sizes.VerySmall) {
                            activeForces.x *= -(Fix64).8;
                        } else if (UFE.config.wallBounceOptions.bounceForce == Sizes.Small) {
                            activeForces.x *= -(Fix64).7;
                        } else if (UFE.config.wallBounceOptions.bounceForce == Sizes.Medium) {
                            activeForces.x *= -(Fix64).6;
                        } else if (UFE.config.wallBounceOptions.bounceForce == Sizes.High) {
                            activeForces.x *= -(Fix64)1;
                        } else if (UFE.config.wallBounceOptions.bounceForce == Sizes.VeryHigh) {
                            activeForces.x *= -(Fix64)1.2;
                        }
                    }

                    wallBounceTimes++;

                    if (activeForces.y > 0 || !IsGrounded()) {
                        if (moveSetScript.basicMoves.airWallBounce.animMap[0].clip != null) {
                            moveSetScript.PlayBasicMove(moveSetScript.basicMoves.airWallBounce);
                            controlScript.currentHitAnimation = moveSetScript.basicMoves.airWallBounce.name;
                        }
                    } else {
                        if (controlScript.currentHit.knockOutOnWallBounce) {
                            moveSetScript.PlayBasicMove(moveSetScript.basicMoves.standingWallBounceKnockdown);
                            controlScript.currentHitAnimation = moveSetScript.basicMoves.standingWallBounceKnockdown.name;
                        } else {
                            moveSetScript.PlayBasicMove(moveSetScript.basicMoves.standingWallBounce);
                            controlScript.currentHitAnimation = moveSetScript.basicMoves.standingWallBounce.name;
                        }
                    }

                    if (UFE.config.wallBounceOptions.bouncePrefab != null) {
                        GameObject pTemp = UFE.SpawnGameObject(UFE.config.wallBounceOptions.bouncePrefab, transform.position, Quaternion.identity, Mathf.RoundToInt(UFE.config.wallBounceOptions.bounceKillTime * UFE.config.fps));
                        pTemp.transform.rotation = UFE.config.wallBounceOptions.bouncePrefab.transform.rotation;
                        if (UFE.config.wallBounceOptions.sticky) pTemp.transform.parent = transform;
                    }

                    if (UFE.config.wallBounceOptions.shakeCamOnBounce) {
                        controlScript.shakeCameraDensity = UFE.config.wallBounceOptions._shakeDensity;
                    }

                    UFE.PlaySound(UFE.config.wallBounceOptions.bounceSound);
                    isWallBouncing = true;
                }

                FPVector distance = new FPVector(30, 0, 0);
                if (UFE.config.gameplayType == GameplayType._2DFighter)
                {
                    worldTransform.Translate(activeForces.x * UFE.fixedDeltaTime, 0, 0);
                }
#if !UFE_LITE && !UFE_BASIC
                else if (UFE.config.gameplayType == GameplayType._3DFighter)
                {
                    Fix64 distanceModifier = (1.3 - controlScript.normalizedDistance) * 10;
                    worldTransform.RotateAround(opWorldTransform.position, FPVector.up, activeForces.z * distanceModifier * UFE.fixedDeltaTime);

                    FPVector target = (FPQuaternion.Euler(0, angularDirection - 90, 0) * distance) + worldTransform.position;
                    worldTransform.position = FPVector.MoveTowards(worldTransform.position, target, activeForces.x * UFE.fixedDeltaTime * -controlScript.mirror);
                }
                else
                {
                    FPVector target = (FPQuaternion.Euler(0, angularDirection - 90, 0) * distance) + worldTransform.position;
                    worldTransform.position = FPVector.MoveTowards(worldTransform.position, target, activeForces.x * UFE.fixedDeltaTime);
                }
#endif
            }
			
			if (move == null || (move != null && !move.ignoreGravity)) {
				if ((activeForces.y < 0 && !IsGrounded()) || activeForces.y > 0)
                {
                    activeForces.y -= appliedGravity * UFE.fixedDeltaTime;
                    if (UFE.config.gameplayType == GameplayType._2DFighter)
                    {
                        worldTransform.Translate(horizontalJumpForce * moveDirection * UFE.fixedDeltaTime, activeForces.y * UFE.fixedDeltaTime, 0);
                    }
#if !UFE_LITE && !UFE_BASIC
                    else
                    {
                        FPVector newPosition = worldTransform.position;
                        newPosition.y += activeForces.y * UFE.fixedDeltaTime;
                        worldTransform.position = newPosition;

                        FPVector target = (FPQuaternion.Euler(0, transform.rotation.eulerAngles.y - 90, 0) * FPVector.right) + worldTransform.position;
                        int direction = UFE.config.gameplayType == GameplayType._3DFighter ? -controlScript.mirror : 1;
                        worldTransform.position = FPVector.MoveTowards(worldTransform.position, target, horizontalJumpForce * moveDirection * UFE.fixedDeltaTime * direction);
                    }
#endif
                }
                else if (activeForces.y < 0 && IsGrounded() && controlScript.currentSubState != SubStates.Stunned)
                {
                    activeForces.y = 0;
				}
			}
		}


        // Clamp Max Distance Between Players
        if (!controlScript.isAssist)
        {
            if (UFE.config.gameplayType == GameplayType._2DFighter)
            {
                Fix64 minDist = opWorldTransform.position.x - UFE.config.cameraOptions._maxDistance;
                Fix64 maxDist = opWorldTransform.position.x + UFE.config.cameraOptions._maxDistance;
                worldTransform.position = new FPVector(FPMath.Clamp(worldTransform.position.x, minDist, maxDist), worldTransform.position.y, worldTransform.position.z);
            }
#if !UFE_LITE && !UFE_BASIC
            else
            {
                if (FPVector.Distance(opWorldTransform.position, worldTransform.position) > UFE.config.cameraOptions._maxDistance)
                {
                    FPVector center = (opWorldTransform.position + worldTransform.position) / 2;
                    FPVector offset = worldTransform.position - center;
                    worldTransform.position = center + FPVector.ClampMagnitude(offset, UFE.config.cameraOptions._maxDistance / 2);
                }
            }
#endif
        }


        // Clamp Max Stage Distance
        if (UFE.config.gameplayType == GameplayType._2DFighter)
        {
            worldTransform.position = new FPVector(
            FPMath.Clamp(worldTransform.position.x, UFE.config.selectedStage.position.x + UFE.config.selectedStage._leftBoundary, UFE.config.selectedStage.position.x + UFE.config.selectedStage._rightBoundary),
            FPMath.Max(worldTransform.position.y, UFE.config.selectedStage.position.y),
            worldTransform.position.z);
        }
        else
        {
            FPVector centerPosition = FPVector.zero;
            Fix64 distanceFromCenter = FPVector.Distance(worldTransform.position, centerPosition);
            Fix64 radius = UFE.config.selectedStage._rightBoundary;
            if (distanceFromCenter > radius)
            {
                FPVector fromOriginToObject = worldTransform.position - centerPosition;
                fromOriginToObject *= radius / distanceFromCenter;
                worldTransform.position = centerPosition + fromOriginToObject;
            }
        }


        // Clamp Ground Distance
        if (worldTransform.position.y < 0) worldTransform.Translate(new FPVector(0, -worldTransform.position.y, 0));

        if (controlScript.currentState == PossibleStates.Down) return;

		if (IsGrounded()){
            if (verticalTotalForce != 0) {
				if (groundBounceTimes < UFE.config.groundBounceOptions._maximumBounces 
                    && controlScript.currentSubState == SubStates.Stunned 
                    && UFE.config.groundBounceOptions.bounceForce != Sizes.None 
                    && activeForces.y <= -UFE.config.groundBounceOptions._minimumBounceForce
                    && controlScript.currentHit.groundBounce)
                {
                    if (controlScript.currentHit.overrideForcesOnGroundBounce) {
                        if (controlScript.currentHit.resetGroundBounceHorizontalPush) activeForces.x = 0;
                        if (controlScript.currentHit.resetGroundBounceVerticalPush) activeForces.y = 0;

                        Fix64 addedH = controlScript.currentHit._groundBouncePushForce.x;
                        Fix64 addedV = controlScript.currentHit._groundBouncePushForce.y;

                        AddForce(new FPVector(addedH, addedV, 0), controlScript.mirror);

                    } else {
                        if (UFE.config.groundBounceOptions.bounceForce == Sizes.VerySmall) {
                            AddForce(new FPVector(0, -activeForces.y * .5, 0), 1);
                        } else if (UFE.config.groundBounceOptions.bounceForce == Sizes.Small) {
                            AddForce(new FPVector(0, -activeForces.y * .6, 0), 1);
                        } else if (UFE.config.groundBounceOptions.bounceForce == Sizes.Medium) {
                            AddForce(new FPVector(0, -activeForces.y * .7, 0), 1);
                        } else if (UFE.config.groundBounceOptions.bounceForce == Sizes.High) {
                            AddForce(new FPVector(0, -activeForces.y * .8, 0), 1);
                        } else if (UFE.config.groundBounceOptions.bounceForce == Sizes.VeryHigh) {
                            AddForce(new FPVector(0, -activeForces.y, 0), 1);
                        }
                    }

					groundBounceTimes ++;

                    if (!isGroundBouncing) {
                        controlScript.stunTime += airTime + UFE.config.knockDownOptions.air._knockedOutTime;

                        if (moveSetScript.basicMoves.groundBounce.animMap[0].clip != null) {
                            controlScript.currentHitAnimation = moveSetScript.basicMoves.groundBounce.name;
                            moveSetScript.PlayBasicMove(moveSetScript.basicMoves.groundBounce);
                        }

						if (UFE.config.groundBounceOptions.bouncePrefab != null) {
							GameObject pTemp = UFE.SpawnGameObject(UFE.config.groundBounceOptions.bouncePrefab, transform.position, Quaternion.identity, Mathf.RoundToInt(UFE.config.groundBounceOptions.bounceKillTime * UFE.config.fps));
                            pTemp.transform.rotation = UFE.config.groundBounceOptions.bouncePrefab.transform.rotation;
                            if (UFE.config.groundBounceOptions.sticky) pTemp.transform.parent = transform;
                        }
						if (UFE.config.groundBounceOptions.shakeCamOnBounce) {
							controlScript.shakeCameraDensity = UFE.config.groundBounceOptions._shakeDensity;
						}
						UFE.PlaySound(UFE.config.groundBounceOptions.bounceSound);
						isGroundBouncing = true;
					}
					return;
				}
				verticalTotalForce = 0;
				airTime = 0;
				moveSetScript.totalAirMoves = 0;
                currentAirJumps = 0;
                horizontalJumpForce = 0;

                BasicMoveInfo airAnimation = null;
                string downAnimation = "";
                
                isGroundBouncing = false;
				groundBounceTimes = 0;

                Fix64 animationSpeed = 0;
                Fix64 delayTime = 0;
				if (controlScript.currentMove != null && controlScript.currentMove.hitAnimationOverride) return;
				if (controlScript.currentSubState == SubStates.Stunned){

                    if (moveSetScript.IsAnimationPlaying(moveSetScript.basicMoves.airRecovery.name)) {
                        controlScript.stunTime = 0;
					    controlScript.currentState = PossibleStates.Stand;

                    } else {
					    controlScript.stunTime = UFE.config.knockDownOptions.air._knockedOutTime + UFE.config.knockDownOptions.air._standUpTime;

                        // Hit Clips
                        if (moveSetScript.IsAnimationPlaying(moveSetScript.basicMoves.getHitKnockBack.name)
                             && moveSetScript.basicMoves.getHitKnockBack.animMap[1].clip != null) {

                            airAnimation = moveSetScript.basicMoves.getHitKnockBack;
                            downAnimation = moveSetScript.GetAnimationString(airAnimation, 2);

                        } else if (moveSetScript.IsAnimationPlaying(moveSetScript.basicMoves.getHitHighKnockdown.name)
                             && moveSetScript.basicMoves.getHitHighKnockdown.animMap[1].clip != null) {

                            airAnimation = moveSetScript.basicMoves.getHitHighKnockdown;
                            downAnimation = moveSetScript.GetAnimationString(airAnimation, 2);
                            controlScript.stunTime = UFE.config.knockDownOptions.high._knockedOutTime + UFE.config.knockDownOptions.high._standUpTime;

                        } else if (moveSetScript.IsAnimationPlaying(moveSetScript.basicMoves.getHitMidKnockdown.name)
                             && moveSetScript.basicMoves.getHitMidKnockdown.animMap[1].clip != null) {

                            airAnimation = moveSetScript.basicMoves.getHitMidKnockdown;
                            downAnimation = moveSetScript.GetAnimationString(airAnimation, 2);
                            controlScript.stunTime = UFE.config.knockDownOptions.highLow._knockedOutTime + UFE.config.knockDownOptions.highLow._standUpTime;

                        } else if (moveSetScript.IsAnimationPlaying(moveSetScript.basicMoves.getHitSweep.name)
                             && moveSetScript.basicMoves.getHitSweep.animMap[1].clip != null) {
                            airAnimation = moveSetScript.basicMoves.getHitSweep;
                            downAnimation = moveSetScript.GetAnimationString(airAnimation, 2);
                            controlScript.stunTime = UFE.config.knockDownOptions.sweep._knockedOutTime + UFE.config.knockDownOptions.sweep._standUpTime;

                        } else if (moveSetScript.IsAnimationPlaying(moveSetScript.basicMoves.getHitCrumple.name)
                             && moveSetScript.basicMoves.getHitCrumple.animMap[1].clip != null) {

                            airAnimation = moveSetScript.basicMoves.getHitCrumple;
                            downAnimation = moveSetScript.GetAnimationString(airAnimation, 2);

                        // Stage Clips
                        } else if (moveSetScript.IsAnimationPlaying(moveSetScript.basicMoves.standingWallBounceKnockdown.name)
                             && moveSetScript.basicMoves.standingWallBounceKnockdown.animMap[1].clip != null) {

                            airAnimation = moveSetScript.basicMoves.standingWallBounceKnockdown;
                            downAnimation = moveSetScript.GetAnimationString(airAnimation, 2);
                            controlScript.stunTime = UFE.config.knockDownOptions.wallbounce._knockedOutTime + UFE.config.knockDownOptions.wallbounce._standUpTime;

                        } else if (moveSetScript.IsAnimationPlaying(moveSetScript.basicMoves.airWallBounce.name)
                             && moveSetScript.basicMoves.airWallBounce.animMap[1].clip != null) {

                            airAnimation = moveSetScript.basicMoves.airWallBounce;
                            downAnimation = moveSetScript.GetAnimationString(airAnimation, 2);
                            controlScript.stunTime = UFE.config.knockDownOptions.wallbounce._knockedOutTime + UFE.config.knockDownOptions.wallbounce._standUpTime;

                        // Fall Clips
                        } else if (moveSetScript.IsAnimationPlaying(moveSetScript.basicMoves.fallingFromAirHit.name)
                            && moveSetScript.basicMoves.fallingFromAirHit.animMap[1].clip != null) {

                            airAnimation = moveSetScript.basicMoves.fallingFromAirHit;
                            downAnimation = moveSetScript.GetAnimationString(airAnimation, 2);

                        } else if (moveSetScript.IsAnimationPlaying(moveSetScript.basicMoves.fallingFromGroundBounce.name)
                            && moveSetScript.basicMoves.fallingFromGroundBounce.animMap[1].clip != null) {

                            airAnimation = moveSetScript.basicMoves.fallingFromGroundBounce;
                            downAnimation = moveSetScript.GetAnimationString(airAnimation, 2);

                        } else {
                            if (moveSetScript.basicMoves.fallDown.animMap[0].clip == null)
                                Debug.LogError("Fall Down From Air Hit animation not found! Make sure you have it set on Character -> Basic Moves -> Fall Down From Air Hit");

                            airAnimation = moveSetScript.basicMoves.fallDown;
                            downAnimation = moveSetScript.GetAnimationString(airAnimation, 1);
                        }
                        
					    controlScript.currentState = PossibleStates.Down;

                    }

				} else if (controlScript.currentState != PossibleStates.Stand) {
                    if (moveSetScript.basicMoves.landing.animMap[0].clip != null
                        && (controlScript.currentMove == null ||
                        (controlScript.currentMove != null && controlScript.currentMove.cancelMoveWheLanding))){

                        controlScript.isAirRecovering = false;
						airAnimation = moveSetScript.basicMoves.landing;
						moveDirection = 0;
                        horizontalJumpForce = 0;
                        isLanding = true;
						controlScript.KillCurrentMove();
                        delayTime = (Fix64)controlScript.myInfo.physics.landingDelay / (Fix64)UFE.config.fps;
                        UFE.DelaySynchronizedAction(ResetLanding, delayTime);

                        if (airAnimation.autoSpeed) {
                            animationSpeed = moveSetScript.GetAnimationLength(airAnimation.name) / delayTime;
                        }
					}

					if (controlScript.currentState != PossibleStates.Crouch) controlScript.currentState = PossibleStates.Stand;

				}

				if (airAnimation != null) {
                    if (downAnimation != "") {
                        moveSetScript.PlayBasicMove(airAnimation, downAnimation);
                    } else {
                        moveSetScript.PlayBasicMove(airAnimation);
                    }

                    if (animationSpeed != 0) {
                        moveSetScript.SetAnimationSpeed(airAnimation.name, animationSpeed);
                    }
				}
			}
			
			if (controlScript.currentSubState != SubStates.Stunned 
                && !controlScript.isBlocking && !controlScript.blockStunned 
                && move == null 
                && !isTakingOff 
                && !isLanding 
                && controlScript.currentState == PossibleStates.Stand)
            {
				if (controlScript.currentSubState == SubStates.MovingForward) {
					if (moveSetScript.basicMoves.moveForward.animMap[0].clip == null)
						Debug.LogError("Move Forward animation not found! Make sure you have it set on Character -> Basic Moves -> Move Forward");
					if (!moveSetScript.IsAnimationPlaying(moveSetScript.basicMoves.moveForward.name)) {
					    moveSetScript.PlayBasicMove(moveSetScript.basicMoves.moveForward);
					}
				}
                else if (controlScript.currentSubState == SubStates.MovingBack) {
					if (moveSetScript.basicMoves.moveBack.animMap[0].clip == null)
						Debug.LogError("Move Back animation not found! Make sure you have it set on Character -> Basic Moves -> Move Back");
					if (!moveSetScript.IsAnimationPlaying(moveSetScript.basicMoves.moveBack.name)) {
						moveSetScript.PlayBasicMove(moveSetScript.basicMoves.moveBack);
					}
                }
                else if (controlScript.currentSubState == SubStates.MovingSideways) {
                    if (moveSetScript.basicMoves.moveSideways.animMap[0].clip == null)
                        Debug.LogError("Move Sidewyas animation not found! Make sure you have it set on Character -> Basic Moves -> Move Sideways");
                    if (!moveSetScript.IsAnimationPlaying(moveSetScript.basicMoves.moveSideways.name))
                    {
                        moveSetScript.PlayBasicMove(moveSetScript.basicMoves.moveSideways);
                    }
                }
            }
        }
        else if (activeForces.y > 0 || !IsGrounded())
        {
			if (move != null && controlScript.currentState == PossibleStates.Stand)
				controlScript.currentState = PossibleStates.NeutralJump;
			if (move == null && activeForces.y / verticalTotalForce > 0 && activeForces.y / verticalTotalForce <= 1) {
				if (isGroundBouncing) return;

				if (moveDirection == 0) {
					controlScript.currentState = PossibleStates.NeutralJump;
				}else{
					if (moveDirection > 0 && controlScript.mirror == -1 ||
					    moveDirection < 0 && controlScript.mirror == 1) {
						controlScript.currentState = PossibleStates.ForwardJump;
					}

					if (moveDirection > 0 && controlScript.mirror == 1||
					    moveDirection < 0 && controlScript.mirror == -1) {
						controlScript.currentState = PossibleStates.BackJump;
					}
				}

                BasicMoveInfo airAnimation = moveSetScript.basicMoves.jumpStraight;
				if (controlScript.currentSubState == SubStates.Stunned) {
                    if (isWallBouncing && moveSetScript.basicMoves.airWallBounce.animMap[0].clip != null) {
                        airAnimation = moveSetScript.basicMoves.airWallBounce;

                    } else if (moveSetScript.basicMoves.getHitKnockBack.animMap[0].clip != null && 
					    FPMath.Abs(activeForces.x) > UFE.config.comboOptions._knockBackMinForce && 
					    UFE.config.comboOptions._knockBackMinForce > 0){
						airAnimation = moveSetScript.basicMoves.getHitKnockBack;
                        airTime *= (Fix64)2;

					} else {
						if (moveSetScript.basicMoves.getHitAir.animMap[0].clip == null)
							Debug.LogError("Get Hit Air animation not found! Make sure you have it set on Character -> Basic Moves -> Get Hit Air");

                        airAnimation = moveSetScript.basicMoves.getHitAir;
                    }
                    if (overrideStunAnimation != null) airAnimation = overrideStunAnimation;

                } else if (controlScript.isAirRecovering 
                    && (moveSetScript.basicMoves.airRecovery.animMap[0].clip != null)) {
						airAnimation = moveSetScript.basicMoves.airRecovery;

				} else {
					if (moveSetScript.basicMoves.jumpForward.animMap[0].clip != null && controlScript.currentState == PossibleStates.ForwardJump) {
						airAnimation = moveSetScript.basicMoves.jumpForward;
					} else if (moveSetScript.basicMoves.jumpBack.animMap[0].clip != null && controlScript.currentState == PossibleStates.BackJump) {
						airAnimation = moveSetScript.basicMoves.jumpBack;
					} else {
						if (moveSetScript.basicMoves.jumpStraight.animMap[0].clip == null)
							Debug.LogError("Jump animation not found! Make sure you have it set on Character -> Basic Moves -> Jump Straight");

						airAnimation = moveSetScript.basicMoves.jumpStraight;
					}
				}

                if (!overrideAirAnimation && !moveSetScript.IsAnimationPlaying(airAnimation.name)) {
                    moveSetScript.PlayBasicMove(airAnimation);

                    if (airAnimation.autoSpeed)
                        moveSetScript.SetAnimationNormalizedSpeed(airAnimation.name, (moveSetScript.GetAnimationLength(airAnimation.name) / airTime));
				}

            } else if (move == null && activeForces.y / verticalTotalForce <= 0) {

                BasicMoveInfo airAnimation = moveSetScript.basicMoves.fallStraight;
                if (isGroundBouncing && moveSetScript.basicMoves.fallingFromGroundBounce.animMap[0].clip != null) {
                    airAnimation = moveSetScript.basicMoves.fallingFromGroundBounce;

                } else if (isWallBouncing && moveSetScript.basicMoves.airWallBounce.animMap[0].clip != null) {
                    airAnimation = moveSetScript.basicMoves.airWallBounce;

				} else {
					if (controlScript.currentSubState == SubStates.Stunned){
						if (moveSetScript.basicMoves.getHitKnockBack.animMap[0].clip != null &&
                            FPMath.Abs(activeForces.x) > UFE.config.comboOptions._knockBackMinForce && 
						    UFE.config.comboOptions._knockBackMinForce > 0){
							airAnimation = moveSetScript.basicMoves.getHitKnockBack;

                        } else {
                            airAnimation = moveSetScript.basicMoves.getHitAir;
                            if (moveSetScript.basicMoves.fallingFromAirHit.animMap[0].clip != null) {
                                airAnimation = moveSetScript.basicMoves.fallingFromAirHit;

                            } else if (moveSetScript.basicMoves.getHitAir.animMap[0].clip == null) {
                                Debug.LogError("Air Juggle animation not found! Make sure you have it set on Character -> Basic Moves -> Air Juggle");
                            }
                        }
                        if (overrideStunAnimation != null) airAnimation = overrideStunAnimation;

                    } else if (controlScript.isAirRecovering 
                        && (moveSetScript.basicMoves.airRecovery.animMap[0].clip != null)) {
                        airAnimation = moveSetScript.basicMoves.airRecovery;

					} else {
						if (moveSetScript.basicMoves.fallForward.animMap[0].clip != null && controlScript.currentState == PossibleStates.ForwardJump) {
							airAnimation = moveSetScript.basicMoves.fallForward;
						} else if (moveSetScript.basicMoves.fallBack.animMap[0].clip != null && controlScript.currentState == PossibleStates.BackJump) {
							airAnimation = moveSetScript.basicMoves.fallBack;
						} else {
							if (moveSetScript.basicMoves.fallStraight.animMap[0].clip == null)
								Debug.LogError("Fall animation not found! Make sure you have it set on Character -> Basic Moves -> Fall Straight");
							
							airAnimation = moveSetScript.basicMoves.fallStraight;
						}
					}
				}

				if (!overrideAirAnimation && !moveSetScript.IsAnimationPlaying(airAnimation.name)){
                    moveSetScript.PlayBasicMove(airAnimation);

                    if (airAnimation.autoSpeed) {
                        moveSetScript.SetAnimationNormalizedSpeed(airAnimation.name, (moveSetScript.GetAnimationLength(airAnimation.name) / airTime));
                    }
				}
			}
		}

        if (activeForces.x == 0 && activeForces.y == 0)
            moveDirection = 0;
    }

	public bool IsGrounded() {
        if (worldTransform.position.y <= UFE.config.selectedStage.position.y) return true;
		return false;
	}
}
