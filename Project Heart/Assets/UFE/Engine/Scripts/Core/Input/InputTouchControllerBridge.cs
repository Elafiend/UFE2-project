using UnityEngine;
///--------------------------------------------------------------------------------------------------------------------
/// <summary>
/// This class is used by Control Freak 2.
/// https://www.assetstore.unity3d.com/#/content/11562
/// </summary>
///--------------------------------------------------------------------------------------------------------------------

namespace UFE3D
{
	public abstract class InputTouchControllerBridge : MonoBehaviour
	{
		virtual public void Init() { }

		abstract public float GetAxis(string axisName);
		abstract public float GetAxisRaw(string axisName);
		abstract public bool GetButton(string axisName);

		abstract public void ShowBattleControls(bool visible, bool animate);

		private bool
			prevBattleGUI,
			prevGamePaused;


		// ----------------
		void OnEnable()
		{
			UFE.OnGameEnds += this.OnGameEnds;
			UFE.OnRoundBegins += this.OnRoundBegins;
			UFE.OnRoundEnds += this.OnRoundEnds;
			UFE.OnGamePaused += this.OnGamePaused;
			UFE.OnScreenChanged += this.OnScreenChanged;

			this.prevBattleGUI = false;
			this.prevGamePaused = false;

			this.Init();
		}


		// ---------------
		void OnDisable()
		{
			UFE.OnGameEnds -= this.OnGameEnds;
			UFE.OnRoundBegins -= this.OnRoundBegins;
			UFE.OnRoundEnds -= this.OnRoundEnds;
			UFE.OnGamePaused -= this.OnGamePaused;
			UFE.OnScreenChanged -= this.OnScreenChanged;

		}


		// ----------------
		public void DoFixedUpdate()
		{
			bool battleGUI = (UFE.battleGUI != null);
			bool gamePaused = UFE.isPaused();

			if (battleGUI != this.prevBattleGUI)
			{
				this.ShowBattleControls(battleGUI && !gamePaused, battleGUI);
			}

			else if (gamePaused != this.prevGamePaused)
			{
				if (battleGUI)
				{
					this.ShowBattleControls(!gamePaused, true);
				}
			}

			this.prevBattleGUI = battleGUI;
			this.prevGamePaused = gamePaused;
		}



		// ---------------
		private void OnGameEnds(ControlsScript winner, ControlsScript loser)
		{
			//Debug.Log(ControlFreak2.CFUtils.LogPrefix() + "OnGameEnds");
			this.ShowBattleControls(false, false);
		}

		// -------------
		private void OnRoundEnds(ControlsScript winner, ControlsScript loser)
		{
			//Debug.Log(ControlFreak2.CFUtils.LogPrefix() + "Round Ends");
			//this.ShowBattleControls(false, true);
		}

		// ---------------
		private void OnRoundBegins(int roundNum)
		{
			//Debug.Log(ControlFreak2.CFUtils.LogPrefix() + "Round Begin : " + roundNum);
			//this.ShowBattleControls(true, true);

		}

		// -------------------
		private void OnGamePaused(bool paused)
		{
			//Debug.Log(ControlFreak2.CFUtils.LogPrefix() + "GamePaused : " + paused);
			//this.ShowBattleControls(!paused, true);
		}

		// -----------------
		private void OnScreenChanged(UFEScreen old, UFEScreen newScreen)
		{
			//Debug.Log(ControlFreak2.CFUtils.LogPrefix() + "Screen change:" + (old != null ? old.GetType().Name : "NULL") + 
			//		" new:" + (newScreen != null ? newScreen.GetType().Name : "NULL"));
		}
	}
}
