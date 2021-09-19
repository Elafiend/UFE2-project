using UnityEngine;
using System;
using System.Collections.Generic;
using FPLibrary;

namespace UFE3D
{
	public static class FluxExtensions
	{
		public static Tuple<Dictionary<InputReferences, InputEvents>, sbyte?> GetInputEvents(
			this IList<InputReferences> inputReferences,
			FrameInput frameInput
		)
		{
			Dictionary<InputReferences, InputEvents> dict = new Dictionary<InputReferences, InputEvents>();

			sbyte? selectedOption =
				(frameInput.selectedOption == FrameInput.NullSelectedOption ? null : new sbyte?(frameInput.selectedOption));

			NetworkButtonPress buttons = frameInput.buttons;
			if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size8Bits)
			{
				buttons &= (NetworkButtonPress)((sbyte)(-1));
			}
			else if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size16Bits)
			{
				buttons &= (NetworkButtonPress)((short)(-1));
			}

			foreach (InputReferences input in inputReferences)
			{
				if (input.inputType == InputType.HorizontalAxis)
				{
					dict[input] = new InputEvents(frameInput.horizontalAxisRaw);
				}
				else if (input.inputType == InputType.VerticalAxis)
				{
					dict[input] = new InputEvents(frameInput.verticalAxisRaw);
				}
				else if (input.inputType == InputType.Button)
				{
					NetworkButtonPress networkButtonPress = input.engineRelatedButton.ToNetworkButtonPress();
					dict[input] = new InputEvents((buttons & networkButtonPress) != NetworkButtonPress.None);
				}
			}

			return new Tuple<Dictionary<InputReferences, InputEvents>, sbyte?>(dict, selectedOption);
		}


		public static FrameInput ToFrameInput(this Dictionary<InputReferences, InputEvents> inputs, sbyte? selectedOption)
		{
			Fix64 horizontalAxisRaw = 0;
			Fix64 verticalAxisRaw = 0;
			NetworkButtonPress buttons = NetworkButtonPress.None;

			foreach (KeyValuePair<InputReferences, InputEvents> pair in inputs)
			{
				InputReferences inputReference = pair.Key;
				InputEvents inputEvent = pair.Value;

				if (inputReference.inputType == InputType.HorizontalAxis)
				{
					horizontalAxisRaw = inputEvent.axisRaw;
				}
				else if (inputReference.inputType == InputType.VerticalAxis)
				{
					verticalAxisRaw = inputEvent.axisRaw;
				}
				else if (inputReference.inputType == InputType.Button && inputEvent.button)
				{
					NetworkButtonPress buttonPress = inputReference.engineRelatedButton.ToNetworkButtonPress();
					if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size8Bits)
					{
						buttonPress &= (NetworkButtonPress)((sbyte)(-1));
					}
					else if (UFE.config.networkOptions.networkMessageSize == NetworkMessageSize.Size16Bits)
					{
						buttonPress &= (NetworkButtonPress)((short)(-1));
					}

					buttons |= buttonPress;

					//buttons |= inputReference.engineRelatedButton.ToNetworkButtonPress();
				}
			}

			if (UFE.config.inputOptions.forceDigitalInput)
			{
				return new FrameInput(
					FPMath.Sign(horizontalAxisRaw),
					FPMath.Sign(verticalAxisRaw),
					buttons,
					selectedOption == null ? FrameInput.NullSelectedOption : selectedOption.Value
				);
			}
			else
			{
				return new FrameInput(
					horizontalAxisRaw,
					verticalAxisRaw,
					buttons,
					selectedOption == null ? FrameInput.NullSelectedOption : selectedOption.Value
				);
			}
		}
	}
}