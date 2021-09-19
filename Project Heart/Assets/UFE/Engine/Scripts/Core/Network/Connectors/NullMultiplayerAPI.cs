namespace UFE3D
{
	public class NullMultiplayerAPI : MultiplayerAPI
	{
		#region public override properties
		public override int Connections
		{
			get
			{
				return 0;
			}
		}

		public override MultiplayerAPI.PlayerInformation Player
		{
			get
			{
				return new MultiplayerAPI.PlayerInformation(null);
			}
		}

		public override float SendRate { get; set; }
		#endregion

		#region public override methods
		// Client
		public override void DisconnectFromMatch()
		{
			this.RaiseOnDisconnection();
		}

		public override void JoinMatch(MatchInformation match, string password = null)
		{
			this.RaiseOnJoinError();
		}

		public override void JoinRandomMatch()
		{
			this.RaiseOnJoinError();
		}

		public override void StartSearchingMatches(int startPage = 0, int pageSize = 20, string filter = null)
		{
			this.RaiseOnMatchDiscoveryError();
		}

		public override void StopSearchingMatches() { }

		// Common
		public override NetworkState GetConnectionState()
		{
			return NetworkState.Disconnected;
		}

		public override int GetLastPing()
		{
			return 0;
		}

		// Server
		public override void CreateMatch(MatchCreationRequest request)
		{
			this.RaiseOnMatchCreationError();
		}

		public override void DestroyMatch()
		{
			this.RaiseOnMatchDestroyed();
		}

		public override void Disconnect()
		{

		}
		#endregion

		#region protected override methods
		protected override bool SendNetworkMessage(byte[] bytes)
		{
			return false;
		}
		#endregion
	}
}