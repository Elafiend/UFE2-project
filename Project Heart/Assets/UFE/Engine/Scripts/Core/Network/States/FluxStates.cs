using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using FPLibrary;
using UFE3D;
using UFENetcode;

public struct FluxStates : UFEInterface {
	#region public instance properties
	public long networkFrame {get; set;}

    public Dictionary<System.Reflection.MemberInfo, System.Object> tracker;

    public GlobalState global;
    public GUIState battleGUI;
    public CameraState camera;
    public List<CharacterState> allCharacterStates;
    #endregion

    #region struct definitions
    public struct GlobalState : UFEInterface {
        // UFE
        public bool freeCamera;
        public bool freezePhysics;
        public bool newRoundCasted;
        public bool normalizedCam;
        public bool pauseTimer;
        public Fix64 timer;
        public List<DelayedAction> delayedActions;
        public List<InstantiatedGameObjectState> instantiatedObjects;

        // GlobalInfo
        public int currentRound;
        public bool lockInputs;
        public bool lockMovements;
        public Fix64 timeScale;
    }
    
    public struct InstantiatedGameObjectState : UFEInterface {
        public string id;
        public GameObject gameObject;
        public MrFusion mrFusion;
        public long creationFrame;
        public long? destructionFrame;
        public TransformState transformState;
    }

    public struct TransformState : UFEInterface {
        public FPVector fpPosition;
        public FPQuaternion fpRotation;
        public Vector3 position;
        public Vector3 localPosition;
        public Quaternion rotation;
        public Quaternion localRotation;
        public Vector3 localScale;
        public bool active;
    }

    public struct GUIState : UFEInterface {
        public List<BattleGUI.InputIconGroup> player1InputIcons;
        public List<BattleGUI.InputIconGroup> player2InputIcons;
    }

    public struct CameraState : UFEInterface {
        // Transform
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 position;
        public Quaternion rotation;

		public bool cameraScript;
        public bool cinematicFreeze;
        public Vector3 currentLookAtPosition;
        public Vector3 defaultCameraPosition;
        public float defaultDistance;
        public bool enabled;
        public float fieldOfView;
        public float freeCameraSpeed;
        public GameObject lastOwner;
        public bool killCamMove;
        public float movementSpeed;
        public float rotationSpeed;
        public float standardGroundHeight;
        public Vector3 targetPosition;
        public Quaternion targetRotation;
        public float targetFieldOfView;

		// Camera Fade
        public bool cameraFade;
        public Color currentScreenOverlayColor;
    }

    public struct CharacterState : UFEInterface {
		// ControlsScript
		public bool controlsScript;

        // Transforms
        public TransformState shellTransform;
        public TransformState characterTransform;

        // Global Properties
        public int playerNum;
        public CombatStances combatStance;
        public Fix64 life;

        // Control
        public bool active;
        public Fix64 afkTimer;
        public int airJuggleHits;
        public AirRecoveryType airRecoveryType;
        public bool applyRootMotion;
        public bool blockStunned;
        public Fix64 comboDamage;
        public Fix64 comboHitDamage;
        public int comboHits;
        public int consecutiveCrumple;
        public BasicMoveReference currentBasicMove;
        public Fix64 currentDrained;
        public Hit currentHit;
        public string currentHitAnimation;
        public PossibleStates currentState;
        public SubStates currentSubState;
        public CombatStances DCStance;
        public bool firstHit;
        public Fix64 gaugeDPS;
        public GaugeId gaugeDrainId;
        public bool hitDetected;
        public Fix64 hitAnimationSpeed;
        public Fix64 horizontalForce;
        public bool inhibitGainWhileDraining;
        public bool isAirRecovering;
        public bool isBlocking;
        public bool isCrouching;
        public bool isDead;
        public bool ignoreCollisionMass;
        public bool introPlayed;
        public bool lit;
        public ControlsScript lockOnTarget;
        public bool lockXMotion;
        public bool lockYMotion;
        public bool lockZMotion;
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

        // Sub Classes
        public PullInState activePullIn;
        public MoveState currentMove;
        public MoveState DCMove;
        public MoveState enterMove;
        public MoveState exitMove;
        public MoveState storedMove;

        // Core Scripts
        public PhysicsState physics;
        public MoveSetState moveSet;
        public HitBoxesState hitBoxes;

        // Arrays
        public Fix64[] gauges;
        public Dictionary<ButtonPress, Fix64> inputHeldDown;
		public List<ProjectileMoveScript> projectiles;

        // Nested Structs
        public struct PullInState : UFEInterface {
            public PullIn pullIn;
            public FPVector position;
        }

        public struct MoveState : UFEInterface {
            public MoveInfo move;
            //public bool cancelable;
            public bool kill;
            public int armorHits;
            public int currentFrame;
            public int overrideStartupFrame;
            public Fix64 animationSpeedTemp;
            public Fix64 currentTick;
            public bool hitConfirmOnBlock;
            public bool hitConfirmOnParry;
            public bool hitConfirmOnStrike;
            public bool hitAnimationOverride;
            public StandUpOptions standUpOptions;
            public CurrentFrameData currentFrameData;
            public long lastFramePlayed;
            public HitState[] hitStates;
            public bool[] frameLinkStates;
            public bool[] castedGauge;
            public bool[] castedBodyPartVisibilityChange;
            public bool[] castedProjectile;
            public bool[] castedAppliedForce;
            public bool[] castedCharacterAssist;
            public bool[] castedMoveParticleEffect;
            public bool[] castedSlowMoEffect;
            public bool[] castedSoundEffect;
            public bool[] castedInGameAlert;
            public bool[] castedStanceChange;
            public bool[] castedCameraMovement;
            public bool[] castedLockOnTarget;
            public bool[] cameraOver;
            public bool[] castedOpponentOverride;
            public Fix64[] cameraTime;
        }

        public struct HitState : UFEInterface {
            public ControlsScript[] impactList;
        }

        public struct PhysicsState : UFEInterface {
            public FPVector activeForces;
            public Fix64 angularDirection;
            public Fix64 airTime;
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
        }

        public struct MoveSetState : UFEInterface {
            public int totalAirMoves;
            public bool animationPaused;
            public Fix64 overrideNextBlendingValue;
            public Fix64 lastTimePress;
            public AnimatiorState animator;

            public Dictionary<ButtonPress, Fix64> chargeValues;
            public List<ButtonSequenceRecord> lastButtonPresses;
            public Dictionary<string, long> lastMovesPlayed;
        }

        public struct HitBoxesState : UFEInterface {
            public bool isHit;
            public HitConfirmType hitConfirmType;
            public Fix64 collisionBoxSize;
            public bool inverted;
            public bool bakeSpeed;
            public FPVector deltaPosition;
            public AnimationMap[] animationMaps;
            public CustomHitBoxesState customHitBoxes;

            public HitBoxState[] hitBoxes;
            public HurtBoxState[] activeHurtBoxes;
            public BlockAreaState blockableArea;
        }

        public struct CustomHitBoxesState : UFEInterface {
            public AnimationClip clip;
            public Fix64 speed;
            public GameObject preview;
            public int totalFrames;

            public CustomHitBox[] customHitBoxes;
        }

        public struct HitBoxState : UFEInterface {
            public BodyPart bodyPart;
            public int state;
            public bool hide;
            public bool visibility;
            public CollisionType collisionType;
            public HitBoxType type;
            public HitBoxShape shape;
            public FPVector mappedPosition;
            public FPVector localPosition;
            public Fix64 radius;
            public FPRect rect;
            public FPVector offSet;
        }

        public struct HurtBoxState : UFEInterface {
            public BodyPart bodyPart;
            public bool followXBounds;
            public bool followYBounds;
            public int hitBoxDefinitionIndex;
            public bool isBlock;
            public FPVector offSet;
            public FPVector position;
            public Fix64 radius;
            public FPRect rect;
            public Rect rendererBounds;
            public HitBoxShape shape;
            public HurtBoxType type;
        }

        public struct BlockAreaState : UFEInterface {
            public BlockArea blockArea;
            public FPVector position;
        }

        public struct AnimatiorState : UFEInterface {
            public AnimationDataState currentAnimationData;
            public bool currentMirror;

            // Mecanim Control
            public string currentState;
            public Fix64 currentSpeed;
            public RuntimeAnimatorController overrideController;

            // MC3
            public int currentInput;
            public int transitionDuration;
            public int transitionTime;
            public Fix64[] weightList;
            public Fix64[] speedList;
            public Fix64[] timeList;

            // Legacy
            public Fix64 globalSpeed;
            public Vector3 lastPosition;
        }

        public struct AnimationDataState : UFEInterface {
            public LegacyAnimationData legacyAnimationData;
            public MecanimAnimationData mecanimAnimationData;
            //public MC3AnimationData mecanimAnimationData;

            // Legacy
            public AnimationState animState;

            // Mecanim

            // Both
            public Fix64 normalizedTime;
            public Fix64 ticksPlayed;
            public Fix64 framesPlayed;
            public Fix64 realFramesPlayed;
            public Fix64 secondsPlayed;
            public Fix64 speed;
            public int timesPlayed;
        }
        #endregion
    }
}
