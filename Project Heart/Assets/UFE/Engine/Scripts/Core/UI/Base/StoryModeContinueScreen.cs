using UnityEngine;
using System.Collections;

namespace UFE3D
{
	public class StoryModeContinueScreen : UFEScreen
	{
		public virtual void RepeatBattle()
		{
			UFE.StartStoryModeBattle();
		}

		public virtual void GoToGameOverScreen()
		{
			UFE.StartStoryModeGameOverScreen();
		}
	}
}