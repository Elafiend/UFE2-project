using UnityEngine;
using FPLibrary;

namespace UFE3D
{
    [System.Serializable]
    public class CameraOptions
    {
        public Vector3 initialDistance;
        public Vector3 initialRotation;
        public bool enableLookAt;

        public float dollyAngle;
        public float distance3d;
        public float height3d;
        public bool enableZoom = true;
        public bool followCharacters = true;

        public float initialFieldOfView;
        public VerticalPriority verticalPriority;
        public float verticalThreshold;
        public float movementSpeed = 15;
        public float minZoom = 38;
        public float maxZoom = 54;
        public Fix64 _maxDistance = 22;
        public float rotationSpeed = 20;
        public float heightOffSet = 4;
        public Vector3 rotationOffSet = new Vector3(0, 4, 0);
        public MotionSensor motionSensor;
        public float motionSensibility = 1;
    }
}