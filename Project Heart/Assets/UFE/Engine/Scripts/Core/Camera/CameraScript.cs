using UnityEngine;
using System.Collections;

namespace UFE3D
{
    public class CameraScript : MonoBehaviour
    {

        #region trackable definitions
        public bool cinematicFreeze;
        public Vector3 currentLookAtPosition;
        public Vector3 defaultCameraPosition;
        public float defaultDistance;
        public float freeCameraSpeed;
        public GameObject currentOwner;
        public bool killCamMove;
        public float movementSpeed;
        public float rotationSpeed;
        public float standardGroundHeight;
        public Vector3 targetPosition;
        public Quaternion targetRotation;
        public float targetFieldOfView;
        #endregion


        public GameObject playerLight;
        public ControlsScript player1;
        public ControlsScript player2;

        public Transform dollyTransform;


        void Start()
        {
            playerLight = GameObject.Find("Player Light");

            ResetCam();
            defaultDistance = Vector3.Distance(player1.transform.position, player2.transform.position);
            defaultCameraPosition = Camera.main.transform.position;
            movementSpeed = UFE.config.cameraOptions.movementSpeed;
            rotationSpeed = UFE.config.cameraOptions.rotationSpeed;
            UFE.freeCamera = false;

            if (UFE.config.gameplayType != GameplayType._2DFighter)
            {
                GameObject dolly = new GameObject("Camera Dolly");
                dollyTransform = dolly.transform;
                dollyTransform.SetParent(UFE.gameEngine.transform);
            }
        }

        public void ResetCam()
        {
            Camera.main.transform.localPosition = UFE.config.cameraOptions.initialDistance;
            Camera.main.transform.position = UFE.config.cameraOptions.initialDistance;
            Camera.main.transform.localRotation = Quaternion.Euler(UFE.config.cameraOptions.initialRotation);
            Camera.main.fieldOfView = UFE.config.cameraOptions.initialFieldOfView;
        }

        public Vector3 LerpByDistance(Vector3 A, Vector3 B, float speed)
        {
            Vector3 P = speed * (float)UFE.fixedDeltaTime * Vector3.Normalize(B - A) + A;
            return P;
        }

        public void DoFixedUpdate()
        {
            if (killCamMove) return;
            if (player1 == null || player2 == null) return;

            if (UFE.freeCamera)
            {
                if (UFE.config.gameplayType != GameplayType._2DFighter)
                {
                    Update3DPosition();
                }

                Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFieldOfView, (float)UFE.fixedDeltaTime * freeCameraSpeed * 1.8f);
                Camera.main.transform.localPosition = Vector3.Lerp(Camera.main.transform.localPosition, targetPosition, (float)UFE.fixedDeltaTime * freeCameraSpeed * 1.8f);
                Camera.main.transform.localRotation = Quaternion.Slerp(Camera.main.transform.localRotation, targetRotation, (float)UFE.fixedDeltaTime * freeCameraSpeed * 1.8f);
            }
            else
            {
                if (UFE.config.gameplayType == GameplayType._2DFighter)
                {
                    Vector3 newPosition = ((player1.transform.position + player2.transform.position) / 2) + UFE.config.cameraOptions.initialDistance;
                    float highestPos = player1.transform.position.y > player2.transform.position.y ? player1.transform.position.y : player2.transform.position.y;
                    if (highestPos >= UFE.config.cameraOptions.verticalThreshold)
                    {
                        if (UFE.config.cameraOptions.verticalPriority == VerticalPriority.AverageDistance)
                        {
                            newPosition.y += Mathf.Abs(player1.transform.position.y - player2.transform.position.y) / 2;
                        }
                        else if (UFE.config.cameraOptions.verticalPriority == VerticalPriority.HighestCharacter)
                        {
                            newPosition.y += highestPos;
                        }
                    }
                    else
                    {
                        newPosition.y = UFE.config.cameraOptions.initialDistance.y;
                    }


                    newPosition.x = Mathf.Clamp(newPosition.x,
                        (float)(UFE.config.selectedStage.position.x + UFE.config.selectedStage._leftBoundary + 8),
                        (float)(UFE.config.selectedStage.position.x + UFE.config.selectedStage._rightBoundary - 8));

                    // Zoom
                    if (UFE.config.cameraOptions.enableZoom)
                    {
                        newPosition.z = UFE.config.cameraOptions.initialDistance.z - Vector3.Distance(player1.transform.position, player2.transform.position) + defaultDistance;
                        newPosition.z = Mathf.Clamp(newPosition.z, -UFE.config.cameraOptions.maxZoom, -UFE.config.cameraOptions.minZoom);
                    }

                    Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, UFE.config.cameraOptions.initialFieldOfView, (float)UFE.fixedDeltaTime * movementSpeed);
                    Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, newPosition, (float)UFE.fixedDeltaTime * movementSpeed);
                    Camera.main.transform.localRotation = Quaternion.Slerp(Camera.main.transform.localRotation, Quaternion.Euler(UFE.config.cameraOptions.initialRotation), (float)UFE.fixedDeltaTime * UFE.config.cameraOptions.movementSpeed);

                    if (Camera.main.transform.localRotation == Quaternion.Euler(UFE.config.cameraOptions.initialRotation))
                        UFE.normalizedCam = true;
                }
                else
                {
                    if ((!UFE.config.characterRotationOptions.allowAirBorneSideSwitch || (player1.Physics.IsGrounded() && player2.Physics.IsGrounded())) &&
                        (player1.currentMove == null || !player1.currentMove.allowSideSwitch) &&
                        (player2.currentMove == null || !player2.currentMove.allowSideSwitch))
                        Update3DPosition();
                    Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, UFE.config.cameraOptions.initialFieldOfView, (float)UFE.fixedDeltaTime * movementSpeed);
                }

                if (UFE.config.cameraOptions.enableLookAt)
                {
                    Vector3 newLookAtPosition = ((player1.transform.position + player2.transform.position) / 2) + UFE.config.cameraOptions.rotationOffSet;

                    if (UFE.config.cameraOptions.motionSensor != MotionSensor.None)
                    {
                        Vector3 acceleration = Input.acceleration;
                        if (UFE.config.cameraOptions.motionSensor == MotionSensor.Gyroscope && SystemInfo.supportsGyroscope) acceleration = Input.gyro.gravity;

#if UNITY_STANDALONE || UNITY_EDITOR
                        if (Input.mousePresent)
                        {
                            Vector3 mouseXY = new Vector3(Input.mousePosition.x - Screen.width / 2, Input.mousePosition.y - Screen.height / 2, 0);
                            acceleration = mouseXY / 1000;
                        }
#endif
                        acceleration *= UFE.config.cameraOptions.motionSensibility;
                        newLookAtPosition -= acceleration;

                        Vector3 newPosition = Camera.main.transform.position;
                        newPosition.y += acceleration.y;
                        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, newPosition, (float)UFE.fixedDeltaTime * movementSpeed);
                    }


                    currentLookAtPosition = Vector3.Lerp(currentLookAtPosition, newLookAtPosition, (float)UFE.fixedDeltaTime * rotationSpeed);
                    Camera.main.transform.LookAt(currentLookAtPosition, Vector3.up);
                }

                if (playerLight != null) playerLight.GetComponent<Light>().enabled = false;

            }
        }

        public void Update3DPosition()
        {
            if (dollyTransform == null) return;
            // Update dolly transform to center around the 2 characters
            Vector3 dollyPos = Vector3.zero;

            Vector3 leftPlayerPos = player1.transform.position;
            Vector3 rightPlayerPos = player2.transform.position;

            if (player1.mirror > 0)
            {
                leftPlayerPos = player2.transform.position;
                rightPlayerPos = player1.transform.position;
            }

            dollyPos = ((leftPlayerPos + rightPlayerPos) / 2) + new Vector3(0, UFE.config.cameraOptions.height3d, 0);

            dollyTransform.position = dollyPos + dollyTransform.forward;
            dollyTransform.LookAt(dollyTransform.position);


            dollyTransform.LookAt(rightPlayerPos);

#if !UFE_LITE && !UFE_BASIC
            Vector3 distance = Vector3.zero;
            float zoomValue = 0;

            if (UFE.config.gameplayType == GameplayType._3DFighter)
            {
                // Zoom
                if (UFE.config.cameraOptions.enableZoom)
                {
                    zoomValue = UFE.config.cameraOptions.distance3d + Vector3.Distance(leftPlayerPos, rightPlayerPos) - defaultDistance;
                    if (currentOwner != null)
                        zoomValue = UFE.config.cameraOptions.distance3d - defaultDistance;
                    distance.x = Mathf.Clamp(zoomValue, UFE.config.cameraOptions.minZoom, UFE.config.cameraOptions.maxZoom);
                }

                // Update camera position to be perpendicular to the center dolly
                if (currentOwner != null)
                    dollyPos.x = currentOwner.transform.position.x;
                Vector3 newPosition = (Quaternion.Euler(0, dollyTransform.rotation.eulerAngles.y - UFE.config.cameraOptions.dollyAngle, 0) * distance) + dollyPos;
                Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, newPosition, (float)UFE.fixedDeltaTime * movementSpeed);
                Camera.main.transform.LookAt(dollyTransform);
                if (currentOwner != null)
                    Camera.main.transform.LookAt(currentOwner.transform.position + new Vector3(0, UFE.config.cameraOptions.height3d, 0));
            }
            /*else if (UFE.config.gameplayType == GameplayType._3DArena)
            {
                Vector3 newPosition = UFE.config.cameraOptions.initialDistance;
                if (UFE.config.cameraOptions.followCharacters)
                    newPosition += dollyTransform.position;

                // Zoom
                if (UFE.config.cameraOptions.enableZoom)
                {
                    zoomValue = Vector3.Distance(leftPlayerPos, rightPlayerPos) - defaultDistance;
                    zoomValue = Mathf.Clamp(zoomValue, UFE.config.cameraOptions.minZoom, UFE.config.cameraOptions.maxZoom);
                    newPosition = Vector3.MoveTowards(newPosition, dollyPos, -zoomValue);
                }

                Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, newPosition, (float)UFE.fixedDeltaTime * movementSpeed);
                Camera.main.transform.localRotation = Quaternion.Slerp(Camera.main.transform.localRotation, Quaternion.Euler(UFE.config.cameraOptions.initialRotation), (float)UFE.fixedDeltaTime * UFE.config.cameraOptions.movementSpeed);
            }*/
#endif
        }

        public void MoveCameraToLocation(Vector3 targetPos, Vector3 targetRot, float targetFOV, float speed, GameObject owner)
        {
#if !UFE_LITE && !UFE_BASIC
            if (UFE.config.gameplayType == GameplayType._3DFighter)
                Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, UFE.config.cameraOptions.height3d, -UFE.config.cameraOptions.distance3d);
#endif

            targetFieldOfView = targetFOV;
            targetPosition = targetPos;
            targetRotation = Quaternion.Euler(targetRot);
            freeCameraSpeed = speed;
            UFE.freeCamera = true;
            UFE.normalizedCam = false;
            currentOwner = owner;
            if (playerLight != null) playerLight.GetComponent<Light>().enabled = true;
        }

        public void DisableCam()
        {
            Camera.main.enabled = false;
        }

        public void ReleaseCam()
        {
            Camera.main.enabled = true;
            cinematicFreeze = false;
            UFE.freeCamera = false;
            currentOwner = null;
        }

        public void OverrideSpeed(float newMovement, float newRotation)
        {
            movementSpeed = newMovement;
            rotationSpeed = newRotation;
        }

        public void RestoreSpeed()
        {
            movementSpeed = UFE.config.cameraOptions.movementSpeed;
            rotationSpeed = UFE.config.cameraOptions.rotationSpeed;
        }

        public void SetCameraOwner(GameObject owner)
        {
            currentOwner = owner;
        }

        public GameObject GetCameraOwner()
        {
            return currentOwner;
        }

        public Vector3 GetRelativePosition(Transform origin, Vector3 position)
        {
            Vector3 distance = position - origin.position;
            Vector3 relativePosition = Vector3.zero;
            relativePosition.x = Vector3.Dot(distance, origin.right.normalized);
            relativePosition.y = Vector3.Dot(distance, origin.up.normalized);
            relativePosition.z = Vector3.Dot(distance, origin.forward.normalized);

            return relativePosition;
        }

        void OnDrawGizmos()
        {
            Vector3 cameraLeftBounds = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, -Camera.main.transform.position.z));
            Vector3 cameraRightBounds = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, -Camera.main.transform.position.z));

            cameraLeftBounds.x = Camera.main.transform.position.x - ((float)UFE.config.cameraOptions._maxDistance / 2);
            cameraRightBounds.x = Camera.main.transform.position.x + ((float)UFE.config.cameraOptions._maxDistance / 2);

            Gizmos.DrawLine(cameraLeftBounds, cameraLeftBounds + new Vector3(0, 15, 0));
            Gizmos.DrawLine(cameraRightBounds, cameraRightBounds + new Vector3(0, 15, 0));


            // 3D Position
            // Update CenterTransform
            /*if (UFE.config.gameplayType != GameplayType._2DFighter)
            {
                Vector3 newPos = ((player1.transform.position + player2.transform.position) / 2) + (new Vector3(0, UFE.config.cameraOptions.height3d, 0));
                Vector3 distance = new Vector3(UFE.config.cameraOptions.distance3d, 0, 0);
                Vector3 target = (Quaternion.Euler(0, dollyTransform.rotation.eulerAngles.y, 0) * distance) + newPos;
                Gizmos.color = Color.gray;
                Gizmos.DrawWireSphere(target, .3f);
            }*/
        }
    }
}