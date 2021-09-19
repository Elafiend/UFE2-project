using System.Collections.Generic;

namespace UFE3D
{
	public class UFEController : AbstractInputController
	{
		#region public instance fields
		public bool isCPU = false;

		public AbstractInputController cpuController
		{
			get
			{
				return this._cpuController;
			}
			set
			{
				this._cpuController = value;

				if (this._cpuController != null && this.inputReferences != null)
				{
					this._cpuController.Initialize(this.inputReferences);
				}
			}
		}

		public AbstractInputController humanController
		{
			get
			{
				return this._humanController;
			}
			set
			{
				this._humanController = value;

				if (this._humanController != null && this.inputReferences != null)
				{
					this._humanController.Initialize(this.inputReferences);
				}
			}
		}

		public override int player
		{
			get { return base.player; }
			set
			{
				base.player = value;

				if (this._humanController != null)
				{
					this.humanController.player = value;
				}

				if (this.cpuController)
				{
					this.cpuController.player = value;
				}
			}
		}
		#endregion

		#region protected instance fields
		protected AbstractInputController _humanController;
		protected AbstractInputController _cpuController;
		#endregion

		#region override methods
		public override void Initialize(IEnumerable<InputReferences> inputs)
		{
			base.Initialize(inputs);
			if (this.cpuController != null)
			{
				this.cpuController.Initialize(inputs);
			}
			if (this.humanController != null)
			{
				this.humanController.Initialize(inputs);
			}
		}

		public override InputEvents ReadInput(InputReferences inputReference)
		{
			return InputEvents.Default;
		}

		public virtual void PressButton(ButtonPress button, InputType inputType = InputType.Button, int axisValue = 0)
		{
			foreach (InputReferences inputReference in this.inputReferences)
			{
				if (!this.isCPU && inputReference.inputType == inputType && inputReference.engineRelatedButton == button)
				{
					if (inputType == InputType.Button)
						this.inputs[inputReference] = new InputEvents(true);
					else
						this.inputs[inputReference] = new InputEvents(axisValue);
				}
			}
		}

		public override void DoFixedUpdate()
		{
			if (this.inputReferences != null)
			{
				//---------------------------------------------------------------------------------------------------------
				// The CPU Controller is only updated when the character is controlled by the CPU...
				//---------------------------------------------------------------------------------------------------------
				if (this.cpuController != null && this.isCPU && UFE.gameRunning && !UFE.isPaused())
				{
					this.cpuController.DoFixedUpdate();
				}

				//---------------------------------------------------------------------------------------------------------
				// But the player controller is always updated because we want to know if the player pressed the "Start"
				// button even if the character is being controlled by the CPU
				//---------------------------------------------------------------------------------------------------------
				if (this.humanController != null)
				{
					this.humanController.DoFixedUpdate();
				}

				//---------------------------------------------------------------------------------------------------------
				// After that, we update every input reference stored in this class.
				//---------------------------------------------------------------------------------------------------------
				foreach (InputReferences inputReference in this.inputReferences)
				{
					if (this.UseHumanController(inputReference))
					{
						this.inputs[inputReference] = this.humanController.inputs[inputReference];
					}
					else if (this.cpuController != null)
					{
						this.inputs[inputReference] = this.cpuController.inputs[inputReference];
					}
					else
					{
						this.inputs[inputReference] = InputEvents.Default;
					}
				}
			}
		}

		public override void DoUpdate()
		{
			//---------------------------------------------------------------------------------------------------------
			// The CPU Controller is only updated when the character is controlled by the CPU...
			//---------------------------------------------------------------------------------------------------------
			if (this.cpuController != null && this.isCPU && UFE.gameRunning && !UFE.isPaused())
			{
				this.cpuController.DoUpdate();
			}

			//---------------------------------------------------------------------------------------------------------
			// But the player controller is always updated because we want to know if the player pressed the "Start"
			// button even if the character is being controlled by the CPU
			//---------------------------------------------------------------------------------------------------------
			if (this.humanController != null)
			{
				this.humanController.DoUpdate();
			}
		}
		#endregion

		#region protected instance methods
		protected bool UseHumanController(InputReferences inputReference)
		{
			return
				this.humanController != null &&
				(
					!(this.isCPU && UFE.gameRunning)
					||
					UFE.isPaused()
					||
					// Even if the character is being controlled by the CPU,
					// we want to listen "Start" button events from the player controller
					inputReference.inputType == InputType.Button && inputReference.engineRelatedButton == ButtonPress.Start
				);
		}
		#endregion
	}
}