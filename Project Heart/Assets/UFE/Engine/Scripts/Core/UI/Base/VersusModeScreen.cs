using UnityEngine;
using System.Collections;

namespace UFE3D
{
	public class VersusModeScreen : UFEScreen
	{
		public virtual void SelectPlayerVersusPlayer()
		{
			UFE.StartPlayerVersusPlayer();
		}

		public virtual void SelectPlayerVersusCpu()
		{
			UFE.StartPlayerVersusCpu();
		}

		public virtual void SelectCpuVersusCpu()
		{
			UFE.StartCpuVersusCpu();
		}

		public virtual void GoToMainMenu()
		{
			UFE.StartMainMenuScreen();
		}
	}
}