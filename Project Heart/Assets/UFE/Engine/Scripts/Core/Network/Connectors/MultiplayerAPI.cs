using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine.Networking.Types;

namespace UFE3D
{
	public struct BasicResponse
	{
		public bool success
		{
			get
			{
				return this._success;
			}
		}

		private bool _success;

		public BasicResponse(bool success)
		{
			this._success = success;
		}
	}

	public struct CreateMatchResponse
	{
		public NetworkAccessToken accessTokenString
		{
			get
			{
				return this._accessTokenString;
			}
		}

		public NetworkID networkId
		{
			get
			{
				return this._networkId;
			}
		}

		public NodeID nodeId
		{
			get
			{
				return this._nodeId;
			}
		}

		public bool success
		{
			get
			{
				return this._success;
			}
		}

		private NetworkAccessToken _accessTokenString;
		private NetworkID _networkId;
		private NodeID _nodeId;
		private bool _success;

		public CreateMatchResponse(bool success, NetworkID networkId, NodeID nodeId, NetworkAccessToken accessTokenString)
		{
			this._success = success;
			this._networkId = networkId;
			this._nodeId = nodeId;
			this._accessTokenString = accessTokenString;
		}
	}

	public struct JoinMatchResponse
	{
		public string accessTokenString
		{
			get
			{
				return this._accessTokenString;
			}
		}

		public bool success
		{
			get
			{
				return this._success;
			}
		}


		private string _accessTokenString;
		private bool _success;

		public JoinMatchResponse(bool success, string accessTokenString)
		{
			this._success = success;
			this._accessTokenString = accessTokenString;
		}
	}

	public class ListMatchResponse
	{
		public bool success;
		public object[] matches;
	}

	public abstract class MultiplayerAPI : MonoBehaviour
	{
		#region class definitions
		public class MatchCreationRequest
		{
			public string matchName = null;
			public int maxPlayers = 2;
			public string password = null;
			public int port = 0;
			public bool publicMatch = true;

			public MatchCreationRequest(
				string matchName = null,
				int maxPlayers = 2,
				bool publicMatch = true,
				string password = null
			) : this(UFE.config.networkOptions.port, matchName, maxPlayers, publicMatch, password) { }

			public MatchCreationRequest(
				int port,
				string matchName = null,
				int maxPlayers = 2,
				bool publicMatch = true,
				string password = null
			)
			{
				this.matchName = matchName;
				this.maxPlayers = maxPlayers;
				this.password = password;
				this.port = port;
				this.publicMatch = publicMatch;
			}
		}

		public class CreatedMatchInformation
		{
			public string matchName = null;
			public NodeID unityHostNodeId = NodeID.Invalid;
			public NetworkID unityNetworkId = NetworkID.Invalid;

			public CreatedMatchInformation() : this(null, NetworkID.Invalid, NodeID.Invalid) { }

			public CreatedMatchInformation(string matchName) : this(matchName, NetworkID.Invalid, NodeID.Invalid) { }

			public CreatedMatchInformation(CreateMatchResponse response) : this(
				response.accessTokenString.GetByteString(),
				response.networkId,
				response.nodeId
			)
			{ }

			public CreatedMatchInformation(string matchName, NetworkID unityNetworkId, NodeID unityHostNodeId)
			{
				this.matchName = matchName;
				this.unityHostNodeId = unityHostNodeId;
				this.unityNetworkId = unityNetworkId;
			}
		}

		public class JoinedMatchInformation
		{
			public string matchName = null;
			public NodeID unityHostNodeId = NodeID.Invalid;
			public NetworkID unityNetworkId = NetworkID.Invalid;

			public JoinedMatchInformation() : this(null, NetworkID.Invalid, NodeID.Invalid) { }

			public JoinedMatchInformation(string matchName) : this(matchName, NetworkID.Invalid, NodeID.Invalid) { }

			public JoinedMatchInformation(JoinMatchResponse response) : this(
				response.accessTokenString
			)
			{ }

			public JoinedMatchInformation(string matchName, NetworkID unityNetworkId, NodeID unityHostNodeId)
			{
				this.matchName = matchName;
				this.unityHostNodeId = unityHostNodeId;
				this.unityNetworkId = unityNetworkId;
			}
		}

		public class MatchInformation
		{
			private List<ConnectionInformation> _connections = new List<ConnectionInformation>();
			public IList<ConnectionInformation> connections
			{
				get
				{
					return this._connections;
				}
			}

			public int averageEloScore = 0;
			public int currentPlayers = 0;
			public bool isPublic = true;
			public string matchName = null;
			public int maxPlayers = 2;
			public NodeID unityHostNodeId = NodeID.Invalid;
			public NetworkID unityNetworkId = NetworkID.Invalid;


			public MatchInformation() { }

			public MatchInformation(string address) : this(address, UFE.config.networkOptions.port) { }

			public MatchInformation(string address, int port)
			{
				this.connections.Add(new ConnectionInformation(address, port));
			}

			public MatchInformation(CreatedMatchInformation match)
			{
				this.unityHostNodeId = match.unityHostNodeId;
				this.unityNetworkId = match.unityNetworkId;
			}
		}

		public class PlayerInformation
		{
			public object networkIdentity { get; private set; }

			public PlayerInformation(object networkIdentity)
			{
				if (networkIdentity == null)
				{
					throw new ArgumentNullException();
				}

				this.networkIdentity = networkIdentity;
			}
		}
		#endregion


		#region public delegate definitions: Common Delegates
		public delegate void OnInitializationErrorDelegate();
		public delegate void OnInitializationSuccessfulDelegate();
		public delegate void OnMessageReceivedDelegate(byte[] bytes);
		#endregion

		#region public delegate definitions: Client Delegates
		public delegate void OnDisconnectionDelegate();
		public delegate void OnJoinedDelegate(JoinedMatchInformation match);
		public delegate void OnJoinErrorDelegate();
		public delegate void OnMatchesDiscoveredDelegate(ReadOnlyCollection<MatchInformation> matches);
		public delegate void OnMatchDiscoveryErrorDelegate();
		#endregion

		#region public delegate definitions: Server Delegates
		public delegate void OnMatchCreatedDelegate(CreatedMatchInformation match);
		public delegate void OnMatchCreationErrorDelegate();
		public delegate void OnMatchDestroyedDelegate();
		public delegate void OnPlayerConnectedToMatchDelegate(PlayerInformation player);
		public delegate void OnPlayerDisconnectedFromMatchDelegate(PlayerInformation player);
		#endregion

		#region public event definitions: Common Events
		public event OnInitializationErrorDelegate OnInitializationError;
		public event OnInitializationSuccessfulDelegate OnInitializationSuccessful;
		public event OnMessageReceivedDelegate OnMessageReceived;
		#endregion

		#region public class event definitions: Client Events
		public event OnDisconnectionDelegate OnDisconnection;
		public event OnJoinedDelegate OnJoined;
		public event OnJoinErrorDelegate OnJoinError;
		public event OnMatchesDiscoveredDelegate OnMatchesDiscovered;
		public event OnMatchDiscoveryErrorDelegate OnMatchDiscoveryError;
		#endregion

		#region public event definitions: Server Events
		public event OnMatchCreatedDelegate OnMatchCreated;
		public event OnMatchCreationErrorDelegate OnMatchCreationError;
		public event OnMatchDestroyedDelegate OnMatchDestroyed;
		public event OnPlayerConnectedToMatchDelegate OnPlayerConnectedToMatch;
		public event OnPlayerDisconnectedFromMatchDelegate OnPlayerDisconnectedFromMatch;
		#endregion

		#region public abstract properties
		public abstract int Connections { get; }
		public abstract PlayerInformation Player { get; }
		public abstract float SendRate { get; set; }
		#endregion

		#region private instance fields
		protected string _uuid = null;
		#endregion

		#region public instance methods
		public virtual void Initialize(string uuid)
		{
			if (uuid != null)
			{
				this._uuid = uuid;
				this.RaiseOnInitializationSuccessful();
			}
			else
			{
				this.RaiseOnInitializationError();
			}
		}
		#endregion

		#region public abstract methods
		// Client
		public abstract void DisconnectFromMatch();
		public abstract void JoinMatch(MatchInformation match, string password = null);
		public abstract void JoinRandomMatch();
		public abstract void StartSearchingMatches(int startPage = 0, int pageSize = 20, string filter = null);
		public abstract void StopSearchingMatches();

		// Common
		public abstract NetworkState GetConnectionState();
		public abstract int GetLastPing();

		// Server
		public abstract void CreateMatch(MatchCreationRequest request);
		public abstract void DestroyMatch();
		public abstract void Disconnect();
		#endregion

		#region public instance methods
		public bool IsClient()
		{
			return this.GetConnectionState() == NetworkState.Client;
		}

		public bool IsConnected()
		{
			return this.GetConnectionState() != NetworkState.Disconnected;
		}

		public bool IsServer()
		{
			return this.GetConnectionState() == NetworkState.Server;
		}

		public bool SendNetworkMessage<T>(NetworkMessage<T> message)
		{
			return this.SendNetworkMessage(message.Serialize());
		}
		#endregion

		#region protected abstract methods
		protected abstract bool SendNetworkMessage(byte[] bytes);
		#endregion

		#region protected instance methods: Common Events
		protected virtual void RaiseOnInitializationError()
		{
			this.OnInitializationError?.Invoke();
		}

		protected virtual void RaiseOnInitializationSuccessful()
		{
			this.OnInitializationSuccessful?.Invoke();
		}

		protected virtual void RaiseOnMessageReceived(byte[] bytes)
		{
			this.OnMessageReceived?.Invoke(bytes);
		}
		#endregion

		#region protected instance methods: Client Events
		protected virtual void RaiseOnDisconnection()
		{
			this.OnDisconnection?.Invoke();
		}

		protected virtual void RaiseOnMatchesDiscovered(ReadOnlyCollection<MatchInformation> matches)
		{
			this.OnMatchesDiscovered?.Invoke(matches);
		}

		protected virtual void RaiseOnMatchDiscoveryError()
		{
			this.OnMatchDiscoveryError?.Invoke();
		}

		protected virtual void RaiseOnJoined(JoinedMatchInformation match)
		{
			this.OnJoined?.Invoke(match);
		}

		protected virtual void RaiseOnJoinError()
		{
			this.OnJoinError?.Invoke();
		}
		#endregion

		#region protected instance methods: Server Events
		protected virtual void RaiseOnMatchCreated(CreatedMatchInformation match)
		{
			this.OnMatchCreated?.Invoke(match);
		}

		protected virtual void RaiseOnMatchCreationError()
		{
			this.OnMatchCreationError?.Invoke();
		}

		protected virtual void RaiseOnMatchDestroyed()
		{
			this.OnMatchDestroyed?.Invoke();
		}

		protected virtual void RaiseOnPlayerConnectedToMatch(PlayerInformation player)
		{
			if (UFE.config.debugOptions.connectionLog) Debug.Log("MultiplayerAPI.RaiseOnPlayerConnectedToMatch");
			this.OnPlayerConnectedToMatch?.Invoke(player);
		}

		protected virtual void RaiseOnPlayerDisconnectedFromMatch(PlayerInformation player)
		{
			if (UFE.config.debugOptions.connectionLog) Debug.Log("MultiplayerAPI.RaiseOnPlayerDisconnectedFromMatch");
			this.OnPlayerDisconnectedFromMatch?.Invoke(player);
		}
		#endregion
	}
}