using System;
using FPLibrary;

namespace UFE3D
{
	[Serializable]
	public struct FrameInput : IEquatable<FrameInput>
	{
		#region public class properties
		public readonly static sbyte NullSelectedOption = sbyte.MinValue;
		#endregion

		#region public instance properties
		public Fix64 horizontalAxisRaw;
		public Fix64 verticalAxisRaw;
		public NetworkButtonPress buttons;
		public sbyte selectedOption;
		#endregion

		#region public instance constructors
		public FrameInput(sbyte selectedOption) : this(NetworkButtonPress.None, selectedOption) { }


		public FrameInput(NetworkButtonPress buttons, sbyte selectedOption) : this(
			((buttons & NetworkButtonPress.Forward) != 0 ? 1 : 0) - ((buttons & NetworkButtonPress.Back) != 0 ? 1 : 0),
			((buttons & NetworkButtonPress.Up) != 0 ? 1 : 0) - ((buttons & NetworkButtonPress.Down) != 0 ? 1 : 0),
			buttons,
			selectedOption
		)
		{ }

		public FrameInput(
			Fix64 horizontalAxisRaw,
			Fix64 verticalAxisRaw,
			NetworkButtonPress buttons,
			sbyte selectedOption
		)
		{
			// Make sure the buttons match the axis values
			if (horizontalAxisRaw == 0f)
			{
				buttons = buttons & ~NetworkButtonPress.Back;
				buttons = buttons & ~NetworkButtonPress.Forward;
			}
			else if (horizontalAxisRaw > 0f)
			{
				buttons = buttons & ~NetworkButtonPress.Back;
				buttons = buttons | NetworkButtonPress.Forward;
			}
			else
			{
				buttons = buttons | NetworkButtonPress.Back;
				buttons = buttons & ~NetworkButtonPress.Forward;
			}

			if (verticalAxisRaw == 0f)
			{
				buttons = buttons & ~NetworkButtonPress.Down;
				buttons = buttons & ~NetworkButtonPress.Up;
			}
			else if (verticalAxisRaw > 0f)
			{
				buttons = buttons & ~NetworkButtonPress.Down;
				buttons = buttons | NetworkButtonPress.Up;
			}
			else
			{
				buttons = buttons | NetworkButtonPress.Down;
				buttons = buttons & ~NetworkButtonPress.Up;
			}

			// Assign the values
			this.horizontalAxisRaw = horizontalAxisRaw;
			this.verticalAxisRaw = verticalAxisRaw;
			this.buttons = buttons;
			this.selectedOption = selectedOption;
		}

		public FrameInput(FrameInput other) : this(
			other.horizontalAxisRaw,
			other.verticalAxisRaw,
			other.buttons,
			other.selectedOption
		)
		{ }
		#endregion

		#region public instance methods
		//	public byte[] Serialize(){
		//		return FrameInput.Serialize(this);
		//	}
		#endregion

		#region public override methods
		public override string ToString()
		{
			return string.Format(
				"[FrameInput | horizontalAxisRaw = {0} | verticalAxisRaw = {1} | buttons = {2} | selected option = {3}]",
				this.horizontalAxisRaw,
				this.verticalAxisRaw,
				this.buttons,
				this.selectedOption
			);
		}
		#endregion

		#region IEquatable<FrameInput> interface implementation
		public override bool Equals(object obj)
		{
			if (obj is FrameInput)
			{
				return this.Equals((FrameInput)obj);
			}
			return false;
		}

		public bool Equals(FrameInput other)
		{
			return
				this.horizontalAxisRaw == other.horizontalAxisRaw &&
				this.verticalAxisRaw == other.verticalAxisRaw &&
				this.buttons == other.buttons &&
				this.selectedOption == other.selectedOption;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return
					(int)(this.buttons) +
					//				11 * this.horizontalAxis +
					//				47 * this.horizontalAxisRaw +
					//				101 * this.verticalAxis + 
					//				449 * this.verticalAxisRaw +
					1553 * this.selectedOption;
			}
		}

		public static bool operator ==(FrameInput f1, FrameInput f2)
		{
			return f1.Equals(f2);
		}

		public static bool operator !=(FrameInput f1, FrameInput f2)
		{
			return !(f1 == f2);
		}
		#endregion
	}
}
