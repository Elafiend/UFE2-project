using System.Collections.Generic;

namespace UFE3D
{
    [System.Serializable]
    public class CharacterStories : DGP.Util.Collections.SerializableDictionary<int, CharacterStory> { }

    [System.Serializable]
    public class StoryMode
    {
        public bool useSameStoryForAllCharacters;
        public bool canCharactersFightAgainstThemselves;
        public CharacterStory defaultStory;

        public List<int> selectableCharactersInStoryMode = new List<int>();
        public List<int> selectableCharactersInVersusMode = new List<int>();
        public CharacterStories characterStories = new CharacterStories();
    }

    [System.Serializable]
    public class StoryModeBattle
    {
        public StoryModeScreen conversationBeforeBattle;
        public StoryModeScreen conversationAfterBattle;
        public int opponentCharacterIndex;
        public List<int> possibleStagesIndexes = new List<int>();
    }

    [System.Serializable]
    public class FightsGroup
    {
        public int maxFights = 4; // maxFights is only used when mode == FightsGroupMode.FightAgainstSeveralRandomOpponents
        public FightsGroupMode mode = FightsGroupMode.FightAgainstAllOpponentsInTheGroupInRandomOrder;
        public string name = string.Empty;
        public StoryModeBattle[] opponents = new StoryModeBattle[0];
        public bool showOpponentsInEditor;
    }

    public enum FightsGroupMode
    {
        FightAgainstSeveralOpponentsInTheGroupInRandomOrder,
        FightAgainstAllOpponentsInTheGroupInRandomOrder,
        FightAgainstAllOpponentsInTheGroupInTheDefinedOrder
    }

    [System.Serializable]
    public class CharacterStory
    {
        public StoryModeScreen opening;
        public StoryModeScreen ending;
        public FightsGroup[] fightsGroups = new FightsGroup[0];
        public bool showStoryInEditor;
    }

    public class StoryModeInfo
    {
        // The information about the character story
        public CharacterStory characterStory = null;

        // Whether the character can fight against himself in Story Mode
        public bool canFightAgainstHimself = false;

        // The index of the current "group"
        public int currentGroup = 0;

        // The index of the current "battle" in the current "group"
        public int currentBattle = 0;

        // The information about the current battle
        public StoryModeBattle currentBattleInformation = null;

        // The indexes of the characters that have been defeated in the current "group".
        // It's used only if the player must fight the opponents in a group in random order.
        public HashSet<int> defeatedOpponents = new HashSet<int>();
    }
}