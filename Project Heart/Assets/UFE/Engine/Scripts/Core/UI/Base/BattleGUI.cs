using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UFE3D;

namespace UFE3D
{
    public class BattleGUI : UFEScreen
    {
        #region public class definitions
        [Serializable]
        public class PlayerInfo
        {
            public ControlsScript controlsScript;
            public float targetLife;
            public float totalLife;
            public int wonRounds;
            public bool winner;
        }
        public class InputIconGroup
        {
            public List<InputIcon> inputs;
            public bool isActive;

            public InputIconGroup()
            {
                this.inputs = new List<InputIcon>();
                this.isActive = false;
            }
        }
        public class InputIcon
        {
            public Image image;
            public Sprite sprite;
            public bool isActive;
        }

        #endregion

        #region public instance properties
        public List<InputIconGroup> player1Inputs = new List<InputIconGroup>();
        public List<InputIconGroup> player2Inputs = new List<InputIconGroup>();
        public Texture2D inputTexturePlaceHolder;
        public int inputViewerBuffer = 12;
        #endregion

        #region protected instance properties
        protected PlayerInfo player1 = new PlayerInfo();
        protected PlayerInfo player2 = new PlayerInfo();
        protected bool isRunning;
        protected Sprite spritePlaceHolder;
        #endregion

        //GameObject leftZoneRegular;
        //GameObject rightZoneRegular;
        //GameObject leftZoneMirror;
        //GameObject rightZoneMirror;
        //int trycount;

        #region input viewer methods
        public void SetImagePosition(Image image, int player, int row, int col)
        {
            float x = player == 1 ? 0f : 1f;
            float y = Mathf.Lerp(0.8f, 0.05f, row / 11f);

            image.rectTransform.anchorMin = new Vector2(x, y);
            image.rectTransform.anchorMax = image.rectTransform.anchorMin;
            image.rectTransform.anchoredPosition = Vector2.zero;
            image.rectTransform.offsetMax = Vector2.zero;
            image.rectTransform.offsetMin = Vector2.zero;
            image.rectTransform.sizeDelta = new Vector2(image.preferredWidth * 200, image.preferredHeight * 200);

            if (player == 1)
            {
                image.rectTransform.pivot = new Vector2(0f, 0.5f);
                image.rectTransform.anchoredPosition = new Vector2(image.rectTransform.sizeDelta.x * col, 0f);
            }
            else
            {
                image.rectTransform.pivot = new Vector2(1f, 0.5f);
                image.rectTransform.anchoredPosition = new Vector2(-image.rectTransform.sizeDelta.x * col, 0f);
            }
        }

        public void RefreshInputs(List<InputIconGroup> newPlayerInputIcons, int player)
        {
            List<InputIconGroup> targetPlayerInputIcons = player == 1 ? player1Inputs : player2Inputs;

            for (int i = 0; i < newPlayerInputIcons.Count; i++)
            {
                targetPlayerInputIcons[i].isActive = newPlayerInputIcons[i].isActive;

                for (int k = 0; k < newPlayerInputIcons[i].inputs.Count; k++)
                {
                    targetPlayerInputIcons[i].inputs[k].image.gameObject.SetActive(newPlayerInputIcons[i].inputs[k].isActive);
                    targetPlayerInputIcons[i].inputs[k].image.sprite = newPlayerInputIcons[i].inputs[k].sprite;
                    targetPlayerInputIcons[i].inputs[k].sprite = newPlayerInputIcons[i].inputs[k].sprite;
                    targetPlayerInputIcons[i].inputs[k].isActive = newPlayerInputIcons[i].inputs[k].isActive;

                    SetImagePosition(targetPlayerInputIcons[i].inputs[k].image, player, i, k);
                }
            }
        }

        public void AddViewerInput(List<Sprite> newInputs, int player)
        {
            List<InputIconGroup> playerInputIcons = player == 1 ? player1Inputs : player2Inputs;

            if (playerInputIcons[playerInputIcons.Count - 1].isActive)
            {
                MoveInputsUp(playerInputIcons, player);
                ClearInput(playerInputIcons, playerInputIcons.Count - 1);
            }

            for (int i = 0; i < playerInputIcons.Count; i++)
            {
                if (!playerInputIcons[i].isActive)
                {
                    for (int k = 0; k < newInputs.Count; k++)
                    {
                        playerInputIcons[i].inputs[k].image.gameObject.SetActive(true);
                        playerInputIcons[i].inputs[k].image.sprite = newInputs[k];
                        playerInputIcons[i].inputs[k].sprite = newInputs[k];
                        playerInputIcons[i].inputs[k].isActive = true;

                        SetImagePosition(playerInputIcons[i].inputs[k].image, player, i, k);
                    }
                    playerInputIcons[i].isActive = true;
                    break;
                }
            }
        }

        public void ClearInput(List<InputIconGroup> playerInputIcons, int position)
        {
            for (int i = 0; i < playerInputIcons.Count; i++)
            {
                if (position == i)
                {
                    for (int k = 0; k < playerInputIcons[i].inputs.Count; k++)
                    {
                        playerInputIcons[i].inputs[k].image.gameObject.SetActive(false);
                        playerInputIcons[i].inputs[k].isActive = false;
                    }
                    playerInputIcons[i].isActive = false;
                    break;
                }
            }
        }

        public void MoveInputsUp(List<InputIconGroup> playerInputIcons, int player)
        {
            for (int i = 0; i < playerInputIcons.Count; i++)
            {
                for (int k = 0; k < playerInputIcons[i].inputs.Count; k++)
                {
                    playerInputIcons[i].inputs[k].image.gameObject.SetActive(false);
                    playerInputIcons[i].inputs[k].isActive = false;
                }

                int nextInput = i + 1;
                if (nextInput >= playerInputIcons.Count) break;

                for (int k = 0; k < playerInputIcons[nextInput].inputs.Count; k++)
                {
                    if (playerInputIcons[nextInput].inputs[k].isActive)
                    {
                        playerInputIcons[i].inputs[k].image.gameObject.SetActive(true);
                        playerInputIcons[i].inputs[k].image.sprite = playerInputIcons[nextInput].inputs[k].sprite;
                        playerInputIcons[i].inputs[k].sprite = playerInputIcons[nextInput].inputs[k].sprite;
                        playerInputIcons[i].inputs[k].isActive = true;

                        SetImagePosition(playerInputIcons[i].inputs[k].image, player, i, k);
                    }
                }
            }
        }

        private void UpdateRectTransform(GameObject go)
        {
            (go.transform as RectTransform).anchoredPosition = Vector2.zero;
            (go.transform as RectTransform).anchorMin = new Vector2(0, 0);
            (go.transform as RectTransform).anchorMax = new Vector2(1, 1);
            (go.transform as RectTransform).anchoredPosition = new Vector2(.5f, .5f);
            (go.transform as RectTransform).offsetMax = Vector2.zero;
            (go.transform as RectTransform).offsetMin = Vector2.zero;
            (go.transform as RectTransform).localPosition = Vector3.zero;
            (go.transform as RectTransform).localRotation = Quaternion.identity;
            (go.transform as RectTransform).localScale = Vector3.one;
        }

        public void CreateInputViewer(int player)
        {
            List<InputIconGroup> playerInputIcons = player == 1 ? player1Inputs : player2Inputs;

            GameObject goGrandParent = new GameObject("Player " + player + " Input Viewer", typeof(RectTransform));
            goGrandParent.transform.SetParent(UFE.battleGUI?.transform);
            UpdateRectTransform(goGrandParent);

            for (int i = 0; i < inputViewerBuffer; i++)
            {
                playerInputIcons.Add(new InputIconGroup());
                playerInputIcons[i].inputs = new List<InputIcon>();

                GameObject goParent = new GameObject("Button Press List", typeof(RectTransform));
                goParent.transform.SetParent(goGrandParent.transform);
                UpdateRectTransform(goParent);

                for (int k = 0; k < inputViewerBuffer; k++)
                {
                    GameObject go = new GameObject("Button Icon", typeof(RectTransform));
                    go.transform.SetParent(goParent.transform);
                    UpdateRectTransform(go);

                    Image image = go.AddComponent<Image>();
                    image.sprite = spritePlaceHolder;
                    SetImagePosition(image, player, i, k);

                    go.SetActive(false);
                    playerInputIcons[i].isActive = false;

                    InputIcon inputIcon = new InputIcon();
                    inputIcon.isActive = false;
                    inputIcon.image = image;
                    inputIcon.sprite = image.sprite;

                    playerInputIcons[i].inputs.Add(inputIcon);
                }
            }
        }
        #endregion

        #region public override methods
        public override void DoFixedUpdate(
            IDictionary<InputReferences, InputEvents> player1PreviousInputs,
            IDictionary<InputReferences, InputEvents> player1CurrentInputs,
            IDictionary<InputReferences, InputEvents> player2PreviousInputs,
            IDictionary<InputReferences, InputEvents> player2CurrentInputs
        )
        {
            /*if (leftZoneRegular == null && UFE.controlFreakPrefab != null && trycount < 10) {
                    //leftZoneRegular = GameObject.Find("/CF2 Swipe(Clone)/CF2-Canvas/CF2-Panel/TouchZone-Left-Tap");
                    leftZoneRegular = GameObject.Find("/CF2-Panel/TouchZone-Left-Tap");
                    rightZoneRegular = GameObject.Find("/CF2-Panel/TouchZone-Right-Tap");
                    leftZoneMirror = GameObject.Find("/CF2-Panel/TouchZone-Left-Tap-Mirror");
                    rightZoneMirror = GameObject.Find("/CF2-Panel/TouchZone-Right-Tap-Mirror");
                    Debug.Log(leftZoneRegular);

                    leftZoneRegular.SetActive(true);
                    rightZoneRegular.SetActive(true);
                    leftZoneMirror.SetActive(false);
                    rightZoneMirror.SetActive(false);
                    trycount ++;
                }
            }*/
            base.DoFixedUpdate(player1PreviousInputs, player1CurrentInputs, player2PreviousInputs, player2CurrentInputs);
        }

        public override void OnShow()
        {
            base.OnShow();

            /* Subscribe to UFE events:
            /* Possible Events:
             * OnLifePointsChange(float newLifePoints, UFE3D.CharacterInfo player)
             * OnNewAlert(string alertMessage, UFE3D.CharacterInfo player)
             * OnHit(MoveInfo move, UFE3D.CharacterInfo hitter)
             * OnMove(MoveInfo move, UFE3D.CharacterInfo player)
             * OnRoundEnds(UFE3D.CharacterInfo winner, UFE3D.CharacterInfo loser)
             * OnRoundBegins(int roundNumber)
             * OnGameEnds(UFE3D.CharacterInfo winner, UFE3D.CharacterInfo loser)
             * OnGameBegins(UFE3D.CharacterInfo player1, UFE3D.CharacterInfo player2, StageOptions stage)
             * 
             * usage:
             * UFE.OnMove += YourFunctionHere;
             * .
             * .
             * void YourFunctionHere(T param1, T param2){...}
             * 
             * The following code bellow show more usage examples
             */

            // Global Events
            UFE.OnGameBegin += this.OnGameBegin;
            UFE.OnGameEnds += this.OnGameEnd;
            UFE.OnGamePaused += this.OnGamePaused;
            UFE.OnRoundBegins += this.OnRoundBegin;
            UFE.OnRoundEnds += this.OnRoundEnd;
            UFE.OnLifePointsChange += this.OnLifePointsChange;
            UFE.OnNewAlert += this.OnNewAlert;
            UFE.OnHit += this.OnHit;
            UFE.OnBlock += this.OnBlock;
            UFE.OnParry += this.OnParry;
            UFE.OnMove += this.OnMove;
            UFE.OnBasicMove += this.OnBasicMove;
            UFE.OnButton += this.OnButtonPress;
            UFE.OnTimer += this.OnTimer;
            UFE.OnTimeOver += this.OnTimeOver;
            UFE.OnInput += this.OnInput;

            // Move Events
            UFE.OnBodyVisibilityChange += this.OnBodyVisibilityChange;
            UFE.OnParticleEffects += this.OnParticleEffects;
            UFE.OnSideSwitch += this.OnSideSwitch;

            // Input Viewer Variables
            //texturePlaceHolder = new Texture2D(1, 1);
            spritePlaceHolder = Sprite.Create(inputTexturePlaceHolder,
                            new Rect(0f, 0f, inputTexturePlaceHolder.width, inputTexturePlaceHolder.height),
                            new Vector2(0.5f * inputTexturePlaceHolder.width, 0.5f * inputTexturePlaceHolder.height));


            player1Inputs = new List<InputIconGroup>(inputViewerBuffer);
            player2Inputs = new List<InputIconGroup>(inputViewerBuffer);
            CreateInputViewer(1);
            CreateInputViewer(2);

        }

        public override void OnHide()
        {
            UFE.OnGameBegin -= this.OnGameBegin;
            UFE.OnGameEnds -= this.OnGameEnd;
            UFE.OnGamePaused -= this.OnGamePaused;
            UFE.OnRoundBegins -= this.OnRoundBegin;
            UFE.OnRoundEnds -= this.OnRoundEnd;
            UFE.OnLifePointsChange -= this.OnLifePointsChange;
            UFE.OnNewAlert -= this.OnNewAlert;
            UFE.OnHit -= this.OnHit;
            UFE.OnBlock -= this.OnBlock;
            UFE.OnParry -= this.OnParry;
            UFE.OnMove -= this.OnMove;
            UFE.OnBasicMove -= this.OnBasicMove;
            UFE.OnButton -= this.OnButtonPress;
            UFE.OnTimer -= this.OnTimer;
            UFE.OnTimeOver -= this.OnTimeOver;
            UFE.OnInput -= this.OnInput;

            UFE.OnBodyVisibilityChange -= this.OnBodyVisibilityChange;
            UFE.OnParticleEffects -= this.OnParticleEffects;
            UFE.OnSideSwitch -= this.OnSideSwitch;

            base.OnHide();
        }
        #endregion

        #region protected instance methods
        protected virtual void OnGameBegin(ControlsScript player1, ControlsScript player2, StageOptions stage)
        {
            this.player1.controlsScript = player1;
            this.player1.targetLife = player1.myInfo.lifePoints;
            this.player1.totalLife = player1.myInfo.lifePoints;
            this.player1.wonRounds = 0;

            this.player2.controlsScript = player2;
            this.player2.targetLife = player2.myInfo.lifePoints;
            this.player2.totalLife = player2.myInfo.lifePoints;
            this.player2.wonRounds = 0;

            UFE.PlayMusic(stage.music);
            this.isRunning = true;
        }

        protected virtual void OnGameEnd(ControlsScript winner, ControlsScript loser)
        {
            this.isRunning = false;
            if (winner.playerNum == this.player1.controlsScript.playerNum) this.player1.winner = true;
            if (winner.playerNum == this.player2.controlsScript.playerNum) this.player2.winner = true;

            UFE.DelaySynchronizedAction(this.OpenMenuAfterBattle, UFE.config.roundOptions._showMenuDelay);
        }

        protected void OpenMenuAfterBattle()
        {
            if (UFE.gameMode == GameMode.VersusMode || UFE.gameMode == GameMode.ChallengeMode)
            {
                UFE.StartVersusModeAfterBattleScreen();
            }
            else if (UFE.gameMode == GameMode.NetworkGame)
            {
                UFE.StartOnlineModeAfterBattleScreen();
            }
            else if (UFE.gameMode == GameMode.StoryMode)
            {
                if (this.player1.winner)
                {
                    UFE.WonStoryModeBattle();
                }
                else
                {
                    UFE.StartStoryModeContinueScreen();
                }
            }
            else
            {
                UFE.StartMainMenuScreen();
            }
        }

        protected virtual void OnGamePaused(bool isPaused)
        {

        }

        protected virtual void OnRoundBegin(int roundNumber)
        {

        }

        protected virtual void OnRoundEnd(ControlsScript winner, ControlsScript loser)
        {
            //++this.player1WonRounds;
            //++this.playe21WonRounds;
        }

        protected virtual void OnLifePointsChange(float newFloat, ControlsScript player)
        {
            // You can use this to have your own custom events when a player's life points changes
            // player.playerNum = 1 or 2
        }

        protected virtual void OnNewAlert(string msg, ControlsScript player)
        {
            // You can use this to have your own custom events when a new text alert is fired from the engine
            // player.playerNum = 1 or 2
        }

        protected virtual void OnHit(HitBox strokeHitBox, MoveInfo move, ControlsScript player)
        {
            // player.playerNum = 1 or 2
            // You can use this to have your own custom events when a character gets hit
        }

        protected virtual void OnBlock(HitBox strokeHitBox, MoveInfo move, ControlsScript player)
        {
            // You can use this to have your own custom events when a player blocks.
            // player.playerNum = 1 or 2
            // player = character blocking
        }

        protected virtual void OnParry(HitBox strokeHitBox, MoveInfo move, ControlsScript player)
        {
            // You can use this to have your own custom events when a character parries an attack
            // player.playerNum = 1 or 2
            // player = character parrying
        }

        protected virtual void OnMove(MoveInfo move, ControlsScript player)
        {
            // Fires when a player successfully executes a move
            // player.playerNum = 1 or 2
        }

        protected virtual void OnBasicMove(BasicMoveReference basicMove, ControlsScript player)
        {
            // Fires when a player successfully executes a move
            // player.playerNum = 1 or 2
        }

        protected virtual void OnButtonPress(ButtonPress buttonPress, ControlsScript player)
        {
            // Fires when a player successfully executes a move
            // player.playerNum = 1 or 2
        }

        protected virtual void OnBodyVisibilityChange(MoveInfo move, ControlsScript player, BodyPartVisibilityChange bodyPartVisibilityChange, HitBox hitBox)
        {
            // Fires when a move casts a body part visibility change
            // player.playerNum = 1 or 2
        }

        protected virtual void OnParticleEffects(MoveInfo move, ControlsScript player, MoveParticleEffect particleEffects)
        {
            // Fires when a move casts a particle effect
            // player.playerNum = 1 or 2
        }

        protected virtual void OnSideSwitch(int side, ControlsScript player)
        {
            // Fires when a character switches orientation
            // player.playerNum = 1 or 2
            /*if (player.playerNum == 1) {
                leftZoneRegular.SetActive(false);
                rightZoneRegular.SetActive(false);
                leftZoneMirror.SetActive(false);
                rightZoneMirror.SetActive(false);

                if (side == -1) {
                    leftZoneMirror.SetActive(true);
                    rightZoneMirror.SetActive(true);
                } else {
                    leftZoneRegular.SetActive(true);
                    rightZoneRegular.SetActive(true);
                }
            }*/
        }

        protected virtual void OnTimer(FPLibrary.Fix64 time)
        {

        }

        protected virtual void OnTimeOver()
        {

        }

        protected virtual void OnInput(InputReferences[] inputReferences, int player)
        {

        }
        #endregion
    }
}