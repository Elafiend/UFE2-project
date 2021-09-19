using UnityEngine;
using System.Collections.Generic;

namespace UFE3D
{
	public class UFEScreen : MonoBehaviour
	{

		public bool canvasPreview = true;
		//public bool enableUFEInput = false;
		public GameObject firstSelectableGameObject = null;
		public bool hasFadeIn = true;
		public bool hasFadeOut = true;
		public bool wrapInput = true;

		public virtual void DoFixedUpdate(
			IDictionary<InputReferences, InputEvents> player1PreviousInputs,
			IDictionary<InputReferences, InputEvents> player1CurrentInputs,
			IDictionary<InputReferences, InputEvents> player2PreviousInputs,
			IDictionary<InputReferences, InputEvents> player2CurrentInputs
		)
		{ }

		public virtual bool IsVisible()
		{
			return this.gameObject.activeInHierarchy;
		}

		public virtual void OnHide() { }
		public virtual void OnShow()
		{
			//UFE.PauseGame(!enableUFEInput);
		}
		public virtual void SelectOption(int option, int player) { }
	}
}