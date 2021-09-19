using FPLibrary;

public class InputEvents{
	#region public class properties
	public static InputEvents Default{
		get{
			return InputEvents._Default;
		}
	}
	#endregion

	#region private class properties
	private static InputEvents _Default = new InputEvents();
	#endregion

	#region public instance properties
	public Fix64 axisRaw {get; protected set;}
	public bool button {get; protected set;}
	#endregion

	#region public constructors
	public InputEvents() : this(0f, false){}
	public InputEvents(bool button) : this(0f, button){}
	public InputEvents(Fix64 axisRaw) : this(axisRaw, axisRaw != 0f){}
	public InputEvents(InputEvents other) : this(other.axisRaw, other.button){}
	#endregion

	#region protected constructors
	protected InputEvents(Fix64 axisRaw, bool button){
		this.axisRaw = axisRaw;
		this.button = button;
	}
	#endregion
}
