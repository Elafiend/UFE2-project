using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using FPLibrary;
using UFE3D;

public class CharacterEditorWindow : EditorWindow {
	public static CharacterEditorWindow characterEditorWindow;
	public static UFE3D.CharacterInfo sentCharacterInfo;
	private UFE3D.CharacterInfo characterInfo;
    private Dictionary<int, MoveSetData> instantiatedMoveSet = new Dictionary<int, MoveSetData>();

	private Vector2 scrollPos;
	private GameObject character;
	private HitBoxesScript hitBoxesScript;
	private bool characterPreviewToggle;

	private int bloodTypeChoice;
    private string[] bloodTypeChoices = new string[] { "Unknown", "O-", "O+", "A-", "A+", "B-", "B+", "AB-", "AB+" };

    private int selectedPrefabIndex;
    private GameObject selectedPrefab;
    private string prefabResourcePath;
	
	private bool prefabsOption;
	private bool hitBoxesOption;
	private bool hitBoxesToggle;
	private bool bendingSegmentsToggle;
    private bool nonAffectedJointsToggle;
    private bool altCostumesToggle;

	private bool physicsOption;
	private bool headLookOption;
	private bool customControlsOption;
	private bool gaugeDisplayOptions;
	private bool inputOptions;
	private bool moveSetOption;
	private bool aiInstructionsOption;
	private bool characterWarning;
	private string errorMsg;


	private string titleStyle;
	private string addButtonStyle;
	private string rootGroupStyle;
	private string subGroupStyle;
	private string arrayElementStyle;
	private string subArrayElementStyle;
	private string toggleStyle;
	private string foldStyle;
	private string enumStyle;

	[MenuItem("Window/U.F.E./Character Editor")]
	public static void Init(){
		characterEditorWindow = EditorWindow.GetWindow<CharacterEditorWindow>(false, "Character", true);
		characterEditorWindow.Show();
		characterEditorWindow.Populate();
    }
	
	void OnSelectionChange(){
		Populate();
		Repaint();
	}
	
	void OnEnable(){
		Populate();
	}
	
	void OnFocus(){
		Populate();
	}

	void OnDisable(){
		ClosePreview();
	}
	
	void OnDestroy(){
		ClosePreview();
	}
	
	void OnLostFocus(){
		//ClosePreview();
	}

	public void PreviewCharacter(bool hasAnimator = false){
        if (selectedPrefab != null) return;
		characterPreviewToggle = true;
		EditorCamera.SetPosition(Vector3.up * 3.5f);
		EditorCamera.SetRotation(Quaternion.identity);
		EditorCamera.SetOrthographic(true);
		EditorCamera.SetSize(8);
		if (character != null){
			character = (GameObject) PrefabUtility.InstantiatePrefab(selectedPrefab);
			character.transform.position = new Vector3(0,0,0);
        }
		hitBoxesScript = character.GetComponent<HitBoxesScript>();
        hitBoxesScript.animationMaps = null;
        hitBoxesScript.previewAllBoxes = false;
        hitBoxesScript.UpdateRenderer();

        if (hasAnimator)
        {
            Animator animator = character.GetComponent<Animator>();
            if (animator == null)
            {
                animator = character.AddComponent<Animator>();
                animator.avatar = characterInfo.avatar;
                if (animator.runtimeAnimatorController == null)
                    animator.runtimeAnimatorController = (RuntimeAnimatorController)Resources.Load("MC_Controller");
            }
        }
	}

    public void ClosePreview(){
		characterPreviewToggle = false;
		if (character != null){
			DestroyImmediate(character);
			character = null;
		}
        selectedPrefab = null;
		hitBoxesScript = null;
	}

	void helpButton(string page){
		if (GUILayout.Button("?", GUILayout.Width(18), GUILayout.Height(18))) 
			Application.OpenURL("http://www.ufe3d.com/doku.php/"+ page);
	}
    
	void Update(){
		if (EditorApplication.isPlayingOrWillChangePlaymode && character != null) {
			ClosePreview();
		}
    }

	void Populate(){
        this.titleContent = new GUIContent("Character", (Texture) Resources.Load("Icons/Character"));

		// Style Definitions
		titleStyle = "MeTransOffRight";
		addButtonStyle = "CN CountBadge";
		rootGroupStyle = "GroupBox";
		subGroupStyle = "ObjectFieldThumb";
        arrayElementStyle = "FrameBox";
        subArrayElementStyle = "HelpBox";
		foldStyle = "Foldout";
		enumStyle = "MiniPopup";
		toggleStyle = "BoldToggle";


#if UFE_LITE
		UFE.isAiAddonInstalled = false;
#else
        UFE.isAiAddonInstalled = UFE.IsInstalled("RuleBasedAI");
#endif
        UFE.isControlFreak2Installed = UFE.IsInstalled("ControlFreak2.UFEBridge");

		if (sentCharacterInfo != null){
			EditorGUIUtility.PingObject( sentCharacterInfo );
			Selection.activeObject = sentCharacterInfo;
			sentCharacterInfo = null;
		}

		UnityEngine.Object[] selection = Selection.GetFiltered(typeof(UFE3D.CharacterInfo), SelectionMode.Assets);
		if (selection.Length > 0){
			if (selection[0] == null) return;
			characterInfo = (UFE3D.CharacterInfo) selection[0];
		}
	}
	
	public void OnGUI(){
		if (characterInfo == null){
			GUILayout.BeginHorizontal("GroupBox");
            GUILayout.Label("Select a character file\nor create a new character.", "CN EntryInfo");
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();
			if (GUILayout.Button("Create new character"))
				ScriptableObjectUtility.CreateAsset<UFE3D.CharacterInfo> ();
			return;
		}

		GUIStyle fontStyle = new GUIStyle();
        fontStyle.font = (Font) Resources.Load("EditorFont");
		fontStyle.fontSize = 30;
		fontStyle.alignment = TextAnchor.UpperCenter;
		fontStyle.normal.textColor = Color.white;
		fontStyle.hover.textColor = Color.white;
		EditorGUILayout.BeginVertical(titleStyle);{
			EditorGUILayout.BeginHorizontal();{
#if !UFE_LITE && !UFE_BASIC
                EditorGUILayout.LabelField("", characterInfo.characterName == ""? "New Character" : characterInfo.characterName + " " + (characterInfo.gameplayType == GameplayType._2DFighter ? "(2D)" : "(3D)"), fontStyle, GUILayout.Height(32));
#else
                EditorGUILayout.LabelField("", characterInfo.characterName == ""? "New Character" : characterInfo.characterName, fontStyle, GUILayout.Height(32));
#endif
                helpButton("character:start");
			}EditorGUILayout.EndHorizontal();
		}EditorGUILayout.EndVertical();
		
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);{
			EditorGUILayout.BeginVertical(rootGroupStyle);{
#if !UFE_LITE && !UFE_BASIC
                characterInfo.gameplayType = (GameplayType)EditorGUILayout.EnumPopup("Gameplay Type:", characterInfo.gameplayType, enumStyle);
#endif
                EditorGUILayout.BeginHorizontal();
                {
                    characterInfo.profilePictureSmall = (Texture2D) EditorGUILayout.ObjectField(characterInfo.profilePictureSmall, typeof(Texture2D), false, GUILayout.Width(100), GUILayout.Height(122));

					EditorGUILayout.BeginVertical();{
						EditorGUIUtility.labelWidth = 90;
                        characterInfo.characterName = EditorGUILayout.TextField("Name:", characterInfo.characterName);
						characterInfo.age = EditorGUILayout.IntField("Age:", characterInfo.age);
						bloodTypeChoice = EditorGUILayout.Popup("Blood Type:", bloodTypeChoice, bloodTypeChoices);
						characterInfo.bloodType = bloodTypeChoices[bloodTypeChoice];
						characterInfo.gender = (Gender) EditorGUILayout.EnumPopup("Gender:", characterInfo.gender);
						characterInfo.height = EditorGUILayout.FloatField("Height:", characterInfo.height);
						characterInfo.lifePoints = EditorGUILayout.IntField("Life Points:", characterInfo.lifePoints);
						characterInfo.maxGaugePoints = EditorGUILayout.IntField("Max Gauge:", characterInfo.maxGaugePoints);
					} EditorGUILayout.EndVertical();
				}
                EditorGUILayout.EndHorizontal();

				EditorGUIUtility.labelWidth = 180;
				EditorGUILayout.LabelField("Portrail Big:");
				characterInfo.profilePictureBig = (Texture2D) EditorGUILayout.ObjectField(characterInfo.profilePictureBig, typeof(Texture2D), false);
				EditorGUILayout.Space();

				characterInfo.selectionAnimation = (AnimationClip) EditorGUILayout.ObjectField("Selection Animation:", characterInfo.selectionAnimation, typeof(UnityEngine.AnimationClip), false);
				characterInfo.selectionSound = (AudioClip) EditorGUILayout.ObjectField("Selection Sound:", characterInfo.selectionSound, typeof(UnityEngine.AudioClip), false);
				characterInfo.deathSound = (AudioClip) EditorGUILayout.ObjectField("Death Sound:", characterInfo.deathSound, typeof(UnityEngine.AudioClip), false);

				EditorGUILayout.Space();
				EditorGUIUtility.labelWidth = 200;
				GUILayout.Label("Description:");
				Rect rect = GUILayoutUtility.GetRect(50, 90);
				EditorStyles.textField.wordWrap = true;
				characterInfo.characterDescription = EditorGUI.TextArea(rect, characterInfo.characterDescription);
				
				EditorGUILayout.Space();
				EditorGUIUtility.labelWidth = 150;

				EditorGUILayout.Space();
			}EditorGUILayout.EndVertical();

            
			// Character Prefabs
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
                    prefabsOption = EditorGUILayout.Foldout(prefabsOption, "Character Prefabs", foldStyle);
					helpButton("character:prefabs");
				}EditorGUILayout.EndHorizontal();


                if (prefabsOption) {
                    EditorGUILayout.BeginVertical(subGroupStyle);{
                        EditorGUI.indentLevel += 1;


                        StorageMode newPrefabPrefabStorage = (StorageMode)EditorGUILayout.EnumPopup("Loading Method:", characterInfo.characterPrefabStorage);
                        if (newPrefabPrefabStorage == StorageMode.SceneFile) newPrefabPrefabStorage = StorageMode.ResourcesFolder;
                        if (characterInfo.characterPrefabStorage != newPrefabPrefabStorage) {
                            characterInfo.characterPrefabStorage = newPrefabPrefabStorage;
                            characterInfo.characterPrefab = null;
                        }

                        if (characterInfo.characterPrefabStorage == StorageMode.Prefab) {
                            characterInfo.characterPrefab = (GameObject)EditorGUILayout.ObjectField("Default Prefab:", characterInfo.characterPrefab, typeof(UnityEngine.GameObject), true);
                        } else {
                            characterInfo.prefabResourcePath = EditorGUILayout.TextField("Default Prefab Path:", characterInfo.prefabResourcePath);
                        }

                        ValidatePrefab(characterInfo.characterPrefab, false);
                        if (characterWarning) {
                            GUILayout.BeginHorizontal("GroupBox");
                            GUILayout.Label(errorMsg, "CN EntryWarn");
                            GUILayout.EndHorizontal();
                        }

                        altCostumesToggle = EditorGUILayout.Foldout(altCostumesToggle, "Alternative Costumes (" + characterInfo.alternativeCostumes.Length + ")", foldStyle);
                        if (altCostumesToggle) {
                            EditorGUILayout.BeginVertical(subGroupStyle);
                            {
                                EditorGUILayout.Space();
                                EditorGUI.indentLevel += 1;

                                for (int i = 0; i < characterInfo.alternativeCostumes.Length; i++) {
                                    EditorGUILayout.Space();
                                    EditorGUILayout.BeginVertical(arrayElementStyle);
                                    {
                                        EditorGUILayout.Space();
                                        EditorGUILayout.BeginHorizontal();
                                        {
                                            characterInfo.alternativeCostumes[i].name = EditorGUILayout.TextField("Name:", characterInfo.alternativeCostumes[i].name);
                                            if (GUILayout.Button("", "PaneOptions")) {
                                                PaneOptions<AltCostume>(characterInfo.alternativeCostumes, characterInfo.alternativeCostumes[i], delegate(AltCostume[] newElement) { characterInfo.alternativeCostumes = newElement; });
                                            }
                                        } EditorGUILayout.EndHorizontal();

                                        StorageMode newAltPrefabStorage = (StorageMode)EditorGUILayout.EnumPopup("Loading Method:", characterInfo.alternativeCostumes[i].characterPrefabStorage);
                                        if (characterInfo.alternativeCostumes[i].characterPrefabStorage != newAltPrefabStorage)
                                        {
                                            characterInfo.alternativeCostumes[i].characterPrefabStorage = newAltPrefabStorage;
                                            characterInfo.alternativeCostumes[i].prefab = null;
                                        }
                                        
                                        if (characterInfo.alternativeCostumes[i].characterPrefabStorage == StorageMode.Prefab) {
                                            characterInfo.alternativeCostumes[i].prefab = (GameObject)EditorGUILayout.ObjectField("Alternative Prefab:", characterInfo.alternativeCostumes[i].prefab, typeof(UnityEngine.GameObject), true);
                                        } else {
                                            characterInfo.alternativeCostumes[i].prefabResourcePath = EditorGUILayout.TextField("Prefab Path:", characterInfo.alternativeCostumes[i].prefabResourcePath);
                                        }
                                        ValidatePrefab(characterInfo.alternativeCostumes[i].prefab, true);

                                        characterInfo.alternativeCostumes[i].enableColorMask = EditorGUILayout.Toggle("Enable Color Mask", characterInfo.alternativeCostumes[i].enableColorMask);
                                        if (characterInfo.alternativeCostumes[i].enableColorMask) characterInfo.alternativeCostumes[i].colorMask = EditorGUILayout.ColorField("Color Mask:", characterInfo.alternativeCostumes[i].colorMask);

                                        if (characterWarning) {
                                            GUILayout.BeginHorizontal("GroupBox");
                                            GUILayout.Label(errorMsg, "CN EntryWarn");
                                            GUILayout.EndHorizontal();
                                        }

                                        EditorGUILayout.Space();
                                    } EditorGUILayout.EndVertical();
                                }

                                if (StyledButton("New Prefab"))
                                    characterInfo.alternativeCostumes = AddElement<AltCostume>(characterInfo.alternativeCostumes, new AltCostume());

                                EditorGUILayout.Space();
                                EditorGUI.indentLevel -= 1;

                            } EditorGUILayout.EndVertical();
                        }

                        EditorGUILayout.Space();
                        EditorGUI.indentLevel -= 1;
                    } EditorGUILayout.EndVertical();

                }

            }EditorGUILayout.EndVertical();


			// Hit Boxes
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					hitBoxesOption = EditorGUILayout.Foldout(hitBoxesOption, "Hit Box Setup", foldStyle);
					helpButton("character:hitbox");
				}EditorGUILayout.EndHorizontal();

				if (hitBoxesOption){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUI.indentLevel += 1;
					
						/*characterInfo.characterPrefab = (GameObject) EditorGUILayout.ObjectField("Character Prefab:", characterInfo.characterPrefab, typeof(UnityEngine.GameObject), true);
						if (characterInfo.characterPrefab != null){
							if (PrefabUtility.GetPrefabType(characterInfo.characterPrefab) != PrefabType.Prefab){
								characterWarning = true;
								errorMsg = "This character is not a prefab.";
								characterInfo.characterPrefab = null;
								ClosePreview();
							}else if (characterInfo.characterPrefab.GetComponent<HitBoxesScript>() == null){
								characterWarning = true;
								errorMsg = "This character doesn't have hitboxes!\n Please add the HitboxesScript and try again.";
								characterInfo.characterPrefab = null;
								ClosePreview();
							}else if (character != null && EditorApplication.isPlayingOrWillChangePlaymode) {
								characterWarning = true;
								errorMsg = "You can't change this field while in play mode.";
								ClosePreview();
							}else {
								characterWarning = false;
								if (character != null && characterInfo.characterPrefab.name != character.name) ClosePreview();
							}
						}

						if (characterWarning){
							GUILayout.BeginHorizontal("GroupBox");
							GUILayout.Label(errorMsg, "CN EntryWarn");
							GUILayout.EndHorizontal();
						}*/

                        int arraySize = characterInfo.alternativeCostumes.Length > 0 ? (characterInfo.alternativeCostumes.Length + 1) : 1;
                        string[] prefabSelect = new string[arraySize];
                        prefabSelect[0] = "Default";
                        for (int i = 0; i < characterInfo.alternativeCostumes.Length; i++) {
                            if (characterInfo.alternativeCostumes[i].prefab != null) {
                                prefabSelect[i + 1] = characterInfo.alternativeCostumes[i].name;
                            }
                        }
                        selectedPrefabIndex = EditorGUILayout.Popup("Prefab Selection:", selectedPrefabIndex, prefabSelect);

                        if (selectedPrefabIndex > 0) {
                            if (characterInfo.alternativeCostumes[selectedPrefabIndex - 1].characterPrefabStorage == StorageMode.Prefab) {
                                selectedPrefab = characterInfo.alternativeCostumes[selectedPrefabIndex - 1].prefab;
                            } else {
                                prefabResourcePath = characterInfo.alternativeCostumes[selectedPrefabIndex - 1].prefabResourcePath;
                                selectedPrefab = Resources.Load<GameObject>(prefabResourcePath);
                            }
                        } else {
                            if (characterInfo.characterPrefabStorage == StorageMode.Prefab) {
                                selectedPrefab = characterInfo.characterPrefab;
                            } else {
                                prefabResourcePath = characterInfo.prefabResourcePath;
                                selectedPrefab = Resources.Load<GameObject>(prefabResourcePath);
                            }
                        }

                        if (selectedPrefab != null) {
							
							if (character != null)
                            {
                                EditorGUILayout.BeginVertical(subGroupStyle);{	
									EditorGUILayout.Space();
									//transformToggle = EditorGUILayout.Foldout(transformToggle, "Transform", EditorStyles.foldout);
									//if (transformToggle){
										EditorGUILayout.BeginVertical(subArrayElementStyle);{
											EditorGUI.indentLevel += 1;
											character.transform.position = EditorGUILayout.Vector3Field("Position", character.transform.position);
                                            characterInfo.initialPosition = FPVector.ToFPVector(character.transform.position);
											character.transform.rotation = Quaternion.Euler(EditorGUILayout.Vector3Field("Rotation", character.transform.rotation.eulerAngles));
                                            characterInfo.initialRotation = FPQuaternion.ToFPQuaternion(character.transform.rotation);
                                            character.transform.localScale = EditorGUILayout.Vector3Field("Scale", character.transform.localScale);
                                            EditorGUILayout.Space();
											EditorGUI.indentLevel -= 1;
										}EditorGUILayout.EndVertical();
									//}
									
									EditorGUILayout.Space();

                                    if (characterInfo.animationType == AnimationType.Mecanim3D && StyledButton("Force T-Pose")) {
                                        AnimationClip animationClip = (AnimationClip)Resources.Load("T-Pose");
                                        animationClip.SampleAnimation(character, 1);
                                    }

                                    EditorGUI.BeginDisabledGroup(FindTransform("Head") == null);{
										if (StyledButton("Auto-Setup for Mixamo Auto-rigger")){
											if (EditorUtility.DisplayDialog("Replace Hitboxes?", 
											                                "This action will delete your current hitbox setup. Are you sure you want to replace it?",
											                                "Yes", "No")){
												hitBoxesScript.hitBoxes = new HitBox[0];
												for(int i = 0; i <= 14; i ++){
													HitBox newHitBox = new HitBox();
													if (i == 0){
														newHitBox.bodyPart = BodyPart.head;
														newHitBox.collisionType = CollisionType.bodyCollider;
														newHitBox.type = HitBoxType.high;
														newHitBox._radius = .7;
														newHitBox.position = FindTransform("Head");
													}else if (i == 1){
														newHitBox.bodyPart = BodyPart.upperTorso;
														newHitBox.collisionType = CollisionType.bodyCollider;
														newHitBox.type = HitBoxType.high;
														newHitBox._radius = .9f;
														newHitBox.position = FindTransform("Spine2");
													}else if (i == 2){
														newHitBox.bodyPart = BodyPart.lowerTorso;
														newHitBox.collisionType = CollisionType.bodyCollider;
														newHitBox.type = HitBoxType.low;
														newHitBox._radius = .9f;
														newHitBox.position = FindTransform("Spine");
													}else if (i == 3){
														newHitBox.bodyPart = BodyPart.leftUpperArm;
														newHitBox.collisionType = CollisionType.hitCollider;
														newHitBox.type = HitBoxType.high;
														newHitBox._radius = .5f;
														newHitBox.position = FindTransform("LeftArm");
													}else if (i == 4){
														newHitBox.bodyPart = BodyPart.rightUpperArm;
														newHitBox.collisionType = CollisionType.hitCollider;
														newHitBox.type = HitBoxType.high;
														newHitBox._radius = .5f;
														newHitBox.position = FindTransform("RightArm");
													}else if (i == 5){
														newHitBox.bodyPart = BodyPart.leftForearm;
														newHitBox.collisionType = CollisionType.noCollider;
														newHitBox._radius = .5f;
														newHitBox.position = FindTransform("LeftForeArm");
													}else if (i == 6){
														newHitBox.bodyPart = BodyPart.rightForearm;
														newHitBox.collisionType = CollisionType.noCollider;
														newHitBox._radius = .5f;
														newHitBox.position = FindTransform("RightForeArm");
													}else if (i == 7){
														newHitBox.bodyPart = BodyPart.leftHand;
														newHitBox.collisionType = CollisionType.noCollider;
														newHitBox._radius = .5f;
														newHitBox.position = FindTransform("LeftHand");
													}else if (i == 8){
														newHitBox.bodyPart = BodyPart.rightHand;
														newHitBox.collisionType = CollisionType.noCollider;
														newHitBox._radius = .5f;
														newHitBox.position = FindTransform("RightHand");
													}else if (i == 9){
														newHitBox.bodyPart = BodyPart.leftThigh;
														newHitBox.collisionType = CollisionType.hitCollider;
														newHitBox.type = HitBoxType.low;
														newHitBox._radius = .5f;
														newHitBox.position = FindTransform("LeftUpLeg");
													}else if (i == 10){
														newHitBox.bodyPart = BodyPart.rightThigh;
														newHitBox.collisionType = CollisionType.hitCollider;
														newHitBox.type = HitBoxType.low;
														newHitBox._radius = .5f;
														newHitBox.position = FindTransform("RightUpLeg");
													}else if (i == 11){
														newHitBox.bodyPart = BodyPart.leftCalf;
														newHitBox.collisionType = CollisionType.hitCollider;
														newHitBox.type = HitBoxType.low;
														newHitBox._radius = .5f;
														newHitBox.position = FindTransform("LeftLeg");
													}else if (i == 12){
														newHitBox.bodyPart = BodyPart.rightCalf;
														newHitBox.collisionType = CollisionType.hitCollider;
														newHitBox.type = HitBoxType.low;
														newHitBox._radius = .5f;
														newHitBox.position = FindTransform("RightLeg");
													}else if (i == 13){
														newHitBox.bodyPart = BodyPart.leftFoot;
														newHitBox.collisionType = CollisionType.hitCollider;
														newHitBox.type = HitBoxType.low;
														newHitBox._radius = .5f;
														newHitBox.position = FindTransform("LeftFoot");
													}else if (i == 14){
														newHitBox.bodyPart = BodyPart.rightFoot;
														newHitBox.collisionType = CollisionType.hitCollider;
														newHitBox.type = HitBoxType.low;
														newHitBox._radius = .5f;
														newHitBox.position = FindTransform("RightFoot");
													}
													hitBoxesScript.hitBoxes = AddElement<HitBox>(hitBoxesScript.hitBoxes, newHitBox);
												}
											}
										}
									}EditorGUI.EndDisabledGroup();
									
									hitBoxesToggle = EditorGUILayout.Foldout(hitBoxesToggle, "Hit Boxes", EditorStyles.foldout);
									if (hitBoxesToggle){
										EditorGUILayout.BeginVertical(subGroupStyle);{
											hitBoxesScript.collisionBoxSize = characterInfo.physics._groundCollisionMass;
											for (int i = 0; i < hitBoxesScript.hitBoxes.Length; i ++){
												EditorGUILayout.Space();
												EditorGUILayout.BeginVertical(subArrayElementStyle);{
													EditorGUILayout.Space();
													EditorGUILayout.BeginHorizontal();{
														hitBoxesScript.hitBoxes[i].bodyPart = (BodyPart) EditorGUILayout.EnumPopup("Body Part:", hitBoxesScript.hitBoxes[i].bodyPart, enumStyle);
														if (GUILayout.Button("", "PaneOptions")){
															PaneOptions<HitBox>(hitBoxesScript.hitBoxes, hitBoxesScript.hitBoxes[i], delegate (HitBox[] newElement) { hitBoxesScript.hitBoxes = newElement; });
														}
													}EditorGUILayout.EndHorizontal();
													hitBoxesScript.hitBoxes[i].position = (Transform) EditorGUILayout.ObjectField("Link:", hitBoxesScript.hitBoxes[i].position, typeof(UnityEngine.Transform), true);

                                                    if (characterInfo.gameplayType == GameplayType._2DFighter)
                                                        hitBoxesScript.hitBoxes[i].shape = (HitBoxShape)EditorGUILayout.EnumPopup("Shape:", hitBoxesScript.hitBoxes[i].shape, enumStyle);
                                                    else
                                                        hitBoxesScript.hitBoxes[i].shape = HitBoxShape.circle;

                                                    if (hitBoxesScript.hitBoxes[i].shape == HitBoxShape.circle){
														hitBoxesScript.hitBoxes[i]._radius = EditorGUILayout.Slider("Radius:", (float)hitBoxesScript.hitBoxes[i]._radius, .1f, 10);
														hitBoxesScript.hitBoxes[i]._offSet = FPVector.ToFPVector(EditorGUILayout.Vector2Field("Off Set:", hitBoxesScript.hitBoxes[i]._offSet.ToVector2()));
                                                    }
                                                    else
                                                    {
														hitBoxesScript.hitBoxes[i].rect = EditorGUILayout.RectField("Rectangle:", hitBoxesScript.hitBoxes[i].rect);
														hitBoxesScript.hitBoxes[i]._rect = new FPRect(hitBoxesScript.hitBoxes[i].rect);
														
														EditorGUIUtility.labelWidth = 200;
														bool tmpFollowXBounds = hitBoxesScript.hitBoxes[i].followXBounds;
														bool tmpFollowYBounds = hitBoxesScript.hitBoxes[i].followYBounds;

														hitBoxesScript.hitBoxes[i].followXBounds = EditorGUILayout.Toggle("Follow Character Bounds (X)", hitBoxesScript.hitBoxes[i].followXBounds);
														hitBoxesScript.hitBoxes[i].followYBounds = EditorGUILayout.Toggle("Follow Character Bounds (Y)", hitBoxesScript.hitBoxes[i].followYBounds);

														if (tmpFollowXBounds != hitBoxesScript.hitBoxes[i].followXBounds)
															hitBoxesScript.hitBoxes[i].rect.width = hitBoxesScript.hitBoxes[i].followXBounds ? 0 : 4;
														if (tmpFollowYBounds != hitBoxesScript.hitBoxes[i].followYBounds)
															hitBoxesScript.hitBoxes[i].rect.height = hitBoxesScript.hitBoxes[i].followYBounds ? 0 : 4;

														EditorGUIUtility.labelWidth = 150;
													}

													hitBoxesScript.hitBoxes[i].collisionType = (CollisionType) EditorGUILayout.EnumPopup("Collision Type:", hitBoxesScript.hitBoxes[i].collisionType, enumStyle);
													EditorGUI.BeginDisabledGroup(hitBoxesScript.hitBoxes[i].collisionType == CollisionType.noCollider || hitBoxesScript.hitBoxes[i].collisionType == CollisionType.throwCollider);{
														hitBoxesScript.hitBoxes[i].type = (HitBoxType) EditorGUILayout.EnumPopup("Hit Box Type:", hitBoxesScript.hitBoxes[i].type, enumStyle);
													}EditorGUI.EndDisabledGroup();

                                                    hitBoxesScript.hitBoxes[i].defaultVisibility = EditorGUILayout.Toggle("Default Visibility", hitBoxesScript.hitBoxes[i].defaultVisibility);
													EditorGUILayout.Space();
												}EditorGUILayout.EndVertical();
											}
											if (StyledButton("New Hit Box"))
												hitBoxesScript.hitBoxes = AddElement<HitBox>(hitBoxesScript.hitBoxes, new HitBox());
											
										}EditorGUILayout.EndVertical();
									}
									
									EditorGUILayout.Space();
									EditorGUILayout.BeginHorizontal();{
										if (StyledButton("Reset Scene View")){
											EditorCamera.SetPosition(Vector3.up * 4f);
											EditorCamera.SetRotation(Quaternion.identity);
											EditorCamera.SetOrthographic(true);
											EditorCamera.SetSize(5);
										}
										if (StyledButton("Apply Changes")){
#if UNITY_2018_3_OR_NEWER
                                            PrefabUtility.ApplyPrefabInstance(character, InteractionMode.AutomatedAction);
#elif UNITY_2018_2
                                            PrefabUtility.ReplacePrefab(character, PrefabUtility.GetCorrespondingObjectFromSource(character), ReplacePrefabOptions.ConnectToPrefab);
#else
                                            PrefabUtility.ReplacePrefab(character, PrefabUtility.GetPrefabParent(character), ReplacePrefabOptions.ConnectToPrefab);
#endif
                                            hitBoxesScript.animationMaps = null;
                                        }
                                    }
                                    EditorGUILayout.EndHorizontal();
									EditorGUILayout.Space();
								}EditorGUILayout.EndVertical();
							}

                            if (!characterPreviewToggle) {
                                if (StyledButton("Open Character")) {
                                    EditorWindow.FocusWindowIfItsOpen<SceneView>();
                                    PreviewCharacter(characterInfo.animationType == AnimationType.Mecanim3D || characterInfo.animationType == AnimationType.Mecanim2D);
                                }
                            } else {
                                if (StyledButton("Close Character")) ClosePreview();
                            }
                        } else {
                            GUILayout.BeginHorizontal("GroupBox");
                            GUILayout.Label("Select a character prefab first", "CN EntryWarn");
                            GUILayout.EndHorizontal();
                        }

						EditorGUI.indentLevel -= 1;
					}EditorGUILayout.EndVertical();
				}else{
					ClosePreview();
				}
			}EditorGUILayout.EndVertical();

			// Physics
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					physicsOption = EditorGUILayout.Foldout(physicsOption, "Physics", foldStyle);
					helpButton("character:physics");
				}EditorGUILayout.EndHorizontal();

				if (physicsOption){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;
						EditorGUIUtility.labelWidth = 190;
						SubGroupTitle("Movement");
						characterInfo.physics._moveForwardSpeed = EditorGUILayout.FloatField("Move Forward Speed:", (float)characterInfo.physics._moveForwardSpeed);
						characterInfo.physics._moveBackSpeed = EditorGUILayout.FloatField("Move Back Speed:", (float)characterInfo.physics._moveBackSpeed);
#if !UFE_LITE && !UFE_BASIC
                        if (characterInfo.gameplayType == GameplayType._3DFighter)
                            characterInfo.physics._moveSidewaysSpeed = EditorGUILayout.FloatField("Move Sideways Speed:", (float)characterInfo.physics._moveSidewaysSpeed);
#endif

                        characterInfo.physics.highMovingFriction = EditorGUILayout.Toggle("High Moving Friction", characterInfo.physics.highMovingFriction);
						characterInfo.physics._friction = EditorGUILayout.FloatField("Friction:", (float)characterInfo.physics._friction);
						EditorGUILayout.Space();

                        /*SubGroupTitle("Crouch Options");
                        characterInfo.physics.canCrouch = EditorGUILayout.Toggle("Enable Crouch", characterInfo.physics.canCrouch);
                        EditorGUI.BeginDisabledGroup(!characterInfo.physics.canCrouch);{
                            characterInfo.physics.crouchDelay = EditorGUILayout.IntField("Crouching Delay (frames):", characterInfo.physics.crouchDelay);
                            characterInfo.physics.standingDelay = EditorGUILayout.IntField("Standing Delay (frames):", characterInfo.physics.standingDelay);
                        } EditorGUI.EndDisabledGroup();
                        EditorGUILayout.Space();*/

                        SubGroupTitle("Jump Options");
						characterInfo.physics.canJump = EditorGUILayout.Toggle("Enable Jump", characterInfo.physics.canJump);
                        EditorGUI.BeginDisabledGroup(!characterInfo.physics.canJump);
                        {
                            characterInfo.physics.pressureSensitiveJump = EditorGUILayout.Toggle("Pressure Sensitive", characterInfo.physics.pressureSensitiveJump);
                            if (characterInfo.physics.pressureSensitiveJump) characterInfo.physics._minJumpForce = EditorGUILayout.FloatField("Min. Jump Force:", (float)characterInfo.physics._minJumpForce);
                            characterInfo.physics._jumpForce = EditorGUILayout.FloatField("Jump Force:", (float)characterInfo.physics._jumpForce);
                            characterInfo.physics._jumpDistance = EditorGUILayout.FloatField("Forward Jump Distance:", (float)characterInfo.physics._jumpDistance);
                            characterInfo.physics._jumpBackDistance = EditorGUILayout.FloatField("Backwards Jump Distance:", (float)characterInfo.physics._jumpBackDistance);
                            if (characterInfo.physics.pressureSensitiveJump) characterInfo.physics.minJumpDelay = EditorGUILayout.IntField("Min. Jump Delay (frames):", characterInfo.physics.minJumpDelay);
							characterInfo.physics.jumpDelay = EditorGUILayout.IntField("Jump Delay (frames):", characterInfo.physics.jumpDelay);
							characterInfo.physics.landingDelay = EditorGUILayout.IntField("Landing Delay (frames):", characterInfo.physics.landingDelay);
                            characterInfo.physics.multiJumps = EditorGUILayout.IntField("Air Jumps:", characterInfo.physics.multiJumps);
                            characterInfo.possibleAirMoves = EditorGUILayout.IntField("Possible Air Moves:", characterInfo.possibleAirMoves); // TODO Move to possibleAirMoves to physics
                        }
                        EditorGUI.EndDisabledGroup();
						EditorGUILayout.Space();

                        SubGroupTitle("Move Control");
                        characterInfo._executionTiming = EditorGUILayout.FloatField("Default Execution Timing:", (float)characterInfo._executionTiming);
                        characterInfo.physics.cumulativeForce = EditorGUILayout.Toggle("Cumulative Force", characterInfo.physics.cumulativeForce);
                        EditorGUILayout.Space();

                        SubGroupTitle("Mass Variation");
						characterInfo.physics._weight = EditorGUILayout.FloatField("Character's Weight:", (float)characterInfo.physics._weight);
						characterInfo.physics._groundCollisionMass = EditorGUILayout.FloatField("Ground Collision Mass:", (float)characterInfo.physics._groundCollisionMass);
						EditorGUIUtility.labelWidth = 150;
						EditorGUILayout.Space();

						EditorGUI.indentLevel -= 1;
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();

			// Head Look
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					headLookOption = EditorGUILayout.Foldout(headLookOption, "Head Look", foldStyle);
					helpButton("character:headlook");
				}EditorGUILayout.EndHorizontal();

				if (headLookOption){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;

						characterInfo.headLook.enabled = EditorGUILayout.Toggle("Enabled", characterInfo.headLook.enabled);
						
						EditorGUI.BeginDisabledGroup(!characterInfo.headLook.enabled);{
							bendingSegmentsToggle = EditorGUILayout.Foldout(bendingSegmentsToggle, "Bending Segments", EditorStyles.foldout);
							if (bendingSegmentsToggle){
								EditorGUILayout.BeginVertical(subGroupStyle);{
									for (int i = 0; i < characterInfo.headLook.segments.Length; i ++){
										EditorGUILayout.Space();
										EditorGUILayout.BeginVertical(subArrayElementStyle);{
											EditorGUILayout.Space();
											EditorGUILayout.BeginHorizontal();{
												characterInfo.headLook.segments[i].bodyPart = (BodyPart) EditorGUILayout.EnumPopup("Affected Body Part:", characterInfo.headLook.segments[i].bodyPart, enumStyle);
												if (GUILayout.Button("", "PaneOptions")){
													PaneOptions<BendingSegment>(characterInfo.headLook.segments, characterInfo.headLook.segments[i], delegate (BendingSegment[] newElement) { characterInfo.headLook.segments = newElement; });
												}
											}EditorGUILayout.EndHorizontal();
											EditorGUIUtility.labelWidth = 190;
											characterInfo.headLook.segments[i].responsiveness = EditorGUILayout.FloatField("Responsiveness:", characterInfo.headLook.segments[i].responsiveness);
											characterInfo.headLook.segments[i].thresholdAngleDifference = EditorGUILayout.FloatField("Threshold Angle Difference:", characterInfo.headLook.segments[i].thresholdAngleDifference);
											characterInfo.headLook.segments[i].bendingMultiplier = EditorGUILayout.FloatField("Bending Multiplier:", characterInfo.headLook.segments[i].bendingMultiplier);
											characterInfo.headLook.segments[i].maxAngleDifference = EditorGUILayout.FloatField("Max Angle Difference:", characterInfo.headLook.segments[i].maxAngleDifference);
											characterInfo.headLook.segments[i].maxBendingAngle = EditorGUILayout.FloatField("Max Bending Angle:", characterInfo.headLook.segments[i].maxBendingAngle);
											EditorGUIUtility.labelWidth = 150;
											EditorGUILayout.Space();
										}EditorGUILayout.EndVertical();
									}
									if (StyledButton("New Bending Segment"))
										characterInfo.headLook.segments = AddElement<BendingSegment>(characterInfo.headLook.segments, new BendingSegment());
									
								}EditorGUILayout.EndVertical();
							}
							
							nonAffectedJointsToggle = EditorGUILayout.Foldout(nonAffectedJointsToggle, "Non Affected Joints", EditorStyles.foldout);
							if (nonAffectedJointsToggle){
								EditorGUILayout.BeginVertical(subGroupStyle);{
									for (int i = 0; i < characterInfo.headLook.nonAffectedJoints.Length; i ++){
										EditorGUILayout.Space();
										EditorGUILayout.BeginVertical(subArrayElementStyle);{
											EditorGUILayout.Space();
											EditorGUILayout.BeginHorizontal();{
												characterInfo.headLook.nonAffectedJoints[i].bodyPart = (BodyPart) EditorGUILayout.EnumPopup("Body Part:", characterInfo.headLook.nonAffectedJoints[i].bodyPart, enumStyle);
												if (GUILayout.Button("", "PaneOptions")){
													PaneOptions<NonAffectedJoints>(characterInfo.headLook.nonAffectedJoints, characterInfo.headLook.nonAffectedJoints[i], delegate (NonAffectedJoints[] newElement) { characterInfo.headLook.nonAffectedJoints = newElement; });
												}
											}EditorGUILayout.EndHorizontal();
											characterInfo.headLook.nonAffectedJoints[i].effect = EditorGUILayout.FloatField("Weight:", characterInfo.headLook.nonAffectedJoints[i].effect);
											EditorGUILayout.Space();
										}EditorGUILayout.EndVertical();
									}
									if (StyledButton("New Non Affected Joint"))
										characterInfo.headLook.nonAffectedJoints = AddElement<NonAffectedJoints>(characterInfo.headLook.nonAffectedJoints, new NonAffectedJoints());
									
								}EditorGUILayout.EndVertical();
							}

							characterInfo.headLook.effect = EditorGUILayout.FloatField("Global Weight:", characterInfo.headLook.effect);
							characterInfo.headLook.target = (BodyPart) EditorGUILayout.EnumPopup("Look At Opponent's:", characterInfo.headLook.target, enumStyle);
							characterInfo.headLook.overrideAnimation = EditorGUILayout.Toggle("Override Animation", characterInfo.headLook.overrideAnimation);
							characterInfo.headLook.disableOnHit = EditorGUILayout.Toggle("Disable On Hit", characterInfo.headLook.disableOnHit);

						}EditorGUI.EndDisabledGroup();

						EditorGUILayout.Space();
						EditorGUI.indentLevel -= 1;
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();


            // Custom Controls
            EditorGUILayout.BeginVertical(rootGroupStyle); {
                EditorGUILayout.BeginHorizontal(); {
                    customControlsOption = EditorGUILayout.Foldout(customControlsOption, "Custom Controls", foldStyle);
                    helpButton("character:customcontrols");
                }
                EditorGUILayout.EndHorizontal();

                if (customControlsOption) {
                    EditorGUILayout.BeginVertical(subGroupStyle);
                    {
                        EditorGUILayout.Space();
                        EditorGUI.indentLevel += 1;
                        EditorGUIUtility.labelWidth = 200;

                        if (UFE.isControlFreak2Installed)
                        {
                            characterInfo.customControls.overrideControlFreak = EditorGUILayout.Toggle("Override Mobile Controls", characterInfo.customControls.overrideControlFreak);
                            if (characterInfo.customControls.overrideControlFreak)
                            {
                                characterInfo.customControls.controlFreak2Prefab = EditorGUILayout.ObjectField(new GUIContent("CF2 Prefab:",
                                    "Prefab of Control Freak 2 Input Rig with \'UFE Bridge\' component."),
                                    characterInfo.customControls.controlFreak2Prefab, typeof(InputTouchControllerBridge), false) as InputTouchControllerBridge;
                            }
                        }
#if !UFE_LITE && !UFE_BASIC
                        if (characterInfo.gameplayType == GameplayType._3DFighter)
                            characterInfo.customControls.zAxisMovement = EditorGUILayout.Toggle("Allow Z Axis Movement", characterInfo.customControls.zAxisMovement);
#endif

                        characterInfo.customControls.disableJump = EditorGUILayout.Toggle("Disable Jump", characterInfo.customControls.disableJump);
                        if (!characterInfo.customControls.disableJump)
                            characterInfo.customControls.jumpButton = (ButtonPress)EditorGUILayout.EnumPopup("Override Jump Button:", characterInfo.customControls.jumpButton, enumStyle);

                        characterInfo.customControls.disableCrouch = EditorGUILayout.Toggle("Disable Crouch", characterInfo.customControls.disableCrouch);
                        if (!characterInfo.customControls.disableCrouch)
                            characterInfo.customControls.crouchButton = (ButtonPress)EditorGUILayout.EnumPopup("Override Crouch Button:", characterInfo.customControls.crouchButton, enumStyle);

                        EditorGUIUtility.labelWidth = 150;
                        EditorGUI.indentLevel -= 1;
                        EditorGUILayout.Space();
                    }
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndVertical();


            // Gauge UI
            EditorGUILayout.BeginVertical(rootGroupStyle);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    gaugeDisplayOptions = EditorGUILayout.Foldout(gaugeDisplayOptions, "Gauge Display", foldStyle);
                    helpButton("character:gaugedisplay");
                }
                EditorGUILayout.EndHorizontal();

                if (gaugeDisplayOptions)
                {
                    EditorGUILayout.BeginVertical(subGroupStyle);
                    {
                        EditorGUI.indentLevel += 1;
                        EditorGUILayout.Space();

                        for (int i = 0; i < characterInfo.hideGauges.Length; i ++)
                        {
                            characterInfo.hideGauges[i] = EditorGUILayout.Toggle("Hide Gauge "+ (i + 1), characterInfo.hideGauges[i]);
                        }

                        EditorGUILayout.Space();
                        EditorGUI.indentLevel -= 1;

                    }
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndVertical();


            // Move Sets
            EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					moveSetOption = EditorGUILayout.Foldout(moveSetOption, "Move Sets ("+ (characterInfo.moves.Length + characterInfo.stanceResourcePath.Length) +")", foldStyle);
					helpButton("character:movesets");
				}EditorGUILayout.EndHorizontal();

				if (moveSetOption){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;

                        SubGroupTitle("Animation Options");
                        EditorGUIUtility.labelWidth = 200;
                        characterInfo.animationType = (AnimationType)EditorGUILayout.EnumPopup("Animation Type:", characterInfo.animationType, enumStyle);
                        if (characterInfo.animationType == AnimationType.Mecanim3D)
                        {
                            characterInfo.avatar = (Avatar)EditorGUILayout.ObjectField("Avatar:", characterInfo.avatar, typeof(Avatar), false);
                        }

                        if (characterInfo.animationType == AnimationType.Mecanim3D || characterInfo.animationType == AnimationType.Mecanim2D)
                        {
                            characterInfo.useScaleFlip = EditorGUILayout.Toggle("Mirror Using Scale Flip", characterInfo.useScaleFlip);
                        }

                        characterInfo._blendingTime = EditorGUILayout.FloatField("Default Blending Duration:", (float)characterInfo._blendingTime);
                        characterInfo.animationFlow = (AnimationFlow) EditorGUILayout.EnumPopup("Animation Control:", characterInfo.animationFlow, enumStyle);
                        characterInfo.normalizeAnimationFrames = EditorGUILayout.Toggle("Normalize Animation Frames", characterInfo.normalizeAnimationFrames);
#if UFE_LITE || UFE_BASIC || UFE_STANDARD
                        characterInfo.useAnimationMaps = false;
#else
                        characterInfo.useAnimationMaps = EditorGUILayout.Toggle("Use Animation Maps", characterInfo.useAnimationMaps);
#endif
                        EditorGUIUtility.labelWidth = 150;

                        EditorGUILayout.Space();
						SubGroupTitle("Stances (Preloaded)");
                        for (int i = 0; i < characterInfo.moves.Length; i ++){
							EditorGUILayout.Space();
                            StanceBlock(characterInfo.moves[i]);
                        }
                        
                        if (StyledButton("New Stance"))
                            characterInfo.moves = AddElement<MoveSetData>(characterInfo.moves, new MoveSetData());


                        EditorGUILayout.Space();
                        SubGroupTitle("Stances (Resource)");
                        for (int i = 0; i < characterInfo.stanceResourcePath.Length; i++){
                            EditorGUILayout.Space();
                            EditorGUILayout.BeginVertical(arrayElementStyle);{
                                EditorGUILayout.Space();
                                EditorGUILayout.BeginHorizontal();{
                                    characterInfo.stanceResourcePath[i] = EditorGUILayout.TextField("Resource Path:", characterInfo.stanceResourcePath[i]);
                                    if (GUILayout.Button("", "PaneOptions")) {
                                        PaneOptions<string>(characterInfo.stanceResourcePath, characterInfo.stanceResourcePath[i], delegate (string[] newElement) { characterInfo.stanceResourcePath = newElement; });
                                    }
                                }EditorGUILayout.EndHorizontal();

                                EditorGUILayout.Space();
                                if (instantiatedMoveSet.ContainsKey(i)) {
                                    StanceBlock(instantiatedMoveSet[i], true);

                                    EditorGUILayout.BeginHorizontal();
                                    {
                                        if (StyledButton("Apply Changes"))
                                        {
                                            ScriptableObjectUtility.CreateAsset(instantiatedMoveSet[i].ConvertData(), Resources.Load<StanceInfo>(characterInfo.stanceResourcePath[i]));
                                        }

                                        if (StyledButton("Close File"))
                                        {
                                            instantiatedMoveSet.Remove(i);
                                        }
                                    }EditorGUILayout.EndHorizontal();
                                } else {
                                    if (StyledButton("Load Stance File")) {
                                        instantiatedMoveSet[i] = Resources.Load<StanceInfo>(characterInfo.stanceResourcePath[i]).ConvertData();
                                    }
                                }
                                EditorGUILayout.Space();
                            }
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.Space();
                        }

                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.BeginHorizontal();
                        if (StyledButton("New Stance Entry")) {
                            characterInfo.stanceResourcePath = AddElement<string>(characterInfo.stanceResourcePath, null);
                        }

                        if (StyledButton("Create Empty Stance File")) {
                            ScriptableObjectUtility.CreateAsset<StanceInfo>();
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();

                        EditorGUI.indentLevel -= 1;
                    }
                    EditorGUILayout.EndVertical();
				}
				
			}EditorGUILayout.EndVertical();
			
			// AI Instructions
			if (UFE.isAiAddonInstalled){
				EditorGUILayout.BeginVertical(rootGroupStyle);{
					EditorGUILayout.BeginHorizontal();{
						aiInstructionsOption = EditorGUILayout.Foldout(aiInstructionsOption, "AI Instructions ("+ characterInfo.aiInstructionsSet.Length +")", foldStyle);
						helpButton("character:aiinstructions");
					}EditorGUILayout.EndHorizontal();
					
					if (aiInstructionsOption){
						EditorGUILayout.BeginVertical(subGroupStyle);{
							EditorGUILayout.Space();
							EditorGUI.indentLevel += 1;
							
							for (int i = 0; i < characterInfo.aiInstructionsSet.Length; i ++){
								EditorGUILayout.Space();
								EditorGUILayout.BeginVertical(arrayElementStyle);{
									EditorGUILayout.Space();
									EditorGUILayout.BeginHorizontal();{
										characterInfo.aiInstructionsSet[i].behavior = (AIBehavior) EditorGUILayout.EnumPopup("Behavior:", characterInfo.aiInstructionsSet[i].behavior, enumStyle);
										if (GUILayout.Button("", "PaneOptions")){
											PaneOptions<AIInstructionsSet>(characterInfo.aiInstructionsSet, characterInfo.aiInstructionsSet[i], delegate (AIInstructionsSet[] newElement) { characterInfo.aiInstructionsSet = newElement; });
										}
									}EditorGUILayout.EndHorizontal();
									characterInfo.aiInstructionsSet[i].aiInfo = (ScriptableObject)EditorGUILayout.ObjectField("Instructions File:", characterInfo.aiInstructionsSet[i].aiInfo, typeof(ScriptableObject), false);
									EditorGUILayout.Space();
								}EditorGUILayout.EndVertical();
								EditorGUILayout.Space();
							}
							
							EditorGUI.indentLevel -= 1;
							
							if (StyledButton("New Instruction"))
								characterInfo.aiInstructionsSet = AddElement<AIInstructionsSet>(characterInfo.aiInstructionsSet, new AIInstructionsSet());
							
						}EditorGUILayout.EndVertical();
					}
				}EditorGUILayout.EndVertical();
			}

		}EditorGUILayout.EndScrollView();


		if (GUI.changed) {
			Undo.RecordObject(characterInfo, "Character Editor Modify");
			EditorUtility.SetDirty(characterInfo);
            if (UFE.autoSaveAssets) AssetDatabase.SaveAssets();
        }
	}
    
	private void SubGroupTitle(string _name){
		Texture2D originalBackground = GUI.skin.box.normal.background;
		GUI.skin.box.normal.background = Texture2D.grayTexture;

		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label(_name);
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

		GUI.skin.box.normal.background = originalBackground;
	}

	public bool StyledButton (string label) {
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

    public void ValidatePrefab(GameObject prefab, bool alt) {
        characterWarning = false;
        errorMsg = "";

        if (prefab != null) {
#if UNITY_2018_3_OR_NEWER
            if (PrefabUtility.GetPrefabAssetType(prefab) != PrefabAssetType.Regular) {
#else
            if (PrefabUtility.GetPrefabType(prefab) != PrefabType.Prefab) {
#endif
                characterWarning = true;
                errorMsg = "This character is not a prefab.";
            } else if (prefab.GetComponent<HitBoxesScript>() == null) {
                characterWarning = true;
                errorMsg = "This character doesn't have hitboxes!\n Please add the HitboxesScript and try again.";
            }
            /*} else if (alt && characterInfo.characterPrefab.GetComponent<HitBoxesScript>() != prefab.GetComponent<HitBoxesScript>()) {
                characterWarning = true;
                errorMsg = "This Hitbox setup is different from the main prefab.";
                ClosePreview();
            }*/
        }
    }

    public void StanceBlock(MoveSetData moveSet, bool resource = false) {
        EditorGUILayout.BeginVertical(resource? subArrayElementStyle : arrayElementStyle);
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            {
                moveSet.combatStance = (CombatStances)EditorGUILayout.EnumPopup("Stance Number:", moveSet.combatStance, enumStyle);
                if (!resource && GUILayout.Button("", "PaneOptions"))
                {
                    PaneOptions<MoveSetData>(characterInfo.moves, moveSet, delegate (MoveSetData[] newElement) { characterInfo.moves = newElement; });
                }
            }
            EditorGUILayout.EndHorizontal();


            moveSet.cinematicIntro = (MoveInfo)EditorGUILayout.ObjectField("Cinematic Intro:", moveSet.cinematicIntro, typeof(MoveInfo), false);
            moveSet.cinematicOutro = (MoveInfo)EditorGUILayout.ObjectField("Cinematic Outro:", moveSet.cinematicOutro, typeof(MoveInfo), false);

            EditorGUILayout.Space();

            moveSet.basicMovesToggle = EditorGUILayout.Foldout(moveSet.basicMovesToggle, "Basic Moves", foldStyle);
            if (moveSet.basicMovesToggle)
            {
                EditorGUILayout.BeginVertical(subGroupStyle);
                {
                    EditorGUILayout.Space();

                    moveSet.enabledBasicMovesToggle = EditorGUILayout.Foldout(moveSet.enabledBasicMovesToggle, "Enabled Moves", foldStyle);
                    if (moveSet.enabledBasicMovesToggle)
                    {
                        EditorGUILayout.BeginVertical(subArrayElementStyle);
                        {
                            EditorGUI.indentLevel += 1;
                            EditorGUILayout.Space();
                            moveSet.basicMoves.moveEnabled = EditorGUILayout.Toggle("Move", moveSet.basicMoves.moveEnabled);
                            moveSet.basicMoves.jumpEnabled = EditorGUILayout.Toggle("Jump", moveSet.basicMoves.jumpEnabled);
                            moveSet.basicMoves.crouchEnabled = EditorGUILayout.Toggle("Crouch", moveSet.basicMoves.crouchEnabled);
                            moveSet.basicMoves.blockEnabled = EditorGUILayout.Toggle("Block", moveSet.basicMoves.blockEnabled);
                            moveSet.basicMoves.parryEnabled = EditorGUILayout.Toggle("Parry", moveSet.basicMoves.parryEnabled);
                            EditorGUILayout.Space();
                            EditorGUI.indentLevel -= 1;
                        }
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.Space();

                    SubGroupTitle("Standard Animations");
                    BasicMoveBlock("Idle (*)", moveSet.basicMoves.idle, WrapMode.Loop, false, true, false, false, false);
                    EditorGUI.BeginDisabledGroup(!moveSet.basicMoves.moveEnabled);
                    {
                        BasicMoveBlock("Move Forward (*)", moveSet.basicMoves.moveForward, WrapMode.Loop, false, true, false, false, false);
#if !UFE_LITE && !UFE_BASIC
                        //if (characterInfo.gameplayType != GameplayType._3DArena)
                            BasicMoveBlock("Move Back (*)", moveSet.basicMoves.moveBack, WrapMode.Loop, false, true, false, false, false);
                        if (characterInfo.gameplayType == GameplayType._3DFighter && characterInfo.customControls.zAxisMovement)
                            BasicMoveBlock("Move Sideways (*)", moveSet.basicMoves.moveSideways, WrapMode.Loop, false, true, false, false, false);
#else
                        BasicMoveBlock("Move Back (*)", moveSet.basicMoves.moveBack, WrapMode.Loop, false, true, false, false, false);
#endif
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.BeginDisabledGroup(!moveSet.basicMoves.crouchEnabled);
                    {
                        BasicMoveBlock("Crouching (*)", moveSet.basicMoves.crouching, WrapMode.Loop, false, true, false, false, false);
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.Space();

                    if (moveSet.basicMoves.jumpEnabled)
                    {
                        SubGroupTitle("Jump Animations");
                        BasicMoveBlock("Take Off", moveSet.basicMoves.takeOff, WrapMode.ClampForever, true, true, false, true, false);
                        BasicMoveBlock("Jump Straight (*)", moveSet.basicMoves.jumpStraight, WrapMode.ClampForever, true, true, false, false, false);
                        BasicMoveBlock("Jump Back", moveSet.basicMoves.jumpBack, WrapMode.ClampForever, true, true, false, false, false);
                        BasicMoveBlock("Jump Forward", moveSet.basicMoves.jumpForward, WrapMode.ClampForever, true, true, false, false, false);
                        EditorGUILayout.Space();

                        SubGroupTitle("Fall Animations");
                        BasicMoveBlock("Fall Straight (*)", moveSet.basicMoves.fallStraight, WrapMode.ClampForever, true, true, false, false, false);
                        BasicMoveBlock("Fall Back", moveSet.basicMoves.fallBack, WrapMode.ClampForever, true, true, false, false, false);
                        BasicMoveBlock("Fall Forward", moveSet.basicMoves.fallForward, WrapMode.ClampForever, true, true, false, false, false);
                        BasicMoveBlock("Landing", moveSet.basicMoves.landing, WrapMode.Once, true, true, false, false, false);
                        EditorGUILayout.Space();
                    };

                    if (moveSet.basicMoves.blockEnabled)
                    {
                        SubGroupTitle("Block Animations");
                        BasicMoveBlock("Standing Pose", moveSet.basicMoves.blockingHighPose, WrapMode.ClampForever, false, true, false, false, false);
                        BasicMoveBlock("Standing High Hit", moveSet.basicMoves.blockingHighHit, WrapMode.Once, true, true, true, false, false);
                        BasicMoveBlock("Standing Low Hit", moveSet.basicMoves.blockingLowHit, WrapMode.Once, true, true, true, false, false);
                        BasicMoveBlock("Crouching Pose", moveSet.basicMoves.blockingCrouchingPose, WrapMode.ClampForever, false, true, false, false, false);
                        BasicMoveBlock("Crouching Hit", moveSet.basicMoves.blockingCrouchingHit, WrapMode.Once, true, true, true, false, false);
                        BasicMoveBlock("Air Pose", moveSet.basicMoves.blockingAirPose, WrapMode.ClampForever, false, true, false, false, false);
                        BasicMoveBlock("Air Hit", moveSet.basicMoves.blockingAirHit, WrapMode.Once, true, true, true, false, false);
                        EditorGUILayout.Space();
                    };

                    if (moveSet.basicMoves.parryEnabled)
                    {
                        SubGroupTitle("Parry Animations");
                        BasicMoveBlock("Standing High Hit", moveSet.basicMoves.parryHigh, WrapMode.Once, true, true, false, false, false);
                        BasicMoveBlock("Standing Low Hit", moveSet.basicMoves.parryLow, WrapMode.Once, true, true, false, false, false);
                        BasicMoveBlock("Crouching Hit", moveSet.basicMoves.parryCrouching, WrapMode.Once, true, true, false, false, false);
                        BasicMoveBlock("Air Hit", moveSet.basicMoves.parryAir, WrapMode.Once, true, true, false, false, false);
                        EditorGUILayout.Space();
                    };

                    SubGroupTitle("Hit Reactions");
                    BasicMoveBlock("Standing High Hit (*)", moveSet.basicMoves.getHitHigh, WrapMode.Once, true, true, true, true, false);
                    BasicMoveBlock("Standing Low Hit", moveSet.basicMoves.getHitLow, WrapMode.Once, true, true, true, true, false);
                    EditorGUI.BeginDisabledGroup(!moveSet.basicMoves.crouchEnabled);
                    {
                        BasicMoveBlock("Crouching Hit (*)", moveSet.basicMoves.getHitCrouching, WrapMode.Once, true, true, true, true, false);
                    }
                    EditorGUI.EndDisabledGroup();
                    BasicMoveBlock("Air Juggle [Launcher]", moveSet.basicMoves.getHitAir, WrapMode.ClampForever, true, true, false, true, false);
                    BasicMoveBlock("Knock Back [Knockdown]", moveSet.basicMoves.getHitKnockBack, WrapMode.ClampForever, true, true, false, true, true);
                    BasicMoveBlock("Standing High Hit [Knockdown]", moveSet.basicMoves.getHitHighKnockdown, WrapMode.ClampForever, false, true, false, true, true);
                    BasicMoveBlock("Standing Mid Hit [Knockdown]", moveSet.basicMoves.getHitMidKnockdown, WrapMode.ClampForever, false, true, false, true, true);
                    BasicMoveBlock("Sweep [Knockdown]", moveSet.basicMoves.getHitSweep, WrapMode.ClampForever, false, true, false, true, true);
                    BasicMoveBlock("Crumple [Knockdown]", moveSet.basicMoves.getHitCrumple, WrapMode.ClampForever, true, true, false, true, true);
                    EditorGUILayout.Space();

                    SubGroupTitle("Stage Reactions");
                    BasicMoveBlock("Ground Bounce [Launcher]", moveSet.basicMoves.groundBounce, WrapMode.ClampForever, true, true, false, true, false);
                    BasicMoveBlock("Standing Wall Bounce", moveSet.basicMoves.standingWallBounce, WrapMode.ClampForever, false, true, false, true, false);
                    BasicMoveBlock("Standing Wall Bounce [Knockdown]", moveSet.basicMoves.standingWallBounceKnockdown, WrapMode.ClampForever, false, true, false, true, false);
                    BasicMoveBlock("Air Wall Bounce [Knockdown]", moveSet.basicMoves.airWallBounce, WrapMode.ClampForever, true, true, false, true, false);
                    EditorGUILayout.Space();

                    SubGroupTitle("Fall Down Reactions");
                    BasicMoveBlock("Default (*)", moveSet.basicMoves.fallDown, WrapMode.ClampForever, true, true, false, true, false);
                    BasicMoveBlock("From Air Juggle [Knockdown]", moveSet.basicMoves.fallingFromAirHit, WrapMode.ClampForever, true, true, false, true, false);
                    BasicMoveBlock("From Ground Bounce [Knockdown]", moveSet.basicMoves.fallingFromGroundBounce, WrapMode.ClampForever, true, true, false, true, false);
                    BasicMoveBlock("Air Recovery", moveSet.basicMoves.airRecovery, WrapMode.ClampForever, true, true, false, true, false);
                    EditorGUILayout.Space();

                    SubGroupTitle("Stand Up Animations");
                    BasicMoveBlock("Default (*)", moveSet.basicMoves.standUp, WrapMode.ClampForever, true, true, false, true, false);
                    BasicMoveBlock("From Air Juggle", moveSet.basicMoves.standUpFromAirHit, WrapMode.ClampForever, true, true, false, true, false);
                    BasicMoveBlock("From Knock Back", moveSet.basicMoves.standUpFromKnockBack, WrapMode.ClampForever, true, true, false, true, false);
                    BasicMoveBlock("From Standing High Hit", moveSet.basicMoves.standUpFromStandingHighHit, WrapMode.ClampForever, true, true, false, true, false);
                    BasicMoveBlock("From Standing Mid Hit", moveSet.basicMoves.standUpFromStandingMidHit, WrapMode.ClampForever, true, true, false, true, false);
                    BasicMoveBlock("From Sweep", moveSet.basicMoves.standUpFromSweep, WrapMode.ClampForever, true, true, false, true, false);
                    BasicMoveBlock("From Crumple", moveSet.basicMoves.standUpFromCrumple, WrapMode.ClampForever, true, true, false, true, false);
                    BasicMoveBlock("From Standing Wall Bounce", moveSet.basicMoves.standUpFromStandingWallBounce, WrapMode.ClampForever, true, true, false, true, false);
                    BasicMoveBlock("From Air Wall Bounce", moveSet.basicMoves.standUpFromAirWallBounce, WrapMode.ClampForever, true, true, false, true, false);
                    BasicMoveBlock("From Ground Bounce", moveSet.basicMoves.standUpFromGroundBounce, WrapMode.ClampForever, true, true, false, true, false);

                    EditorGUILayout.Space();

                    GUILayout.Label("* Required", "MiniBoldLabel");

                }
                EditorGUILayout.EndVertical();
            }

            moveSet.attackMovesToggle = EditorGUILayout.Foldout(moveSet.attackMovesToggle, "Attack & Special Moves (" + moveSet.attackMoves.Length + ")", foldStyle);
            if (moveSet.attackMovesToggle)
            {
                EditorGUILayout.BeginVertical(subGroupStyle);
                {
                    EditorGUILayout.Space();
                    EditorGUI.indentLevel += 1;

                    for (int y = 0; y < moveSet.attackMoves.Length; y++)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.BeginVertical(subArrayElementStyle);
                        {
                            EditorGUILayout.Space();
                            EditorGUIUtility.labelWidth = 120;
                            EditorGUILayout.BeginHorizontal();
                            {
                                moveSet.attackMoves[y] = (MoveInfo)EditorGUILayout.ObjectField("Move File:", moveSet.attackMoves[y], typeof(MoveInfo), false);
                                if (GUILayout.Button("", "PaneOptions"))
                                {
                                    PaneOptions<MoveInfo>(moveSet.attackMoves, moveSet.attackMoves[y], delegate (MoveInfo[] newElement) { moveSet.attackMoves = newElement; });
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                            EditorGUIUtility.labelWidth = 150;

                            if (GUILayout.Button("Open in the Move Editor"))
                            {
                                MoveEditorWindow.sentMoveInfo = moveSet.attackMoves[y];
                                MoveEditorWindow.Init();
                            }
                        }
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.Space();
                    if (StyledButton("New Move"))
                        moveSet.attackMoves = AddElement<MoveInfo>(moveSet.attackMoves, null);

                    EditorGUILayout.Space();
                    EditorGUI.indentLevel -= 1;
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space();
        }

        if (!resource && StyledButton("Export Stance"))
            ScriptableObjectUtility.CreateAsset<StanceInfo>(moveSet.ConvertData());

        EditorGUILayout.EndVertical();
    }

	public void BasicMoveBlock(string label, BasicMoveInfo basicMove, WrapMode wrapMode, bool autoSpeed, bool hasSound, bool hasHitStrength, bool invincible, bool loops){
		basicMove.editorToggle = EditorGUILayout.Foldout(basicMove.editorToggle, label, foldStyle);

        //GUIStyle foldoutStyle;
        //foldoutStyle = new GUIStyle(EditorStyles.foldout);
        //foldoutStyle.normal.textColor = Color.cyan;
        //basicMove.editorToggle = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), basicMove.editorToggle, label, true, foldoutStyle);
		
        if (basicMove.editorToggle){
            EditorGUILayout.BeginVertical(subArrayElementStyle);
            {
                EditorGUILayout.Space();
                EditorGUI.indentLevel += 1;
                EditorGUIUtility.labelWidth = 180;

                if (label != "Idle (*)")
                {
                    basicMove.useMoveFile = EditorGUILayout.Toggle("Use Move File", basicMove.useMoveFile, toggleStyle);
                }
                else
                {
                    basicMove.useMoveFile = false;
                }

                if (basicMove.useMoveFile)
                {
                    basicMove.moveInfo = (MoveInfo)EditorGUILayout.ObjectField("Move:", basicMove.moveInfo, typeof(MoveInfo), false);
                }
                else
                {
                    if (hasHitStrength)
                    {
                        // UFE 2.0.3 update
                        if (basicMove.animMap.Length <= 8)
                        {
                            Array.Resize(ref basicMove.animMap, 9);
                            basicMove.animMap[6] = new SerializedAnimationMap();
                            basicMove.animMap[7] = new SerializedAnimationMap();
                            basicMove.animMap[8] = new SerializedAnimationMap();
                        }

                        string required = label.IndexOf("*") != -1 ? " (*)" : "";
                        AnimationFieldBlock(basicMove.animMap[0], "Weak Hit", required);
                        AnimationFieldBlock(basicMove.animMap[1], "Medium Hit");
                        AnimationFieldBlock(basicMove.animMap[2], "Heavy Hit");
                        AnimationFieldBlock(basicMove.animMap[3], "Custom 1 Hit");
                        AnimationFieldBlock(basicMove.animMap[4], "Custom 2 Hit");
                        AnimationFieldBlock(basicMove.animMap[5], "Custom 3 Hit");
                        AnimationFieldBlock(basicMove.animMap[6], "Custom 4 Hit");
                        AnimationFieldBlock(basicMove.animMap[7], "Custom 5 Hit");
                        AnimationFieldBlock(basicMove.animMap[8], "Custom 6 Hit");

                    }
                    else if (label == "Idle (*)")
                    {
                        AnimationFieldBlock(basicMove.animMap[0], "Default", " (*)");
                        AnimationFieldBlock(basicMove.animMap[1], "AFK 1");
                        AnimationFieldBlock(basicMove.animMap[2], "AFK 2");
                        AnimationFieldBlock(basicMove.animMap[3], "AFK 3");
                        AnimationFieldBlock(basicMove.animMap[4], "AFK 4");
                        AnimationFieldBlock(basicMove.animMap[5], "AFK 5");
                        basicMove._restingClipInterval = EditorGUILayout.FloatField("Resting Interval:", (float)basicMove._restingClipInterval);

                    }
                    else if (label == "Stand Up (*)")
                    {
                        AnimationFieldBlock(basicMove.animMap[0], "Default", " (*)");
                        AnimationFieldBlock(basicMove.animMap[1], "High Knockdown");
                        AnimationFieldBlock(basicMove.animMap[2], "Low Knockdown");
                        AnimationFieldBlock(basicMove.animMap[3], "Sweep");
                        AnimationFieldBlock(basicMove.animMap[4], "Crumple");
                        AnimationFieldBlock(basicMove.animMap[5], "Wall Bounce");

                    }
                    else if (label == "Crouching (*)")
                    {
                        AnimationFieldBlock(basicMove.animMap[0], "Crouched");
                        AnimationFieldBlock(basicMove.animMap[1], "Crouching Down");
                        AnimationFieldBlock(basicMove.animMap[2], "Standing Up");
                    }
                    else if (label.IndexOf("[Knockdown]") != -1)
                    {
                        AnimationFieldBlock(basicMove.animMap[0], "Fall Clip", " (*)");
                        AnimationFieldBlock(basicMove.animMap[1], "Down Clip");
                        basicMove.loopDownClip = EditorGUILayout.Toggle("Loop Down Clip", basicMove.loopDownClip, toggleStyle);
                    }
                    else if (loops)
                    {
                        AnimationFieldBlock(basicMove.animMap[1], "Transition");
                        AnimationFieldBlock(basicMove.animMap[0], "Animation", " (*)");
                    }
                    else
                    {
                        AnimationFieldBlock(basicMove.animMap[0], "Animation");
                    }

                    if (autoSpeed)
                    {
                        basicMove.autoSpeed = EditorGUILayout.Toggle("Auto Speed", basicMove.autoSpeed, toggleStyle);
                    }
                    else
                    {
                        basicMove.autoSpeed = false;
                    }

                    if (basicMove.autoSpeed)
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.TextField("Animation Speed:", basicMove._animationSpeed.ToString());
                        EditorGUI.EndDisabledGroup();
                    }
                    else
                    {
                        basicMove._animationSpeed = EditorGUILayout.FloatField("Animation Speed:", (float)basicMove._animationSpeed);
                    }

                    EditorGUILayout.BeginHorizontal();
                    {
                        basicMove.wrapMode = (WrapMode)EditorGUILayout.EnumPopup("Wrap Mode:", basicMove.wrapMode, enumStyle);
                        if (basicMove.wrapMode == WrapMode.Default) basicMove.wrapMode = wrapMode;
                        if (GUILayout.Button("Default", "minibutton", GUILayout.Width(60))) basicMove.wrapMode = wrapMode;

                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
                    EditorGUILayout.Space();

                    basicMove.overrideBlendingIn = EditorGUILayout.Toggle("Override Blending (In)", basicMove.overrideBlendingIn, toggleStyle);
                    if (basicMove.overrideBlendingIn)
                    {
                        basicMove._blendingIn = EditorGUILayout.FloatField("Blend In Duration:", (float)basicMove._blendingIn);
                    }

                    basicMove.overrideBlendingOut = EditorGUILayout.Toggle("Override Blending (Out)", basicMove.overrideBlendingOut, toggleStyle);
                    if (basicMove.overrideBlendingOut)
                    {
                        basicMove._blendingOut = EditorGUILayout.FloatField("Blend Out Duration:", (float)basicMove._blendingOut);
                    }

                    if (invincible) basicMove.invincible = EditorGUILayout.Toggle("Hide hitboxes", basicMove.invincible, toggleStyle);

                    basicMove.disableHeadLook = EditorGUILayout.Toggle("Disable Head Look", basicMove.disableHeadLook, toggleStyle);
                    basicMove.applyRootMotion = EditorGUILayout.Toggle("Apply Root Motion", basicMove.applyRootMotion, toggleStyle);
                    if (basicMove.applyRootMotion)
                    {
                        EditorGUI.indentLevel += 1;
                        basicMove.lockXMotion = EditorGUILayout.Toggle("Lock X Motion", basicMove.lockXMotion, toggleStyle);
                        basicMove.lockYMotion = EditorGUILayout.Toggle("Lock Y Motion", basicMove.lockYMotion, toggleStyle);
                        basicMove.lockZMotion = EditorGUILayout.Toggle("Lock Z Motion", basicMove.lockZMotion, toggleStyle);
                        EditorGUI.indentLevel -= 1;
                    }

                    EditorGUILayout.Space();
                    GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
                    EditorGUILayout.Space();

                    basicMove.particleEffect.editorToggle = EditorGUILayout.Foldout(basicMove.particleEffect.editorToggle, "Particle Effect", foldStyle);
                    if (basicMove.particleEffect.editorToggle)
                    {
                        EditorGUILayout.BeginVertical(subGroupStyle);
                        {
                            EditorGUILayout.Space();
                            basicMove.particleEffect.prefab = (GameObject)EditorGUILayout.ObjectField("Particle Prefab:", basicMove.particleEffect.prefab, typeof(UnityEngine.GameObject), true);
                            basicMove.particleEffect.duration = EditorGUILayout.FloatField("Duration (seconds):", basicMove.particleEffect.duration);
                            basicMove.particleEffect.stick = EditorGUILayout.Toggle("Sticky", basicMove.particleEffect.stick, toggleStyle);
                            basicMove.particleEffect.bodyPart = (BodyPart)EditorGUILayout.EnumPopup("Body Part:", basicMove.particleEffect.bodyPart, enumStyle);
                            basicMove.particleEffect.positionOffSet = EditorGUILayout.Vector3Field("Off Set (relative):", basicMove.particleEffect.positionOffSet);
                            basicMove.particleEffect.mirrorOn2PSide = EditorGUILayout.Toggle("Mirror on Right Side", basicMove.particleEffect.mirrorOn2PSide);

                            EditorGUILayout.Space();
                        }
                        EditorGUILayout.EndVertical();
                    }
                    if (hasSound)
                    {
                        basicMove.soundEffectsToggle = EditorGUILayout.Foldout(basicMove.soundEffectsToggle, "Possible Sound Effects (" + basicMove.soundEffects.Length + ")", EditorStyles.foldout);
                        if (basicMove.soundEffectsToggle)
                        {
                            EditorGUILayout.BeginVertical(subGroupStyle);
                            {
                                basicMove.continuousSound = EditorGUILayout.Toggle("Continuous Sound", basicMove.continuousSound, toggleStyle);
                                EditorGUILayout.Space();

                                EditorGUIUtility.labelWidth = 150;
                                for (int i = 0; i < basicMove.soundEffects.Length; i++)
                                {
                                    EditorGUILayout.Space();
                                    EditorGUILayout.BeginVertical(subArrayElementStyle);
                                    {
                                        EditorGUILayout.Space();
                                        EditorGUILayout.BeginHorizontal();
                                        {
                                            basicMove.soundEffects[i] = (AudioClip)EditorGUILayout.ObjectField("Audio Clip:", basicMove.soundEffects[i], typeof(UnityEngine.AudioClip), true);
                                            if (GUILayout.Button("", "PaneOptions"))
                                            {
                                                PaneOptions<AudioClip>(basicMove.soundEffects, basicMove.soundEffects[i], delegate (AudioClip[] newElement) { basicMove.soundEffects = newElement; });
                                            }
                                        }
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUILayout.Space();
                                    }
                                    EditorGUILayout.EndVertical();
                                }
                                if (StyledButton("New Sound Effect"))
                                    basicMove.soundEffects = AddElement<AudioClip>(basicMove.soundEffects, null);

                            }
                            EditorGUILayout.EndVertical();
                        }
                    }
                }

                EditorGUI.indentLevel -= 1;
                EditorGUILayout.Space();

            } EditorGUILayout.EndVertical();
		}
	}

	public Transform FindTransform(string searchString) {
		if (character == null) return null;
		Transform[] transformChildren = character.GetComponentsInChildren<Transform>();
		Transform found;
		foreach(Transform transformChild in transformChildren){
			found = transformChild.Find("mixamorig:"+ searchString);
			if (found == null) found = transformChild.Find(character.name + ":" + searchString);
			if (found == null) found = transformChild.Find(searchString);
			if (found != null) return found;
		}
		return null;
	}
    
    private void AnimationFieldBlock(SerializedAnimationMap animMap, string label, string required = "")
    {
        EditorGUILayout.BeginHorizontal();
        animMap.clip = (AnimationClip)EditorGUILayout.ObjectField(label + " Clip" + required + ":", animMap.clip, typeof(AnimationClip), false, GUILayout.ExpandWidth(true));
        if (characterInfo.gameplayType == GameplayType._2DFighter)
        {
            Rect lastRect = GUILayoutUtility.GetLastRect();
            lastRect.x += lastRect.width - 32;
            lastRect.width = 105;
            animMap.hitBoxDefinitionType = (HitBoxDefinitionType)EditorGUI.EnumPopup(lastRect, animMap.hitBoxDefinitionType);
            EditorGUIUtility.labelWidth = 100;
            EditorGUILayout.LabelField("", GUILayout.Width(70));
            EditorGUIUtility.labelWidth = 180;
        }
        EditorGUILayout.EndHorizontal();

        if (animMap.hitBoxDefinitionType == HitBoxDefinitionType.Custom)
        {
            animMap.customHitBoxDefinition = (CustomHitBoxesInfo)EditorGUILayout.ObjectField(label + " Map" + required + ":", animMap.customHitBoxDefinition, typeof(CustomHitBoxesInfo), false, GUILayout.ExpandWidth(true));
            if (animMap.customHitBoxDefinition != null && animMap.customHitBoxDefinition.clip != null && animMap.clip == null)
            {
                animMap.clip = animMap.customHitBoxDefinition.clip;
                animMap.length = animMap.clip.length;
            }
        }

    }

    public void PaneOptions<T> (T[] elements, T element, System.Action<T[]> callback) {
		if (elements == null || elements.Length == 0) return;
		GenericMenu toolsMenu = new GenericMenu();
		
		if ((elements[0] != null && elements[0].Equals(element)) || (elements[0] == null && element == null) || elements.Length == 1){
			toolsMenu.AddDisabledItem(new GUIContent("Move Up"));
			toolsMenu.AddDisabledItem(new GUIContent("Move To Top"));
		}else {
			toolsMenu.AddItem(new GUIContent("Move Up"), false, delegate() {callback(MoveElement<T>(elements, element, -1));});
			toolsMenu.AddItem(new GUIContent("Move To Top"), false, delegate() {callback(MoveElement<T>(elements, element, -elements.Length));});
		}
		if ((elements[elements.Length - 1] != null && elements[elements.Length - 1].Equals(element)) || elements.Length == 1){
			toolsMenu.AddDisabledItem(new GUIContent("Move Down"));
			toolsMenu.AddDisabledItem(new GUIContent("Move To Bottom"));
		}else{
			toolsMenu.AddItem(new GUIContent("Move Down"), false, delegate() {callback(MoveElement<T>(elements, element, 1));});
			toolsMenu.AddItem(new GUIContent("Move To Bottom"), false, delegate() {callback(MoveElement<T>(elements, element, elements.Length));});
		}
		
		toolsMenu.AddSeparator("");
		
		if (element != null && element is System.ICloneable){
			toolsMenu.AddItem(new GUIContent("Copy"), false, delegate() {callback(CopyElement<T>(elements, element));});
		}else{
			toolsMenu.AddDisabledItem(new GUIContent("Copy"));
		}
		
		if (element != null && CloneObject.objCopy != null && CloneObject.objCopy.GetType() == typeof(T)){
			toolsMenu.AddItem(new GUIContent("Paste"), false, delegate() {callback(PasteElement<T>(elements, element));});
		}else{
			toolsMenu.AddDisabledItem(new GUIContent("Paste"));
		}
		
		toolsMenu.AddSeparator("");
		
		if (!(element is System.ICloneable)){
			toolsMenu.AddDisabledItem(new GUIContent("Duplicate"));
		}else{
			toolsMenu.AddItem(new GUIContent("Duplicate"), false, delegate() {callback(DuplicateElement<T>(elements, element));});
		}
		toolsMenu.AddItem(new GUIContent("Remove"), false, delegate() {callback(RemoveElement<T>(elements, element));});
		
		toolsMenu.ShowAsContext();
		EditorGUIUtility.ExitGUI();
	}
	
	public T[] RemoveElement<T> (T[] elements, T element) {
		List<T> elementsList = new List<T>(elements);
		elementsList.Remove(element);
		return elementsList.ToArray();
	}
	
	public T[] AddElement<T> (T[] elements, T element) {
		List<T> elementsList = new List<T>(elements);
		elementsList.Add(element);
		return elementsList.ToArray();
	}
	
	public T[] CopyElement<T> (T[] elements, T element) {
		CloneObject.objCopy = (object)(element as ICloneable).Clone();
		return elements;
	}
	
	public T[] PasteElement<T> (T[] elements, T element) {
		if (CloneObject.objCopy == null) return elements;
		List<T> elementsList = new List<T>(elements);
		elementsList.Insert(elementsList.IndexOf(element) + 1, (T)CloneObject.objCopy);
		//CloneObject.objCopy = null;
		return elementsList.ToArray();
	}
	
	public T[] DuplicateElement<T> (T[] elements, T element) {
		List<T> elementsList = new List<T>(elements);
		elementsList.Insert(elementsList.IndexOf(element) + 1, (T)(element as ICloneable).Clone());
		return elementsList.ToArray();
	}
	
	public T[] MoveElement<T> (T[] elements, T element, int steps) {
		List<T> elementsList = new List<T>(elements);
		int newIndex = Mathf.Clamp(elementsList.IndexOf(element) + steps, 0, elements.Length - 1);
		elementsList.Remove(element);
		elementsList.Insert(newIndex, element);
		return elementsList.ToArray();
	}
}