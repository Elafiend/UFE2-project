#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FPLibrary;
using UFE3D;

[System.Serializable]
public class MapRecorder : MonoBehaviour {
    
    [SerializeField]
    public UFE3D.CharacterInfo characterInfo;
    public bool searchResource = false;
    public bool bakeSpeedValues = false;
    public bool bakeGameSpeed = false;

    private HitBoxesScript hitBoxesScript;
    private MecanimControl mecanimControl;
    private LegacyControl legacyControl;
    private List<MoveSetData> loadedMoveSets = new List<MoveSetData>();

    private Animator mAnimator;
    private Animation lAnimator;
    private AnimationClip defaultClip;
    private GameObject character;
    private BasicMoveInfo currentBasicMove;
    private MoveInfo currentMove;
    private int currentStanceNum;
    private int currentClipNum;
    private int currentMoveNum;
    private int currentFrame;
    private int totalFrames;
    private bool recording = false;
    private bool doOnce = false;

    // Dropdown
    private Vector2 scrollViewVector = Vector2.zero;
    public static List<string> scrollList = new List<string>();
    int indexNumber = 0;
    bool showDropDown = false;

    void Awake()
    {
        if (characterInfo.characterPrefabStorage == StorageMode.Prefab) {
            character = Instantiate(characterInfo.characterPrefab);
        } else {
            character = GameObject.Instantiate(Resources.Load<GameObject>(characterInfo.prefabResourcePath));
        }
        character.transform.position = new Vector3(0, 0, 0);

        if (searchResource)
        {
            foreach (string path in characterInfo.stanceResourcePath)
            {
                loadedMoveSets.Add(Resources.Load<StanceInfo>(path).ConvertData());
            }
        }
        else
        {
            foreach (MoveSetData moveSetData in characterInfo.moves)
            {
                loadedMoveSets.Add(moveSetData);
            }
        }

        if (characterInfo.animationType == AnimationType.Legacy) {
            lAnimator = character.GetComponent<Animation>();
            if (lAnimator == null) lAnimator = character.AddComponent<Animation>();
            legacyControl = character.AddComponent<LegacyControl>();
            legacyControl.AddClip(loadedMoveSets[0].basicMoves.idle.animMap[0].clip, "default");
            legacyControl.overrideAnimatorUpdate = true;
        } else {
            mAnimator = character.GetComponent<Animator>();
            if (mAnimator == null) mAnimator = character.AddComponent<Animator>();
            mecanimControl = character.AddComponent<MecanimControl>();
            mecanimControl.overrideAnimatorUpdate = true;
            mecanimControl.normalizeFrames = false;
            mAnimator.applyRootMotion = true;
            mAnimator.avatar = characterInfo.avatar;
        }
        hitBoxesScript = character.GetComponent<HitBoxesScript>();
        hitBoxesScript.UpdateRenderer();

        UFE.timeScale = bakeGameSpeed? UFE.config._gameSpeed : 1;

        Camera.main.transform.position = new Vector3(0, 4, -40);
    }

    void FixedUpdate() {
        if (recording)
        {
            UFE.fixedDeltaTime = 1 / (Fix64)UFE.fps;

            if (characterInfo.animationType == AnimationType.Legacy) {
                legacyControl.DoFixedUpdate();
            }
            else {
                mecanimControl.DoFixedUpdate();
            }
            MapHitBoxes();
        }
        else
        {
            UFE.fixedDeltaTime = 0;
        }
    }

    private void OnGUI() {
        scrollList.Clear();
        foreach (MoveInfo move in loadedMoveSets[currentStanceNum].attackMoves)
        {
            scrollList.Add(move.moveName);
        }

        GUI.Box(new Rect(10, 10, 220, 190), "Animation Map Recorder");
        GUI.BeginGroup(new Rect(20, 30, 200, 170));
        {
            GUILayout.Label("Selected Move Set: " + (currentStanceNum + 1));

            if (recording)
            {
                if (GUILayout.Button("Stop Recording")) recording = false;
            }
            else
            {
                string[] selStrings = new string[loadedMoveSets.Count];
                for (int i = 0; i < loadedMoveSets.Count; i++)
                {
                    selStrings[i] = "Stance " + (i + 1);
                }
                currentStanceNum = GUILayout.SelectionGrid(currentStanceNum, selStrings, 3);

                if (GUILayout.Button("Record All Basic Moves"))
                {
                    currentBasicMove = loadedMoveSets[currentStanceNum].basicMoves.idle;
                    currentMove = null;
                    currentClipNum = 0;
                    currentFrame = 0;
                    totalFrames = 0;
                    recording = true;
                    doOnce = false;
                }

                if (GUILayout.Button("Record All Special Moves"))
                {
                    currentBasicMove = null;
                    currentMove = loadedMoveSets[currentStanceNum].attackMoves[0];
                    currentMoveNum = 0;
                    currentFrame = 0;
                    totalFrames = 0;
                    recording = true;
                    doOnce = false;
                }

                GUILayout.Space(10);

                if (GUILayout.Button("Special Move List"))
                {
                    if (!showDropDown && scrollList.Count > 0)
                    {
                        showDropDown = true;
                    }
                    else
                    {
                        showDropDown = false;
                    }
                }

                if (showDropDown)
                {
                    scrollViewVector = GUILayout.BeginScrollView(scrollViewVector, GUILayout.Width(200), GUILayout.Height(160));

                    for (int index = 0; index < scrollList.Count; index++)
                    {

                        if (GUILayout.Button(scrollList[index]))
                        {
                            showDropDown = false;
                            indexNumber = index;
                        }

                    }

                    GUI.EndScrollView();
                }
                else
                {
                    if (GUILayout.Button("Record " + scrollList[indexNumber]))
                    {
                        currentBasicMove = null;
                        currentMove = loadedMoveSets[currentStanceNum].attackMoves[indexNumber];
                        currentMoveNum = indexNumber;
                        currentFrame = 0;
                        totalFrames = 0;
                        recording = true;
                        doOnce = true;
                    }
                }
            }
        }
        GUI.EndGroup();
    }

    public void MapHitBoxes() {
        MoveSetData moveSetData = loadedMoveSets[currentStanceNum];

        bool finished = false;
        if (currentMove != null)
        {
            currentMove = MapSpecialMove(currentMove, ref finished);

            if (finished)
            {
                if (doOnce)
                {
                    doOnce = false;
                    recording = false;
                }
                else if (moveSetData.cinematicIntro != null && currentMove.name == moveSetData.cinematicIntro.name)
                {
                    if (moveSetData.cinematicOutro != null)
                    {
                        currentMove = MapSpecialMove(moveSetData.cinematicOutro, ref finished);
                    }
                    else
                    {
                        recording = false;
                    }
                }
                else if (moveSetData.cinematicOutro != null && currentMove.name == moveSetData.cinematicOutro.name)
                {
                    recording = false;
                }
                else
                {

                    moveSetData.attackMoves[currentMoveNum] = currentMove;
                    currentMoveNum++;
                    if (currentMoveNum == moveSetData.attackMoves.Length)
                    {
                        if (moveSetData.cinematicIntro == null && moveSetData.cinematicOutro == null)
                        {
                            recording = false;
                        }
                        else if (moveSetData.cinematicIntro != null)
                        {
                            currentMove = MapSpecialMove(moveSetData.cinematicIntro, ref finished);
                        }
                        else
                        {
                            currentMove = MapSpecialMove(moveSetData.cinematicOutro, ref finished);
                        }
                    }
                    else
                    {
                        currentMove = moveSetData.attackMoves[currentMoveNum];
                    }
                }
            }

        }
        else
        {
            currentBasicMove = MapBasicMove(currentBasicMove, ref finished);
            if (finished)
            {
                if (currentBasicMove == moveSetData.basicMoves.idle)
                {
                    currentBasicMove = moveSetData.basicMoves.moveForward;
                }
                else if (currentBasicMove == moveSetData.basicMoves.moveForward)
                {
                    currentBasicMove = moveSetData.basicMoves.moveBack;
                }
                else if (currentBasicMove == moveSetData.basicMoves.moveBack)
                {
                    currentBasicMove = moveSetData.basicMoves.moveSideways;
                }
                else if (currentBasicMove == moveSetData.basicMoves.moveSideways)
                {
                    currentBasicMove = moveSetData.basicMoves.crouching;
                }
                else if (currentBasicMove == moveSetData.basicMoves.crouching)
                {
                    currentBasicMove = moveSetData.basicMoves.takeOff;

                }
                else if (currentBasicMove == moveSetData.basicMoves.takeOff)
                {
                    currentBasicMove = moveSetData.basicMoves.jumpStraight;
                }
                else if (currentBasicMove == moveSetData.basicMoves.jumpStraight)
                {
                    currentBasicMove = moveSetData.basicMoves.jumpBack;
                }
                else if (currentBasicMove == moveSetData.basicMoves.jumpBack)
                {
                    currentBasicMove = moveSetData.basicMoves.jumpForward;
                }
                else if (currentBasicMove == moveSetData.basicMoves.jumpForward)
                {
                    currentBasicMove = moveSetData.basicMoves.fallStraight;
                }
                else if (currentBasicMove == moveSetData.basicMoves.fallStraight)
                {
                    currentBasicMove = moveSetData.basicMoves.fallBack;
                }
                else if (currentBasicMove == moveSetData.basicMoves.fallBack)
                {
                    currentBasicMove = moveSetData.basicMoves.fallForward;
                }
                else if (currentBasicMove == moveSetData.basicMoves.fallForward)
                {
                    currentBasicMove = moveSetData.basicMoves.landing;
                }
                else if (currentBasicMove == moveSetData.basicMoves.landing)
                {
                    currentBasicMove = moveSetData.basicMoves.blockingHighPose;

                }
                else if (currentBasicMove == moveSetData.basicMoves.blockingHighPose)
                {
                    currentBasicMove = moveSetData.basicMoves.blockingHighHit;
                }
                else if (currentBasicMove == moveSetData.basicMoves.blockingHighHit)
                {
                    currentBasicMove = moveSetData.basicMoves.blockingLowHit;
                }
                else if (currentBasicMove == moveSetData.basicMoves.blockingLowHit)
                {
                    currentBasicMove = moveSetData.basicMoves.blockingCrouchingPose;
                }
                else if (currentBasicMove == moveSetData.basicMoves.blockingCrouchingPose)
                {
                    currentBasicMove = moveSetData.basicMoves.blockingCrouchingHit;
                }
                else if (currentBasicMove == moveSetData.basicMoves.blockingCrouchingHit)
                {
                    currentBasicMove = moveSetData.basicMoves.blockingAirPose;
                }
                else if (currentBasicMove == moveSetData.basicMoves.blockingAirPose)
                {
                    currentBasicMove = moveSetData.basicMoves.blockingAirHit;
                }
                else if (currentBasicMove == moveSetData.basicMoves.blockingAirHit)
                {
                    currentBasicMove = moveSetData.basicMoves.parryHigh;

                }
                else if (currentBasicMove == moveSetData.basicMoves.parryHigh)
                {
                    currentBasicMove = moveSetData.basicMoves.parryLow;
                }
                else if (currentBasicMove == moveSetData.basicMoves.parryLow)
                {
                    currentBasicMove = moveSetData.basicMoves.parryCrouching;
                }
                else if (currentBasicMove == moveSetData.basicMoves.parryCrouching)
                {
                    currentBasicMove = moveSetData.basicMoves.parryAir;
                }
                else if (currentBasicMove == moveSetData.basicMoves.parryAir)
                {
                    currentBasicMove = moveSetData.basicMoves.getHitHigh;

                }
                else if (currentBasicMove == moveSetData.basicMoves.getHitHigh)
                {
                    currentBasicMove = moveSetData.basicMoves.getHitLow;
                }
                else if (currentBasicMove == moveSetData.basicMoves.getHitLow)
                {
                    currentBasicMove = moveSetData.basicMoves.getHitCrouching;
                }
                else if (currentBasicMove == moveSetData.basicMoves.getHitCrouching)
                {
                    currentBasicMove = moveSetData.basicMoves.getHitAir;
                }
                else if (currentBasicMove == moveSetData.basicMoves.getHitAir)
                {
                    currentBasicMove = moveSetData.basicMoves.getHitKnockBack;
                }
                else if (currentBasicMove == moveSetData.basicMoves.getHitKnockBack)
                {
                    currentBasicMove = moveSetData.basicMoves.getHitHighKnockdown;
                }
                else if (currentBasicMove == moveSetData.basicMoves.getHitHighKnockdown)
                {
                    currentBasicMove = moveSetData.basicMoves.getHitMidKnockdown;
                }
                else if (currentBasicMove == moveSetData.basicMoves.getHitMidKnockdown)
                {
                    currentBasicMove = moveSetData.basicMoves.getHitSweep;
                }
                else if (currentBasicMove == moveSetData.basicMoves.getHitSweep)
                {
                    currentBasicMove = moveSetData.basicMoves.getHitCrumple;
                }
                else if (currentBasicMove == moveSetData.basicMoves.getHitCrumple)
                {
                    currentBasicMove = moveSetData.basicMoves.fallDown;
                }
                else if (currentBasicMove == moveSetData.basicMoves.fallDown)
                {
                    currentBasicMove = moveSetData.basicMoves.airRecovery;
                }
                else if (currentBasicMove == moveSetData.basicMoves.airRecovery)
                {
                    currentBasicMove = moveSetData.basicMoves.groundBounce;
                }
                else if (currentBasicMove == moveSetData.basicMoves.groundBounce)
                {
                    currentBasicMove = moveSetData.basicMoves.standingWallBounce;
                }
                else if (currentBasicMove == moveSetData.basicMoves.standingWallBounce)
                {
                    currentBasicMove = moveSetData.basicMoves.standingWallBounceKnockdown;
                }
                else if (currentBasicMove == moveSetData.basicMoves.standingWallBounceKnockdown)
                {
                    currentBasicMove = moveSetData.basicMoves.airWallBounce;
                }
                else if (currentBasicMove == moveSetData.basicMoves.airWallBounce)
                {
                    currentBasicMove = moveSetData.basicMoves.fallingFromGroundBounce;
                }
                else if (currentBasicMove == moveSetData.basicMoves.fallingFromGroundBounce)
                {
                    currentBasicMove = moveSetData.basicMoves.fallingFromAirHit;
                }
                else if (currentBasicMove == moveSetData.basicMoves.fallingFromAirHit)
                {
                    currentBasicMove = moveSetData.basicMoves.standUp;
                }
                else if (currentBasicMove == moveSetData.basicMoves.standUp)
                {
                    currentBasicMove = moveSetData.basicMoves.standUpFromAirHit;
                }
                else if (currentBasicMove == moveSetData.basicMoves.standUpFromAirHit)
                {
                    currentBasicMove = moveSetData.basicMoves.standUpFromKnockBack;
                }
                else if (currentBasicMove == moveSetData.basicMoves.standUpFromKnockBack)
                {
                    currentBasicMove = moveSetData.basicMoves.standUpFromStandingHighHit;
                }
                else if (currentBasicMove == moveSetData.basicMoves.standUpFromStandingHighHit)
                {
                    currentBasicMove = moveSetData.basicMoves.standUpFromStandingMidHit;
                }
                else if (currentBasicMove == moveSetData.basicMoves.standUpFromStandingMidHit)
                {
                    currentBasicMove = moveSetData.basicMoves.standUpFromCrumple;
                }
                else if (currentBasicMove == moveSetData.basicMoves.standUpFromCrumple)
                {
                    currentBasicMove = moveSetData.basicMoves.standUpFromSweep;
                }
                else if (currentBasicMove == moveSetData.basicMoves.standUpFromSweep)
                {
                    currentBasicMove = moveSetData.basicMoves.standUpFromStandingWallBounce;
                }
                else if (currentBasicMove == moveSetData.basicMoves.standUpFromStandingWallBounce)
                {
                    currentBasicMove = moveSetData.basicMoves.standUpFromAirWallBounce;
                }
                else if (currentBasicMove == moveSetData.basicMoves.standUpFromAirWallBounce)
                {
                    currentBasicMove = moveSetData.basicMoves.standUpFromGroundBounce;
                }
                else if (currentBasicMove == moveSetData.basicMoves.standUpFromGroundBounce)
                {
                    recording = false;
                }
            }
        }

        if (!recording) {
            if (searchResource)
            {
                StanceInfo newStanceInfo = moveSetData.ConvertData();
                StanceInfo reference = Resources.Load<StanceInfo>(characterInfo.stanceResourcePath[currentStanceNum]);
                string path = AssetDatabase.GetAssetPath(reference);
                if (path == "")
                {
                    path = "Assets";
                }
                else if (Path.GetExtension(path) != "")
                {
                    path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(reference)), "");
                }
                string assetPathAndName = path + reference.name + ".asset";

                if (!AssetDatabase.Contains(newStanceInfo)) AssetDatabase.CreateAsset(newStanceInfo, assetPathAndName);
                AssetDatabase.SaveAssets();
            }
            else
            {
                characterInfo.moves[currentStanceNum] = moveSetData;
            }
            EditorUtility.SetDirty(characterInfo);

            Debug.Log("Maps Created");
        }
    }

    private SerializedAnimationMap AnimationSetup(SerializedAnimationMap animMap, Fix64 speed)
    {
        animMap.animationMaps = new AnimationMap[0];

        if (characterInfo.animationType == AnimationType.Legacy)
        {
            legacyControl.RemoveAllClips();
            legacyControl.AddClip(animMap.clip, animMap.clip.name, speed, WrapMode.Clamp);
            legacyControl.Play(animMap.clip.name, 0, 0);
        }
        else
        {
            mecanimControl.SetDefaultClip(animMap.clip, animMap.clip.name, speed, WrapMode.Clamp, false);
            mecanimControl.currentAnimationData = mecanimControl.defaultAnimation;
            mecanimControl.currentAnimationData.stateName = "State1";
            mecanimControl.currentAnimationData.length = animMap.clip.length;
            mecanimControl.Play(mecanimControl.defaultAnimation, 0, 0, false);
        }

        animMap.length = animMap.clip.length;
        totalFrames = (int)FPMath.Round((animMap.length / speed) * UFE.fps);

        return animMap;
    }

    private MoveInfo MapSpecialMove(MoveInfo moveInfo, ref bool over)
    {
        bool finished = false;
        if (moveInfo.animMap.clip != null)
        {
            if (currentFrame == 0) {
                Fix64 speed = 1;
                if (bakeSpeedValues && moveInfo.fixedSpeed) {
                    speed = FPMath.Abs(moveInfo._animationSpeed);
                }else if (bakeSpeedValues && !moveInfo.fixedSpeed) {
                    Debug.LogWarning("Speed keyframe is currently not supported on the Animation Recorder.");
                }
                moveInfo.animMap = AnimationSetup(moveInfo.animMap, speed);
                moveInfo.animMap.bakeSpeed = bakeSpeedValues;
            }

            AnimationMap[] animationMaps = moveInfo.animMap.animationMaps;
            moveInfo.animMap.animationMaps = MapFrame(animationMaps, moveInfo.animMap.clip, ref finished);

            if (finished)
            {
                character.transform.position = Vector3.zero;
                character.transform.localPosition = Vector3.zero;
                if (characterInfo.animationType != AnimationType.Legacy)
                {
                    mAnimator.rootPosition = Vector3.zero;
                    mAnimator.bodyPosition = Vector3.zero;
                    mAnimator.WriteDefaultValues();
                    mAnimator.gameObject.SetActive(false);
                    mAnimator.gameObject.SetActive(true);
                }
                Debug.Log("Saved");
                over = true;
            }
        }
        else
        {
            over = true;
        }

        EditorUtility.SetDirty(moveInfo);
        return moveInfo;
    }

    private BasicMoveInfo MapBasicMove(BasicMoveInfo basicMove, ref bool over)
    {
        if (currentClipNum > 8)
        {
            currentClipNum = 0;
            over = true;
        }
        else
        {
            bool finished = false;
            if (currentClipNum < basicMove.animMap.Length  && basicMove.animMap[currentClipNum].clip != null)
            {
                if (currentFrame == 0) {
                    Fix64 speed = bakeSpeedValues && !basicMove.autoSpeed ? FPMath.Abs(basicMove._animationSpeed) : 1;
                    basicMove.animMap[currentClipNum] = AnimationSetup(basicMove.animMap[currentClipNum], speed);
                    basicMove.animMap[currentClipNum].bakeSpeed = bakeSpeedValues;
                }

                AnimationMap[] animationMaps = basicMove.animMap[currentClipNum].animationMaps;
                basicMove.animMap[currentClipNum].animationMaps = MapFrame(animationMaps, basicMove.animMap[currentClipNum].clip, ref finished);

                if (finished)
                {
                    character.transform.position = new Vector3(0, 0, 0);
                    Debug.Log("Saved");
                    currentClipNum++;
                }
            }
            else
            {
                currentClipNum++;
            }

            over = false;
        }

        return basicMove;
    }

    private AnimationMap[] MapFrame(AnimationMap[] animationMaps, AnimationClip animationClip, ref bool finished)
    {
        List<AnimationMap> _animationMaps = new List<AnimationMap>(animationMaps);

        Debug.Log("Mapping " + animationClip.name + " (" + currentFrame + ")");

        AnimationMap animationMap = new AnimationMap();
        animationMap.frame = currentFrame;
        animationMap.hitBoxMaps = hitBoxesScript.GetAnimationMaps();

        if (characterInfo.animationType == AnimationType.Legacy) {
            animationMap.deltaDisplacement = FPVector.ToFPVector(legacyControl.GetDeltaPosition());
        } else {
            animationMap.deltaDisplacement = FPVector.ToFPVector(mecanimControl.GetDeltaPosition());
        }

        _animationMaps.Add(animationMap);

        // preview
        hitBoxesScript.animationMaps = _animationMaps.ToArray();
        hitBoxesScript.UpdateMap(currentFrame);
        
        currentFrame++;
        if (currentFrame >= totalFrames)
        {
            currentFrame = 0;
            finished = true;
        }

        return _animationMaps.ToArray();
    }
}
#endif