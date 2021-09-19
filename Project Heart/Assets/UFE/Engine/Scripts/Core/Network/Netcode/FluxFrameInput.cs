namespace UFE3D
{
	public struct FluxFrameInput
	{
		#region public instance fields
		public FrameInput Player1PreviousInput;
		public FrameInput Player1CurrentInput;
		public FrameInput Player2PreviousInput;
		public FrameInput Player2CurrentInput;
		#endregion

		#region public instance constructors
		public FluxFrameInput(
			FrameInput player1PreviousInput,
			FrameInput player1CurrentInput,
			FrameInput player2PreviousInput,
			FrameInput player2CurrentInput
		)
		{
			this.Player1PreviousInput = player1PreviousInput;
			this.Player1CurrentInput = player1CurrentInput;
			this.Player2PreviousInput = player2PreviousInput;
			this.Player2CurrentInput = player2CurrentInput;
		}
		#endregion
	}
}