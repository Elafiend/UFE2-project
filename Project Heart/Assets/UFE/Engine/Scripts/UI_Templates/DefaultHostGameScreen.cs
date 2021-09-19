﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UFE3D;

public class DefaultHostGameScreen : HostGameScreen {
	#region public instance fields
	public AudioClip onLoadSound;
	public AudioClip music;
	public AudioClip selectSound;
	public AudioClip cancelSound;
	public AudioClip moveCursorSound;
	public float delayBeforePlayingMusic = 0.1f;
	public Text connectionStatus;
	#endregion

	#region public override methods
	public override void DoFixedUpdate(
		IDictionary<InputReferences, InputEvents> player1PreviousInputs,
		IDictionary<InputReferences, InputEvents> player1CurrentInputs,
		IDictionary<InputReferences, InputEvents> player2PreviousInputs,
		IDictionary<InputReferences, InputEvents> player2CurrentInputs
	){
		base.DoFixedUpdate(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);

		this.DefaultNavigationSystem(
			player1PreviousInputs,
			player1CurrentInputs,
			player2PreviousInputs,
			player2CurrentInputs,
			this.moveCursorSound,
			this.selectSound,
			this.cancelSound,
			this.GoToNetworkGameScreen
		);
	}

    public override void StartHostGame(Text textUI) {
        connectionStatus.text = "Waiting for players...";

        UFE.multiplayerAPI.OnMatchCreated -= this.OnMatchCreated;
        UFE.multiplayerAPI.OnMatchCreated += this.OnMatchCreated;
        UFE.multiplayerAPI.OnMatchCreationError -= this.OnMatchCreationError;
        UFE.multiplayerAPI.OnMatchCreationError += this.OnMatchCreationError;
        UFE.multiplayerAPI.CreateMatch(new MultiplayerAPI.MatchCreationRequest(textUI.text, 3, true));
    }
	
	public override void OnShow (){
		base.OnShow ();
		this.HighlightOption(this.FindFirstSelectable());
		
		if (this.music != null){
			UFE.DelayLocalAction(delegate(){UFE.PlayMusic(this.music);}, this.delayBeforePlayingMusic);
		}
		
		if (this.onLoadSound != null){
			UFE.DelayLocalAction(delegate(){UFE.PlaySound(this.onLoadSound);}, this.delayBeforePlayingMusic);
		}
	}
	#endregion
}
