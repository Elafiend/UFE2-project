using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Fuzzy;
using AI4Unity.Fuzzy;
using UFE3D;

public class RuleBasedAI : RandomAI{
	#region auxiliar classes definitions
	protected class MovementInfo{
		public string name;
		public ButtonPress[][] simulatedInput;
		public float weight;
		
		public MovementInfo(string name, ButtonPress[][] simulatedInput = null, float weight = 0f){
			this.name = name;
			this.weight = weight;
			
			if (simulatedInput != null){
				this.simulatedInput = simulatedInput;
			}else{
				this.simulatedInput = new ButtonPress[0][];
			}
			
			for (int i = 0; i < this.simulatedInput.Length; ++i){
				if (this.simulatedInput[i] == null){
					this.simulatedInput[i] = new ButtonPress[0];
				}
			}
		}
	}
	#endregion
	
	#region protected readonly class fields
	protected static readonly string gaugeUsageNone = GaugeUsage.None.ToString();
	protected static readonly string gaugeUsageQuarter = GaugeUsage.Quarter.ToString();
	protected static readonly string gaugeUsageHalf = GaugeUsage.Half.ToString();
	protected static readonly string gaugeUsageThreeQuarters = GaugeUsage.ThreeQuarters.ToString();
	protected static readonly string gaugeUsageAll = GaugeUsage.All.ToString();
	
	protected static readonly string damageVeryWeak = AIDamage.VeryWeak.ToString();
	protected static readonly string damageWeak = AIDamage.Weak.ToString();
	protected static readonly string damageMedium = AIDamage.Medium.ToString();
	protected static readonly string damageStrong = AIDamage.Strong.ToString();
	protected static readonly string damageVeryStrong = AIDamage.VeryStrong.ToString();
	#endregion

	#region public instance properties
	public override Dictionary<InputReferences, InputEvents> inputs{
		get{
			return inputBuffer[1];
		}
		protected set{
			inputBuffer[1] = value;
		}
	}

	public virtual Dictionary<InputReferences, InputEvents> previousInputs{
		get{
			return inputBuffer[0];
		}
		protected set{
			inputBuffer[0] = value;
		}
	}

	#endregion

	#region protected instance fields
	protected AIInfo ai = null;
	protected AIInfo initialAI = null;
	protected List<Dictionary<InputReferences, InputEvents>> inputBuffer;

	// Information about the character positions
	protected Vector3? previousPositionSelf;
	protected Vector3? previousPositionOpponent;
	
	// Information retrieved during the last decision process
	Dictionary<string, float> aiOutput = null;
	InferenceSystemThread inferenceEngine = null;
	
	// Cached information to improve performance
	protected ButtonPress[] noButtonsPressed = new ButtonPress[0];
	protected List<MovementInfo> movements = new List<MovementInfo>();
	protected LinguisticVariable gaugeVar;
	protected LinguisticVariable damageVar;
	
	//protected List<ButtonPress[][]> movesSimulatedInput = new List<ButtonPress[][]>();
	protected ButtonPress[][] idleSimulatedInput = null;
	protected ButtonPress[][] moveForwardSimulatedInput = null;
	protected ButtonPress[][] moveBackwardSimulatedInput = null;
	protected ButtonPress[][] crouchSimulatedInput = null;
	protected ButtonPress[][] crouchBlockSimulatedInput = null;
	protected ButtonPress[][] jumpBackwardSimulatedInput = null;
	protected ButtonPress[][] jumpBlockSimulatedInput = null;
	protected ButtonPress[][] jumpForwardSimulatedInput = null;
	protected ButtonPress[][] jumpStraightSimulatedInput = null;
	protected ButtonPress[][] standBlockSimulatedInput = null;
	#endregion



	#region public override methods
	public override void Initialize (IEnumerable<InputReferences> inputs){
		UFE.OnRoundBegins -= OnRoundBegins;
		UFE.OnRoundBegins += OnRoundBegins;

		//-------------------------------------------------
		// We need at least a buffer of 2 positions:
		// + buffer[0] -------> previous Input
		// + buffer[1] -------> current Input
		// + buffer[i > 1] ---> future Inputs 
		//-------------------------------------------------
		int bufferSize = 2;

		this.inputBuffer = new List<Dictionary<InputReferences, InputEvents>>();
		for (int i = 0; i < bufferSize; ++i){
			this.inputBuffer.Add(new Dictionary<InputReferences, InputEvents>());
		}

		if (inputs != null){
			foreach (InputReferences input in inputs){
				if (input != null){
					for (int i = 0; i < bufferSize; ++i){
						this.inputBuffer[i][input] = InputEvents.Default;
					}
				}
			}
		}

		base.Initialize (inputs);
	}

	public override void DoFixedUpdate(){
		// Store initial AI
		if (initialAI == null && ai != null) initialAI = ai;

		if (this.inferenceEngine != null && UFE.config.aiOptions.engine == AIEngine.FuzzyAI){
			ControlsScript self = UFE.GetControlsScript(this.player);
			if (this.inputReferences != null && this.inputBuffer != null && self != null){
				ControlsScript opponent = self.opControlsScript;
				if (opponent != null){
					//-------------------------------------------------------------------------------------------------
					// Check the information stored in the input buffer...
					//-------------------------------------------------------------------------------------------------
					if (this.inputBuffer.Count == 0){
						//---------------------------------------------------------------------------------------------
						// If the we don't have the input of the previous frame, use the default input...
						//---------------------------------------------------------------------------------------------
						Dictionary<InputReferences, InputEvents> frame = new Dictionary<InputReferences, InputEvents>();
						foreach (InputReferences input in this.inputReferences){
							frame[input] = InputEvents.Default;
						}
						this.inputBuffer.Add(frame);
					}else if (this.inputBuffer.Count >= 2){
						this.inputBuffer.RemoveAt(0);
					}
					
					//-------------------------------------------------------------------------------------------------
					// If we haven't decided the input for the current frame yet...
					//-------------------------------------------------------------------------------------------------
					if (this.inputBuffer.Count < 2){
						//---------------------------------------------------------------------------------------------
						// Ask the AI to choose the most appropriated movement...
						//---------------------------------------------------------------------------------------------
						MovementInfo chosenMovement = this.ChooseMovement(self, opponent, Time.fixedDeltaTime);
						
						//---------------------------------------------------------------------------------------------
						// And simulate the input required for executing the next movement
						//---------------------------------------------------------------------------------------------
						if (chosenMovement != null && chosenMovement.simulatedInput.Length > 0){
							// HACK: added debug information, we should place this code in a more appropriated place
							/*
							RenderTexture renderTexture = new RenderTexture(300,40,24);
							RenderTexture.active = renderTexture;

							GameObject tempObject = new GameObject("Temporary");
							tempObject.transform.position = new Vector3(-10000f, -10000f, -10000f);
							tempObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

							Camera myCamera = tempObject.AddComponent<Camera>();
							myCamera.orthographic = true;
							myCamera.orthographicSize = 4;
							myCamera.targetTexture = renderTexture;

							GameObject childObject = new GameObject("TextMesh");
							childObject.transform.parent = tempObject.transform;
							childObject.transform.localPosition = new Vector3(0f, 0f, 1f);
							childObject.transform.localScale = Vector3.one;
							childObject.transform.localRotation = Quaternion.identity;

							TextMesh tm = childObject.AddComponent<TextMesh>();
							tm.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
							tm.renderer.material = new Material(Shader.Find("GUI/Text Shader"));
							tm.renderer.material.mainTexture = tm.font.material.mainTexture;
							tm.renderer.material.color = Color.black;
							tm.fontSize = 36;
							tm.fontStyle = FontStyle.Bold;
							tm.alignment = TextAlignment.Center;
							tm.anchor = TextAnchor.MiddleCenter;
							tm.text = (self.currentMove == null) + " -> " + chosenMovement.name;
							myCamera.Render();

							RenderTexture.active = null;
							GameObject.Destroy(tempObject);
							 */
							
							//UFE.CastInput(InputType.Button, 0f, renderTexture, this.player);
							//---------------------------------------------------------------------------------//
							
							//float sign = Mathf.Sign(opponent.transform.position.x - self.transform.position.x);
							float sign = -self.mirror;
							foreach (ButtonPress[] buttonPresses in chosenMovement.simulatedInput){
								Dictionary<InputReferences,InputEvents> frame = new Dictionary<InputReferences,InputEvents>();
								foreach (InputReferences input in this.inputReferences){
									frame[input] = InputEvents.Default;
								}
								
								foreach (InputReferences input in this.inputReferences){
									if (input.inputType == InputType.HorizontalAxis){
										foreach (ButtonPress buttonPress in buttonPresses){
											if (buttonPress == ButtonPress.Back || buttonPress == ButtonPress.DownBack || buttonPress == ButtonPress.UpBack){
												frame[input] = new InputEvents(-1f * sign);
											}else if (buttonPress == ButtonPress.Forward || buttonPress == ButtonPress.DownForward || buttonPress == ButtonPress.UpForward){
												frame[input] = new InputEvents(1f * sign);
											}
										}
									}else if (input.inputType == InputType.VerticalAxis){
										foreach (ButtonPress buttonPress in buttonPresses){
											if (buttonPress == ButtonPress.Up || buttonPress == ButtonPress.UpBack || buttonPress == ButtonPress.UpForward){
												frame[input] = new InputEvents(1f);
											}else if (buttonPress == ButtonPress.Down || buttonPress == ButtonPress.DownBack || buttonPress == ButtonPress.DownForward){
												frame[input] = new InputEvents(-1f);
											}
										}
									}else{
										foreach (ButtonPress buttonPress in buttonPresses){
											if (input.engineRelatedButton == buttonPress){
												frame[input] = new InputEvents(true);
											}
										}
									}
								}
								this.inputBuffer.Add(frame);
							}
						}else{
							Dictionary<InputReferences, InputEvents> frame = new Dictionary<InputReferences, InputEvents>();
							foreach (InputReferences input in this.inputReferences){
								frame[input] = InputEvents.Default;
							}
							this.inputBuffer.Add(frame);
						}
					}
				}
			}
			
			
			/*
			string debug = "Player " + this.player;
			foreach (InputReferences input in this.inputReferences){
				if (input.inputType == InputType.HorizontalAxis){
					debug += "\nHorizontal: " + this.GetAxis(input);
				}else if (input.inputType == InputType.VerticalAxis){
					debug += "\nVertical: " + this.GetAxis(input);
				}else{
					debug += "\n" + input.engineRelatedButton + ": " + this.GetButton(input);
				}
			}
			Debug.Log(debug);
			*/
			
		}else{
			base.DoFixedUpdate();
		}
	}
	
	public override void DoUpdate(){
		// First, check if the inference engine is defined...
		if (this.inferenceEngine != null && UFE.config.aiOptions.engine == AIEngine.FuzzyAI){
			// In that case, find out if the ControlsScript of the character and his opponent are defined...
			ControlsScript self = UFE.GetControlsScript(this.player);
			
			if (self != null){
				ControlsScript opponent = self.opControlsScript;
				
				if (this.inputReferences != null && this.inputBuffer != null && opponent != null){
					// Check if we have already received the response from the last request to the inference system...
					if (this.inferenceEngine.Done){
						this.aiOutput = this.inferenceEngine.Output;
						
						// And check if we should make a new request to the inference system...
						float time = Time.realtimeSinceStartup;
						float dt = time - this.timeLastDecision;

						float newTimeBetweenDecisions = (UFE.GetAIDifficulty() != null && UFE.GetAIDifficulty().overrideTimeBetweenDecisions) ? 
							UFE.GetAIDifficulty().timeBetweenDecisions : this.ai.advancedOptions.timeBetweenDecisions;

						if (dt > 0f && dt > newTimeBetweenDecisions){
							this.timeLastDecision = time;
							this.RequestAIUpdate(self, opponent, dt);
						}
					}
				}
			}
		}else{
			base.DoUpdate();
		}
	}
	
	public override InputEvents ReadInput (InputReferences inputReference){
		if (UFE.config.aiOptions.engine == AIEngine.FuzzyAI){
			if(
				this.inputReferences != null && 
				this.inputBuffer != null && 
				this.inputBuffer.Count >= 2 && 
				this.inputs != null && 
				this.inputs.ContainsKey(inputReference)
			){
				return this.inputs[inputReference];
			}
			return InputEvents.Default;
		}else{
			return base.ReadInput(inputReference);
		}
	}
	#endregion
	
	#region public instance methods
	public virtual AIInfo GetAIInformation(){
		return this.ai;
	}
	
	public void SetAIInformation(ScriptableObject ai){
		// This method is required to access the method below using Reflection
		this.SetAIInformation(ai as AIInfo);
	}
	
	public virtual void SetAIInformation(AIInfo ai){
		this.ai = ai;
		
		if (ai != null){
			AI4Unity.Fuzzy.InferenceSystem inferenceSystem = ai.GenerateInferenceSystem();
			
			if (inferenceSystem != null){
				this.inferenceEngine = new InferenceSystemThread(
					inferenceSystem, 
					ai.GetDesirabilityScore(ai.advancedOptions.defaultDesirability)
					);
				
				this.gaugeVar = this.inferenceEngine.GetInputVariable(AICondition.Attacking_GaugeUsage_Self);
				this.damageVar = this.inferenceEngine.GetInputVariable(AICondition.Attacking_Damage_Self);
			}else{
				this.inferenceEngine = null;
				this.gaugeVar = null;
				this.damageVar = null;
			}
		}else{
			this.inferenceEngine = null;
			this.gaugeVar = null;
			this.damageVar = null;
		}


		UFE.OnGameBegin -= this.OnGameBegin;
		UFE.OnGameBegin += this.OnGameBegin;

		this.Initialize(this.inputReferences);
	}
	#endregion
	
	#region protected instance methods
	protected virtual void OnRoundBegins(int round){
		if (!UFE.config.aiOptions.persistentBehavior && round > 1 && initialAI != null) SetAIInformation(initialAI);
	}

	protected virtual MovementInfo ChooseMovement(ControlsScript self, ControlsScript opponent, float deltaTime){
		// Find out if this AI is controlling the first or the second player.
		if (self != null && opponent != null){
			if (this.aiOutput != null){
				// Check if we want to use the best available move or the Weighted Random Selection in this decision step...
				float newRuleCompliance = (UFE.GetAIDifficulty() != null && UFE.GetAIDifficulty().overrideRuleCompliance) ? 
					UFE.GetAIDifficulty().ruleCompliance : this.ai.advancedOptions.ruleCompliance;

				bool useBestAvailableMove = UnityEngine.Random.Range(0f, 1f) < newRuleCompliance;
				
				// Then calculate the desirability of each possible movement...
				this.movements.Clear();

				float attackWeight = (UFE.GetAIDifficulty() != null && UFE.GetAIDifficulty().overrideAggressiveness) ? 
					UFE.GetAIDifficulty().aggressiveness : this.ai.advancedOptions.aggressiveness;

				if (self.currentMove != null && opponent.currentSubState == SubStates.Stunned) {
					float newComboEfficiency = (UFE.GetAIDifficulty() != null && UFE.GetAIDifficulty().overrideAggressiveness) ? 
						UFE.GetAIDifficulty().comboEfficiency : this.ai.advancedOptions.comboEfficiency;

					attackWeight = newComboEfficiency/2;
				}
				
				float basicMoveWeight =  1f - attackWeight;
				float weight;

				float newTimeBetweenDecisions = (UFE.GetAIDifficulty() != null && UFE.GetAIDifficulty().overrideTimeBetweenDecisions) ? 
					UFE.GetAIDifficulty().timeBetweenDecisions : this.ai.advancedOptions.timeBetweenDecisions;

				int frames = Mathf.FloorToInt(newTimeBetweenDecisions / deltaTime) + 1;

				int paddingBeforeJump = Mathf.Max(UFE.config.plinkingDelay + 1, Mathf.FloorToInt((float)self.myInfo._executionTiming / deltaTime) + 1);
				
				if (this.aiOutput.TryGetValue(AIReaction.Crouch, out weight) && this.ValidateReaction(AIReactionType.Crouch, self, opponent)){
					this.movements.Add(new MovementInfo(AIReaction.Crouch, this.crouchSimulatedInput, weight * basicMoveWeight));
				}
				if (this.aiOutput.TryGetValue(AIReaction.Idle, out weight) && this.ValidateReaction(AIReactionType.Idle, self, opponent)){
					this.movements.Add(new MovementInfo(AIReaction.Idle, this.idleSimulatedInput, weight * basicMoveWeight));
				}
				if (this.aiOutput.TryGetValue(AIReaction.JumpBackward, out weight) && this.ValidateReaction(AIReactionType.JumpBack, self, opponent)){
					// We don't want to jump accidentally in the wrong direction, so we must have plinking into account
					ButtonPress[][] jumpSimulatedInput = this.jumpBackwardSimulatedInput;
					
					if (!Mathf.Approximately((float)this.inputBuffer[this.inputBuffer.Count - 1][this.horizontalAxis].axisRaw, 0f)){
						List<ButtonPress[]> temp = new List<ButtonPress[]>();
						
						for (int i = 0; i < paddingBeforeJump; ++i){
							temp.Add(this.noButtonsPressed);
						}
						
						temp.AddRange(jumpSimulatedInput);
						jumpSimulatedInput = temp.ToArray();
					}
					
					this.movements.Add(new MovementInfo(AIReaction.JumpBackward, jumpSimulatedInput.ToArray(), weight * basicMoveWeight));
				}
				if (this.aiOutput.TryGetValue(AIReaction.JumpForward, out weight) && this.ValidateReaction(AIReactionType.JumpForward, self, opponent)){
					// We don't want to jump accidentally in the wrong direction, so we must have plinking into account
					ButtonPress[][] jumpSimulatedInput = this.jumpForwardSimulatedInput;
					
					if (!Mathf.Approximately((float)this.inputBuffer[this.inputBuffer.Count - 1][this.horizontalAxis].axisRaw, 0f)){
						List<ButtonPress[]> temp = new List<ButtonPress[]>();
						
						for (int i = 0; i < paddingBeforeJump; ++i){
							temp.Add(this.noButtonsPressed);
						}
						
						temp.AddRange(jumpSimulatedInput);
						jumpSimulatedInput = temp.ToArray();
					}
					
					this.movements.Add(new MovementInfo(AIReaction.JumpForward, jumpSimulatedInput.ToArray(), weight * basicMoveWeight));
				}
				if (this.aiOutput.TryGetValue(AIReaction.JumpStraight, out weight) && this.ValidateReaction(AIReactionType.JumpStraight, self, opponent)){
					// We don't want to jump accidentally in the wrong direction, so we must have plinking into account
					ButtonPress[][] jumpSimulatedInput = this.jumpBackwardSimulatedInput;
					
					if (!Mathf.Approximately((float)this.inputBuffer[this.inputBuffer.Count - 1][this.horizontalAxis].axisRaw, 0f)){
						List<ButtonPress[]> temp = new List<ButtonPress[]>();
						
						for (int i = 0; i < paddingBeforeJump; ++i){
							temp.Add(this.noButtonsPressed);
						}
						
						temp.AddRange(jumpSimulatedInput);
						jumpSimulatedInput = temp.ToArray();
					}
					
					this.movements.Add(new MovementInfo(AIReaction.JumpStraight, jumpSimulatedInput.ToArray(), weight * basicMoveWeight));
				}
				if (this.aiOutput.TryGetValue(AIReaction.MoveBackward, out weight) && this.ValidateReaction(AIReactionType.MoveBack, self, opponent)){
					this.movements.Add(new MovementInfo(AIReaction.MoveBackward, this.moveBackwardSimulatedInput, weight * basicMoveWeight));
				}
				if (this.aiOutput.TryGetValue(AIReaction.MoveForward, out weight) && this.ValidateReaction(AIReactionType.MoveForward, self, opponent)){
					this.movements.Add(new MovementInfo(AIReaction.MoveForward, this.moveForwardSimulatedInput, weight * basicMoveWeight));
				}
				
				// Including blocks...
				if (this.aiOutput.TryGetValue(AIReaction.CrouchBlock, out weight) && this.ValidateReaction(AIReactionType.CrouchBlock, self, opponent)){
					this.movements.Add(new MovementInfo(AIReaction.CrouchBlock, this.crouchBlockSimulatedInput, weight * basicMoveWeight));
				}
				if (this.aiOutput.TryGetValue(AIReaction.JumpBlock, out weight) && this.ValidateReaction(AIReactionType.JumpBlock, self, opponent)){
					this.movements.Add(new MovementInfo(AIReaction.JumpBlock, this.jumpBlockSimulatedInput, weight * basicMoveWeight));
				}
				if (this.aiOutput.TryGetValue(AIReaction.StandBlock, out weight) && this.ValidateReaction(AIReactionType.StandBlock, self, opponent)){
					this.movements.Add(new MovementInfo(AIReaction.StandBlock, this.standBlockSimulatedInput, weight * basicMoveWeight));
				}

				// Changing AI behaviours...
				if (this.aiOutput.TryGetValue(AIReaction.ChangeBehaviour_Aggressive, out weight)){
					this.movements.Add(new MovementInfo(AIReaction.ChangeBehaviour_Aggressive, null, weight));
				}
				if (this.aiOutput.TryGetValue(AIReaction.ChangeBehaviour_Any, out weight)){
					this.movements.Add(new MovementInfo(AIReaction.ChangeBehaviour_Any, null, weight));
				}
				if (this.aiOutput.TryGetValue(AIReaction.ChangeBehaviour_Balanced, out weight)){
					this.movements.Add(new MovementInfo(AIReaction.ChangeBehaviour_Balanced, null, weight));
				}
				if (this.aiOutput.TryGetValue(AIReaction.ChangeBehaviour_Defensive, out weight)){
					this.movements.Add(new MovementInfo(AIReaction.ChangeBehaviour_Defensive, null, weight));
				}
				if (this.aiOutput.TryGetValue(AIReaction.ChangeBehaviour_VeryAggressive, out weight)){
					this.movements.Add(new MovementInfo(AIReaction.ChangeBehaviour_VeryAggressive, null, weight));
				}
				if (this.aiOutput.TryGetValue(AIReaction.ChangeBehaviour_VeryDefensive, out weight)){
					this.movements.Add(new MovementInfo(AIReaction.ChangeBehaviour_VeryDefensive, null, weight));
				}

				
				// And attack movements...
				if(
					this.aiOutput.ContainsKey(AIReaction.PlayMove_RandomAttack) &&
					this.ValidateReaction(AIReactionType.PlayMove, self, opponent)
					){
					this.AddValidMoves(
						self, 
						opponent,
						this.inferenceEngine.DefaultValue,
						useBestAvailableMove,
						deltaTime
						);
				}
				
				
				// Check if the character can execute any movement...
				MovementInfo currentMovement;
				if (this.movements.Count > 0){
					// Check if we want to select only the best available movement or if we want to choose 
					// a random movement giving higher chances to movements with higher desirability...
					//
					// If we want to select the best available movement, but there are several movements with 
					// the best weight, we choose one of those movements randomly.
					this.movements.Sort(delegate(MovementInfo move1, MovementInfo move2) {
						return move2.weight.CompareTo(move1.weight);
					});
					
					self.aiDebugger = "";
					if (self.debugger != null && UFE.config.debugOptions.debugMode && ai.debugMode && ai.debug_ReactionWeight){
						self.aiDebugger = "-----AI Reaction Weights-----\n";
						StringBuilder sb = new StringBuilder();
						sb.Append("Instruction Set: ").Append(ai.instructionsName).AppendLine();
						
						foreach(MovementInfo mInfo in this.movements){
							sb.Append(mInfo.name).Append(" = ").Append(mInfo.weight).AppendLine();
						}

						self.aiDebugger += sb.ToString();
					}

					if (useBestAvailableMove){
						//---------------------------------------------------------------------------------------------
						// If we want to use the best available move, find the move with the best weight and discard
						// the rest of the moves. If there are several moves with the best weight, choose one of them
						// randomly.
						//---------------------------------------------------------------------------------------------
						float maxWeight = 0f;
						
						for (int i = 0; i < this.movements.Count; ++i){
							currentMovement = this.movements[i];
							
							//-----------------------------------------------------------------------------------------
							// We know the moves are already sorted by its weight, 
							// but we want to check if the simulated input is null 
							// because those are special cases.
							//-----------------------------------------------------------------------------------------
							if (currentMovement != null && currentMovement.simulatedInput != null){
								maxWeight = currentMovement.weight;
								break;
							}
						}
						
						
						for (int i = this.movements.Count - 1; i > 0; --i){
							currentMovement = this.movements[i];
							
							if (
								currentMovement == null 
								|| 
								(
								currentMovement.weight < maxWeight &&
								!Mathf.Approximately(currentMovement.weight, maxWeight)
								)
								){
								this.movements.RemoveAt(i);
							}
						}
					}

					MovementInfo chosenMovement = null;
					while (chosenMovement == null || chosenMovement.simulatedInput == null){
						//---------------------------------------------------------------------------------------------
						// Calculate the total weight of all possible moves
						//---------------------------------------------------------------------------------------------

						//---------------------------------------------------------------------------------------------
						// It's look like the iOS AOT Compiler doesn't like this function, so I implement it myself.
						//---------------------------------------------------------------------------------------------
						//float totalWeight = this.movements.Sum((MovementInfo m) => m.weight);
						float totalWeight = 0f;
						foreach (MovementInfo movement in movements){
							if (movement != null){
								totalWeight += movement.weight;
							}
						}

						if (totalWeight > 0f){
							//-----------------------------------------------------------------------------------------
							// If the total weight is greater than zero, choose a random movement
							// (taking the weight of that movement into account)
							//-----------------------------------------------------------------------------------------
							float random = UnityEngine.Random.Range(0f, totalWeight);
							
							int length = this.movements.Count;
							float accumulatedWeight = 0f;
							float currentWeight;
							
							for (int i = 0; i < length - 1; ++i){
								currentMovement = this.movements[i];
								
								if (currentMovement != null){
									currentWeight = currentMovement.weight;
									
									if (currentWeight > 0f){
										accumulatedWeight += currentWeight;
										
										if (random < accumulatedWeight){
											chosenMovement = currentMovement;
											break;
										}
									}
								}
							}
							
							if (chosenMovement == null && length > 0){
								chosenMovement = this.movements[length - 1];
							}
							
							//-----------------------------------------------------------------------------------------
							// If the chosen movement doesn't have a simulated input, it's a special case
							//-----------------------------------------------------------------------------------------
							if (chosenMovement != null && chosenMovement.simulatedInput.Length == 0){
								if (AIReaction.ChangeBehaviour_Aggressive.Equals(chosenMovement.name)){
									foreach (AIInstructionsSet instructionsSet in self.myInfo.aiInstructionsSet){
										if (instructionsSet != null && instructionsSet.behavior == AIBehavior.Aggressive){
											this.SetAIInformation(instructionsSet.aiInfo);
											break;
										}
									}
								}else if (AIReaction.ChangeBehaviour_Any.Equals(chosenMovement.name)){
									List<AIInstructionsSet> instructions = new List<AIInstructionsSet>(self.myInfo.aiInstructionsSet);
									AIInstructionsSet instructionsSet = null;
									
									while (instructionsSet == null && instructions.Count > 0){
										int r = UnityEngine.Random.Range(0, instructions.Count);
										instructionsSet = instructions[r];
										instructions.RemoveAt(r);
									}
									
									if (instructionsSet != null){
										this.SetAIInformation(instructionsSet.aiInfo);
									}
									
								}else if (AIReaction.ChangeBehaviour_Balanced.Equals(chosenMovement.name)){
									foreach (AIInstructionsSet instructionsSet in self.myInfo.aiInstructionsSet){
										if (instructionsSet != null && instructionsSet.behavior == AIBehavior.Balanced){
											this.SetAIInformation(instructionsSet.aiInfo);
											break;
										}
									}
								}else if (AIReaction.ChangeBehaviour_Defensive.Equals(chosenMovement.name)){
									foreach (AIInstructionsSet instructionsSet in self.myInfo.aiInstructionsSet){
										if (instructionsSet != null && instructionsSet.behavior == AIBehavior.Defensive){
											this.SetAIInformation(instructionsSet.aiInfo);
											break;
										}
									}
								}else if (AIReaction.ChangeBehaviour_VeryAggressive.Equals(chosenMovement.name)){
									foreach (AIInstructionsSet instructionsSet in self.myInfo.aiInstructionsSet){
										if (instructionsSet != null && instructionsSet.behavior == AIBehavior.VeryAggressive){
											this.SetAIInformation(instructionsSet.aiInfo);
											break;
										}
									}
								}else if (AIReaction.ChangeBehaviour_VeryDefensive.Equals(chosenMovement.name)){
									foreach (AIInstructionsSet instructionsSet in self.myInfo.aiInstructionsSet){
										if (instructionsSet != null && instructionsSet.behavior == AIBehavior.VeryDefensive){
											this.SetAIInformation(instructionsSet.aiInfo);
											break;
										}
									}
								}
								
								//-------------------------------------------------------------------------------------
								// As soon as the special case is processed, 
								// remove it from the list and choose another movement
								//-------------------------------------------------------------------------------------
								this.movements.Remove (chosenMovement);
							}
						}else{
							break;
						}
					}

					if (chosenMovement == null){
						chosenMovement = new MovementInfo(AIReaction.Idle, this.Repeat(this.noButtonsPressed, frames), 1f);
					}

					return chosenMovement;
				}
			}
		}
		
		return null;
	}
	
	protected virtual void AddValidMoves(
		ControlsScript self, 
		ControlsScript opponent, 
		float defaultValue, 
		bool useBestAvailableMove,
		float deltaTime
		){
		int oldCount = this.movements.Count;
		float weight;
		
		//-------------------------------------------------------------------------------------------------------------
		// Cached values (required for improving performance)
		//-------------------------------------------------------------------------------------------------------------
		float attackWeight = this.ai.advancedOptions.aggressiveness;
		float maxGaugePoints = (float)(self.myInfo.maxGaugePoints);
		//float executionTiming = self.myInfo.executionTiming;
		//int plinkingDelay = UFE.config.plinkingDelay + 1;
		//int executionTimingDelay = Mathf.FloorToInt(executionTiming / deltaTime) + 2;
		//int timeBetweenActionsDelay = Mathf.FloorToInt(this.ai.advancedOptions.timeBetweenActions / deltaTime) + 2;
		//int framesAfterAttack = Mathf.Max(plinkingDelay, executionTimingDelay, timeBetweenActionsDelay);
		
		
		
		float attackTypeAntiAir = 0f;
		float attackTypeBackLauncher = 0f;
		float attackTypeDive = 0f;
		float attackTypeForwardLauncher = 0f;
		float attackTypeNeutral = 0f;
		float attackTypeNormalAttack = 0f;
		float attackTypeProjectile = 0f;
		
		float damageVeryWeak = 0f;
		float damageWeak = 0f;
		float damageMedium = 0f;
		float damageStrong = 0f;
		float damageVeryStrong = 0f;
		
		float gaugeUsageNone = 0f;
		float gaugeUsageQuarter = 0f;
		float gaugeUsageHalf = 0f;
		float gaugeUsageThreeQuarters = 0f;
		float gaugeUsageAll = 0f;
		
		float hitConfirmTypeHit = 0f;
		float hitConfirmTypeThrow = 0f;
		
		float hitTypeHighKnockdown = 0f;
		float hitTypeHighLow = 0f;
		float hitTypeKnockBack = 0f;
		float hitTypeLauncher = 0f;
		float hitTypeLow = 0f;
		float hitTypeMidKnockdown = 0f;
		float hitTypeOverhead = 0f;
		float hitTypeSweep = 0f;
		
		float preferableDistanceVeryClose = 0f;
		float preferableDistanceClose = 0f;
		float preferableDistanceMid = 0f;
		float preferableDistanceFar = 0f;
		float preferableDistanceVeryFar = 0f;
		
		float recoverySpeedVeryFast = 0f;
		float recoverySpeedFast = 0f;
		float recoverySpeedNormal = 0f;
		float recoverySpeedSlow = 0f;
		float recoverySpeedVerySlow = 0f;
		
		float startupSpeedVeryFast = 0f;
		float startupSpeedFast = 0f;
		float startupSpeedNormal = 0f;
		float startupSpeedSlow = 0f;
		float startupSpeedVerySlow = 0f;
		
		
		if (this.ai.advancedOptions.reactionParameters.enableAttackTypeFilter){
			attackTypeAntiAir = this.aiOutput[AIReaction.PlayMove_AttackType_AntiAir];
			attackTypeBackLauncher = this.aiOutput[AIReaction.PlayMove_AttackType_BackLauncher];
			attackTypeDive = this.aiOutput[AIReaction.PlayMove_AttackType_Dive];
			attackTypeForwardLauncher = this.aiOutput[AIReaction.PlayMove_AttackType_ForwardLauncher];
			attackTypeNeutral = this.aiOutput[AIReaction.PlayMove_AttackType_Neutral];
			attackTypeNormalAttack = this.aiOutput[AIReaction.PlayMove_AttackType_NormalAttack];
			attackTypeProjectile = this.aiOutput[AIReaction.PlayMove_AttackType_Projectile];
		}
		
		if (ai.advancedOptions.reactionParameters.enableDamageFilter){
			damageVeryWeak = this.aiOutput[AIReaction.PlayMove_Damage_VeryWeak];
			damageWeak = this.aiOutput[AIReaction.PlayMove_Damage_Weak];
			damageMedium = this.aiOutput[AIReaction.PlayMove_Damage_Medium];
			damageStrong = this.aiOutput[AIReaction.PlayMove_Damage_Strong];
			damageVeryStrong = this.aiOutput[AIReaction.PlayMove_Damage_VeryStrong];
		}
		
		if (ai.advancedOptions.reactionParameters.enableGaugeFilter){
			gaugeUsageNone = this.aiOutput[AIReaction.PlayMove_GaugeUsage_None];
			gaugeUsageQuarter = this.aiOutput[AIReaction.PlayMove_GaugeUsage_Quarter];
			gaugeUsageHalf = this.aiOutput[AIReaction.PlayMove_GaugeUsage_Half];
			gaugeUsageThreeQuarters = this.aiOutput[AIReaction.PlayMove_GaugeUsage_ThreeQuarters];
			gaugeUsageAll = this.aiOutput[AIReaction.PlayMove_GaugeUsage_All];
		}
		
		if (ai.advancedOptions.reactionParameters.enableDistanceFilter){
			preferableDistanceVeryClose = this.aiOutput[AIReaction.PlayMove_PreferableDistance_VeryClose];
			preferableDistanceClose = this.aiOutput[AIReaction.PlayMove_PreferableDistance_Close];
			preferableDistanceMid = this.aiOutput[AIReaction.PlayMove_PreferableDistance_Mid];
			preferableDistanceFar = this.aiOutput[AIReaction.PlayMove_PreferableDistance_Far];
			preferableDistanceVeryFar = this.aiOutput[AIReaction.PlayMove_PreferableDistance_VeryFar];
		}
		
		if (ai.advancedOptions.reactionParameters.enableHitConfirmTypeFilter){
			hitConfirmTypeHit = this.aiOutput[AIReaction.PlayMove_HitConfirmType_Hit];
			hitConfirmTypeThrow = this.aiOutput[AIReaction.PlayMove_HitConfirmType_Throw];
		}
		
		if (ai.advancedOptions.reactionParameters.enableAttackSpeedFilter){
			startupSpeedVeryFast = this.aiOutput[AIReaction.PlayMove_StartupSpeed_VeryFast];
			startupSpeedFast = this.aiOutput[AIReaction.PlayMove_StartupSpeed_Fast];
			startupSpeedNormal = this.aiOutput[AIReaction.PlayMove_StartupSpeed_Normal];
			startupSpeedSlow = this.aiOutput[AIReaction.PlayMove_StartupSpeed_Slow];
			startupSpeedVerySlow = this.aiOutput[AIReaction.PlayMove_StartupSpeed_VerySlow];
			
			recoverySpeedVeryFast = this.aiOutput[AIReaction.PlayMove_RecoverySpeed_VeryFast];
			recoverySpeedFast = this.aiOutput[AIReaction.PlayMove_RecoverySpeed_Fast];
			recoverySpeedNormal = this.aiOutput[AIReaction.PlayMove_RecoverySpeed_Normal];
			recoverySpeedSlow = this.aiOutput[AIReaction.PlayMove_RecoverySpeed_Slow];
			recoverySpeedVerySlow = this.aiOutput[AIReaction.PlayMove_RecoverySpeed_VerySlow];
		}
		
		
		if (ai.advancedOptions.reactionParameters.enableHitTypeFilter){
			hitTypeHighKnockdown = this.aiOutput[AIReaction.PlayMove_HitType_HighKnockdown];
			hitTypeHighLow = this.aiOutput[AIReaction.PlayMove_HitType_HighLow];
			hitTypeKnockBack = this.aiOutput[AIReaction.PlayMove_HitType_KnockBack];
			hitTypeLauncher = this.aiOutput[AIReaction.PlayMove_HitType_Launcher];
			hitTypeLow = this.aiOutput[AIReaction.PlayMove_HitType_Low];
			hitTypeMidKnockdown = this.aiOutput[AIReaction.PlayMove_HitType_MidKnockdown];
			hitTypeOverhead = this.aiOutput[AIReaction.PlayMove_HitType_Overhead];
			hitTypeSweep = this.aiOutput[AIReaction.PlayMove_HitType_Sweep];
		}
		
		
		
		//-------------------------------------------------------------------------------------------------------------
		// Retrieve a list with the movements that can be executed at this moment.
		//-------------------------------------------------------------------------------------------------------------
		// When we decide if a movement can be executed at this moment, we should take into account (for example) 
		// the current stance of the character, if the character is in the middle of a combo, the gauge usage of 
		// the attack or if the character is blocking, stunned or down.
		//-------------------------------------------------------------------------------------------------------------
		int index = 0;
		for (int i = 0; i < self.loadedMoves.Length; ++i){
			MoveSetData move = self.loadedMoves[i];
			
			for (int j = 0; j < move.attackMoves.Length; ++j){
				MoveInfo moveInfo = move.attackMoves[j];
				
				if (moveInfo != null && self.MoveSet.ValidateMoveExecution(moveInfo)){
					//-------------------------------------------------------------------------------------------------
					// Now that we know the attack can be executed at this moment, 
					// we simulate the input required to execute the attack...
					//-------------------------------------------------------------------------------------------------
					ButtonPress? chargeButton = null;
					int chargeFrames = 0;
					int count;
					
					if (moveInfo.defaultInputs.buttonSequence.Length > 0){
						if (moveInfo.defaultInputs.chargeMove){
							// If it's a "charge move", check if we have already started to charge the attack
							chargeButton = moveInfo.defaultInputs.buttonSequence[0];
							float charged = 0f;
							
							if (chargeButton == ButtonPress.Back){
                                if (this.previousInputs[this.horizontalAxis].axisRaw < 0f) {
                                    charged = (float)self.inputHeldDown[this.horizontalAxis.engineRelatedButton];
								}
							}else if (chargeButton == ButtonPress.Forward){
								if (this.previousInputs[this.horizontalAxis].axisRaw > 0f){
                                    charged = (float)self.inputHeldDown[this.horizontalAxis.engineRelatedButton];
								}
							}else if (chargeButton == ButtonPress.Up){
								if (this.previousInputs[this.verticalAxis].axisRaw > 0f){
                                    charged = (float)self.inputHeldDown[this.verticalAxis.engineRelatedButton];
								}
							}else if (chargeButton == ButtonPress.Down){
								if (this.previousInputs[this.verticalAxis].axisRaw < 0f){
                                    charged = (float)self.inputHeldDown[this.verticalAxis.engineRelatedButton];
								}
							}else{
								foreach (InputReferences input in this.inputReferences){
									if (input.inputType == InputType.Button && input.engineRelatedButton == chargeButton){
                                        charged = (float)self.inputHeldDown[input.engineRelatedButton];
										break;
									}
								}
							}
							
							// Calculate how many frames do we need to press the button to charge the attack
							chargeFrames = Mathf.FloorToInt(((float)moveInfo.defaultInputs._chargeTiming - charged) * UFE.config.fps);
							//chargeFrames = Mathf.FloorToInt((moveInfo.chargeTiming - charged) / deltaTime) + 1;
						}else{
							// TODO: if it wasn't a charge attack but we already had the first button 
							// of the sequence pressed, we could use that input to execute the movement faster.
							
							// If it isn't a charge move, we enter a few empty frames before the move
							// just to be completely sure the move is executed correctly
							//chargeFrames = Mathf.Max(plinkingDelay, executionTimingDelay);
						}
					}
					
					List<ButtonPress[]> sequence = new List<ButtonPress[]>();
					if (chargeButton != null){
						// If it's a "charge move", we need to repeat the first input for several frames
						for (int k = 0; k < chargeFrames; ++k){
							sequence.Add(new ButtonPress[]{chargeButton.Value});
						}
					}else{
						// Otherwise, add some "empty frames" before the attack
						for (int k = 0; k < chargeFrames; ++k){
							sequence.Add(this.noButtonsPressed);
						}
					}
					
					sequence.AddRange(moveInfo.simulatedInputs);
					
					
					//-------------------------------------------------------------------------------------------------
					// Finally, calcultate the weight associated to each valid movement...
					//-------------------------------------------------------------------------------------------------
					float attackTypeWeight = 0f;
					float gaugeUsageWeight = 0f;
					float preferableDistanceWeight = 0f;
					float hitConfirmTypeWeight = 0f;
					float startupSpeedWeight = 0f;
					float recoverySpeedWeight = 0f;
					float hitTypeWeight = 0f;
					float damageWeight = 0f;
					
					weight = 0f;
					count = 0;
					
					if (this.ai.advancedOptions.reactionParameters.enableAttackTypeFilter){
						switch(moveInfo.moveClassification.attackType){
						case AttackType.AntiAir:		attackTypeWeight += attackTypeAntiAir;			break;
						case AttackType.BackLauncher:	attackTypeWeight += attackTypeBackLauncher;		break;
						case AttackType.Dive:			attackTypeWeight += attackTypeDive;				break;
						case AttackType.ForwardLauncher:attackTypeWeight += attackTypeForwardLauncher;	break;
						case AttackType.Neutral:		attackTypeWeight += attackTypeNeutral;			break;
						case AttackType.NormalAttack:	attackTypeWeight += attackTypeNormalAttack;		break;
						case AttackType.Projectile:		attackTypeWeight += attackTypeProjectile;		break;
						default:
							attackTypeWeight += (
								attackTypeAntiAir + 
								attackTypeBackLauncher + 
								attackTypeDive +
								attackTypeForwardLauncher +
								attackTypeNeutral + 
								attackTypeNormalAttack +
								attackTypeProjectile
								) / 7f;
							break;
						}
						++count;
					}
					
					if (ai.advancedOptions.reactionParameters.enableGaugeFilter){
						float gaugeUsage = moveInfo.gauges.Length > 0 ? (float)(moveInfo.gauges[0]._gaugeUsage / maxGaugePoints) : 0;
						gaugeUsageWeight += gaugeVar.GetLabelMembership(RuleBasedAI.gaugeUsageNone, gaugeUsage) * gaugeUsageNone;
						gaugeUsageWeight += gaugeVar.GetLabelMembership(RuleBasedAI.gaugeUsageQuarter, gaugeUsage) * gaugeUsageQuarter;
						gaugeUsageWeight += gaugeVar.GetLabelMembership(RuleBasedAI.gaugeUsageHalf, gaugeUsage) * gaugeUsageHalf;
						gaugeUsageWeight += gaugeVar.GetLabelMembership(RuleBasedAI.gaugeUsageThreeQuarters, gaugeUsage) * gaugeUsageThreeQuarters;
						gaugeUsageWeight += gaugeVar.GetLabelMembership(RuleBasedAI.gaugeUsageAll, gaugeUsage) * gaugeUsageAll;
						++count;
					}
					
					if (ai.advancedOptions.reactionParameters.enableDistanceFilter){
						switch(moveInfo.moveClassification.preferableDistance){
						case CharacterDistance.VeryClose:	preferableDistanceWeight += preferableDistanceVeryClose;	break;
						case CharacterDistance.Close:		preferableDistanceWeight += preferableDistanceClose;		break;
						case CharacterDistance.Mid:			preferableDistanceWeight += preferableDistanceMid;			break;
						case CharacterDistance.Far:			preferableDistanceWeight += preferableDistanceFar;			break;
						case CharacterDistance.VeryFar:		preferableDistanceWeight += preferableDistanceVeryFar;		break;
						default:
							preferableDistanceWeight += (
								preferableDistanceVeryClose + 
								preferableDistanceClose + 
								preferableDistanceMid +
								preferableDistanceFar +
								preferableDistanceVeryFar
								) / 5f;
							break;
						}
						++count;
					}
					
					float damage = 0f;
					foreach(Hit hit in moveInfo.hits) damage += (float)hit._damageOnHit;
					
					
					if (moveInfo.hits.Length > 0){
						if (ai.advancedOptions.reactionParameters.enableDamageFilter){
							damage /= (float)(opponent.myInfo.lifePoints);
							damageWeight += damageVar.GetLabelMembership(RuleBasedAI.damageVeryWeak, damage) * damageVeryWeak;
							damageWeight += damageVar.GetLabelMembership(RuleBasedAI.damageWeak, damage) * damageWeak;
							damageWeight += damageVar.GetLabelMembership(RuleBasedAI.damageMedium, damage) * damageMedium;
							damageWeight += damageVar.GetLabelMembership(RuleBasedAI.damageStrong, damage) * damageStrong;
							damageWeight += damageVar.GetLabelMembership(RuleBasedAI.damageVeryStrong, damage) * damageVeryStrong;
							++count;
						}
						
						if (ai.advancedOptions.reactionParameters.enableHitConfirmTypeFilter){
							switch(moveInfo.moveClassification.hitConfirmType){
							case HitConfirmType.Hit:	hitConfirmTypeWeight += hitConfirmTypeHit;		break;
							case HitConfirmType.Throw:	hitConfirmTypeWeight += hitConfirmTypeThrow;	break;
							default:
								hitConfirmTypeWeight += (hitConfirmTypeHit + hitConfirmTypeThrow) / 2f;
								break;
							}
							++count;
						}
						
						if (ai.advancedOptions.reactionParameters.enableAttackSpeedFilter){
							switch(moveInfo.moveClassification.startupSpeed){
							case FrameSpeed.VeryFast:	startupSpeedWeight += startupSpeedVeryFast;	break;
							case FrameSpeed.Fast:		startupSpeedWeight += startupSpeedFast;		break;
							case FrameSpeed.Normal:		startupSpeedWeight += startupSpeedNormal;	break;
							case FrameSpeed.Slow:		startupSpeedWeight += startupSpeedSlow;		break;
							case FrameSpeed.VerySlow:	startupSpeedWeight += startupSpeedVerySlow;	break;
							default:
								startupSpeedWeight += (
									startupSpeedVeryFast + 
									startupSpeedFast + 
									startupSpeedNormal + 
									startupSpeedSlow + 
									startupSpeedVerySlow
									) / 5f;
								break;
							}
							++count;
							
							switch(moveInfo.moveClassification.recoverySpeed){
							case FrameSpeed.VeryFast:	recoverySpeedWeight += recoverySpeedVeryFast;	break;
							case FrameSpeed.Fast:		recoverySpeedWeight += recoverySpeedFast;		break;
							case FrameSpeed.Normal:		recoverySpeedWeight += recoverySpeedNormal;		break;
							case FrameSpeed.Slow:		recoverySpeedWeight += recoverySpeedSlow;		break;
							case FrameSpeed.VerySlow:	recoverySpeedWeight += recoverySpeedVerySlow;	break;
							default:
								recoverySpeedWeight += (
									recoverySpeedVeryFast + 
									recoverySpeedFast + 
									recoverySpeedNormal + 
									recoverySpeedSlow + 
									recoverySpeedVerySlow
									) / 5f;
								break;
							}
							++count;
						}
						
						if (ai.advancedOptions.reactionParameters.enableHitTypeFilter){
							switch(moveInfo.moveClassification.hitType){
							case HitType.HighKnockdown:	hitTypeWeight += hitTypeHighKnockdown;	break;
							case HitType.Mid:		hitTypeWeight += hitTypeHighLow;		break;
							case HitType.KnockBack:		hitTypeWeight += hitTypeKnockBack;		break;
							case HitType.Launcher:		hitTypeWeight += hitTypeLauncher;		break;
							case HitType.Low:			hitTypeWeight += hitTypeLow;			break;
							case HitType.MidKnockdown:	hitTypeWeight += hitTypeMidKnockdown;	break;
							case HitType.Overhead:		hitTypeWeight += hitTypeOverhead;		break;
							case HitType.Sweep:			hitTypeWeight += hitTypeSweep;			break;
							default:
								hitTypeWeight += (
									hitTypeHighKnockdown + 
									hitTypeHighLow + 
									hitTypeKnockBack + 
									hitTypeLauncher + 
									hitTypeLow +
									hitTypeMidKnockdown +
									hitTypeOverhead +
									hitTypeSweep
									) / 8f;
								break;
							}
							++count;
						}
					}
					
					if (count > 0){
						if (this.ai.advancedOptions.attackDesirabilityCalculation == AIAttackDesirabilityCalculation.Average){
							weight = (
								attackTypeWeight + 
								gaugeUsageWeight +
								preferableDistanceWeight +
								hitConfirmTypeWeight +
								startupSpeedWeight +
								recoverySpeedWeight +
								hitTypeWeight +
								damageWeight
								) / (float)(count);
						}else if (this.ai.advancedOptions.attackDesirabilityCalculation == AIAttackDesirabilityCalculation.ClampedSum){
							weight = Mathf.Clamp01(
								attackTypeWeight + 
								gaugeUsageWeight +
								preferableDistanceWeight +
								hitConfirmTypeWeight +
								startupSpeedWeight +
								recoverySpeedWeight +
								hitTypeWeight +
								damageWeight
								);
						}else if (this.ai.advancedOptions.attackDesirabilityCalculation == AIAttackDesirabilityCalculation.Max){
							weight = Mathf.Max(
								attackTypeWeight,
								gaugeUsageWeight,
								preferableDistanceWeight,
								hitConfirmTypeWeight,
								startupSpeedWeight,
								recoverySpeedWeight,
								hitTypeWeight,
								damageWeight
								);
						}else if (this.ai.advancedOptions.attackDesirabilityCalculation == AIAttackDesirabilityCalculation.Min){
							weight = Mathf.Min(
								!this.ai.advancedOptions.reactionParameters.enableAttackTypeFilter ? float.PositiveInfinity : attackTypeWeight,
								!this.ai.advancedOptions.reactionParameters.enableGaugeFilter ? float.PositiveInfinity : gaugeUsageWeight,
								!this.ai.advancedOptions.reactionParameters.enableDistanceFilter ? float.PositiveInfinity : preferableDistanceWeight,
								!this.ai.advancedOptions.reactionParameters.enableHitConfirmTypeFilter ? float.PositiveInfinity : hitConfirmTypeWeight,
								!this.ai.advancedOptions.reactionParameters.enableAttackSpeedFilter ? float.PositiveInfinity : startupSpeedWeight,
								!this.ai.advancedOptions.reactionParameters.enableHitTypeFilter ? float.PositiveInfinity : hitTypeWeight,
								!this.ai.advancedOptions.reactionParameters.enableDamageFilter ? float.PositiveInfinity : damageWeight
								);
						}
					}
					
					this.movements.Add(new MovementInfo(moveInfo.moveName, sequence.ToArray(), weight * attackWeight));
					++index;
				}
			}
		}
		
		// Check if we have found any valid movement...
		int newCount = this.movements.Count;
		if (newCount > oldCount){
			// In that case, check if we're using the "Weighted Random Selection" mode to compensate the weights of the moves...
			if (!useBestAvailableMove){
				float compensation = (float)(newCount - oldCount);
				
				for (int i = oldCount; i < newCount; ++i){
					this.movements[i].weight /= compensation;
				}
			}
			
			// And add the possibility of executing a random move...
			if (ai.advancedOptions.playRandomMoves){
				if (this.aiOutput.TryGetValue(AIReaction.PlayMove_RandomAttack, out weight)){
					MovementInfo randomMovement = this.movements[UnityEngine.Random.Range(oldCount, newCount)];
					this.movements.Add(new MovementInfo("Random: " + randomMovement.name, randomMovement.simulatedInput, weight * attackWeight));
				}
			}
		}
	}
	
	protected virtual void OnGameBegin(ControlsScript player1, ControlsScript player2, StageOptions stage){
		ControlsScript self = UFE.GetControlsScript(this.player);
		//this.movesSimulatedInput.Clear();

		if (self != null){
			if (this.ai == null) Debug.LogError("AI Instruction file for character \'"+ self.myInfo.characterName +"\' not found!");

			// First, find out which button is used for blocking...
			ButtonPress? blockButton = null;
			
			switch (UFE.config.blockOptions.blockType){
			case BlockType.HoldBack:		blockButton = ButtonPress.Back;		break;
			case BlockType.HoldButton1:		blockButton = ButtonPress.Button1;	break;
			case BlockType.HoldButton2:		blockButton = ButtonPress.Button2;	break;
			case BlockType.HoldButton3:		blockButton = ButtonPress.Button3;	break;
			case BlockType.HoldButton4:		blockButton = ButtonPress.Button4;	break;
			case BlockType.HoldButton5:		blockButton = ButtonPress.Button5;	break;
			case BlockType.HoldButton6:		blockButton = ButtonPress.Button6;	break;
			case BlockType.HoldButton7:		blockButton = ButtonPress.Button7;	break;
			case BlockType.HoldButton8:		blockButton = ButtonPress.Button8;	break;
			case BlockType.HoldButton9:		blockButton = ButtonPress.Button9;	break;
			case BlockType.HoldButton10:	blockButton = ButtonPress.Button10;	break;
			case BlockType.HoldButton11:	blockButton = ButtonPress.Button11;	break;
			case BlockType.HoldButton12:	blockButton = ButtonPress.Button12;	break;
			}
			
			//int count;
			float deltaTime = 1f / (float)(UFE.config.fps);
			//int frames = Mathf.FloorToInt(this.ai.advancedOptions.timeBetweenDecisions / deltaTime) + 1;
			int frames = Mathf.FloorToInt(this.ai.advancedOptions.movementDuration * (float)UFE.config.fps);

			float newTimeBetweenActions = (UFE.GetAIDifficulty() != null && UFE.GetAIDifficulty().overrideTimeBetweenActions) ? 
				UFE.GetAIDifficulty().timeBetweenActions : this.ai.advancedOptions.timeBetweenActions;
			

			int framesAfterAttack = Mathf.FloorToInt(newTimeBetweenActions * (float)UFE.config.fps);

			//int jumpDelay = self.myInfo.physics.jumpDelay;
			int jumpDelay = 1;

			int paddingAfterJump = Mathf.Max(UFE.config.plinkingDelay + 1, Mathf.FloorToInt((float)self.myInfo._executionTiming / deltaTime) + 1);
			
			List<ButtonPress[]> jumpBackward = new List<ButtonPress[]>();
			List<ButtonPress[]> jumpForward = new List<ButtonPress[]>();
			List<ButtonPress[]> jumpStraight = new List<ButtonPress[]>();
			
			// 1) Then we enter the horizontal input
			jumpBackward.Add(new ButtonPress[]{ButtonPress.Back});
			jumpForward.Add(new ButtonPress[]{ButtonPress.Forward});
			
			
			// 2) Finally we enter the vertical input...
			jumpBackward.AddRange(this.Repeat(new ButtonPress[]{ self.myInfo.customControls.jumpButton }, jumpDelay));
			jumpForward.AddRange(this.Repeat(new ButtonPress[]{ self.myInfo.customControls.jumpButton }, jumpDelay));
			jumpStraight.AddRange(this.Repeat(new ButtonPress[]{ self.myInfo.customControls.jumpButton }, jumpDelay));
			
			// 3) add some padding to avoid accidental plinking
			for (int i = 0; i < paddingAfterJump; ++i){
				jumpBackward.Add(this.noButtonsPressed);
				jumpForward.Add(this.noButtonsPressed);
				//jumpStraight.Add(this.noButtonsPressed);
			}
			
			// Finally, set the inputs for the basic moves
			this.crouchSimulatedInput = this.Repeat(new ButtonPress[]{ButtonPress.Down}, frames);
			this.idleSimulatedInput = this.Repeat(this.noButtonsPressed, frames);
			this.jumpBackwardSimulatedInput = jumpBackward.ToArray();
			this.jumpForwardSimulatedInput = jumpForward.ToArray();
			this.jumpStraightSimulatedInput = jumpStraight.ToArray();
			this.moveForwardSimulatedInput = this.Repeat(new ButtonPress[]{ButtonPress.Forward}, frames);
			this.moveBackwardSimulatedInput = this.Repeat(new ButtonPress[]{ButtonPress.Back}, frames);
			
			// Including blocks...
			if (blockButton != null){
				// If we need to press a button to block the attack, we press that button...
				this.crouchBlockSimulatedInput = this.Repeat(new ButtonPress[]{ButtonPress.Down, blockButton.Value}, frames);
				this.jumpBlockSimulatedInput = this.Repeat(new ButtonPress[]{blockButton.Value}, frames);
				this.standBlockSimulatedInput = this.Repeat(new ButtonPress[]{blockButton.Value}, frames);
				
			}else if (UFE.config.blockOptions.blockType == BlockType.None){
				// If the character can't block, at least we try to move backwards...
				this.crouchBlockSimulatedInput = this.Repeat(new ButtonPress[]{ButtonPress.Down, ButtonPress.Back}, frames);
				this.jumpBlockSimulatedInput = this.Repeat(new ButtonPress[]{ButtonPress.Back}, frames);
				this.standBlockSimulatedInput = this.Repeat(new ButtonPress[]{ButtonPress.Back}, frames);
			}else{
				// If the character will try to block automatically, we don't press any input
				this.crouchBlockSimulatedInput = this.Repeat(new ButtonPress[]{ButtonPress.Down}, frames);
				this.jumpBlockSimulatedInput = this.Repeat(new ButtonPress[]{}, frames);
				this.standBlockSimulatedInput = this.Repeat(this.noButtonsPressed, frames);
			}
			
			// ...and the attack moves
			int index = 0;
			for (int i = 0; i < self.loadedMoves.Length; ++i){
				MoveSetData move = self.loadedMoves[i];
				
				for (int j = 0; j < move.attackMoves.Length; ++j){
					MoveInfo moveInfo = move.attackMoves[j];
					
					if (moveInfo != null){
						List<ButtonPress[]> sequence = new List<ButtonPress[]>();
						
						// Then add the rest of the input sequence required to execute the attack
						for (int k = 0; k < moveInfo.defaultInputs.buttonSequence.Length; ++k){
							ButtonPress currentInput = moveInfo.defaultInputs.buttonSequence[k];
							//count = sequence.Count;
							
							sequence.Add(new ButtonPress[]{currentInput});
							
							for (int l = 0; l < ai.advancedOptions.buttonSequenceInterval; ++l){
								sequence.Add(this.noButtonsPressed);
							}
						}
						
						/*for (int k = 0; k < plinkingDelay; ++k){
							sequence.Add(this.noButtonsPressed);
						}*/
						
						// Now, complete the attack adding the last command
						if (moveInfo.selfConditions.possibleMoveStates.Length == 1
						    && moveInfo.selfConditions.possibleMoveStates[0].possibleState == PossibleStates.Crouch
						    ){
							
							List<ButtonPress> newExecution = moveInfo.defaultInputs.buttonExecution.ToList();
							newExecution.Add(ButtonPress.Down);
							
							sequence.AddRange(this.crouchSimulatedInput);
							sequence.Add(newExecution.ToArray());
							//sequence.AddRange(this.crouchSimulatedInput);
						}else{
							sequence.Add(moveInfo.defaultInputs.buttonExecution);
						}
						
						/*foreach(PossibleMoveStates possibleMoveState in moveInfo.selfConditions.possibleMoveStates){
							if (possibleMoveState.possibleState == PossibleStates.Crouch){
								List<ButtonPress> newExecution = moveInfo.buttonExecution.ToList();
								newExecution.Add(ButtonPress.Down);
								sequence.Add(new ButtonPress[]{ButtonPress.Down});
								sequence.Add(newExecution.ToArray());
								sequence.Add(new ButtonPress[]{ButtonPress.Down});
								break;
							}else{
								sequence.Add(moveInfo.buttonExecution);
							}
						}*/
						
						// Finally, add one or more "empty frame" after each attack to avoid 
						// the accidental execution of another movement.
						for (int k = 0; k < framesAfterAttack; ++k){
							sequence.Add(this.noButtonsPressed);
						}
						
						// Now we store the sequence inside the move itself
						moveInfo.simulatedInputs = sequence.ToArray();
						++index;
					}
				}
			}
		}else{
			this.idleSimulatedInput = null;
			this.moveForwardSimulatedInput = null;
			this.moveBackwardSimulatedInput = null;
			this.crouchSimulatedInput = null;
			this.crouchBlockSimulatedInput = null;
			this.jumpBackwardSimulatedInput = null;
			this.jumpBlockSimulatedInput = null;
			this.jumpForwardSimulatedInput = null;
			this.jumpStraightSimulatedInput = null;
			this.standBlockSimulatedInput = null;
		}
	}
	
	protected virtual ButtonPress[][] Repeat(ButtonPress[] originalInput, int frames){
		ButtonPress[][] input = new ButtonPress[frames][];
		
		for (int i = 0; i < frames; ++i){
			input[i] = originalInput;
		}
		
		return input;
	}
	
	protected virtual void RequestAIUpdate(ControlsScript self, ControlsScript opponent, float deltaTime){
		if (self != null && opponent != null){
			// If both ControlsScript are defined, retrieve the current position of each character...
			Vector3 currentPositionSelf = self.transform.position;
			if (this.previousPositionSelf == null){
				this.previousPositionSelf = currentPositionSelf;
			}
			
			Vector3 currentPositionOpponent = opponent.transform.position;
			if (this.previousPositionOpponent == null){
				this.previousPositionOpponent = currentPositionOpponent;
			}
			
			// Calculate the "normalized speed" of each character...
			// (if the enemy is on the left side of the screen, multiply the speed.X by -1)
			Vector3 speedSelf = (currentPositionSelf - this.previousPositionSelf.Value) / deltaTime;
			Vector3 speedOpponent = (currentPositionOpponent - this.previousPositionOpponent.Value) / deltaTime;
			
			if (currentPositionOpponent.x < currentPositionSelf.x){
				speedSelf = new Vector3(-speedSelf.x, speedSelf.y, speedSelf.z);
				speedOpponent = new Vector3(-speedOpponent.x, speedOpponent.y, speedOpponent.z);
			}
			
			// Update the "previous position" so we can calculate the speed the next time we invoke this function
			this.previousPositionSelf = currentPositionSelf;
			this.previousPositionOpponent = currentPositionOpponent;
			
			
			//---------------------------------------------------------------------------------------------------------
			// INFERENCE SYSTEM REQUEST: ASK THE GAME ENGINE THE VALUES OF THE INPUT VARIABLES
			//---------------------------------------------------------------------------------------------------------
			// Retrieve the information of the AI-Controlled character
			float attackingSelf = (float)((int)AIBoolean.FALSE);
			float attackTypeSelf = float.MinValue;
			float attackDamageSelf = float.MinValue;
			float attackGaugeSelf = float.MinValue;
			float attackHitConfirmSelf = float.MinValue;
			float attackStartupSpeedSelf = float.MinValue;
			float attackRecoverySpeedSelf = float.MinValue;
			float attackHitTypeSelf = float.MinValue;
			float attackFrameDataSelf = float.MinValue;
			float attackPreferableDistanceSelf = float.MinValue;
			float characterBlockingSelf = (float)((int)(self.isBlocking ? AIBoolean.TRUE : AIBoolean.FALSE));
            float characterDistanceSelf = (float)self.normalizedDistance;
			float characterDownSelf = (float)((int)(self.currentState == PossibleStates.Down ? AIBoolean.TRUE : AIBoolean.FALSE));
			float characterGaugeSelf = (float)self.currentGaugesPoints[0] / (float)(self.myInfo.maxGaugePoints);
            float characterHealthSelf = (float)self.currentLifePoints / (float)(self.myInfo.lifePoints);
			float characterHorizontalMovementSelf;
			float characterHorizontalMovementSpeedSelf;
            float characterJumpArcSelf = (float)self.normalizedJumpArc;
			float characterStunnedSelf = (float)((int)(self.currentSubState == SubStates.Stunned ? AIBoolean.TRUE : AIBoolean.FALSE));
			
			float normalizedHorizontalSpeedSelf = speedSelf.x;
			if (Mathf.Approximately(normalizedHorizontalSpeedSelf, 0f)){
				characterHorizontalMovementSelf = (float)((int)AIHorizontalMovement.Still);
				characterHorizontalMovementSpeedSelf = 0f;
			}else if (normalizedHorizontalSpeedSelf > 0f){
				characterHorizontalMovementSelf = (float)((int)AIHorizontalMovement.MovingForward);
				characterHorizontalMovementSpeedSelf = normalizedHorizontalSpeedSelf;
			}else{
				characterHorizontalMovementSelf = (float)((int)AIHorizontalMovement.MovingBack);
				characterHorizontalMovementSpeedSelf = -normalizedHorizontalSpeedSelf;
			}
			
			float characterVerticalMovementSelf = float.MinValue;
			if (self.currentState == PossibleStates.Crouch){
				characterVerticalMovementSelf = (float)((int)AIVerticalMovement.Crouching);
			}else if(self.currentState == PossibleStates.BackJump || self.currentState == PossibleStates.NeutralJump || self.currentState == PossibleStates.ForwardJump){
				characterVerticalMovementSelf = (float)((int)AIVerticalMovement.Jumping);
			}else{
				characterVerticalMovementSelf = (float)((int)AIVerticalMovement.Standing);
			}
			
			MoveInfo myMoveInfo = self.currentMove;
			if (myMoveInfo != null){
				attackingSelf = (float)((int)AIBoolean.TRUE);
				attackTypeSelf = (float)((int)myMoveInfo.moveClassification.attackType);
				attackGaugeSelf = myMoveInfo.gauges.Length > 0 ? (float)(myMoveInfo.gauges[0]._gaugeUsage / self.myInfo.maxGaugePoints) : 0;
				attackFrameDataSelf = (float)((int)myMoveInfo.currentFrameData);
				attackPreferableDistanceSelf = (float)((int)myMoveInfo.moveClassification.preferableDistance);
				
				attackHitConfirmSelf = (float)((int)myMoveInfo.moveClassification.hitConfirmType);
				attackStartupSpeedSelf = (float)((int)myMoveInfo.moveClassification.startupSpeed);
				attackRecoverySpeedSelf = (float)((int)myMoveInfo.moveClassification.recoverySpeed);
				attackHitTypeSelf = (float)((int)myMoveInfo.moveClassification.hitType);
				
				attackDamageSelf = 0f;
				foreach(Hit hit in myMoveInfo.hits) attackDamageSelf += (float)hit._damageOnHit;
			}
			
			// Retrieve the information about the opponent...
			float attackingOpponent = (float)((int)AIBoolean.FALSE);
			float attackTypeOpponent = float.MinValue;
			float attackDamageOpponent = float.MinValue;
			float attackGaugeOpponent = float.MinValue;
			float attackHitConfirmOpponent = float.MinValue;
			float attackStartupSpeedOpponent = float.MinValue;
			float attackRecoverySpeedOpponent = float.MinValue;
			float attackHitTypeOpponent = float.MinValue;
			float attackFrameDataOpponent = float.MinValue;
			float attackPreferableDistanceOpponent = float.MinValue;
			float characterBlockingOpponent = (float)((int)(opponent.isBlocking ? AIBoolean.TRUE : AIBoolean.FALSE));
            float characterDistanceOpponent = (float)opponent.normalizedDistance;
			float characterDownOpponent = (float)((int)(opponent.currentState == PossibleStates.Down ? AIBoolean.TRUE : AIBoolean.FALSE));
            float characterGaugeOpponent = (float)opponent.currentGaugesPoints[0] / (float)(opponent.myInfo.maxGaugePoints);
            float characterHealthOpponent = (float)opponent.currentLifePoints / (float)(opponent.myInfo.lifePoints);
			float characterHorizontalMovementOpponent;
			float characterHorizontalMovementSpeedOpponent;
            float characterJumpArcOpponent = (float)opponent.normalizedJumpArc;
			float characterStunnedOpponent = (float)((int)(opponent.currentSubState == SubStates.Stunned ? AIBoolean.TRUE : AIBoolean.FALSE));
			
			float normalizedHorizontalSpeedOpponent = speedOpponent.x;
			if (Mathf.Approximately(normalizedHorizontalSpeedOpponent, 0f)){
				characterHorizontalMovementOpponent = (float)((int)AIHorizontalMovement.Still);
				characterHorizontalMovementSpeedOpponent = 0f;
			}else if (normalizedHorizontalSpeedOpponent > 0f){
				characterHorizontalMovementOpponent = (float)((int)AIHorizontalMovement.MovingForward);
				characterHorizontalMovementSpeedOpponent = normalizedHorizontalSpeedOpponent;
			}else{
				characterHorizontalMovementOpponent = (float)((int)AIHorizontalMovement.MovingBack);
				characterHorizontalMovementSpeedOpponent = -normalizedHorizontalSpeedOpponent;
			}
			
			float characterVerticalMovementOpponent = float.MinValue;
			if (opponent.currentState == PossibleStates.Crouch){
				characterVerticalMovementOpponent = (float)((int)AIVerticalMovement.Crouching);
			}else if(opponent.currentState == PossibleStates.BackJump || opponent.currentState == PossibleStates.NeutralJump || opponent.currentState == PossibleStates.ForwardJump){
				characterVerticalMovementOpponent = (float)((int)AIVerticalMovement.Jumping);
			}else{
				characterVerticalMovementOpponent = (float)((int)AIVerticalMovement.Standing);
			}
			
			MoveInfo opMoveInfo = opponent.currentMove;
			if (opMoveInfo != null){
				attackingOpponent = (float)((int)AIBoolean.TRUE);
				attackTypeOpponent = (float)((int)opMoveInfo.moveClassification.attackType);
				attackGaugeOpponent = opMoveInfo.gauges.Length > 0 ? (float)(opMoveInfo.gauges[0]._gaugeUsage / opponent.myInfo.maxGaugePoints) : 0;
				attackFrameDataOpponent = (float)((int)opMoveInfo.currentFrameData);
				attackPreferableDistanceOpponent =  (float)((int)opMoveInfo.moveClassification.preferableDistance);
				
				attackHitConfirmOpponent = (float)((int)opMoveInfo.moveClassification.hitConfirmType);
				attackStartupSpeedOpponent = (float)((int)opMoveInfo.moveClassification.startupSpeed);
				attackRecoverySpeedOpponent = (float)((int)opMoveInfo.moveClassification.recoverySpeed);
				attackHitTypeOpponent = (float)((int)opMoveInfo.moveClassification.hitType);
				
				attackDamageOpponent = 0f;
				foreach(Hit hit in opMoveInfo.hits) attackDamageOpponent += (float)hit._damageOnHit;
			}
			
			
			//---------------------------------------------------------------------------------------------------------
			// INFERENCE SYSTEM REQUEST: SEND THE INFORMATION OF THE ENGINE TO THE INFERENCE ENGINE 
			//---------------------------------------------------------------------------------------------------------
			this.inferenceEngine.SetInput(AICondition.Attacking_Self, attackingSelf);
			this.inferenceEngine.SetInput(AICondition.Attacking_AttackType_Self, attackTypeSelf);
			this.inferenceEngine.SetInput(AICondition.Attacking_Damage_Self, attackDamageSelf);
			this.inferenceEngine.SetInput(AICondition.Attacking_GaugeUsage_Self, attackGaugeSelf);
			this.inferenceEngine.SetInput(AICondition.Attacking_HitConfirmType_Self, attackHitConfirmSelf);
			this.inferenceEngine.SetInput(AICondition.Attacking_StartupSpeed_Self, attackStartupSpeedSelf);
			this.inferenceEngine.SetInput(AICondition.Attacking_RecoverySpeed_Self, attackRecoverySpeedSelf);
			this.inferenceEngine.SetInput(AICondition.Attacking_HitType_Self, attackHitTypeSelf);
			this.inferenceEngine.SetInput(AICondition.Attacking_FrameData_Self, attackFrameDataSelf);
			this.inferenceEngine.SetInput(AICondition.Attacking_PreferableDistance_Self, attackPreferableDistanceSelf);
			this.inferenceEngine.SetInput(AICondition.Blocking_Self, characterBlockingSelf);
			this.inferenceEngine.SetInput(AICondition.Distance_Self, characterDistanceSelf);
			this.inferenceEngine.SetInput(AICondition.Down_Self, characterDownSelf);
			this.inferenceEngine.SetInput(AICondition.Gauge_Self, characterGaugeSelf);
			this.inferenceEngine.SetInput(AICondition.Health_Self, characterHealthSelf);
			this.inferenceEngine.SetInput(AICondition.HorizontalMovement_Self, characterHorizontalMovementSelf);
			this.inferenceEngine.SetInput(AICondition.HorizontalMovementSpeed_Self, characterHorizontalMovementSpeedSelf);
			this.inferenceEngine.SetInput(AICondition.JumpArc_Self, characterJumpArcSelf);
			this.inferenceEngine.SetInput(AICondition.Stunned_Self, characterStunnedSelf);
			this.inferenceEngine.SetInput(AICondition.VerticalMovement_Self, characterVerticalMovementSelf);
			
			this.inferenceEngine.SetInput(AICondition.Attacking_Opponent, attackingOpponent);
			this.inferenceEngine.SetInput(AICondition.Attacking_AttackType_Opponent, attackTypeOpponent);
			this.inferenceEngine.SetInput(AICondition.Attacking_Damage_Opponent, attackDamageOpponent);
			this.inferenceEngine.SetInput(AICondition.Attacking_GaugeUsage_Opponent, attackGaugeOpponent);
			this.inferenceEngine.SetInput(AICondition.Attacking_HitConfirmType_Opponent, attackHitConfirmOpponent);
			this.inferenceEngine.SetInput(AICondition.Attacking_StartupSpeed_Opponent, attackStartupSpeedOpponent);
			this.inferenceEngine.SetInput(AICondition.Attacking_RecoverySpeed_Opponent, attackRecoverySpeedOpponent);
			this.inferenceEngine.SetInput(AICondition.Attacking_HitType_Opponent, attackHitTypeOpponent);
			this.inferenceEngine.SetInput(AICondition.Attacking_FrameData_Opponent, attackFrameDataOpponent);
			this.inferenceEngine.SetInput(AICondition.Attacking_PreferableDistance_Opponent, attackPreferableDistanceOpponent);
			this.inferenceEngine.SetInput(AICondition.Blocking_Opponent, characterBlockingOpponent);
			this.inferenceEngine.SetInput(AICondition.Distance_Opponent, characterDistanceOpponent);
			this.inferenceEngine.SetInput(AICondition.Down_Opponent, characterDownOpponent);
			this.inferenceEngine.SetInput(AICondition.Gauge_Opponent, characterGaugeOpponent);
			this.inferenceEngine.SetInput(AICondition.Health_Opponent, characterHealthOpponent);
			this.inferenceEngine.SetInput(AICondition.HorizontalMovement_Opponent, characterHorizontalMovementOpponent);
			this.inferenceEngine.SetInput(AICondition.HorizontalMovementSpeed_Opponent, characterHorizontalMovementSpeedOpponent);
			this.inferenceEngine.SetInput(AICondition.JumpArc_Opponent, characterJumpArcOpponent);
			this.inferenceEngine.SetInput(AICondition.Stunned_Opponent, characterStunnedOpponent);
			this.inferenceEngine.SetInput(AICondition.VerticalMovement_Opponent, characterVerticalMovementOpponent);
			
			
			//---------------------------------------------------------------------------------------------------------
			// INFERENCE SYSTEM REQUEST: CHECK WHICH OF THE POSSIBLE REACTIONS ARE PHISICALLY POSSIBLE AT THIS MOMENT
			//---------------------------------------------------------------------------------------------------------
			HashSet<string> requestedOutputs = new HashSet<string>();
			if (this.ValidateReaction(AIReactionType.Crouch, self, opponent)){
				requestedOutputs.Add(AIReaction.Crouch);
			}
			if (this.ValidateReaction(AIReactionType.CrouchBlock, self, opponent)){
				requestedOutputs.Add(AIReaction.CrouchBlock);
			}
			if (this.ValidateReaction(AIReactionType.Idle, self, opponent)){
				requestedOutputs.Add(AIReaction.Idle);
			}
			if (this.ValidateReaction(AIReactionType.JumpBack, self, opponent)){
				requestedOutputs.Add(AIReaction.JumpBackward);
			}
			if (this.ValidateReaction(AIReactionType.JumpBlock, self, opponent)){
				requestedOutputs.Add(AIReaction.JumpBlock);
			}
			if (this.ValidateReaction(AIReactionType.JumpForward, self, opponent)){
				requestedOutputs.Add(AIReaction.JumpForward);
			}
			if (this.ValidateReaction(AIReactionType.JumpStraight, self, opponent)){
				requestedOutputs.Add(AIReaction.JumpStraight);
			}
			if (this.ValidateReaction(AIReactionType.MoveBack, self, opponent)){
				requestedOutputs.Add(AIReaction.MoveBackward);
			}
			if (this.ValidateReaction(AIReactionType.MoveForward, self, opponent)){
				requestedOutputs.Add(AIReaction.MoveForward);
			}
			if (this.ValidateReaction(AIReactionType.StandBlock, self, opponent)){
				requestedOutputs.Add(AIReaction.StandBlock);
			}
			if (this.ValidateReaction(AIReactionType.PlayMove, self, opponent)){
				requestedOutputs.Add(AIReaction.PlayMove_RandomAttack);
				
				if (ai.advancedOptions.reactionParameters.enableAttackTypeFilter){
					requestedOutputs.Add(AIReaction.PlayMove_AttackType_AntiAir);
					requestedOutputs.Add(AIReaction.PlayMove_AttackType_BackLauncher);
					requestedOutputs.Add(AIReaction.PlayMove_AttackType_Dive);
					requestedOutputs.Add(AIReaction.PlayMove_AttackType_ForwardLauncher);
					requestedOutputs.Add(AIReaction.PlayMove_AttackType_Neutral);
					requestedOutputs.Add(AIReaction.PlayMove_AttackType_NormalAttack);
					requestedOutputs.Add(AIReaction.PlayMove_AttackType_Projectile);
				}
				
				if (ai.advancedOptions.reactionParameters.enableDamageFilter){
					requestedOutputs.Add(AIReaction.PlayMove_Damage_Medium);
					requestedOutputs.Add(AIReaction.PlayMove_Damage_Strong);
					requestedOutputs.Add(AIReaction.PlayMove_Damage_VeryStrong);
					requestedOutputs.Add(AIReaction.PlayMove_Damage_VeryWeak);
					requestedOutputs.Add(AIReaction.PlayMove_Damage_Weak);
				}
				
				if (ai.advancedOptions.reactionParameters.enableGaugeFilter){
					requestedOutputs.Add(AIReaction.PlayMove_GaugeUsage_All);
					requestedOutputs.Add(AIReaction.PlayMove_GaugeUsage_Half);
					requestedOutputs.Add(AIReaction.PlayMove_GaugeUsage_None);
					requestedOutputs.Add(AIReaction.PlayMove_GaugeUsage_Quarter);
					requestedOutputs.Add(AIReaction.PlayMove_GaugeUsage_ThreeQuarters);
				}
				
				if (ai.advancedOptions.reactionParameters.enableHitConfirmTypeFilter){
					requestedOutputs.Add(AIReaction.PlayMove_HitConfirmType_Hit);
					requestedOutputs.Add(AIReaction.PlayMove_HitConfirmType_Throw);
				}
				
				if (ai.advancedOptions.reactionParameters.enableAttackSpeedFilter){
					requestedOutputs.Add(AIReaction.PlayMove_StartupSpeed_VeryFast);
					requestedOutputs.Add(AIReaction.PlayMove_StartupSpeed_Fast);
					requestedOutputs.Add(AIReaction.PlayMove_StartupSpeed_Normal);
					requestedOutputs.Add(AIReaction.PlayMove_StartupSpeed_Slow);
					requestedOutputs.Add(AIReaction.PlayMove_StartupSpeed_VerySlow);
					
					requestedOutputs.Add(AIReaction.PlayMove_RecoverySpeed_VeryFast);
					requestedOutputs.Add(AIReaction.PlayMove_RecoverySpeed_Fast);
					requestedOutputs.Add(AIReaction.PlayMove_RecoverySpeed_Normal);
					requestedOutputs.Add(AIReaction.PlayMove_RecoverySpeed_Slow);
					requestedOutputs.Add(AIReaction.PlayMove_RecoverySpeed_VerySlow);
				}
				
				if (ai.advancedOptions.reactionParameters.enableHitTypeFilter){
					requestedOutputs.Add(AIReaction.PlayMove_HitType_HighKnockdown);
					requestedOutputs.Add(AIReaction.PlayMove_HitType_HighLow);
					requestedOutputs.Add(AIReaction.PlayMove_HitType_KnockBack);
					requestedOutputs.Add(AIReaction.PlayMove_HitType_Launcher);
					requestedOutputs.Add(AIReaction.PlayMove_HitType_Low);
					requestedOutputs.Add(AIReaction.PlayMove_HitType_MidKnockdown);
					requestedOutputs.Add(AIReaction.PlayMove_HitType_Overhead);
					requestedOutputs.Add(AIReaction.PlayMove_HitType_Sweep);
				}
				
				if (ai.advancedOptions.reactionParameters.enableDistanceFilter){
					requestedOutputs.Add(AIReaction.PlayMove_PreferableDistance_Close);
					requestedOutputs.Add(AIReaction.PlayMove_PreferableDistance_Far);
					requestedOutputs.Add(AIReaction.PlayMove_PreferableDistance_Mid);
					requestedOutputs.Add(AIReaction.PlayMove_PreferableDistance_VeryClose);
					requestedOutputs.Add(AIReaction.PlayMove_PreferableDistance_VeryFar);
				}
			}
			
			//---------------------------------------------------------------------------------------------------------
			// INFERENCE SYSTEM REQUEST: THE "CHANGE BEHAVIOUR" REACTIONS ARE ALWASY POSSIBLE
			//---------------------------------------------------------------------------------------------------------
			requestedOutputs.Add(AIReaction.ChangeBehaviour_Aggressive);
			requestedOutputs.Add(AIReaction.ChangeBehaviour_Any);
			requestedOutputs.Add(AIReaction.ChangeBehaviour_Balanced);
			requestedOutputs.Add(AIReaction.ChangeBehaviour_Defensive);
			requestedOutputs.Add(AIReaction.ChangeBehaviour_VeryAggressive);
			requestedOutputs.Add(AIReaction.ChangeBehaviour_VeryDefensive);
			
			//---------------------------------------------------------------------------------------------------------
			// INFERENCE SYSTEM REQUEST: FINALLY, MAKE THE REQUEST TO THE INFERENCE SYSTEM
			//---------------------------------------------------------------------------------------------------------
			if (UFE.config.aiOptions.multiCoreSupport){
				this.inferenceEngine.AsyncCalculateOutputs(requestedOutputs);
			}else{
				this.inferenceEngine.SyncCalculateOutputs(requestedOutputs);
				this.aiOutput = this.inferenceEngine.Output;
			}
		}
	}
	
	protected virtual bool ValidateReaction(AIReactionType reactionType, ControlsScript self, ControlsScript opponent){
		if (self == null || opponent == null){
			return false;
		}
		
		if (reactionType == AIReactionType.Idle){
			//---------------------------------------------------------------------------------------------------------
			// We can always choose to not press any key, although the character won't become idle immediately
			//---------------------------------------------------------------------------------------------------------
			return true;
		}else if (self.Physics.isTakingOff){
			//---------------------------------------------------------------------------------------------------------
			// To avoid confusions (we don't know if the character is standing or jumping)
			// the AI shouldn't take any decision if the character is taking off, 
			// the AI should wait until the character is in the middle of the jump.
			//---------------------------------------------------------------------------------------------------------
			return false;
		}
		
		
		bool attackMovesLocked = UFE.config.lockMovements;
		bool basicMovesLocked = UFE.config.lockInputs && !UFE.config.roundOptions.allowMovementStart;
		bool isAttacking = self.currentMove != null || self.storedMove != null;
		bool isDown = (!ai.advancedOptions.reactionParameters.inputWhenDown && self.currentState == PossibleStates.Down);
		bool isStunned = (!ai.advancedOptions.reactionParameters.inputWhenStunned && self.currentSubState == SubStates.Stunned);
		bool isBlocking = (!ai.advancedOptions.reactionParameters.inputWhenBlocking && self.currentSubState == SubStates.Blocking);
		
		bool isJumping = 
			self.currentState == PossibleStates.BackJump ||
				self.currentState == PossibleStates.ForwardJump ||
				self.currentState == PossibleStates.NeutralJump;
		
		
		//bool isOpponentAttacking = opponent.currentMove != null || opponent.storedMove != null;
		bool isOpponentDown = opponent.currentState == PossibleStates.Down;
		bool isOpponentStunned = opponent.currentSubState == SubStates.Stunned;
		bool isOpponentBlocking = opponent.currentSubState == SubStates.Blocking;
		
		//-------------------------------------------------------------------------------------------------------------
		// Check if the character can execute the desired reaction at this moment:
		//-------------------------------------------------------------------------------------------------------------
		switch(reactionType){
		case AIReactionType.Crouch:
			//---------------------------------------------------------------------------------------------------------
			// The character can't crouch while he is jumping, stunned, down or executing a non-crouched attack.
			//---------------------------------------------------------------------------------------------------------
			return !basicMovesLocked && !isJumping && !isStunned && !isDown && !isAttacking;
			
		case AIReactionType.CrouchBlock:
			//---------------------------------------------------------------------------------------------------------
			// The character can't crouch-block while he is jumping, stunned, down or executing any kind of attack.
			//---------------------------------------------------------------------------------------------------------
			return 
				!basicMovesLocked && 
					!isJumping && 
					!isStunned && 
					!isDown && 
					!isAttacking &&
					!(this.ai.advancedOptions.reactionParameters.stopBlockingWhenEnemyIsStunned && isOpponentStunned);
			
		case AIReactionType.JumpBack:
			//---------------------------------------------------------------------------------------------------------
			// The character can't jump backwards while he is stunned or down.
			//---------------------------------------------------------------------------------------------------------
			return 
				!basicMovesLocked &&
					!isStunned && 
					!isDown && 
					!isAttacking && 
					self.Physics.currentAirJumps < self.myInfo.physics.multiJumps;
			
		case AIReactionType.JumpBlock:
			//---------------------------------------------------------------------------------------------------------
			// The character can't jump-block if he isn't jumping or if he is stunned or down.
			//---------------------------------------------------------------------------------------------------------
			return 
				UFE.config.blockOptions.allowAirBlock && 
					!basicMovesLocked && 
					!isStunned && 
					!isDown && 
					isJumping && 
					!isAttacking &&
					!(this.ai.advancedOptions.reactionParameters.stopBlockingWhenEnemyIsStunned && isOpponentStunned);
			
		case AIReactionType.JumpForward:
			//---------------------------------------------------------------------------------------------------------
			// The character can't jump forwards while he is stunned or down.
			//---------------------------------------------------------------------------------------------------------
			return 
				!basicMovesLocked &&
					!isStunned && 
					!isDown && 
					!isAttacking && 
					self.Physics.currentAirJumps < self.myInfo.physics.multiJumps;
			
		case AIReactionType.JumpStraight:
			//---------------------------------------------------------------------------------------------------------
			// The character can't jump straight while he is stunned or down.
			// He can't jump either if he is already jumping and has reached the max number of jumps without landing.
			//---------------------------------------------------------------------------------------------------------
			return 
				!basicMovesLocked &&
					!isStunned && 
					!isDown && 
					!isAttacking && self.Physics.currentAirJumps < self.myInfo.physics.multiJumps;
			
		case AIReactionType.MoveBack:
			//---------------------------------------------------------------------------------------------------------
			// The character can't move backwards if he is jumping or if he is stunned or down.
			//---------------------------------------------------------------------------------------------------------
			return !basicMovesLocked && !isStunned && !isDown && !isJumping && !isAttacking;
			
		case AIReactionType.MoveForward:
			//---------------------------------------------------------------------------------------------------------
			// The character can't move forwards if he is jumping or if he is stunned or down.
			//---------------------------------------------------------------------------------------------------------
			return !basicMovesLocked && !isStunned && !isDown && !isJumping && !isAttacking;
			
		case AIReactionType.StandBlock:
			//---------------------------------------------------------------------------------------------------------
			// The character can't stand block if he is jumping or if he is stunned or down.
			//---------------------------------------------------------------------------------------------------------
			return 
				!basicMovesLocked && 
					!isStunned && 
					!isDown && 
					!isJumping && 
					!isAttacking &&
					!(this.ai.advancedOptions.reactionParameters.stopBlockingWhenEnemyIsStunned && isOpponentStunned);
			
		case AIReactionType.PlayMove:
			//---------------------------------------------------------------------------------------------------------
			// The character can't execute any "attack move" while he is stunned or down. 
			//---------------------------------------------------------------------------------------------------------
			return 
				!basicMovesLocked &&
					!attackMovesLocked &&
					!isStunned && 
					!isDown && 
					!isBlocking &&
					(this.ai.advancedOptions.reactionParameters.attackWhenEnemyIsDown || !isOpponentDown) &&
					(this.ai.advancedOptions.reactionParameters.attackWhenEnemyIsBlocking || !isOpponentBlocking);
		default:
			return true;
		}
	}
	#endregion
}
