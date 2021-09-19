using System;
using System.IO;

namespace UFE3D
{
	public class InputBufferMessage : NetworkMessage<InputBufferMessageContent>
	{
		#region public override methods
		public InputBufferMessage(int playerIndex, long currentFrame, InputBufferMessageContent data) :
		base(NetworkMessageType.InputBuffer, playerIndex, currentFrame, data)
		{ }

		public InputBufferMessage(byte[] serializedNetworkMessage) : base(serializedNetworkMessage)
		{
			if (this.MessageType != NetworkMessageType.InputBuffer)
			{
				throw new System.FormatException(string.Format(
					"The message type was {0}, but it should have been {1}.",
					this.MessageType,
					NetworkMessageType.InputBuffer
				));
			}
		}
		#endregion

		#region protected override methods
		protected override void AddToStream(BinaryWriter writer, InputBufferMessageContent data)
		{
			writer.Write(data.NextExpectedFrame);
			writer.Write(data.InputBuffer != null ? data.InputBuffer.Count : 0);

			if (UFE.config.inputOptions.forceDigitalInput)
			{
				for (int i = 0; i < data.InputBuffer.Count; ++i)
				{
					writer.Write(data.InputBuffer[i].Item1);

					NetworkButtonPress button = data.InputBuffer[i].Item2.buttons;
					if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size8Bits)
					{
						button &= (NetworkButtonPress)((sbyte)(-1));
						writer.Write((byte)button);
					}
					else if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size16Bits)
					{
						button &= (NetworkButtonPress)((short)(-1));
						writer.Write((ushort)button);
					}
					else if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size32Bits)
					{
						writer.Write((uint)button);
					}

					writer.Write(data.InputBuffer[i].Item2.selectedOption);
				}
			}
			else
			{
				for (int i = 0; i < data.InputBuffer.Count; ++i)
				{
					writer.Write(data.InputBuffer[i].Item1);
					writer.Write((float)data.InputBuffer[i].Item2.horizontalAxisRaw);
					writer.Write((float)data.InputBuffer[i].Item2.verticalAxisRaw);

					NetworkButtonPress button = data.InputBuffer[i].Item2.buttons;
					if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size8Bits)
					{
						button &= (NetworkButtonPress)((sbyte)(-1));
						writer.Write((byte)button);
					}
					else if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size16Bits)
					{
						button &= (NetworkButtonPress)((short)(-1));
						writer.Write((ushort)button);
					}
					else if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size32Bits)
					{
						writer.Write((uint)button);
					}

					writer.Write(data.InputBuffer[i].Item2.selectedOption);
				}
			}
		}

		protected override InputBufferMessageContent ReadFromStream(BinaryReader reader)
		{
			long nextExpectedFrame = reader.ReadInt64();
			Tuple<long, FrameInput>[] buffer = new Tuple<long, FrameInput>[reader.ReadInt32()];

			if (UFE.config.inputOptions.forceDigitalInput)
			{
				if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size8Bits)
				{
					for (int i = 0; i < buffer.Length; ++i)
					{
						buffer[i] = new Tuple<long, FrameInput>(
							reader.ReadInt64()
							,
							new FrameInput(
								(NetworkButtonPress)reader.ReadByte(),
								reader.ReadSByte()
							)
						);
					}
				}
				else if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size16Bits)
				{
					for (int i = 0; i < buffer.Length; ++i)
					{
						buffer[i] = new Tuple<long, FrameInput>(
							reader.ReadInt64()
							,
							new FrameInput(
								(NetworkButtonPress)reader.ReadUInt16(),
								reader.ReadSByte()
							)
						);
					}
				}
				else if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size32Bits)
				{
					for (int i = 0; i < buffer.Length; ++i)
					{
						buffer[i] = new Tuple<long, FrameInput>(
							reader.ReadInt64()
							,
							new FrameInput(
								(NetworkButtonPress)reader.ReadUInt32(),
								reader.ReadSByte()
							)
						);
					}
				}
			}
			else
			{
				if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size8Bits)
				{
					for (int i = 0; i < buffer.Length; ++i)
					{
						buffer[i] = new Tuple<long, FrameInput>(
							reader.ReadInt64()
							,
							new FrameInput(
								reader.ReadSingle(),
								reader.ReadSingle(),
								(NetworkButtonPress)reader.ReadByte(),
								reader.ReadSByte()
							)
						);
					}
				}
				else if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size16Bits)
				{
					for (int i = 0; i < buffer.Length; ++i)
					{
						buffer[i] = new Tuple<long, FrameInput>(
							reader.ReadInt64()
							,
							new FrameInput(
								reader.ReadSingle(),
								reader.ReadSingle(),
								(NetworkButtonPress)reader.ReadUInt16(),
								reader.ReadSByte()
							)
						);
					}
				}
				else if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size32Bits)
				{
					for (int i = 0; i < buffer.Length; ++i)
					{
						buffer[i] = new Tuple<long, FrameInput>(
							reader.ReadInt64()
							,
							new FrameInput(
								reader.ReadSingle(),
								reader.ReadSingle(),
								(NetworkButtonPress)reader.ReadUInt32(),
								reader.ReadSByte()
							)
						);
					}
				}
			}

			return new InputBufferMessageContent(nextExpectedFrame, buffer);
		}
		#endregion
	}
}