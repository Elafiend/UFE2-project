namespace UFE3D
{
	public class ConnectionLostScreen : UFEScreen
	{
		public virtual void GoToMainMenu()
		{
			UFE.StartMainMenuScreen();
		}

		public virtual void GoToNetworkGameScreen()
		{
			UFE.GoToNetworkGameScreen();
		}
	}
}