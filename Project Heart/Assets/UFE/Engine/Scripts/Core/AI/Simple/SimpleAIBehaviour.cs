using UnityEngine;
using System;

namespace UFE3D
{
	[Serializable]
	public class SimpleAIBehaviour : ScriptableObject
	{
		public SimpleAIStep[] steps = new SimpleAIStep[0];
		public bool blockAfterFirstHit;

		[HideInInspector]
		public bool showInInspector;

		[HideInInspector]
		public bool showStepsInInspector;

	}

	[Serializable]
	public class SimpleAIStep
	{
		public ButtonPress[] buttons = new ButtonPress[0];
		public int frames;

		[HideInInspector]
		public bool showInInspector;
	}
}