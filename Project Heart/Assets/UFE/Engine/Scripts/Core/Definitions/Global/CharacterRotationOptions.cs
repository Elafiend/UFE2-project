using FPLibrary;

[System.Serializable]
public class CharacterRotationOptions
{
    public bool autoMirror = true;
    public bool rotateWhileJumping = false;
    public bool rotateOnMoveOnly = false;
    public bool allowAirBorneSideSwitch = false;
    public bool fixRotationWhenStunned = false;
    public bool fixRotationWhenBlocking = true;
    public bool fixRotationOnHit = true;
    public bool smoothRotation = true;
    public Fix64 _rotationSpeed;
    public Fix64 _mirrorBlending;
}