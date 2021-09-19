using UnityEngine;
using System;

namespace UFE3D
{
    [System.Serializable]
    public class InputReferences : ICloneable
    {
        // Common Parameters
        public InputType inputType;
        public string inputButtonName;
        public ButtonPress engineRelatedButton;

        // Input Manager parameters
        public string joystickAxisName;

        // cInput parameters
        public string cInputPositiveKeyName;
        public string cInputPositiveDefaultKey;
        public string cInputPositiveAlternativeKey;

        public string cInputNegativeKeyName;
        public string cInputNegativeDefaultKey;
        public string cInputNegativeAlternativeKey;

        // Input Viewer
        public Texture2D inputViewerIcon1;
        public Texture2D inputViewerIcon2;
        public Texture2D inputViewerIcon3;
        public Texture2D inputViewerIcon4;
        public Texture2D inputViewerIcon5;
        public Texture2D inputViewerIcon6;
        public Texture2D activeIcon { get; set; }

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}