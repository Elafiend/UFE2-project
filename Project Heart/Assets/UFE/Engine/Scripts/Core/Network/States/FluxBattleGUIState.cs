using System.Collections.Generic;

namespace UFE3D
{
	public class FluxBattleGUIState
	{
		#region public instance properties
		//public List<List<Image>> player1ButtonPresses{get; set;}
		//public List<List<Image>> player2ButtonPresses{get; set;}

		public List<InputReferences[]> player1InputReferences { get; set; }
		public List<InputReferences[]> player2InputReferences { get; set; }
		#endregion
	}
}