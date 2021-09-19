using UnityEngine;
using System;
using System.Reflection;
using UFE3D;

public class CharacterSelectionScreen : UFEScreen {
	#region public instance properties
	public AudioClip selectSound;
	public AudioClip cancelSound;
	#endregion

	#region protected instance fields
	protected int p1HoverIndex = 0;
	protected int p2HoverIndex = 0;
	protected bool closing = false;
	protected UFE3D.CharacterInfo[] selectableCharacters = new UFE3D.CharacterInfo[0];
	#endregion

	#region public instance methods
	public virtual int GetHoverIndex(int player){
		if (player == 1){
			return this.p1HoverIndex;
		}else if (player == 2){
			return this.p2HoverIndex;
		}

		throw new ArgumentOutOfRangeException("player");
	}

	public virtual void GoToPreviousScreen(){
		this.closing = true;

		if (UFE.gameMode == GameMode.VersusMode){
			UFE.DelaySynchronizedAction(this.GoToVersusModeScreen, 0);
		}else if (UFE.gameMode == GameMode.NetworkGame){
			UFE.DelaySynchronizedAction(this.DisconnectFromMatch, 0);
		}else{
			UFE.DelaySynchronizedAction(this.GoToMainMenuScreen, 0);
		}
	}

	public virtual void OnCharacterSelectionAllowed(int characterIndex, int player){
		// If we haven't started loading a different screen....
		if (!this.closing){
			// Check if we are trying to select or deselect a character...
			if (characterIndex >= 0 && characterIndex <= this.GetMaxCharacterIndex()){
				// If we are selecting a character, check if the player has already selected a character...
				if(
					player == 1 && UFE.config.player1Character == null ||
					player == 2 && UFE.config.player2Character == null
				){
					// If the player hasn't selected any character yet, process the request...
					this.SetHoverIndex(player, characterIndex);
					UFE3D.CharacterInfo character = this.selectableCharacters[characterIndex];
					if (this.selectSound != null) UFE.PlaySound(this.selectSound);
					if (character != null && character.selectionSound != null) UFE.PlaySound(character.selectionSound);
					UFE.SetPlayer(player, character);


					// And check if we should start loading the next screen...
					if(
						UFE.config.player1Character != null && 
						(UFE.config.player2Character != null || UFE.gameMode == GameMode.StoryMode)
					){
						this.GoToNextScreen();
					}
				}
			}else if (characterIndex < 0){
				if(
					// If we are trying to deselect a character, check if at least one player has selected a character
					UFE.config.player1Character != null || UFE.config.player2Character != null 
					||
					// In network games, we also allow to return to the previous screen if the one of the player 
					// doesn't have a character selected and he presses the back button. We want to return to the 
					// previous screen even if the other player has a character selected.
					UFE.gameMode == GameMode.NetworkGame 
					&&
					(
						player == 1 && UFE.config.player1Character != null ||
						player == 2 && UFE.config.player2Character != null
					)
				){
					// In that case, check if the player that wants to deselect his current character has already
					// selected a character and try to deselect that character.
					if(
						player == 1 && UFE.config.player1Character != null ||
						player == 2 && UFE.config.player2Character != null
					){
						if (this.cancelSound != null) UFE.PlaySound(this.cancelSound);
						UFE.SetPlayer(player, null);
					}
				}else{
					// If none of the players has selected a character and one of the player wanted to deselect
					// his current character, that means that the player wants to return to the previous menu instead.
					this.GoToPreviousScreen();
				}
			}
		}
	}

	public virtual void SetHoverIndex(int player, int characterIndex){
		if (!this.closing){
			if (characterIndex >= 0 && characterIndex <= this.GetMaxCharacterIndex()){
				if (player == 1){
					p1HoverIndex = characterIndex;
				}else if (player == 2){
					p2HoverIndex = characterIndex;
				}
			}
		}
	}

	public void TryDeselectCharacter(){
		if (!UFE.isConnected){
			// If it's a local game, update the corresponding character immediately...
			if (UFE.config.player2Character != null && UFE.gameMode != GameMode.StoryMode && !UFE.GetCPU(2)){
				this.TryDeselectCharacter(2);
			}else{
				this.TryDeselectCharacter(1);
			}
		}else{
			// If it's an online game, find out if the local player is Player1 or Player2
			// and update the selection only for the local player...
			this.TryDeselectCharacter(UFE.GetLocalPlayer());
		}
	}

	public void TryDeselectCharacter(int player){
		this.TrySelectCharacter(-1, player);
	}

	public void TrySelectCharacter(){
		// If it's a local game, update the corresponding character immediately...
		if (!UFE.isConnected){
			if (UFE.config.player1Character == null){
				this.TrySelectCharacter(this.p1HoverIndex, 1);
			}else if (UFE.config.player2Character == null){
				this.TrySelectCharacter(this.p2HoverIndex, 2);
			}
		}else{
			// If it's an online game, find out if the local player is Player1 or Player2
			// and update the selection only for the local player...
			int localPlayer = UFE.GetLocalPlayer();

			if (localPlayer == 1){
				this.TrySelectCharacter(this.p1HoverIndex, localPlayer);
			}else if (localPlayer == 2){
				this.TrySelectCharacter(this.p2HoverIndex, localPlayer);
			}
		}
	}

	public void TrySelectCharacter(int characterIndex){
		if (!UFE.isConnected){
			// If it's a local game, update the corresponding character immediately...
			if (UFE.config.player1Character == null){
				this.TrySelectCharacter(characterIndex, 1);
			}else if (UFE.config.player2Character == null && UFE.gameMode != GameMode.StoryMode){
				this.TrySelectCharacter(characterIndex, 2);
			}
		}else{
			// If it's an online game, find out if the local player is Player1 or Player2
			// and update the selection only for the local player...
			this.TrySelectCharacter(characterIndex, UFE.GetLocalPlayer());
		}
	}
	
	public virtual void TrySelectCharacter(int characterIndex, int player){
		// Check if he was playing online or not...
		if (!UFE.isConnected){
			// If it's a local game, update the corresponding character immediately...
			this.OnCharacterSelectionAllowed(characterIndex, player);
		}else{
			// If it's an online game, find out if the requesting player is the local player
			// because we will only accept requests for the local player...
			int localPlayer = UFE.GetLocalPlayer();
			if (player == localPlayer){
				// We don't invoke the OnCharacterSelected() method immediately because we are using the frame-delay 
				// algorithm to keep players synchronized, so we can't invoke the OnCharacterSelected() method
				// until the other player has received the message with our choice.
				UFE.fluxCapacitor.RequestOptionSelection(localPlayer, (sbyte)characterIndex);
			}
		}
	}
	#endregion

	#region public override methods
	public override void OnShow (){
		base.OnShow();

		if (UFE.gameMode == GameMode.StoryMode){
			this.selectableCharacters = UFE.GetStoryModeSelectableCharacters();
		}else if (UFE.gameMode == GameMode.TrainingRoom){
			this.selectableCharacters = UFE.GetTrainingRoomSelectableCharacters();
		}else{
			this.selectableCharacters = UFE.GetVersusModeSelectableCharacters();
		}

		UFE.SetPlayer1(null);
		UFE.SetPlayer2(null);
		this.SetHoverIndex(1, 0);
		this.SetHoverIndex(2, this.GetMaxCharacterIndex());
	}

	public override void SelectOption (int option, int player){
		this.OnCharacterSelectionAllowed(option, player);
	}
	#endregion

	#region protected instance methods
	protected virtual int GetMaxCharacterIndex(){
		return this.selectableCharacters.Length - 1;
	}

	protected void GoToMainMenuScreen(){
		this.closing = true;
		UFE.StartMainMenuScreen();
	}

	protected void DisconnectFromMatch(){
		this.closing = true;
		UFE.DisconnectFromGame();
        UFE.StartConnectionLostScreen();
    }

	protected virtual void GoToNextScreen(){
		this.closing = true;

		if (UFE.gameMode == GameMode.StoryMode){
			UFE.DelaySynchronizedAction(this.StartStoryMode, 0.8);
		}else{
			UFE.DelaySynchronizedAction(this.GoToStageSelectionScreen, 0.8);
		}
	}

	protected void GoToStageSelectionScreen(){
		this.closing = true;
		UFE.StartStageSelectionScreen();
	}
	
	protected void GoToVersusModeScreen(){
		this.closing = true;
		UFE.StartVersusModeScreen();
	}

	protected void StartStoryMode(){
		this.closing = true;
		UFE.StartStoryModeOpeningScreen();
	}
	#endregion
}
