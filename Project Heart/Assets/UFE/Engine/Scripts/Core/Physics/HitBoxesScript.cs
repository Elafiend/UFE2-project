using UnityEngine;
using System.Collections.Generic;
using FPLibrary;
using UFE3D;

namespace UFE3D
{
    public class HitBoxesScript : MonoBehaviour
    {

        #region trackable definitions
        public bool isHit;
        public HitBox[] hitBoxes;
        public HurtBox[] activeHurtBoxes;
        public BlockArea blockableArea;
        public HitConfirmType hitConfirmType;
        public Fix64 collisionBoxSize;
        public bool inverted;
        public bool bakeSpeed;
        public FPVector deltaPosition;
        public AnimationMap[] animationMaps = new AnimationMap[0];
        #endregion

        [HideInInspector] public ControlsScript controlsScript;
        [HideInInspector] public bool previewInvertRotation;
        [HideInInspector] public bool previewMirror;
        [HideInInspector] public HitBox[] autoHitBoxes;

        public bool rectangleHitBoxLocationTest;
        public Texture2D rectTexture;

        public MoveSetScript moveSetScript;
        private Renderer characterRenderer;

        private FPVector worldPosition { get { return controlsScript != null ? controlsScript.worldTransform.position : FPVector.ToFPVector(transform.position); } set { controlsScript.worldTransform.position = value; } }

        public bool previewAllBoxes = true;
        public CustomHitBoxesInfo customHitBoxes;

        void Start()
        {
            if (transform.parent != null)
            {
                controlsScript = transform.parent.gameObject.GetComponent<ControlsScript>();
            }
            moveSetScript = GetComponent<MoveSetScript>();
            UpdateRenderer();

            if (moveSetScript != null)
            {
                foreach (MoveInfo move in moveSetScript.moves)
                {
                    if (move == null)
                    {
                        Debug.LogWarning("You have empty entries in your move list. Check your special moves under Character Editor.");
                        continue;
                    }
                    foreach (InvincibleBodyParts invBodyPart in move.invincibleBodyParts)
                    {
                        List<HitBox> invHitBoxes = new List<HitBox>();
                        foreach (BodyPart bodyPart in invBodyPart.bodyParts)
                        {
                            foreach (HitBox hitBox in hitBoxes)
                            {
                                if (bodyPart == hitBox.bodyPart)
                                {
                                    invHitBoxes.Add(hitBox);
                                    break;
                                }
                            }
                        }
                        invBodyPart.hitBoxes = invHitBoxes.ToArray();
                    }
                }
            }

            rectangleHitBoxLocationTest = false;
            rectTexture = new Texture2D(1, 1);
            rectTexture.SetPixel(0, 0, Color.red);
            rectTexture.Apply();
        }

        public bool GetDefaultVisibility(BodyPart bodyPart)
        {
            foreach (HitBox hitBox in hitBoxes)
            {
                if (bodyPart == hitBox.bodyPart && hitBox.defaultVisibility) return true;
            }

            return false;
        }

        public FPVector GetPosition(BodyPart bodyPart, bool local = false)
        {
            // If its running from the editor, load positions from transform
            if (animationMaps == null) return FPVector.ToFPVector(GetTransformPosition(bodyPart));

            foreach (HitBox hitBox in hitBoxes)
            {
                if (hitBox.bodyPart == bodyPart)
                {
                    if (local)
                    {
                        return hitBox.localPosition;
                    }
                    else
                    {
                        return hitBox.mappedPosition;
                    }
                }
            }
            return FPVector.zero;
        }

        public Vector3 GetTransformPosition(BodyPart bodyPart)
        {
            foreach (HitBox hitBox in hitBoxes)
            {
                if (bodyPart == hitBox.bodyPart)
                {
                    return hitBox.position.position;
                }
            }
            return Vector3.zero;
        }

        public FPVector GetDeltaPosition()
        {
            if (controlsScript.myInfo.useAnimationMaps)
            {
                return deltaPosition * -controlsScript.mirror;

            }
            else
            {
                return FPVector.ToFPVector(moveSetScript.GetDeltaPosition());
            }
        }

        public HitBoxMap[] GetAnimationMaps()
        {
            List<HitBoxMap> animMaps = new List<HitBoxMap>();
            foreach (HitBox hitBox in hitBoxes)
            {
                HitBoxMap animMap = new HitBoxMap();
                animMap.bodyPart = hitBox.bodyPart;
                animMap.mappedPosition = FPVector.ToFPVector(hitBox.position.position);
                animMaps.Add(animMap);
            }

            return animMaps.ToArray();
        }

        public Transform GetTransform(BodyPart bodyPart)
        {
            foreach (HitBox hitBox in hitBoxes)
            {
                if (bodyPart == hitBox.bodyPart) return hitBox.position;
            }
            return null;
        }

        public void SetTransform(BodyPart bodyPart, Transform transform)
        {
            foreach (HitBox hitBox in hitBoxes)
            {
                if (bodyPart == hitBox.bodyPart)
                {
                    hitBox.position = transform;
                    return;
                }
            }
        }

        public HitBox[] GetHitBoxes(BodyPart[] bodyParts)
        {
            List<HitBox> hitBoxesList = new List<HitBox>();
            foreach (HitBox hitBox in hitBoxes)
            {
                foreach (BodyPart bodyPart in bodyParts)
                {
                    if (bodyPart == hitBox.bodyPart)
                    {
                        hitBoxesList.Add(hitBox);
                        break;
                    }
                }
            }

            return hitBoxesList.ToArray();
        }

        public void ResetHit()
        {
            //if (!isHit) return;
            foreach (HitBox hitBox in hitBoxes)
            {
                if (hitBox.state == 1) hitBox.state = 0;
            }
            isHit = false;
        }

        public HitBox GetStrokeHitBox()
        {
            if (!isHit) return null;
            foreach (HitBox hitBox in hitBoxes)
            {
                if (hitBox.state == 1) return hitBox;
            }
            return null;
        }

        public void HideHitBoxes(HitBox[] invincibleHitBoxes, bool hide)
        {
            foreach (HitBox invHitBox in invincibleHitBoxes)
            {
                foreach (HitBox hitBox in hitBoxes)
                {
                    if (invHitBox.bodyPart == hitBox.bodyPart)
                    {
                        hitBox.hide = hide;
                        break;
                    }
                }
            }
        }

        public void HideHitBoxes(bool hide)
        {
            foreach (HitBox hitBox in hitBoxes)
            {
                hitBox.hide = hide;
            }
        }

        public void InvertHitBoxes(bool mirror)
        {
            if (inverted == mirror) return;
            inverted = mirror;

            foreach (HitBox hitBox in hitBoxes)
            {
                foreach (HitBox hitBox2 in hitBoxes)
                {
                    if ((hitBox.bodyPart == BodyPart.leftCalf && hitBox2.bodyPart == BodyPart.rightCalf) ||
                        (hitBox.bodyPart == BodyPart.leftFoot && hitBox2.bodyPart == BodyPart.rightFoot) ||
                        (hitBox.bodyPart == BodyPart.leftForearm && hitBox2.bodyPart == BodyPart.rightForearm) ||
                        (hitBox.bodyPart == BodyPart.leftHand && hitBox2.bodyPart == BodyPart.rightHand) ||
                        (hitBox.bodyPart == BodyPart.leftThigh && hitBox2.bodyPart == BodyPart.rightThigh) ||
                        (hitBox.bodyPart == BodyPart.leftUpperArm && hitBox2.bodyPart == BodyPart.rightUpperArm))
                        invertTransform(hitBox, hitBox2);
                }
            }
        }

        private void invertTransform(HitBox hb1, HitBox hb2)
        {
            Transform hb2Transform = hb2.position;
            hb2.position = hb1.position;
            hb1.position = hb2Transform;
        }

        public Transform FindTransform(string searchString)
        {
            Transform[] transformChildren = GetComponentsInChildren<Transform>();
            Transform found;
            foreach (Transform transformChild in transformChildren)
            {
                found = transformChild.Find("mixamorig:" + searchString);
                if (found == null) found = transformChild.Find(gameObject.name + ":" + searchString);
                if (found == null) found = transformChild.Find(searchString);
                if (found != null) return found;
            }
            return null;
        }

        public Rect GetBounds()
        {
            if (characterRenderer != null)
            {
                return new Rect(characterRenderer.bounds.min.x,
                                characterRenderer.bounds.min.y,
                                characterRenderer.bounds.max.x,
                                characterRenderer.bounds.max.y);
            }
            else
            {
                // alternative bounds
            }

            return new Rect();
        }

        public void UpdateMap()
        {
            UpdateMap(controlsScript.MoveSet.GetCurrentClipFrame(bakeSpeed));
        }

        public void UpdateMap(int frame)
        {
            if (customHitBoxes != null)
            {
                hitBoxes = GenerateHitBoxes(frame, customHitBoxes);
            }
            else
            {
                if (autoHitBoxes.Length > 0 && hitBoxes != autoHitBoxes) hitBoxes = autoHitBoxes;

                bool useAnimationMaps = true;
                if (controlsScript != null) useAnimationMaps = controlsScript.myInfo.useAnimationMaps;

                if (animationMaps == null && useAnimationMaps)
                {
                    Debug.LogWarning("Animation '" + moveSetScript.GetCurrentClipName() + "' has no animation maps.");
                    foreach (HitBox hitBox in hitBoxes)
                    {
                        hitBox.mappedPosition = new FPVector();
                    }
                    return;
                }

                if (useAnimationMaps)
                {
                    deltaPosition = new FPVector();
                    HitBoxMap[] hitBoxMaps = new HitBoxMap[0];
                    int highestFrame = 0;

                    foreach (AnimationMap map in animationMaps)
                    {
                        highestFrame = map.frame;
                        if (map.frame == frame)
                        {
                            hitBoxMaps = map.hitBoxMaps;
                            deltaPosition = map.deltaDisplacement;
                            break;
                        }
                    }

                    // If frame can't be found, cast the highest possible frame
                    if (hitBoxMaps.Length == 0 && animationMaps.Length > 0)
                    {
                        hitBoxMaps = animationMaps[highestFrame].hitBoxMaps;
                    }

                    foreach (HitBoxMap map in hitBoxMaps)
                    {
                        foreach (HitBox hitBox in hitBoxes)
                        {
                            if (hitBox.bodyPart == map.bodyPart)
                            {
                                hitBox.mappedPosition = map.mappedPosition;
                                hitBox.mappedPosition += hitBox._offSet;
                                hitBox.localPosition = hitBox.mappedPosition;

                                if (inverted)
                                {
                                    if (controlsScript != null && UFE.config.gameplayType != GameplayType._2DFighter && UFE.config.characterRotationOptions.autoMirror)
                                    {
                                        hitBox.mappedPosition.z *= -1;
                                    }
                                    else
                                    {
                                        hitBox.mappedPosition.x *= -1;
                                    }
                                }

                                if (controlsScript != null && UFE.config.gameplayType != GameplayType._2DFighter)
                                {
                                    // Rotate positions based on current worldTrasnform rotation
                                    hitBox.mappedPosition = (FPQuaternion.Euler(0, transform.rotation.eulerAngles.y - 90, 0) * hitBox.mappedPosition) + worldPosition;
                                }
                                else
                                {
                                    hitBox.mappedPosition += worldPosition;
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (HitBox hitBox in hitBoxes)
                    {
                        hitBox.mappedPosition = FPVector.ToFPVector(hitBox.position.position);
                        hitBox.mappedPosition += new FPVector(hitBox._offSet.x * -controlsScript.mirror, hitBox._offSet.y, 0);
                    }
                }
            }
        }

        public HitBox[] GenerateHitBoxes(int castingFrame, CustomHitBoxesInfo animatorInfo)
        {
            List<HitBox> hitBoxList = new List<HitBox>();
            foreach (CustomHitBox hitboxDef in animatorInfo.customHitBoxes)
            {
                if (hitboxDef == null) continue;
                if (castingFrame >= hitboxDef.activeFrames.Length) castingFrame = hitboxDef.activeFrames.Length - 1;

                if (hitboxDef.activeFrames == null ||
                    hitboxDef.activeFrames[castingFrame] == null ||
                    !hitboxDef.activeFrames[castingFrame].active) continue;

                HitBox hitBox = new HitBox();
                hitBox.position = controlsScript != null ? controlsScript.transform : transform;
                hitBox.collisionType = hitboxDef.collisionType;
                hitBox.type = hitboxDef.hitBoxType;
                hitBox.shape = hitboxDef.shape;
                hitBox._radius = hitboxDef.activeFrames[castingFrame].radius;

                Fix64 rectX = inverted ? -hitboxDef.activeFrames[castingFrame].cubeWidth : 0;
                hitBox._rect = new FPRect(rectX, 0, hitboxDef.activeFrames[castingFrame].cubeWidth, hitboxDef.activeFrames[castingFrame].cubeHeight);
                hitBox.rect = hitBox._rect.ToRect();

                hitBox.mappedPosition = hitboxDef.activeFrames[castingFrame].position;
                hitBox.localPosition = hitBox.mappedPosition;

                if (inverted)
                {
                    if (UFE.config.gameplayType != GameplayType._2DFighter && UFE.config.characterRotationOptions.autoMirror)
                    {
                        hitBox.mappedPosition.z *= -1;
                    }
                    else if (UFE.config.characterRotationOptions.autoMirror)
                    {
                        hitBox.mappedPosition.x *= -1;
                    }
                }

                if (controlsScript != null && UFE.config.gameplayType != GameplayType._2DFighter)
                {
                    // Rotate positions based on current worldTrasnform rotation
                    hitBox.localPosition = hitBox.mappedPosition;
                    hitBox.mappedPosition = (FPQuaternion.Euler(0, transform.rotation.eulerAngles.y - 90, 0) * hitBox.mappedPosition) + worldPosition;
                }
                else
                {
                    hitBox.mappedPosition += worldPosition;
                }

                hitBoxList.Add(hitBox);
            }

            return hitBoxList.ToArray();
        }

        public void UpdateBounds(HurtBox[] hurtBoxes)
        {
            foreach (HurtBox hurtBox in hurtBoxes) if (hurtBox.followXBounds || hurtBox.followYBounds) hurtBox.rendererBounds = GetBounds();
        }

        public void UpdateRenderer()
        {
            bool confirmUpdate = false;
            foreach (HitBox hitBox in hitBoxes)
            {
                if (hitBox.followXBounds || hitBox.followYBounds) confirmUpdate = true;
            }

            if (moveSetScript != null)
            {
                foreach (MoveInfo move in moveSetScript.moves)
                {
                    if (move == null)
                    {
                        Debug.LogWarning("You have empty entries in your move list. Check your special moves under Character Editor.");
                        continue;
                    }
                    foreach (Hit hit in move.hits)
                    {
                        foreach (HurtBox hurtbox in hit.hurtBoxes)
                        {
                            if (hurtbox.followXBounds || hurtbox.followYBounds) confirmUpdate = true;
                        }
                    }

                    if (move.blockableArea != null && (move.blockableArea.followXBounds || move.blockableArea.followYBounds))
                        confirmUpdate = true;
                }
            }

            if (confirmUpdate)
            {
                Renderer[] rendererList = GetComponentsInChildren<Renderer>();
                foreach (Renderer childRenderer in rendererList)
                {
                    characterRenderer = childRenderer;
                    return;
                }
                Debug.LogWarning("Warning: You are trying to access the character's bounds, but it does not have a renderer.");
            }
        }

        private void GizmosDrawRectangle(Vector3 topLeft, Vector3 bottomLeft, Vector3 bottomRight, Vector3 topRight)
        {
            Gizmos.DrawLine(topLeft, bottomLeft);
            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
        }

        private void GizmosDrawCuboid(Vector3 fTopLeft, Vector3 fBottomLeft, Vector3 fBottomRight, Vector3 fTopRight, Vector3 bTopLeft, Vector3 bBottomLeft, Vector3 bBottomRight, Vector3 bTopRight)
        {
            // Front
            Gizmos.DrawLine(fTopLeft, fBottomLeft);
            Gizmos.DrawLine(fBottomLeft, fBottomRight);
            Gizmos.DrawLine(fBottomRight, fTopRight);
            Gizmos.DrawLine(fTopRight, fTopLeft);

            //Back
            Gizmos.DrawLine(bTopLeft, bBottomLeft);
            Gizmos.DrawLine(bBottomLeft, bBottomRight);
            Gizmos.DrawLine(bBottomRight, bTopRight);
            Gizmos.DrawLine(bTopRight, bTopLeft);

            // Connect
            Gizmos.DrawLine(fTopLeft, bTopLeft);
            Gizmos.DrawLine(fBottomLeft, bBottomLeft);
            Gizmos.DrawLine(fBottomRight, bBottomRight);
            Gizmos.DrawLine(fTopRight, bTopRight);
        }

        void OnDrawGizmos()
        {
            // HITBOXES
            if (hitBoxes == null) return;
            int mirrorAdjust = controlsScript != null ? controlsScript.mirror : -1;
            Vector3 rootPosition = worldPosition.ToVector();

            Gizmos.color = Color.red;
            FPVector distance = new FPVector(30, 0, 0);

            /*if (UFE.config != null && UFE.config.gameplayType != GameplayType._2DFighter)
            {
                FPVector target = (FPQuaternion.Euler(0, transform.rotation.eulerAngles.y - 90, 0) * distance) + worldPosition;
                Gizmos.DrawWireSphere(target.ToVector(), 1f);
            }*/

            foreach (HitBox hitBox in hitBoxes)
            {
                if (hitBox.position == null) continue;
                if (hitBox.hide) continue;

                bool fillBox = false;
                if (hitBox.state == 1)
                {
                    Gizmos.color = Color.red;
                }
                else if (isHit)
                {
                    Gizmos.color = Color.magenta;
                }
                else if (hitBox.collisionType == CollisionType.bodyCollider)
                {
                    Gizmos.color = UFE.GetActiveConfig().colorBodyCollider;
                    fillBox = UFE.GetActiveConfig().colorBodyColliderFill;
                }
                else if (hitBox.collisionType == CollisionType.noCollider)
                {
                    Gizmos.color = UFE.GetActiveConfig().colorNoCollider;
                    fillBox = UFE.GetActiveConfig().colorNoColliderFill;
                }
                else if (hitBox.collisionType == CollisionType.throwCollider)
                {
                    Gizmos.color = UFE.GetActiveConfig().colorThrowCollider;
                    fillBox = UFE.GetActiveConfig().colorThrowColliderFill;
                }
                else if (hitBox.collisionType == CollisionType.physicalInvincibleCollider)
                {
                    Gizmos.color = UFE.GetActiveConfig().colorPhysicalInvincibleCollider;
                    fillBox = UFE.GetActiveConfig().colorPhysicalInvincibleColliderFill;
                }
                else if (hitBox.collisionType == CollisionType.projectileInvincibleCollider)
                {
                    Gizmos.color = UFE.GetActiveConfig().colorProjectileInvincibleCollider;
                    fillBox = UFE.GetActiveConfig().colorProjectileInvincibleColliderFill;
                }
                else
                {
                    Gizmos.color = Color.green;
                }

                Vector3 currentPosition = hitBox.mappedPosition.ToVector();


                //if (UFE.config.characterRotationOptions.alwaysLookAtOpponent)
                //currentPosition = (Quaternion.Euler(0, (float)worldTransform.rotation.eulerAngles.y - 90, 0) * hitBox.localPosition.ToVector()) + rootPosition;

                if (animationMaps == null)
                {
                    currentPosition = hitBox.position.position;
                    currentPosition += new Vector3((float)hitBox._offSet.x * -mirrorAdjust, (float)hitBox._offSet.y, 0);
                }

                if (hitBox.shape == HitBoxShape.rectangle && rectangleHitBoxLocationTest)
                {
                    Rect hitBoxRectPos = hitBox._rect.ToRect();
                    hitBoxRectPos.x *= -mirrorAdjust;
                    hitBoxRectPos.width *= -mirrorAdjust;

                    hitBoxRectPos.x += currentPosition.x;
                    hitBoxRectPos.y += currentPosition.y;
                    Gizmos.DrawGUITexture(hitBoxRectPos, rectTexture);
                }

                Vector3 hitBoxPosition = currentPosition;
                if (UFE.GetActiveConfig() == null || !UFE.GetActiveConfig().detect3D_Hits) hitBoxPosition.z = -1;
                if (hitBox.shape == HitBoxShape.circle && hitBox._radius > 0)
                {
                    if (fillBox) Gizmos.DrawSphere(hitBoxPosition, (float)hitBox._radius);

                    Gizmos.DrawWireSphere(hitBoxPosition, (float)hitBox._radius);
                }
                else if (hitBox.shape == HitBoxShape.rectangle)
                {
                    Rect hitBoxRectPosTemp = hitBox._rect.ToRect();

                    if (UFE.config.gameplayType != GameplayType._2DFighter)
                    {
                        hitBoxRectPosTemp.x += hitBox.mappedPosition.ToVector().x;
                        hitBoxRectPosTemp.y += hitBox.mappedPosition.ToVector().y;
                    }
                    else
                    {
                        hitBoxRectPosTemp.x += currentPosition.x;
                        hitBoxRectPosTemp.y += currentPosition.y;
                    }

                    Vector3 topLeft = new Vector3(hitBoxRectPosTemp.x, hitBoxRectPosTemp.y, hitBoxPosition.z);
                    Vector3 topRight = new Vector3(hitBoxRectPosTemp.xMax, hitBoxRectPosTemp.y, hitBoxPosition.z);
                    Vector3 bottomLeft = new Vector3(hitBoxRectPosTemp.x, hitBoxRectPosTemp.yMax, hitBoxPosition.z);
                    Vector3 bottomRight = new Vector3(hitBoxRectPosTemp.xMax, hitBoxRectPosTemp.yMax, hitBoxPosition.z);

                    if (UFE.config.gameplayType != GameplayType._2DFighter)
                    {
                        topLeft.z = -.4f;
                        topRight.z = -.4f;
                        bottomLeft.z = -.4f;
                        bottomRight.z = -.4f;

                        topLeft = (Quaternion.Euler(0, transform.rotation.eulerAngles.y - 90, 0) * topLeft) + rootPosition;
                        topRight = (Quaternion.Euler(0, transform.rotation.eulerAngles.y - 90, 0) * topRight) + rootPosition;
                        bottomLeft = (Quaternion.Euler(0, transform.rotation.eulerAngles.y - 90, 0) * bottomLeft) + rootPosition;
                        bottomRight = (Quaternion.Euler(0, transform.rotation.eulerAngles.y - 90, 0) * bottomRight) + rootPosition;

                        Vector3 vtopLeft = new Vector3(hitBoxRectPosTemp.x, hitBoxRectPosTemp.y, .4f);
                        Vector3 vtopRight = new Vector3((hitBoxRectPosTemp.xMax), hitBoxRectPosTemp.y, .4f);
                        Vector3 vbottomLeft = new Vector3(hitBoxRectPosTemp.x, hitBoxRectPosTemp.yMax, .4f);
                        Vector3 vbottomRight = new Vector3((hitBoxRectPosTemp.xMax), hitBoxRectPosTemp.yMax, .4f);

                        vtopLeft = (Quaternion.Euler(0, transform.rotation.eulerAngles.y - 90, 0) * vtopLeft) + rootPosition;
                        vtopRight = (Quaternion.Euler(0, transform.rotation.eulerAngles.y - 90, 0) * vtopRight) + rootPosition;
                        vbottomLeft = (Quaternion.Euler(0, transform.rotation.eulerAngles.y - 90, 0) * vbottomLeft) + rootPosition;
                        vbottomRight = (Quaternion.Euler(0, transform.rotation.eulerAngles.y - 90, 0) * vbottomRight) + rootPosition;

                        // Draw Volume
                        GizmosDrawRectangle(vtopLeft, vbottomLeft, vbottomRight, vtopRight);
                        Gizmos.DrawLine(topLeft, vtopLeft);
                        Gizmos.DrawLine(bottomLeft, vbottomLeft);
                        Gizmos.DrawLine(bottomRight, vbottomRight);
                        Gizmos.DrawLine(topRight, vtopRight);
                    }


                    if (hitBox.followXBounds)
                    {
                        hitBox.rect.x = 0;
                        topLeft.x = GetBounds().x - (hitBox.rect.width / 2);
                        topRight.x = GetBounds().width + (hitBox.rect.width / 2);
                        bottomLeft.x = GetBounds().x - (hitBox.rect.width / 2);
                        bottomRight.x = GetBounds().width + (hitBox.rect.width / 2);
                    }

                    if (hitBox.followYBounds)
                    {
                        hitBox.rect.y = 0;
                        topLeft.y = GetBounds().height + (hitBox.rect.height / 2);
                        topRight.y = GetBounds().height + (hitBox.rect.height / 2);
                        bottomLeft.y = GetBounds().y - (hitBox.rect.height / 2);
                        bottomRight.y = GetBounds().y - (hitBox.rect.height / 2);
                    }

                    if (fillBox)
                    {
                        float centerX = (topRight.x + topLeft.x) / 2;
                        float centerY = (topRight.y + bottomRight.y) / 2;
                        float width = (topRight.x - topLeft.x);
                        float height = (topRight.y - bottomRight.y);

                        if (width < 0)
                            width *= -1;
                        if (height < 0)
                            height *= -1;

                        Vector3 center = new Vector3(centerX, centerY, transform.position.z);
                        Vector3 size = new Vector3(width, height);
                        Gizmos.DrawCube(center, size);
                    }

                    GizmosDrawRectangle(topLeft, bottomLeft, bottomRight, topRight);

                    hitBoxPosition.x = (topLeft.x + topRight.x) / 2;
                    hitBoxPosition.y = (topLeft.y + bottomLeft.y) / 2;
                }

                if (hitBox.collisionType != CollisionType.noCollider)
                {
                    if (hitBox.type == HitBoxType.low)
                    {
                        Gizmos.color = Color.red;
                    }
                    else
                    {
                        Gizmos.color = Color.yellow;
                    }
                    Gizmos.DrawWireSphere(hitBoxPosition, .1f);
                }
            }


            // COLLISION BOX SIZE
            if (previewAllBoxes)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, (float)collisionBoxSize);
            }


            // HURTBOXES
            if (activeHurtBoxes != null)
            {
                bool fillBox = false;
                if (hitConfirmType == HitConfirmType.Throw)
                {
                    Gizmos.color = UFE.GetActiveConfig().colorHurtBoxThrow;
                    fillBox = UFE.GetActiveConfig().colorHurtBoxThrowFill;
                }
                else
                {
                    Gizmos.color = UFE.GetActiveConfig().colorHurtBoxNotThrow;
                    fillBox = UFE.GetActiveConfig().colorHurtBoxNotThrowFill;
                }

                foreach (HurtBox hurtBox in activeHurtBoxes)
                {
                    Vector3 hurtBoxPosition = hurtBox.position.ToVector();

                    if (UFE.GetActiveConfig() == null || !UFE.GetActiveConfig().detect3D_Hits) hurtBoxPosition.z = -1;

                    if (hurtBox.shape == HitBoxShape.circle)
                    {
                        hurtBoxPosition += new Vector3((float)hurtBox._offSet.x * -mirrorAdjust, (float)hurtBox._offSet.y, 0);

                        if (fillBox) Gizmos.DrawSphere(hurtBoxPosition, (float)hurtBox._radius);
                        Gizmos.DrawWireSphere(hurtBoxPosition, (float)hurtBox._radius);
                    }
                    else
                    {
                        Vector3 topLeft = new Vector3();
                        Vector3 topRight = new Vector3();
                        Vector3 bottomLeft = new Vector3();
                        Vector3 bottomRight = new Vector3();

                        if (UFE.config.gameplayType == GameplayType._2DFighter)
                        {
                            topLeft = new Vector3((float)hurtBox._rect.x * -mirrorAdjust, (float)hurtBox._rect.y) + hurtBoxPosition;
                            topRight = new Vector3((float)(hurtBox._rect.x + hurtBox._rect.width) * -mirrorAdjust, (float)hurtBox._rect.y) + hurtBoxPosition;
                            bottomLeft = new Vector3((float)(hurtBox._rect.x * -mirrorAdjust), (float)(hurtBox._rect.y + hurtBox._rect.height)) + hurtBoxPosition;
                            bottomRight = new Vector3((float)(hurtBox._rect.x + hurtBox._rect.width) * -mirrorAdjust, (float)(hurtBox._rect.y + hurtBox._rect.height)) + hurtBoxPosition;
                        }
                        else
                        {
                            Rect hurtBoxRectPosTemp = hurtBox._rect.ToRect();
                            hurtBoxRectPosTemp.x += GetPosition(hurtBox.bodyPart, true).ToVector().x;
                            hurtBoxRectPosTemp.y += GetPosition(hurtBox.bodyPart, true).ToVector().y;

                            topLeft = new Vector3(hurtBoxRectPosTemp.x, hurtBoxRectPosTemp.y, -.4f);
                            topRight = new Vector3(hurtBoxRectPosTemp.xMax, hurtBoxRectPosTemp.y, -.4f);
                            bottomLeft = new Vector3(hurtBoxRectPosTemp.x, hurtBoxRectPosTemp.yMax, -.4f);
                            bottomRight = new Vector3(hurtBoxRectPosTemp.xMax, hurtBoxRectPosTemp.yMax, -.4f);

                            topLeft = (Quaternion.Euler(0, transform.rotation.eulerAngles.y - 90, 0) * topLeft) + rootPosition;
                            topRight = (Quaternion.Euler(0, transform.rotation.eulerAngles.y - 90, 0) * topRight) + rootPosition;
                            bottomLeft = (Quaternion.Euler(0, transform.rotation.eulerAngles.y - 90, 0) * bottomLeft) + rootPosition;
                            bottomRight = (Quaternion.Euler(0, transform.rotation.eulerAngles.y - 90, 0) * bottomRight) + rootPosition;

                            Vector3 vtopLeft = new Vector3(hurtBoxRectPosTemp.x, hurtBoxRectPosTemp.y, .4f);
                            Vector3 vtopRight = new Vector3(hurtBoxRectPosTemp.xMax, hurtBoxRectPosTemp.y, .4f);
                            Vector3 vbottomLeft = new Vector3(hurtBoxRectPosTemp.x, hurtBoxRectPosTemp.yMax, .4f);
                            Vector3 vbottomRight = new Vector3(hurtBoxRectPosTemp.xMax, hurtBoxRectPosTemp.yMax, .4f);

                            vtopLeft = (Quaternion.Euler(0, transform.rotation.eulerAngles.y - 90, 0) * vtopLeft) + rootPosition;
                            vtopRight = (Quaternion.Euler(0, transform.rotation.eulerAngles.y - 90, 0) * vtopRight) + rootPosition;
                            vbottomLeft = (Quaternion.Euler(0, transform.rotation.eulerAngles.y - 90, 0) * vbottomLeft) + rootPosition;
                            vbottomRight = (Quaternion.Euler(0, transform.rotation.eulerAngles.y - 90, 0) * vbottomRight) + rootPosition;

                            // Draw Volume
                            GizmosDrawRectangle(vtopLeft, vbottomLeft, vbottomRight, vtopRight);
                            Gizmos.DrawLine(topLeft, vtopLeft);
                            Gizmos.DrawLine(bottomLeft, vbottomLeft);
                            Gizmos.DrawLine(bottomRight, vbottomRight);
                            Gizmos.DrawLine(topRight, vtopRight);
                        }

                        if (hurtBox.followXBounds)
                        {
                            hurtBox._rect.x = 0;
                            topLeft.x = GetBounds().x - (float)(hurtBox._rect.width / 2);
                            topRight.x = GetBounds().width + (float)(hurtBox._rect.width / 2);
                            bottomLeft.x = GetBounds().x - (float)(hurtBox._rect.width / 2);
                            bottomRight.x = GetBounds().width + (float)(hurtBox._rect.width / 2);
                        }

                        if (hurtBox.followYBounds)
                        {
                            hurtBox._rect.y = 0;
                            topLeft.y = GetBounds().height + (float)(hurtBox._rect.height / 2);
                            topRight.y = GetBounds().height + (float)(hurtBox._rect.height / 2);
                            bottomLeft.y = GetBounds().y - (float)(hurtBox._rect.height / 2);
                            bottomRight.y = GetBounds().y - (float)(hurtBox._rect.height / 2);
                        }

                        if (fillBox)
                        {
                            float centerX = (topRight.x + topLeft.x) / 2;
                            float centerY = (topRight.y + bottomRight.y) / 2;
                            float width = (topRight.x - topLeft.x);
                            float height = (topRight.y - bottomRight.y);

                            if (width < 0)
                                width *= -1;
                            if (height < 0)
                                height *= -1;

                            Vector3 center = new Vector3(centerX, centerY);
                            Vector3 size = new Vector3(width, height);
                            Gizmos.DrawCube(center, size);
                        }

                        GizmosDrawRectangle(topLeft, bottomLeft, bottomRight, topRight);
                    }
                }
            }


            // BLOCKBOXES
            if (blockableArea != null && blockableArea.enabled && previewAllBoxes)
            {
                Gizmos.color = UFE.GetActiveConfig().colorBlockBox;
                bool fillBox = UFE.GetActiveConfig().colorBlockBoxFill;

                Vector3 blockableAreaPosition = blockableArea.position.ToVector();

                if (UFE.GetActiveConfig() == null || !UFE.GetActiveConfig().detect3D_Hits) blockableAreaPosition.z = -1;

                if (blockableArea.shape == HitBoxShape.circle)
                {
                    blockableAreaPosition += new Vector3((float)blockableArea._offSet.x * -mirrorAdjust, (float)blockableArea._offSet.y, 0);

                    if (fillBox) Gizmos.DrawSphere(blockableAreaPosition, (float)blockableArea._radius);

                    Gizmos.DrawWireSphere(blockableAreaPosition, (float)blockableArea._radius);
                }
                else
                {
                    Vector3 topLeft = new Vector3(blockableArea.rect.x * -mirrorAdjust, blockableArea.rect.y) + blockableAreaPosition;
                    Vector3 topRight = new Vector3((blockableArea.rect.x + blockableArea.rect.width) * -mirrorAdjust, blockableArea.rect.y) + blockableAreaPosition;
                    Vector3 bottomLeft = new Vector3(blockableArea.rect.x * -mirrorAdjust, blockableArea.rect.y + blockableArea.rect.height) + blockableAreaPosition;
                    Vector3 bottomRight = new Vector3((blockableArea.rect.x + blockableArea.rect.width) * -mirrorAdjust, blockableArea.rect.y + blockableArea.rect.height) + blockableAreaPosition;

                    if (fillBox)
                    {
                        float centerX = (topRight.x + topLeft.x) / 2;
                        float centerY = (topRight.y + bottomRight.y) / 2;
                        float width = (topRight.x - topLeft.x);
                        float height = (topRight.y - bottomRight.y);

                        if (width < 0)
                            width *= -1;
                        if (height < 0)
                            height *= -1;

                        Vector3 center = new Vector3(centerX, centerY);
                        Vector3 size = new Vector3(width, height);
                        Gizmos.DrawCube(center, size);
                    }
                    GizmosDrawRectangle(topLeft, bottomLeft, bottomRight, topRight);
                }
            }
        }
    }
}