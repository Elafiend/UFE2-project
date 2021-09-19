using System;
using System.Collections.Generic;
using System.Text;

namespace UFE3D
{
	public class InputBufferMessageContent
	{
		#region public instance properties
		public long NextExpectedFrame { get; private set; }
		public IList<Tuple<long, FrameInput>> InputBuffer { get; private set; }
		#endregion

		#region public override methods
		public InputBufferMessageContent(long nextExpectedFrame, IList<Tuple<long, FrameInput>> inputBuffer)
		{
			this.NextExpectedFrame = nextExpectedFrame;
			this.InputBuffer = inputBuffer ?? new List<Tuple<long, FrameInput>>();
		}
		#endregion

		#region public override methods
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("{");
			for (int i = 0; i < this.InputBuffer.Count; ++i)
			{
				if (sb.Length > 0)
				{
					sb.Append(", ");
				}

				sb.Append("\"").Append(this.InputBuffer[i].Item1).Append("\":\"")
					.Append(this.InputBuffer[i].Item2).Append("\"");
			}
			sb.Append("}");

			return string.Format(
				"[{0} | nextExpectedFrame = {1} | inputBuffer = {2}]",
				this.GetType().ToString(),
				this.NextExpectedFrame,
				sb.ToString()
			);
		}
		#endregion
	}
}