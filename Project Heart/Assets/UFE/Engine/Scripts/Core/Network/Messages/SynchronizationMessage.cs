using System;
using System.IO;
using System.Text;

namespace UFE3D
{
	public class SynchronizationMessage : NetworkMessage<FluxSyncState>
	{
		#region public override methods
		public SynchronizationMessage(int playerIndex, long currentFrame, FluxSyncState data) :
		base(NetworkMessageType.Syncronization, playerIndex, currentFrame, data)
		{ }

		public SynchronizationMessage(byte[] serializedNetworkMessage) : base(serializedNetworkMessage)
		{
			if (this.MessageType != NetworkMessageType.Syncronization)
			{
				throw new System.FormatException(string.Format(
					"The message type was {0}, but it should have been {1}.",
					this.MessageType,
					NetworkMessageType.Syncronization
				));
			}
		}
		#endregion

		#region protected override methods
		protected override void AddToStream(BinaryWriter writer, FluxSyncState gameState)
		{
			FluxSyncState.AddToStream(writer, gameState);
		}

		protected override FluxSyncState ReadFromStream(BinaryReader reader)
		{
			return FluxSyncState.ReadFromStream(reader);
		}
		#endregion

		#region public override methods
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("{")
				.Append("\"Sync Info\"=\"")
				.Append(this.Data.syncInfo)
				.Append("\"}");

			return string.Format(
				"[{0} | messageType = {1} | playerIndex = {2} | currentFrame = {3} | data = {4}]",
				this.GetType().ToString(),
				this.MessageType,
				this.PlayerIndex,
				this.CurrentFrame,
				sb.ToString()
			);
		}
		#endregion
	}
}