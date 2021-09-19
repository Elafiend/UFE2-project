using UnityEngine;
using System;
using System.Collections.Generic;


namespace UFE3D
{
	public class FluxPlayerManager
	{
		#region constant definitions
		public const int NumberOfPlayers = 2;
		#endregion

		#region public instance fields
		public FluxPlayer player1 = new FluxPlayer(1);
		public FluxPlayer player2 = new FluxPlayer(2);
		#endregion

		#region public instance methods
		public bool ArePredictedAndConfirmedInputsEqual(int player, long frame)
		{
			return this.GetPlayer(player).inputBuffer.ArePredictedAndConfirmedInputsEqual(frame);
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Whether there are remote characters or not.
		/// </summary>
		/// <returns><c>true</c>, if there are remote characters, <c>false</c> otherwise.</returns>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public virtual bool AreThereRemoteCharacters()
		{
			return this.player1.isRemotePlayer || this.player2.isRemotePlayer;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the character.
		/// </summary>
		/// <returns>The character.</returns>
		/// <param name="player">Character index.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public FluxPlayer GetPlayer(int player)
		{
			if (player == 1) return this.player1;
			else if (player == 2) return this.player2;
			else return null;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Get the first frame where the predicted input didn't match the confirmed input 
		/// or a negative value if all predicted inputs match the confirmed inputs.
		/// </summary>
		/// <returns>The first frame where the predicted input didn't match the confirmed input.</returns>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public long GetFirstFrameWhereRollbackIsRequired()
		{
			long firstFrameWhereRollbackIsRequired = -1;

			for (int i = 1; i <= FluxPlayerManager.NumberOfPlayers; ++i)
			{
				long temp = this.GetPlayer(i).inputBuffer.GetFirstFrameWhereRollbackIsRequired();
				if (temp >= 0)
				{
					if (firstFrameWhereRollbackIsRequired < 0)
					{
						firstFrameWhereRollbackIsRequired = temp;
					}
					else
					{
						firstFrameWhereRollbackIsRequired = Math.Min(temp, firstFrameWhereRollbackIsRequired);
					}
				}
			}

			return firstFrameWhereRollbackIsRequired;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the frame of the last input confirmed by the user or a negative value if no input has been confirmed.
		/// </summary>
		/// <returns>The last confirmed frame.</returns>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public long GetLastFrameWithConfirmedInput()
		{
			long lastFrameWithConfirmedInput = -1L;

			for (int i = 1; i <= FluxPlayerManager.NumberOfPlayers; ++i)
			{
				long temp = this.GetPlayer(i).inputBuffer.GetLastFrameWithConfirmedInput();
				if (temp >= 0)
				{
					if (lastFrameWithConfirmedInput < 0)
					{
						lastFrameWithConfirmedInput = temp;
					}
					else
					{
						lastFrameWithConfirmedInput = Math.Min(temp, lastFrameWithConfirmedInput);
					}
				}
			}

			return lastFrameWithConfirmedInput;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the frame of the last input predicted by the system or a negative value if no input has been 
		/// confirmed.
		/// </summary>
		/// <returns>The last confirmed frame.</returns>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public long GetLastFrameWithPredictedInput()
		{
			long lastFrameWithPredictedInput = -1;

			for (int i = 1; i <= FluxPlayerManager.NumberOfPlayers; ++i)
			{
				long temp = this.GetPlayer(i).inputBuffer.GetLastFrameWithPredictedInput();
				if (temp >= 0)
				{
					if (lastFrameWithPredictedInput < 0)
					{
						lastFrameWithPredictedInput = temp;
					}
					else
					{
						lastFrameWithPredictedInput = Math.Min(temp, lastFrameWithPredictedInput);
					}
				}
			}

			return lastFrameWithPredictedInput;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the frame of the last input confirmed by the user or a negative value if no input has been confirmed.
		/// </summary>
		/// <returns>The last confirmed frame.</returns>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public long GetLastFrameWithReadyInput()
		{
			long lastFrameWithReadyInput = -1;

			for (int i = 1; i <= FluxPlayerManager.NumberOfPlayers; ++i)
			{
				long temp = this.GetPlayer(i).inputBuffer.GetLastFrameWithReadyInput();
				if (temp >= 0)
				{
					if (lastFrameWithReadyInput < 0)
					{
						lastFrameWithReadyInput = temp;
					}
					else
					{
						lastFrameWithReadyInput = Math.Min(temp, lastFrameWithReadyInput);
					}
				}
			}

			return lastFrameWithReadyInput;
		}

		public long GetNextExpectedFrame()
		{
			long p1 = this.GetNextExpectedFrame(1);
			long p2 = this.GetNextExpectedFrame(2);

			if (p1 >= 0L && p2 >= 0L)
			{
				return Math.Min(p1, p2);
			}
			else
			{
				return Math.Max(p1, p2);
			}
		}

		public long GetNextExpectedFrame(int player)
		{
			FluxPlayer p = this.GetPlayer(player);

			if (p != null)
			{
				//			// Return the first frame without a confirmed input
				//			for (long i = p.inputBuffer.FirstFrame; i <= p.inputBuffer.LastFrame; ++i){
				//				int index = p.inputBuffer.GetIndex(i);
				//
				//				if (index >= 0 && p.inputBuffer[index].ConfirmedInput == null){
				//					return i;
				//				}
				//			}
				//
				//			return p.inputBuffer.LastFrame + 1L;

				return p.inputBuffer.GetLastFrameWithConfirmedInput() + 1L;
			}

			return 0L;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Initialize this instance.
		/// </summary>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public virtual void Initialize()
		{
			this.Initialize(0);
		}

		public virtual void Initialize(long currentFrame)
		{
			this.Initialize(currentFrame, -1);
		}

		public virtual void Initialize(long currentFrame, int maxBufferSize)
		{
			this.player1.Initialize(currentFrame, maxBufferSize);
			this.player2.Initialize(currentFrame, maxBufferSize);
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Whether the specified characters is controlled locally.
		/// </summary>
		/// <returns><c>true</c>, if there are remote characters, <c>false</c> otherwise.</returns>
		/// <param name="player">Character Index</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public virtual bool IsLocalCharacter(int player)
		{
			return this.GetPlayer(player).isLocalPlayer;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Whether the specified characters is controlled remotely.
		/// </summary>
		/// <returns><c>true</c>, if there are remote characters, <c>false</c> otherwise.</returns>
		/// <param name="player">Character Index</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public virtual bool IsRemoteCharacter(int player)
		{
			return this.GetPlayer(player).isRemotePlayer;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Reads the inputs.
		/// </summary>
		/// <param name="frame">Frame.</param>
		/// <param name="remoteCharacterInputPrediction">If set to <c>true</c> remote character input prediction.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public virtual void ReadInputs(
			long frame,
			int randomSeed,
			IList<int?> selectedOptions,
			bool remoteCharacterInputPrediction
		)
		{
			// Iterate over all the characters...
			for (int i = 1; i <= FluxPlayerManager.NumberOfPlayers; ++i)
			{

			}
		}

		public virtual bool ReadInputs(
			int player,
			long frame,
			sbyte? selectedOption,
			bool remoteCharacterInputPrediction
		)
		{
			FluxPlayer p = this.GetPlayer(player);

			if (p != null && p.inputBuffer != null && p.inputController != null && !p.inputBuffer.IsFull())
			{
				FrameInput? oldInput;

				// As we don't want to override existing values, 
				// we need to check if there was already a predicted input for the specified frame...
				if (!p.inputBuffer.TryGetInput(frame, out oldInput) || oldInput == null)
				{
					FrameInput currentInput;

					// Check if the specified player is using a Dummy Controller...
					if (p.inputController is DummyInputController && p.inputBuffer.TryGetInput(frame - 1L, out oldInput))
					{
						// In that case, repeat the input from last frame (if any)
						currentInput = oldInput != null ? oldInput.Value : new FrameInput(FrameInput.NullSelectedOption);
					}
					else
					{
						// Retrieve the "predicted input" for the specified frame...
						try
						{
							currentInput = p.inputController.inputs.ToFrameInput(selectedOption);
						}
						catch (Exception e)
						{
							Debug.LogError("Read Input: " + player + " | " + frame + " | " + selectedOption);
							throw e;
						}
					}

					// Find out if it's a local character or if the prediction of remote character input is enabled
					bool localCharacter = p.isLocalPlayer;
					if (remoteCharacterInputPrediction || localCharacter)
					{
						p.inputBuffer.TrySetPredictedInput(frame, currentInput);
					}

					// If it's a local character, then mark the "predicted input" as "confirmed"...
					if (localCharacter)
					{
						p.inputBuffer.TrySetConfirmedInput(frame, currentInput);
					}

					return true;
				}
			}
			return false;
		}

		public bool RemoveNextInput()
		{
			return this.player1.RemoveNextInput() || this.player2.RemoveNextInput();
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
			isConfirmed = true;

			// Iterate over all characters trying to find out if the input is confirmed for the specified frame...
			for (int i = 1; isConfirmed && i <= FluxPlayerManager.NumberOfPlayers; ++i)
			{
				if (!this.GetPlayer(i).inputBuffer.TryCheckIfInputIsConfirmed(frame, out isConfirmed) || !isConfirmed)
				{
					// If there has been an error while processing the request for any character, return false
					isConfirmed = false;
					return false;
				}
			}
			return true;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Determines whether the specified character have a confirmed input.
		/// </summary>
		/// <returns><c>true</c> if this instance is confirmed; otherwise, <c>false</c>.</returns>
		/// <param name="character">Character Index.</param>
		/// <param name="frame">Frame.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool TryCheckIfInputIsConfirmed(int player, long frame, out bool isReady)
		{
			if (player >= 1 && player <= FluxPlayerManager.NumberOfPlayers)
			{
				return this.GetPlayer(player).inputBuffer.TryCheckIfInputIsConfirmed(frame, out isReady);
			}

			isReady = false;
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
			isPredicted = true;

			// Iterate over all characters trying to find out if the input is predicted for the specified frame...
			for (int i = 1; isPredicted && i <= FluxPlayerManager.NumberOfPlayers; ++i)
			{
				if (!this.GetPlayer(i).inputBuffer.TryCheckIfInputIsPredicted(frame, out isPredicted) || !isPredicted)
				{
					// If there has been an error while processing the request for any character, return false
					isPredicted = false;
					return false;
				}
			}
			return true;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Determines whether the specified character have a predicted input.
		/// </summary>
		/// <returns><c>true</c> if this instance is predicted; otherwise, <c>false</c>.</returns>
		/// <param name="character">Character Index.</param>
		/// <param name="frame">Frame.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool TryCheckIfInputIsPredicted(int player, long frame, out bool isPredicted)
		{
			if (player >= 1 && player <= FluxPlayerManager.NumberOfPlayers)
			{
				return this.GetPlayer(player).inputBuffer.TryCheckIfInputIsPredicted(frame, out isPredicted);
			}

			isPredicted = false;
			return false;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Determines whether this instance is ready because all characters have at least a predicted or confirmed 
		/// input.
		/// </summary>
		/// <returns><c>true</c> if this instance is ready; otherwise, <c>false</c>.</returns>
		/// <param name="frame">Frame.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool TryCheckIfInputIsReady(long frame, out bool isReady)
		{
			isReady = true;

			// Iterate over all characters trying to find out if the input is ready for the specified frame...
			for (int i = 1; isReady && i <= FluxPlayerManager.NumberOfPlayers; ++i)
			{
				if (!this.GetPlayer(i).inputBuffer.TryCheckIfInputIsReady(frame, out isReady) || !isReady)
				{
					// If there has been an error while processing the request for any character, return false
					isReady = false;
					return false;
				}
			}
			return true;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Determines whether the specified character have at least a predicted or confirmed input.
		/// </summary>
		/// <returns><c>true</c> if this instance is ready; otherwise, <c>false</c>.</returns>
		/// <param name="character">Character Index.</param>
		/// <param name="frame">Frame.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool TryCheckIfInputIsReady(int player, long frame, out bool isReady)
		{
			if (player >= 1 && player <= FluxPlayerManager.NumberOfPlayers)
			{
				return this.GetPlayer(player).inputBuffer.TryCheckIfInputIsReady(frame, out isReady);
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
		/// <param name="character">Character.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool TryConfirmPredictedInput(int player, long frame)
		{
			if (player >= 1 && player <= FluxPlayerManager.NumberOfPlayers)
			{
				return this.GetPlayer(player).inputBuffer.TryConfirmPredictedInput(frame);
			}

			return false;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Try to get the input associated to the specified character in the specified frame, 
		/// regardless of if the input has been predicted by the system or confirmed by the user.
		/// </summary>
		/// <returns>Whether the input could be retrieved successfully.</returns>
		/// <param name="frame">Frame.</param>
		/// <param name="character">Character.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool TryGetInput(int player, long frame, out FrameInput? input)
		{
			if (player >= 1 && player <= FluxPlayerManager.NumberOfPlayers)
			{
				return this.GetPlayer(player).inputBuffer.TryGetInput(frame, out input);
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
		/// <param name="character">Character.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool TryOverridePredictionWithConfirmedInput(int player, long frame)
		{
			if (player >= 1 && player <= FluxPlayerManager.NumberOfPlayers)
			{
				return this.GetPlayer(player).inputBuffer.TryOverridePredictionWithConfirmedInput(frame);
			}

			return false;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Try to set the input that has been confirmed by the character.
		/// </summary>
		/// <returns>Whether the input could be set successfully.</returns>
		/// <param name="frame">Frame.</param>
		/// <param name="character">Character.</param>
		/// <param name="characterInput">Character Input.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool TrySetConfirmedInput(int player, long frame, FrameInput characterInput)
		{
			return this.TrySetConfirmedInput(player, frame, characterInput, false);
		}

		public bool TrySetConfirmedInput(int player, long frame, FrameInput characterInput, bool overridePrediction)
		{
			if (player >= 1 && player <= FluxPlayerManager.NumberOfPlayers)
			{
				FluxPlayerInputBuffer buffer = this.GetPlayer(player).inputBuffer;
				bool isPredicted = false;
				bool isConfirmed = false;

				//---------------------------------------------------------------------------------------------------------
				// If the specified input wasn't already predicted, 
				// try to use the specified input as the input prediction for this frame
				//---------------------------------------------------------------------------------------------------------
				//			if (!overridePrediction && !buffer.TryCheckIfInputIsPredicted(frame, out isPredicted) || !isPredicted){
				//				buffer.TrySetPredictedInput(frame, characterInput);
				//			}

				if (overridePrediction || !buffer.TryCheckIfInputIsPredicted(frame, out isPredicted) || !isPredicted)
				{
					buffer.TrySetPredictedInput(frame, characterInput);
				}

				//---------------------------------------------------------------------------------------------------------
				// If we have other predictions which haven't been confirmed after the specified frame,
				// try to update those predictions with the specified input.
				//---------------------------------------------------------------------------------------------------------
				long lastPredictedInput = this.GetLastFrameWithPredictedInput();
				for (long i = frame + 1; i <= lastPredictedInput; ++i)
				{
					if (
						(buffer.TryCheckIfInputIsPredicted(i, out isPredicted) && isPredicted)
						&&
						(!buffer.TryCheckIfInputIsConfirmed(i, out isConfirmed) || !isConfirmed)
					)
					{
						buffer.TrySetPredictedInput(i, characterInput);
					}
				}

				//---------------------------------------------------------------------------------------------------------
				// Finally, try to confirm the specified input.
				//---------------------------------------------------------------------------------------------------------
				return buffer.TrySetConfirmedInput(frame, characterInput, overridePrediction);
			}
			return false;
		}

		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Try to set the input that has been predicted by the system.
		/// </summary>
		/// <returns>Whether the input could be set successfully.</returns>
		/// <param name="character">Character.</param>
		/// <param name="characterInput">Character Input.</param>
		///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public bool TrySetPredictedInput(int player, long frame, FrameInput characterInput)
		{
			if (player >= 1 && player <= FluxPlayerManager.NumberOfPlayers)
			{
				return this.GetPlayer(player).inputBuffer.TrySetPredictedInput(frame, characterInput);
			}
			return false;
		}
		#endregion
	}
}