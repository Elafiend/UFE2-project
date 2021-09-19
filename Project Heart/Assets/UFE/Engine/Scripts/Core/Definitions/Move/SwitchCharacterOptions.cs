using System;

namespace UFE3D
{
    public enum CharacterSwitchType
    {
        NextCharacter = 12,
        NearestCharacter = 13,
        Character1 = 0,
        Character2 = 1,
        Character3 = 2,
        Character4 = 3,
        Character5 = 4,
        Character6 = 5,
        Character7 = 6,
        Character8 = 7,
        Character9 = 8,
        Character10 = 9,
        Character11 = 10,
        Character12 = 11
    }

    [System.Serializable]
    public class SwitchCharacterOptions : ICloneable
    {
        public int castingFrame;

        public CharacterSwitchType characterSwitchType;

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}