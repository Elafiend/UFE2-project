using UnityEngine;
using FPLibrary;

namespace UFE3D
{
    [System.Serializable]
    public class MoveInfo : ScriptableObject
    {
        public GameplayType gameplayType;
        public SerializedAnimationMap animMap = new SerializedAnimationMap();
        public Fix64 _animationSpeed = 1;
        public WrapMode wrapMode;

        public GameObject characterPrefab;
        public string moveName;
        public string description;
        public int fps = 60;
        public bool ignoreGravity;
        public bool ignoreFriction;
        public bool cancelMoveWheLanding;
        public bool forceMirrorLeft;
        public bool forceMirrorRight;
        public bool invertRotationLeft;
        public bool invertRotationRight;
        public bool autoCorrectRotation;
        public bool allowSideSwitch;
        public int frameWindowRotation;
        public bool cooldown;
        public int cooldownFrames;

        public bool disableHeadLook = true;

        public bool speedKeyFrameToggle = false;
        public bool fixedSpeed = true;
        public AnimSpeedKeyFrame[] animSpeedKeyFrame = new AnimSpeedKeyFrame[0];
        public int totalFrames = 15;

        public int startUpFrames = 0;
        public int activeFrames = 1;
        public int recoveryFrames = 2;
        public bool applyRootMotion = false;
        public bool lockXMotion = false;
        public bool lockYMotion = false;
        public bool lockZMotion = false;
        public bool forceGrounded = false;
        public BodyPart rootMotionNode = BodyPart.none;
        public bool overrideBlendingIn = true;
        public bool overrideBlendingOut = false;
        public Fix64 _blendingIn = 0;
        public Fix64 _blendingOut = 0;

        public MoveInputs defaultInputs = new MoveInputs();
        public MoveInputs altInputs = new MoveInputs();

        public MoveInfo[] previousMoves = new MoveInfo[0];
        public PlayerConditions opponentConditions = new PlayerConditions();
        public PlayerConditions selfConditions = new PlayerConditions();
        public MoveClassification moveClassification;

        public ButtonPress[][] simulatedInputs;

        #region trackable definitions
        public FrameLink[] frameLinks = new FrameLink[0];
        public GaugeInfo[] gauges = new GaugeInfo[0];
        public MoveSortOrder[] sortOrder = new MoveSortOrder[0];
        public MoveParticleEffect[] particleEffects = new MoveParticleEffect[0];
        public AppliedForce[] appliedForces = new AppliedForce[0];
        public SlowMoEffect[] slowMoEffects = new SlowMoEffect[0];
        public BodyPartVisibilityChange[] bodyPartVisibilityChanges = new BodyPartVisibilityChange[0];
        public OpponentOverride[] opponentOverride = new OpponentOverride[0];
        public CharacterAssist[] characterAssist = new CharacterAssist[0];
        public SoundEffect[] soundEffects = new SoundEffect[0];
        public InGameAlert[] inGameAlert = new InGameAlert[0];
        public StanceChange[] stanceChanges = new StanceChange[0];
        public CameraMovement[] cameraMovements = new CameraMovement[0];
        public Hit[] hits = new Hit[0];
        public BlockArea blockableArea;
        public MoveLockOnOptions[] lockOnTargets = new MoveLockOnOptions[0];
        public SwitchCharacterOptions[] switchCharacterOptions = new SwitchCharacterOptions[0];
        public InvincibleBodyParts[] invincibleBodyParts = new InvincibleBodyParts[0];
        public StateOverride[] stateOverride = new StateOverride[0];
        public ArmorOptions armorOptions;
        public Projectile[] projectiles = new Projectile[0];

        public bool cancelable { get; set; }
        public bool kill { get; set; }
        public int currentFrame { get; set; }
        public int overrideStartupFrame { get; set; }
        public Fix64 currentTick { get; set; }
        public bool hitConfirmOnBlock { get; set; }
        public bool hitConfirmOnParry { get; set; }
        public bool hitConfirmOnStrike { get; set; }
        public bool hitAnimationOverride { get; set; }
        public StandUpOptions standUpOptions { get; set; }
        public CurrentFrameData currentFrameData { get; set; }
        #endregion


        [HideInInspector] public Vector3 rotationPreview = new Vector3(0, 90, 0);

        public bool IsThrow(bool techable)
        {
            foreach (Hit hit in this.hits)
            {
                if (hit.hitConfirmType == HitConfirmType.Throw && hit.techable == techable) return true;
            }
            return false;
        }

        public MoveInfo GetTechMove()
        {
            foreach (Hit hit in this.hits)
            {
                if (hit.hitConfirmType == HitConfirmType.Throw && hit.techable) return hit.techMove;
            }
            return null;
        }
    }
}