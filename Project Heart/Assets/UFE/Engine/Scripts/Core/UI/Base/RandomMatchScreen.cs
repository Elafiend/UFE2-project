using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UFE3D
{
	public class RandomMatchScreen : UFEScreen
	{
		#region public instance fields
		public int pageSize = 20;
		#endregion

		#region protected instance field
		protected bool _connecting = false;
		protected int _currentPage = 0;
		protected IList<MultiplayerAPI.MatchInformation> _foundMatches = new List<MultiplayerAPI.MatchInformation>();
		#endregion

		#region public override methods
		public override void OnShow()
		{
			base.OnShow();

			this.StopSearchingMatchGames();

			UFE.multiplayerMode = UFE.MultiplayerMode.Online;
			this._currentPage = 0;
			this.JoinOrCreateMatchGame();
		}
		#endregion

		#region public instance methods
		public virtual void GoToMainMenuScreen()
		{
			this.StopSearchingMatchGames();
			UFE.StartMainMenuScreen();
		}

		public virtual void GoToConnectionLostScreen()
		{
			this.StopSearchingMatchGames();
			UFE.StartConnectionLostScreen();
		}

		public virtual void JoinOrCreateMatchGame()
		{
			this._connecting = true;
			UFE.multiplayerAPI.OnMatchCreated -= this.OnMatchCreated;
			UFE.multiplayerAPI.OnMatchCreationError -= this.OnMatchCreationError;

			UFE.multiplayerAPI.OnMatchesDiscovered += this.OnMatchesDiscovered;
			UFE.multiplayerAPI.OnMatchDiscoveryError += this.OnMatchDiscoveryError;

			UFE.multiplayerAPI.StartSearchingMatches(this._currentPage, this.pageSize, null);
		}

		public virtual void StopSearchingMatchGames()
		{
			UFE.multiplayerAPI.OnMatchesDiscovered -= this.OnMatchesDiscovered;
			UFE.multiplayerAPI.OnMatchDiscoveryError -= this.OnMatchDiscoveryError;

			this._foundMatches.Clear();
			this._connecting = false;
		}
		#endregion

		#region protected instance methods
		protected virtual void OnMatchCreated(MultiplayerAPI.CreatedMatchInformation match)
		{
			UFE.multiplayerAPI.OnMatchCreated -= this.OnMatchCreated;
			UFE.multiplayerAPI.OnMatchCreationError -= this.OnMatchCreationError;

			this.StopSearchingMatchGames();
		}

		protected virtual void OnMatchCreationError()
		{
			UFE.multiplayerAPI.OnMatchCreated -= this.OnMatchCreated;
			UFE.multiplayerAPI.OnMatchCreationError -= this.OnMatchCreationError;

			this.GoToConnectionLostScreen();
		}

		protected virtual void OnMatchesDiscovered(ReadOnlyCollection<MultiplayerAPI.MatchInformation> matches)
		{
			UFE.multiplayerAPI.OnMatchesDiscovered -= this.OnMatchesDiscovered;
			UFE.multiplayerAPI.OnMatchDiscoveryError -= this.OnMatchDiscoveryError;

			if (matches != null)
			{
				for (int i = 0; i < matches.Count; ++i)
				{
					if (matches[i] != null)
					{
						this._foundMatches.Add(matches[i]);

						//					if (matches[i].directConnectInfos != null){
						//						for (int j = 0; j < matches[i].directConnectInfos.Count; ++j){
						//							MatchDirectConnectInfo connectionInfo = matches[i].directConnectInfos[j];
						//
						//							if (connectionInfo != null){
						//								Debug.Log(connectionInfo.privateAddress + "\n" + connectionInfo.publicAddress);
						//							}
						//						}
						//					}
					}
				}
			}

			this.TryConnect();
		}

		protected virtual void OnMatchDiscoveryError()
		{
			UFE.multiplayerAPI.OnMatchesDiscovered -= this.OnMatchesDiscovered;
			UFE.multiplayerAPI.OnMatchDiscoveryError -= this.OnMatchDiscoveryError;

			this.GoToConnectionLostScreen();
		}

		protected virtual void OnJoined(MultiplayerAPI.JoinedMatchInformation match)
		{
			UFE.multiplayerAPI.OnJoined -= this.OnJoined;
			UFE.multiplayerAPI.OnJoinError -= this.OnJoinError;

			UFE.multiplayerMode = UFE.MultiplayerMode.Online;
			this.StopSearchingMatchGames();
		}

		protected virtual void OnJoinError()
		{
			UFE.multiplayerAPI.OnJoined -= this.OnJoined;
			UFE.multiplayerAPI.OnJoinError -= this.OnJoinError;

			// Try to connect to other found matches
			this._connecting = false;
			this.TryConnect();
		}

		protected virtual void OnLanGameNotFound()
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
				while (
					this._foundMatches.Count > 0 &&
					(match == null || match.currentPlayers == 0 || match.currentPlayers >= match.maxPlayers)
				)
				{
					match = this._foundMatches[0];
					this._foundMatches.RemoveAt(0);

					if (match != null && match.currentPlayers > 0 && match.currentPlayers < match.maxPlayers)
					{
						// In that case, try connecting to that match
						this._connecting = true;

						UFE.multiplayerAPI.OnJoined += this.OnJoined;
						UFE.multiplayerAPI.OnJoinError += this.OnJoinError;
						UFE.multiplayerAPI.JoinMatch(match);

						return;
					}
				}





				// Otherwise, create a new match
				this._connecting = true;
				UFE.multiplayerAPI.OnMatchCreated += this.OnMatchCreated;
				UFE.multiplayerAPI.OnMatchCreationError += this.OnMatchCreationError;
				UFE.multiplayerAPI.CreateMatch(new MultiplayerAPI.MatchCreationRequest());

			}
		}
		#endregion
	}
}