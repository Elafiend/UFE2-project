using UnityEngine;

namespace UFE3D
{
    public class BluetoothJoinGameScreen : UFEScreen
    {
        #region protected instance fields
        protected bool _connecting = false;
        #endregion

        public string matchName = string.Empty;

        #region public override methods
        public override void OnShow()
        {
            base.OnShow();

            UFE.multiplayerMode = UFE.MultiplayerMode.Bluetooth;
        }
        #endregion

        #region public instance methods
        public virtual void GoToBluetoothGameScreen()
        {
            this.RemoveListeners();
            UFE.StartBluetoothGameScreen();
        }

        public virtual void GoToBluetoothConnectionLostScreen()
        {
            this.RemoveListeners();
            UFE.StartConnectionLostScreen();
        }

        public virtual void FindAndJoin()
        {
            this.TryConnect(this.matchName);
        }

        public virtual void JoinError(string errorMsg = null)
        {

        }
        #endregion

        #region protected instance methods
        protected virtual void AddListeners()
        {
            UFE.multiplayerAPI.OnJoined += this.OnJoined;
            UFE.multiplayerAPI.OnJoinError += this.OnJoinError;
        }

        protected virtual void RemoveListeners()
        {
            UFE.multiplayerAPI.OnJoined -= this.OnJoined;
            UFE.multiplayerAPI.OnJoinError -= this.OnJoinError;
        }

        protected virtual void OnJoined(MultiplayerAPI.JoinedMatchInformation match)
        {
            if (UFE.config.debugOptions.connectionLog) Debug.Log("Match Starting...");
            this.RemoveListeners();
            UFE.StartNetworkGame((float)UFE.config.gameGUI.screenFadeDuration, 2, false);
        }

        protected virtual void OnJoinError()
        {
            if (UFE.config.debugOptions.connectionLog) Debug.Log("Match Not Found.");
            this.RemoveListeners();
            this.JoinError("Match Not Found.");
        }

        protected virtual void TryConnect(string matchName)
        {
            // First, we check that we aren't already connected to a client or a server...
            if (!UFE.multiplayerAPI.IsConnected() && !this._connecting)
            {
                MultiplayerAPI.MatchInformation match = new MultiplayerAPI.MatchInformation();
                match.matchName = matchName;

                if (UFE.config.debugOptions.connectionLog) Debug.Log("Joining Match...");
                this.AddListeners();
                UFE.multiplayerAPI.JoinMatch(match);
            }
        }
        #endregion
    }
}