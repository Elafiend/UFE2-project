using System;

namespace UFE3D
{
    [System.Serializable]
    public class BasicMoves : ICloneable
    {
        public BasicMoveInfo idle = new BasicMoveInfo();
        public BasicMoveInfo moveForward = new BasicMoveInfo();
        public BasicMoveInfo moveBack = new BasicMoveInfo();
        public BasicMoveInfo moveSideways = new BasicMoveInfo();
        public BasicMoveInfo crouching = new BasicMoveInfo();
        public BasicMoveInfo takeOff = new BasicMoveInfo();
        public BasicMoveInfo jumpStraight = new BasicMoveInfo();
        public BasicMoveInfo jumpBack = new BasicMoveInfo();
        public BasicMoveInfo jumpForward = new BasicMoveInfo();
        public BasicMoveInfo fallStraight = new BasicMoveInfo();
        public BasicMoveInfo fallBack = new BasicMoveInfo();
        public BasicMoveInfo fallForward = new BasicMoveInfo();
        public BasicMoveInfo landing = new BasicMoveInfo();
        public BasicMoveInfo blockingCrouchingPose = new BasicMoveInfo();
        public BasicMoveInfo blockingCrouchingHit = new BasicMoveInfo();
        public BasicMoveInfo blockingHighPose = new BasicMoveInfo();
        public BasicMoveInfo blockingHighHit = new BasicMoveInfo();
        public BasicMoveInfo blockingLowHit = new BasicMoveInfo();
        public BasicMoveInfo blockingAirPose = new BasicMoveInfo();
        public BasicMoveInfo blockingAirHit = new BasicMoveInfo();
        public BasicMoveInfo parryCrouching = new BasicMoveInfo();
        public BasicMoveInfo parryHigh = new BasicMoveInfo();
        public BasicMoveInfo parryLow = new BasicMoveInfo();
        public BasicMoveInfo parryAir = new BasicMoveInfo();
        public BasicMoveInfo groundBounce = new BasicMoveInfo();
        public BasicMoveInfo standingWallBounce = new BasicMoveInfo();
        public BasicMoveInfo standingWallBounceKnockdown = new BasicMoveInfo();
        public BasicMoveInfo airWallBounce = new BasicMoveInfo();
        public BasicMoveInfo fallingFromGroundBounce = new BasicMoveInfo();
        public BasicMoveInfo fallingFromAirHit = new BasicMoveInfo();
        public BasicMoveInfo fallDown = new BasicMoveInfo();
        public BasicMoveInfo airRecovery = new BasicMoveInfo();
        public BasicMoveInfo getHitCrouching = new BasicMoveInfo();
        public BasicMoveInfo getHitHigh = new BasicMoveInfo();
        public BasicMoveInfo getHitLow = new BasicMoveInfo();
        public BasicMoveInfo getHitHighKnockdown = new BasicMoveInfo();
        public BasicMoveInfo getHitMidKnockdown = new BasicMoveInfo();
        public BasicMoveInfo getHitAir = new BasicMoveInfo();
        public BasicMoveInfo getHitCrumple = new BasicMoveInfo();
        public BasicMoveInfo getHitKnockBack = new BasicMoveInfo();
        public BasicMoveInfo getHitSweep = new BasicMoveInfo();
        public BasicMoveInfo standUp = new BasicMoveInfo();
        public BasicMoveInfo standUpFromAirHit = new BasicMoveInfo();
        public BasicMoveInfo standUpFromKnockBack = new BasicMoveInfo();
        public BasicMoveInfo standUpFromStandingHighHit = new BasicMoveInfo();
        public BasicMoveInfo standUpFromStandingMidHit = new BasicMoveInfo();
        public BasicMoveInfo standUpFromCrumple = new BasicMoveInfo();
        public BasicMoveInfo standUpFromSweep = new BasicMoveInfo();
        public BasicMoveInfo standUpFromStandingWallBounce = new BasicMoveInfo();
        public BasicMoveInfo standUpFromAirWallBounce = new BasicMoveInfo();
        public BasicMoveInfo standUpFromGroundBounce = new BasicMoveInfo();

        public bool moveEnabled = true;
        public bool jumpEnabled = true;
        public bool crouchEnabled = true;
        public bool blockEnabled = true;
        public bool parryEnabled = true;

        public object Clone()
        {
            return CloneObject.Clone(this);
        }
    }
}