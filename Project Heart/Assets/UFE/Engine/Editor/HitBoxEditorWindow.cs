using UnityEngine;
using UnityEditor;
using FPLibrary;
using System;
using System.Collections.Generic;
using UFE3D;

public class HitBoxEditorWindow : EditorWindow
{
    public static HitBoxEditorWindow hitBoxEditorWindow;
    private CustomHitBoxesInfo animatorInfo;
    private Vector2 scrollPos;
    private Vector2 scrollPos2;
    private Vector2 scrollPos3;
    private string titleStyle;
    private string buttonStyle;
    private string buttonStyleGreen;
    private string borderBarStyle;
    private string subGroupStyle;
    private string subArrayElementStyle;
    private GUIStyle labelStyle;
    private GUIContent helpGUIContent = new GUIContent();
    private bool animatorSettings;

    private Texture circleTexture;
    private Texture squareTexture;
    private Texture zoomInTexture;
    private Texture zoomOutTexture;

    private Vector2 mousePos;
    private Vector2 lmousePos;
    private Vector2 rMousePos;
    private Vector2 tMousePos;
    private bool mouseDragging;

    private int currentMouseOverFrame;
    private int currentPreviewFrame;
    private ActiveRectDefinition currentRectDef;

    private int renamingTarget = -1;
    private bool renameFocus = false;
    private Rect renameRect = Rect.zero;

    private bool showOverlayBox;
    private Rect popUpRect = Rect.zero;
    private FrameDefinition targetFrameDefinition;
    private CustomHitBox targetHitBoxDefinition;

    private bool previewToggle = false;
    private GameObject goInstance;

    private float currentZoom = 1;
    private Fix64 speedTemp = 1;

    public class ActiveRectDefinition
    {
        public string id;
        public int firstFrame;
        public int lastFrame;
        public Rect rect;
    }

    [MenuItem("Window/U.F.E./Hit Box Editor")]
    public static void Init()
    {
        hitBoxEditorWindow = EditorWindow.GetWindow<HitBoxEditorWindow>(false, "Animator", true);
        hitBoxEditorWindow.Show();
        hitBoxEditorWindow.Populate();
    }


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

    void Update()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Clear();
        }
    }

    void Clear()
    {
        if (animatorInfo != null)
        {
            // Close Preview
        }
    }

    void helpButton(string page)
    {
        if (GUILayout.Button("?", GUILayout.Width(18), GUILayout.Height(18)))
            Application.OpenURL("http://www.ufe3d.com/doku.php/" + page);
    }

    void Populate()
    {
        this.titleContent = new GUIContent("Hit Box Editor", (Texture)Resources.Load("Icons/Animator"));

        // Style Definitions
        titleStyle = "MeTransOffRight";
        borderBarStyle = "ProgressBarBack";
        buttonStyle = "minibutton";
        buttonStyleGreen = "minibutton";
        subGroupStyle = "ObjectFieldThumb";
        subArrayElementStyle = "HelpBox";

        labelStyle = new GUIStyle();
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.normal.textColor = Color.white;

        helpGUIContent.text = "";
        helpGUIContent.tooltip = "Open Live Docs";

        UnityEngine.Object[] selection = Selection.GetFiltered(typeof(CustomHitBoxesInfo), SelectionMode.Assets);
        if (selection.Length > 0)
        {
            if (selection[0] == null) return;
            animatorInfo = (CustomHitBoxesInfo)selection[0];
        }
        if (animatorInfo != null) speedTemp = animatorInfo.speed;

        wantsMouseMove = true;

        circleTexture = (Texture)Resources.Load("Icons/Circle");
        squareTexture = (Texture)Resources.Load("Icons/Square");
        zoomInTexture = (Texture)Resources.Load("Icons/ZoomIn");
        zoomOutTexture = (Texture)Resources.Load("Icons/ZoomOut");
    }

    public void OnGUI()
    {
        if (animatorInfo == null)
        {
            GUILayout.BeginHorizontal("GroupBox");
            GUILayout.Label("Select a UFE Animator file\nor create a new one.", "CN EntryInfo");
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("Create new UFE Animator file"))
                ScriptableObjectUtility.CreateAsset<CustomHitBoxesInfo>();
            return;
        }

        // Set default skin textures
        Texture2D originalBackground = GUI.skin.box.normal.background;
        GUI.skin.box.normal.background = Texture2D.whiteTexture;

        // Header
        GUIStyle fontStyle = new GUIStyle();
        fontStyle.font = (Font)Resources.Load("EditorFont");
        fontStyle.fontSize = 30;
        fontStyle.alignment = TextAnchor.UpperCenter;
        fontStyle.normal.textColor = Color.white;
        fontStyle.hover.textColor = Color.white;
        EditorGUILayout.BeginVertical(titleStyle);
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("", (animatorInfo.clip == null ? "No Clip" : animatorInfo.name), fontStyle, GUILayout.Height(32));
                helpButton("animator:start");
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        EditorGUIUtility.labelWidth = 150;


        // Animation and Preview Controls
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUIUtility.labelWidth = 98;
            EditorGUIUtility.fieldWidth = 240;
            AnimationClip clipTemp = animatorInfo.clip;
            clipTemp = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip:", clipTemp, typeof(AnimationClip), true);
            if (clipTemp != animatorInfo.clip)
            {
                bool updateConfirm = EditorUtility.DisplayDialog("Replace Animation", "Replacing animations may cause data loss in the current hitbox definitions.", "Confirm", "Cancel");
                if (updateConfirm)
                {
                    animatorInfo.clip = clipTemp;
                    animatorInfo.totalFrames = animatorInfo.clip != null ? (int)Mathf.Abs(Mathf.Ceil(UFE.fps * (animatorInfo.clip.length / (float)animatorInfo.speed))) : 1; //TODO: Adjustable FPS; Detach UFE
                    ResizeFrameDefinitions();
                }
            }

            EditorGUIUtility.labelWidth = 50;
            EditorGUIUtility.fieldWidth = 30;

            GUI.SetNextControlName("SpeedField");
            speedTemp = EditorGUILayout.FloatField(new GUIContent("Speed:", "Press 'Enter' to change the speed value"), (float)speedTemp);

            // Apply speed when user presses 'Enter'
            if ((Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter) 
                && Event.current.type == EventType.KeyUp
                && speedTemp != animatorInfo.speed && Event.current.isKey 
                && GUI.GetNameOfFocusedControl() == "SpeedField")
            {
                bool updateConfirm2 = EditorUtility.DisplayDialog("Update Speed", "Changing the speed value may cause data loss in the current hitbox definitions.", "Confirm", "Cancel");
                if (updateConfirm2)
                {
                    RecordChange(animatorInfo, "Speed Change");
                    animatorInfo.speed = speedTemp;
                    animatorInfo.totalFrames = animatorInfo.clip != null ? (int)Mathf.Abs(Mathf.Ceil(UFE.fps * (animatorInfo.clip.length/(float)animatorInfo.speed))) : 1; //TODO: Adjustable FPS; Detach UFE
                    ResizeFrameDefinitions();
                }
                else
                {
                    speedTemp = animatorInfo.speed;
                }
            }

            GUILayout.FlexibleSpace();

            GameObject previewTemp = animatorInfo.preview;
            EditorGUIUtility.labelWidth = 120;
            EditorGUIUtility.fieldWidth = 240;
            animatorInfo.preview = (GameObject)EditorGUILayout.ObjectField("Character Preview:", animatorInfo.preview, typeof(GameObject), true);

            if (previewTemp != animatorInfo.preview && animatorInfo.preview.GetComponent<HitBoxesScript>() == null)
            {
                Debug.LogWarning("Prefab must have the HitBoxesScript component attached to it.");
                animatorInfo.preview = null;
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("", "PaneOptions")) {
                GenericMenu toolsMenu = new GenericMenu();

                if (animatorInfo.customHitBoxes.Length > 0) {
                    toolsMenu.AddItem(new GUIContent("Copy All Hit Boxes"), false, delegate () {
                        List<CustomHitBox> list = new List<CustomHitBox>();
                        list.AddRange(animatorInfo.customHitBoxes);
                        CloneObject.arrayCopy = CloneObject.ReflectionCloneArray(list.ToArray());
                        ResizeFrameDefinitions();
                    });
                } else {
                    toolsMenu.AddDisabledItem(new GUIContent("Copy All Hit Boxes"));
                }
                if (CloneObject.arrayCopy != null && CloneObject.arrayCopy.Length > 0 && CloneObject.arrayCopy[0].GetType() == typeof(CustomHitBox)) {
                    toolsMenu.AddItem(new GUIContent("Paste All Hit Boxes"), false, delegate () {
                        List<CustomHitBox> list = new List<CustomHitBox>();
                        list.AddRange(animatorInfo.customHitBoxes);
                        list.AddRange((CustomHitBox[])CloneObject.arrayCopy);
                        animatorInfo.customHitBoxes = list.ToArray();
                        ResizeFrameDefinitions();
                    });
                } else {
                    toolsMenu.AddDisabledItem(new GUIContent("Paste All Hit Boxes"));
                }
                if (animatorInfo.customHitBoxes.Length > 0) {
                    toolsMenu.AddItem(new GUIContent("Remove All Hit Boxes"), false, delegate () {
                        bool removeConfirm = EditorUtility.DisplayDialog("Remove All Hit Boxes", "Removing all hit boxes will result in data loss to the current hitbox definitions. Are you sure?", "Confirm", "Cancel");
                        if (removeConfirm) {
                            List<CustomHitBox> list = new List<CustomHitBox>();
                            list.AddRange(animatorInfo.customHitBoxes);
                            list.Clear();
                            animatorInfo.customHitBoxes = list.ToArray();
                        }
                        ResizeFrameDefinitions();
                    });
                } else {
                    toolsMenu.AddDisabledItem(new GUIContent("Remove All Hit Boxes"));
                }
                toolsMenu.AddSeparator("");
                if (animatorInfo.clip != null) {
                    toolsMenu.AddItem(new GUIContent("Reload Animation"), false, delegate () {
                        AnimationClip clipTemp2 = animatorInfo.clip;
                        bool updateConfirm = EditorUtility.DisplayDialog("Reload Animation", "Reloading animations may cause data loss in the current hitbox definitions.", "Confirm", "Cancel");
                        if (updateConfirm) {
                            animatorInfo.clip = clipTemp2;
                            animatorInfo.totalFrames = animatorInfo.clip != null ? (int)Mathf.Abs(Mathf.Ceil(UFE.fps * (animatorInfo.clip.length / (float)animatorInfo.speed))) : 1; //TODO: Adjustable FPS; Detach UFE
                        }
                        ResizeFrameDefinitions();
                    });
                } else {
                    toolsMenu.AddDisabledItem(new GUIContent("Reload Animation"));
                }
                toolsMenu.ShowAsContext();
                GUIUtility.ExitGUI();
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        {

            //GUILayout.FlexibleSpace();

            EditorGUIUtility.labelWidth = 100;
            EditorGUIUtility.fieldWidth = 38;
            currentPreviewFrame = EditorGUILayout.IntSlider("Preview Frame:", currentPreviewFrame, 0, animatorInfo.totalFrames - 1);

            //GUILayout.FlexibleSpace();

            EditorGUIUtility.labelWidth = 120;
            string onOff;
            string currentButtonStyle;
            if (previewToggle) {
                onOff = "On";
                currentButtonStyle = buttonStyleGreen;
            } else {
                onOff = "Off";
                currentButtonStyle = buttonStyle;
            }
            if (GUILayout.Button("Preview: " + onOff, currentButtonStyle, GUILayout.Width(100))) {
                previewToggle = !previewToggle;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUIUtility.labelWidth = 150;
        EditorGUIUtility.fieldWidth = 150;

        // Animation Preview
        if (animatorInfo.preview != null)
        {
            if (previewToggle)
            {
                if (goInstance == null)
                {
                    EditorCamera.SetPosition(Vector3.up * 4);
                    EditorCamera.SetRotation(Quaternion.identity);
                    EditorCamera.SetOrthographic(true);
                    EditorCamera.SetSize(10);

                    goInstance = (GameObject)PrefabUtility.InstantiatePrefab(animatorInfo.preview);
                    goInstance.transform.position = new Vector3(0, 0, 0);
                }

                AnimationSampler(currentPreviewFrame);
            }
            else
            {
                if (goInstance != null)
                {
                    Editor.DestroyImmediate(goInstance);
                    goInstance = null;
                }
            }
        }
        else
        {
            if (goInstance != null)
            {
                Editor.DestroyImmediate(goInstance);
                goInstance = null;
            }
            previewToggle = false;
        }


        // Global Mouse Move Events
        if (Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDrag)
        {
            mousePos = Event.current.mousePosition;
            float initHeight = 7;
            float lineHeight = 18;

            // Relative mouse position for left table
            lmousePos = mousePos;
            lmousePos.y += scrollPos.y - 97; // Top Corner
            lmousePos.y += scrollPos.y - initHeight; // Top Corner

            // Relative mouse position for right table
            rMousePos = mousePos;
            rMousePos.x += scrollPos3.x - 151; // Left Corner TODO: Resizible
            rMousePos.y += scrollPos3.y - (initHeight + (lineHeight * 5)); // Top Corner

            // Relative mouse position for timeline table
            tMousePos = mousePos;
            tMousePos.x += scrollPos3.x - 151; // Left Corner TODO: Resizible
            tMousePos.y += scrollPos3.y - (initHeight + (lineHeight * 4)); // Top Corner
        }

        // Global Mouse Up Events
        if (Event.current.type == EventType.MouseUp)
        {
            currentRectDef = null;
            mouseDragging = false;
        }

        // Global Mouse Down Events
        if (Event.current.type == EventType.MouseDown)
        {
            // Disable rename field if mouse click happened outside renameRect
            if (!renameRect.Contains(lmousePos))
            {
                renameFocus = false;
                renamingTarget = -1;
            }

            // Disable popup field if mouse click happens outside popUpRect
            if (!popUpRect.Contains(mousePos))
            {
                showOverlayBox = false;
                targetFrameDefinition = null;
                targetHitBoxDefinition = null;
            }
        }

        EditorGUI.BeginChangeCheck();

        // Editor Window
        EditorGUILayout.BeginHorizontal();
        {
            // Left Side
            EditorGUILayout.BeginVertical(subArrayElementStyle, GUILayout.Width(140));
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(zoomOutTexture, GUILayout.Height(18), GUILayout.Width(18));
                currentZoom = GUILayout.HorizontalSlider(currentZoom, .4f, 1);
                GUILayout.Label(zoomInTexture, GUILayout.Height(18), GUILayout.Width(18));
                EditorGUILayout.EndHorizontal();

                // Hit Box Definitions
                using (var h = new EditorGUILayout.VerticalScope())
                {
                    using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos, GUIStyle.none, GUIStyle.none))
                    {
                        scrollPos = scrollView.scrollPosition;


                        for (int i = 0; i < animatorInfo.customHitBoxes.Length; i++)
                        {
                            EditorGUILayout.BeginHorizontal(borderBarStyle);
                            {
                                // Hit Box Shape
                                if (animatorInfo.customHitBoxes[i].shape == HitBoxShape.circle)
                                {
                                    GUILayout.Label(circleTexture, GUILayout.Height(14), GUILayout.Width(14));
                                }
                                else
                                {
                                    GUILayout.Label(squareTexture, GUILayout.Height(14), GUILayout.Width(14));
                                }

                                if (renamingTarget >= 0 && renamingTarget == i)
                                {
                                    // Rename field
                                    GUI.SetNextControlName("RenameField");

                                    EditorGUI.BeginChangeCheck();
                                    string renamedValue = EditorGUILayout.TextField(animatorInfo.customHitBoxes[i].name, GUILayout.Width(97));
                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        RecordChange(animatorInfo, "Hit Box Renamed");
                                        animatorInfo.customHitBoxes[i].name = renamedValue;
                                    }

                                    if (GUILayoutUtility.GetLastRect().x > 0) renameRect = GUILayoutUtility.GetLastRect();

                                    // Focus on text field when user clicks 'Rename' on the pane
                                    if (!renameFocus)
                                    {
                                        EditorGUI.FocusTextInControl("RenameField");
                                        renameFocus = true;
                                    }

                                    // Disable text field when user presses 'Enter'
                                    if (Event.current.isKey && Event.current.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl() == "RenameField")
                                    {
                                        renameFocus = false;
                                        renamingTarget = -1;
                                    }
                                }
                                else
                                {
                                    // Label
                                    GUI.SetNextControlName("LabelField");
                                    EditorGUILayout.LabelField(animatorInfo.customHitBoxes[i].name, GUILayout.Width(97));
                                }

                                // Pane Options
                                /*if (GUILayout.Button("", "PaneOptions"))
                                {
                                    PaneOptions(animatorInfo.hitBoxes, animatorInfo.hitBoxes[i], delegate (HitBoxDefinition[] newElement) { animatorInfo.hitBoxes = newElement; });
                                }*/
                            }
                            EditorGUILayout.EndHorizontal();

                            // Right Click Menu
                            if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && GUILayoutUtility.GetLastRect().Contains(lmousePos))
                            {
                                PaneOptions(animatorInfo.customHitBoxes, animatorInfo.customHitBoxes[i], delegate (CustomHitBox[] newElement) { animatorInfo.customHitBoxes = newElement; });
                            }
                        }
                        EditorGUILayout.Space();

                        //if (StyledButton("Add Hit Box"))
                        if (GUILayout.Button("Add Hit Box", buttonStyle))
                        {
                            if (animatorInfo.clip == null)
                            {
                                Debug.LogError("You must attach an animation first");
                            }
                            else
                            {
                                AddHitBoxDefinition(new CustomHitBox());
                            }
                        }
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                    }
                }
            }
            EditorGUILayout.EndVertical();


            // Right Side
            EditorGUILayout.BeginVertical(subGroupStyle);
            {
                // Animation Frames Timeline
                using (var n = new EditorGUILayout.HorizontalScope(GUILayout.Height(22)))
                {
                    using (var scrollView2 = new EditorGUILayout.ScrollViewScope(scrollPos2, GUIStyle.none, GUIStyle.none))
                    {
                        EditorGUILayout.BeginHorizontal(GUILayout.Height(22), GUILayout.ExpandWidth(false));
                        {
                            Rect labelRect = new Rect(- scrollPos2.x, 0, 6 * currentZoom, 16);

                            GUIStyle lineStyle = new GUIStyle();
                            labelStyle.alignment = TextAnchor.MiddleCenter;
                            lineStyle.fontSize = (int)Mathf.Round(11 * currentZoom);
                            lineStyle.normal.textColor = new Color(.9f, .9f, .9f, .5f);

                            GUI.Label(labelRect, "|", lineStyle);

                            for (int i = 0; i < animatorInfo.totalFrames; i++)
                            {
                                string timeLabel = i.ToString();

                                EditorGUILayout.BeginHorizontal(GUILayout.Height(22), GUILayout.ExpandWidth(false));
                                {
                                    GUIStyle labelStyle = new GUIStyle();
                                    labelStyle.alignment = TextAnchor.MiddleCenter;
                                    labelStyle.fontSize = (int)Mathf.Round(11 * currentZoom);
                                    labelStyle.normal.textColor = new Color(.9f, .9f, .9f, 1f);
                                    if (labelStyle.fontSize < 8) labelStyle.fontSize = 8;

                                    if (i == currentPreviewFrame)
                                    {
                                        // Show yellow text if animation preview is in this frame
                                        labelStyle.fontStyle = FontStyle.Bold;
                                        labelStyle.normal.textColor = Color.yellow;
                                    }
                                    else if (i == currentMouseOverFrame)
                                    {
                                        // Show red text if mouse is over the line
                                        labelStyle.fontStyle = FontStyle.Bold;
                                        labelStyle.normal.textColor = Color.red;
                                    }

                                    // Time Label
                                    labelRect = new Rect(4 + (41 * i * currentZoom) - scrollPos2.x, 0, 41 * currentZoom, 16);
                                    EditorGUI.LabelField(labelRect, timeLabel, labelStyle);

                                    // Clicking on a number sets the current preview frame to this point
                                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && labelRect.Contains(tMousePos))
                                    {
                                        currentPreviewFrame = i;
                                    }

                                    // Vertical Line Label
                                    labelRect.x += labelRect.width - (3 * currentZoom);
                                    labelRect.width = 6 * currentZoom;
                                    EditorGUI.LabelField(labelRect, "|", lineStyle);
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }

                // Active Frames Grid
                using (var n = new EditorGUILayout.HorizontalScope())
                {
                    using (var scrollView3 = new EditorGUILayout.ScrollViewScope(scrollPos3))
                    {
                        scrollPos2.x = scrollView3.scrollPosition.x; // Timeline
                        scrollPos.y = scrollView3.scrollPosition.y; // HitBoxes
                        scrollPos3 = scrollView3.scrollPosition; // Grid

                        for (int row = 0; row < animatorInfo.customHitBoxes.Length; row++)
                        {
                            EditorGUILayout.BeginHorizontal("ProjectBrowserSubAssetBgMiddle", GUILayout.Height(22), GUILayout.ExpandWidth(false));
                            {
                                List<ActiveRectDefinition> activeRectDefs = new List<ActiveRectDefinition>();
                                int currentActiveGroup = 0;
                                int remainingActive = 0;
                                FPVector recurringPosition = FPVector.zero;
                                Fix64 recurringRadius = .5;
                                Fix64 recurringWidth = .5;
                                Fix64 recurringHeight = .5;

                                // Empty Label for y reference and horizontal scroll
                                EditorGUILayout.LabelField(GUIContent.none, GUILayout.Width(41 * animatorInfo.totalFrames * currentZoom), GUILayout.Height(22), GUILayout.ExpandWidth(false));
                                Rect lastRect = GUILayoutUtility.GetLastRect();

                                for (int col = 0; col < animatorInfo.totalFrames; col++)
                                {
                                    // Active Variable Definitions
                                    Rect fieldRect = new Rect(4 + (41 * col * currentZoom), lastRect.y, 41 * currentZoom, 20);
                                    Rect activeRect = Rect.zero;
                                    int firstActiveFrame = 0;
                                    FrameDefinition firstFrameDef = null;

                                    // Active Frames Begin
                                    if (animatorInfo.customHitBoxes[row].activeFrames[col] != null)
                                    {
                                        if (remainingActive > 0)
                                        {
                                            animatorInfo.customHitBoxes[row].activeFrames[col].active = true;
                                            animatorInfo.customHitBoxes[row].activeFrames[col].position = recurringPosition;
                                            animatorInfo.customHitBoxes[row].activeFrames[col].radius = recurringRadius;
                                            animatorInfo.customHitBoxes[row].activeFrames[col].cubeWidth = recurringWidth;
                                            animatorInfo.customHitBoxes[row].activeFrames[col].cubeHeight = recurringHeight;
                                            remainingActive--;
                                        }
                                        else
                                        {
                                            animatorInfo.customHitBoxes[row].activeFrames[col].active = false;
                                        }

                                        if (animatorInfo.customHitBoxes[row].activeFrames[col].range > 0)
                                        {
                                            // Rect Definitions
                                            activeRect = new Rect(4 + (41 * col * currentZoom), lastRect.y, (41 * animatorInfo.customHitBoxes[row].activeFrames[col].range * currentZoom) - 1, 20); // 2019.3 update

                                            // Rect Border
                                            //GUI.Box(activeRect, "", borderBarStyle); // TODO: Review border usability
                                            EditorGUI.DrawRect(activeRect, new Color(0, 0, 0, 1));

                                            // Rect Overlay
                                            Color originalBColor = GUI.backgroundColor;
                                            GUI.backgroundColor = GetHitBoxColor(animatorInfo.customHitBoxes[row].collisionType);
                                            GUI.Box(activeRect, new GUIContent());
                                            GUI.backgroundColor = originalBColor;

                                            // List of active frame group in this line
                                            currentActiveGroup++;

                                            // Store first Frame Definition
                                            firstActiveFrame = col;
                                            firstFrameDef = animatorInfo.customHitBoxes[row].activeFrames[col];

                                            // Store Active Definitions
                                            ActiveRectDefinition activeRectDef = new ActiveRectDefinition();
                                            activeRectDef.id = row + "-" + currentActiveGroup;
                                            activeRectDef.rect = activeRect;
                                            activeRectDef.firstFrame = col;
                                            activeRectDef.lastFrame = col + animatorInfo.customHitBoxes[row].activeFrames[col].range;
                                            activeRectDefs.Add(activeRectDef);

                                            // Set this frame and every frame within the width window to active
                                            animatorInfo.customHitBoxes[row].activeFrames[col].active = true;
                                            remainingActive = animatorInfo.customHitBoxes[row].activeFrames[col].range - 1;
                                            recurringPosition = animatorInfo.customHitBoxes[row].activeFrames[col].position;
                                            recurringRadius = animatorInfo.customHitBoxes[row].activeFrames[col].radius;
                                            recurringWidth = animatorInfo.customHitBoxes[row].activeFrames[col].cubeWidth;
                                            recurringHeight = animatorInfo.customHitBoxes[row].activeFrames[col].cubeHeight;
                                        }
                                    }

                                    // Draw vertical red line When mouse over the grid
                                    if (rMousePos.x >= fieldRect.x && rMousePos.x <= fieldRect.x + fieldRect.width)
                                    {
                                        if (currentMouseOverFrame != col)
                                        {
                                            currentMouseOverFrame = col;
                                            Repaint();
                                        }
                                        Rect rect = new Rect((24 * currentZoom) + (41 * col * currentZoom), row * 26, 1, 26);
                                        EditorGUI.DrawRect(rect, new Color(1, 0, 0, 1));

                                        // Clicking on the column sets the current preview frame to this point
                                        if (!showOverlayBox && Event.current.type == EventType.MouseDown && Event.current.button == 0)
                                        {
                                            currentPreviewFrame = currentMouseOverFrame;
                                        }
                                    }

                                    // Draw vertical yellow line to show which frame the preview is displaying
                                    if (col == currentPreviewFrame)
                                    {
                                        Rect rect = new Rect((24 * currentZoom) + (41 * col * currentZoom), row * 26, 1, 26);
                                        EditorGUI.DrawRect(rect, new Color(1, .5f, .5f, 1));
                                    }

                                    // Test mouse clicks
                                    if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
                                    {
                                        // Right Click Menu
                                        if (activeRect.Contains(rMousePos))
                                        {
                                            //When on top of active frames
                                            GenericMenu toolsMenu = new GenericMenu();

                                            // Set Position
                                            toolsMenu.AddItem(new GUIContent("Edit..."), false, delegate () { ShowPositionPopUp(firstFrameDef, animatorInfo.customHitBoxes[row], animatorInfo.customHitBoxes[row].shape); });

                                            // Copy
                                            toolsMenu.AddItem(new GUIContent("Copy"), false, delegate () { CopyElement(animatorInfo.customHitBoxes[row].activeFrames, firstFrameDef); });

                                            toolsMenu.AddSeparator("");

                                            // Split
                                            if (firstFrameDef.range > 1)
                                            {
                                                int splitFrame = (currentMouseOverFrame == firstActiveFrame) ? currentMouseOverFrame + 1 : currentMouseOverFrame;
                                                toolsMenu.AddItem(new GUIContent("Split"), false, delegate () { SplitActiveFrames(firstActiveFrame, splitFrame, firstFrameDef, animatorInfo.customHitBoxes[row].activeFrames[splitFrame]); });
                                            }
                                            else
                                            {
                                                toolsMenu.AddDisabledItem(new GUIContent("Split"));
                                            }

                                            // Delete
                                            toolsMenu.AddItem(new GUIContent("Delete"), false, delegate () { DeleteActiveFrames(firstFrameDef); });

                                            // Delete This Frame
                                            int targetSplit = (currentMouseOverFrame < animatorInfo.customHitBoxes[row].activeFrames.Length - 1 &&
                                                currentMouseOverFrame < firstActiveFrame + firstFrameDef.range) ? currentMouseOverFrame + 1 : currentMouseOverFrame;
                                            toolsMenu.AddItem(new GUIContent("Delete This Frame"), false, delegate () { DeleteThisFrame(firstActiveFrame, currentMouseOverFrame, firstFrameDef, animatorInfo.customHitBoxes[row].activeFrames[targetSplit]); });

                                            toolsMenu.ShowAsContext();
                                            GUIUtility.ExitGUI();
                                        }
                                        else if (fieldRect.Contains(rMousePos))
                                        {
                                            //When on top of empty frames
                                            GenericMenu toolsMenu = new GenericMenu();

                                            // Add New
                                            toolsMenu.AddItem(new GUIContent("Add New Active Frame"), false, delegate () { AddActiveFrames(animatorInfo.customHitBoxes[row].activeFrames[currentMouseOverFrame]); });

                                            // Paste
                                            if (CloneObject.objCopy != null && CloneObject.objCopy.GetType() == typeof(FrameDefinition))
                                            {
                                                int maxWidth = animatorInfo.totalFrames;
                                                foreach (ActiveRectDefinition activeRectDef in activeRectDefs)
                                                {
                                                    if (activeRectDef.firstFrame > currentMouseOverFrame) maxWidth = activeRectDef.firstFrame - currentMouseOverFrame;
                                                }
                                                toolsMenu.AddItem(new GUIContent("Paste"), false, delegate () { PasteActiveFrames(animatorInfo.customHitBoxes[row].activeFrames[currentMouseOverFrame], maxWidth); });
                                            }
                                            else
                                            {
                                                toolsMenu.AddDisabledItem(new GUIContent("Paste"));
                                            }

                                            toolsMenu.ShowAsContext();
                                            GUIUtility.ExitGUI();
                                        }
                                    }
                                }


                                // Test mouse over row and create sliders
                                float lastKnownFrame = 0;
                                bool visibleSlider = false;
                                for (int i = 0; i < activeRectDefs.Count; i++)
                                {
                                    float maxValue = (i + 1 < activeRectDefs.Count) ? activeRectDefs[i + 1].firstFrame : animatorInfo.totalFrames;
                                    Rect sliderMouseOverRect = new Rect(activeRectDefs[i].rect.x - 15, activeRectDefs[i].rect.y, activeRectDefs[i].rect.width + 25, 20);
                                    Rect sliderRect = new Rect(0, activeRectDefs[i].rect.y, 8 + (41 * animatorInfo.totalFrames * currentZoom), 20);

                                    if (!visibleSlider && !showOverlayBox && ((!mouseDragging && sliderMouseOverRect.Contains(rMousePos)) || (mouseDragging && currentRectDef.id == activeRectDefs[i].id)))
                                    {
                                        // Set Mouse Drag Reference
                                        if (Event.current.type == EventType.MouseDown)
                                        {
                                            currentRectDef = activeRectDefs[i];
                                            mouseDragging = true;
                                        }


                                        // Change GUIStyle skin
                                        GUIStyle backupHSlider = GUI.skin.horizontalSlider;
                                        GUI.skin.horizontalSlider = GUIStyle.none;

                                        // Create Slider
                                        visibleSlider = true;
                                        float vmin = activeRectDefs[i].firstFrame;
                                        float vmax = activeRectDefs[i].lastFrame;
                                        EditorGUI.MinMaxSlider(sliderRect, ref vmin, ref vmax, 0, animatorInfo.totalFrames);
                                        int newvMin = (int)Mathf.Max(lastKnownFrame, Mathf.Round(vmin));
                                        int newvMax = (int)Mathf.Min(maxValue, Mathf.Round(vmax));

                                        // Restore GUI Skin
                                        GUI.skin.horizontalSlider = backupHSlider;


                                        // Limit auto-resizing
                                        /*if ((vmin < lastKnownFrame && vmax < activeRectDefs[i].lastFrame) 
                                            || (vmin > activeRectDefs[i].firstFrame && vmax > maxValue) // TODO: Test usability of disabling auto-resizing only when moving to the left
                                            )*/
                                        if (vmin < lastKnownFrame)
                                        {
                                            newvMax = activeRectDefs[i].lastFrame;
                                            newvMin = activeRectDefs[i].firstFrame;
                                        }

                                        if (newvMax - newvMin > 0)
                                        {
                                            // If first frame changed, set width of old frame to 0 and new width
                                            if (newvMin != activeRectDefs[i].firstFrame)
                                            {
                                                RecordChange(animatorInfo, "Update Active Frames");

                                                animatorInfo.customHitBoxes[row].activeFrames[newvMin].position = animatorInfo.customHitBoxes[row].activeFrames[activeRectDefs[i].firstFrame].position;
                                                animatorInfo.customHitBoxes[row].activeFrames[newvMin].radius = animatorInfo.customHitBoxes[row].activeFrames[activeRectDefs[i].firstFrame].radius;
                                                animatorInfo.customHitBoxes[row].activeFrames[newvMin].cubeWidth = animatorInfo.customHitBoxes[row].activeFrames[activeRectDefs[i].firstFrame].cubeWidth;
                                                animatorInfo.customHitBoxes[row].activeFrames[newvMin].cubeHeight = animatorInfo.customHitBoxes[row].activeFrames[activeRectDefs[i].firstFrame].cubeHeight;
                                                animatorInfo.customHitBoxes[row].activeFrames[newvMin].range = newvMax - newvMin;
                                                animatorInfo.customHitBoxes[row].activeFrames[activeRectDefs[i].firstFrame].range = 0;
                                            }

                                            // If last frame changed, set the new width
                                            if (newvMax != activeRectDefs[i].lastFrame)
                                            {
                                                RecordChange(animatorInfo, "Update Active Frames");

                                                animatorInfo.customHitBoxes[row].activeFrames[newvMin].range = newvMax - newvMin;
                                            }
                                        }
                                    }

                                    lastKnownFrame = activeRectDefs[i].lastFrame;
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();

                        Repaint();
                    }
                }
            }

            if (showOverlayBox)
            {
                Rect boxPopUp = popUpRect;
                boxPopUp.x -= 5;
                boxPopUp.width += 14;
                GUI.Box(boxPopUp, new GUIContent());
                if (targetFrameDefinition != null)
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 newPosition = EditorGUI.Vector3Field(popUpRect, "Position", targetFrameDefinition.position.ToVector());
                    if (EditorGUI.EndChangeCheck())
                    {
                        targetFrameDefinition.position = FPVector.ToFPVector(newPosition);
                        RecordChange(animatorInfo, "Changed Hit Box Position");
                    }

                    Rect fieldRect = popUpRect;
                    fieldRect.y += 45;
                    fieldRect.height = 20;

                    EditorGUIUtility.labelWidth = 80;
                    EditorGUIUtility.fieldWidth = 60;

                    if (targetHitBoxDefinition.shape == HitBoxShape.rectangle)
                    {
                        EditorGUI.BeginChangeCheck();
                        float newCubeWidth = EditorGUI.FloatField(fieldRect, "Width", targetFrameDefinition.cubeWidth.AsFloat());
                        if (EditorGUI.EndChangeCheck())
                        {
                            targetFrameDefinition.cubeWidth = newCubeWidth;
                            RecordChange(animatorInfo, "Changed Hit Box Width");
                        }

                        fieldRect.y += 25;

                        EditorGUI.BeginChangeCheck();
                        float newcubeHeight = EditorGUI.FloatField(fieldRect, "Height", targetFrameDefinition.cubeHeight.AsFloat());
                        if (EditorGUI.EndChangeCheck())
                        {
                            targetFrameDefinition.cubeHeight = newcubeHeight;
                            RecordChange(animatorInfo, "Changed Hit Box Height");
                        }
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        float newRadius = EditorGUI.FloatField(fieldRect, "Radius", targetFrameDefinition.radius.AsFloat());
                        if (EditorGUI.EndChangeCheck())
                        {
                            targetFrameDefinition.radius = newRadius;
                            RecordChange(animatorInfo, "Changed Hit Box Radius");
                        }
                    }

                    EditorGUIUtility.labelWidth = 150;
                    EditorGUIUtility.fieldWidth = 150;

                }
            }

            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUI.EndChangeCheck();


        // Restore default skin textures
        GUI.skin.box.normal.background = originalBackground;

        EditorGUILayout.Space();

        if (GUI.changed) {
            EditorUtility.SetDirty(animatorInfo);
        }
    }

    public void RecordChange(CustomHitBoxesInfo animatorInfo, string description)
    {
        Undo.RecordObject(animatorInfo, description);
        if (UFE.autoSaveAssets) AssetDatabase.SaveAssets();
    }

    // Animation Preview
    public void AnimationSampler(int castingFrame)
    {
        GlobalInfo config = UFE.GetActiveConfig();
        float animTime = (castingFrame / (float)config.fps) * (float)animatorInfo.speed;

        HitBoxesScript hitBoxesScript = goInstance.GetComponent<HitBoxesScript>();
        hitBoxesScript.previewAllBoxes = false;
        hitBoxesScript.hitBoxes = hitBoxesScript.GenerateHitBoxes(castingFrame, animatorInfo);

        animatorInfo.clip.SampleAnimation(goInstance, animTime);
    }

    /*public HitBox[] GenerateHitBoxes(int castingFrame)
    {
        List<HitBox> hitBoxList = new List<HitBox>();
        foreach(CustomHitBox hitboxDef in animatorInfo.customHitBoxes)
        {
            if (!hitboxDef.activeFrames[castingFrame].active) continue;

            HitBox hitBox = new HitBox();
            hitBox.nameTag = hitboxDef.name;
            hitBox.position = goInstance.transform;
            hitBox.collisionType = hitboxDef.collisionType;
            hitBox.type = hitboxDef.hitBoxType;
            hitBox.shape = hitboxDef.shape;
            hitBox._radius = hitboxDef.activeFrames[castingFrame].radius;
            hitBox._rect = new FPRect(0, 0, hitboxDef.activeFrames[castingFrame].cubeWidth, hitboxDef.activeFrames[castingFrame].cubeHeight);
            hitBox.mappedPosition = hitboxDef.activeFrames[castingFrame].position;

            hitBoxList.Add(hitBox);
        }

        return hitBoxList.ToArray();
    }*/


    // Grid Options
    public void ShowPositionPopUp(FrameDefinition frameDef, CustomHitBox hitBoxDef, HitBoxShape shape)
    {
        // Set pop-up position based on editor screen size
        Vector2 windowPos = mousePos;
        if (position.width - mousePos.x < 260) windowPos.x = mousePos.x - 260;
        if (position.height - mousePos.y < 100) windowPos.y = mousePos.y - 100;

        // Store definitions for the pop-up
        targetFrameDefinition = frameDef;
        targetHitBoxDefinition = hitBoxDef;

        if (shape == HitBoxShape.rectangle)
        {
            popUpRect = new Rect(windowPos, new Vector2(260, 100));
        }
        else
        {
            popUpRect = new Rect(windowPos, new Vector2(260, 75));
        }
        showOverlayBox = true;
    }

    public void AddActiveFrames(FrameDefinition targetFrame)
    {
        RecordChange(animatorInfo, "Add Frames");
        targetFrame.range = 1;

        RefreshPreview();
    }

    public void SplitActiveFrames(int firstFrame, int currentFrame, FrameDefinition firstFrameDef, FrameDefinition targetFrameDef)
    {
        if (firstFrameDef.range < 2) return;

        RecordChange(animatorInfo, "Split Frames");

        targetFrameDef.range = (firstFrame + firstFrameDef.range) - currentFrame;
        firstFrameDef.range -= targetFrameDef.range;
    }

    public void DeleteThisFrame(int firstFrame, int currentFrame, FrameDefinition firstFrameDef, FrameDefinition targetFrameDef)
    {
        RecordChange(animatorInfo, "Delete Frame");

        // If definition has only one frame remove it
        if (firstFrameDef.range == 1)
        {
            firstFrameDef.range = 0;
        }
        // If frame is last frame, reduce one from range
        else if (currentFrame == firstFrame + firstFrameDef.range)
        {
            firstFrameDef.range -= 1;
        }
        // If frame is first frame, reduce one from range
        else if (currentFrame == firstFrame)
        {
            targetFrameDef.range = firstFrameDef.range - 1;
            firstFrameDef.range = 0;

        }
        // If any other frame, split and delete
        else
        {
            targetFrameDef.range = firstFrameDef.range - (currentFrame - firstFrame);
            firstFrameDef.range -= targetFrameDef.range;
            targetFrameDef.range -= 1;
        }

        RefreshPreview();
    }

    public void DeleteActiveFrames(FrameDefinition firstFrameDef)
    {
        RecordChange(animatorInfo, "Delete Frames");

        firstFrameDef.range = 0;

        RefreshPreview();
    }

    public void PasteActiveFrames(FrameDefinition targetFrame, int maxWidth)
    {
        if (CloneObject.objCopy == null || CloneObject.objCopy.GetType() != typeof(FrameDefinition)) return;

        RecordChange(animatorInfo, "Paste Frames");

        targetFrame.position = (CloneObject.objCopy as FrameDefinition).position;
        targetFrame.radius = (CloneObject.objCopy as FrameDefinition).radius;
        targetFrame.cubeWidth = (CloneObject.objCopy as FrameDefinition).cubeWidth;
        targetFrame.cubeHeight = (CloneObject.objCopy as FrameDefinition).cubeHeight;
        targetFrame.range = Mathf.Min(maxWidth, (CloneObject.objCopy as FrameDefinition).range);

        RefreshPreview();
    }


    // Pane Options
    public void PaneOptions<T>(T[] elements, T element, System.Action<T[]> callback)
    {
        if (elements == null || elements.Length == 0) return;

        GenericMenu toolsMenu = new GenericMenu();
        string activeOption = "";

        activeOption = ((element as CustomHitBox).shape == HitBoxShape.circle) ? "🗸 " : "    ";
        toolsMenu.AddItem(new GUIContent("Shape/" + activeOption + "Sphere"), false, delegate () { SetShape(element as CustomHitBox, HitBoxShape.circle); });

        activeOption = ((element as CustomHitBox).shape == HitBoxShape.rectangle) ? "🗸 " : "    ";
        toolsMenu.AddItem(new GUIContent("Shape/" + activeOption + "Rectangle"), false, delegate () { SetShape(element as CustomHitBox, HitBoxShape.rectangle); });

        activeOption = ((element as CustomHitBox).collisionType == CollisionType.bodyCollider) ? "🗸 " : "    ";
        toolsMenu.AddItem(new GUIContent("Collision Type/" + activeOption + "Body Collider"), false, delegate () { SetCollisionType(element as CustomHitBox, CollisionType.bodyCollider); });

        activeOption = ((element as CustomHitBox).collisionType == CollisionType.hitCollider) ? "🗸 " : "    ";
        toolsMenu.AddItem(new GUIContent("Collision Type/" + activeOption + "Hit Collider"), false, delegate () { SetCollisionType(element as CustomHitBox, CollisionType.hitCollider); });

        activeOption = ((element as CustomHitBox).collisionType == CollisionType.throwCollider) ? "🗸 " : "    ";
        toolsMenu.AddItem(new GUIContent("Collision Type/" + activeOption + "Throw Collider"), false, delegate () { SetCollisionType(element as CustomHitBox, CollisionType.throwCollider); });

        activeOption = ((element as CustomHitBox).collisionType == CollisionType.physicalInvincibleCollider) ? "🗸 " : "    ";
        toolsMenu.AddItem(new GUIContent("Collision Type/" + activeOption + "Projectile Only Collider"), false, delegate () { SetCollisionType(element as CustomHitBox, CollisionType.physicalInvincibleCollider); });

        activeOption = ((element as CustomHitBox).collisionType == CollisionType.projectileInvincibleCollider) ? "🗸 " : "    ";
        toolsMenu.AddItem(new GUIContent("Collision Type/" + activeOption + "Physical Only Collider"), false, delegate () { SetCollisionType(element as CustomHitBox, CollisionType.projectileInvincibleCollider); });

        activeOption = ((element as CustomHitBox).collisionType == CollisionType.noCollider) ? "🗸 " : "    ";
        toolsMenu.AddItem(new GUIContent("Collision Type/"+ activeOption + "No Collider"), false, delegate () { SetCollisionType(element as CustomHitBox, CollisionType.noCollider); });

        activeOption = ((element as CustomHitBox).hitBoxType == HitBoxType.high) ? "🗸 " : "    ";
        toolsMenu.AddItem(new GUIContent("Hit Type/" + activeOption + "High"), false, delegate () { SetHitType(element as CustomHitBox, HitBoxType.high); });

        activeOption = ((element as CustomHitBox).hitBoxType == HitBoxType.low) ? "🗸 " : "    ";
        toolsMenu.AddItem(new GUIContent("Hit Type/" + activeOption + "Low"), false, delegate () { SetHitType(element as CustomHitBox, HitBoxType.low); });

        toolsMenu.AddSeparator("");

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

        List<T> eList = new List<T>(elements);
        toolsMenu.AddItem(new GUIContent("Add New Above"), false, delegate () { AddHitBoxDefinition(new CustomHitBox(), eList.IndexOf(element)); });

        toolsMenu.AddItem(new GUIContent("Rename"), false, delegate () { RenameHitBox(eList.IndexOf(element)); });


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

    public void SetCollisionType(CustomHitBox hitBoxDef, CollisionType collisionType)
    {
        RecordChange(animatorInfo, "Changed Collision Type");

        hitBoxDef.collisionType = collisionType;

        RefreshPreview();
    }

    public void SetShape(CustomHitBox hitBoxDef, HitBoxShape shape)
    {
        RecordChange(animatorInfo, "Changed Shape");

        hitBoxDef.shape = shape;

        RefreshPreview();
    }

    public void SetHitType(CustomHitBox hitBoxDef, HitBoxType hitBoxType)
    {
        RecordChange(animatorInfo, "Changed Hit Type");

        hitBoxDef.hitBoxType = hitBoxType;

        RefreshPreview();
    }

    public void RenameHitBox(int index = -1)
    {
        renameFocus = false;
        renamingTarget = index;
    }

    public void AddHitBoxDefinition(CustomHitBox hitBoxDef, int index = -1)
    {
        RecordChange(animatorInfo, "New Hit Box");

        List<CustomHitBox> hitBoxDefList = new List<CustomHitBox>(animatorInfo.customHitBoxes);
        if (index >= 0)
        {
            hitBoxDefList.Insert(index, hitBoxDef);
        }
        else
        {
            hitBoxDefList.Add(hitBoxDef);
            index = hitBoxDefList.Count - 1;
        }
        animatorInfo.customHitBoxes = hitBoxDefList.ToArray();
        animatorInfo.customHitBoxes[index].activeFrames = new FrameDefinition[animatorInfo.totalFrames];
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

    public T[] AddElementAt<T>(T[] elements, T element, int index)
    {
        List<T> elementsList = new List<T>(elements);
        elementsList.Insert(index, element);
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

    public void RefreshPreview()
    {
        EditorUtility.SetDirty(animatorInfo);
        if (goInstance != null) AnimationSampler(currentPreviewFrame);
    }


    // Centered Button
    public bool StyledButton(string label)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        bool clickResult = GUILayout.Button(label, buttonStyle);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        return clickResult;
    }


    // Get hit box color definition from UFE Config
    private Color GetHitBoxColor(CollisionType collisionType)
    {
        GlobalInfo config = UFE.GetActiveConfig();
        switch (collisionType)
        {
            case CollisionType.bodyCollider:
                return config.colorBodyCollider;
            case CollisionType.hitCollider:
                return config.colorHitCollider;
            case CollisionType.noCollider:
                return config.colorNoCollider;
            case CollisionType.throwCollider:
                return config.colorThrowCollider;
            case CollisionType.projectileInvincibleCollider:
                return config.colorProjectileInvincibleCollider;
            case CollisionType.physicalInvincibleCollider:
                return config.colorPhysicalInvincibleCollider;
            default:
                return GUI.backgroundColor;
        }
    }


    // Resize frame definitions in case of animation change
    public void ResizeFrameDefinitions()
    {
        foreach (CustomHitBox hitBoxDef in animatorInfo.customHitBoxes)
        {
            List<FrameDefinition> frameDefinitions = new List<FrameDefinition>(hitBoxDef.activeFrames);
            frameDefinitions = ResizeList(frameDefinitions, animatorInfo.totalFrames);
            hitBoxDef.activeFrames = frameDefinitions.ToArray();
        }
    }

    private static List<T> ResizeList<T>(List<T> list, int size, T element = default(T))
    {
        int count = list.Count;

        if (size < count)
        {
            list.RemoveRange(size, count - size);
        }
        else if (size > count)
        {
            if (size > list.Capacity)   // Optimization
                list.Capacity = size;

            list.AddRange(System.Linq.Enumerable.Repeat(element, size - count));
        }

        return list;
    }
}