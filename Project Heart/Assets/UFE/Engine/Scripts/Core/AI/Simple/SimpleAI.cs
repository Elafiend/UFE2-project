using UnityEngine;
using System;
using System.Collections.Generic;

namespace UFE3D
{
	public class SimpleAI : AbstractInputController
	{
		#region public instance fields
		public SimpleAIBehaviour behaviour;
		#endregion

		#region protected instance fields: Cached information to improve performance
		protected ButtonPress[] noButtonsPressed = new ButtonPress[0];
		protected List<Dictionary<InputReferences, InputEvents>> inputBuffer;
		#endregion

		#region overriden methods
		public override void Initialize(IEnumerable<InputReferences> inputs)
		{
			//		UFE.OnRoundBegins -= OnRoundBegins;
			//		UFE.OnRoundBegins += OnRoundBegins;

			//-------------------------------------------------
			// We need at least a buffer of 2 positions:
			// + buffer[0] -------> previous Input
			// + buffer[1] -------> current Input
			// + buffer[i > 1] ---> future Inputs 
			//-------------------------------------------------
			int bufferSize = 2;

			this.inputBuffer = new List<Dictionary<InputReferences, InputEvents>>();
			for (int i = 0; i < bufferSize; ++i)
			{
				this.inputBuffer.Add(new Dictionary<InputReferences, InputEvents>());
			}

			if (inputs != null)
			{
				foreach (InputReferences input in inputs)
				{
					if (input != null)
					{
						for (int i = 0; i < bufferSize; ++i)
						{
							this.inputBuffer[i][input] = InputEvents.Default;
						}
					}
				}
			}

			base.Initialize(inputs);
		}

		public override void DoFixedUpdate()
		{
			//this.ShowDebugInformation();


			ControlsScript self = UFE.GetControlsScript(this.player);
			if (this.inputReferences != null && this.inputBuffer != null && self != null)
			{
				ControlsScript opponent = self.opControlsScript;
				if (opponent != null)
				{
					//-------------------------------------------------------------------------------------------------
					// Check the information stored in the input buffer...
					//-------------------------------------------------------------------------------------------------
					if (this.inputBuffer.Count == 0)
					{
						//---------------------------------------------------------------------------------------------
						// If the we don't have the input of the previous frame, use the default input...
						//---------------------------------------------------------------------------------------------
						Dictionary<InputReferences, InputEvents> frame = new Dictionary<InputReferences, InputEvents>();
						foreach (InputReferences input in this.inputReferences)
						{
							frame[input] = InputEvents.Default;
						}
						this.inputBuffer.Add(frame);
					}
					else if (this.inputBuffer.Count >= 2)
					{
						this.inputBuffer.RemoveAt(0);
					}

					//-----------------------------------------------------------------------------------------------------
					// If we haven't decided the input for the current frame yet...
					//-----------------------------------------------------------------------------------------------------
					if (this.inputBuffer.Count < 2)
					{
						//-------------------------------------------------------------------------------------------------
						// And simulate the input required for executing the next movement
						//-------------------------------------------------------------------------------------------------
						if (
							this.behaviour != null
							&&
							this.behaviour.steps.Length > 0
							&&
							self.currentMove == null
							&&
							(
								self.currentBasicMove == BasicMoveReference.Idle ||
								self.currentBasicMove == BasicMoveReference.Crouching
							)
						)
						{
							float sign = Mathf.Sign(opponent.transform.position.x - self.transform.position.x);

							foreach (SimpleAIStep step in this.behaviour.steps)
							{
								Dictionary<InputReferences, InputEvents> frame = new Dictionary<InputReferences, InputEvents>();
								foreach (InputReferences input in this.inputReferences)
								{
									frame[input] = InputEvents.Default;
								}

								foreach (InputReferences input in this.inputReferences)
								{
									if (input.inputType == InputType.HorizontalAxis)
									{
										foreach (ButtonPress buttonPress in step.buttons)
										{
											if (buttonPress == ButtonPress.Back)
											{
												frame[input] = new InputEvents(-1f * sign);
											}
											else if (buttonPress == ButtonPress.Forward)
											{
												frame[input] = new InputEvents(1f * sign);
											}
										}
									}
									else if (input.inputType == InputType.VerticalAxis)
									{
										foreach (ButtonPress buttonPress in step.buttons)
										{
											if (buttonPress == ButtonPress.Up)
											{
												frame[input] = new InputEvents(1f);
											}
											else if (buttonPress == ButtonPress.Down)
											{
												frame[input] = new InputEvents(-1f);
											}
										}
									}
									else
									{
										foreach (ButtonPress buttonPress in step.buttons)
										{
											if (input.engineRelatedButton == buttonPress)
											{
												frame[input] = new InputEvents(true);
											}
										}
									}
								}

								for (int i = 0; i < step.frames; ++i)
								{
									this.inputBuffer.Add(frame);
								}
							}
						}
						else
						{
							Dictionary<InputReferences, InputEvents> frame = new Dictionary<InputReferences, InputEvents>();
							foreach (InputReferences input in this.inputReferences)
							{
								frame[input] = InputEvents.Default;
							}
							this.inputBuffer.Add(frame);
						}
					}
				}
			}
		}

		public override void DoUpdate() { }

		public override InputEvents ReadInput(InputReferences inputReference)
		{
			if (
				this.behaviour != null &&
				this.inputReferences != null &&
				this.inputBuffer != null &&
				this.inputBuffer.Count >= 2
			)
			{
				return this.inputs[inputReference];
			}
			return InputEvents.Default;
		}
		#endregion

		#region protected instance methods

		#endregion
	}
}