namespace UFE3D
{
    public class PauseScreen : UFEScreen
    {

        public int backToMenuFrameDelay = 6;

        public virtual void GoToMainMenu()
        {
            UFE.DelayLocalAction(GoToMainMenuDelayed, backToMenuFrameDelay);
        }

        private void GoToMainMenuDelayed()
        {
            UFE.EndGame();
            UFE.FireGameEnds();
            UFE.StartMainMenuScreen();
            UFE.PauseGame(false);
        }

        public virtual void ResumeGame()
        {
            UFE.PauseGame(false);
        }
    }
}