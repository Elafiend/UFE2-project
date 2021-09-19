using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UFE3D
{
	///--------------------------------------------------------------------------------------------------------------------
	/// <summary>
	/// This class tries to read the player input using cInput:
	/// https://www.assetstore.unity3d.com/#/content/3129
	/// 
	/// If cInput is not available, it will use the Unity Input instead.
	/// </summary>
	///--------------------------------------------------------------------------------------------------------------------
	public class InputController : AbstractInputController
	{
		#region public instance properties
		//-----------------------------------------------------------------------------
		// TODO: This value should be read from cInput
		protected string None = "None";
		//-----------------------------------------------------------------------------
		#endregion

		#region protected instance properties
		protected Func<string, float> getAxis = null;
		protected Func<string, float> getAxisRaw = null;
		protected Func<string, bool> getButton = null;
		protected bool inputManager = false;
		#endregion

		#region public overriden methods 
		public override void Initialize(IEnumerable<InputReferences> inputs)
		{
			base.Initialize(inputs);
			this.SelectInputType();
		}

		public override InputEvents ReadInput(InputReferences inputReference)
		{
			if (inputReference != null)
			{
				string buttonName = inputReference.inputButtonName;
				string joystickAxisName = inputReference.joystickAxisName;

				if (
					inputReference.inputType == InputType.HorizontalAxis ||
					inputReference.inputType == InputType.VerticalAxis
				)
				{
					float axisRaw = this.getAxisRaw(buttonName);

					if (this.inputManager && !string.IsNullOrEmpty(joystickAxisName))
					{
						axisRaw += this.getAxisRaw(joystickAxisName);
					}

					// If we try to read the axis value as if it were a button,
					// it will return count as pressed if the value of the axis is not zero
					return new InputEvents(axisRaw);
				}
				else
				{
					return new InputEvents(this.getButton(buttonName));
				}
			}
			else
			{
				return InputEvents.Default;
			}
		}
		#endregion

		#region protected instance methods
		protected virtual void SelectInputType()
		{
			// Check if we have already selected if we are going to use CInput or the built-in Unity Input
			if (this.getAxis == null)
			{
				// If we haven't made a decision yet, check if CInput is installed
				if (UFE.isCInputInstalled && UFE.config.inputOptions.inputManagerType == InputManagerType.cInput)
				{
					this.InitializeCInput();
				}
				else
				{
					this.InitializeInput();
				}
			}
		}

		protected virtual void InitializeInput()
		{
			// Otherwise, use the built-in Unity Input
			if (this.getAxis == null)
			{
				this.getAxis = Input.GetAxis;
			}

			if (this.getAxisRaw == null)
			{
				this.getAxisRaw = Input.GetAxisRaw;
			}

			if (this.getButton == null)
			{
				this.getButton = Input.GetButton;
			}

			this.inputManager = true;
		}

		protected virtual void InitializeCInput()
		{
			// If cInput is defined, use cInput
			Type inputType = UFE.SearchClass("cInput");

			if (inputType != null)
			{
				// Retrieve the required methods using the Reflection API to avoid 
				// compilation errors if cInput hasn't been imported into the project
				// We will cache the method information to call these methods later
				MethodInfo getAxisInfo = inputType.GetMethod(
					"GetAxis",
					BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy,
					null,
					new Type[] { typeof(string) },
					null
				);

				if (getAxisInfo != null)
				{
					this.getAxis = delegate (string axis)
					{
						return (float)getAxisInfo.Invoke(null, new object[] { axis });
					};
				}

				MethodInfo getAxisRawInfo = inputType.GetMethod(
					"GetAxisRaw",
					BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy,
					null,
					new Type[] { typeof(string) },
					null
				);

				if (getAxisRawInfo != null)
				{
					this.getAxisRaw = delegate (string axis)
					{
						return (float)getAxisRawInfo.Invoke(null, new object[] { axis });
					};
				}


				MethodInfo getButtonInfo = inputType.GetMethod(
					"GetButton",
					BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy,
					null,
					new Type[] { typeof(string) },
					null
				);

				if (getButtonInfo != null)
				{
					this.getButton = delegate (string button)
					{
						return (bool)getButtonInfo.Invoke(null, new object[] { button });
					};
				}


				MethodInfo setAxisInfo = inputType.GetMethod(
					"SetAxis",
					BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy,
					null,
					new Type[] { typeof(string), typeof(string), typeof(string) },
					null
				);

				Action<string, string, string> setAxis = delegate (string axis, string negativeButton, string positiveButton)
				{
					setAxisInfo.Invoke(null, new object[] { axis, negativeButton, positiveButton });
				};


				MethodInfo setKeyInfo = inputType.GetMethod(
					"SetKey",
					BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy,
					null,
					new Type[] { typeof(string), typeof(string), typeof(string) },
					null
				);

				Action<string, string, string> setKey = delegate (string key, string primary, string secondary)
				{
					setKeyInfo.Invoke(null, new object[] { key, primary, secondary });
				};


				MethodInfo isAxisDefinedInfo = inputType.GetMethod(
					"IsAxisDefined",
					BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy,
					null,
					new Type[] { typeof(string) },
					null
				);

				Func<string, bool> isAxisDefined = delegate (string axis)
				{
					return (bool)isAxisDefinedInfo.Invoke(null, new object[] { axis });
				};


				MethodInfo isKeyDefinedInfo = inputType.GetMethod(
					"IsKeyDefined",
					BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy,
					null,
					new Type[] { typeof(string) },
					null
				);

				Func<string, bool> isKeyDefined = delegate (string key)
				{
					return (bool)isKeyDefinedInfo.Invoke(null, new object[] { key });
				};

				PropertyInfo allowDuplicatesInfo = inputType.GetProperty("allowDuplicates");
				allowDuplicatesInfo.SetValue(
					null,
					Convert.ChangeType(UFE.config.inputOptions.cInputAllowDuplicates, allowDuplicatesInfo.PropertyType),
					null
				);

				inputType.GetField("gravity").SetValue(null, UFE.config.inputOptions.cInputGravity);
				inputType.GetField("sensitivity").SetValue(null, UFE.config.inputOptions.cInputSensitivity);
				inputType.GetField("deadzone").SetValue(null, UFE.config.inputOptions.cInputDeadZone);


				// Iterate over all the input references...
				foreach (InputReferences input in this.inputReferences)
				{
					// Check the type of input...
					if (input.inputType == InputType.Button)
					{
						// If this input reference represents the vertical axis,
						// check if the reference is defined in cInput...
						if (!isKeyDefined(input.inputButtonName))
						{
							string defaultKey = input.cInputPositiveDefaultKey;
							string alternativeKey = input.cInputPositiveAlternativeKey;

							if (string.IsNullOrEmpty(defaultKey))
							{
								defaultKey = this.None;
							}

							if (string.IsNullOrEmpty(alternativeKey))
							{
								alternativeKey = this.None;
							}

							// If it wasn't defined, define the input with the default values
							setKey(input.inputButtonName, defaultKey, alternativeKey);
						}
					}
					else
					{
						string negativeKeyName = input.cInputNegativeKeyName;
						string positiveKeyName = input.cInputPositiveKeyName;
						string negativeDefaultKey = input.cInputNegativeDefaultKey;
						string positiveDefaultKey = input.cInputPositiveDefaultKey;
						string positiveAlternativeKey = input.cInputPositiveAlternativeKey;
						string negativeAlternativeKey = input.cInputNegativeAlternativeKey;

						if (input.inputType == InputType.HorizontalAxis)
						{
							// If this input reference represents the horizontal axis,
							// check if we should use the default values...
							if (string.IsNullOrEmpty(negativeKeyName))
							{
								negativeKeyName = input.inputButtonName + "_Left";
							}

							if (string.IsNullOrEmpty(positiveKeyName))
							{
								positiveKeyName = input.inputButtonName + "_Right";
							}

							if (string.IsNullOrEmpty(negativeDefaultKey))
							{
								negativeDefaultKey = "LeftArrow";
							}

							if (string.IsNullOrEmpty(positiveDefaultKey))
							{
								positiveDefaultKey = "RightArrow";
							}
						}
						else
						{
							// If this input reference represents the vertical axis,
							// check if we should use the default values...
							if (string.IsNullOrEmpty(negativeKeyName))
							{
								negativeKeyName = input.inputButtonName + "_Down";
							}

							if (string.IsNullOrEmpty(positiveKeyName))
							{
								positiveKeyName = input.inputButtonName + "_Up";
							}

							if (string.IsNullOrEmpty(negativeDefaultKey))
							{
								negativeDefaultKey = "DownArrow";
							}

							if (string.IsNullOrEmpty(positiveDefaultKey))
							{
								positiveDefaultKey = "UpArrow";
							}
						}

						if (string.IsNullOrEmpty(positiveAlternativeKey))
						{
							positiveAlternativeKey = this.None;
						}

						if (string.IsNullOrEmpty(negativeAlternativeKey))
						{
							negativeAlternativeKey = this.None;
						}

						// Finally, check if the axis is defined in cInput...
						if (!isAxisDefined(input.inputButtonName))
						{
							if (!isKeyDefined(negativeKeyName))
							{
								setKey(negativeKeyName, negativeDefaultKey, negativeAlternativeKey);
							}
							if (!isKeyDefined(positiveKeyName))
							{
								setKey(positiveKeyName, positiveDefaultKey, positiveAlternativeKey);
							}
							setAxis(input.inputButtonName, negativeKeyName, positiveKeyName);
						}
					}
				}
			}
		}
		#endregion
	}
}