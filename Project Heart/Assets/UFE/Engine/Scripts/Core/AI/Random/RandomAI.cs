using UnityEngine;
using System.Collections.Generic;

namespace UFE3D
{
	public class RandomAI : AbstractInputController
	{
		#region protected instance fields
		protected float timeLastDecision = float.NegativeInfinity;
		#endregion

		#region public override methods
		public override void Initialize(IEnumerable<InputReferences> inputs)
		{
			this.timeLastDecision = float.NegativeInfinity;
			base.Initialize(inputs);
		}

		public override void DoUpdate()
		{
			if (this.inputReferences != null)
			{
				//---------------------------------------------------------------------------------------------------------
				// Check the time that has passed since the last update.
				//---------------------------------------------------------------------------------------------------------
				float currentTime = Time.realtimeSinceStartup;

				if (this.timeLastDecision < 0f)
				{
					this.timeLastDecision = currentTime;
				}

				//---------------------------------------------------------------------------------------------------------
				// If the time since the last update is greater than the input frequency, read the AI input.
				// Otherwise, don't press any input.
				//---------------------------------------------------------------------------------------------------------
				this.inputs.Clear();
				if (currentTime - this.timeLastDecision >= UFE.config.aiOptions.inputFrequency)
				{
					this.timeLastDecision = currentTime;

					foreach (InputReferences input in this.inputReferences)
					{
						this.inputs[input] = this.ReadInput(input);
					}
				}
				else
				{
					foreach (InputReferences input in this.inputReferences)
					{
						this.inputs[input] = InputEvents.Default;
					}
				}
			}
		}

		public override InputEvents ReadInput(InputReferences inputReference)
		{
			ControlsScript self = UFE.GetControlsScript(this.player);
			if (self != null)
			{
				ControlsScript opponent = self.opControlsScript;

				if (opponent != null)
				{
					bool isOpponentDown = opponent.currentState == PossibleStates.Down;
					float dx = opponent.transform.position.x - self.transform.position.x;
					int distance = Mathf.RoundToInt(100f * Mathf.Clamp01((float)self.normalizedDistance));

					float maxDistance = float.NegativeInfinity;
					AIDistanceBehaviour behaviour = null;

					// Try to find the correct "Distance Behaviour"
					// If there are several overlapping "Distance Behaviour", we choose the first in the list.
					foreach (AIDistanceBehaviour thisBehaviour in UFE.config.aiOptions.distanceBehaviour)
					{
						if (thisBehaviour != null)
						{
							if (distance >= thisBehaviour.proximityRangeBegins && distance <= thisBehaviour.proximityRangeEnds)
							{
								behaviour = thisBehaviour;
								break;
							}

							if (thisBehaviour.proximityRangeEnds > maxDistance)
							{
								maxDistance = thisBehaviour.proximityRangeEnds;
							}
						}
					}

					// If we don't find the correct "Distance Behaviour", make our best effort...
					if (behaviour == null)
					{
						foreach (AIDistanceBehaviour thisBehaviour in UFE.config.aiOptions.distanceBehaviour)
						{
							if (thisBehaviour != null && thisBehaviour.proximityRangeEnds == maxDistance)
							{
								behaviour = thisBehaviour;
							}
						}
					}

					if (behaviour == null)
					{
						return InputEvents.Default;
					}
					else if (inputReference.inputType == InputType.HorizontalAxis)
					{
						float axis = 0f;
						if (UFE.config.aiOptions.moveWhenEnemyIsDown || !isOpponentDown)
						{
							axis =
								Mathf.Sign(dx)
								*
								(
									(Random.Range(0f, 1f) < behaviour.movingForwardProbability ? 1f : 0f) -
									(Random.Range(0f, 1f) < behaviour.movingBackProbability ? 1f : 0f)
								);
						}

						return new InputEvents(axis);
					}
					else if (inputReference.inputType == InputType.VerticalAxis)
					{
						float axis = 0f;
						if (UFE.config.aiOptions.moveWhenEnemyIsDown || !isOpponentDown)
						{
							axis =
								(Random.Range(0f, 1f) < behaviour.jumpingProbability ? 1f : 0f) -
								(Random.Range(0f, 1f) < behaviour.movingBackProbability ? 1f : 0f);
						}

						return new InputEvents(axis);
					}
					else
					{
						if (!UFE.config.aiOptions.attackWhenEnemyIsDown && isOpponentDown)
						{
							return InputEvents.Default;
						}
						else if (inputReference.engineRelatedButton == ButtonPress.Button1)
						{
							return new InputEvents(Random.Range(0f, 1f) < behaviour.attackProbability);
						}
						else if (inputReference.engineRelatedButton == ButtonPress.Button2)
						{
							return new InputEvents(Random.Range(0f, 1f) < behaviour.attackProbability);
						}
						else if (inputReference.engineRelatedButton == ButtonPress.Button3)
						{
							return new InputEvents(Random.Range(0f, 1f) < behaviour.attackProbability);
						}
						else if (inputReference.engineRelatedButton == ButtonPress.Button4)
						{
							return new InputEvents(Random.Range(0f, 1f) < behaviour.attackProbability);
						}
						else if (inputReference.engineRelatedButton == ButtonPress.Button5)
						{
							return new InputEvents(Random.Range(0f, 1f) < behaviour.attackProbability);
						}
						else if (inputReference.engineRelatedButton == ButtonPress.Button6)
						{
							return new InputEvents(Random.Range(0f, 1f) < behaviour.attackProbability);
						}
						else if (inputReference.engineRelatedButton == ButtonPress.Button7)
						{
							return new InputEvents(Random.Range(0f, 1f) < behaviour.attackProbability);
						}
						else if (inputReference.engineRelatedButton == ButtonPress.Button8)
						{
							return new InputEvents(Random.Range(0f, 1f) < behaviour.attackProbability);
						}
						else if (inputReference.engineRelatedButton == ButtonPress.Button9)
						{
							return new InputEvents(Random.Range(0f, 1f) < behaviour.attackProbability);
						}
						else if (inputReference.engineRelatedButton == ButtonPress.Button10)
						{
							return new InputEvents(Random.Range(0f, 1f) < behaviour.attackProbability);
						}
						else if (inputReference.engineRelatedButton == ButtonPress.Button11)
						{
							return new InputEvents(Random.Range(0f, 1f) < behaviour.attackProbability);
						}
						else if (inputReference.engineRelatedButton == ButtonPress.Button12)
						{
							return new InputEvents(Random.Range(0f, 1f) < behaviour.attackProbability);
						}
						else
						{
							return InputEvents.Default;
						}
					}
				}
			}
			return InputEvents.Default;
		}
		#endregion
	}
}