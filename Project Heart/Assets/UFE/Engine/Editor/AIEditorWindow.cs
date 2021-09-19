using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UFE3D;

public class AIEditorWindow : EditorWindow {
	public static AIEditorWindow aIEditorWindow;
	public static UFE3D.AIInfo sentAIInfo;
	private UFE3D.AIInfo aiInfo;
	
	private Vector2 scrollPos;
	
	private bool predefinedRulesOptions;
	private bool customRulesOptions;
	private bool aiDebugInformation;
	private bool aiDefinitionsOptions;
	private bool aiDefinitionsDamageOptions;
	private bool aiDefinitionsDistanceOptions;
	private bool aiDefinitionsDesirabilityOptions;
	private bool aiDefinitionsHealthOptions;
	private bool aiDefinitionsSpeedOptions;
	private bool aiAdvancedOptions;
	private bool aiParametersSelfStatusOptions;
	private bool aiParametersOpponentStatusOptions;
	private bool aiParametersAttackInformationOptions;
	
	private List<string> debugInformation = new List<string>();
	private string titleStyle;
	private string addButtonStyle;
	private string rootGroupStyle;
	private string subGroupStyle;
	private string arrayElementStyle;
	private string subArrayElementStyle;
	private string toggleStyle;
	private string foldStyle;
	private string enumStyle;
	private GUIStyle labelStyle;
	
	[MenuItem("Window/U.F.E./A.I. Editor")]
	public static void Init(){
		aIEditorWindow = EditorWindow.GetWindow<AIEditorWindow>(false, "A.I.", true);
		aIEditorWindow.Show();
		aIEditorWindow.Populate();
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
	}
	
	void OnDestroy(){
	}
	
	void OnLostFocus(){
	}
	
	void helpButton(string page){
		if (GUILayout.Button("?", GUILayout.Width(18), GUILayout.Height(18))) 
			Application.OpenURL("http://www.ufe3d.com/doku.php/"+ page);
	}


    void Populate() {
        this.titleContent = new GUIContent("A.I.", (Texture)Resources.Load("Icons/A.I."));

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
		
		labelStyle = new GUIStyle();
		labelStyle.alignment = TextAnchor.MiddleCenter;
		labelStyle.fontStyle = FontStyle.Bold;
		labelStyle.normal.textColor = Color.white;
		
		
		if (sentAIInfo != null){
			EditorGUIUtility.PingObject( sentAIInfo );
			Selection.activeObject = sentAIInfo;
			sentAIInfo = null;
		}
		
		UnityEngine.Object[] selection = Selection.GetFiltered(typeof(AIInfo), SelectionMode.Assets);
		if (selection.Length > 0){
			if (selection[0] == null) return;
			aiInfo = (AIInfo) selection[0];
		}
	}
	
	public void OnGUI(){
		if (aiInfo == null){
			GUILayout.BeginHorizontal("GroupBox");
			GUILayout.Label("Select an A.I. Instructions file\nor create a new one.","CN EntryInfo");
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();
			if (GUILayout.Button("Create new A.I. Instructions File"))
				ScriptableObjectUtility.CreateAsset<AIInfo> ();
			return;
		}
		
		GUIStyle fontStyle = new GUIStyle();
        //fontStyle.font = (Font)EditorGUIUtility.Load("EditorFont.TTF");
        fontStyle.font = (Font)Resources.Load("EditorFont");
		fontStyle.fontSize = 30;
		fontStyle.alignment = TextAnchor.UpperCenter;
		fontStyle.normal.textColor = Color.white;
		fontStyle.hover.textColor = Color.white;
		EditorGUILayout.BeginVertical(titleStyle);{
			EditorGUILayout.BeginHorizontal();{
				EditorGUILayout.LabelField("", (aiInfo.instructionsName == ""? "New Instructions":aiInfo.instructionsName) , fontStyle, GUILayout.Height(32));
				helpButton("ai:start");
			}EditorGUILayout.EndHorizontal();
		}EditorGUILayout.EndVertical();
		
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);{
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				
				EditorGUIUtility.labelWidth = 170;
				aiInfo.instructionsName = EditorGUILayout.TextField("Instructions Name:", aiInfo.instructionsName);
				EditorGUILayout.Space();
				
				aiInfo.debugMode = EditorGUILayout.Toggle("Show Debug Info", aiInfo.debugMode);
				if (aiInfo.debugMode){
					EditorGUI.indentLevel += 1;
					aiInfo.debug_ReactionWeight = EditorGUILayout.Toggle("Reaction Weights", aiInfo.debug_ReactionWeight);
					EditorGUIUtility.labelWidth = 150;
					EditorGUI.indentLevel -= 1;
				}
				EditorGUIUtility.labelWidth = 150;
				
			}EditorGUILayout.EndVertical();
			
			// Predefined Rules
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					predefinedRulesOptions = EditorGUILayout.Foldout(predefinedRulesOptions, "Predefined Rules", foldStyle);
					helpButton("ai:predefinedrules");
				}EditorGUILayout.EndHorizontal();
				
				if (predefinedRulesOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;
						
						EditorGUIUtility.labelWidth = 200;
						aiInfo.rulesGenerator.autoMove = EditorGUILayout.Toggle("Auto Move", aiInfo.rulesGenerator.autoMove);
						EditorGUI.BeginDisabledGroup(!aiInfo.rulesGenerator.autoMove);{
							EditorGUI.indentLevel += 1;
							aiInfo.rulesGenerator.restOnLocation = EditorGUILayout.Toggle("Rest On Location", aiInfo.rulesGenerator.restOnLocation);
							aiInfo.rulesGenerator.preferableCombatDistance = (CharacterDistance)EditorGUILayout.EnumPopup("Preferable Distance:", aiInfo.rulesGenerator.preferableCombatDistance, enumStyle);
							if (aiInfo.rulesGenerator.preferableCombatDistance == CharacterDistance.Any) aiInfo.rulesGenerator.preferableCombatDistance = CharacterDistance.VeryClose;
							
							aiInfo.rulesGenerator.moveFrequency = EditorGUILayout.IntSlider("Move Frequency:", aiInfo.rulesGenerator.moveFrequency, 0, 6);
							EditorGUI.indentLevel -= 1;
						}EditorGUI.EndDisabledGroup();
						
						EditorGUILayout.Space();
						GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
						
						aiInfo.rulesGenerator.autoJump = EditorGUILayout.Toggle("Auto Jump", aiInfo.rulesGenerator.autoJump);
						EditorGUI.BeginDisabledGroup(!aiInfo.rulesGenerator.autoJump);{
							EditorGUI.indentLevel += 1;
							aiInfo.rulesGenerator.jumpBackFrequency = EditorGUILayout.IntSlider("Jump Back Frequency:", aiInfo.rulesGenerator.jumpBackFrequency, 0, 6);
							aiInfo.rulesGenerator.jumpStraightFrequency = EditorGUILayout.IntSlider("Jump Straight Frequency:", aiInfo.rulesGenerator.jumpStraightFrequency, 0, 6);
							aiInfo.rulesGenerator.jumpForwardFrequency = EditorGUILayout.IntSlider("Jump Forward Frequency:", aiInfo.rulesGenerator.jumpForwardFrequency, 0, 6);
							EditorGUI.indentLevel -= 1;
						}EditorGUI.EndDisabledGroup();
						
						EditorGUILayout.Space();
						GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
						
						aiInfo.rulesGenerator.autoAttack = EditorGUILayout.Toggle("Auto Attack", aiInfo.rulesGenerator.autoAttack);
						EditorGUI.BeginDisabledGroup(!aiInfo.rulesGenerator.autoAttack);{
							EditorGUI.indentLevel += 1;
							
							EditorGUI.BeginDisabledGroup(!aiInfo.advancedOptions.reactionParameters.enableDistanceFilter);{
								aiInfo.rulesGenerator.obeyPreferableDistances = EditorGUILayout.Toggle("Use Range Filters", aiInfo.rulesGenerator.obeyPreferableDistances);
							}EditorGUI.EndDisabledGroup();
							if (!aiInfo.advancedOptions.reactionParameters.enableDistanceFilter) aiInfo.rulesGenerator.obeyPreferableDistances = false;
							
							aiInfo.rulesGenerator.attackFrequency = EditorGUILayout.IntSlider("Attack Frequency:", aiInfo.rulesGenerator.attackFrequency, 0, 6);
							EditorGUI.indentLevel -= 1;
						}EditorGUI.EndDisabledGroup();
						
						EditorGUILayout.Space();
						GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
						
						aiInfo.rulesGenerator.autoBlock = EditorGUILayout.Toggle("Auto Block", aiInfo.rulesGenerator.autoBlock);
						EditorGUI.BeginDisabledGroup(!aiInfo.rulesGenerator.autoBlock);{
							EditorGUI.indentLevel += 1;
							
							EditorGUI.BeginDisabledGroup(!aiInfo.advancedOptions.reactionParameters.enableHitTypeFilter);{
								aiInfo.rulesGenerator.obeyHitType = EditorGUILayout.Toggle("Use Hit Type Filters", aiInfo.rulesGenerator.obeyHitType);
							}EditorGUI.EndDisabledGroup();
							if (!aiInfo.advancedOptions.reactionParameters.enableHitTypeFilter) aiInfo.rulesGenerator.obeyHitType = false;
							
							string adverb = aiInfo.rulesGenerator.obeyHitType ? "Accuracy": "Frequency";
							aiInfo.rulesGenerator.standBlockAccuracy = EditorGUILayout.IntSlider("Stand Block "+ adverb +":", aiInfo.rulesGenerator.standBlockAccuracy, 0, 6);
							aiInfo.rulesGenerator.crouchBlockAccuracy = EditorGUILayout.IntSlider("Crouch Block "+ adverb +":", aiInfo.rulesGenerator.crouchBlockAccuracy, 0, 6);
							aiInfo.rulesGenerator.jumpBlockAccuracy = EditorGUILayout.IntSlider("Jump Block Frequency:", aiInfo.rulesGenerator.jumpBlockAccuracy, 0, 6);
							EditorGUI.indentLevel -= 1;
						}EditorGUI.EndDisabledGroup();
						
						EditorGUILayout.Space();
						GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
						EditorGUILayout.Space();
						
						aiInfo.rulesGenerator.debugToggle = EditorGUILayout.Foldout(aiInfo.rulesGenerator.debugToggle, "Generated Fuzzy Rules", foldStyle);
						if (aiInfo.rulesGenerator.debugToggle){
							List<string> fuzzyRules = aiInfo.rulesGenerator.GenerateRules();
							EditorGUILayout.BeginVertical();{
								EditorGUI.indentLevel -= 1;
								if (fuzzyRules.Count > 0){
									foreach (string rule in fuzzyRules){
										EditorGUILayout.HelpBox(rule, MessageType.None);
										EditorGUILayout.Space();
									}
								}else{
									EditorGUILayout.HelpBox("NO VALID RULES", MessageType.None);
									EditorGUILayout.Space();
								}
								
								EditorGUI.indentLevel += 1;
							}EditorGUILayout.EndVertical();
						}
						
						EditorGUILayout.Space();
						
						EditorGUIUtility.labelWidth = 150;
						EditorGUI.indentLevel -= 1;
						
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();
			
			// Custom Rules
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					customRulesOptions = EditorGUILayout.Foldout(customRulesOptions, "Custom Rules ("+ aiInfo.aiRules.Length +")", foldStyle);
					helpButton("ai:customrules");
				}EditorGUILayout.EndHorizontal();
				
				if (customRulesOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;
						EditorGUILayout.Space();
						
						//EditorGUI.indentLevel += 1;
						for (int i = 0; i < aiInfo.aiRules.Length; i ++){
							EditorGUILayout.Space();
							EditorGUILayout.BeginVertical(arrayElementStyle);{
								EditorGUILayout.Space();
								EditorGUIUtility.labelWidth = 110;
								EditorGUILayout.BeginHorizontal();{
									aiInfo.aiRules[i].ruleName = EditorGUILayout.TextField("Rule Name:", 
									                                                       (aiInfo.aiRules[i].ruleName == "" || aiInfo.aiRules[i].ruleName == null? "Rule "+ (i + 1): aiInfo.aiRules[i].ruleName));
									if (GUILayout.Button("", "PaneOptions")){
										PaneOptions<AIRule>(aiInfo.aiRules, aiInfo.aiRules[i], delegate (AIRule[] newElement) { aiInfo.aiRules = newElement; });
									}
								}EditorGUILayout.EndHorizontal();
								EditorGUIUtility.labelWidth = 150;
								
								EditorGUILayout.Space();
								aiInfo.aiRules[i].eventsToggle = EditorGUILayout.Foldout(aiInfo.aiRules[i].eventsToggle, "Events ("+ aiInfo.aiRules[i].events.Length +")", foldStyle);
								if (aiInfo.aiRules[i].eventsToggle){
									EditorGUILayout.BeginVertical(subGroupStyle);{
										EditorGUI.indentLevel += 1;
										EditorGUILayout.Space();
										
										for (int j = 0; j < aiInfo.aiRules[i].events.Length; j ++){
											EditorGUILayout.Space();
											EditorGUILayout.BeginVertical(subArrayElementStyle);{
												EditorGUILayout.Space();
												EditorGUILayout.BeginHorizontal();{
													EditorGUIUtility.labelWidth = 220;
													aiInfo.aiRules[i].events[j].boolean = (AIBoolean)EditorGUILayout.EnumPopup("Valid when all conditions are", aiInfo.aiRules[i].events[j].boolean, enumStyle);
													EditorGUIUtility.labelWidth = 160;
													if (GUILayout.Button("", "PaneOptions")){
														PaneOptions<AIEvent>(aiInfo.aiRules[i].events, aiInfo.aiRules[i].events[j], delegate (AIEvent[] newElement) { aiInfo.aiRules[i].events = newElement; });
													}
												}EditorGUILayout.EndHorizontal();
												
												EditorGUILayout.Space();
												aiInfo.aiRules[i].events[j].enabled = EditorGUILayout.Toggle("Enabled", aiInfo.aiRules[i].events[j].enabled);
												
												EditorGUI.BeginDisabledGroup(!aiInfo.aiRules[i].events[j].enabled);{
													aiInfo.aiRules[i].events[j].conditionsToggle = EditorGUILayout.Foldout(aiInfo.aiRules[i].events[j].conditionsToggle, "Conditions ("+ aiInfo.aiRules[i].events[j].conditions.Length +")", foldStyle);
													if (aiInfo.aiRules[i].events[j].conditionsToggle){
														for (int m = 0; m < aiInfo.aiRules[i].events[j].conditions.Length; m ++){
															EditorGUILayout.Space();
															EditorGUILayout.BeginVertical(subGroupStyle);{
																EditorGUILayout.Space();
																EditorGUILayout.BeginHorizontal();{
																	EditorGUIUtility.labelWidth = 210;
																	aiInfo.aiRules[i].events[j].conditions[m].boolean = (AIBoolean)EditorGUILayout.EnumPopup("Valid when condition is", aiInfo.aiRules[i].events[j].conditions[m].boolean, enumStyle);
																	EditorGUIUtility.labelWidth = 160;
																	if (GUILayout.Button("", "PaneOptions")){
																		PaneOptions<AICondition>(aiInfo.aiRules[i].events[j].conditions, aiInfo.aiRules[i].events[j].conditions[m], delegate (AICondition[] newElement) { aiInfo.aiRules[i].events[j].conditions = newElement; });
																	}
																}EditorGUILayout.EndHorizontal();
																
																EditorGUILayout.Space();
																aiInfo.aiRules[i].events[j].conditions[m].enabled = EditorGUILayout.Toggle("Enabled", aiInfo.aiRules[i].events[j].conditions[m].enabled);
																EditorGUI.BeginDisabledGroup(!aiInfo.aiRules[i].events[j].conditions[m].enabled);{
																	aiInfo.aiRules[i].events[j].conditions[m].targetCharacter = (TargetCharacter)EditorGUILayout.EnumPopup("Target:", aiInfo.aiRules[i].events[j].conditions[m].targetCharacter, enumStyle);
																	aiInfo.aiRules[i].events[j].conditions[m].conditionType = (AIConditionType)EditorGUILayout.EnumPopup("Condition Type:", aiInfo.aiRules[i].events[j].conditions[m].conditionType, enumStyle);
																	
																	if (aiInfo.aiRules[i].events[j].conditions[m].conditionType == AIConditionType.Attacking){
																		EditorGUIUtility.labelWidth = 180;
																		aiInfo.aiRules[i].events[j].conditions[m].moveFrameData = (CurrentFrameData)EditorGUILayout.EnumPopup("Current Frame Data:", aiInfo.aiRules[i].events[j].conditions[m].moveFrameData, enumStyle);
																		if (aiInfo.advancedOptions.reactionParameters.enableDamageFilter) aiInfo.aiRules[i].events[j].conditions[m].moveDamage = (AIDamage)EditorGUILayout.EnumPopup("Damage: ", aiInfo.aiRules[i].events[j].conditions[m].moveDamage, enumStyle);
																		EditorGUIUtility.labelWidth = 150;
																		MoveClassificationGroup(aiInfo.aiRules[i].events[j].conditions[m].moveClassification);
																		
																	}else if (aiInfo.aiRules[i].events[j].conditions[m].conditionType == AIConditionType.HealthStatus){
																		aiInfo.aiRules[i].events[j].conditions[m].healthStatus = (HealthStatus)EditorGUILayout.EnumPopup("Health:", aiInfo.aiRules[i].events[j].conditions[m].healthStatus, enumStyle);
																		
																	}else if (aiInfo.aiRules[i].events[j].conditions[m].conditionType == AIConditionType.GaugeStatus){
																		aiInfo.aiRules[i].events[j].conditions[m].gaugeStatus = (GaugeUsage)EditorGUILayout.EnumPopup("Gauge:", aiInfo.aiRules[i].events[j].conditions[m].gaugeStatus, enumStyle);
																		
																	}else if (aiInfo.aiRules[i].events[j].conditions[m].conditionType == AIConditionType.HorizontalMovement){
																		aiInfo.aiRules[i].events[j].conditions[m].horizontalMovement = (AIHorizontalMovement)EditorGUILayout.EnumPopup("Direction:", aiInfo.aiRules[i].events[j].conditions[m].horizontalMovement, enumStyle);
																		if (aiInfo.aiRules[i].events[j].conditions[m].horizontalMovement != AIHorizontalMovement.Still){
																			aiInfo.aiRules[i].events[j].conditions[m].movementSpeed = (AIMovementSpeed)EditorGUILayout.EnumPopup("Movement Speed:", aiInfo.aiRules[i].events[j].conditions[m].movementSpeed, enumStyle);
																		}
																		
																	}else if (aiInfo.aiRules[i].events[j].conditions[m].conditionType == AIConditionType.VerticalMovement){
																		aiInfo.aiRules[i].events[j].conditions[m].verticalMovement = (AIVerticalMovement)EditorGUILayout.EnumPopup("Direction:", aiInfo.aiRules[i].events[j].conditions[m].verticalMovement, enumStyle);
																		if (aiInfo.aiRules[i].events[j].conditions[m].verticalMovement == AIVerticalMovement.Jumping){
																			aiInfo.aiRules[i].events[j].conditions[m].jumping = (JumpArc)EditorGUILayout.EnumPopup("Jump Arc:", aiInfo.aiRules[i].events[j].conditions[m].jumping, enumStyle);
																		}
																		
																	}else if (aiInfo.aiRules[i].events[j].conditions[m].conditionType == AIConditionType.Distance){
																		aiInfo.aiRules[i].events[j].conditions[m].playerDistance = (CharacterDistance)EditorGUILayout.EnumPopup("Proximity:", aiInfo.aiRules[i].events[j].conditions[m].playerDistance, enumStyle);
																		
																	}else if (aiInfo.aiRules[i].events[j].conditions[m].conditionType == AIConditionType.Blocking){
																		aiInfo.aiRules[i].events[j].conditions[m].blocking = (AIBlocking)EditorGUILayout.EnumPopup("Blocking State:", aiInfo.aiRules[i].events[j].conditions[m].blocking, enumStyle);
																	}
																}EditorGUI.EndDisabledGroup();
																
																EditorGUILayout.Space();
															}EditorGUILayout.EndVertical();
														}
														EditorGUILayout.Space();
														if (StyledButton("New Condition"))
															aiInfo.aiRules[i].events[j].conditions = AddElement<AICondition>(aiInfo.aiRules[i].events[j].conditions, null);
														
													}
												}EditorGUI.EndDisabledGroup();
												
											}EditorGUILayout.EndVertical();
										}
										EditorGUILayout.Space();
										if (StyledButton("New Event"))
											aiInfo.aiRules[i].events = AddElement<AIEvent>(aiInfo.aiRules[i].events, null);
										
										EditorGUI.indentLevel -= 1;
										EditorGUILayout.Space();
										
									}EditorGUILayout.EndVertical();
								}
								
								aiInfo.aiRules[i].reactionsToggle = EditorGUILayout.Foldout(aiInfo.aiRules[i].reactionsToggle, "Reactions (" + aiInfo.aiRules[i].reactions.Length + ")", foldStyle);
								if (aiInfo.aiRules[i].reactionsToggle){
									EditorGUILayout.BeginVertical(subGroupStyle);{
										EditorGUILayout.Space();
										EditorGUI.indentLevel += 1;
										EditorGUIUtility.labelWidth = 180;
										
										for (int j = 0; j < aiInfo.aiRules[i].reactions.Length; j ++){
											EditorGUILayout.Space();
											EditorGUILayout.BeginVertical(subArrayElementStyle);{
												EditorGUILayout.Space();
												EditorGUILayout.BeginHorizontal();{
													aiInfo.aiRules[i].reactions[j].reactionType = (AIReactionType)EditorGUILayout.EnumPopup("Reaction Type:", aiInfo.aiRules[i].reactions[j].reactionType, enumStyle);
													
													if (GUILayout.Button("", "PaneOptions")){
														PaneOptions<AIReaction>(aiInfo.aiRules[i].reactions, aiInfo.aiRules[i].reactions[j], delegate (AIReaction[] newElement) { aiInfo.aiRules[i].reactions = newElement; });
													}
												}EditorGUILayout.EndHorizontal();
												if (aiInfo.aiRules[i].reactions[j].reactionType == AIReactionType.PlayMove){
													if (aiInfo.advancedOptions.reactionParameters.enableDamageFilter) aiInfo.aiRules[i].reactions[j].moveDamage = (AIDamage)EditorGUILayout.EnumPopup("Damage: ", aiInfo.aiRules[i].reactions[j].moveDamage, enumStyle);
													MoveClassificationGroup(aiInfo.aiRules[i].reactions[j].moveClassification);
													/*
												}else if (aiInfo.aiRules[i].reactions[j].reactionType == AIReactionType.PressButton){
													aiInfo.aiRules[i].reactions[j].buttonPress = (ButtonPress)EditorGUILayout.EnumPopup("Button: ",aiInfo.aiRules[i].reactions[j].buttonPress, enumStyle);
												}else if (aiInfo.aiRules[i].reactions[j].reactionType == AIReactionType.PlaySpecificMove){
													aiInfo.aiRules[i].reactions[j].specificMove = (MoveInfo) EditorGUILayout.ObjectField("Move:", aiInfo.aiRules[i].reactions[j].specificMove, typeof(MoveInfo), false);
												*/
												}else if (aiInfo.aiRules[i].reactions[j].reactionType == AIReactionType.ChangeBehavior){
													aiInfo.aiRules[i].reactions[j].behavior = (AIBehavior)EditorGUILayout.EnumPopup("Behavior: ",aiInfo.aiRules[i].reactions[j].behavior, enumStyle);
												}
												
												EditorGUILayout.Space();
												aiInfo.aiRules[i].reactions[j].desirability = (AIDesirability)EditorGUILayout.EnumPopup("Desirability: ",aiInfo.aiRules[i].reactions[j].desirability, enumStyle);
												EditorGUILayout.Space();
											}EditorGUILayout.EndVertical();
											EditorGUILayout.Space();
										}
										EditorGUIUtility.labelWidth = 150;
										
										if (StyledButton("New Reaction"))
											aiInfo.aiRules[i].reactions = AddElement<AIReaction>(aiInfo.aiRules[i].reactions, null);
										
										
										EditorGUI.indentLevel -= 1;
									}EditorGUILayout.EndVertical();
								}
								
								EditorGUILayout.Space();
								List<string> debug = aiInfo.aiRules[i].ToDebugInformation();
								aiInfo.aiRules[i].debugToggle = EditorGUILayout.Foldout(aiInfo.aiRules[i].debugToggle, "Generated Fuzzy Rules ("+ debug.Count +")", foldStyle);
								
								if (aiInfo.aiRules[i].debugToggle){
									EditorGUILayout.BeginVertical();{
										EditorGUI.indentLevel += 1;
										if (debug.Count > 0){
											foreach (string rule in debug){
												EditorGUILayout.HelpBox(rule, MessageType.None);
												EditorGUILayout.Space();
											}
										}else{
											EditorGUILayout.HelpBox("NO VALID RULES", MessageType.None);
											EditorGUILayout.Space();
										}
										
										EditorGUI.indentLevel -= 1;
									}EditorGUILayout.EndVertical();
								}
								
								EditorGUILayout.Space();
							}EditorGUILayout.EndVertical();
							EditorGUILayout.Space();
						}
						EditorGUILayout.Space();
						if (StyledButton("New Rule"))
							aiInfo.aiRules = AddElement<AIRule>(aiInfo.aiRules, new AIRule());
						
						EditorGUILayout.Space();
						EditorGUI.indentLevel -= 1;
						
					}EditorGUILayout.EndVertical();
				}
				
			}EditorGUILayout.EndVertical();
			
			
			// AI DEFINITIONS:
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					aiDefinitionsOptions = EditorGUILayout.Foldout(aiDefinitionsOptions, "Definitions", foldStyle);
					helpButton("ai:definitions");
				}EditorGUILayout.EndHorizontal();
				
				if (aiDefinitionsOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUI.indentLevel += 1;
						
						EditorGUIUtility.labelWidth = 180;
						// DAMAGE:
						EditorGUILayout.Space();
						EditorGUILayout.BeginVertical(arrayElementStyle);{
							EditorGUILayout.Space();
							EditorGUILayout.BeginHorizontal();{
								aiDefinitionsDamageOptions = EditorGUILayout.Foldout(aiDefinitionsDamageOptions, "Damage", foldStyle);
							}EditorGUILayout.EndHorizontal();
							
							if (aiDefinitionsDamageOptions){
								EditorGUI.indentLevel += 1;
								EditorGUILayout.BeginVertical(subArrayElementStyle);{
									EditorGUILayout.Space();
									aiInfo.aiDefinitions.damage.veryWeak = Mathf.Clamp01(EditorGUILayout.Slider("Very Weak:", aiInfo.aiDefinitions.damage.veryWeak, 0f, 1f));
									aiInfo.aiDefinitions.damage.weak = Mathf.Clamp01(EditorGUILayout.Slider("Weak:", aiInfo.aiDefinitions.damage.weak, 0f, 1f));
									aiInfo.aiDefinitions.damage.medium = Mathf.Clamp01(EditorGUILayout.Slider("Medium:", aiInfo.aiDefinitions.damage.medium, 0f, 1f));
									aiInfo.aiDefinitions.damage.strong = Mathf.Clamp01(EditorGUILayout.Slider("Strong:", aiInfo.aiDefinitions.damage.strong, 0f, 1f));
									aiInfo.aiDefinitions.damage.veryStrong = Mathf.Clamp01(EditorGUILayout.Slider("Very Strong:", aiInfo.aiDefinitions.damage.veryStrong, 0f, 1f));
									EditorGUILayout.Space();
								}EditorGUILayout.EndVertical();
								EditorGUI.indentLevel -= 1;
								EditorGUILayout.Space();
							}
							EditorGUILayout.Space();
						}EditorGUILayout.EndVertical();
						EditorGUILayout.Space();
						
						// DISTANCE:
						EditorGUILayout.Space();
						EditorGUILayout.BeginVertical(arrayElementStyle);{
							EditorGUILayout.Space();
							EditorGUILayout.BeginHorizontal();{
								aiDefinitionsDistanceOptions = EditorGUILayout.Foldout(aiDefinitionsDistanceOptions, "Distance", foldStyle);
							}EditorGUILayout.EndHorizontal();
							
							if (aiDefinitionsDistanceOptions){
								EditorGUI.indentLevel += 1;
								EditorGUILayout.BeginVertical(subArrayElementStyle);{
									EditorGUILayout.Space();
									aiInfo.aiDefinitions.distance.veryClose = Mathf.Clamp01(EditorGUILayout.Slider("Very Close:", aiInfo.aiDefinitions.distance.veryClose, 0f, 1f));
									aiInfo.aiDefinitions.distance.close = Mathf.Clamp01(EditorGUILayout.Slider("Close:", aiInfo.aiDefinitions.distance.close, 0f, 1f));
									aiInfo.aiDefinitions.distance.mid = Mathf.Clamp01(EditorGUILayout.Slider("Mid:", aiInfo.aiDefinitions.distance.mid, 0f, 1f));
									aiInfo.aiDefinitions.distance.far = Mathf.Clamp01(EditorGUILayout.Slider("Far:", aiInfo.aiDefinitions.distance.far, 0f, 1f));
									aiInfo.aiDefinitions.distance.veryFar = Mathf.Clamp01(EditorGUILayout.Slider("Very Far:", aiInfo.aiDefinitions.distance.veryFar, 0f, 1f));
									EditorGUILayout.Space();
								}EditorGUILayout.EndVertical();
								EditorGUI.indentLevel -= 1;
								EditorGUILayout.Space();
							}
							EditorGUILayout.Space();
						}EditorGUILayout.EndVertical();
						EditorGUILayout.Space();
						
						
						// DESIRABILITY:
						EditorGUILayout.Space();
						EditorGUILayout.BeginVertical(arrayElementStyle);{
							EditorGUILayout.Space();
							EditorGUILayout.BeginHorizontal();{
								aiDefinitionsDesirabilityOptions = EditorGUILayout.Foldout(aiDefinitionsDesirabilityOptions, "Desirability", foldStyle);
							}EditorGUILayout.EndHorizontal();
							
							if (aiDefinitionsDesirabilityOptions){
								EditorGUI.indentLevel += 1;
								EditorGUILayout.BeginVertical(subArrayElementStyle);{
									EditorGUILayout.Space();
									aiInfo.aiDefinitions.desirability.theWorstOption = Mathf.Clamp01(EditorGUILayout.Slider("The Worst Option:", aiInfo.aiDefinitions.desirability.theWorstOption, 0f, 1f));
									aiInfo.aiDefinitions.desirability.veryUndesirable = Mathf.Clamp01(EditorGUILayout.Slider("Very Undesirable:", aiInfo.aiDefinitions.desirability.veryUndesirable, 0f, 1f));
									aiInfo.aiDefinitions.desirability.undesirable = Mathf.Clamp01(EditorGUILayout.Slider("Undesirable:", aiInfo.aiDefinitions.desirability.undesirable, 0f, 1f));
									aiInfo.aiDefinitions.desirability.notBad = Mathf.Clamp01(EditorGUILayout.Slider("Not Bad:", aiInfo.aiDefinitions.desirability.notBad, 0f, 1f));
									aiInfo.aiDefinitions.desirability.desirable = Mathf.Clamp01(EditorGUILayout.Slider("Desirable:", aiInfo.aiDefinitions.desirability.desirable, 0f, 1f));
									aiInfo.aiDefinitions.desirability.veryDesirable = Mathf.Clamp01(EditorGUILayout.Slider("Very Desirable:", aiInfo.aiDefinitions.desirability.veryDesirable, 0f, 1f));
									aiInfo.aiDefinitions.desirability.theBestOption = Mathf.Clamp01(EditorGUILayout.Slider("The Best Option:", aiInfo.aiDefinitions.desirability.theBestOption, 0f, 1f));
									EditorGUILayout.Space();
								}EditorGUILayout.EndVertical();
								EditorGUI.indentLevel -= 1;
								EditorGUILayout.Space();
							}
							EditorGUILayout.Space();
						}EditorGUILayout.EndVertical();
						EditorGUILayout.Space();
						
						
						// HEALTH:
						EditorGUILayout.Space();
						EditorGUILayout.BeginVertical(arrayElementStyle);{
							EditorGUILayout.Space();
							EditorGUILayout.BeginHorizontal();{
								aiDefinitionsHealthOptions = EditorGUILayout.Foldout(aiDefinitionsHealthOptions, "Health", foldStyle);
							}EditorGUILayout.EndHorizontal();
							
							if (aiDefinitionsHealthOptions){
								EditorGUI.indentLevel += 1;
								EditorGUILayout.BeginVertical(subArrayElementStyle);{
									EditorGUILayout.Space();
									aiInfo.aiDefinitions.health.healthy = Mathf.Clamp01(0.01f * EditorGUILayout.IntSlider("Healthy:", Mathf.RoundToInt(100f * aiInfo.aiDefinitions.health.healthy), 0, 100));
									aiInfo.aiDefinitions.health.scratched = Mathf.Clamp01(0.01f * EditorGUILayout.IntSlider("Scratched:", Mathf.RoundToInt(100f * aiInfo.aiDefinitions.health.scratched), 0, 100));
									aiInfo.aiDefinitions.health.lightlyWounded = Mathf.Clamp01(0.01f * EditorGUILayout.IntSlider("Lightly Wounded:", Mathf.RoundToInt(100f * aiInfo.aiDefinitions.health.lightlyWounded), 0, 100));
									aiInfo.aiDefinitions.health.moderatelyWounded = Mathf.Clamp01(0.01f * EditorGUILayout.IntSlider("Moderately Wounded:", Mathf.RoundToInt(100f * aiInfo.aiDefinitions.health.moderatelyWounded), 0, 100));
									aiInfo.aiDefinitions.health.seriouslyWounded = Mathf.Clamp01(0.01f * EditorGUILayout.IntSlider("Seriously Wounded:", Mathf.RoundToInt(100f * aiInfo.aiDefinitions.health.seriouslyWounded), 0, 100));
									aiInfo.aiDefinitions.health.criticallyWounded = Mathf.Clamp01(0.01f * EditorGUILayout.IntSlider("Critically Wounded:", Mathf.RoundToInt(100f * aiInfo.aiDefinitions.health.criticallyWounded), 0, 100));
									aiInfo.aiDefinitions.health.almostDead = Mathf.Clamp01(0.01f * EditorGUILayout.IntSlider("Almost Dead:", Mathf.RoundToInt(100f * aiInfo.aiDefinitions.health.almostDead), 0, 100));
									aiInfo.aiDefinitions.health.dead = Mathf.Clamp01(0.01f * EditorGUILayout.IntSlider("Dead:", Mathf.RoundToInt(100f * aiInfo.aiDefinitions.health.dead), 0, 100));
									EditorGUILayout.Space();
								}EditorGUILayout.EndVertical();
								EditorGUI.indentLevel -= 1;
								EditorGUILayout.Space();
							}
							EditorGUILayout.Space();
						}EditorGUILayout.EndVertical();
						EditorGUILayout.Space();
						
						// SPEED:
						EditorGUILayout.Space();
						EditorGUILayout.BeginVertical(arrayElementStyle);{
							EditorGUILayout.Space();
							EditorGUILayout.BeginHorizontal();{
								aiDefinitionsSpeedOptions = EditorGUILayout.Foldout(aiDefinitionsSpeedOptions, "Movement Speed", foldStyle);
							}EditorGUILayout.EndHorizontal();
							
							if (aiDefinitionsSpeedOptions){
								EditorGUI.indentLevel += 1;
								EditorGUILayout.BeginVertical(subArrayElementStyle);{
									EditorGUILayout.Space();
									aiInfo.aiDefinitions.speed.verySlow = EditorGUILayout.Slider("Very Slow:", aiInfo.aiDefinitions.speed.verySlow, 0f, 100f);
									aiInfo.aiDefinitions.speed.slow = EditorGUILayout.Slider("Slow:", aiInfo.aiDefinitions.speed.slow, 0f, 100f);
									aiInfo.aiDefinitions.speed.normal = EditorGUILayout.Slider("Normal:", aiInfo.aiDefinitions.speed.normal, 0f, 100f);
									aiInfo.aiDefinitions.speed.fast = EditorGUILayout.Slider("Fast:", aiInfo.aiDefinitions.speed.fast, 0f, 100f);
									aiInfo.aiDefinitions.speed.veryFast = EditorGUILayout.Slider("Very Fast:", aiInfo.aiDefinitions.speed.veryFast, 0f, 100f);
									EditorGUILayout.Space();
								}EditorGUILayout.EndVertical();
								EditorGUI.indentLevel -= 1;
								EditorGUILayout.Space();
							}
							EditorGUILayout.Space();
						}EditorGUILayout.EndVertical();
						EditorGUILayout.Space();
						
						EditorGUIUtility.labelWidth = 150;
						
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();
			
			
			// Advanced Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					aiAdvancedOptions = EditorGUILayout.Foldout(aiAdvancedOptions, "Advanced Options", foldStyle);
                    helpButton("ai:advancedoptions");
				}EditorGUILayout.EndHorizontal();
				
				if (aiAdvancedOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;
						SubGroupTitle("Intervals");
						EditorGUIUtility.labelWidth = 200;
						aiInfo.advancedOptions.timeBetweenDecisions = EditorGUILayout.Slider("Time Between Decisions:", aiInfo.advancedOptions.timeBetweenDecisions, 0f, 0.5f);
						aiInfo.advancedOptions.timeBetweenActions = EditorGUILayout.Slider("Time Between Actions:", aiInfo.advancedOptions.timeBetweenActions, 0f, 0.5f);
						aiInfo.advancedOptions.movementDuration = EditorGUILayout.Slider("Directional Padding:", aiInfo.advancedOptions.movementDuration, 0f, 2f);
						
						EditorGUILayout.Space();
						SubGroupTitle("Behavior");
						EditorGUIUtility.labelWidth = 160;
						aiInfo.advancedOptions.ruleCompliance = EditorGUILayout.Slider("Rule Compliance:", aiInfo.advancedOptions.ruleCompliance, 0f, 1f);
						aiInfo.advancedOptions.aggressiveness = EditorGUILayout.Slider("Aggressiveness:", aiInfo.advancedOptions.aggressiveness, 0.1f, 0.9f);
						aiInfo.advancedOptions.comboEfficiency = EditorGUILayout.Slider("Combo Efficiency:", aiInfo.advancedOptions.comboEfficiency, 0f, 1f);
						aiInfo.advancedOptions.defaultDesirability = (AIDesirability)EditorGUILayout.EnumPopup("Default Desarability: ", aiInfo.advancedOptions.defaultDesirability, enumStyle);
						EditorGUIUtility.labelWidth = 150;
						
						EditorGUILayout.Space();
						SubGroupTitle("Inputs Filters");
						aiParametersSelfStatusOptions = EditorGUILayout.Foldout(aiParametersSelfStatusOptions, "Self Status Options", foldStyle);
						if (aiParametersSelfStatusOptions){
							EditorGUI.indentLevel += 1;
							EditorGUILayout.BeginVertical(subArrayElementStyle);{
								EditorGUILayout.Space();
								EditorGUIUtility.labelWidth = 280;
								aiInfo.advancedOptions.reactionParameters.inputWhenDown = EditorGUILayout.Toggle("Attempt inputs when down:", aiInfo.advancedOptions.reactionParameters.inputWhenDown);
								aiInfo.advancedOptions.reactionParameters.inputWhenBlocking = EditorGUILayout.Toggle("Attempt inputs when blocking:", aiInfo.advancedOptions.reactionParameters.inputWhenBlocking);
								aiInfo.advancedOptions.reactionParameters.inputWhenStunned = EditorGUILayout.Toggle("Attempt inputs when stunned:", aiInfo.advancedOptions.reactionParameters.inputWhenStunned);
								EditorGUIUtility.labelWidth = 150;
								EditorGUILayout.Space();
							}EditorGUILayout.EndVertical();
							EditorGUI.indentLevel -= 1;
							EditorGUILayout.Space();
						}
						
						aiParametersOpponentStatusOptions = EditorGUILayout.Foldout(aiParametersOpponentStatusOptions, "Opponent Status Options", foldStyle);
						if (aiParametersOpponentStatusOptions){
							EditorGUI.indentLevel += 1;
							EditorGUILayout.BeginVertical(subArrayElementStyle);{
								EditorGUILayout.Space();
								EditorGUIUtility.labelWidth = 280;
								aiInfo.advancedOptions.reactionParameters.attackWhenEnemyIsDown = EditorGUILayout.Toggle("Attack when enemy is down:", aiInfo.advancedOptions.reactionParameters.attackWhenEnemyIsDown);
								aiInfo.advancedOptions.reactionParameters.attackWhenEnemyIsBlocking = EditorGUILayout.Toggle("Attack while enemy is blocking:", aiInfo.advancedOptions.reactionParameters.attackWhenEnemyIsBlocking);
								aiInfo.advancedOptions.reactionParameters.stopBlockingWhenEnemyIsStunned = EditorGUILayout.Toggle("Stop blocking while enemy is stunned:", aiInfo.advancedOptions.reactionParameters.stopBlockingWhenEnemyIsStunned);
								EditorGUIUtility.labelWidth = 150;
								EditorGUILayout.Space();
							}EditorGUILayout.EndVertical();
							EditorGUI.indentLevel -= 1;
							EditorGUILayout.Space();
						}
						
						aiParametersAttackInformationOptions = EditorGUILayout.Foldout(aiParametersAttackInformationOptions, "Attack Options", foldStyle);
						if (aiParametersAttackInformationOptions){
							EditorGUI.indentLevel += 1;
							EditorGUILayout.BeginVertical(subArrayElementStyle);{
								EditorGUILayout.Space();
								EditorGUIUtility.labelWidth = 280;
								aiInfo.advancedOptions.reactionParameters.enableAttackTypeFilter = EditorGUILayout.Toggle("Enable \"Attack Type\" Filters", aiInfo.advancedOptions.reactionParameters.enableAttackTypeFilter);
								aiInfo.advancedOptions.reactionParameters.enableGaugeFilter = EditorGUILayout.Toggle("Enable \"Gauge\" Filters", aiInfo.advancedOptions.reactionParameters.enableGaugeFilter);
								aiInfo.advancedOptions.reactionParameters.enableDamageFilter = EditorGUILayout.Toggle("Enable \"Damage\" Filters", aiInfo.advancedOptions.reactionParameters.enableDamageFilter);
								aiInfo.advancedOptions.reactionParameters.enableDistanceFilter = EditorGUILayout.Toggle("Enable \"Range\" Filters", aiInfo.advancedOptions.reactionParameters.enableDistanceFilter);
								aiInfo.advancedOptions.reactionParameters.enableHitConfirmTypeFilter = EditorGUILayout.Toggle("Enable \"Hit Confirm Type\" Filters", aiInfo.advancedOptions.reactionParameters.enableHitConfirmTypeFilter);
								aiInfo.advancedOptions.reactionParameters.enableAttackSpeedFilter = EditorGUILayout.Toggle("Enable \"Attack Speed\" Filters", aiInfo.advancedOptions.reactionParameters.enableAttackSpeedFilter);
								aiInfo.advancedOptions.reactionParameters.enableHitTypeFilter = EditorGUILayout.Toggle("Enable \"Hit Type\" Filters", aiInfo.advancedOptions.reactionParameters.enableHitTypeFilter);
								
								aiInfo.advancedOptions.playRandomMoves = EditorGUILayout.Toggle("Allow random moves to be played", aiInfo.advancedOptions.playRandomMoves);
								
								EditorGUILayout.Space();
								//EditorGUILayout.Space();

                                EditorGUIUtility.labelWidth = 260;
                                aiInfo.advancedOptions.buttonSequenceInterval = EditorGUILayout.IntField("Button Sequence Interval (frames):", Mathf.Max(1, aiInfo.advancedOptions.buttonSequenceInterval));
								aiInfo.advancedOptions.attackDesirabilityCalculation = (AIAttackDesirabilityCalculation)EditorGUILayout.EnumPopup("Attack Desirability System:", aiInfo.advancedOptions.attackDesirabilityCalculation, enumStyle);
								EditorGUIUtility.labelWidth = 150;
								
								EditorGUILayout.Space();
								
							}EditorGUILayout.EndVertical();
							EditorGUI.indentLevel -= 1;
							EditorGUILayout.Space();
						}
						
						EditorGUI.indentLevel -= 1;
						EditorGUILayout.Space();
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();
			
			
			// Generated Fuzzy Rules
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					aiDebugInformation = EditorGUILayout.Foldout(aiDebugInformation, "Generated Fuzzy Rules (" + debugInformation.Count +")", foldStyle);
					helpButton("global:advanced");
				}EditorGUILayout.EndHorizontal();
				
				if (aiDebugInformation){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						if (this.StyledButton("Refresh")){
							try{
								aiInfo.GenerateInferenceSystem();
								debugInformation.Clear();
								
								
								string generatedRulePrefix = "Generated Rule: ";
								string userRulePrefix = "User Rule: ";
								int suffix = 1;
								
								// Retrieve the debug information of the fuzzy rules generated automatically
								foreach (string fuzzyRule in aiInfo.rulesGenerator.ToDebugInformation()){
									if (!string.IsNullOrEmpty(fuzzyRule)){
										string rulename = generatedRulePrefix + suffix;
										
										debugInformation.Add(
											rulename + "\n" + 
											new string('-', Mathf.Max(50, rulename.Length + 20)) + "\n" + 
											fuzzyRule
											);
										
										++suffix;
									}
								}
								
								// Retrieve the debug information of the fuzzy rules generated by the user
								foreach (AIRule rule in aiInfo.aiRules){
									if (rule != null && !string.IsNullOrEmpty(rule.ruleName)){
										List<string> fuzzyRules = rule.ToDebugInformation();
										int count = fuzzyRules.Count;
										
										for (int i = 0; i < count; ++i){
											string fuzzyRule = fuzzyRules[i];
											if (!string.IsNullOrEmpty(fuzzyRule)){
												string rulename = userRulePrefix + rule.ruleName;
												if (count > 0){
													rulename = rulename + "_" + (i+1);
												}
												
												debugInformation.Add(
													rulename + "\n" + 
													new string('-', Mathf.Max(50, rulename.Length + 20)) + "\n" + 
													fuzzyRule
													);
											}
										}
									}
								}
							}catch(System.Exception e){
								debugInformation.Clear();
								debugInformation.Add(e.Message + "\n\n" + e.StackTrace);
							}
						}
						
						foreach (string fuzzyRule in debugInformation){
							EditorGUILayout.HelpBox(fuzzyRule, MessageType.None);
							EditorGUILayout.Space();
						}
						
						EditorGUILayout.Space();
						
					}EditorGUILayout.EndVertical();
				}
				
				
			}EditorGUILayout.EndVertical();
			
			// AI DEBUG INFORMATION:
			/*EditorGUIUtility.labelWidth = 180;
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				if (this.StyledButton("Refresh")){
					try{
						aiInfo.GenerateInferenceSystem();
						debugInformation.Clear();


						string generatedRulePrefix = "Generated Rule: ";
						string userRulePrefix = "User Rule: ";
						int suffix = 1;
						
						// Retrieve the debug information of the fuzzy rules generated automatically
						foreach (string fuzzyRule in aiInfo.rulesGenerator.ToDebugInformation()){
							if (!string.IsNullOrEmpty(fuzzyRule)){
								string rulename = generatedRulePrefix + suffix;

								debugInformation.Add(
									rulename + "\n" + 
									new string('-', Mathf.Max(50, rulename.Length + 20)) + "\n" + 
									fuzzyRule
								);

								++suffix;
							}
						}

						// Retrieve the debug information of the fuzzy rules generated by the user
						foreach (AIRule rule in aiInfo.aiRules){
							if (rule != null && !string.IsNullOrEmpty(rule.ruleName)){
								List<string> fuzzyRules = rule.ToDebugInformation();
								int count = fuzzyRules.Count;

								for (int i = 0; i < count; ++i){
									string fuzzyRule = fuzzyRules[i];
									if (!string.IsNullOrEmpty(fuzzyRule)){
										string rulename = userRulePrefix + rule.ruleName;
										if (count > 0){
											rulename = rulename + "_" + (i+1);
										}

										debugInformation.Add(
											rulename + "\n" + 
											new string('-', Mathf.Max(50, rulename.Length + 20)) + "\n" + 
											fuzzyRule
										);
									}
								}
							}
						}

						aiDebugInformation = true;
					}catch(System.Exception e){
						debugInformation.Clear();
						debugInformation.Add(e.Message + "\n\n" + e.StackTrace);
					}
				}
				EditorGUILayout.Space();

				EditorGUILayout.Space();
				if (debugInformation != null && debugInformation.Count > 0){
					EditorGUILayout.BeginHorizontal();{
						aiDebugInformation = EditorGUILayout.Foldout(aiDebugInformation, "Debug Information (" + debugInformation.Count + " Rules):", foldStyle);
					}EditorGUILayout.EndHorizontal();
					
					if (aiDebugInformation){
						EditorGUI.indentLevel += 1;
						foreach (string fuzzyRule in debugInformation){
							EditorGUILayout.HelpBox(fuzzyRule, MessageType.None);
							EditorGUILayout.Space();
						}
						EditorGUI.indentLevel -= 1;
						EditorGUILayout.Space();
					}
					EditorGUILayout.Space();
				}
			}EditorGUILayout.EndVertical();
			*/
		}EditorGUILayout.EndScrollView();
		
		
		if (GUI.changed) {
			Undo.RecordObject(aiInfo, "A.I. Instructions Editor Modify");
			EditorUtility.SetDirty(aiInfo);
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

	public void MoveClassificationGroup(MoveClassification moveClassification){
		EditorGUIUtility.labelWidth = 180;
		
		if (aiInfo.advancedOptions.reactionParameters.enableAttackTypeFilter){
			moveClassification.anyAttackType = EditorGUILayout.Toggle("Any Attack Type", moveClassification.anyAttackType, toggleStyle);
			EditorGUI.BeginDisabledGroup(moveClassification.anyAttackType);{
				moveClassification.attackType = (AttackType)EditorGUILayout.EnumPopup("Attack Type: ",moveClassification.attackType, enumStyle);
			}EditorGUI.EndDisabledGroup();
		}
		
		if (aiInfo.advancedOptions.reactionParameters.enableGaugeFilter){
			moveClassification.gaugeUsage = (GaugeUsage)EditorGUILayout.EnumPopup("Gauge Usage: ",moveClassification.gaugeUsage, enumStyle);
		}
		
		if (aiInfo.advancedOptions.reactionParameters.enableHitTypeFilter){
			moveClassification.anyHitType = EditorGUILayout.Toggle("Any Hit Type", moveClassification.anyHitType, toggleStyle);
			EditorGUI.BeginDisabledGroup(moveClassification.anyHitType);{
				moveClassification.hitType = (HitType)EditorGUILayout.EnumPopup("Hit Type: ",moveClassification.hitType, enumStyle);
			}EditorGUI.EndDisabledGroup();
		}
		
		if (aiInfo.advancedOptions.reactionParameters.enableHitConfirmTypeFilter){
			moveClassification.anyHitConfirmType = EditorGUILayout.Toggle("Any Hit Confirm Type", moveClassification.anyHitConfirmType, toggleStyle);
			EditorGUI.BeginDisabledGroup(moveClassification.anyHitConfirmType);{
				moveClassification.hitConfirmType = (HitConfirmType)EditorGUILayout.EnumPopup("Hit Confirm Type: ",moveClassification.hitConfirmType, enumStyle);
			}EditorGUI.EndDisabledGroup();
		}
		
		if (aiInfo.advancedOptions.reactionParameters.enableAttackSpeedFilter){
			moveClassification.startupSpeed = (FrameSpeed)EditorGUILayout.EnumPopup("Startup Speed: ",moveClassification.startupSpeed, enumStyle);
			moveClassification.recoverySpeed = (FrameSpeed)EditorGUILayout.EnumPopup("Recovery Speed: ",moveClassification.recoverySpeed, enumStyle);
		}
		
		if (aiInfo.advancedOptions.reactionParameters.enableDistanceFilter){
			moveClassification.preferableDistance = (CharacterDistance)EditorGUILayout.EnumPopup("Attack Range:", moveClassification.preferableDistance, enumStyle);
			if (moveClassification.preferableDistance == CharacterDistance.Other) moveClassification.preferableDistance = CharacterDistance.Any;
		}
		
		EditorGUIUtility.labelWidth = 150;
		EditorGUILayout.Space();
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
	
	public void PaneOptions<T> (T[] elements, T element, System.Action<T[]> callback){
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
		
		if (element != null){
			toolsMenu.AddItem(new GUIContent("Copy"), false, delegate() {callback(CopyElement<T>(elements, element));});
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
		CloneObject.objCopy = null;
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