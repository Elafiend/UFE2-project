using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UFE3D
{
	public class FluxPlayerInputBuffer
	{
		#region public instance properties
		public long Count
		{
			get
			{
				return (long)this._buffer.Count;
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The frame of the input in the first position of the buffer.
		/// </summary>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public long FirstFrame { get; set; }

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The frame of the input in the last position of the buffer.
		/// </summary>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public long LastFrame
		{
			get
			{
				return this.FirstFrame + this.Count - 1;
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The max size of the buffer. A value equals to or lesser than zero means that there is no limit.
		/// </summary>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public int MaxBufferSize { get; private set; }

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the <see cref="FluxPlayerInputBuffer"/> at the specified index.
		/// </summary>
		/// <param name="index">Index.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public FluxPlayerInput this[int index]
		{
			get
			{
				return this._buffer[index];
			}
		}
		#endregion

		#region private instance fields
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The buffer with the player inputs during a few frames.
		/// </summary>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		private List<FluxPlayerInput> _buffer = new List<FluxPlayerInput>();
		#endregion

		#region public instance methods
		public bool ArePredictedAndConfirmedInputsEqual(long frame)
		{
			return this._buffer[this.GetIndex(frame)].ArePredictedAndConfirmedInputsEqual();
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the input buffer with all inputs that has been confirmed by the player.
		/// </summary>
		/// <returns>The input buffer.</returns>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public ReadOnlyCollection<Tuple<long, FrameInput>> GetConfirmedInputBuffer()
		{
			return this.GetConfirmedInputBuffer(0L);
		}

		public ReadOnlyCollection<Tuple<long, FrameInput>> GetConfirmedInputBuffer(long firstFrame)
		{
			List<Tuple<long, FrameInput>> buffer = new List<Tuple<long, FrameInput>>();

			for (int i = 0; i < this._buffer.Count; ++i)
			{
				long currentFrame = this.GetFrame(i);

				if (currentFrame >= firstFrame)
				{
					FrameInput? input = this._buffer[i].ConfirmedInput;

					if (input != null)
					{
						buffer.Add(new Tuple<long, FrameInput>(currentFrame, input.Value));
					}
					else
					{
						break;
					}
				}
			}

			return new ReadOnlyCollection<Tuple<long, FrameInput>>(buffer);
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the frame associated to the specified index... assuming that the index has a valid value.
		/// </summary>
		/// <returns>The frame.</returns>
		/// <param name="index">Index.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public long GetFrame(int index)
		{
			return (long)(index) + this.FirstFrame;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the index where the information of the specified frame will be stored in the input buffer...
		/// assuming that the returned index has a valid value.
		/// </summary>
		/// <returns>The index.</returns>
		/// <param name="frame">Frame.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public int GetIndex(long frame)
		{
			return (int)(frame - this.FirstFrame);
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the input buffer with all inputs that has been predicted by the system or confirmed by the player.
		/// </summary>
		/// <returns>The input buffer.</returns>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public ReadOnlyCollection<FrameInput> GetInputBuffer()
		{
			List<FrameInput> buffer = new List<FrameInput>();

			for (int i = 0; i < this._buffer.Count; ++i)
			{
				FrameInput? input = this._buffer[i].GetInput();

				if (input != null)
				{
					buffer.Add(input.Value);
				}
				else
				{
					break;
				}
			}

			return new ReadOnlyCollection<FrameInput>(buffer);
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the input buffer with all inputs that has been predicted by the system.
		/// </summary>
		/// <returns>The input buffer.</returns>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public ReadOnlyCollection<FrameInput> GetPredictedInputBuffer()
		{
			List<FrameInput> buffer = new List<FrameInput>();

			for (int i = 0; i < this._buffer.Count; ++i)
			{
				FrameInput? input = this._buffer[i].PredictedInput;

				if (input != null)
				{
					buffer.Add(input.Value);
				}
				else
				{
					break;
				}
			}

			return new ReadOnlyCollection<FrameInput>(buffer);
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Get the first frame where a rollback is required because the predicted input didn't match 
		/// the confirmed input or a negative value if all predicted inputs match the confirmed inputs.
		/// </summary>
		/// <returns>The first frame where the predicted input didn't match the confirmed input.</returns>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public long GetFirstFrameWhereRollbackIsRequired()
		{
			for (int i = 0; i < this._buffer.Count; ++i)
			{
				FluxPlayerInput input = this._buffer[i];

				if (input != null && input.ConfirmedInput != null && !input.ArePredictedAndConfirmedInputsEqual())
				{
					return this.GetFrame(i);
				}
			}
			return -1;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the frame of the last input confirmed by the user or a negative value if no input has been confirmed.
		/// </summary>
		/// <returns>The last confirmed frame.</returns>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public long GetLastFrameWithConfirmedInput()
		{
			//		for (int i = this._buffer.Count - 1; i >= 0; --i){
			//			if (this._buffer[i] != null && this._buffer[i].IsInputConfirmed()){
			//				return this.GetFrame(i);
			//			}
			//		}
			//		return -1L;


			// Return the first frame without a confirmed input
			for (int i = 0; i < this._buffer.Count; ++i)
			{
				//for (int i = this._buffer.Count - 1; i >= 0; --i){
				if (this._buffer[i] == null || !this._buffer[i].IsInputConfirmed())
				{
					return this.GetFrame(i) - 1L;
				}
			}

			return this.LastFrame;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the frame of the last input predicted by the system or a negative value if no input has been predicted.
		/// </summary>
		/// <returns>The last confirmed frame.</returns>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public long GetLastFrameWithPredictedInput()
		{
			//		for (int i = this._buffer.Count - 1; i >= 0; --i){
			//			if (this._buffer[i] != null && this._buffer[i].IsInputPredicted()){
			//				return this.GetFrame(i);
			//			}
			//		}
			//		return -1;


			// Return the first frame without a predicted input
			for (int i = 0; i < this._buffer.Count; ++i)
			{
				if (this._buffer[i] == null || !this._buffer[i].IsInputPredicted())
				{
					return this.GetFrame(i) - 1L;
				}
			}

			return this.LastFrame;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the frame of the last input confirmed by the user or a negative value if no input has been confirmed.
		/// </summary>
		/// <returns>The last confirmed frame.</returns>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public long GetLastFrameWithReadyInput()
		{
			//		for (int i = this._buffer.Count - 1; i >= 0; --i){
			//			if (this._buffer[i] != null && this._buffer[i].IsInputReady()){
			//				return this.GetFrame(i);
			//			}
			//		}
			//		return -1;

			// Return the first frame without a predicted input
			for (int i = 0; i < this._buffer.Count; ++i)
			{
				if (this._buffer[i] == null || !this._buffer[i].IsInputReady())
				{
					return this.GetFrame(i) - 1L;
				}
			}

			return this.LastFrame;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Initializes a new instance of the <see cref="BUM.Runtime.GameEngine.PlayerInputBuffer"/> class.
		/// </summary>
		/// <param name="maxBufferSize">Max buffer size.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public virtual void Initialize()
		{
			this.Initialize(0);
		}

		public virtual void Initialize(long firstFrame)
		{
			this.Initialize(firstFrame, -1);
		}

		public virtual void Initialize(long firstFrame, int maxBufferSize)
		{
			this.FirstFrame = firstFrame;
			this.MaxBufferSize = maxBufferSize > 0 ? maxBufferSize : -1;
			this._buffer.Clear();
		}

		public bool IsEmpty()
		{
			return this._buffer.Count == 0;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Determines whether the input buffer is full.
		/// </summary>
		/// <returns><c>true</c> if the input buffer is full; otherwise, <c>false</c>.</returns>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool IsFull()
		{
			return this.MaxBufferSize > 0 && this._buffer.Count == this.MaxBufferSize;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Remove the inputs associated to all existing frames until reaching the specified frame.
		/// </summary>
		/// <param name="frame">Frame.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool RemoveInputsUntilFrame(long frame)
		{
			// Check if we have already passed the specified frame...
			if (this.FirstFrame > frame)
			{
				return false;
			}

			// If we haven't reached the specified frame yet, remove the first frame of the buffer 
			// and add a new frame to the end of the buffer until we reach the specified frame...
			while (this.FirstFrame < frame)
			{
				this.RemoveNextInput();
			}

			return true;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Remove the next input of the buffer.
		/// </summary>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool RemoveNextInput()
		{
			if (this._buffer.Count > 0)
			{
				this._buffer.RemoveAt(0);
				++this.FirstFrame;

				return true;
			}
			return false;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Determines whether this instance is confirmed.
		/// </summary>
		/// <returns><c>true</c> if this instance is confirmed; otherwise, <c>false</c>.</returns>
		/// <param name="frame">Frame.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool TryCheckIfInputIsConfirmed(long frame, out bool isConfirmed)
		{
			int index = this.GetIndex(frame);

			if (index >= 0 && index < this._buffer.Count)
			{
				isConfirmed = this._buffer[this.GetIndex(frame)].IsInputConfirmed();
				return true;
			}

			isConfirmed = false;
			return false;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Determines whether this instance is predicted.
		/// </summary>
		/// <returns><c>true</c> if this instance is predicted; otherwise, <c>false</c>.</returns>
		/// <param name="frame">Frame.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool TryCheckIfInputIsPredicted(long frame, out bool isPredicted)
		{
			int index = this.GetIndex(frame);

			if (index >= 0 && index < this._buffer.Count)
			{
				isPredicted = this._buffer[this.GetIndex(frame)].IsInputPredicted();
				return true;
			}

			isPredicted = false;
			return false;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Determines whether this instance is ready because all players have at least a predicted or confirmed input.
		/// </summary>
		/// <returns><c>true</c> if this instance is ready; otherwise, <c>false</c>.</returns>
		/// <param name="frame">Frame.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool TryCheckIfInputIsReady(long frame, out bool isReady)
		{
			int index = this.GetIndex(frame);

			if (index >= 0 && index < this._buffer.Count)
			{
				isReady = this._buffer[this.GetIndex(frame)].IsInputReady();
				return true;
			}

			isReady = false;
			return false;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Try to confirm the predicted input.
		/// </summary>
		/// <returns>Whether the input could be marked as confirmed successfully.</returns>
		/// <param name="frame">Frame.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool TryConfirmPredictedInput(long frame)
		{
			int index = this.GetIndex(frame);

			if (index >= 0 && index < this._buffer.Count)
			{
				this._buffer[index].ConfirmPredictedInput();
				return true;
			}

			return false;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Try to get the input associated to the specified player in the specified frame, 
		/// regardless of if the input has been predicted by the system or confirmed by the user.
		/// </summary>
		/// <returns>Whether the input could be retrieved successfully.</returns>
		/// <param name="frame">Frame.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool TryGetInput(long frame, out FrameInput? input)
		{
			int index = this.GetIndex(frame);

			if (index >= 0 && index < this._buffer.Count)
			{
				input = this._buffer[index].GetInput();
				return true;
			}

			input = null;
			return false;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Try to override the predicted input with the confirmed input.
		/// </summary>
		/// <returns>Whether the input could be marked as confirmed successfully.</returns>
		/// <param name="frame">Frame.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool TryOverridePredictionWithConfirmedInput(long frame)
		{
			int index = this.GetIndex(frame);

			if (index >= 0 && index < this._buffer.Count)
			{
				this._buffer[index].OverridePredictionWithConfirmedInput();
				return true;
			}

			return false;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Try to set the input that has been confirmed by the player.
		/// </summary>
		/// <returns>Whether the input could be set successfully.</returns>
		/// <param name="frame">Frame.</param>
		/// <param name="playerInput">Player Input.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool TrySetConfirmedInput(long frame, FrameInput playerInput)
		{
			return this.TrySetConfirmedInput(frame, playerInput, false);
		}

		public bool TrySetConfirmedInput(long frame, FrameInput playerInput, bool overridePrediction)
		{
			int index = this.GetIndex(frame);

			// If the index is greater than or equal to zero...
			if (index >= 0)
			{
				// Check if we need to make room in the buffer for the new input...
				while (index >= this._buffer.Count && !this.IsFull())
				{
					this._buffer.Add(new FluxPlayerInput());
				}

				if (index < this._buffer.Count)
				{
					// And add the new confirmed input to the buffer
					this._buffer[index].ConfirmedInput = playerInput;

					if (overridePrediction)
					{
						this._buffer[index].PredictedInput = playerInput;
					}

					return true;
				}
			}
			return false;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Try to set the input that has been predicted by the system.
		/// </summary>
		/// <returns>Whether the input could be set successfully.</returns>
		/// <param name="playerInput">Player Input.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool TrySetPredictedInput(long frame, FrameInput playerInput)
		{
			int index = this.GetIndex(frame);

			// If the index is greater than or equal to zero...
			if (index >= 0)
			{
				// Check if we need to make room in the buffer for the new input...
				while (index >= this._buffer.Count && !this.IsFull())
				{
					this._buffer.Add(new FluxPlayerInput());
				}

				if (index < this._buffer.Count)
				{
					// And add the new predicted input to the buffer
					this._buffer[index].PredictedInput = playerInput;
					return true;
				}
			}
			return false;
		}
		#endregion
	}
}