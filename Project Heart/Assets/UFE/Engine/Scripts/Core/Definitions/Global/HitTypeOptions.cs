using UnityEngine;
using FPLibrary;

[System.Serializable]
public class HitTypeOptions
{
    public GameObject hitParticle;
    public float killTime;
    public AudioClip hitSound;
    public HitEffectSpawnPoint spawnPoint = HitEffectSpawnPoint.StrokeHitBox;
    public Fix64 _freezingTime;
    public Fix64 _animationSpeed = .1;
    public bool autoHitStop = true;
    public Fix64 _hitStop = .1;
    public bool mirrorOn2PSide = false;
    public bool shakeCharacterOnHit = true;
    public bool shakeCameraOnHit = true;
    public Fix64 _shakeDensity = .8;
    public Fix64 _shakeCameraDensity = .8;
    public bool editorToggle;
}