using UnityEngine;
using FPLibrary;

namespace UFE3D
{
    [System.Serializable]
    public class GameGUI
    {
        public bool hasGauge = true;
        public Fix64 screenFadeDuration = .5;
        public Fix64 gameFadeDuration = .5;
        public Fix64 roundFadeDuration = .5;
        public Color screenFadeColor = Color.black;
        public Color gameFadeColor = Color.black;
        public Color roundFadeColor = Color.black;
        public bool useCanvasScaler = false;
        public CanvasScalerInformation canvasScaler = new CanvasScalerInformation();

        public IntroScreen introScreen;
        public MainMenuScreen mainMenuScreen;
        public OptionsScreen optionsScreen;
        public CreditsScreen creditsScreen;
        public PauseScreen pauseScreen;

        public CharacterSelectionScreen characterSelectionScreen;
        public CharacterSelectionScreen teamSelectionScreen;
        public StageSelectionScreen stageSelectionScreen;
        public LoadingBattleScreen loadingBattleScreen;
        public BattleGUI battleGUI;

        public StoryModeContinueScreen storyModeContinueScreen;
        public StoryModeScreen storyModeGameOverScreen;
        public StoryModeScreen storyModeCongratulationsScreen;

        public VersusModeScreen versusModeScreen;
        public VersusModeAfterBattleScreen versusModeAfterBattleScreen;

        public ConnectionLostScreen connectionLostScreen;
        public ReplayMode replayTools;
        public HostGameScreen hostGameScreen;
        public JoinGameScreen joinGameScreen;
        public NetworkOptionsScreen networkOptionsScreen;
        public NetworkRoomMatchScreen roomMatchScreen;
        public BluetoothGameScreen bluetoothGameScreen;
        public BluetoothHostGameScreen bluetoothHostGameScreen;
        public BluetoothJoinGameScreen bluetoothJoinGameScreen;
        public RandomMatchScreen randomMatchScreen;
        public SearchMatchScreen searchMatchScreen;
        public OnlineModeAfterBattleScreen onlineModeAfterBattleScreen;
    }
}