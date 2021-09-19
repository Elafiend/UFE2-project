using FPLibrary;

[System.Serializable]
public class PhysicsData
{
    public Fix64 _moveForwardSpeed = 4; // How fast this character can move forward
    public Fix64 _moveBackSpeed = 3.5; // How fast this character can move backwards
    public Fix64 _moveSidewaysSpeed = 4; // How fast this character can move sizeways (3D Gameplay)
    public bool highMovingFriction = true; // When releasing the horizontal controls character will stop imediatelly
    public Fix64 _friction = 30; // Friction used in case of highMovingFriction false. Also used when player is pushed

    public bool canCrouch = true;
    public int crouchDelay = 2;
    public int standingDelay = 2;

    public bool canJump = true;
    public bool pressureSensitiveJump = false; // How high this character will jumps
    public Fix64 _jumpForce = 40; // How high this character will jumps
    public Fix64 _minJumpForce = 30; // When using pressure sensitive jumping, what is the minimium force applied
    public int minJumpDelay = 4;
    public Fix64 _jumpDistance = 8; // How far this character will move horizontally forward while jumping
    public Fix64 _jumpBackDistance = 8; // How far this character will move horizontally backwards while jumping
    public bool cumulativeForce = true; // If this character is being juggled, should new forces add to or replace existing force?
    public int multiJumps = 1; // Can this character double or triple jump? Set how many times the character can jump here
    public Fix64 _weight = 175;
    public int jumpDelay = 8;
    public int landingDelay = 7;
    public Fix64 _groundCollisionMass = 2;
}