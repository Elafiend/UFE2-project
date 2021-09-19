using UnityEngine;
using System.Collections.Generic;
using FPLibrary;

namespace UFE3D
{
    [System.Serializable]
    public class GlobalInfo : ScriptableObject
    {
        #region public instance fields
        public float version;
        public CharacterInfo player1Character;
        public CharacterInfo player2Character;
        public CharacterInfo[] player1Team = new CharacterInfo[0];
        public CharacterInfo[] player2Team = new CharacterInfo[0];

        public StageOptions selectedStage;
        public string gameName;
        public GameplayType gameplayType;
        public LanguageOptions selectedLanguage;

        public GameGUI gameGUI;
        public StoryMode storyMode;
        
        public int fps = 60;
        public Fix64 _gameSpeed = 1;
        public int executionBufferTime = 10;
        public ExecutionBufferType executionBufferType;
        public int plinkingDelay = 1;

        public Fix64 _preloadingTime = 1;
        public bool preloadStage = true;
        public bool preloadHitEffects = true;
        public bool warmAllShaders = true;

        public Fix64 _gravity = .37;
        public bool detect3D_Hits;
        public bool lockZAxis = true;
        public bool runInBackground;
        public bool useFixedUpdateInputs;
        public bool sortCharacterOnHit;
        public int foregroundSortLayer;
        public int backgroundSortLayer;
        public LanguageOptions[] languages = new LanguageOptions[] { new LanguageOptions() };
        public DeploymentOptions deploymentOptions;
        public CameraOptions cameraOptions;
        public GlobalLockOnOptions lockOnOptions;
        public CharacterRotationOptions characterRotationOptions;
        public RoundOptions roundOptions;
        public BounceOptions groundBounceOptions;
        public BounceOptions wallBounceOptions;
        public CounterHitOptions counterHitOptions;
        public ComboOptions comboOptions;
        public BlockOptions blockOptions;
        public KnockDownOptions knockDownOptions;
        public HitOptions hitOptions;

        public InputReferences[] player1_Inputs = new InputReferences[0]; // Reference to Unity's InputManager to UFE's keys
        public InputReferences[] player2_Inputs = new InputReferences[0]; // Reference to Unity's InputManager to UFE's keys
        public InputOptions inputOptions = new InputOptions();

        public StageOptions[] stages = new StageOptions[0];
        public CharacterInfo[] characters = new CharacterInfo[0];
        public TeamModeOptions[] teamModes = new TeamModeOptions[0];
        public DebugOptions debugOptions = new DebugOptions();
        public TrainingModeOptions trainingModeOptions = new TrainingModeOptions();
        public ChallengeModeOptions[] challengeModeOptions = new ChallengeModeOptions[0];
        public AIOptions aiOptions = new AIOptions();
        public NetworkOptions networkOptions = new NetworkOptions();

        public bool music = true;
        public float musicVolume = 1f;
        public bool soundfx = true;
        public float soundfxVolume = 1f;

        public Color colorStateOne = Color.red;
        public Color colorIsHit = Color.magenta;
        public Color colorBodyCollider = Color.yellow;
        public Color colorNoCollider = Color.white;
        public Color colorThrowCollider = new Color(1f, 0, .5f);
        public Color colorPhysicalInvincibleCollider = Color.gray;
        public Color colorProjectileInvincibleCollider = Color.cyan;
        public Color colorHitCollider = Color.green;
        public Color colorLowHitBoxType = Color.red;
        public Color colorHighHitBoxType = Color.yellow;
        public Color colorCollisionBox = Color.yellow;
        public Color colorHurtBoxThrow = new Color(1f, .5f, 0);
        public Color colorHurtBoxNotThrow = Color.cyan;
        public Color colorBlockBox = Color.blue;

        public bool colorStateOneFill = false;
        public bool colorIsHitFill = false;
        public bool colorBodyColliderFill = false;
        public bool colorNoColliderFill = false;
        public bool colorThrowColliderFill = false;
        public bool colorPhysicalInvincibleColliderFill = false;
        public bool colorProjectileInvincibleColliderFill = false;
        public bool colorHitColliderFill = false;
        public bool colorLowHitBoxTypeFill = false;
        public bool colorHighHitBoxTypeFill = false;
        public bool colorCollisionBoxFill = false;
        public bool colorHurtBoxThrowFill = false;
        public bool colorHurtBoxNotThrowFill = false;
        public bool colorBlockBoxFill = false;
        #endregion


        #region trackable definitions
        public int currentRound { get; set; }
        public bool lockInputs { get; set; }
        public bool lockMovements { get; set; }
        public int selectedChallenge { get; set; }
        public int selectedTeamMode { get; set; }
        public MatchType selectedMatchType { get; set; }
        #endregion

        #region public instance methods
        public virtual void ValidateStoryModeInformation()
        {
            // First, check that every character index in Story Mode is valid
            for (int i = this.storyMode.selectableCharactersInStoryMode.Count - 1; i >= 0; --i)
            {
                int character = this.storyMode.selectableCharactersInStoryMode[i];

                if (character < 0 || character >= this.characters.Length)
                {
                    this.storyMode.characterStories.Remove(character);
                    this.storyMode.selectableCharactersInStoryMode.RemoveAt(i);
                }
                else if (!this.storyMode.characterStories.ContainsKey(character))
                {
                    this.storyMode.characterStories[character] = new CharacterStory();
                }
            }

            // Then check that every character index in Versus Mode is valid
            for (int i = this.storyMode.selectableCharactersInVersusMode.Count - 1; i >= 0; --i)
            {
                int character = this.storyMode.selectableCharactersInVersusMode[i];
                if (character < 0 || character >= this.characters.Length)
                {
                    this.storyMode.selectableCharactersInVersusMode.RemoveAt(i);
                }
            }

            // Finally, check that every character and stage index are valid in the Character Stories
            this.ValidateCharacterStory(this.storyMode.defaultStory);
            foreach (CharacterStory story in this.storyMode.characterStories.Values)
            {
                this.ValidateCharacterStory(story);
            }
        }
        #endregion

        #region protected instance methods
        protected virtual void ValidateCharacterStory(CharacterStory story)
        {
            if (story != null && story.fightsGroups != null)
            {
                foreach (FightsGroup group in story.fightsGroups)
                {
                    List<StoryModeBattle> battles = new List<StoryModeBattle>(group.opponents);

                    for (int i = battles.Count - 1; i >= 0; --i)
                    {
                        StoryModeBattle battle = battles[i];

                        if (battle.opponentCharacterIndex < 0 || battle.opponentCharacterIndex >= this.characters.Length)
                        {
                            battles.RemoveAt(i);
                        }
                        else
                        {
                            for (int j = battle.possibleStagesIndexes.Count - 1; j >= 0; --j)
                            {
                                int stageIndex = battle.possibleStagesIndexes[j];

                                if (stageIndex < 0 || stageIndex >= this.stages.Length)
                                {
                                    battle.possibleStagesIndexes.RemoveAt(j);
                                }
                            }

                            if (battle.possibleStagesIndexes.Count == 0 && this.stages.Length > 0)
                            {
                                battle.possibleStagesIndexes.Add(i % this.stages.Length);
                            }
                        }
                    }

                    group.opponents = battles.ToArray();
                }
            }
        }
        #endregion
    }
}