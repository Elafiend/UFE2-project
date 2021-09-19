using System;
using System.Collections.Generic;
using System.Threading;
using AI4Unity.Fuzzy;

public class InferenceSystemThread{
	#region public instance properties
	public float DefaultValue{
		get{return this.defaultValue;}
	}

	public bool Done{
		get{return this.done;}
	}

	public Dictionary<string, float> Output{
		get{return this.output;}
	}
	#endregion

	#region private instance fields
	private float defaultValue;
	private volatile bool done;
	private InferenceSystem inferenceEngine;
	private Dictionary<string, float> output;
	private HashSet<string> requestedOutputs;
	#endregion

	#region public instance constructors
	public InferenceSystemThread(InferenceSystem inferenceEngine, float defaultValue){
		this.requestedOutputs = new HashSet<string>();
		this.inferenceEngine = inferenceEngine;
		this.defaultValue = defaultValue;

		this.output = null;
		this.done = true;
	}
	#endregion

	#region public instance methods
	public Thread AsyncCalculateOutputs(HashSet<string> requestedOutputs){
		this.done = false;
		this.output = null;
		this.requestedOutputs = requestedOutputs;
		
		Thread t = new Thread(this.Run);
		t.Start();
		return t;
	}

	public AForge.Fuzzy.LinguisticVariable GetInputVariable(string variableName){
		return this.inferenceEngine.GetInputVariable(variableName);
	}

	public void SetInput(string variableName, float input){
		this.inferenceEngine.SetInput(variableName, input);
	}

	public void SetInputs(Dictionary<string, float> inputs){
		this.inferenceEngine.SetInputs(inputs);
	}

	public void SyncCalculateOutputs(HashSet<string> requestedOutputs){
		this.done = false;
		this.output = null;
		this.requestedOutputs = requestedOutputs;
		this.Run();
	}
	#endregion

	#region protected instance methods
	protected void Run(){
		this.output = this.inferenceEngine.Evaluate(this.requestedOutputs, this.defaultValue);
		this.done = true;
	}
	#endregion
}
