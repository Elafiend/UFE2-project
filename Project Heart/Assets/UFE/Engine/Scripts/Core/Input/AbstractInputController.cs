using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace UFE3D
{
	public abstract class AbstractInputController : MonoBehaviour
	{
		#region public instance properties
		public ReadOnlyCollection<InputReferences> inputReferences { get; protected set; }
		public InputReferences horizontalAxis { get; protected set; }
		public InputReferences verticalAxis { get; protected set; }
		public ReadOnlyCollection<InputReferences> buttons { get; protected set; }
		public virtual Dictionary<InputReferences, InputEvents> inputs { get; protected set; }
		public virtual int player { get; set; }
		#endregion

		#region public instance methods
		public InputEvents GetInput(InputReferences inputReference)
		{
			InputEvents currentEvent = null;
			if (inputReference != null && this.inputs.TryGetValue(inputReference, out currentEvent))
			{
				return currentEvent;
			}
			return null;
		}

		public InputReferences GetInputReference(ButtonPress button)
		{
			foreach (InputReferences inputReference in this.inputReferences)
			{
				if (inputReference != null && inputReference.engineRelatedButton == button)
				{
					return inputReference;
				}
			}
			return null;
		}

		public virtual void Initialize(IEnumerable<InputReferences> inputReferences)
		{
			List<InputReferences> buttonList = new List<InputReferences>();
			List<InputReferences> inputReferenceList = new List<InputReferences>();

			this.inputs = new Dictionary<InputReferences, InputEvents>();
			if (inputReferences != null)
			{
				foreach (InputReferences inputReference in inputReferences)
				{
					if (inputReference != null)
					{
						this.inputs[inputReference] = InputEvents.Default;

						inputReferenceList.Add(inputReference);
						if (inputReference.inputType == InputType.HorizontalAxis)
						{
							this.horizontalAxis = inputReference;
						}
						else if (inputReference.inputType == InputType.VerticalAxis)
						{
							this.verticalAxis = inputReference;
						}
						else
						{
							buttonList.Add(inputReference);
						}
					}
				}
			}

			this.inputReferences = new ReadOnlyCollection<InputReferences>(inputReferenceList);
			this.buttons = new ReadOnlyCollection<InputReferences>(buttonList);
		}
		#endregion

		#region abstract methods definition
		public abstract InputEvents ReadInput(InputReferences inputReference);
		#endregion

		#region MonoBehaviour methods
		public virtual void DoUpdate()
		{
			if (this.inputReferences != null)
			{
				//---------------------------------------------------------------------------------------------------------
				// Read the player input.
				//---------------------------------------------------------------------------------------------------------
				foreach (InputReferences inputReference in this.inputReferences)
				{
					this.inputs[inputReference] = this.ReadInput(inputReference);
				}
			}
		}

		public virtual void DoFixedUpdate() { }
		#endregion
	}
}