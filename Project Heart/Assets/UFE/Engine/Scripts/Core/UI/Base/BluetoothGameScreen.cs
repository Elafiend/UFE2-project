namespace UFE3D
{
	public class BluetoothGameScreen : UFEScreen
	{
		public virtual void GoToMainMenu()
		{
			UFE.StartMainMenuScreen();
		}

		public virtual void HostGame()
		{
			UFE.StartBluetoothHostGameScreen();
		}

		public virtual void JoinGame()
		{
			UFE.StartBluetoothJoinGameScreen();
		}
	}
}