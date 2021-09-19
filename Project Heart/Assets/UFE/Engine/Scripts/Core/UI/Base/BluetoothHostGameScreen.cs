using UnityEngine;
using UnityEngine.UI;

namespace UFE3D
{
    public class BluetoothHostGameScreen : UFEScreen
    {
        #region public override methods
        public override void OnShow()
        {
            base.OnShow();

            UFE.multiplayerMode = UFE.MultiplayerMode.Bluetooth;
        }
        #endregion


        public virtual void GoToBluetoothGameScreen()
        {
            UFE.DisconnectFromGame();
            UFE.StartBluetoothGameScreen();
        }

        public virtual void GoToBluetoothConnectionLostScreen()
        {
            UFE.StartConnectionLostScreen();
        }

        public virtual void HostGame(Text textUI)
        {
            UFE.multiplayerAPI.OnMatchCreated -= this.OnMatchCreated;
            UFE.multiplayerAPI.OnMatchCreated += this.OnMatchCreated;
            UFE.multiplayerAPI.OnMatchCreationError -= this.OnMatchCreationError;
            UFE.multiplayerAPI.OnMatchCreationError += this.OnMatchCreationError;
            UFE.multiplayerAPI.CreateMatch(new MultiplayerAPI.MatchCreationRequest(textUI.text, 2, true));
        }

        #region protected instance methods
        protected virtual void OnMatchCreated(MultiplayerAPI.CreatedMatchInformation match)
        {
            UFE.multiplayerAPI.OnMatchCreated -= this.OnMatchCreated;
            UFE.multiplayerAPI.OnMatchCreationError -= this.OnMatchCreationError;
            UFE.multiplayerAPI.OnPlayerConnectedToMatch -= this.OnPlayerConnectedToMatch;
            UFE.multiplayerAPI.OnPlayerConnectedToMatch += this.OnPlayerConnectedToMatch;

            if (UFE.config.debugOptions.connectionLog) Debug.Log("Match Created: " + match.matchName);
            if (UFE.config.debugOptions.connectionLog) Debug.Log("Waiting for players...");
        }

        protected virtual void OnMatchCreationError()
        {
            UFE.multiplayerAPI.OnMatchCreated -= this.OnMatchCreated;
            UFE.multiplayerAPI.OnMatchCreationError -= this.OnMatchCreationError;

            //this.GoToConnectionLostScreen();
            if (UFE.config.debugOptions.connectionLog) Debug.Log("Error Creating Match.");
        }

        protected virtual void OnPlayerConnectedToMatch(MultiplayerAPI.PlayerInformation player)
        {
            UFE.multiplayerAPI.OnPlayerConnectedToMatch -= this.OnPlayerConnectedToMatch;
            UFE.multiplayerAPI.OnPlayerDisconnectedFromMatch -= UFE.OnPlayerDisconnectedFromMatch;

            if (UFE.config.debugOptions.connectionLog) Debug.Log("(Host) Match Starting...");
            UFE.StartNetworkGame((float)UFE.config.gameGUI.screenFadeDuration, 1, false);
        }

        protected virtual void OnPlayerDisconnectedFromMatch(MultiplayerAPI.PlayerInformation player)
        {
            UFE.multiplayerAPI.OnPlayerConnectedToMatch -= this.OnPlayerConnectedToMatch;
            UFE.multiplayerAPI.OnPlayerDisconnectedFromMatch -= this.OnPlayerDisconnectedFromMatch;

            this.GoToBluetoothConnectionLostScreen();
            if (UFE.config.debugOptions.connectionLog) Debug.Log("Player Disconnected From Match.");
        }
        #endregion
    }
}