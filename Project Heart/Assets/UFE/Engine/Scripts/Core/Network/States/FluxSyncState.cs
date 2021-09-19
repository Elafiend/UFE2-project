using UnityEngine;
using System;
using System.IO;

[Serializable]
public struct FluxSyncState : IEquatable<FluxSyncState>{
	#region public class definitions
	public struct SyncInformation : IEquatable<SyncInformation>{
		public Vector3 data;


		public SyncInformation(Vector3 data) {
			this.data = data;
		}

		public override int GetHashCode (){
			unchecked{
				return 11 * this.data.GetHashCode();
			}
		}

		public override bool Equals (object obj){
			if (obj is SyncInformation){
				return this.Equals((SyncInformation)obj);
			}
			return false;
		}

		public bool Equals(SyncInformation other){
			return Vector3.Equals(this.data, other.data);
		}

		public override string ToString (){
			return string.Format(
				"[P1 anim frame = {0}, P2 anim frame = {1}, Distance = {2})]", 
				this.data.x,
				this.data.y,
				this.data.z
			);
		}
	}
	#endregion

	#region public instance properties
	public SyncInformation syncInfo;

	public long frame;

	#endregion

	#region public instance constructors
	public FluxSyncState(SyncInformation syncInfo, long frame){
		this.syncInfo = syncInfo;
		this.frame = frame;
    }

    public FluxSyncState(Vector3 syncInfo, long frame) {
        this.syncInfo = new SyncInformation(syncInfo);
        this.frame = frame;
    }

    public FluxSyncState(FluxStates state)
	{
		Vector3 info = Vector3.zero;
		Vector3 p1Pos = Vector3.zero;
		Vector3 p2Pos = Vector3.zero;

		foreach (FluxStates.CharacterState charState in state.allCharacterStates)
        {
			if (charState.playerNum == 1)
            {
				info.x += (float)charState.life;
				foreach (FPLibrary.Fix64 gauge in charState.gauges)
					info.x += (float)gauge;
				info.x += (float)charState.moveSet.animator.currentAnimationData.framesPlayed;
				p1Pos += charState.shellTransform.position;
			}
            else
			{
				info.y += (float)charState.life;
				foreach (FPLibrary.Fix64 gauge in charState.gauges)
					info.y += (float)gauge;
				info.y += (float)charState.moveSet.animator.currentAnimationData.framesPlayed;
				p2Pos += charState.shellTransform.position;
			}
        }

		info.z = Vector3.Distance(p1Pos, p2Pos);
		this.syncInfo = new SyncInformation(info);
		this.frame = state.networkFrame;
	}
	#endregion

	#region IEquatable<FluxSimpleState> implementation
	public override bool Equals(object obj){
		return (obj is FluxSyncState) && this.Equals((FluxSyncState)obj);
	}

	public override int GetHashCode (){
		unchecked{
			return 
				11 * this.syncInfo.GetHashCode() + 
				13 * this.frame.GetHashCode();
		}
	}

	public bool Equals(FluxSyncState other){
		return 
			this.syncInfo.Equals(other.syncInfo) && 
			this.frame.Equals(other.frame);
	}
	#endregion

	#region public instance methods
	public byte[] Serialize(){
		return FluxSyncState.Serialize(this);
	}
	#endregion

	#region public override methods
	public override string ToString (){
		return string.Format(
			"[FluxSimpleState | info = {0} | frame = {1}]", 
			this.syncInfo.ToString(), 
			this.frame
		);
	}
	#endregion

	#region public class methods
	public static void AddToStream(BinaryWriter writer, FluxSyncState gameState){
		writer.Write(gameState.syncInfo.data.x);
		writer.Write(gameState.syncInfo.data.y);
		writer.Write(gameState.syncInfo.data.z);
		writer.Write(gameState.frame);
	}


	public static FluxSyncState Deserialize(byte[] bytes){
		using (MemoryStream stream = new MemoryStream(bytes)){
			using (BinaryReader reader = new BinaryReader(stream)){
				return FluxSyncState.ReadFromStream(reader);
			}
		}
	}

	public static FluxSyncState ReadFromStream(BinaryReader reader){
		return new FluxSyncState(new SyncInformation(new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle())), reader.ReadInt64());
	}

	public static byte[] Serialize(FluxSyncState gameState){
		using (MemoryStream stream = new MemoryStream()){
			using (BinaryWriter writer = new BinaryWriter(stream)){
				FluxSyncState.AddToStream(writer, gameState);
				writer.Flush();
				return stream.ToArray();
			}
		}
	}
	#endregion
}
