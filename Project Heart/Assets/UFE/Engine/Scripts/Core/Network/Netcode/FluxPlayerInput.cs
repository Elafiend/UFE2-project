namespace UFE3D
{
	public class FluxPlayerInput
	{
		#region public instance properties
		public FrameInput? PredictedInput;
		public FrameInput? ConfirmedInput;
		#endregion

		#region public instance constructors
		public FluxPlayerInput() : this(null, null) { }

		public FluxPlayerInput(FrameInput? predictedInput) : this(predictedInput, null) { }

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Initializes a new instance of the <see cref="BUM.Runtime.GameEngine.PlayerInput"/> class.
		/// </summary>
		/// <param name="predictedInput">Predicted input.</param>
		/// <param name="confirmedInput">Confirmed input.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public FluxPlayerInput(FrameInput? predictedInput, FrameInput? confirmedInput)
		{
			this.PredictedInput = predictedInput;
			this.ConfirmedInput = confirmedInput;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Initializes a new instance of the <see cref="BUM.InputSystem.PlayerInput"/> class.
		/// </summary>
		/// <param name="source">Source.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public FluxPlayerInput(FluxPlayerInput source) : this(source.PredictedInput, source.ConfirmedInput) { }
		#endregion

		#region public instance methods
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Determines whether the predicted and confirmed values are equal.
		/// </summary>
		/// <returns><c>true</c> if the input values are equal; otherwise, <c>false</c>.</returns>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool ArePredictedAndConfirmedInputsEqual()
		{
			return
				this.PredictedInput == null &&
				this.ConfirmedInput == null
				||
				this.PredictedInput != null &&
				this.ConfirmedInput != null &&
				this.PredictedInput.Value.Equals(this.ConfirmedInput.Value);
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Confirms the predicted input.
		/// </summary>
		/// <returns><c>true</c>, if input as confirmed was marked, <c>false</c> otherwise.</returns>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool ConfirmPredictedInput()
		{
			if (this.PredictedInput != null)
			{
				this.ConfirmedInput = this.PredictedInput;
				return true;
			}
			return false;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// This method returns the confirmed input if it's defined; otherwise, it returns the predicted input.
		/// </summary>
		/// <returns>The input.</returns>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public FrameInput? GetInput()
		{
			return this.ConfirmedInput ?? this.PredictedInput;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Determines whether the input has been confirmed by the player.
		/// </summary>
		/// <returns><c>true</c> if the input has been confirmed by the player; otherwise, <c>false</c>.</returns>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool IsInputConfirmed()
		{
			return this.ConfirmedInput != null;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Determines whether the input has been predicted by the system.
		/// </summary>
		/// <returns><c>true</c> if the input has been predicted by the system; otherwise, <c>false</c>.</returns>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool IsInputPredicted()
		{
			return this.PredictedInput != null;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Determines whether this instance is ready because the player have at least a predicted or a confirmed input.
		/// </summary>
		/// <returns><c>true</c> if this instance is ready; otherwise, <c>false</c>.</returns>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool IsInputReady()
		{
			return this.GetInput() != null;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Overrides the predicted input with the confirmed input.
		/// </summary>
		/// <returns><c>true</c>, if input as confirmed was marked, <c>false</c> otherwise.</returns>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool OverridePredictionWithConfirmedInput()
		{
			if (this.ConfirmedInput != null)
			{
				this.PredictedInput = this.ConfirmedInput;
				return true;
			}
			return false;
		}
		#endregion
	}
}