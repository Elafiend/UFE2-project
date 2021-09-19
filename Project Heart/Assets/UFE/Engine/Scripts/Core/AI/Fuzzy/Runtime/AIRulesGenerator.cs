using System;
using System.Collections.Generic;

namespace UFE3D
{
	[Serializable]
	public class AIRulesGenerator
	{
		public CharacterDistance preferableCombatDistance = CharacterDistance.Any;
		public AIDesirability attacksAtPreferableDistance = AIDesirability.VeryDesirable;
		public bool autoMove;
		public bool restOnLocation = true;
		public int moveFrequency = 4;
		public bool autoJump;
		public int jumpBackFrequency = 1;
		public int jumpStraightFrequency = 2;
		public int jumpForwardFrequency = 3;
		public bool autoBlock;
		public bool obeyHitType = true;
		public int standBlockAccuracy = 6;
		public int crouchBlockAccuracy = 6;
		public int jumpBlockAccuracy = 0;
		public bool autoAttack;
		public bool obeyPreferableDistances = false;
		public int attackFrequency = 5;
		public bool debugToggle;

		public AIDesirability GetDesirabilityValue(float value)
		{
			DesirabilityDefinitions desirability = new DesirabilityDefinitions();
			if (value >= desirability.theBestOption) return AIDesirability.TheBestOption;
			if (value >= desirability.veryDesirable) return AIDesirability.VeryDesirable;
			if (value >= desirability.desirable) return AIDesirability.Desirable;
			if (value >= desirability.notBad) return AIDesirability.NotBad;
			if (value >= desirability.undesirable) return AIDesirability.Undesirable;
			if (value >= desirability.veryUndesirable) return AIDesirability.VeryUndesirable;

			return AIDesirability.TheWorstOption;
		}

		public AIDesirability GetDesirabilityValue(int value)
		{
			if (value >= 6) return AIDesirability.TheBestOption;
			if (value == 5) return AIDesirability.VeryDesirable;
			if (value == 4) return AIDesirability.Desirable;
			if (value == 3) return AIDesirability.NotBad;
			if (value == 2) return AIDesirability.Undesirable;
			if (value == 1) return AIDesirability.VeryUndesirable;

			return AIDesirability.TheWorstOption;
		}

		public List<string> GenerateRules()
		{
			List<string> fuzzyRules = new List<string>();

			if (this.autoMove)
			{
				if (this.restOnLocation) fuzzyRules = addDistanceReaction(fuzzyRules, this.preferableCombatDistance, AIReaction.Idle, this.moveFrequency);
				fuzzyRules = addSystematicRules(fuzzyRules, this.preferableCombatDistance, AIReaction.MoveForward, 1, this.moveFrequency);
				fuzzyRules = addSystematicRules(fuzzyRules, this.preferableCombatDistance, AIReaction.MoveBackward, -1, this.moveFrequency);

				if (this.autoJump)
				{
					fuzzyRules = addDistanceReaction(fuzzyRules, this.preferableCombatDistance, AIReaction.JumpStraight, this.jumpStraightFrequency);
					fuzzyRules = addSystematicRules(fuzzyRules, this.preferableCombatDistance, AIReaction.JumpForward, 1, this.jumpForwardFrequency);
					fuzzyRules = addSystematicRules(fuzzyRules, this.preferableCombatDistance, AIReaction.JumpBackward, -1, this.jumpBackFrequency);
				}
			}

			if (this.autoJump && !this.autoMove)
			{
				if (this.jumpStraightFrequency > 0)
				{
					fuzzyRules.Add(
						AIRule.Rule_IF +
						AICondition.Health_Self +
						AIRule.Rule_IS +
						AIRule.Rule_NOT +
						HealthStatus.Dead +

						AIRule.Rule_THEN +
						AIReaction.JumpStraight +
						AIRule.Rule_IS +
						GetDesirabilityValue(this.jumpStraightFrequency)
						);
				}

				if (this.jumpBackFrequency > 0)
				{
					fuzzyRules.Add(
						AIRule.Rule_IF +
						AICondition.Health_Self +
						AIRule.Rule_IS +
						AIRule.Rule_NOT +
						HealthStatus.Dead +

						AIRule.Rule_THEN +
						AIReaction.JumpBackward +
						AIRule.Rule_IS +
						GetDesirabilityValue(this.jumpBackFrequency)
						);
				}

				if (this.jumpForwardFrequency > 0)
				{
					fuzzyRules.Add(
						AIRule.Rule_IF +
						AICondition.Health_Self +
						AIRule.Rule_IS +
						AIRule.Rule_NOT +
						HealthStatus.Dead +

						AIRule.Rule_THEN +
						AIReaction.JumpForward +
						AIRule.Rule_IS +
						GetDesirabilityValue(this.jumpForwardFrequency)
						);
				}
			}

			if (this.autoAttack)
			{
				if (this.obeyPreferableDistances)
				{
					fuzzyRules = addSystematicRules(fuzzyRules, CharacterDistance.VeryClose, AIReaction.PlayMove_PreferableDistance_VeryClose, 1, this.attackFrequency, true);
					fuzzyRules = addSystematicRules(fuzzyRules, CharacterDistance.Close, AIReaction.PlayMove_PreferableDistance_Close, 1, this.attackFrequency, true);
					fuzzyRules = addSystematicRules(fuzzyRules, CharacterDistance.Mid, AIReaction.PlayMove_PreferableDistance_Mid, 1, this.attackFrequency, true);
					fuzzyRules = addSystematicRules(fuzzyRules, CharacterDistance.Far, AIReaction.PlayMove_PreferableDistance_Far, 1, this.attackFrequency, true);
					fuzzyRules = addSystematicRules(fuzzyRules, CharacterDistance.VeryFar, AIReaction.PlayMove_PreferableDistance_VeryFar, 1, this.attackFrequency, true);

					/*fuzzyRules = addDistanceReaction(fuzzyRules, CharacterDistance.VeryClose, AIReaction.PlayMove_PreferableDistance_VeryClose, this.attackFrequency);
					fuzzyRules = addDistanceReaction(fuzzyRules, CharacterDistance.Close, AIReaction.PlayMove_PreferableDistance_Close, this.attackFrequency);
					fuzzyRules = addDistanceReaction(fuzzyRules, CharacterDistance.Mid, AIReaction.PlayMove_PreferableDistance_Mid, this.attackFrequency);
					fuzzyRules = addDistanceReaction(fuzzyRules, CharacterDistance.Far, AIReaction.PlayMove_PreferableDistance_Far, this.attackFrequency);
					fuzzyRules = addDistanceReaction(fuzzyRules, CharacterDistance.VeryFar, AIReaction.PlayMove_PreferableDistance_VeryFar, this.attackFrequency);
					fuzzyRules = addDistanceReaction(fuzzyRules, CharacterDistance.Any, AIReaction.PlayMove_PreferableDistance, this.aggressiveness);*/

				}
				else
				{
					if (this.autoMove)
					{
						//fuzzyRules = addDistanceReaction(fuzzyRules, this.preferableCombatDistance, AIReaction.PlayMove_RandomAttack, this.attackFrequency);
						fuzzyRules = addSystematicRules(fuzzyRules, this.preferableCombatDistance, AIReaction.PlayMove_RandomAttack, 1, this.attackFrequency, true);

					}
					else
					{
						fuzzyRules.Add(
							AIRule.Rule_IF +
							AICondition.Health_Self +
							AIRule.Rule_IS +
							AIRule.Rule_NOT +
							HealthStatus.Dead +

							AIRule.Rule_THEN +
							AIReaction.PlayMove_RandomAttack +
							AIRule.Rule_IS +
							GetDesirabilityValue(this.attackFrequency)
							);
					}
				}
			}

			if (this.autoBlock)
			{
				fuzzyRules = addBlockReaction(fuzzyRules, CurrentFrameData.StartupFrames, CurrentFrameData.ActiveFrames,
											  AIReaction.StandBlock, this.standBlockAccuracy, this.obeyHitType);
				fuzzyRules = addBlockReaction(fuzzyRules, CurrentFrameData.StartupFrames, CurrentFrameData.ActiveFrames,
											  AIReaction.CrouchBlock, this.crouchBlockAccuracy, this.obeyHitType);

				fuzzyRules = addBlockReaction(fuzzyRules, CurrentFrameData.RecoveryFrames, AIReaction.StandBlock, 0);
				fuzzyRules = addBlockReaction(fuzzyRules, CurrentFrameData.RecoveryFrames, AIReaction.CrouchBlock, 0);

				/*fuzzyRules.Add(
					AIRule.Rule_IF								+ 
					AICondition.Attacking_Opponent				+ 
					AIRule.Rule_IS								+ 
					AIBoolean.TRUE								+

					AIRule.Rule_THEN							+ 
					AIReaction.StandBlock						+ 
					AIRule.Rule_IS								+
					GetDesirabilityValue(this.standBlockAccuracy)
					);

				fuzzyRules.Add(
					AIRule.Rule_IF								+ 
					AICondition.Attacking_Opponent				+ 
					AIRule.Rule_IS								+ 
					AIBoolean.TRUE								+

					AIRule.Rule_THEN							+ 
					AIReaction.CrouchBlock						+ 
					AIRule.Rule_IS								+
					GetDesirabilityValue(this.crouchBlockAccuracy)
					);

				fuzzyRules.Add(
					AIRule.Rule_IF								+ 
					AICondition.Attacking_Opponent				+ 
					AIRule.Rule_IS								+ 
					AIBoolean.TRUE								+

					AIRule.Rule_THEN							+ 
					AIReaction.JumpBlock						+ 
					AIRule.Rule_IS								+
					GetDesirabilityValue(this.jumpBlockAccuracy)
					);*/
			}

			return fuzzyRules;
		}

		private List<string> addSystematicRules(List<string> fuzzyRules, CharacterDistance preferableDistance, string reaction, int multiplier, int frequencyVariant)
		{
			return addSystematicRules(fuzzyRules, preferableDistance, reaction, multiplier, frequencyVariant, false);
		}

		private List<string> addSystematicRules(List<string> fuzzyRules, CharacterDistance preferableDistance, string reaction, int multiplier, int frequencyVariant, bool parabola)
		{
			if (frequencyVariant == 0) return fuzzyRules;
			// predefined values for preferableDistance == CharacterDistance.VeryClose
			int veryCloseVariant = 0;
			int closeVariant = 0;
			int midVariant = 0;
			int farVariant = 0;
			int veryFarVariant = 0;
			int parabolaVariant = parabola ? -1 : 1;

			if (preferableDistance == CharacterDistance.VeryClose)
			{
				veryCloseVariant = parabola ? frequencyVariant : frequencyVariant - 3;
				closeVariant = frequencyVariant + (1 * multiplier * parabolaVariant);
				midVariant = frequencyVariant + (2 * multiplier * parabolaVariant);
				farVariant = frequencyVariant + (3 * multiplier * parabolaVariant);
				veryFarVariant = frequencyVariant + (5 * multiplier * parabolaVariant);

			}
			else if (preferableDistance == CharacterDistance.Close)
			{
				veryCloseVariant = frequencyVariant - (1 * multiplier);
				closeVariant = parabola ? frequencyVariant : frequencyVariant - 3;
				midVariant = frequencyVariant + (1 * multiplier * parabolaVariant);
				farVariant = frequencyVariant + (2 * multiplier * parabolaVariant);
				veryFarVariant = frequencyVariant + (3 * multiplier * parabolaVariant);

			}
			else if (preferableDistance == CharacterDistance.Mid)
			{
				veryCloseVariant = frequencyVariant - (2 * multiplier);
				closeVariant = frequencyVariant - (1 * multiplier);
				midVariant = parabola ? frequencyVariant : frequencyVariant - 3;
				farVariant = frequencyVariant + (1 * multiplier * parabolaVariant);
				veryFarVariant = frequencyVariant + (2 * multiplier * parabolaVariant);

			}
			else if (preferableDistance == CharacterDistance.Far)
			{
				veryCloseVariant = frequencyVariant - (3 * multiplier);
				closeVariant = frequencyVariant - (2 * multiplier);
				midVariant = frequencyVariant - (1 * multiplier);
				farVariant = parabola ? frequencyVariant : frequencyVariant - 3;
				veryFarVariant = frequencyVariant + (1 * multiplier * parabolaVariant);

			}
			else if (preferableDistance == CharacterDistance.VeryFar)
			{
				veryCloseVariant = frequencyVariant - (5 * multiplier);
				closeVariant = frequencyVariant - (3 * multiplier);
				midVariant = frequencyVariant - (2 * multiplier);
				farVariant = frequencyVariant - (1 * multiplier);
				veryFarVariant = parabola ? frequencyVariant : frequencyVariant - 3;
			}

			//---------------------------------------------------------------------------------------------------------
			// Add Conditions:
			//---------------------------------------------------------------------------------------------------------
			//if (parabola || preferableDistance != CharacterDistance.VeryClose){
			fuzzyRules.Add(
				AIRule.Rule_IF +
				AICondition.Distance_Self +
				AIRule.Rule_IS +
				CharacterDistance.VeryClose +

				AIRule.Rule_THEN +
				reaction +
				AIRule.Rule_IS +
				GetDesirabilityValue(veryCloseVariant)
				);
			//}

			//if (parabola || preferableDistance != CharacterDistance.Close){
			fuzzyRules.Add(
				AIRule.Rule_IF +
				AICondition.Distance_Self +
				AIRule.Rule_IS +
				CharacterDistance.Close +

				AIRule.Rule_THEN +
				reaction +
				AIRule.Rule_IS +
				GetDesirabilityValue(closeVariant)
				);
			//}

			//if (parabola || preferableDistance != CharacterDistance.Mid){
			fuzzyRules.Add(
				AIRule.Rule_IF +
				AICondition.Distance_Self +
				AIRule.Rule_IS +
				CharacterDistance.Mid +

				AIRule.Rule_THEN +
				reaction +
				AIRule.Rule_IS +
				GetDesirabilityValue(midVariant)
				);
			//}

			//if (parabola || preferableDistance != CharacterDistance.Far){
			fuzzyRules.Add(
				AIRule.Rule_IF +
				AICondition.Distance_Self +
				AIRule.Rule_IS +
				CharacterDistance.Far +

				AIRule.Rule_THEN +
				reaction +
				AIRule.Rule_IS +
				GetDesirabilityValue(farVariant)
				);
			//}

			//if (parabola || preferableDistance != CharacterDistance.VeryFar){
			fuzzyRules.Add(
				AIRule.Rule_IF +
				AICondition.Distance_Self +
				AIRule.Rule_IS +
				CharacterDistance.VeryFar +

				AIRule.Rule_THEN +
				reaction +
				AIRule.Rule_IS +
				GetDesirabilityValue(veryFarVariant)
				);
			//}

			return fuzzyRules;
		}

		private List<string> addBlockReaction(List<string> fuzzyRules, CurrentFrameData frameData1, string reaction, int frequency)
		{
			fuzzyRules.Add(
				AIRule.Rule_IF +
				AICondition.Attacking_Opponent +
				AIRule.Rule_IS +
				AIBoolean.TRUE +
				AIRule.Rule_AND +
				AICondition.Attacking_FrameData_Opponent +
				AIRule.Rule_IS +
				frameData1 +

				AIRule.Rule_THEN +
				reaction +
				AIRule.Rule_IS +
				GetDesirabilityValue(frequency)
				);

			return fuzzyRules;
		}

		private List<string> addBlockReaction(List<string> fuzzyRules, CurrentFrameData frameData1, CurrentFrameData frameData2, string reaction, int frequency, bool obeyHitType)
		{
			string hitTypeString = "";
			if (obeyHitType)
			{
				if (reaction == AIReaction.StandBlock)
				{
					hitTypeString = AIRule.Rule_AND;
					hitTypeString += AIRule.Rule_Open_Parenthesis;
					hitTypeString += AICondition.Attacking_HitType_Opponent + AIRule.Rule_IS + AIRule.Rule_NOT + HitType.Low;
					hitTypeString += AIRule.Rule_AND + AICondition.Attacking_HitType_Opponent + AIRule.Rule_IS + AIRule.Rule_NOT + HitType.Sweep;
					hitTypeString += AIRule.Rule_Close_Parenthesis;
				}
				else if (reaction == AIReaction.CrouchBlock)
				{
					hitTypeString = AIRule.Rule_AND;
					hitTypeString += AIRule.Rule_Open_Parenthesis;
					hitTypeString += AICondition.Attacking_HitType_Opponent + AIRule.Rule_IS + AIRule.Rule_NOT + HitType.Overhead;
					hitTypeString += AIRule.Rule_AND + AICondition.Attacking_HitType_Opponent + AIRule.Rule_IS + AIRule.Rule_NOT + HitType.HighKnockdown;
					hitTypeString += AIRule.Rule_Close_Parenthesis;
				}
			}
			fuzzyRules.Add(
				AIRule.Rule_IF +
				AICondition.Attacking_Opponent +
				AIRule.Rule_IS +
				AIBoolean.TRUE +
				hitTypeString +
				AIRule.Rule_AND +
				AIRule.Rule_Open_Parenthesis +
				AICondition.Attacking_FrameData_Opponent +
				AIRule.Rule_IS +
				frameData1 +
				AIRule.Rule_OR +
				AICondition.Attacking_FrameData_Opponent +
				AIRule.Rule_IS +
				frameData2 +
				AIRule.Rule_Close_Parenthesis +

				AIRule.Rule_THEN +
				reaction +
				AIRule.Rule_IS +
				GetDesirabilityValue(frequency)
				);

			return fuzzyRules;
		}

		private List<string> addDistanceReaction(List<string> fuzzyRules, CharacterDistance distance, string reaction, int frequency)
		{
			fuzzyRules.Add(
				AIRule.Rule_IF +
				AICondition.Distance_Self +
				AIRule.Rule_IS +
				distance +

				AIRule.Rule_THEN +
				reaction +
				AIRule.Rule_IS +
				GetDesirabilityValue(frequency)
				);

			return fuzzyRules;
		}

		public List<string> ToDebugInformation()
		{
			List<string> debugInformation = new List<string>();
			List<string> rules = this.GenerateRules();

			if (rules != null && rules.Count > 0)
			{
				foreach (string rule in rules)
				{
					if (!string.IsNullOrEmpty(rule))
					{
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
	}
}