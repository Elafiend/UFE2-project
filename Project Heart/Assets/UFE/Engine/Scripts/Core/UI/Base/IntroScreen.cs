namespace UFE3D
{
	public class IntroScreen : UFEScreen
	{

		public int mainMenuFrameDelay = 6;

		public virtual void GoToMainMenu()
		{
			UFE.DelayLocalAction(GoToMainMenuDelayed, mainMenuFrameDelay);
		}

		public virtual void GoToMainMenuDelayed()
		{
			UFE.StartMainMenuScreen(0f);
		}
	}
}