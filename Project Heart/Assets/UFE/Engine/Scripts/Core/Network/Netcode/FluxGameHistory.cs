using System.Collections.Generic;

namespace UFE3D
{
	public class FluxGameHistory
	{
		#region public instance properties
		public long Count
		{
			get
			{
				return this._history.Count;
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The frame of the first position of the buffer.
		/// </summary>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public long FirstStoredFrame { get; private set; }

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the last stored frame.
		/// </summary>
		/// <value>The last stored frame.</value>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public long LastStoredFrame
		{
			get
			{
				return this.FirstStoredFrame + this._history.Count - 1;
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// If this property contains a positive value, it will be the max size of the history. 
		/// If it contains a number lesser than or equal to zero, it means the history buffer doesn't have any limit.
		/// </summary>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public long MaxBufferSize { get; private set; }
		#endregion

		#region protected instance 
		protected List<KeyValuePair<FluxStates, FluxFrameInput>> _history = new List<KeyValuePair<FluxStates, FluxFrameInput>>();
		#endregion

		#region public instance methods
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Initializes a new instance of the <see cref="BUM.Runtime.GameEngine.GameHistory"/> class.
		/// </summary>
		/// <param name="firstFrame">First frame.</param>
		/// <param name="bufferSize">Max buffer size.</param>
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
			this.FirstStoredFrame = firstFrame;
			this.MaxBufferSize = maxBufferSize;

			// Reserve space in the history buffer 
			this._history.Clear();
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Determines whether the input buffer is empty.
		/// </summary>
		/// <returns><c>true</c> if the input buffer is empty; otherwise, <c>false</c>.</returns>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool IsBufferEmpty()
		{
			return this._history.Count == 0;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Determines whether the input buffer is full.
		/// </summary>
		/// <returns><c>true</c> if the input buffer is full; otherwise, <c>false</c>.</returns>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool IsBufferFull()
		{
			return this.MaxBufferSize > 0 && this._history.Count >= this.MaxBufferSize;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the state of the game at the specified frame.
		/// </summary>
		/// <returns>The state.</returns>
		/// <param name="frame">Frame.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool TryGetState(long frame, out FluxStates state)
		{
			KeyValuePair<FluxStates, FluxFrameInput> pair;
			bool result = this.TryGetStateAndInput(frame, out pair);
			state = pair.Key;
			return result;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the state at the specified frame.
		/// </summary>
		/// <returns>The state.</returns>
		/// <param name="frame">Frame.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public FluxStates GetState(long frame)
		{
			return this.GetStateAndInput(frame).Key;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the state of the game at the specified frame.
		/// </summary>
		/// <returns>The state.</returns>
		/// <param name="frame">Frame.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool TryGetStateAndInput(long frame, out KeyValuePair<FluxStates, FluxFrameInput> stateAndInput)
		{
			int index = this.GetIndex(frame);

			if (this.IsValidIndex(index))
			{
				stateAndInput = this._history[index];
				return true;
			}

			stateAndInput = new KeyValuePair<FluxStates, FluxFrameInput>(new FluxStates(), new FluxFrameInput());
			return false;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the state at the specified frame.
		/// </summary>
		/// <returns>The state.</returns>
		/// <param name="frame">Frame.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public KeyValuePair<FluxStates, FluxFrameInput> GetStateAndInput(long frame)
		{
			return this._history[this.GetFrameIndex(frame)];
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the player inputs at the specified frame.
		/// </summary>
		/// <returns>The player inputs.</returns>
		/// <param name="frame">Frame.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool TryGetInput(long frame, out FluxFrameInput input)
		{
			KeyValuePair<FluxStates, FluxFrameInput> pair;
			bool result = this.TryGetStateAndInput(frame, out pair);
			input = pair.Value;
			return result;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the player inputs at the specified frame.
		/// </summary>
		/// <returns>The player inputs.</returns>
		/// <param name="frame">Frame.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public FluxFrameInput GetInput(long frame)
		{
			return this.GetStateAndInput(frame).Value;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Go to the specified frame, removing the existing frames from the buffer if necessary.
		/// </summary>
		/// <param name="frame">Frame.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool TryGoToFrame(long frame)
		{
			// Check if we have already passed the specified frame...
			if (this.FirstStoredFrame > frame)
			{
				return false;
			}

			// If we haven't reached the specified frame yet, remove the first frame of the buffer 
			// and add a new frame to the end of the buffer until we reach the specified frame...
			while (this.FirstStoredFrame < frame)
			{
				this.RemoveNextFrame();
			}

			return true;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Sets the state of the game at the specified frame.
		/// </summary>
		/// <returns>Whether the state could be set successfully.</returns>
		/// <param name="frame">Frame.</param>
		/// <param name="state">The state.</param>
		/// <param name="input">The input that was applied to the specified state to get the next state.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool TrySetState(FluxStates state, FluxFrameInput input)
		{
			return this.TrySetState(new KeyValuePair<FluxStates, FluxFrameInput>(state, input));
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Sets the state of the game at the specified frame.
		/// </summary>
		/// <returns><c>true</c>, if set state was tryed, <c>false</c> otherwise.</returns>
		/// <param name="state">The game state (including the player inputs).</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool TrySetState(KeyValuePair<FluxStates, FluxFrameInput> state)
		{
			int index = this.GetIndex(state.Key.networkFrame);

			if (index == this._history.Count && !this.IsBufferFull())
			{
				this._history.Add(state);
				return true;
			}
			else if (this.IsValidIndex(index))
			{
				this._history[index] = state;
				return true;
			}

			return false;
		}
		#endregion

		#region protected instance methods
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the frame associated to the specified index... assuming that the index has a valid value.
		/// </summary>
		/// <returns>The frame.</returns>
		/// <param name="index">Index.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		protected long GetFrame(int index)
		{
			return (long)(index) + this.FirstStoredFrame;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the index where the information of the specified frame will be stored in the input buffer...
		/// assuming that the returned index has a valid value.
		/// </summary>
		/// <returns>The index.</returns>
		/// <param name="frame">Frame.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		protected int GetIndex(long frame)
		{
			return (int)(frame - this.FirstStoredFrame);
		}

		protected int GetFrameIndex(long frame)
		{
			int i = 0;
			foreach (KeyValuePair<FluxStates, FluxFrameInput> pair in this._history)
			{
				if (frame == pair.Key.networkFrame) return i;
				i++;
			}
			return -1;
		}

		protected bool IsValidIndex(int index)
		{
			return index >= 0 && index < this._history.Count;
		}

		protected bool IsValidFrame(int frame)
		{
			foreach (KeyValuePair<FluxStates, FluxFrameInput> pair in this._history)
			{
				if (frame == pair.Key.networkFrame) return true;
			}
			return false;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Move the buffer to the next frame.
		/// </summary>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool RemoveNextFrame()
		{
			if (this._history.Count > 0)
			{
				this._history.RemoveAt(0);
				++this.FirstStoredFrame;

				return true;
			}
			return false;
		}
		#endregion
	}
}