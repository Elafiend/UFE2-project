using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UFE3D;

public class ChallengeMode : MonoBehaviour {

    private List<ActionSequence> challengeActions;
    public ControlsScript cScript;
    public int currentChallenge = 0;
    public int currentAction = 0;
    public bool complete;
    public bool moveToNext;
    public bool resetRound;
    public GUIStyle style = new GUIStyle();
    public Font font;

    public void Start() {
        //style.fontSize = 30;
        style.font = (Font)Resources.Load("Robustik");
        style.alignment = TextAnchor.MiddleCenter;
        currentChallenge = UFE.config.selectedChallenge;
        challengeActions = new List<ActionSequence>(UFE.GetChallenge(currentChallenge).actionSequence);

        Run();
	}

    public void Run() {
        UFE.OnMove += this.OnMove;
        UFE.OnBasicMove += this.OnBasicMove;
        UFE.OnButton += this.OnButtonPress;
        complete = false;
        moveToNext = false;
        resetRound = false;
        currentAction = 0;
    }

    public void Stop() {
        UFE.OnMove -= this.OnMove;
        UFE.OnBasicMove -= this.OnBasicMove;
        UFE.OnButton -= this.OnButtonPress;
    }
    
    protected virtual void OnMove(MoveInfo move, ControlsScript player) {
        // Fires when a player successfully executes a move
        // player.playerNum = 1 or 2
        if (player.playerNum == 1
            && !complete
            && !UFE.config.lockInputs
            && UFE.gameMode == GameMode.ChallengeMode
            && challengeActions[currentAction].actionType == ActionType.SpecialMove
            && challengeActions[currentAction].specialMove == move) {
            currentAction++;
            testChallenge();
        } else {
            currentAction = 0;
        }
    }

    protected virtual void OnBasicMove(BasicMoveReference basicMove, ControlsScript player) {
        // Fires when a player successfully executes a move
        // player.playerNum = 1 or 2
        if (player.playerNum == 1
            && !complete
            && !UFE.config.lockInputs
            && UFE.gameMode == GameMode.ChallengeMode
            && challengeActions[currentAction].actionType == ActionType.BasicMove
            && challengeActions[currentAction].basicMove == basicMove) {
            currentAction++;
            testChallenge();
        } else {
            currentAction = 0;
        }
    }

    protected virtual void OnButtonPress(ButtonPress buttonPress, ControlsScript player) {
        // Fires when a player successfully executes a move
        // player.playerNum = 1 or 2
        if (player.playerNum == 1
            && !complete
            && !UFE.config.lockInputs
            && UFE.gameMode == GameMode.ChallengeMode
            && challengeActions[currentAction].actionType == ActionType.ButtonPress
            && challengeActions[currentAction].button == buttonPress) {
            currentAction++;
            testChallenge();
        } else {
            currentAction = 0;
        }
    }

    private void testChallenge() {
        if (currentAction == challengeActions.Count) {
            if (UFE.GetChallenge(currentChallenge).challengeSequence == ChallengeAutoSequence.MoveToNext) {
                moveToNext = true;
                if (UFE.GetChallenge(currentChallenge).resetData) resetRound = true;

                currentChallenge++;
                challengeActions = new List<ActionSequence>(UFE.GetChallenge(currentChallenge).actionSequence);
            } else {
                moveToNext = false;
            }

            complete = true;
        } 
    }

    public void OnGUI() {
        if (UFE.GetChallenge(currentChallenge).description != "" 
            && !complete
            && !UFE.config.lockInputs
            && !UFE.config.lockMovements) {

            if (GUI.Button(new Rect(Screen.width - 120, 50, 70, 30), "Skip")) {
                moveToNext = false;
                complete = true;
                UFE.fluxCapacitor.EndRound();
            }

            GUI.Box(new Rect(0, 150, Screen.width, 40), UFE.GetChallenge(currentChallenge).description, style);
            //GUI.Box(new Rect(0, Screen.height - 60, Screen.width, 40), UFE.GetChallenge(currentChallenge).description);
            /*GUI.BeginGroup(new Rect(0, Screen.height - 100, Screen.width, 100));
            {
                GUILayout.Label(UFE.GetChallenge(currentChallenge).description);
            } GUI.EndGroup();*/
        }
    }
}
