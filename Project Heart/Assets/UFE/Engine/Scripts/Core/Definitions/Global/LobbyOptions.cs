using System;

namespace UFE3D
{
    [Serializable]
    public class LobbyOptions : ICloneable
    {
        public string lobbyName;
        public LobbyMatchCreationSystem matchMakingType;
        public GameMode gameMode;
        public bool allowPrivateRooms;
        public bool matchMakingToggle;
        public bool winnerToggle;
        public bool loserToggle;
        public NetworkUserData[] matchMakingUserData = new NetworkUserData[0];
        public NetworkUserData[] winnerUserData = new NetworkUserData[0];
        public NetworkUserData[] loserUserData = new NetworkUserData[0];

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}