namespace UFE3D
{
    [System.Serializable]
    public class NetworkOptions
    {
        //general options
        public bool forceAnimationControl;
        public bool disableRootMotion;
        public bool disableBlending;
        public bool disableRotationBlend;
        public bool applyFrameDelayOffline;
        public int recordingBuffer;
        public int port = 1337;
        public float floatDesynchronizationThreshold = 0.5f;
        public bool logSyncMsg;
        public bool postRollbackRecording;
        public bool generateVariableLog;
        public string textFilePath;

        //online service
        public NetworkService networkService;
        public string authKey;
        public LobbyOptions[] lobbies = new LobbyOptions[] { new LobbyOptions() };

        //Photon options
        public PhotonHostingService photonHostingService = PhotonHostingService.PhotonServer;
        public string playFabTitleId;
        public string photonApplicationId;

        // LAN Game Discovery
        public int lanDiscoveryPort = 1338;
        public float lanDiscoveryBroadcastInterval = 1f;
        public float lanDiscoverySearchInterval = 5f;
        public float lanDiscoverySearchTimeout = 120f;

        //netcode
        public bool allowRollBacks = false;
        public NetworkRollbackBalancing rollbackBalancing = NetworkRollbackBalancing.Conservative;
        public NetworkFrameDelay frameDelayType = NetworkFrameDelay.Auto;
        public int minFrameDelay = 4;
        public int maxFrameDelay = 30;
        public int defaultFrameDelay = 6;
        public int maxBufferSize = 30;
        public int maxFastForwards = 10;
        public int spawnBuffer = 30;
        public bool ufeTrackers = false;
        public NetworkMessageSize networkMessageSize = NetworkMessageSize.Size32Bits;
        public bool onlySendInputChanges = true;
        public NetworkInputMessageFrequency inputMessageFrequency = NetworkInputMessageFrequency.EveryFrame;
        public NetworkInputMessageFrequency synchronizationMessageFrequency = NetworkInputMessageFrequency.EveryFrame;
        public NetworkSynchronizationAction synchronizationAction = NetworkSynchronizationAction.PlaybackTool;
    }
}