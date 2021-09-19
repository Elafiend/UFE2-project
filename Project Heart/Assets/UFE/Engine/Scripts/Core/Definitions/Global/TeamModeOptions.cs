using UnityEngine;
using System;
using FPLibrary;

namespace UFE3D
{
    public enum TeamType
    {
        Tag,
        AllActive
    }

    public enum PlayerController
    {
        Player1,
        Player2
        //Player3,
        //Player4
    }

    [System.Serializable]
    public class TeamModeOptions : ICloneable
    {
        public string modeName;
        public Team[] teams = new Team[2];
        public bool teamsToggle;

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }

    [System.Serializable]
    public class Team: ICloneable
    {
        public CharacterController[] characters = new CharacterController[1];
        public string teamName;
        public TeamType teamType;
        public bool charactersToggle;

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }

    [System.Serializable]
    public class CharacterController : ICloneable
    {
        public PlayerController player = PlayerController.Player1;
        public bool endWhenDies = false;
        public FPVector spawnPosition;

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}