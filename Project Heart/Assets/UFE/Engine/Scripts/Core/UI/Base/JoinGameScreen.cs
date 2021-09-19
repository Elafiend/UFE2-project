using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UFE3D
{
    public class JoinGameScreen : UFEScreen
    {
        #region protected instance fields
        protected bool _connecting = false;
        protected bool _spectator = false;
        protected IList<MultiplayerAPI.MatchInformation> _foundServers = new List<MultiplayerAPI.MatchInformation>();
        #endregion

        public string matchName = null;

        #region public override methods
        public override void OnShow()
        {
            base.OnShow();

            UFE.multiplayerMode = UFE.MultiplayerMode.Online;
            RefreshGameList();
        }
        #endregion

        #region public instance methods
        public virtual void GoToNetworkGameScreen()
        {
            RemoveListeners();
            UFE.StartRoomMatchScreen();
        }

        public virtual void GoToConnectionLostScreen()
        {
            RemoveListeners();
            UFE.StartConnectionLostScreen();
        }

        public virtual void RefreshGameList()
        {
            _foundServers.Clear();

            UFE.multiplayerAPI.OnMatchesDiscovered -= this.OnMatchesDiscovered;
            UFE.multiplayerAPI.OnMatchesDiscovered += this.OnMatchesDiscovered;
            UFE.multiplayerAPI.OnMatchDiscoveryError -= this.OnMatchDiscoveryError;
            UFE.multiplayerAPI.OnMatchDiscoveryError += this.OnMatchDiscoveryError;

            UFE.multiplayerAPI.StartSearchingMatches();
        }

        public virtual void RemoveListeners()
        {
            UFE.multiplayerAPI.OnJoined -= this.OnJoined;
            UFE.multiplayerAPI.OnJoinError -= this.OnJoinError;
            UFE.multiplayerAPI.OnMatchesDiscovered -= this.OnMatchesDiscovered;
            UFE.multiplayerAPI.OnMatchDiscoveryError -= this.OnMatchDiscoveryError;
        }

        public virtual void FindAndJoin(string matchName)
        {
            this.matchName = matchName;
            TryConnect();
        }

        public virtual void FindAndSpectate(string matchName)
        {
            _spectator = true;
            this.matchName = matchName;
            TryConnect();
        }

        public virtual void JoinError(string errorMsg = null) { }
        #endregion

        #region protected instance methods
        protected virtual void OnJoined(MultiplayerAPI.JoinedMatchInformation match)
        {
            UFE.multiplayerAPI.OnJoined -= this.OnJoined;
            UFE.multiplayerAPI.OnJoinError -= this.OnJoinError;

            if (UFE.config.debugOptions.connectionLog) Debug.Log("Match Starting...");
            int playerNum = _spectator ? 3 : 2;
            UFE.StartNetworkGame((float)UFE.config.gameGUI.screenFadeDuration, playerNum, false);
        }

        protected virtual void OnJoinError()
        {
            UFE.multiplayerAPI.OnJoined -= this.OnJoined;
            UFE.multiplayerAPI.OnJoinError -= this.OnJoinError;

            // Try to connect to other found matches
            this._connecting = false;
            this.TryConnect();
        }

        protected virtual void OnMatchesDiscovered(ReadOnlyCollection<MultiplayerAPI.MatchInformation> matches)
        {
            UFE.multiplayerAPI.OnMatchesDiscovered -= this.OnMatchesDiscovered;
            UFE.multiplayerAPI.OnMatchDiscoveryError -= this.OnMatchDiscoveryError;

            if (matches != null && matches.Count > 0)
            {
                for (int i = 0; i < matches.Count; ++i)
                {
                    if (matches[i] != null)
                    {
                        _foundServers.Add(matches[i]);
                    }
                }
            }
        }

        protected virtual void OnMatchDiscoveryError()
        {
            this.GoToConnectionLostScreen();
        }

        protected virtual void TryConnect()
        {
            // First, we check that we aren't already connected to a client or a server...
            if (!UFE.multiplayerAPI.IsConnected() && !this._connecting)
            {
                MultiplayerAPI.MatchInformation match = null;

                // After that, check if we have found one match with at least one player which isn't already full...
                foreach (MultiplayerAPI.MatchInformation matchInformation in _foundServers)
                {
                    if (matchInformation.matchName == matchName)
                    {
                        match = matchInformation;
                    }
                }

                if (match != null)
                {
                    // In that case, try connecting to that match
                    this._connecting = true;

                    if (!_spectator && match.currentPlayers > 1)
                    {
                        if (UFE.config.debugOptions.connectionLog) Debug.Log("Match is full!");
                        JoinError("Match is full!");
                    }
                    else
                    {
                        UFE.multiplayerAPI.OnJoined += this.OnJoined;
                        UFE.multiplayerAPI.OnJoinError += this.OnJoinError;

                        if (UFE.config.debugOptions.connectionLog) Debug.Log("Match Found! Joining Match...");
                        UFE.multiplayerAPI.JoinMatch(match);
                    }
                }
                else
                {
                    // Otherwise, return match not found
                    if (UFE.config.debugOptions.connectionLog) Debug.Log("Match Not Found.");
                    JoinError("Match Not Found.");
                }
            }
        }
        #endregion
    }
}