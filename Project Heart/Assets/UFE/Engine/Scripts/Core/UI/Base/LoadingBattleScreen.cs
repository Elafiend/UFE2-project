namespace UFE3D
{
	public class LoadingBattleScreen : UFEScreen
	{
		#region public instance methods
		public virtual void StartBattle()
		{
			UFE.StartGame((float)UFE.config.gameGUI.gameFadeDuration);
		}
		#endregion
	}
}