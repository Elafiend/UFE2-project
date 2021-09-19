namespace UFE3D
{
    public class OnlineModeAfterBattleScreen : UFEScreen
    {
        #region protected enum definitions
        protected enum Option
        {
            RepeatBattle = 0,
            CharacterSelectionScreen = 1,
            SearchNewMatch = 2,
            MainMenu = 3,
        }
        #endregion

        protected Option mySelection = Option.MainMenu;
        protected Option opSelection = Option.MainMenu;

        #region public instance methods
        public virtual void GoToCharacterSelectionScreen()
        {
            this.TrySelectOption((int)OnlineModeAfterBattleScreen.Option.CharacterSelectionScreen, UFE.GetLocalPlayer(), true);
        }

        public virtual void GoToMainMenu()
        {
            this.TrySelectOption((int)OnlineModeAfterBattleScreen.Option.MainMenu, UFE.GetLocalPlayer(), true);
        }

        public virtual void GoToSearchMatchScreen()
        {
            this.TrySelectOption((int)OnlineModeAfterBattleScreen.Option.SearchNewMatch, UFE.GetLocalPlayer(), true);
        }

        public virtual void RepeatBattle()
        {
            this.TrySelectOption((int)OnlineModeAfterBattleScreen.Option.RepeatBattle, UFE.GetLocalPlayer(), true);
        }

        public virtual void TrySelectOption(int option, int player, bool broadcast)
        {
            if (!broadcast || UFE.disconnecting)
            {
                this.SelectOption(option, player);
            }
            else
            {
                // We don't invoke the SelectOption() method immediately because we are using the frame-delay 
                // algorithm to keep players synchronized, so we can't invoke the SelectOption() method
                // until the other player has received the message with our choice.
                UFE.fluxCapacitor.RequestOptionSelection(player, (sbyte)option);
            }
        }

        public virtual void DisableMenuOption(string buttonName) { }

        public virtual void HighlightMenuOption(string buttonName) { }
        #endregion


        #region public override methods
        public override void SelectOption(int option, int player)
        {
            OnlineModeAfterBattleScreen.Option selectedOption = (OnlineModeAfterBattleScreen.Option)option;
            if (selectedOption == OnlineModeAfterBattleScreen.Option.CharacterSelectionScreen)
            {
                HighlightMenuOption("Button_Character_Selection_Screen");

                if (UFE.GetLocalPlayer() == player)
                {
                    mySelection = selectedOption;
                }
                else
                {
                    opSelection = selectedOption;
                    DisableMenuOption("Button_Repeat_Battle");
                }

                if (UFE.GetLocalPlayer() == player)
                {
                    DisableMenuOption("Button_Repeat_Battle");
                    DisableMenuOption("Button_Character_Selection_Screen");
                    //DisableMenuOption("Button_Search_Match_Screen");
                }

                if ((mySelection == Option.CharacterSelectionScreen && (opSelection == Option.CharacterSelectionScreen || opSelection == Option.RepeatBattle)) ||
                    (opSelection == Option.CharacterSelectionScreen && (mySelection == Option.CharacterSelectionScreen || mySelection == Option.RepeatBattle)))
                {
                    UFE.EndGame();
                    UFE.FireGameEnds();

                    UFE.StartCharacterSelectionScreen();
                    UFE.PauseGame(false);
                }
            }
            else if (selectedOption == OnlineModeAfterBattleScreen.Option.MainMenu)
            {
                if (UFE.GetLocalPlayer() == player)
                {
                    UFE.EndGame();
                    UFE.FireGameEnds();
                    UFE.DisconnectFromGame();

                    UFE.StartMainMenuScreen();
                    UFE.PauseGame(false);
                }
                else
                {
                    DisableMenuOption("Button_Repeat_Battle");
                    DisableMenuOption("Button_Character_Selection_Screen");
                }
            }
            else if (selectedOption == OnlineModeAfterBattleScreen.Option.SearchNewMatch)
            {
                if (UFE.GetLocalPlayer() == player)
                {
                    UFE.EndGame();
                    UFE.FireGameEnds();
                    UFE.DisconnectFromGame();

                    UFE.StartSearchMatchScreen();
                    UFE.PauseGame(false);
                }
                else
                {
                    DisableMenuOption("Button_Repeat_Battle");
                    DisableMenuOption("Button_Character_Selection_Screen");
                }
            }
            else if (selectedOption == OnlineModeAfterBattleScreen.Option.RepeatBattle)
            {
                HighlightMenuOption("Button_Repeat_Battle");

                if (UFE.GetLocalPlayer() == player)
                {
                    mySelection = selectedOption;
                }
                else
                {
                    opSelection = selectedOption;
                }

                if (UFE.GetLocalPlayer() == player)
                {
                    DisableMenuOption("Button_Repeat_Battle");
                    DisableMenuOption("Button_Character_Selection_Screen");
                    //DisableMenuOption("Button_Search_Match_Screen");
                }

                if ((mySelection == Option.RepeatBattle && (opSelection == Option.CharacterSelectionScreen || opSelection == Option.RepeatBattle)) ||
                    (opSelection == Option.RepeatBattle && (mySelection == Option.CharacterSelectionScreen || mySelection == Option.RepeatBattle)))
                {
                    UFE.EndGame();
                    UFE.FireGameEnds();

                    if (opSelection == Option.CharacterSelectionScreen || mySelection == Option.CharacterSelectionScreen)
                    {
                        UFE.StartCharacterSelectionScreen();
                    }
                    else
                    {
                        UFE.StartLoadingBattleScreen();
                    }
                    UFE.PauseGame(false);
                }
            }
        }
        #endregion
    }
}