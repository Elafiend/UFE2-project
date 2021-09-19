namespace UFE3D
{
    [System.Serializable]
    public class CustomControls
    {
        public bool overrideControlFreak = false;
        public InputTouchControllerBridge controlFreak2Prefab = null;

        public bool zAxisMovement = false;
        public bool disableJump = false;
        public bool disableCrouch = false;
        public ButtonPress jumpButton = ButtonPress.Up;
        public ButtonPress crouchButton = ButtonPress.Down;
    }
}