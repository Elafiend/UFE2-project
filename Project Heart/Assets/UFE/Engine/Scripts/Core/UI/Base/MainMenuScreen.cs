using UnityEngine;
using System.Collections;

namespace UFE3D
{
	public class MainMenuScreen : UFEScreen
	{
		public virtual void Quit()
		{
			UFE.Quit();
		}

		public virtual void GoToBluetoothPlayScreen()
		{
			UFE.StartBluetoothGameScreen();
		}

		public virtual void GoToSearchMatchScreen()
		{
			UFE.StartSearchMatchScreen();
		}

		public virtual void GoToStoryModeScreen()
		{
			UFE.StartStoryMode();
		}

		public virtual void GoToVersusModeScreen()
		{
			UFE.StartVersusModeScreen();
		}

		public virtual void GoToTrainingModeScreen()
		{
			UFE.StartTrainingMode();
		}

		public virtual void GoToNetworkOptionsScreen()
		{
			UFE.StartNetworkOptionsScreen();
		}

		public virtual void GoToOptionsScreen()
		{
			UFE.StartOptionsScreen();
		}

		public virtual void GoToCreditsScreen()
		{
			UFE.StartCreditsScreen();
		}
	}
}