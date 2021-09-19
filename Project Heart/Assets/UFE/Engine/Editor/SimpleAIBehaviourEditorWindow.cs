using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace UFE3D
{
	public class SimpleAIBehaviourEditorWindow : EditorWindow
	{
		#region public class properties
		public static SimpleAIBehaviourEditorWindow window;
		#endregion

		#region private class properties
		private SimpleAIBehaviour behaviour;

		private string titleStyle;
		private string addButtonStyle;
		private string subGroupStyle;
		private string arrayElementStyle;
		private string foldStyle;
		private string enumStyle;
		private GUIStyle labelStyle;
		private Vector2 scrollPos;
		#endregion

		#region public class methods
		[MenuItem("Window/U.F.E./Simple AI Editor")]
		public static void Init()
		{
			window = EditorWindow.GetWindow<SimpleAIBehaviourEditorWindow>(false, "Simple AI", true);
			window.Show();
			window.Populate();
		}

		#endregion

		#region public instance methods
		void Clear()
		{
			//		if (behaviour != null){
			//			CloseGUICanvas();
			//		}
		}

		void Populate()
		{
			this.titleContent = new GUIContent("Simple AI", (Texture)Resources.Load("Icons/Global"));

			// Style Definitions
			titleStyle = "MeTransOffRight";
			addButtonStyle = "CN CountBadge";
			subGroupStyle = "ObjectFieldThumb";
			arrayElementStyle = "FrameBox";
			foldStyle = "Foldout";
			enumStyle = "MiniPopup";

			labelStyle = new GUIStyle();
			labelStyle.alignment = TextAnchor.MiddleCenter;
			labelStyle.fontStyle = FontStyle.Bold;
			labelStyle.normal.textColor = Color.white;


			//		helpGUIContent.text = "";
			//		helpGUIContent.tooltip = "Open Live Docs";
			//		//helpGUIContent.image = (Texture2D) EditorGUIUtility.Load("icons/SVN_Local.png");


			UnityEngine.Object[] selection = Selection.GetFiltered(typeof(SimpleAIBehaviour), SelectionMode.Assets);
			if (selection.Length > 0 && selection[0] != null)
			{
				behaviour = (SimpleAIBehaviour)selection[0];
			}
		}
		#endregion

		#region EditorWindow methods
		void OnSelectionChange()
		{
			Populate();
			Repaint();
		}

		void OnEnable()
		{
			Populate();
		}

		void OnFocus()
		{
			Populate();
		}

		void OnDisable()
		{
			Clear();
		}

		void OnDestroy()
		{
			Clear();
		}

		void OnLostFocus()
		{
			//Clear();
		}

		void Update()
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
			{
				Clear();
			}
		}

		public void OnGUI()
		{
			if (behaviour == null)
			{
				GUILayout.BeginHorizontal("GroupBox");
				GUILayout.Label("Select a Simple AI file\nor create a new one.", "CN EntryInfo");
				GUILayout.EndHorizontal();
				EditorGUILayout.Space();
				if (GUILayout.Button("Create new Simple AI"))
					ScriptableObjectUtility.CreateAsset<SimpleAIBehaviour>();
				return;
			}

			//string path = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length) + AssetDatabase.GetAssetPath(behaviour);
			//string title = /*"Simple AI:\t" + */path;

			string path = AssetDatabase.GetAssetPath(behaviour);
			string title = path.Substring(Mathf.Min(path.Length - 1, path.LastIndexOf('/') + 1));


			int dotPosition = title.LastIndexOf('.');
			if (dotPosition > 0)
			{
				title = title.Substring(0, dotPosition);
			}




			GUIStyle fontStyle = new GUIStyle();
			//fontStyle.font = (Font)EditorGUIUtility.Load("EditorFont.TTF");
			fontStyle.font = (Font)Resources.Load("EditorFont");
			fontStyle.fontSize = 30;
			fontStyle.alignment = TextAnchor.UpperCenter;
			fontStyle.normal.textColor = Color.white;
			fontStyle.hover.textColor = Color.white;

			EditorGUILayout.BeginVertical(titleStyle);
			{
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField("", title, fontStyle, GUILayout.Height(32));
					//helpButton("global:start");
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndVertical();


			EditorGUILayout.Space();
			EditorGUILayout.Space();

			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
			{
				behaviour.blockAfterFirstHit = EditorGUILayout.Toggle("Block after first hit:", behaviour.blockAfterFirstHit);

				//		EditorGUILayout.Space();
				//		EditorGUILayout.Space();

				EditorGUILayout.LabelField("Steps (" + behaviour.steps.Length + "):");
				EditorGUILayout.BeginVertical(subGroupStyle);
				{
					for (int j = 0; j < behaviour.steps.Length; ++j)
					{
						SimpleAIStep step = behaviour.steps[j];
						EditorGUILayout.Space();

						EditorGUILayout.BeginVertical(arrayElementStyle);
						{
							EditorGUILayout.Space();
							EditorGUILayout.BeginHorizontal();
							{
								step.showInInspector = EditorGUILayout.Foldout(step.showInInspector, "Step " + (j + 1), foldStyle);
								if (GUILayout.Button("", "PaneOptions"))
								{
									PaneOptions<SimpleAIStep>(behaviour.steps, step, delegate (SimpleAIStep[] newElement) { behaviour.steps = newElement; });
								}
							}
							EditorGUILayout.EndHorizontal();

							if (step.showInInspector)
							{
								EditorGUI.indentLevel += 1;
								step.frames = Mathf.Max(1, EditorGUILayout.IntField("Repeat for X frames:", step.frames));
								EditorGUILayout.LabelField("Buttons Pressed (" + step.buttons.Length + "):");
								EditorGUILayout.BeginVertical(subGroupStyle);
								{
									for (int k = 0; k < step.buttons.Length; ++k)
									{
										EditorGUILayout.Space();
										EditorGUILayout.BeginVertical(arrayElementStyle);
										{
											EditorGUILayout.Space();
											EditorGUILayout.BeginHorizontal();
											{
												step.buttons[k] = (ButtonPress)EditorGUILayout.EnumPopup("Button:", step.buttons[k], enumStyle);
												if (GUILayout.Button("", "PaneOptions"))
												{
													PaneOptions<ButtonPress>(step.buttons, step.buttons[k], delegate (ButtonPress[] newElement) { step.buttons = newElement; });
												}
											}
											EditorGUILayout.EndHorizontal();
											EditorGUILayout.Space();
										}
										EditorGUILayout.EndVertical();
									}

									if (StyledButton("New Button"))
									{
										step.buttons = AddElement<ButtonPress>(step.buttons, ButtonPress.Forward);
									}
								}
								EditorGUILayout.EndVertical();

								EditorGUI.indentLevel -= 1;
							}
							EditorGUILayout.Space();
						}
						EditorGUILayout.EndVertical();
					}

					EditorGUILayout.Space();
					if (StyledButton("New Step"))
					{
						behaviour.steps = AddElement<SimpleAIStep>(behaviour.steps, new SimpleAIStep());
					}

				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndScrollView();
		}
		#endregion


		#region Utility Methods which should be in a different class
		public void PaneOptions<T>(T[] elements, T element, System.Action<T[]> callback)
		{
			if (elements == null || elements.Length == 0) return;
			GenericMenu toolsMenu = new GenericMenu();

			if ((elements[0] != null && elements[0].Equals(element)) || (elements[0] == null && element == null) || elements.Length == 1)
			{
				toolsMenu.AddDisabledItem(new GUIContent("Move Up"));
				toolsMenu.AddDisabledItem(new GUIContent("Move To Top"));
			}
			else
			{
				toolsMenu.AddItem(new GUIContent("Move Up"), false, delegate () { callback(MoveElement<T>(elements, element, -1)); });
				toolsMenu.AddItem(new GUIContent("Move To Top"), false, delegate () { callback(MoveElement<T>(elements, element, -elements.Length)); });
			}
			if ((elements[elements.Length - 1] != null && elements[elements.Length - 1].Equals(element)) || elements.Length == 1)
			{
				toolsMenu.AddDisabledItem(new GUIContent("Move Down"));
				toolsMenu.AddDisabledItem(new GUIContent("Move To Bottom"));
			}
			else
			{
				toolsMenu.AddItem(new GUIContent("Move Down"), false, delegate () { callback(MoveElement<T>(elements, element, 1)); });
				toolsMenu.AddItem(new GUIContent("Move To Bottom"), false, delegate () { callback(MoveElement<T>(elements, element, elements.Length)); });
			}

			toolsMenu.AddSeparator("");

			if (element != null && element is System.ICloneable)
			{
				toolsMenu.AddItem(new GUIContent("Copy"), false, delegate () { callback(CopyElement<T>(elements, element)); });
			}
			else
			{
				toolsMenu.AddDisabledItem(new GUIContent("Copy"));
			}

			if (element != null && CloneObject.objCopy != null && CloneObject.objCopy.GetType() == typeof(T))
			{
				toolsMenu.AddItem(new GUIContent("Paste"), false, delegate () { callback(PasteElement<T>(elements, element)); });
			}
			else
			{
				toolsMenu.AddDisabledItem(new GUIContent("Paste"));
			}

			toolsMenu.AddSeparator("");

			if (!(element is System.ICloneable))
			{
				toolsMenu.AddDisabledItem(new GUIContent("Duplicate"));
			}
			else
			{
				toolsMenu.AddItem(new GUIContent("Duplicate"), false, delegate () { callback(DuplicateElement<T>(elements, element)); });
			}
			toolsMenu.AddItem(new GUIContent("Remove"), false, delegate () { callback(RemoveElement<T>(elements, element)); });

			toolsMenu.ShowAsContext();
			EditorGUIUtility.ExitGUI();
		}

		public bool StyledButton(string label)
		{
			EditorGUILayout.Space();
			GUILayoutUtility.GetRect(1, 20);
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			bool clickResult = GUILayout.Button(label, addButtonStyle);
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			return clickResult;
		}

		public T[] RemoveElement<T>(T[] elements, T element)
		{
			List<T> elementsList = new List<T>(elements);
			elementsList.Remove(element);
			return elementsList.ToArray();
		}

		public T[] AddElement<T>(T[] elements, T element)
		{
			List<T> elementsList = new List<T>(elements);
			elementsList.Add(element);
			return elementsList.ToArray();
		}

		public T[] CopyElement<T>(T[] elements, T element)
		{
			CloneObject.objCopy = (object)(element as ICloneable).Clone();
			return elements;
		}

		public T[] PasteElement<T>(T[] elements, T element)
		{
			if (CloneObject.objCopy == null) return elements;
			List<T> elementsList = new List<T>(elements);
			elementsList.Insert(elementsList.IndexOf(element) + 1, (T)CloneObject.objCopy);
			CloneObject.objCopy = null;
			return elementsList.ToArray();
		}

		public T[] DuplicateElement<T>(T[] elements, T element)
		{
			List<T> elementsList = new List<T>(elements);
			elementsList.Insert(elementsList.IndexOf(element) + 1, (T)(element as ICloneable).Clone());
			return elementsList.ToArray();
		}

		public T[] MoveElement<T>(T[] elements, T element, int steps)
		{
			List<T> elementsList = new List<T>(elements);
			int newIndex = Mathf.Clamp(elementsList.IndexOf(element) + steps, 0, elements.Length - 1);
			elementsList.Remove(element);
			elementsList.Insert(newIndex, element);
			return elementsList.ToArray();
		}
		#endregion
	}
}