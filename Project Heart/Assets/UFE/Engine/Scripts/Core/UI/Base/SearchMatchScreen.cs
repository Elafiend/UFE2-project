using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UFE3D
{
    public class SearchMatchScreen : UFEScreen
    {
        #region public instance fields
        public int pageSize = 20;
        public int searchDelay = 180;
        public int maxSearchTimes = 3;
        public UFE.MultiplayerMode multiplayerMode;
        #endregion

        #region protected instance field
        protected bool _connecting = false;
        protected int _currentPage = 0;
        protected int _currentSearchTime = 0;
        protected IList<MultiplayerAPI.MatchInformation> _foundMatches = new List<MultiplayerAPI.MatchInformation>();
        protected IList<MultiplayerAPI.MatchInformation> _triedMatches = new List<MultiplayerAPI.MatchInformation>();
        protected MultiplayerAPI.MatchInformation current_match = null;
        #endregion

        #region public override methods
        public override void OnShow()
        {
            base.OnShow();

            //this.StopSearchingMatchGames();

            UFE.multiplayerMode = multiplayerMode;
            this._currentPage = 0;
            this._currentSearchTime = 0;

            //UFE.onlineMultiplayerAPI.Disconnect();
            //UFE.onlineMultiplayerAPI.Initialize(UFE.config.gameName ?? "UFE");
            //UFE.multiplayerAPI.OnInitializationSuccessful -= this.OnInitializationSuccessful;
            //UFE.multiplayerAPI.OnInitializationSuccessful += this.OnInitializationSuccessful;
            this.StartSearchingGames();
        }
        #endregion

        /*protected virtual void OnInitializationSuccessful()
        {
            this.StartSearchingGames();
        }*/

        #region public instance methods
        public virtual void GoToMainMenuScreen()
        {
            UFE.DisconnectFromGame();
            this.StopSearchingMatchGames();
            //UFE.EnsureNetworkDisconnection();
            UFE.StartMainMenuScreen();
        }

        public virtual void GoToNetworkOptionsScreen()
        {
            UFE.DisconnectFromGame();
            this.StopSearchingMatchGames();
            UFE.StartNetworkOptionsScreen();
        }

        public virtual void GoToConnectionLostScreen()
        {
            this.StopSearchingMatchGames();
            //UFE.EnsureNetworkDisconnection();
            UFE.StartConnectionLostScreen();
        }

        public virtual void StartSearchingGames()
        {
            UFE.multiplayerAPI.OnMatchesDiscovered -= this.OnMatchesDiscovered;
            UFE.multiplayerAPI.OnMatchesDiscovered += this.OnMatchesDiscovered;
            UFE.multiplayerAPI.OnMatchDiscoveryError -= this.OnMatchDiscoveryError;
            UFE.multiplayerAPI.OnMatchDiscoveryError += this.OnMatchDiscoveryError;

            UFE.multiplayerAPI.StartSearchingMatches(this._currentPage, this.pageSize, null);
        }

        public virtual void StopSearchingMatchGames(bool enforce = true)
        {
            this._connecting = false;
            UFE.multiplayerAPI.OnMatchesDiscovered -= this.OnMatchesDiscovered;
            UFE.multiplayerAPI.OnMatchDiscoveryError -= this.OnMatchDiscoveryError;

            if (enforce) UFE.FindAndRemoveDelayLocalAction(StartSearchingGames);
            UFE.multiplayerAPI.StopSearchingMatches();
            this._foundMatches.Clear();
        }
        #endregion

        #region protected instance methods
        protected virtual void OnMatchCreated(MultiplayerAPI.CreatedMatchInformation match)
        {
            UFE.multiplayerAPI.OnMatchCreated -= this.OnMatchCreated;
            UFE.multiplayerAPI.OnMatchCreationError -= this.OnMatchCreationError;
            UFE.multiplayerAPI.OnPlayerConnectedToMatch -= this.OnPlayerConnectedToMatch;
            UFE.multiplayerAPI.OnPlayerConnectedToMatch += this.OnPlayerConnectedToMatch;

            this.StopSearchingMatchGames();
            this.current_match = new MultiplayerAPI.MatchInformation(match);
            this._triedMatches.Add(this.current_match);

            if (UFE.config.networkOptions.networkService == NetworkService.Unity)
            {
                if (UFE.config.debugOptions.connectionLog) Debug.Log("Match Created: " + match.unityNetworkId);
            }
            else
            {
                if (UFE.config.debugOptions.connectionLog) Debug.Log("Match Created: " + match.matchName);
            }
            if (UFE.config.debugOptions.connectionLog) Debug.Log("Waiting for opponent...");
        }

        protected virtual void OnMatchCreationError()
        {
            UFE.multiplayerAPI.OnMatchCreated -= this.OnMatchCreated;
            UFE.multiplayerAPI.OnMatchCreationError -= this.OnMatchCreationError;

            //this.GoToConnectionLostScreen();
            if (UFE.config.debugOptions.connectionLog) Debug.Log("OnMatchCreationError");
        }

        protected virtual void OnMatchesDiscovered(ReadOnlyCollection<MultiplayerAPI.MatchInformation> matches)
        {
            int unique = 0;
            if (matches != null)
            {
                for (int i = 0; i < matches.Count; ++i)
                {
                    if (matches[i] != null)
                    {
                        bool duplicate = false;
                        for (int f = 0; f < _foundMatches.Count; f++)
                        {
                            if (_foundMatches[f].unityNetworkId == matches[i].unityNetworkId)
                                duplicate = true;
                        }
                        for (int t = 0; t < _triedMatches.Count; t++)
                        {
                            if (_triedMatches[t].unityNetworkId == matches[i].unityNetworkId)
                                duplicate = true;
                        }

                        if (UFE.config.networkOptions.networkService == NetworkService.Photon)
                        {
                            duplicate = false;
                        }


                        if (duplicate)
                        {
                            if (UFE.config.networkOptions.networkService == NetworkService.Unity)
                            {
                                if (UFE.config.debugOptions.connectionLog) Debug.Log("Match Found: " + matches[i].unityNetworkId + " [duplicate]");
                            }
                            else
                            {
                                if (UFE.config.debugOptions.connectionLog) Debug.Log("Match Found: " + matches[i].matchName + " [duplicate]");
                            }
                        }
                        else
                        {
                            if (UFE.config.networkOptions.networkService == NetworkService.Unity)
                            {
                                if (UFE.config.debugOptions.connectionLog) Debug.Log("Match Found: " + matches[i].unityNetworkId);
                            }
                            else
                            {
                                if (UFE.config.debugOptions.connectionLog) Debug.Log("Match Found: " + matches[i].matchName);
                            }

                            this._foundMatches.Add(matches[i]);
                            unique++;
                        }
                    }
                }
                if (UFE.config.debugOptions.connectionLog) Debug.Log("Matches Found (available/total): " + unique + "/" + matches.Count);
            }

            if (unique > 0 || _currentSearchTime >= maxSearchTimes)
            {
                this.TryConnect();
            }
            else
            {
                UFE.DelayLocalAction(StartSearchingGames, searchDelay);
                _currentSearchTime++;
            }
            this.StopSearchingMatchGames(false);
        }

        protected virtual void OnMatchDiscoveryError()
        {
            UFE.multiplayerAPI.OnMatchesDiscovered -= this.OnMatchesDiscovered;
            UFE.multiplayerAPI.OnMatchDiscoveryError -= this.OnMatchDiscoveryError;

            this.GoToConnectionLostScreen();
            if (UFE.config.debugOptions.connectionLog) Debug.Log("OnMatchDiscoveryError");
        }

        protected virtual void OnJoined(MultiplayerAPI.JoinedMatchInformation match)
        {
            UFE.multiplayerAPI.OnJoined -= this.OnJoined;
            UFE.multiplayerAPI.OnJoinError -= this.OnJoinError;
            //UFE.multiplayerAPI.OnPlayerDisconnectedFromMatch += UFE.OnPlayerDisconnectedFromMatch;

            UFE.multiplayerMode = UFE.MultiplayerMode.Online;
            this.StopSearchingMatchGames();

            if (UFE.config.debugOptions.connectionLog) Debug.Log("(OnJoined) Match Starting...");
            UFE.StartNetworkGame(0.5f, 2, false);
        }

        protected virtual void OnJoinError()
        {
            UFE.multiplayerAPI.OnJoined -= this.OnJoined;
            UFE.multiplayerAPI.OnJoinError -= this.OnJoinError;

            // Try to connect to other found matches
            this._connecting = false;
            this.TryConnect();
        }

        protected virtual void OnPlayerConnectedToMatch(MultiplayerAPI.PlayerInformation player)
        {
            UFE.multiplayerAPI.OnPlayerConnectedToMatch -= this.OnPlayerConnectedToMatch;
            //UFE.multiplayerAPI.OnPlayerDisconnectedFromMatch += UFE.OnPlayerDisconnectedFromMatch;

            if (UFE.config.debugOptions.connectionLog) Debug.Log("(OnPlayerConnectedToMatch) Match Starting...");
            UFE.StartNetworkGame(0.5f, 1, false);
        }

        protected virtual void OnPlayerDisconnectedFromMatch(MultiplayerAPI.PlayerInformation player)
        {
            UFE.multiplayerAPI.OnPlayerDisconnectedFromMatch -= this.OnPlayerDisconnectedFromMatch;

            this.GoToConnectionLostScreen();
            if (UFE.config.debugOptions.connectionLog) Debug.Log("OnPlayerDisconnectedFromMatch");
        }

        protected virtual void TryConnect()
        {
            // First, we check that we aren't already connected to a client or a server...
            if (!UFE.multiplayerAPI.IsConnected() && !this._connecting)
            {

                if (UFE.config.debugOptions.connectionLog) Debug.Log("Connecting...");
                MultiplayerAPI.MatchInformation match = null;

                // After that, check if we have found one match with at least one player which isn't already full...
                while (this._foundMatches.Count > 0)
                {
                    match = this._foundMatches[0];
                    this._foundMatches.RemoveAt(0);
                    this._triedMatches.Add(match);

                    if (match != null && match.currentPlayers > 0 && match.currentPlayers < match.maxPlayers)
                    {
                        // In that case, try connecting to that match
                        this._connecting = true;

                        UFE.multiplayerAPI.OnJoined -= this.OnJoined;
                        UFE.multiplayerAPI.OnJoined += this.OnJoined;
                        UFE.multiplayerAPI.OnJoinError -= this.OnJoinError;
                        UFE.multiplayerAPI.OnJoinError += this.OnJoinError;

                        if (UFE.config.debugOptions.connectionLog) Debug.Log("Match Found! Joining Match...");
                        UFE.multiplayerAPI.JoinMatch(match);

                        return;
                    }
                }

                // Otherwise, create a new match
                this._connecting = true;
                UFE.multiplayerAPI.OnMatchCreated -= this.OnMatchCreated;
                UFE.multiplayerAPI.OnMatchCreated += this.OnMatchCreated;
                UFE.multiplayerAPI.OnMatchCreationError -= this.OnMatchCreationError;
                UFE.multiplayerAPI.OnMatchCreationError += this.OnMatchCreationError;
                UFE.multiplayerAPI.CreateMatch(new MultiplayerAPI.MatchCreationRequest());

                if (UFE.config.debugOptions.connectionLog) Debug.Log("No Matches Found. Creating Match...");
            }
        }
        #endregion
    }
}