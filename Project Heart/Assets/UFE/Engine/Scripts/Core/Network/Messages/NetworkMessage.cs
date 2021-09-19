using System.IO;

namespace UFE3D
{
	public abstract class NetworkMessage<T>
	{
		#region public instance properties
		public NetworkMessageType MessageType { get; private set; }
		public int PlayerIndex { get; private set; }
		public long CurrentFrame { get; private set; }
		public T Data { get; private set; }
		#endregion

		#region protected instance constructors
		protected NetworkMessage(NetworkMessageType messageType, int playerIndex, long currentFrame, T data)
		{
			this.MessageType = messageType;
			this.PlayerIndex = playerIndex;
			this.CurrentFrame = currentFrame;
			this.Data = data;
		}

		protected NetworkMessage(byte[] serializedNetworkMessage)
		{
			using (MemoryStream stream = new MemoryStream(serializedNetworkMessage))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					// Read the information from the stream...
					this.MessageType = (NetworkMessageType)reader.ReadByte();
					this.PlayerIndex = reader.ReadInt32();
					this.CurrentFrame = reader.ReadInt64();
					this.Data = this.ReadFromStream(reader);
				}
			}
		}
		#endregion

		#region public instance methods
		public byte[] Serialize()
		{
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					// Write the information into the stream...
					writer.Write((byte)this.MessageType);
					writer.Write(this.PlayerIndex);
					writer.Write(this.CurrentFrame);
					this.AddToStream(writer, this.Data);
					writer.Flush();

					// and return the information stored in the stream as a byte[]
					return stream.ToArray();
				}
			}
		}
		#endregion

		#region public override methods
		public override string ToString()
		{
			return string.Format(
				"[{0} | messageType = {1} | playerIndex = {2} | currentFrame = {3} | data = {4}]",
				this.GetType().ToString(),
				this.MessageType,
				this.PlayerIndex,
				this.CurrentFrame,
				this.Data.ToString()
			);
		}
		#endregion

		#region protected instance methods
		protected abstract void AddToStream(BinaryWriter writer, T data);
		protected abstract T ReadFromStream(BinaryReader reader);
		#endregion
	}
}