using UnityEngine;
using FPLibrary;

[System.Serializable]
public class RoundOptions
{
    public int totalRounds = 3;
    public bool hasTimer = true;
    public Fix64 _timer = 99;
    public Fix64 _timerSpeed = 100;
    public FPVector _p1XPosition = new FPVector(-5, 0, 0);
    public FPVector _p2XPosition = new FPVector(5, 0, 0);
    public FPVector _p1XRotation = new FPVector(0, 90, 0);
    public FPVector _p2XRotation = new FPVector(0, -90, 0);
    public Fix64 _endGameDelay = 4;
    public Fix64 _showMenuDelay = 3.5;
    public Fix64 _newRoundDelay = 1;
    public Fix64 _slowMoTimer = 3;
    public Fix64 _slowMoSpeed = .2;
    public AudioClip victoryMusic;
    public bool resetLifePoints = true;
    public bool resetPositions = true;
    public bool allowMovementStart = true;
    public bool allowMovementEnd = true;
    public bool playIntrosAtSameTime = false;
    public bool inhibitGaugeGain = true;
    public bool rotateBodyKO = true;
    public bool slowMotionKO = true;
    public bool cameraZoomKO = true;
    public bool freezeCamAfterOutro = true;
}