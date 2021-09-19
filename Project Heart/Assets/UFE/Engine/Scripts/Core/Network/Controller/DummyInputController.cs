using System.Collections.Generic;

namespace UFE3D
{
	public class DummyInputController : UFEController
	{
		#region public override methods
		public override void DoFixedUpdate() { }
		public override void DoUpdate() { }
		public override InputEvents ReadInput(InputReferences inputReference)
		{
			return InputEvents.Default;
		}
		#endregion

		#region public instance methods
		public virtual void SetInput(IDictionary<InputReferences, InputEvents> inputs)
		{
			foreach (KeyValuePair<InputReferences, InputEvents> pair in inputs)
			{
				this.SetInput(pair.Key, pair.Value);
			}
		}

		public virtual void SetInput(InputReferences inputReference, InputEvents ev)
		{
			this.inputs[inputReference] = ev;
		}
		#endregion
	}
}