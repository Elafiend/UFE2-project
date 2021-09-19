using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FPLibrary;
//using UnityEngine.Experimental.Director;

[System.Serializable]
public class MecanimAnimationData {
	public AnimationClip clip;
	public string clipName;
	public WrapMode wrapMode;
    public bool applyRootMotion;
    public Fix64 length = 1;
    public Fix64 originalSpeed = 1;

    public Fix64 transitionDuration = -1;
    public Fix64 normalizedSpeed = 1;
    public string stateName;

    #region trackable definitions
    public Fix64 normalizedTime = 1;
    public Fix64 secondsPlayed = 0;
    public Fix64 ticksPlayed = 0;
    public Fix64 framesPlayed = 0;
    public Fix64 realFramesPlayed = 0;
    public int timesPlayed = 0;
    public Fix64 speed = 1;
    #endregion
}

[RequireComponent (typeof (Animator))]
public class MecanimControl : MonoBehaviour {

	public MecanimAnimationData defaultAnimation = new MecanimAnimationData();
	public MecanimAnimationData[] animations = new MecanimAnimationData[0];
    
	public bool debugMode = false;

    public bool normalizeFrames = true;
    public bool overrideAnimatorUpdate = false;
    public Fix64 defaultTransitionDuration = 0.15;
	public WrapMode defaultWrapMode = WrapMode.Loop;


    public Animator animator;

    #region trackable definitions
    public RuntimeAnimatorController runtimeAnimatorController { get { return this.animator.runtimeAnimatorController; } set { this.animator.runtimeAnimatorController = value; } }
    public bool applyRootMotion { get { return this.animator.applyRootMotion; } set { this.animator.applyRootMotion = value; } }
    public AnimatorOverrideController overrideController;
    public MecanimAnimationData currentAnimationData;
    public bool currentMirror;
    public string currentState;
    public Fix64 currentSpeed;
    public Vector3 deltaDisplacement;
    #endregion

    public Vector3 lastPosition;

	public delegate void AnimEvent(MecanimAnimationData animationData);
	public static event AnimEvent OnAnimationBegin;
	public static event AnimEvent OnAnimationEnd;
	public static event AnimEvent OnAnimationLoop;


    public bool alwaysPlay = false;
    public bool overrideRootMotion = false;

    private RuntimeAnimatorController controller;

	// UNITY METHODS
	void Awake () {
        animator = gameObject.GetComponent<Animator>();
        animator.logWarnings = false;
        animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
        controller = (RuntimeAnimatorController)Instantiate(Resources.Load("MC_Controller"));

        foreach (MecanimAnimationData animData in animations) {
			if (animData.wrapMode == WrapMode.Default) animData.wrapMode = defaultWrapMode;
			animData.clip.wrapMode = animData.wrapMode;
		}

	}
	
	void Start(){
		if (defaultAnimation.clip == null && animations.Length > 0){
			SetDefaultClip(animations[0].clip, "State1", animations[0].speed, animations[0].wrapMode, false);
		}

        if (defaultAnimation.clip != null && currentAnimationData == null) {
			foreach(MecanimAnimationData animData in animations) {
				if (animData.clip == defaultAnimation.clip)
					defaultAnimation.clip = (AnimationClip) Instantiate(defaultAnimation.clip);
			}
            defaultAnimation.stateName = "State1";
            defaultAnimation.length = 1;
			currentAnimationData = defaultAnimation;

            AnimatorOverrideController overrideController = new AnimatorOverrideController();
            overrideController.runtimeAnimatorController = controller;
            overrideController["Default"] = currentAnimationData.clip;
            overrideController["State1"] = currentAnimationData.clip;

			animator.runtimeAnimatorController = overrideController;
			animator.Play("State1", 0, 0);

			if (overrideRootMotion) animator.applyRootMotion = currentAnimationData.applyRootMotion;
			SetSpeed(currentAnimationData.speed);
		}
	}
	
	public void DoFixedUpdate(){
        //WrapMode emulator
        if (overrideAnimatorUpdate) {
            animator.enabled = false;
            animator.Update((float)UFE.fixedDeltaTime);
        }

        if (currentAnimationData == null || currentAnimationData.clip == null) return;
        
        deltaDisplacement += animator.deltaPosition;
        currentAnimationData.secondsPlayed += FPMath.Abs(UFE.fixedDeltaTime * GetSpeed());
        currentAnimationData.ticksPlayed += FPMath.Abs(UFE.fixedDeltaTime * UFE.fps * GetSpeed());
        currentAnimationData.framesPlayed = (int)currentAnimationData.ticksPlayed;
        currentAnimationData.realFramesPlayed += FPMath.Abs(UFE.fixedDeltaTime * UFE.fps * GetNormalizedSpeed());

        if (normalizeFrames)
            OffSetNormalizedFrame();

        if (currentAnimationData.secondsPlayed > currentAnimationData.length) currentAnimationData.secondsPlayed = currentAnimationData.length;
        currentAnimationData.normalizedTime = currentAnimationData.secondsPlayed / currentAnimationData.length;

        if (currentAnimationData.secondsPlayed == currentAnimationData.length)
        {
            if (currentAnimationData.clip.wrapMode == WrapMode.Loop || currentAnimationData.clip.wrapMode == WrapMode.PingPong) {
				if (OnAnimationLoop != null) OnAnimationLoop(currentAnimationData);
				currentAnimationData.timesPlayed ++;
				
				if (currentAnimationData.clip.wrapMode == WrapMode.Loop) {
					SetCurrentClipPosition(0);
				}
				
				if (currentAnimationData.clip.wrapMode == WrapMode.PingPong) {
					SetSpeed(currentAnimationData.clipName, -currentAnimationData.speed);
					SetCurrentClipPosition(0);
				}
				
			}else if (currentAnimationData.timesPlayed == 0) {
				if (OnAnimationEnd != null) OnAnimationEnd(currentAnimationData);
                currentAnimationData.timesPlayed = 1;

				if ((currentAnimationData.clip.wrapMode == WrapMode.Once ||
                    currentAnimationData.clip.wrapMode == WrapMode.Clamp) 
                    && alwaysPlay) {
					Play(defaultAnimation, currentMirror);
                } else if (!alwaysPlay) {
                    SetSpeed(0);
				}
			}
        }
    }

    private void OnAnimatorMove() {
        // Implements controlled root motion for recording and output
    }

    void OnGUI(){
		//Toggle debug mode to see the live data in action
		if (debugMode) {
			GUI.Box (new Rect (Screen.width - 340,40,340,440), "Animation Data");
			GUI.BeginGroup(new Rect (Screen.width - 330,60,400,440));{
				
				AnimatorClipInfo[] animationInfoArray = animator.GetCurrentAnimatorClipInfo(0);
				foreach (AnimatorClipInfo animationInfo in animationInfoArray){
					AnimatorStateInfo animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
					GUILayout.Label(animationInfo.clip.name);
					GUILayout.Label("-Wrap Mode: "+ animationInfo.clip.wrapMode);
					GUILayout.Label("-Is Playing: "+ IsPlaying(animationInfo.clip));
					GUILayout.Label("-Blend Weight: "+ animationInfo.weight);
					GUILayout.Label("-Normalized Time: "+ animatorStateInfo.normalizedTime);
					GUILayout.Label("-True Length: "+ animationInfo.clip.length);
					GUILayout.Label("----");
				}

                GUILayout.Label("Global Speed: " + GetSpeed().ToString());

				GUILayout.Label("--Current Animation Data--");
                GUILayout.Label("-Clip Name: " + currentAnimationData.clipName);
                GUILayout.Label("-Animation Speed: " + GetSpeed(currentAnimationData).ToString());
				GUILayout.Label("-Normalized Speed: "+ GetNormalizedSpeed(currentAnimationData));
				GUILayout.Label("-Times Played: "+ currentAnimationData.timesPlayed);
				GUILayout.Label("-Seconds Played: "+ currentAnimationData.secondsPlayed);
                GUILayout.Label("-Emulated Length: " + currentAnimationData.length);
                GUILayout.Label("-Frames Played ("+ UFE.fps + " FPS): " + currentAnimationData.framesPlayed);
                GUILayout.Label("-Normalized Time: " + currentAnimationData.normalizedTime);
			}GUI.EndGroup();
		}
	}
	

	// MECANIM CONTROL METHODS
	public void RemoveClip(string name) {
		List<MecanimAnimationData> animationDataList = new List<MecanimAnimationData>(animations);
		animationDataList.Remove(GetAnimationData(name));
		animations = animationDataList.ToArray();
	}

	public void RemoveClip(AnimationClip clip) {
		List<MecanimAnimationData> animationDataList = new List<MecanimAnimationData>(animations);
		animationDataList.Remove(GetAnimationData(clip));
		animations = animationDataList.ToArray();
    }

    public void Clear() {
        animations = new MecanimAnimationData[0];
    }

    public void SetDefaultClip(AnimationClip clip, string name, Fix64 speed, WrapMode wrapMode, bool mirror) {
		defaultAnimation.clip = (AnimationClip) Instantiate(clip);
		defaultAnimation.clip.wrapMode = wrapMode;
		defaultAnimation.clipName = name;
		defaultAnimation.speed = speed;
		defaultAnimation.originalSpeed = speed;
		defaultAnimation.transitionDuration = -1;
		defaultAnimation.wrapMode = wrapMode;
	}
	
	public void AddClip(AnimationClip clip, string newName) {
		AddClip(clip, newName, 1, defaultWrapMode);
	}

    public void AddClip(AnimationClip clip, string newName, Fix64 speed, WrapMode wrapMode) {
        AddClip(clip, newName, speed, wrapMode, clip.length);
    }

    public void AddClip(AnimationClip clip, string newName, Fix64 speed, WrapMode wrapMode, Fix64 length) {
		if (GetAnimationData(newName) != null) Debug.LogWarning("An animation with the name '"+ newName +"' already exists.");
		MecanimAnimationData animData = new MecanimAnimationData();
		animData.clip = (AnimationClip) Instantiate(clip);
		//if (wrapMode == WrapMode.Default) wrapMode = defaultWrapMode;
		animData.clip.wrapMode = wrapMode;
		animData.clip.name = newName;
		animData.clipName = newName;
        animData.speed = speed;
        animData.originalSpeed = speed;
        animData.length = length;
		animData.wrapMode = wrapMode;

		List<MecanimAnimationData> animationDataList = new List<MecanimAnimationData>(animations);
		animationDataList.Add(animData);
		animations = animationDataList.ToArray();
	}

	public MecanimAnimationData GetAnimationData(string clipName){
		foreach(MecanimAnimationData animData in animations){
			if (animData.clipName == clipName){
				return animData;
			}
		}
		if (clipName == defaultAnimation.clipName) return defaultAnimation;
		return null;
	}

	public MecanimAnimationData GetAnimationData(AnimationClip clip){
		foreach(MecanimAnimationData animData in animations){
			if (animData.clip == clip){
				return animData;
			}
		}
		if (clip == defaultAnimation.clip) return defaultAnimation;
		return null;
	}

    public void CopyAnimationData(MecanimAnimationData from, ref MecanimAnimationData to) {
        if (from == null || from.clip == null) return;
        to.clip = (AnimationClip)Instantiate(from.clip);
        to.clip.wrapMode = from.clip.wrapMode;
        to.clip.name = from.clip.name;
        to.clipName = from.clipName;
        to.speed = from.speed;
        to.transitionDuration = from.transitionDuration;
        to.wrapMode = from.wrapMode;
        to.applyRootMotion = from.applyRootMotion;
        to.timesPlayed = from.timesPlayed;
        to.secondsPlayed = from.secondsPlayed;
        to.length = from.length;
        to.originalSpeed = from.originalSpeed;
        to.normalizedSpeed = from.normalizedSpeed;
        to.normalizedTime = from.normalizedTime;
        to.stateName = from.stateName;
    }
	
	public void CrossFade(string clipName, Fix64 blendingTime){
		CrossFade(clipName, blendingTime, 0, currentMirror);
	}

    public void CrossFade(string clipName, Fix64 blendingTime, Fix64 normalizedTime, bool mirror) {
        Play(GetAnimationData(clipName), blendingTime, normalizedTime, mirror);
	}

    public void CrossFade(MecanimAnimationData animationData, Fix64 blendingTime, Fix64 normalizedTime, bool mirror) {
        Play(animationData, blendingTime, normalizedTime, mirror);
	}

    public void Play(string clipName, Fix64 blendingTime, Fix64 normalizedTime, bool mirror) {
        Play(GetAnimationData(clipName), blendingTime, normalizedTime, mirror);
	}

    public void Play(AnimationClip clip, Fix64 blendingTime, Fix64 normalizedTime, bool mirror) {
        Play(GetAnimationData(clip), blendingTime, normalizedTime, mirror);
	}

	public void Play(string clipName, bool mirror){
        Play(GetAnimationData(clipName), 0, 0, mirror);
	}

	public void Play(string clipName){
        Play(GetAnimationData(clipName), 0, 0, currentMirror);
	}
	
	public void Play(AnimationClip clip, bool mirror){
        Play(GetAnimationData(clip), 0, 0, mirror);
	}

	public void Play(AnimationClip clip){
        Play(GetAnimationData(clip), 0, 0, currentMirror);
	}

	public void Play(MecanimAnimationData animationData, bool mirror){
        Play(animationData, animationData.transitionDuration, 0, mirror);
	}

	public void Play(MecanimAnimationData animationData){
        Play(animationData, animationData.transitionDuration, 0, currentMirror);
    }

    public void Play(MecanimAnimationData animationData, Fix64 blendingTime, Fix64 normalizedTime, bool mirror) {
		_playAnimation(animationData, blendingTime, normalizedTime, mirror);
        //DirectorPlay(animationData, blendingTime, normalizedTime, mirror);
	}

	public void Play(){
        SetSpeed(currentAnimationData.speed);
	}

    public void Refresh()
    {
        //overrideController.runtimeAnimatorController = controller;
        //animator.runtimeAnimatorController = overrideController;
        animator.Play(currentState, 0, (float)currentAnimationData.normalizedTime);
        animator.applyRootMotion = currentAnimationData.applyRootMotion;
        animator.Update(0);
        SetSpeed(currentSpeed);
    }

    public void OffSetNormalizedFrame()
    {
        animator.Play(currentAnimationData.stateName, 0, (float)(((currentAnimationData.framesPlayed + 0.1f) / UFE.fps) / currentAnimationData.length));
        animator.Update(0);
    }

    private void _playAnimation(MecanimAnimationData targetAnimationData, Fix64 blendingTime, Fix64 normalizedTime, bool mirror) {
		//The overrite machine. Creates an overrideController, replace its core animations and restate it back in
		if (targetAnimationData == null || targetAnimationData.clip == null) return;

        bool prevMirror = currentMirror;
        currentMirror = mirror;

        Fix64 animSpeed = targetAnimationData.originalSpeed * (targetAnimationData.originalSpeed < 0? - 1 : 1);

		Fix64 currentNormalizedTime = GetCurrentClipPosition();
        currentState = "State1";

        if (!mirror){
            if (targetAnimationData.originalSpeed >= 0){
                currentState = "State1";
			}else{
                currentState = "State2";
			}
		}else{
            if (targetAnimationData.originalSpeed >= 0){
                currentState = "State3";
			}else{
                currentState = "State4";
			}
		}

        overrideController = new AnimatorOverrideController();
        overrideController.runtimeAnimatorController = controller;

        if (currentAnimationData != null && currentAnimationData.clip != null) 
            overrideController["Default"] = currentAnimationData.clip;

        overrideController[currentState] = targetAnimationData.clip;

        if (blendingTime == -1) blendingTime = currentAnimationData.transitionDuration;
        if (blendingTime == -1) blendingTime = defaultTransitionDuration;

        if (blendingTime <= 0 || currentAnimationData == null) {
			animator.runtimeAnimatorController = overrideController;
            animator.Play(currentState, 0, (float)normalizedTime);
		}else{
			animator.runtimeAnimatorController = overrideController;
            
			currentAnimationData.stateName = "Default";
            SetCurrentClipPosition(currentNormalizedTime);

            animator.Play("Default", 0, (float)normalizedTime);
            animator.CrossFade(currentState, (float)(blendingTime / animSpeed), 0, (float)normalizedTime);
        }

        // Update Previous Mirror
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (info.IsName("Default")) {
            if (animator.GetBool("Mirror") != prevMirror) {
                animator.SetBool("Mirror", prevMirror);
            }
        }
        animator.Update(0);
        deltaDisplacement = new Vector3();

        targetAnimationData.timesPlayed = 0;
        targetAnimationData.secondsPlayed = (normalizedTime * targetAnimationData.length) / animSpeed;
        targetAnimationData.ticksPlayed = targetAnimationData.secondsPlayed * UFE.fps;
        targetAnimationData.framesPlayed = (int)targetAnimationData.ticksPlayed;
        targetAnimationData.realFramesPlayed = normalizedTime * targetAnimationData.length * UFE.fps;
        targetAnimationData.normalizedTime = normalizedTime;
        targetAnimationData.speed = targetAnimationData.originalSpeed;

        if (overrideRootMotion) animator.applyRootMotion = targetAnimationData.applyRootMotion;
        SetSpeed(targetAnimationData.originalSpeed);
		
        if (currentAnimationData != null) {
            currentAnimationData.speed = currentAnimationData.originalSpeed;
            currentAnimationData.normalizedSpeed = 1;
            currentAnimationData.timesPlayed = 0;
        }

		currentAnimationData = targetAnimationData;
        currentAnimationData.stateName = currentState;

		if (MecanimControl.OnAnimationBegin != null) MecanimControl.OnAnimationBegin(currentAnimationData);
	}
	
	public bool IsPlaying(string clipName){
		return IsPlaying(GetAnimationData(clipName));
	}
	
	public bool IsPlaying(string clipName, float weight){
		return IsPlaying(GetAnimationData(clipName), weight);
	}
	
	public bool IsPlaying(AnimationClip clip){
		return IsPlaying(GetAnimationData(clip));
	}
	
	public bool IsPlaying(AnimationClip clip, float weight){
		return IsPlaying(GetAnimationData(clip), weight);
	}
	
	public bool IsPlaying(MecanimAnimationData animData, float weight = 1){
		if (animData == null) return false;
		if (currentAnimationData == null) return false;
		if (currentAnimationData == animData && animData.wrapMode == WrapMode.Once && animData.timesPlayed > 0) return false;
        if (currentAnimationData == animData && animData.wrapMode == WrapMode.Clamp && animData.timesPlayed > 0) return false;
		if (currentAnimationData == animData && animData.wrapMode == WrapMode.ClampForever) return true;
		if (currentAnimationData == animData) return true;

		AnimatorClipInfo[] animationInfoArray = animator.GetCurrentAnimatorClipInfo(0);
		foreach (AnimatorClipInfo animationInfo in animationInfoArray){
			if (animData.clip == animationInfo.clip && animationInfo.weight >= weight) return true;
		}
		return false;
	}

    public int GetTimesPlayed(string clipName)
    {
        if (GetAnimationData(clipName) == null) return 0;
        return GetAnimationData(clipName).timesPlayed;
    }
	
	public string GetCurrentClipName(){
		return currentAnimationData.clipName;
	}
	
	public MecanimAnimationData GetCurrentAnimationData(){
		return currentAnimationData;
	}
	
	public int GetCurrentClipPlayCount(){
		return currentAnimationData.timesPlayed;
	}
	
	public Fix64 GetCurrentClipTime(){
        return currentAnimationData.secondsPlayed;
    }

	public int GetCurrentClipFrame(bool bakeSpeed){
        if (bakeSpeed) return (int)currentAnimationData.realFramesPlayed;
        return (int)currentAnimationData.framesPlayed;
    }

    public Fix64 GetCurrentClipNormalizedTime() {
        return currentAnimationData.normalizedTime;
    }

    public Fix64 GetCurrentClipLength() {
		return currentAnimationData.length;
	}

    public Vector3 GetDeltaDisplacement() {
        return deltaDisplacement;
    }

    public Vector3 GetDeltaPosition()
    {
        if (UFE.isConnected || (UFE.config != null && UFE.config.debugOptions.emulateNetwork && UFE.config.networkOptions.disableRootMotion)) 
            return new Vector3();

        return animator.deltaPosition;
    }

    public void ApplyBuiltinRootMotion() {
        animator.ApplyBuiltinRootMotion();
    }

    public void SetCurrentClipPosition(Fix64 normalizedTime){
		SetCurrentClipPosition(normalizedTime, false);
	}

    public void SetCurrentClipPosition(Fix64 normalizedTime, bool pause) {
        if (normalizedTime > 1) normalizedTime = 1;
        if (normalizedTime < 0) normalizedTime = 0;
        currentAnimationData.secondsPlayed = normalizedTime * currentAnimationData.length;
        currentAnimationData.ticksPlayed = currentAnimationData.secondsPlayed * UFE.config.fps;
        currentAnimationData.framesPlayed = (int)currentAnimationData.ticksPlayed;
        currentAnimationData.realFramesPlayed = normalizedTime * currentAnimationData.length * UFE.config.fps;
        currentAnimationData.normalizedTime = normalizedTime;

        animator.Play(currentAnimationData.stateName, 0, (float)normalizedTime);
        animator.Update(0);

        if (pause) Pause();
    }

    public Fix64 GetCurrentClipPosition() {
        if (currentAnimationData == null) return 0;
		return currentAnimationData.secondsPlayed/currentAnimationData.length;
	}
	
	public void Stop(){
		Play(defaultAnimation.clip, defaultTransitionDuration, 0, currentMirror);
	}

    public void Pause() {
        SetSpeed(0);
	}

    public void SetSpeed(AnimationClip clip, Fix64 speed) {
        SetSpeed(GetAnimationData(clip), speed);
    }

    public void SetSpeed(string clipName, Fix64 speed) {
        SetSpeed(GetAnimationData(clipName), speed);
    }

    public void SetSpeed(MecanimAnimationData animData, Fix64 speed) {
        if (animData != null) {
            animData.normalizedSpeed = speed / animData.originalSpeed;

            animData.speed = speed;
            if (IsPlaying(animData)) SetSpeed(speed);
        }
    }

    public void SetSpeed(Fix64 speed) {
        if (currentAnimationData != null) currentAnimationData.normalizedSpeed = speed / currentAnimationData.originalSpeed;
        animator.speed = Mathf.Abs((float)speed);
        currentSpeed = speed;
    }

    public void SetNormalizedSpeed(AnimationClip clip, Fix64 normalizedSpeed) {
        SetNormalizedSpeed(GetAnimationData(clip), normalizedSpeed);
    }

    public void SetNormalizedSpeed(string clipName, Fix64 normalizedSpeed) {
        SetNormalizedSpeed(GetAnimationData(clipName), normalizedSpeed);
    }

    public void SetNormalizedSpeed(MecanimAnimationData animData, Fix64 normalizedSpeed) {
        if (animData == null) return;
        animData.normalizedSpeed = normalizedSpeed;
        animData.speed = animData.speed * animData.normalizedSpeed;
        if (IsPlaying(animData)) SetSpeed(animData.speed);
    }
	
	public void RestoreSpeed(){
        SetNormalizedSpeed(currentAnimationData, 1);
	}
	
	public void Rewind(){
		SetSpeed(-currentAnimationData.speed);
	}

	public void SetWrapMode(WrapMode wrapMode){
		defaultWrapMode = wrapMode;
	}
	
	public void SetWrapMode(MecanimAnimationData animationData, WrapMode wrapMode){
		animationData.wrapMode = wrapMode;
		animationData.clip.wrapMode = wrapMode;
	}

	public void SetWrapMode(AnimationClip clip, WrapMode wrapMode){
		MecanimAnimationData animData = GetAnimationData(clip);
		animData.wrapMode = wrapMode;
		animData.clip.wrapMode = wrapMode;
	}

	public void SetWrapMode(string clipName, WrapMode wrapMode){
		MecanimAnimationData animData = GetAnimationData(clipName);
		animData.wrapMode = wrapMode;
		animData.clip.wrapMode = wrapMode;
	}

    public Fix64 GetSpeed(AnimationClip clip) {
        return GetSpeed(GetAnimationData(clip));
	}

    public Fix64 GetSpeed(string clipName) {
        return GetSpeed(GetAnimationData(clipName));
	}

    public Fix64 GetOriginalSpeed(string clipName) {
        return GetAnimationData(clipName).originalSpeed;
    }

    public Fix64 GetSpeed(MecanimAnimationData animData) {
        return animData.speed;
    }

    public Fix64 GetSpeed() {
		return currentSpeed;
	}
    
    public Fix64 GetNormalizedSpeed() {
        return currentAnimationData.normalizedSpeed;
    }

    public Fix64 GetNormalizedSpeed(AnimationClip clip) {
        return GetNormalizedSpeed(GetAnimationData(clip));
    }

    public Fix64 GetNormalizedSpeed(string clipName) {
        return GetNormalizedSpeed(GetAnimationData(clipName));
    }

    public Fix64 GetNormalizedSpeed(MecanimAnimationData animData) {
        return animData.normalizedSpeed;
    }

    public void OverrideCurrentWrapMode(WrapMode wrap)
    {
        currentAnimationData.wrapMode = wrap;
    }

    public bool GetMirror(){
		return currentMirror;
	}

	public void SetMirror(bool toggle){
		SetMirror(toggle, 0, false);
	}

    public void SetMirror(bool toggle, Fix64 blendingTime) {
		SetMirror(toggle, blendingTime, false);
	}

    public void SetMirror(bool toggle, Fix64 blendingTime, bool forceMirror) {
		if (currentMirror == toggle && !forceMirror) return;
		
		if (blendingTime == 0) blendingTime = defaultTransitionDuration;
		_playAnimation(currentAnimationData, blendingTime, GetCurrentClipPosition(), toggle);
	}
}
