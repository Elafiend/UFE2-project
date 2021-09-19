using System;

namespace UFE3D
{
    [System.Serializable]
    public class LanguageOptions : ICloneable
    {
        public string languageName = "English";
        public string start = "Start";
        public string options = "Options";
        public string credits = "Credits";
        public string selectYourCharacter = "Select Your Character";
        public string selectYourStage = "Select Your Stage";
        public string round = "Round %round%";
        public string finalRound = "Final Round";
        public string fight = "Fight!";
        public string firstHit = "First Hit!";
        public string combo = "%number% hit combo!";
        public string parry = "Parry!";
        public string counterHit = "Counter!";
        public string victory = "%character% wins!";
        public string challengeBegins = "Start!";
        public string challengeEnds = "Success!";
        public string timeOver = "Time Over";
        public string perfect = "Perfect!";
        public string rematch = "Rematch";
        public string quit = "Quit";
        public string ko = "K.O.";
        public string draw = "Draw";
        public bool defaultSelection;

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}