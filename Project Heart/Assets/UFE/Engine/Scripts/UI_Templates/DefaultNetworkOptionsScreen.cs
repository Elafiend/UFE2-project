using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UFE3D;

public class DefaultNetworkOptionsScreen : NetworkOptionsScreen {
	#region public instance fields
	public AudioClip onLoadSound;
	public AudioClip music;
	public AudioClip selectSound;
	public AudioClip cancelSound;
	public AudioClip moveCursorSound;
	public float delayBeforePlayingMusic = 0.1f;
    public Button buttonBluetooth;
    public Button buttonRandomMatch;
    public Button buttonRoomMatch;
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
			this.GoToMainMenu
		);
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

		if (buttonBluetooth != null){
            buttonBluetooth.interactable = UFE.isBluetoothAddonInstalled;
		}

		if (buttonRandomMatch != null){
			buttonRandomMatch.interactable = UFE.isNetworkAddonInstalled;
		}

		if (buttonRoomMatch != null){
			buttonRoomMatch.interactable = UFE.isNetworkAddonInstalled;
		}
	}
	#endregion
}
