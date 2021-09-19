using UnityEngine;

namespace UFE3D
{
    [System.Serializable]
    public class InputOptions
    {
        public InputManagerType inputManagerType;
        public bool cInputAllowDuplicates = false;
        public float cInputGravity = 3;
        public float cInputSensitivity = 3;
        public float cInputDeadZone = 0.001f;
        public GUISkin cInputSkin = null;
        public GameObject controlFreakPrefab = null;
        public InputTouchControllerBridge controlFreak2Prefab = null;
        public float controlFreakDeadZone = 0.5f;

        public bool forceDigitalInput = true;
        public ButtonPress confirmButton;
        public ButtonPress cancelButton;
    }
}