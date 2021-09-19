using UnityEngine;
using System.Collections;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;

public class ConnectionInformation{
	public int port = 0;
	public string publicAddress = null;
	public string privateAddress = null;
	public NodeID unityNodeId = NodeID.Invalid;

	public ConnectionInformation(){}

	public ConnectionInformation(string address, int port) : this(address, address, port){}

	public ConnectionInformation(string privateAddress, string publicAddress, int port) : 
	this (privateAddress, publicAddress, port, NodeID.Invalid){}

	public ConnectionInformation(
		string privateAddress, 
		string publicAddress, 
		int port, 
		NodeID unityNodeId
	){
		this.privateAddress = privateAddress;
		this.publicAddress = publicAddress;
		this.port = port;
		this.unityNodeId = unityNodeId;
	}

#if !UNITY_2019_1_OR_NEWER
    public ConnectionInformation(MatchInfoSnapshot.MatchInfoDirectConnectSnapshot info) : this(
		info.privateAddress,
		info.publicAddress,
		UFE.config.networkOptions.port,
		info.nodeId
	){}
#endif
}
