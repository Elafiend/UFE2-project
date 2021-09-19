using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using AForge.Fuzzy;
using AI4Unity.Fuzzy;
using UFE3D;


[Serializable]
public class AIDefinitions{
	public DamageDefinitions damage;
	public DistanceDefinitions distance;
	public DesirabilityDefinitions desirability;
	public HealthDefinitions health;
	public SpeedDefinitions speed;
}

[Serializable]
public class DamageDefinitions{
	public float veryWeak = 0.05f;
	public float weak = 0.10f;
	public float medium = 0.15f;
	public float strong = 0.20f;
	public float veryStrong = 0.25f;
}

[Serializable]
public class DistanceDefinitions{
	public float veryClose = 0.05f;
	public float close = 0.25f;
	public float mid = 0.5f;
	public float far = 0.75f;
	public float veryFar = 0.95f;
}

[Serializable]
public class DesirabilityDefinitions{
	public float theWorstOption = 0.00f;
	public float veryUndesirable = 0.15f;
	public float undesirable = 0.30f;
	public float notBad = 0.45f;
	public float desirable = 0.60f;
	public float veryDesirable = 0.80f;
	public float theBestOption = 1.00f;
}

[Serializable]
public class HealthDefinitions{
	public float healthy = 1.0f;
	public float scratched = 0.9f;
	public float lightlyWounded = 0.8f;
	public float moderatelyWounded = 0.6f;
	public float seriouslyWounded = 0.4f;
	public float criticallyWounded = 0.2f;
	public float almostDead = 0.1f;
	public float dead = 0.0f;
}

[Serializable]
public class AIAdvancedOptions{
	public float timeBetweenDecisions = 0;
	public float timeBetweenActions = 0.05f;
	public float aggressiveness = 0.5f;
	public float ruleCompliance = .9f; // 0 = Weighted Random Selection / 1 = Use the Best Available Move
	public float comboEfficiency = 1f;
    public float movementDuration = .1f;
    public int buttonSequenceInterval = 1;
	public AIAttackDesirabilityCalculation attackDesirabilityCalculation = AIAttackDesirabilityCalculation.Max;
	public AIDesirability defaultDesirability = AIDesirability.TheWorstOption;
	public bool playRandomMoves;
	public AIReactionParameters reactionParameters = new AIReactionParameters();
}

[Serializable]
public class AIReactionParameters{
	public bool attackWhenEnemyIsDown = false;
	public bool attackWhenEnemyIsBlocking = true;
	public bool stopBlockingWhenEnemyIsStunned = true;
	
	public bool inputWhenDown = false;
	public bool inputWhenBlocking = true;
	public bool inputWhenStunned = true;

	public bool enableAttackTypeFilter = true;
	public bool enableGaugeFilter = true;
	public bool enableDistanceFilter = true;
	public bool enableDamageFilter = true;
	public bool enableHitConfirmTypeFilter = true;
	public bool enableAttackSpeedFilter = false;
	public bool enableHitTypeFilter = true;
}

[Serializable]
public class SpeedDefinitions{
	public float verySlow = 0.5f;
	public float slow = 1.0f;
	public float normal = 3.0f;
	public float fast = 5.0f;
	public float veryFast = 7.0f;
}


public enum HealthStatus {
	Healthy,
	Scratched,
	LightlyWounded,
	ModeratelyWounded,
	SeriouslyWounded,
	CriticallyWounded,
	AlmostDead,
	Dead
}

public enum TargetCharacter {
	Self,
	Opponent
}

public enum AIAttackDesirabilityCalculation{
	Average,
	ClampedSum,
	Max,
	Min
}

public enum AIConditionType {
	Idle,
	HorizontalMovement,
	VerticalMovement,
	HealthStatus,
	GaugeStatus,
	Distance,
	Attacking,
	Blocking,
	Stunned,
	Down,
	//ProjectileDistance, // In front of or behind the character?
	//ProjectileSpeed,
}

public enum AIBlocking {
	Air,
	High,
	Low
}

public enum AIDesirability{
	TheWorstOption,
	VeryUndesirable,
	Undesirable,
	NotBad,
	Desirable,
	VeryDesirable,
	TheBestOption
}

public enum AIReactionType {
	Idle,
	MoveForward,
	MoveBack,
	Crouch,
	JumpStraight,
	JumpForward,
	JumpBack,
	CrouchBlock,
	StandBlock,
	JumpBlock,
	PlayMove,
	ChangeBehavior
}

public enum AIBoolean{
	TRUE,
	FALSE
}

public enum AIDamage{
	Any,
	VeryWeak,
	Weak,
	Medium,
	Strong,
	VeryStrong
}

public enum AIHorizontalMovement {
	MovingForward,
	Still,
	MovingBack
}

public enum AIMovementSpeed {
	Any,
	VerySlow,
	Slow,
	Normal,
	Fast,
	VeryFast
}

public enum AIVerticalMovement {
	//Down,
	Crouching,
	Standing,
	Jumping
}

[Serializable]
public class AICondition:ICloneable {
	//-----------------------------------------------------------------------------------------------------------------
	// Public class properties
	//-----------------------------------------------------------------------------------------------------------------
	// We use a numeric prefix for each condition to make the string comparisons faster
	public static readonly string Attacking_Self = "000_" + AIConditionType.Attacking + "_" +TargetCharacter.Self;
	public static readonly string Attacking_Opponent = "001_" + AIConditionType.Attacking + TargetCharacter.Opponent;

	public static readonly string Attacking_AttackType_Self = "002_" + AIConditionType.Attacking + "_" + typeof(AttackType) + "_" + TargetCharacter.Self;
	public static readonly string Attacking_AttackType_Opponent = "003_" + AIConditionType.Attacking + "_" + typeof(AttackType) + "_" + TargetCharacter.Opponent;

	public static readonly string Attacking_Damage_Self = "004_" + AIConditionType.Attacking + "_" + typeof(AIDamage) + "_" + TargetCharacter.Self;
	public static readonly string Attacking_Damage_Opponent = "005_" + AIConditionType.Attacking + "_" + typeof(AIDamage) + "_" + TargetCharacter.Opponent;

	public static readonly string Attacking_GaugeUsage_Self = "006_" + AIConditionType.Attacking + "_" + typeof(GaugeUsage) + "_" + TargetCharacter.Self;
	public static readonly string Attacking_GaugeUsage_Opponent = "007_" + AIConditionType.Attacking + "_" + typeof(GaugeUsage) + "_" + TargetCharacter.Opponent;

	public static readonly string Attacking_HitType_Self = "008_" + AIConditionType.Attacking + "_" + typeof(HitType) + "_" + TargetCharacter.Self;
	public static readonly string Attacking_HitType_Opponent = "009_" + AIConditionType.Attacking + "_" + typeof(HitType) + "_" + TargetCharacter.Opponent;

	public static readonly string Attacking_StartupSpeed_Self = "010_" + AIConditionType.Attacking + "_StartupSpeed_" + TargetCharacter.Self;
	public static readonly string Attacking_StartupSpeed_Opponent = "011_" + AIConditionType.Attacking + "_StartupSpeed_" + TargetCharacter.Opponent;
	
	public static readonly string Attacking_RecoverySpeed_Self = "012_" + AIConditionType.Attacking + "_RecoverySpeed_" + TargetCharacter.Self;
	public static readonly string Attacking_RecoverySpeed_Opponent = "013_" + AIConditionType.Attacking + "_RecoverySpeed_" + TargetCharacter.Opponent;

	public static readonly string Attacking_HitConfirmType_Self = "014_" + AIConditionType.Attacking + "_" + typeof(HitConfirmType) + "_" + TargetCharacter.Self;
	public static readonly string Attacking_HitConfirmType_Opponent = "015_" + AIConditionType.Attacking + "_" + typeof(HitConfirmType) + "_" + TargetCharacter.Opponent;

	public static readonly string Attacking_FrameData_Self = "016_" + AIConditionType.Attacking + "_" + typeof(CurrentFrameData) + "_" + TargetCharacter.Self;
	public static readonly string Attacking_FrameData_Opponent = "017_" + AIConditionType.Attacking + "_" + typeof(CurrentFrameData) + "_" + TargetCharacter.Opponent;

	public static readonly string Attacking_PreferableDistance_Self = "018_" + AIConditionType.Attacking + "_" + typeof(CharacterDistance) + "_" + TargetCharacter.Self;
	public static readonly string Attacking_PreferableDistance_Opponent = "019_" + AIConditionType.Attacking + "_" + typeof(CharacterDistance) + "_" + TargetCharacter.Opponent;

	public static readonly string Blocking_Self = "020_" + AIConditionType.Blocking + "_" + TargetCharacter.Self;
	public static readonly string Blocking_Opponent = "021_" + AIConditionType.Blocking + "_" + TargetCharacter.Opponent;

	public static readonly string Distance_Self = "022_" + AIConditionType.Distance + "_" + TargetCharacter.Self;
	public static readonly string Distance_Opponent = "023_" + AIConditionType.Distance + "_" + TargetCharacter.Opponent;

	public static readonly string Down_Self = "024_" + AIConditionType.Down + "_" + TargetCharacter.Self;
	public static readonly string Down_Opponent = "025_" + AIConditionType.Down + "_" + TargetCharacter.Opponent;

	public static readonly string Gauge_Self = "026_" + AIConditionType.GaugeStatus + "_" + TargetCharacter.Self;
	public static readonly string Gauge_Opponent = "027_" + AIConditionType.GaugeStatus + "_" + TargetCharacter.Opponent;

	public static readonly string Health_Self = "028_" + AIConditionType.HealthStatus + "_" + TargetCharacter.Self;
	public static readonly string Health_Opponent = "029_" + AIConditionType.HealthStatus + "_" + TargetCharacter.Opponent;

	public static readonly string HorizontalMovement_Self = "030_" + AIConditionType.HorizontalMovement + "_" + TargetCharacter.Self;
	public static readonly string HorizontalMovement_Opponent = "031_" + AIConditionType.HorizontalMovement + "_" + TargetCharacter.Opponent;

	public static readonly string HorizontalMovementSpeed_Self = "032_" + AIConditionType.HorizontalMovement + typeof(AIMovementSpeed) + "_" + TargetCharacter.Self;
	public static readonly string HorizontalMovementSpeed_Opponent = "033_" + AIConditionType.HorizontalMovement + typeof(AIMovementSpeed) + "_" + TargetCharacter.Opponent;

	public static readonly string JumpArc_Self = "034_" + AIConditionType.VerticalMovement + typeof(JumpArc) + "_" + TargetCharacter.Self;
	public static readonly string JumpArc_Opponent = "035_" + AIConditionType.VerticalMovement + typeof(JumpArc) + "_" + TargetCharacter.Opponent;

	public static readonly string Stunned_Self = "036_" + AIConditionType.Stunned + "_" + TargetCharacter.Self;
	public static readonly string Stunned_Opponent = "037_" + AIConditionType.Stunned + "_" + TargetCharacter.Opponent;

	public static readonly string VerticalMovement_Self = "038_" + AIConditionType.VerticalMovement + "_" + TargetCharacter.Self;
	public static readonly string VerticalMovement_Opponent = "039_" + AIConditionType.VerticalMovement + "_" + TargetCharacter.Opponent;

	// Public instance properties
	public bool enabled = true;
	public AIBoolean boolean = AIBoolean.TRUE;

	public TargetCharacter targetCharacter = TargetCharacter.Self;
	public AIConditionType conditionType = AIConditionType.Idle;
	public AIHorizontalMovement horizontalMovement = AIHorizontalMovement.Still;
	public AIVerticalMovement verticalMovement = AIVerticalMovement.Standing;
	public AIMovementSpeed movementSpeed = AIMovementSpeed.Any;
	public HealthStatus healthStatus = HealthStatus.Healthy;
	public GaugeUsage gaugeStatus = GaugeUsage.Any;
	public CharacterDistance playerDistance = CharacterDistance.Mid;
	public JumpArc jumping = global::JumpArc.Any;
	public AIBlocking blocking = AIBlocking.High;
	public MoveClassification moveClassification;
	public CurrentFrameData moveFrameData = CurrentFrameData.Any;
	public AIDamage moveDamage = AIDamage.Any;

	public object Clone() {
		return CloneObject.Clone(this, true);
	}
}

[Serializable]
public class AIEvent: System.ICloneable {
	public bool enabled = true;
	public AIBoolean boolean = AIBoolean.TRUE;
	public AICondition[] conditions = new AICondition[0];
	
	[HideInInspector] public bool conditionsToggle;

	public object Clone() {
		return CloneObject.Clone(this, true);
	}
}

[Serializable]
public class AIReaction: System.ICloneable {
	//-----------------------------------------------------------------------------------------------------------------
	// Public class properties
	//-----------------------------------------------------------------------------------------------------------------
	// We use a numeric prefix for each condition to make the string comparisons faster
	public static readonly string Crouch = "000_" + AIReactionType.Crouch;
	public static readonly string CrouchBlock = "001_" + AIReactionType.CrouchBlock;
	public static readonly string Idle = "002_" + AIReactionType.Idle;
	public static readonly string JumpBackward = "003_" + AIReactionType.JumpBack;
	public static readonly string JumpBlock = "004_" + AIReactionType.JumpBlock;
	public static readonly string JumpForward = "005_" + AIReactionType.JumpForward;
	public static readonly string JumpStraight = "006_" + AIReactionType.JumpStraight;
	public static readonly string MoveForward = "007_" + AIReactionType.MoveForward;
	public static readonly string MoveBackward = "008_" + AIReactionType.MoveBack;
	public static readonly string StandBlock = "009_" + AIReactionType.StandBlock;

	public static readonly string PlayMove_AttackType_AntiAir = "010_" + AIReactionType.PlayMove + "_" + typeof(AttackType) + "_" + AttackType.AntiAir;
	public static readonly string PlayMove_AttackType_BackLauncher = "011_" + AIReactionType.PlayMove + "_" + typeof(AttackType) + "_" + AttackType.BackLauncher;
	public static readonly string PlayMove_AttackType_Dive = "012_" + AIReactionType.PlayMove + "_" + typeof(AttackType) + "_" + AttackType.Dive;
	public static readonly string PlayMove_AttackType_ForwardLauncher = "013_" + AIReactionType.PlayMove + "_" + typeof(AttackType) + "_" + AttackType.ForwardLauncher;
	public static readonly string PlayMove_AttackType_Neutral = "014_" + AIReactionType.PlayMove + "_" + typeof(AttackType) + "_" + AttackType.Neutral;
	public static readonly string PlayMove_AttackType_NormalAttack = "015_" + AIReactionType.PlayMove + "_" + typeof(AttackType) + "_" + AttackType.NormalAttack;
	public static readonly string PlayMove_AttackType_Projectile = "016_" + AIReactionType.PlayMove + "_" + typeof(AttackType) + "_" + AttackType.Projectile;

	public static readonly string PlayMove_Damage_VeryWeak = "020_" + AIReactionType.PlayMove + "_" + typeof(AIDamage) + "_" + AIDamage.VeryWeak;
	public static readonly string PlayMove_Damage_Weak = "021_" + AIReactionType.PlayMove + "_" + typeof(AIDamage) + "_" + AIDamage.Weak;
	public static readonly string PlayMove_Damage_Medium = "022_" + AIReactionType.PlayMove + "_" + typeof(AIDamage) + "_" + AIDamage.Medium;
	public static readonly string PlayMove_Damage_Strong = "023_" + AIReactionType.PlayMove + "_" + typeof(AIDamage) + "_" + AIDamage.Strong;
	public static readonly string PlayMove_Damage_VeryStrong = "024_" + AIReactionType.PlayMove + "_" + typeof(AIDamage) + "_" + AIDamage.VeryStrong;

	public static readonly string PlayMove_HitType_HighKnockdown = "030_" + AIReactionType.PlayMove + "_" + typeof(HitType) + "_" + HitType.HighKnockdown;
	public static readonly string PlayMove_HitType_HighLow = "031_" + AIReactionType.PlayMove + "_" + typeof(HitType) + "_" + HitType.Mid;
	public static readonly string PlayMove_HitType_KnockBack = "032_" + AIReactionType.PlayMove + "_" + typeof(HitType) + "_" + HitType.KnockBack;
	public static readonly string PlayMove_HitType_Launcher = "033_" + AIReactionType.PlayMove + "_" + typeof(HitType) + "_" + HitType.Launcher;
	public static readonly string PlayMove_HitType_Low = "034_" + AIReactionType.PlayMove + "_" + typeof(HitType) + "_" + HitType.Low;
	public static readonly string PlayMove_HitType_MidKnockdown = "035_" + AIReactionType.PlayMove + "_" + typeof(HitType) + "_" + HitType.MidKnockdown;
	public static readonly string PlayMove_HitType_Overhead = "036_" + AIReactionType.PlayMove + "_" + typeof(HitType) + "_" + HitType.Overhead;
	public static readonly string PlayMove_HitType_Sweep = "037_" + AIReactionType.PlayMove + "_" + typeof(HitType) + "_" + HitType.Sweep;

	public static readonly string PlayMove_StartupSpeed_VeryFast = "040_" + AIReactionType.PlayMove + "_StartupSpeed_" + FrameSpeed.VeryFast;
	public static readonly string PlayMove_StartupSpeed_Fast = "041_" + AIReactionType.PlayMove + "_StartupSpeed_" + FrameSpeed.Fast;
	public static readonly string PlayMove_StartupSpeed_Normal = "042_" + AIReactionType.PlayMove + "_StartupSpeed_" + FrameSpeed.Normal;
	public static readonly string PlayMove_StartupSpeed_Slow = "043_" + AIReactionType.PlayMove + "_StartupSpeed_" + FrameSpeed.Slow;
	public static readonly string PlayMove_StartupSpeed_VerySlow = "044_" + AIReactionType.PlayMove + "_StartupSpeed_" + FrameSpeed.VerySlow;

	public static readonly string PlayMove_RecoverySpeed_VeryFast = "050_" + AIReactionType.PlayMove + "_RecoverySpeed_" + FrameSpeed.VeryFast;
	public static readonly string PlayMove_RecoverySpeed_Fast = "051_" + AIReactionType.PlayMove + "_RecoverySpeed_" + FrameSpeed.Fast;
	public static readonly string PlayMove_RecoverySpeed_Normal = "052_" + AIReactionType.PlayMove + "_RecoverySpeed_" + FrameSpeed.Normal;
	public static readonly string PlayMove_RecoverySpeed_Slow = "053_" + AIReactionType.PlayMove + "_RecoverySpeed_" + FrameSpeed.Slow;
	public static readonly string PlayMove_RecoverySpeed_VerySlow = "054_" + AIReactionType.PlayMove + "_RecoverySpeed_" + FrameSpeed.VerySlow;

	public static readonly string PlayMove_HitConfirmType_Hit = "060_" + AIReactionType.PlayMove + "_" + typeof(HitConfirmType) + "_" + HitConfirmType.Hit;
	public static readonly string PlayMove_HitConfirmType_Throw = "061_" + AIReactionType.PlayMove + "_" + typeof(HitConfirmType) + "_" + HitConfirmType.Throw;

	public static readonly string PlayMove_GaugeUsage_All = "070_" + AIReactionType.PlayMove + "_" + typeof(GaugeUsage) + "_" + GaugeUsage.All;
	public static readonly string PlayMove_GaugeUsage_Half = "071_" + AIReactionType.PlayMove + "_" + typeof(GaugeUsage) + "_" + GaugeUsage.Half;
	public static readonly string PlayMove_GaugeUsage_None = "072_" + AIReactionType.PlayMove + "_" + typeof(GaugeUsage) + "_" + GaugeUsage.None;
	public static readonly string PlayMove_GaugeUsage_Quarter = "073_" + AIReactionType.PlayMove + "_" + typeof(GaugeUsage) + "_" + GaugeUsage.Quarter;
	public static readonly string PlayMove_GaugeUsage_ThreeQuarters = "074_" + AIReactionType.PlayMove + "_" + typeof(GaugeUsage) + "_" + GaugeUsage.ThreeQuarters;

	public static readonly string PlayMove_PreferableDistance_VeryClose = "080_" + AIReactionType.PlayMove + "_" + typeof(CharacterDistance) + "_" + CharacterDistance.VeryClose;
	public static readonly string PlayMove_PreferableDistance_Close = "081_" + AIReactionType.PlayMove + "_" + typeof(CharacterDistance) + "_" + CharacterDistance.Close;
	public static readonly string PlayMove_PreferableDistance_Mid = "082_" + AIReactionType.PlayMove + "_" + typeof(CharacterDistance) + "_" + CharacterDistance.Mid;
	public static readonly string PlayMove_PreferableDistance_Far = "083_" + AIReactionType.PlayMove + "_" + typeof(CharacterDistance) + "_" + CharacterDistance.Far;
	public static readonly string PlayMove_PreferableDistance_VeryFar = "084_" + AIReactionType.PlayMove + "_" + typeof(CharacterDistance) + "_" + CharacterDistance.VeryFar;
	public static readonly string PlayMove_RandomAttack = "090_" + AIReactionType.PlayMove + "_Random";

	public static readonly string ChangeBehaviour_Aggressive = "A00_" + AIReactionType.ChangeBehavior + "_" + AIBehavior.Aggressive;
	public static readonly string ChangeBehaviour_Any = "A01_" + AIReactionType.ChangeBehavior + "_" + AIBehavior.Any;
	public static readonly string ChangeBehaviour_Balanced = "A02_" + AIReactionType.ChangeBehavior + "_" + AIBehavior.Balanced;
	public static readonly string ChangeBehaviour_Defensive = "A03_" + AIReactionType.ChangeBehavior + "_" + AIBehavior.Defensive;
	public static readonly string ChangeBehaviour_VeryAggressive = "A04_" + AIReactionType.ChangeBehavior + "_" + AIBehavior.VeryAggressive;
	public static readonly string ChangeBehaviour_VeryDefensive = "A105_" + AIReactionType.ChangeBehavior + "_" + AIBehavior.VeryDefensive;

	// Public instance properties
	public AIReactionType reactionType;
	public MoveClassification moveClassification;			// When Attack is chosen
	public AIDamage moveDamage = AIDamage.Any;				// When Attack is chosen
	public MoveInfo specificMove;							// When Play Specific Move is chosen
	public ButtonPress buttonPress = ButtonPress.Button1;	// Press Button
	public AIBehavior behavior;								// Change Behavior
	public AIDesirability desirability = AIDesirability.NotBad;	// Desirability score

	public object Clone() {
		return CloneObject.Clone(this, true);
	}
}

[Serializable]
public class AIRule: System.ICloneable {
	public static readonly string Rule_AND = " AND ";
	public static readonly string Rule_Close_Parenthesis = ") ";
	public static readonly string Rule_IF = "IF ";
	public static readonly string Rule_IS = " IS ";
	public static readonly string Rule_NOT = " NOT ";
	public static readonly string Rule_Open_Parenthesis = " (";
	public static readonly string Rule_OR = " OR ";
	public static readonly string Rule_THEN = " THEN ";
	
	public static readonly string Debug_AND = " AND ";
	public static readonly string Debug_Close_Parenthesis = ") ";
	public static readonly string Debug_IF = "IF\t\t";
	public static readonly string Debug_IS = " IS ";
	public static readonly string Debug_NOT = " NOT ";
	public static readonly string Debug_Open_Parenthesis = " (";
	public static readonly string Debug_OR = "\nOR\t\t";
	public static readonly string Debug_THEN = "\nTHEN\t";
	
	// Public instance properties
	public string ruleName;								// The name of the rule
	public AIEvent[] events = new AIEvent[0];			// Events
	public AIReaction[] reactions = new AIReaction[0];	// Reactions triggered when one of the events is true

	// Protected instance properties
	[HideInInspector] public bool debugToggle;
	[HideInInspector] public bool eventsToggle;
	[HideInInspector] public bool reactionsToggle;

	public object Clone() {
		return CloneObject.Clone(this, true);
	}

	// Public instance methods
	public List<string> ToRules(){
		List<string> rules = new List<string>();
		List<string> reactions = this.ReactionToStrings();

		if (reactions != null && reactions.Count > 0){
			string condition = this.ConditionToString();

			if (!string.IsNullOrEmpty(condition)){
				foreach (string reaction in reactions){
					rules.Add(condition + reaction);
				}
			}
		}

		return rules;
	}

	public List<string> ToDebugInformation(){
		List<string> debugInformation = new List<string>();
		List<string> rules = this.ToRules();

		if (rules != null && rules.Count > 0){
			foreach (string rule in rules){
				if (!string.IsNullOrEmpty(rule)){
					debugInformation.Add(
						rule.Replace(AIRule.Rule_AND, AIRule.Debug_AND)
							.Replace(AIRule.Rule_Close_Parenthesis, AIRule.Debug_Close_Parenthesis)
							.Replace(AIRule.Rule_IF, AIRule.Debug_IF)
							.Replace(AIRule.Rule_IS, AIRule.Debug_IS)
							.Replace(AIRule.Rule_NOT, AIRule.Debug_NOT)
							.Replace(AIRule.Rule_Open_Parenthesis, AIRule.Debug_Open_Parenthesis)
							.Replace(AIRule.Rule_OR, AIRule.Debug_OR)
							.Replace(AIRule.Rule_THEN, AIRule.Debug_THEN)
					);
				}
			}
		}

		return debugInformation;
	}

	// Protected instance methods
	protected string ConditionToString(){
		if (this.events != null && this.events.Length > 0){
			StringBuilder sb = new StringBuilder();

			foreach (AIEvent e in this.events){
				if (e != null && e.conditions != null && e.conditions.Length > 0 && e.enabled){
					StringBuilder sb2 = new StringBuilder();
					foreach (AICondition condition in e.conditions){
						if (condition != null && condition.enabled){
							TargetCharacter target = condition.targetCharacter;
							StringBuilder sb3 = new StringBuilder();

							if (condition.conditionType == AIConditionType.Distance){
								if (condition.playerDistance != CharacterDistance.Any && condition.playerDistance != CharacterDistance.Other){
									sb3	.Append(
											target == TargetCharacter.Self 
											? AICondition.Distance_Self 
											: AICondition.Distance_Opponent
										)
										.Append(AIRule.Rule_IS);

									if (condition.boolean == AIBoolean.FALSE){
										sb3.Append(AIRule.Rule_NOT);
									}
									sb3.Append(condition.playerDistance.ToString());
								}

							}else if (condition.conditionType == AIConditionType.Attacking){
								// Define the attack information
								if (condition.boolean == AIBoolean.FALSE){
									sb3.Append(AIRule.Rule_NOT).Append(AIRule.Rule_Open_Parenthesis);
								}

								sb3.Append(
									target == TargetCharacter.Self 
									? AICondition.Attacking_Self 
									: AICondition.Attacking_Opponent
								).Append (AIRule.Rule_IS).Append(AIBoolean.TRUE);

								if (condition.moveFrameData != CurrentFrameData.Any){
									sb3	.Append(AIRule.Rule_AND)
										.Append(
											target == TargetCharacter.Self 
											? AICondition.Attacking_FrameData_Self 
											: AICondition.Attacking_FrameData_Opponent
										)
										.Append(AIRule.Rule_IS)
										.Append(condition.moveFrameData.ToString());
								}
								if (!condition.moveClassification.anyAttackType){
									sb3	.Append(AIRule.Rule_AND)
										.Append(
											target == TargetCharacter.Self 
											? AICondition.Attacking_AttackType_Self 
											: AICondition.Attacking_AttackType_Opponent
										)
										.Append(AIRule.Rule_IS)
										.Append(condition.moveClassification.attackType.ToString());
								}
								if (!condition.moveClassification.anyHitConfirmType){
									sb3	.Append(AIRule.Rule_AND)
										.Append(
											target == TargetCharacter.Self 
											? AICondition.Attacking_HitConfirmType_Self 
											: AICondition.Attacking_HitConfirmType_Opponent
										)
										.Append(AIRule.Rule_IS)
										.Append(condition.moveClassification.hitConfirmType.ToString());
								}
								if (condition.moveClassification.startupSpeed != FrameSpeed.Any){
									sb3	.Append(AIRule.Rule_AND)
										.Append(
											target == TargetCharacter.Self 
											? AICondition.Attacking_StartupSpeed_Self
											: AICondition.Attacking_StartupSpeed_Opponent
										)
										.Append(AIRule.Rule_IS)
										.Append(condition.moveClassification.startupSpeed.ToString());
								}
								if (condition.moveClassification.recoverySpeed != FrameSpeed.Any){
									sb3	.Append(AIRule.Rule_AND)
										.Append(
											target == TargetCharacter.Self 
											? AICondition.Attacking_RecoverySpeed_Self
											: AICondition.Attacking_RecoverySpeed_Opponent
											)
											.Append(AIRule.Rule_IS)
											.Append(condition.moveClassification.recoverySpeed.ToString());
								}
								if (!condition.moveClassification.anyHitType){
									sb3	.Append(AIRule.Rule_AND)
										.Append(
											target == TargetCharacter.Self 
											? AICondition.Attacking_HitType_Self
											: AICondition.Attacking_HitType_Opponent
										)
										.Append(AIRule.Rule_IS)
										.Append(condition.moveClassification.hitType.ToString());
								}
								if (condition.moveDamage != AIDamage.Any){
									sb3	.Append(AIRule.Rule_AND)
										.Append(
											target == TargetCharacter.Self 
											? AICondition.Attacking_Damage_Self
											: AICondition.Attacking_Damage_Opponent
										)
										.Append(AIRule.Rule_IS)
										.Append(condition.moveDamage.ToString());
								}
								if (condition.moveClassification.gaugeUsage != GaugeUsage.Any){
									sb3	.Append(AIRule.Rule_AND)
										.Append(
											target == TargetCharacter.Self 
											? AICondition.Attacking_GaugeUsage_Self
											: AICondition.Attacking_GaugeUsage_Opponent
										)
										.Append(AIRule.Rule_IS)
										.Append(condition.moveClassification.gaugeUsage.ToString());
								}
								if (condition.moveClassification.preferableDistance != CharacterDistance.Any && condition.moveClassification.preferableDistance != CharacterDistance.Other){
									sb3	.Append(AIRule.Rule_AND)
										.Append(
											target == TargetCharacter.Self 
											? AICondition.Attacking_PreferableDistance_Self
											: AICondition.Attacking_PreferableDistance_Opponent
										)
										.Append(AIRule.Rule_IS)
										.Append(condition.moveClassification.preferableDistance.ToString());
								}

								if (condition.boolean == AIBoolean.FALSE){
									sb3.Append(AIRule.Rule_Close_Parenthesis);
								}


							}else if (condition.conditionType == AIConditionType.Blocking){
								sb3	.Append(
										target == TargetCharacter.Self 
										? AICondition.Blocking_Self
										: AICondition.Blocking_Opponent
									)
									.Append(AIRule.Rule_IS);

								if (condition.boolean == AIBoolean.FALSE){
									sb3.Append(AIRule.Rule_NOT);
								}
								sb3.Append(condition.blocking.ToString());

							}else if (condition.conditionType == AIConditionType.Down){
								sb3	.Append(
										target == TargetCharacter.Self 
										? AICondition.Down_Self
										: AICondition.Down_Opponent
									)
									.Append(AIRule.Rule_IS)
									.Append(condition.boolean);

							}else if (condition.conditionType == AIConditionType.GaugeStatus){
								sb3	.Append(
										target == TargetCharacter.Self 
										? AICondition.Gauge_Self
										: AICondition.Gauge_Opponent
									)
									.Append(AIRule.Rule_IS);

								if (condition.boolean == AIBoolean.FALSE){
									sb3.Append(AIRule.Rule_NOT);
								}
								sb3.Append(condition.gaugeStatus.ToString());

							}else if (condition.conditionType == AIConditionType.HealthStatus){
								sb3	.Append(
										target == TargetCharacter.Self 
										? AICondition.Health_Self
										: AICondition.Health_Opponent
									)
									.Append(AIRule.Rule_IS);

								if (condition.boolean == AIBoolean.FALSE){
									sb3.Append(AIRule.Rule_NOT);
								}
								sb3.Append(condition.healthStatus.ToString());

							}else if (condition.conditionType == AIConditionType.Idle){
								if (condition.boolean == AIBoolean.FALSE){
									sb3.Append(AIRule.Rule_NOT).Append(AIRule.Rule_Open_Parenthesis);
								}

								sb3	.Append(
										target == TargetCharacter.Self 
										? AICondition.VerticalMovement_Self
										: AICondition.VerticalMovement_Opponent
									)
									.Append(AIRule.Rule_IS)
									.Append(AIVerticalMovement.Standing.ToString())
									.Append(AIRule.Rule_AND)
									.Append(
										target == TargetCharacter.Self 
										? AICondition.HorizontalMovement_Self
										: AICondition.HorizontalMovement_Opponent
									)
									.Append(AIRule.Rule_IS)
									.Append(AIHorizontalMovement.Still.ToString());
									

								if (condition.boolean == AIBoolean.FALSE){
									sb3.Append(AIRule.Rule_Close_Parenthesis);
								}

							}else if (condition.conditionType == AIConditionType.HorizontalMovement){
								if (condition.boolean == AIBoolean.FALSE){
									sb3.Append(AIRule.Rule_NOT).Append(AIRule.Rule_Open_Parenthesis);
								}

								sb3	.Append(
										target == TargetCharacter.Self 
										? AICondition.HorizontalMovement_Self
										: AICondition.HorizontalMovement_Opponent
									)
									.Append(AIRule.Rule_IS)
									.Append(condition.horizontalMovement.ToString());

								if (condition.horizontalMovement != AIHorizontalMovement.Still && condition.movementSpeed != AIMovementSpeed.Any){
									sb3	.Append(AIRule.Rule_AND)
										.Append(
											target == TargetCharacter.Self 
											? AICondition.HorizontalMovementSpeed_Self
											: AICondition.HorizontalMovementSpeed_Opponent
										)
										.Append(AIRule.Rule_IS)
										.Append(condition.movementSpeed.ToString());
								}

								if (condition.boolean == AIBoolean.FALSE){
									sb3.Append(AIRule.Rule_Close_Parenthesis);
								}

							}else if (condition.conditionType == AIConditionType.VerticalMovement){
								if (condition.boolean == AIBoolean.FALSE){
									sb3.Append(AIRule.Rule_NOT).Append(AIRule.Rule_Open_Parenthesis);
								}
								
								sb3	.Append(
										target == TargetCharacter.Self 
										? AICondition.VerticalMovement_Self
										: AICondition.VerticalMovement_Opponent
									)
									.Append(AIRule.Rule_IS)
									.Append(condition.verticalMovement.ToString());

								if (
									condition.verticalMovement == AIVerticalMovement.Jumping && 
									condition.jumping != JumpArc.Any &&
								 	condition.jumping != JumpArc.Other
								 ){
									sb3	.Append(AIRule.Rule_AND)
										.Append(
											target == TargetCharacter.Self 
											? AICondition.JumpArc_Self
											: AICondition.JumpArc_Opponent
										)
										.Append(AIRule.Rule_IS)
										.Append(condition.jumping.ToString());
								}
								
								if (condition.boolean == AIBoolean.FALSE){
									sb3.Append(AIRule.Rule_Close_Parenthesis);
								}

							}else if (condition.conditionType == AIConditionType.Stunned){
								sb3	.Append(
										target == TargetCharacter.Self 
										? AICondition.Stunned_Self
										: AICondition.Stunned_Opponent
									)
									.Append(AIRule.Rule_IS)
									.Append(condition.boolean);
							}

							if (sb3.Length > 0){
								if (sb2.Length == 0){
									sb2.Append(AIRule.Rule_Open_Parenthesis);
								}else{
									sb2.Append(AIRule.Rule_AND);
								}

								sb2.Append(sb3.ToString());
							}
						}
					}

					if (sb2.Length > 0){
						if (sb.Length <= 0){
							sb.Append(AIRule.Rule_IF);
						}else{
							sb.Append(AIRule.Rule_OR);
						}
						
						if (e.boolean == AIBoolean.FALSE){
							sb.Append(AIRule.Rule_NOT);
						}

						sb.Append(sb2.ToString()).Append(AIRule.Rule_Close_Parenthesis);
					}
				}
			}

			if (sb.Length > 0){
				sb.Append(AIRule.Rule_THEN);
				return sb.ToString();
			}
		}
		return string.Empty;
	}

	protected List<string> ReactionToStrings(){
		List<string> reactions = new List<string>();

		// Iterate over all the reactions associated to this rule...
		if (this.reactions != null){
			foreach (AIReaction reaction in this.reactions){
				if (reaction != null){
					// Create the desirability string..
					string desirability = AIRule.Rule_IS + reaction.desirability;

					// Find out the type of reaction...
					if (reaction.reactionType == AIReactionType.Crouch){
						reactions.Add(AIReaction.Crouch + desirability);
					}else if (reaction.reactionType == AIReactionType.CrouchBlock){
						reactions.Add(AIReaction.CrouchBlock + desirability);
					}else if (reaction.reactionType == AIReactionType.ChangeBehavior){
						if (reaction.behavior == AIBehavior.Aggressive){
							reactions.Add(AIReaction.ChangeBehaviour_Aggressive + desirability);
						}else if (reaction.behavior == AIBehavior.Any){
							reactions.Add(AIReaction.ChangeBehaviour_Any + desirability);
						}else if (reaction.behavior == AIBehavior.Balanced){
							reactions.Add(AIReaction.ChangeBehaviour_Balanced + desirability);
						}else if (reaction.behavior == AIBehavior.Defensive){
							reactions.Add(AIReaction.ChangeBehaviour_Defensive + desirability);
						}else if (reaction.behavior == AIBehavior.VeryAggressive){
							reactions.Add(AIReaction.ChangeBehaviour_VeryAggressive + desirability);
						}else if (reaction.behavior == AIBehavior.VeryDefensive){
							reactions.Add(AIReaction.ChangeBehaviour_VeryDefensive + desirability);
						}
					}else if (reaction.reactionType == AIReactionType.Idle){
						reactions.Add(AIReaction.Idle + desirability);
					}else if (reaction.reactionType == AIReactionType.JumpBack){
						reactions.Add(AIReaction.JumpBackward + desirability);
					}else if (reaction.reactionType == AIReactionType.JumpForward){
						reactions.Add(AIReaction.JumpForward + desirability);
					}else if (reaction.reactionType == AIReactionType.JumpStraight){
						reactions.Add(AIReaction.JumpStraight + desirability);
					}else if (reaction.reactionType == AIReactionType.MoveBack){
						reactions.Add(AIReaction.MoveBackward + desirability);
					}else if (reaction.reactionType == AIReactionType.MoveForward){
						reactions.Add(AIReaction.MoveForward + desirability);
					}else if (reaction.reactionType == AIReactionType.PlayMove){
						// If it's an attack, define the type of attack...
						List<string> attackInformation = new List<string>();

						if (!reaction.moveClassification.anyAttackType){
							if (reaction.moveClassification.attackType == AttackType.AntiAir){
								attackInformation.Add(AIReaction.PlayMove_AttackType_AntiAir + desirability);
							}else if (reaction.moveClassification.attackType == AttackType.BackLauncher){
								attackInformation.Add(AIReaction.PlayMove_AttackType_BackLauncher + desirability);
							}else if (reaction.moveClassification.attackType == AttackType.Dive){
								attackInformation.Add(AIReaction.PlayMove_AttackType_Dive + desirability);
							}else if (reaction.moveClassification.attackType == AttackType.ForwardLauncher){
								attackInformation.Add(AIReaction.PlayMove_AttackType_ForwardLauncher + desirability);
							}else if (reaction.moveClassification.attackType == AttackType.Neutral){
								attackInformation.Add(AIReaction.PlayMove_AttackType_Neutral + desirability);
							}else if (reaction.moveClassification.attackType == AttackType.NormalAttack){
								attackInformation.Add(AIReaction.PlayMove_AttackType_NormalAttack + desirability);
							}else if (reaction.moveClassification.attackType == AttackType.Projectile){
								attackInformation.Add(AIReaction.PlayMove_AttackType_Projectile + desirability);
							}
						}
						if (!reaction.moveClassification.anyHitConfirmType){
							if (reaction.moveClassification.hitConfirmType == HitConfirmType.Hit){
								attackInformation.Add(AIReaction.PlayMove_HitConfirmType_Hit + desirability);
							}else if (reaction.moveClassification.hitConfirmType == HitConfirmType.Throw){
								attackInformation.Add(AIReaction.PlayMove_HitConfirmType_Throw + desirability);
							}
						}

						if (reaction.moveClassification.startupSpeed != FrameSpeed.Any){
							if (reaction.moveClassification.startupSpeed == FrameSpeed.VeryFast){
								attackInformation.Add(AIReaction.PlayMove_StartupSpeed_VeryFast + desirability);
							}else if (reaction.moveClassification.startupSpeed == FrameSpeed.Fast){
								attackInformation.Add(AIReaction.PlayMove_StartupSpeed_Fast + desirability);
							}else if (reaction.moveClassification.startupSpeed == FrameSpeed.Normal){
								attackInformation.Add(AIReaction.PlayMove_StartupSpeed_Normal + desirability);
							}else if (reaction.moveClassification.startupSpeed == FrameSpeed.Slow){
								attackInformation.Add(AIReaction.PlayMove_StartupSpeed_Slow + desirability);
							}else if (reaction.moveClassification.startupSpeed == FrameSpeed.Slow){
								attackInformation.Add(AIReaction.PlayMove_StartupSpeed_VerySlow + desirability);
							}
						}

						if (reaction.moveClassification.recoverySpeed != FrameSpeed.Any){
							if (reaction.moveClassification.recoverySpeed == FrameSpeed.VeryFast){
								attackInformation.Add(AIReaction.PlayMove_RecoverySpeed_VeryFast + desirability);
							}else if (reaction.moveClassification.recoverySpeed == FrameSpeed.Fast){
								attackInformation.Add(AIReaction.PlayMove_RecoverySpeed_Fast + desirability);
							}else if (reaction.moveClassification.recoverySpeed == FrameSpeed.Normal){
								attackInformation.Add(AIReaction.PlayMove_RecoverySpeed_Normal + desirability);
							}else if (reaction.moveClassification.recoverySpeed == FrameSpeed.Slow){
								attackInformation.Add(AIReaction.PlayMove_RecoverySpeed_Slow + desirability);
							}else if (reaction.moveClassification.recoverySpeed == FrameSpeed.Slow){
								attackInformation.Add(AIReaction.PlayMove_RecoverySpeed_VerySlow + desirability);
							}
						}

						if (!reaction.moveClassification.anyHitType){
							if (reaction.moveClassification.hitType == HitType.HighKnockdown){
								attackInformation.Add(AIReaction.PlayMove_HitType_HighKnockdown + desirability);
							}else if (reaction.moveClassification.hitType == HitType.Mid){
								attackInformation.Add(AIReaction.PlayMove_HitType_HighLow + desirability);
							}else if (reaction.moveClassification.hitType == HitType.KnockBack){
								attackInformation.Add(AIReaction.PlayMove_HitType_KnockBack + desirability);
							}else if (reaction.moveClassification.hitType == HitType.Launcher){
								attackInformation.Add(AIReaction.PlayMove_HitType_Launcher + desirability);
							}else if (reaction.moveClassification.hitType == HitType.Low){
								attackInformation.Add(AIReaction.PlayMove_HitType_Low + desirability);
							}else if (reaction.moveClassification.hitType == HitType.MidKnockdown){
								attackInformation.Add(AIReaction.PlayMove_HitType_MidKnockdown + desirability);
							}else if (reaction.moveClassification.hitType == HitType.Overhead){
								attackInformation.Add(AIReaction.PlayMove_HitType_Overhead + desirability);
							}else if (reaction.moveClassification.hitType == HitType.Sweep){
								attackInformation.Add(AIReaction.PlayMove_HitType_Sweep + desirability);
							}

						}
						if (reaction.moveDamage != AIDamage.Any){
							if (reaction.moveDamage == AIDamage.VeryWeak){
								attackInformation.Add(AIReaction.PlayMove_Damage_VeryWeak + desirability);
							}else if (reaction.moveDamage == AIDamage.Weak){
								attackInformation.Add(AIReaction.PlayMove_Damage_Weak + desirability);
							}else if (reaction.moveDamage == AIDamage.Medium){
								attackInformation.Add(AIReaction.PlayMove_Damage_Medium + desirability);
							}else if (reaction.moveDamage == AIDamage.Strong){
								attackInformation.Add(AIReaction.PlayMove_Damage_Strong + desirability);
							}else if (reaction.moveDamage == AIDamage.VeryStrong){
								attackInformation.Add(AIReaction.PlayMove_Damage_VeryStrong + desirability);
							}

						}
						if (reaction.moveClassification.gaugeUsage != GaugeUsage.Any){
							if (reaction.moveClassification.gaugeUsage == GaugeUsage.None){
								attackInformation.Add(AIReaction.PlayMove_GaugeUsage_None + desirability);
							}else if (reaction.moveClassification.gaugeUsage == GaugeUsage.Quarter){
								attackInformation.Add(AIReaction.PlayMove_GaugeUsage_Quarter + desirability);
							}else if (reaction.moveClassification.gaugeUsage == GaugeUsage.Half){
								attackInformation.Add(AIReaction.PlayMove_GaugeUsage_Half + desirability);
							}else if (reaction.moveClassification.gaugeUsage == GaugeUsage.ThreeQuarters){
								attackInformation.Add(AIReaction.PlayMove_GaugeUsage_ThreeQuarters + desirability);
							}else if (reaction.moveClassification.gaugeUsage == GaugeUsage.All){
								attackInformation.Add(AIReaction.PlayMove_GaugeUsage_All + desirability);
							}

						}
						if (reaction.moveClassification.preferableDistance != CharacterDistance.Any && reaction.moveClassification.preferableDistance != CharacterDistance.Other){
							if (reaction.moveClassification.preferableDistance == CharacterDistance.VeryClose){
								attackInformation.Add(AIReaction.PlayMove_PreferableDistance_VeryClose + desirability);
							}else if (reaction.moveClassification.preferableDistance == CharacterDistance.Close){
								attackInformation.Add(AIReaction.PlayMove_PreferableDistance_Close + desirability);
							}else if (reaction.moveClassification.preferableDistance == CharacterDistance.Mid){
								attackInformation.Add(AIReaction.PlayMove_PreferableDistance_Mid + desirability);
							}else if (reaction.moveClassification.preferableDistance == CharacterDistance.Far){
								attackInformation.Add(AIReaction.PlayMove_PreferableDistance_Far + desirability);
							}else if (reaction.moveClassification.preferableDistance == CharacterDistance.VeryFar){
								attackInformation.Add(AIReaction.PlayMove_PreferableDistance_VeryFar + desirability);
							}
						}

						// If we don't have any information about the attack, choose a random attack...
						if (attackInformation.Count > 0){
							reactions.AddRange(attackInformation);
						}else{
							reactions.Add(AIReaction.PlayMove_RandomAttack + desirability);
						}

					//}else if (reaction.reactionType == AIReactionType.PlaySpecificMove){
					//}else if (reaction.reactionType == AIReactionType.PressButton){
					}else if (reaction.reactionType == AIReactionType.StandBlock){
						reactions.Add(AIReaction.StandBlock + desirability);
					}
				}
			}
		}

		return reactions;
	}
}

namespace UFE3D
{
    [Serializable]
    public class AIInfo : ScriptableObject
    {
        // public instance properties
        public string instructionsName;
        public bool debugMode;
        public bool debug_ReactionWeight;
        public AIAdvancedOptions advancedOptions;
        public AIRulesGenerator rulesGenerator;
        public AIRule[] aiRules = new AIRule[0];
        public AIDefinitions aiDefinitions;

        //-----------------------------------------------------------------------------------------------------------------
        // PUBLIC METHODS
        //-----------------------------------------------------------------------------------------------------------------
        public float GetDesirabilityScore(AIDesirability desirability)
        {
            switch (desirability)
            {
                case AIDesirability.Desirable: return this.aiDefinitions.desirability.desirable;
                case AIDesirability.NotBad: return this.aiDefinitions.desirability.notBad;
                case AIDesirability.Undesirable: return this.aiDefinitions.desirability.undesirable;
                case AIDesirability.TheBestOption: return this.aiDefinitions.desirability.theBestOption;
                case AIDesirability.TheWorstOption: return this.aiDefinitions.desirability.theWorstOption;
                case AIDesirability.VeryDesirable: return this.aiDefinitions.desirability.veryDesirable;
                case AIDesirability.VeryUndesirable: return this.aiDefinitions.desirability.veryUndesirable;
                default: return 0f;
            }
        }

        public AI4Unity.Fuzzy.InferenceSystem GenerateInferenceSystem()
        {
            AI4Unity.Fuzzy.InferenceSystem inferenceSystem = new AI4Unity.Fuzzy.InferenceSystem(DefuzzificationMethod.Average);

            // INPUT VARIABLES
            inferenceSystem.AddInputVariable(this.DefineBooleanVariable(AICondition.Attacking_Self));
            inferenceSystem.AddInputVariable(this.DefineAttackTypeVariable(AICondition.Attacking_AttackType_Self));
            inferenceSystem.AddInputVariable(this.DefineDamageVariable(AICondition.Attacking_Damage_Self, 0f, 1f));
            inferenceSystem.AddInputVariable(this.DefineGaugeVariable(AICondition.Attacking_GaugeUsage_Self));
            inferenceSystem.AddInputVariable(this.DefineHitConfirmTypeVariable(AICondition.Attacking_HitConfirmType_Self));
            inferenceSystem.AddInputVariable(this.DefineFrameSpeedVariable(AICondition.Attacking_StartupSpeed_Self));
            inferenceSystem.AddInputVariable(this.DefineFrameSpeedVariable(AICondition.Attacking_RecoverySpeed_Self));
            inferenceSystem.AddInputVariable(this.DefineHitTypeVariable(AICondition.Attacking_HitType_Self));
            inferenceSystem.AddInputVariable(this.DefineFrameDataVariable(AICondition.Attacking_FrameData_Self));
            inferenceSystem.AddInputVariable(this.DefineDistanceVariable(AICondition.Attacking_PreferableDistance_Self));
            inferenceSystem.AddInputVariable(this.DefineBlockingVariable(AICondition.Blocking_Self));
            inferenceSystem.AddInputVariable(this.DefineDistanceVariable(AICondition.Distance_Self, 0f, 1f));
            inferenceSystem.AddInputVariable(this.DefineBooleanVariable(AICondition.Down_Self));
            inferenceSystem.AddInputVariable(this.DefineGaugeVariable(AICondition.Gauge_Self));
            inferenceSystem.AddInputVariable(this.DefineHealthVariable(AICondition.Health_Self, 0f, 1f));
            inferenceSystem.AddInputVariable(this.DefineHorizontalMovementVariable(AICondition.HorizontalMovement_Self));
            inferenceSystem.AddInputVariable(this.DefineMovementSpeedVariable(AICondition.HorizontalMovementSpeed_Self, 0f, 100f));
            inferenceSystem.AddInputVariable(this.DefineJumpArcVariable(AICondition.JumpArc_Self));
            inferenceSystem.AddInputVariable(this.DefineBooleanVariable(AICondition.Stunned_Self));
            inferenceSystem.AddInputVariable(this.DefineVerticalMovementVariable(AICondition.VerticalMovement_Self));


            inferenceSystem.AddInputVariable(this.DefineBooleanVariable(AICondition.Attacking_Opponent));
            inferenceSystem.AddInputVariable(this.DefineAttackTypeVariable(AICondition.Attacking_AttackType_Opponent));
            inferenceSystem.AddInputVariable(this.DefineDamageVariable(AICondition.Attacking_Damage_Opponent, 0f, 1));
            inferenceSystem.AddInputVariable(this.DefineGaugeVariable(AICondition.Attacking_GaugeUsage_Opponent));
            inferenceSystem.AddInputVariable(this.DefineHitConfirmTypeVariable(AICondition.Attacking_HitConfirmType_Opponent));
            inferenceSystem.AddInputVariable(this.DefineFrameSpeedVariable(AICondition.Attacking_StartupSpeed_Opponent));
            inferenceSystem.AddInputVariable(this.DefineFrameSpeedVariable(AICondition.Attacking_RecoverySpeed_Opponent));
            inferenceSystem.AddInputVariable(this.DefineHitTypeVariable(AICondition.Attacking_HitType_Opponent));
            inferenceSystem.AddInputVariable(this.DefineFrameDataVariable(AICondition.Attacking_FrameData_Opponent));
            inferenceSystem.AddInputVariable(this.DefineDistanceVariable(AICondition.Attacking_PreferableDistance_Opponent));
            inferenceSystem.AddInputVariable(this.DefineBlockingVariable(AICondition.Blocking_Opponent));
            inferenceSystem.AddInputVariable(this.DefineDistanceVariable(AICondition.Distance_Opponent, 0f, 1f));
            inferenceSystem.AddInputVariable(this.DefineBooleanVariable(AICondition.Down_Opponent));
            inferenceSystem.AddInputVariable(this.DefineGaugeVariable(AICondition.Gauge_Opponent));
            inferenceSystem.AddInputVariable(this.DefineHealthVariable(AICondition.Health_Opponent, 0f, 1f));
            inferenceSystem.AddInputVariable(this.DefineHorizontalMovementVariable(AICondition.HorizontalMovement_Opponent));
            inferenceSystem.AddInputVariable(this.DefineMovementSpeedVariable(AICondition.HorizontalMovementSpeed_Opponent, 0f, 100f));
            inferenceSystem.AddInputVariable(this.DefineJumpArcVariable(AICondition.JumpArc_Opponent));
            inferenceSystem.AddInputVariable(this.DefineBooleanVariable(AICondition.Stunned_Opponent));
            inferenceSystem.AddInputVariable(this.DefineVerticalMovementVariable(AICondition.VerticalMovement_Opponent));

            // OUTPUT VARIABLES
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.Crouch));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.CrouchBlock));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.Idle));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.JumpBlock));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.JumpBackward));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.JumpForward));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.JumpStraight));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.MoveForward));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.MoveBackward));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.StandBlock));

            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.ChangeBehaviour_Aggressive));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.ChangeBehaviour_Any));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.ChangeBehaviour_Balanced));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.ChangeBehaviour_Defensive));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.ChangeBehaviour_VeryAggressive));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.ChangeBehaviour_VeryDefensive));

            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_RandomAttack));

            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_AttackType_AntiAir));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_AttackType_BackLauncher));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_AttackType_Dive));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_AttackType_ForwardLauncher));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_AttackType_Neutral));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_AttackType_NormalAttack));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_AttackType_Projectile));

            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_Damage_Medium));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_Damage_Strong));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_Damage_VeryStrong));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_Damage_VeryWeak));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_Damage_Weak));

            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_GaugeUsage_All));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_GaugeUsage_Half));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_GaugeUsage_None));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_GaugeUsage_Quarter));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_GaugeUsage_ThreeQuarters));

            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_HitConfirmType_Hit));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_HitConfirmType_Throw));

            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_StartupSpeed_VeryFast));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_StartupSpeed_Fast));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_StartupSpeed_Normal));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_StartupSpeed_Slow));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_StartupSpeed_VerySlow));

            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_RecoverySpeed_VeryFast));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_RecoverySpeed_Fast));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_RecoverySpeed_Normal));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_RecoverySpeed_Slow));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_RecoverySpeed_VerySlow));

            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_HitType_HighKnockdown));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_HitType_HighLow));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_HitType_KnockBack));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_HitType_Launcher));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_HitType_Low));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_HitType_MidKnockdown));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_HitType_Overhead));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_HitType_Sweep));

            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_PreferableDistance_Close));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_PreferableDistance_Far));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_PreferableDistance_Mid));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_PreferableDistance_VeryClose));
            inferenceSystem.AddOutputVariable(this.DefineOutputVariable(AIReaction.PlayMove_PreferableDistance_VeryFar));

            string generatedRulePrefix = "Generated Rule: ";
            string userRulePrefix = "User Rule: ";
            int suffix = 1;

            // Add the fuzzy rules generated automatically
            foreach (string fuzzyRule in this.rulesGenerator.GenerateRules())
            {
                if (!string.IsNullOrEmpty(fuzzyRule))
                {
                    inferenceSystem.NewRule(generatedRulePrefix + suffix, fuzzyRule);
                    ++suffix;
                }
            }

            // Generate the Inference System with all the Rules defined by the user
            foreach (AIRule rule in this.aiRules)
            {
                if (rule != null && !string.IsNullOrEmpty(rule.ruleName))
                {
                    List<string> fuzzyRules = rule.ToRules();

                    if (fuzzyRules != null)
                    {
                        if (fuzzyRules.Count == 1)
                        {
                            string fuzzyRule = fuzzyRules[0];
                            if (!string.IsNullOrEmpty(fuzzyRule))
                            {
                                inferenceSystem.NewRule(userRulePrefix + rule.ruleName, fuzzyRule);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < fuzzyRules.Count; ++i)
                            {
                                string fuzzyRule = fuzzyRules[i];
                                if (!string.IsNullOrEmpty(fuzzyRule))
                                {
                                    inferenceSystem.NewRule(userRulePrefix + rule.ruleName + "_" + (i + 1), fuzzyRule);
                                }
                            }
                        }
                    }
                }
            }

            // Finally, return the generated Inference System
            return inferenceSystem;
        }

        //-----------------------------------------------------------------------------------------------------------------
        // PROTECTED METHODS
        //-----------------------------------------------------------------------------------------------------------------
        protected LinguisticVariable DefineBooleanVariable(string name, float start = -1f, float end = 1f)
        {
            LinguisticVariable varAttacking = new LinguisticVariable(name, start, end);
            varAttacking.AddLabel(new FuzzySet(AIBoolean.FALSE.ToString(), new SingletonFunction((float)((int)AIBoolean.FALSE))));
            varAttacking.AddLabel(new FuzzySet(AIBoolean.TRUE.ToString(), new SingletonFunction((float)((int)AIBoolean.TRUE))));

            return varAttacking;
        }

        protected LinguisticVariable DefineAttackTypeVariable(string name, float start = -1f, float end = 6f)
        {
            LinguisticVariable varAttackType = new LinguisticVariable(name, start, end);
            varAttackType.AddLabel(new FuzzySet(AttackType.AntiAir.ToString(), new SingletonFunction((float)((int)AttackType.AntiAir))));
            varAttackType.AddLabel(new FuzzySet(AttackType.BackLauncher.ToString(), new SingletonFunction((float)((int)AttackType.BackLauncher))));
            varAttackType.AddLabel(new FuzzySet(AttackType.Dive.ToString(), new SingletonFunction((float)((int)AttackType.Dive))));
            varAttackType.AddLabel(new FuzzySet(AttackType.ForwardLauncher.ToString(), new SingletonFunction((float)((int)AttackType.ForwardLauncher))));
            varAttackType.AddLabel(new FuzzySet(AttackType.Neutral.ToString(), new SingletonFunction((float)((int)AttackType.Neutral))));
            varAttackType.AddLabel(new FuzzySet(AttackType.NormalAttack.ToString(), new SingletonFunction((float)((int)AttackType.NormalAttack))));
            varAttackType.AddLabel(new FuzzySet(AttackType.Projectile.ToString(), new SingletonFunction((float)((int)AttackType.Projectile))));

            return varAttackType;
        }

        protected LinguisticVariable DefineGaugeVariable(string name, float start = -1f, float end = 4f)
        {
            LinguisticVariable varGaugeUsage = new LinguisticVariable(name, start, end);
            varGaugeUsage.AddLabel(new FuzzySet(GaugeUsage.None.ToString(), new TrapezoidalFunction(start, 0.00f, 0.24f, 0.26f)));
            varGaugeUsage.AddLabel(new FuzzySet(GaugeUsage.Quarter.ToString(), new TrapezoidalFunction(0.25f, 0.25f, 0.49f, 0.51f)));
            varGaugeUsage.AddLabel(new FuzzySet(GaugeUsage.Half.ToString(), new TrapezoidalFunction(0.50f, 0.50f, 0.74f, 0.76f)));
            varGaugeUsage.AddLabel(new FuzzySet(GaugeUsage.ThreeQuarters.ToString(), new TrapezoidalFunction(0.75f, 0.75f, 0.99f, 1.01f)));
            varGaugeUsage.AddLabel(new FuzzySet(GaugeUsage.All.ToString(), new TrapezoidalFunction(1.00f, 1.00f, end)));

            return varGaugeUsage;
        }

        protected LinguisticVariable DefineHitConfirmTypeVariable(string name, float start = -1f, float end = 1f)
        {
            LinguisticVariable varHitConfirmType = new LinguisticVariable(name, start, end);
            varHitConfirmType.AddLabel(new FuzzySet(HitConfirmType.Hit.ToString(), new SingletonFunction((float)((int)HitConfirmType.Hit))));
            varHitConfirmType.AddLabel(new FuzzySet(HitConfirmType.Throw.ToString(), new SingletonFunction((float)((int)HitConfirmType.Throw))));

            return varHitConfirmType;
        }

        protected LinguisticVariable DefineFrameSpeedVariable(string name, float start = -1f, float end = 4f)
        {
            LinguisticVariable varFrameSpeed = new LinguisticVariable(name, start, end);
            varFrameSpeed.AddLabel(new FuzzySet(FrameSpeed.VerySlow.ToString(), new SingletonFunction(0f)));
            varFrameSpeed.AddLabel(new FuzzySet(FrameSpeed.Slow.ToString(), new SingletonFunction(1f)));
            varFrameSpeed.AddLabel(new FuzzySet(FrameSpeed.Normal.ToString(), new SingletonFunction(2f)));
            varFrameSpeed.AddLabel(new FuzzySet(FrameSpeed.Fast.ToString(), new SingletonFunction(3f)));
            varFrameSpeed.AddLabel(new FuzzySet(FrameSpeed.VeryFast.ToString(), new SingletonFunction(4f)));

            return varFrameSpeed;
        }

        protected LinguisticVariable DefineHitTypeVariable(string name, float start = -1f, float end = 7f)
        {
            LinguisticVariable varHitType = new LinguisticVariable(name, start, end);
            varHitType.AddLabel(new FuzzySet(HitType.HighKnockdown.ToString(), new SingletonFunction((float)((int)HitType.HighKnockdown))));
            varHitType.AddLabel(new FuzzySet(HitType.Mid.ToString(), new SingletonFunction((float)((int)HitType.Mid))));
            varHitType.AddLabel(new FuzzySet(HitType.KnockBack.ToString(), new SingletonFunction((float)((int)HitType.KnockBack))));
            varHitType.AddLabel(new FuzzySet(HitType.Launcher.ToString(), new SingletonFunction((float)((int)HitType.Launcher))));
            varHitType.AddLabel(new FuzzySet(HitType.Low.ToString(), new SingletonFunction((float)((int)HitType.Low))));
            varHitType.AddLabel(new FuzzySet(HitType.MidKnockdown.ToString(), new SingletonFunction((float)((int)HitType.MidKnockdown))));
            varHitType.AddLabel(new FuzzySet(HitType.Overhead.ToString(), new SingletonFunction((float)((int)HitType.Overhead))));
            varHitType.AddLabel(new FuzzySet(HitType.Sweep.ToString(), new SingletonFunction((float)((int)HitType.Sweep))));

            return varHitType;
        }

        protected LinguisticVariable DefineFrameDataVariable(string name, float start = -1f, float end = 3f)
        {
            LinguisticVariable varFrameData = new LinguisticVariable(name, start, end);
            varFrameData.AddLabel(new FuzzySet(CurrentFrameData.ActiveFrames.ToString(), new SingletonFunction((float)((int)CurrentFrameData.ActiveFrames))));
            varFrameData.AddLabel(new FuzzySet(CurrentFrameData.RecoveryFrames.ToString(), new SingletonFunction((float)((int)CurrentFrameData.RecoveryFrames))));
            varFrameData.AddLabel(new FuzzySet(CurrentFrameData.StartupFrames.ToString(), new SingletonFunction((float)((int)CurrentFrameData.StartupFrames))));

            return varFrameData;
        }

        protected LinguisticVariable DefineBlockingVariable(string name, float start = -1f, float end = 2f)
        {
            LinguisticVariable varBlocking = new LinguisticVariable(name, start, end);
            varBlocking.AddLabel(new FuzzySet(AIBlocking.Air.ToString(), new SingletonFunction((float)((int)AIBlocking.Air))));
            varBlocking.AddLabel(new FuzzySet(AIBlocking.High.ToString(), new SingletonFunction((float)((int)AIBlocking.High))));
            varBlocking.AddLabel(new FuzzySet(AIBlocking.Low.ToString(), new SingletonFunction((float)((int)AIBlocking.Low))));

            return varBlocking;
        }

        protected LinguisticVariable DefineHorizontalMovementVariable(string name, float start = -1f, float end = 2f)
        {
            LinguisticVariable varHorizontalMovement = new LinguisticVariable(name, start, end);
            varHorizontalMovement.AddLabel(new FuzzySet(AIHorizontalMovement.MovingBack.ToString(), new SingletonFunction((float)((int)AIHorizontalMovement.MovingBack))));
            varHorizontalMovement.AddLabel(new FuzzySet(AIHorizontalMovement.MovingForward.ToString(), new SingletonFunction((float)((int)AIHorizontalMovement.MovingForward))));
            varHorizontalMovement.AddLabel(new FuzzySet(AIHorizontalMovement.Still.ToString(), new SingletonFunction((float)((int)AIHorizontalMovement.Still))));

            return varHorizontalMovement;
        }

        protected LinguisticVariable DefineJumpArcVariable(string name, float start = 0f, float end = 1f)
        {
            LinguisticVariable varJumpArc = new LinguisticVariable(name, start - 1f, end + 1f);
            varJumpArc.AddLabel(new FuzzySet(
                JumpArc.TakeOff.ToString(),
                new TrapezoidalFunction(start - 1f, start, 0.3f, 0.4f)
            ));
            varJumpArc.AddLabel(new FuzzySet(
                JumpArc.Jumping.ToString(),
                new TrapezoidalFunction(0.3f, 0.4f, 0.55f, 0.65f)
            ));
            varJumpArc.AddLabel(new FuzzySet(
                JumpArc.Top.ToString(),
                new TrapezoidalFunction(0.55f, 0.65f, 0.75f)
            ));
            varJumpArc.AddLabel(new FuzzySet(
                JumpArc.Falling.ToString(),
                new TrapezoidalFunction(0.65f, 0.75f, 0.85f, 0.95f)
            ));
            varJumpArc.AddLabel(new FuzzySet(
                JumpArc.Landing.ToString(),
                new TrapezoidalFunction(0.85f, 0.95f, end, end + 1f)
            ));

            return varJumpArc;
        }

        protected LinguisticVariable DefineVerticalMovementVariable(string name, float start = 0f, float end = 2f)
        {
            LinguisticVariable varVerticalMovement = new LinguisticVariable(name, start, end);
            varVerticalMovement.AddLabel(new FuzzySet(AIVerticalMovement.Crouching.ToString(), new SingletonFunction((float)((int)AIVerticalMovement.Crouching))));
            varVerticalMovement.AddLabel(new FuzzySet(AIVerticalMovement.Jumping.ToString(), new SingletonFunction((float)((int)AIVerticalMovement.Jumping))));
            varVerticalMovement.AddLabel(new FuzzySet(AIVerticalMovement.Standing.ToString(), new SingletonFunction((float)((int)AIVerticalMovement.Standing))));

            return varVerticalMovement;
        }

        protected LinguisticVariable DefineDamageVariable(string name, float start = 0f, float end = 1f)
        {
            LinguisticVariable varDamage = new LinguisticVariable(name, start - 1f, end + 1f);
            varDamage.AddLabel(new FuzzySet(
                AIDamage.VeryWeak.ToString(),
                new TrapezoidalFunction(start - 1f, start, this.aiDefinitions.damage.veryWeak, this.aiDefinitions.damage.weak)
            ));
            varDamage.AddLabel(new FuzzySet(
                AIDamage.Weak.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.damage.veryWeak, this.aiDefinitions.damage.weak, this.aiDefinitions.damage.medium)
            ));
            varDamage.AddLabel(new FuzzySet(
                AIDamage.Medium.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.damage.weak, this.aiDefinitions.damage.medium, this.aiDefinitions.damage.strong)
            ));
            varDamage.AddLabel(new FuzzySet(
                AIDamage.Strong.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.damage.medium, this.aiDefinitions.damage.strong, this.aiDefinitions.damage.veryStrong)
            ));
            varDamage.AddLabel(new FuzzySet(
                AIDamage.VeryStrong.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.damage.strong, this.aiDefinitions.damage.veryStrong, end, end + 1f)
            ));

            return varDamage;
        }

        protected LinguisticVariable DefineDistanceVariable(string name, float start = 0f, float end = 1f)
        {
            LinguisticVariable varDistance = new LinguisticVariable(name, start - 1f, end + 1f);
            varDistance.AddLabel(new FuzzySet(
                CharacterDistance.VeryClose.ToString(),
                new TrapezoidalFunction(start - 1f, start, this.aiDefinitions.distance.veryClose, this.aiDefinitions.distance.close)
            ));
            varDistance.AddLabel(new FuzzySet(
                CharacterDistance.Close.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.distance.veryClose, this.aiDefinitions.distance.close, this.aiDefinitions.distance.mid)
            ));
            varDistance.AddLabel(new FuzzySet(
                CharacterDistance.Mid.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.distance.close, this.aiDefinitions.distance.mid, this.aiDefinitions.distance.far)
            ));
            varDistance.AddLabel(new FuzzySet(
                CharacterDistance.Far.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.distance.mid, this.aiDefinitions.distance.far, this.aiDefinitions.distance.veryFar)
            ));
            varDistance.AddLabel(new FuzzySet(
                CharacterDistance.VeryFar.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.distance.far, this.aiDefinitions.distance.veryFar, end, end + 1f)
            ));

            return varDistance;
        }

        protected LinguisticVariable DefineHealthVariable(string name, float start = 0f, float end = 1f)
        {
            LinguisticVariable varHealth = new LinguisticVariable(name, start - 1f, end + 1f);
            if (this.aiDefinitions.health.healthy <= start)
            {
                varHealth.AddLabel(new FuzzySet(HealthStatus.Dead.ToString(), new SingletonFunction(start)));
            }
            else
            {
                varHealth.AddLabel(new FuzzySet(
                    HealthStatus.Dead.ToString(),
                    new TrapezoidalFunction(start - 1f, start, this.aiDefinitions.health.dead, this.aiDefinitions.health.almostDead)
                ));
            }
            varHealth.AddLabel(new FuzzySet(
                HealthStatus.AlmostDead.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.health.dead, this.aiDefinitions.health.almostDead, this.aiDefinitions.health.criticallyWounded)
            ));
            varHealth.AddLabel(new FuzzySet(
                HealthStatus.CriticallyWounded.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.health.almostDead, this.aiDefinitions.health.criticallyWounded, this.aiDefinitions.health.seriouslyWounded)
            ));
            varHealth.AddLabel(new FuzzySet(
                HealthStatus.SeriouslyWounded.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.health.criticallyWounded, this.aiDefinitions.health.seriouslyWounded, this.aiDefinitions.health.moderatelyWounded)
            ));
            varHealth.AddLabel(new FuzzySet(
                HealthStatus.ModeratelyWounded.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.health.seriouslyWounded, this.aiDefinitions.health.moderatelyWounded, this.aiDefinitions.health.lightlyWounded)
            ));
            varHealth.AddLabel(new FuzzySet(
                HealthStatus.LightlyWounded.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.health.moderatelyWounded, this.aiDefinitions.health.lightlyWounded, this.aiDefinitions.health.scratched)
            ));
            varHealth.AddLabel(new FuzzySet(
                HealthStatus.Scratched.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.health.lightlyWounded, this.aiDefinitions.health.scratched, this.aiDefinitions.health.healthy)
            ));
            if (this.aiDefinitions.health.healthy >= end)
            {
                varHealth.AddLabel(new FuzzySet(HealthStatus.Healthy.ToString(), new SingletonFunction(end)));
            }
            else
            {
                varHealth.AddLabel(new FuzzySet(
                    HealthStatus.Healthy.ToString(),
                    new TrapezoidalFunction(this.aiDefinitions.health.scratched, this.aiDefinitions.health.healthy, end, end + 1f)
                ));
            }

            return varHealth;
        }

        protected LinguisticVariable DefineOutputVariable(string name)
        {
            float start = 0f;
            float end = 1f;

            LinguisticVariable varOutput = new LinguisticVariable(name, start - 1f, end + 1f);
            varOutput.AddLabel(new FuzzySet(
                AIDesirability.TheWorstOption.ToString(),
                new TrapezoidalFunction(start - 1f, start, this.aiDefinitions.desirability.theWorstOption, this.aiDefinitions.desirability.veryUndesirable)
            ));
            varOutput.AddLabel(new FuzzySet(
                AIDesirability.VeryUndesirable.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.desirability.theWorstOption, this.aiDefinitions.desirability.veryUndesirable, this.aiDefinitions.desirability.undesirable)
            ));
            varOutput.AddLabel(new FuzzySet(
                AIDesirability.Undesirable.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.desirability.veryUndesirable, this.aiDefinitions.desirability.undesirable, this.aiDefinitions.desirability.notBad)
            ));
            varOutput.AddLabel(new FuzzySet(
                AIDesirability.NotBad.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.desirability.undesirable, this.aiDefinitions.desirability.notBad, this.aiDefinitions.desirability.desirable)
            ));
            varOutput.AddLabel(new FuzzySet(
                AIDesirability.Desirable.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.desirability.notBad, this.aiDefinitions.desirability.desirable, this.aiDefinitions.desirability.veryDesirable)
            ));
            varOutput.AddLabel(new FuzzySet(
                AIDesirability.VeryDesirable.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.desirability.desirable, this.aiDefinitions.desirability.veryDesirable, this.aiDefinitions.desirability.theBestOption)
            ));
            varOutput.AddLabel(new FuzzySet(
                AIDesirability.TheBestOption.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.desirability.veryDesirable, this.aiDefinitions.desirability.theBestOption, end, end + 1f)
            ));

            return varOutput;
        }

        protected LinguisticVariable DefineMovementSpeedVariable(string name, float start = 0f, float end = 1000f)
        {
            LinguisticVariable varMovementSpeed = new LinguisticVariable(name, start - 1f, end + 1f);
            varMovementSpeed.AddLabel(new FuzzySet(
                AIMovementSpeed.VerySlow.ToString(),
                new TrapezoidalFunction(start - 1f, start, this.aiDefinitions.speed.verySlow, this.aiDefinitions.speed.slow)
            ));
            varMovementSpeed.AddLabel(new FuzzySet(
                AIMovementSpeed.Slow.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.speed.verySlow, this.aiDefinitions.speed.slow, this.aiDefinitions.speed.normal)
            ));
            varMovementSpeed.AddLabel(new FuzzySet(
                AIMovementSpeed.Normal.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.speed.slow, this.aiDefinitions.speed.normal, this.aiDefinitions.speed.fast)
            ));
            varMovementSpeed.AddLabel(new FuzzySet(
                AIMovementSpeed.Fast.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.speed.normal, this.aiDefinitions.speed.fast, this.aiDefinitions.speed.veryFast)
            ));
            varMovementSpeed.AddLabel(new FuzzySet(
                AIMovementSpeed.VeryFast.ToString(),
                new TrapezoidalFunction(this.aiDefinitions.speed.fast, this.aiDefinitions.speed.veryFast, end, end + 1f)
            ));

            return varMovementSpeed;
        }
    }
}