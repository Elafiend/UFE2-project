using System;
using System.Reflection;
using UnityEngine;

namespace UFE3D
{
	///--------------------------------------------------------------------------------------------------------------------
	/// <summary>
	/// This class tries to read the player input using Control-Freak's TouchController:
	/// https://www.assetstore.unity3d.com/#/content/11562
	/// 
	/// If Control-Freak's TouchController is not available, it will use cInput or the Unity Input instead.
	/// </summary>
	///--------------------------------------------------------------------------------------------------------------------
	public class InputTouchController : InputController
	{
		#region public instance properties
		public float deadZone = 0f;
		#endregion

		#region public instance properties
		protected bool useControlFreak = false;
		protected InputTouchControllerBridge touchControllerBridge = null;      // [DGT]
		#endregion

		#region public overriden methods
		public override InputEvents ReadInput(InputReferences inputReference)
		{
			InputEvents ev = base.ReadInput(inputReference);

			if (this.useControlFreak && inputReference.inputType != InputType.Button && Mathf.Abs((float)ev.axisRaw) < this.deadZone)
			{
				return new InputEvents(0f);
			}

			return ev;
		}
		#endregion

		#region protected overriden methods
		protected override void SelectInputType()
		{

			// [DGT]
			// First, look for Control Freak 2 rig with UFE Bridge component...

			InputTouchControllerBridge bridge =
				GameObject.FindObjectOfType<InputTouchControllerBridge>();

			if (bridge != null)
			{
				this.InitializeTouchControllerBridge(bridge);
				return;
			}

			// Then, look for Control Freak 1.x controller...

			else
			{
				Type type = UFE.SearchClass("TouchController");
				UnityEngine.Object touchController = null;

				if ((type != null) && ((touchController = GameObject.FindObjectOfType(type)) != null))
				{
					this.InitializeControlFreakTouchController(touchController);
					return;
				}
			}

			// If nothing found, use standard Input...

			base.SelectInputType();

		}
		#endregion

		#region protected instance methods

		// [DGT]
		// Init Control Freak 2 bridge.

		protected void InitializeTouchControllerBridge(InputTouchControllerBridge bridge)
		{
			this.touchControllerBridge = bridge;
			this.touchControllerBridge.Init();

			this.touchControllerBridge.ShowBattleControls(false, false);            //  Start with battle controls hidden.

			this.getAxis = this.touchControllerBridge.GetAxis;
			this.getAxisRaw = this.touchControllerBridge.GetAxisRaw;
			this.getButton = this.touchControllerBridge.GetButton;
		}


		protected virtual void InitializeControlFreakTouchController(UnityEngine.Object touchController)
		{
			if (touchController != null)
			{
				Type inputType = touchController.GetType();

				if (inputType != null)
				{
					this.deadZone = UFE.config.inputOptions.controlFreakDeadZone;
					this.useControlFreak = true;

					// Retrieve the required methods using the Reflection API to avoid 
					// compilation errors if Control-Freak's TouchController hasn't been 
					// imported into the project. We will cache the method information 
					// to call these methods later
					MethodInfo getAxisInfo = inputType.GetMethod(
						"GetAxis",
						BindingFlags.Instance | BindingFlags.Public,
						null,
						new Type[] { typeof(string) },
						null
					);

					if (getAxisInfo != null)
					{
						this.getAxis = delegate (string axis)
						{
							return (float)getAxisInfo.Invoke(touchController, new object[] { axis });
						};
					}

					MethodInfo getAxisRawInfo = inputType.GetMethod(
						"GetAxisRaw",
						BindingFlags.Instance | BindingFlags.Public,
						null,
						new Type[] { typeof(string) },
						null
					);

					if (getAxisRawInfo != null)
					{
						this.getAxisRaw = delegate (string axis)
						{
							return (float)getAxisRawInfo.Invoke(touchController, new object[] { axis });
						};
					}


					MethodInfo getButtonInfo = inputType.GetMethod(
						"GetButton",
						BindingFlags.Instance | BindingFlags.Public,
						null,
						new Type[] { typeof(string) },
						null
					);

					if (getButtonInfo != null)
					{
						this.getButton = delegate (string button)
						{
							return (bool)getButtonInfo.Invoke(touchController, new object[] { button });
						};
					}
				}
			}
		}
		#endregion
	}
}