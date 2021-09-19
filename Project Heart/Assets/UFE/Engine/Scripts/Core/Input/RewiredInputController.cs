using System;
using System.Collections.Generic;
using UnityEngine;

namespace UFE3D
{
    ///--------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This class reads the player input using Rewired:
    /// https://www.assetstore.unity3d.com/en/#!/content/21676
    /// 
    /// If Rewired is not available, it will use Unity Input instead.
    /// </summary>
    ///--------------------------------------------------------------------------------------------------------------------
    public sealed class RewiredInputController : AbstractInputController
    {

        #region Static Members

        private static IInputSource _inputSource;
        private static ITouchInputUI _touchInputUI;
        private static IInputConfiguration _inputConfiguration;

        public static IInputSource inputSource
        {
            get
            {
                return _inputSource ?? (_inputSource = new UnityInputSource());
            }
            set
            {
                _inputSource = value;
            }
        }

        public static ITouchInputUI touchInputUI
        {
            get
            {
                return _touchInputUI;
            }
            set
            {
                _touchInputUI = value;
            }
        }

        public static IInputConfiguration inputConfiguration
        {
            get
            {
                return _inputConfiguration;
            }
            set
            {
                _inputConfiguration = value;
            }
        }

        #endregion

        #region Instance Members

        public int rewiredPlayerId;

        private bool prevBattleGUI;
        private bool prevGamePaused;

        #region Overriden Methods 

        public override void Initialize(IEnumerable<InputReferences> inputs)
        {
            base.Initialize(inputs);
        }

        public override void DoUpdate()
        {
            base.DoUpdate();

            bool battleGUI = (UFE.battleGUI != null);
            bool gamePaused = UFE.isPaused();

            if (touchInputUI != null)
            {
                if (battleGUI != this.prevBattleGUI)
                {
                    touchInputUI.showTouchControls = battleGUI && !gamePaused;
                }
                else if (gamePaused != this.prevGamePaused)
                {
                    if (battleGUI)
                    {
                        touchInputUI.showTouchControls = !gamePaused;
                    }
                }
            }

            this.prevBattleGUI = battleGUI;
            this.prevGamePaused = gamePaused;
        }

        public override InputEvents ReadInput(InputReferences inputReference)
        {
            if (inputReference != null)
            {
                string buttonName = inputReference.inputButtonName;
                string axisName = inputReference.joystickAxisName;

                if (
                    inputReference.inputType == InputType.HorizontalAxis ||
                    inputReference.inputType == InputType.VerticalAxis
                )
                {
                    return new InputEvents(
                        inputSource.GetAxisRaw(rewiredPlayerId, axisName)
                    );
                }
                else
                {
                    return new InputEvents(
                        inputSource.GetButton(rewiredPlayerId, buttonName)
                    );
                }
            }
            else
            {
                return InputEvents.Default;
            }
        }

        #endregion

        #endregion

        #region Classes / Interfaces

        public interface IInputSource
        {
            bool GetButton(int playerId, string name);
            float GetAxis(int playerId, string name);
            float GetAxisRaw(int playerId, string name);
        }

        public interface ITouchInputUI
        {
            bool showTouchControls { get; set; }
        }

        public interface IInputConfiguration
        {
            bool showInputConfigurationUI { get; set; }
            void ShowInputConfigurationUI(Action closedCallback);
        }

        private class UnityInputSource : IInputSource
        {
            public float GetAxis(int playerId, string name)
            {
                return Input.GetAxis(name);
            }

            public float GetAxisRaw(int playerId, string name)
            {
                return Input.GetAxisRaw(name);
            }

            public bool GetButton(int playerId, string name)
            {
                return Input.GetButton(name);
            }
        }

        #endregion
    }
}