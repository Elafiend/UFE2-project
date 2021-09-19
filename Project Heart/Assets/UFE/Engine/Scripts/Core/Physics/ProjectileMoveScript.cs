using UnityEngine;
using UFENetcode;
using FPLibrary;
using System.Collections.Generic;

namespace UFE3D
{
    public class ProjectileMoveScript : UFEBehaviour, UFEInterface
    {
        public ControlsScript opControlsScript { get { return this.myControlsScript.opControlsScript; } }

        public Projectile data;
        public int mirror = -1;
        public ControlsScript myControlsScript;
        public HurtBox hurtBox;
        public HitBox hitBox;
        public BlockArea blockableArea;


        [RecordVar] public Renderer projectileRenderer;
        [RecordVar] public Fix64 isHit = 0;
        [RecordVar] public int totalHits = 1;
        [RecordVar] public bool destroyMe;
        [RecordVar] public bool onView;
        [RecordVar] public Fix64 verticalForce;
        [RecordVar] public FPVector fpPos { get { return fpTransform.position; } set { fpTransform.position = value; } }
        [RecordVar] public FPVector target;


        // Runtime properties which are only modified when the projectile is instantiated.
        [HideInInspector] public FPVector directionVector = new FPVector(1, 0, 0);
        [HideInInspector] public FPVector movement;
        [HideInInspector] public Fix64 spaceBetweenHits = .1;
        [HideInInspector] public Hit hit;
        [HideInInspector] public FPTransform fpTransform;

        void Start()
        {
            //gameObject.AddComponent<SphereCollider>();
        }

        public void Init()
        {
            if (mirror == 1) directionVector.x = -1;

            totalHits = data.totalHits;

            // If not using gravity, use direction and speed
            Fix64 angleRad = ((Fix64)data.directionAngle / 180) * FPMath.Pi;
            movement = ((FPMath.Sin(angleRad) * FPVector.up) + (FPMath.Cos(angleRad) * directionVector)) * data.speed;
            fpTransform.Translate(new FPVector(data._castingOffSet.x * -mirror, data._castingOffSet.y, data._castingOffSet.z));

            // if gravity is applied, use regular force values
            verticalForce = data.forceApplied.y;

            // Create Blockable Area
            blockableArea = new BlockArea();
            blockableArea = data.blockableArea;

            // Create Hurtbox
            hurtBox = new HurtBox();
            hurtBox = data.hurtBox;
            hurtBox.type = HurtBoxType.projectile;

            // Create Hitbox
            hitBox = new HitBox();
            hitBox.shape = hurtBox.shape;
            hitBox._rect = hurtBox._rect;
            hitBox.followXBounds = hurtBox.followXBounds;
            hitBox.followYBounds = hurtBox.followYBounds;
            hitBox._radius = hurtBox._radius;
            hitBox._offSet = hurtBox._offSet;
            hitBox.position = gameObject.transform;

            UpdateRenderer();

            if (data.spaceBetweenHits == Sizes.VerySmall)
            {
                spaceBetweenHits = .1;
            }
            else if (data.spaceBetweenHits == Sizes.Small)
            {
                spaceBetweenHits = .15;
            }
            else if (data.spaceBetweenHits == Sizes.Medium)
            {
                spaceBetweenHits = .2;
            }
            else if (data.spaceBetweenHits == Sizes.High)
            {
                spaceBetweenHits = .3;
            }
            else if (data.spaceBetweenHits == Sizes.VeryHigh)
            {
                spaceBetweenHits = .4;
            }


            // Create Hit data
            hit = new Hit();
            hit.hitType = data.hitType;
            hit.spaceBetweenHits = data.spaceBetweenHits;
            hit.hitStrength = data.hitStrength;
            hit.hitStunType = HitStunType.Frames;
            hit._hitStunOnHit = data.hitStunOnHit;
            hit._hitStunOnBlock = data.hitStunOnBlock;
            hit._damageOnHit = data._damageOnHit;
            hit._damageOnBlock = data._damageOnBlock;
            hit.damageScaling = data.damageScaling;
            hit.damageType = data.damageType;
            hit.groundHit = data.groundHit;
            hit.airHit = data.airHit;
            hit.downHit = data.downHit;
            hit.overrideHitEffects = data.overrideHitEffects;
            hit.fixRotation = data.fixRotation;
            hit.armorBreaker = data.armorBreaker;
            hit.hitEffects = data.hitEffects;
            hit.resetPreviousHorizontalPush = data.resetPreviousHorizontalPush;
            hit.resetPreviousVerticalPush = data.resetPreviousVerticalPush;
            hit.applyDifferentAirForce = data.applyDifferentAirForce;
            hit.applyDifferentBlockForce = data.applyDifferentBlockForce;
            hit._pushForce = data._pushForce;
            hit._pushForceAir = data._pushForceAir;
            hit._pushForceBlock = data._pushForceBlock;
            hit.pullEnemyIn = new PullIn();
            hit.pullSelfIn = new PullIn();

            if (data.mirrorOn2PSide && mirror > 0)
            {
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y + 180, transform.localEulerAngles.z);
            }

            if (UFE.config.gameplayType != GameplayType._2DFighter)
            {
                movement.x *= -mirror;
                FPVector distance = new FPVector(30, 0, 0);
                distance += movement;
                target = (FPQuaternion.Euler(0, myControlsScript.transform.rotation.eulerAngles.y - 90, 0) * distance) + fpTransform.position;
            }
        }

        public void UpdateRenderer()
        {
            if (hurtBox.followXBounds || hurtBox.followYBounds)
            {
                Renderer[] rendererList = GetComponentsInChildren<Renderer>();
                foreach (Renderer childRenderer in rendererList)
                {
                    projectileRenderer = childRenderer;
                }
                if (projectileRenderer == null)
                    Debug.LogWarning("Warning: You are trying to access the projectile's bounds, but it does not have a renderer.");

            }
        }

        public bool IsDestroyed()
        {
            if (this == null) return true;
            if (destroyMe)
                UFE.DestroyGameObject(gameObject);

            return destroyMe;
        }

        public override void UFEFixedUpdate()
        {
            if (!this.isActiveAndEnabled || destroyMe)
            {
                return;
            }

            if (isHit > 0)
            {
                isHit -= UFE.fixedDeltaTime;
                return;
            }

            // Check if both controllers are ready
            if (UFE.freezePhysics) return;


            // Update Position
            if (data.applyGravity)
            {
                verticalForce -= (UFE.config._gravity * data.weight) * UFE.fixedDeltaTime;
                FPVector newPosition = fpTransform.position;

                if (UFE.config.gameplayType == GameplayType._2DFighter)
                    newPosition.x += data.forceApplied.x * UFE.fixedDeltaTime * -mirror;
                else
                    newPosition = FPVector.MoveTowards(fpTransform.position, target, data.forceApplied.x * UFE.fixedDeltaTime);

                newPosition.y += verticalForce * UFE.fixedDeltaTime;
                fpTransform.position = newPosition;
            }
            else
            {
                if (UFE.config.gameplayType == GameplayType._2DFighter)
                {
                    fpTransform.position += (movement * UFE.fixedDeltaTime);
                }
                else
                {
                    FPVector newPosition = fpTransform.position;
                    newPosition = FPVector.MoveTowards(fpTransform.position, target, movement.x * UFE.fixedDeltaTime);

                    newPosition.y += movement.y * UFE.fixedDeltaTime;
                    fpTransform.position = newPosition;
                }
            }



            // Get Auto Bounds
            hurtBox.position = fpTransform.position;
            hitBox.mappedPosition = fpTransform.position;
            if (projectileRenderer != null && (hurtBox.followXBounds || hurtBox.followYBounds))
            {
                //hurtBox.rendererBounds = GetBounds();
                //hitBox.rendererBounds = GetBounds();
            }


            // Check Block Area Contact
            blockableArea.position = fpTransform.position;
            if (!opControlsScript.isBlocking
                && !opControlsScript.blockStunned
                && opControlsScript.currentSubState != SubStates.Stunned
                && CollisionManager.TestCollision(opControlsScript.HitBoxes.hitBoxes, blockableArea, opControlsScript.mirror > 0, mirror > 0).Length > 0)
            {
                opControlsScript.CheckBlocking(true);
            }


            // Test Collision with Opponent and their Projectiles 
            List<ControlsScript> allOpCScripts = UFE.GetAllControlsScriptsByPlayer(opControlsScript.playerNum);
            if (data.projectileCollision)
                foreach (ControlsScript cScript in allOpCScripts) IsCollidingProjectile(cScript);
            foreach (ControlsScript cScript in allOpCScripts) IsCollidingCharacter(cScript);


            // Test out of bounds - stage
            if (fpTransform.position.x > UFE.config.selectedStage.position.x + UFE.config.selectedStage._rightBoundary + 5
                || fpTransform.position.x < UFE.config.selectedStage.position.x + UFE.config.selectedStage._leftBoundary - 5)
            {
                destroyMe = true;
                return;
            }


            // Test out of bounds - camera
            Fix64 centerX = ((opControlsScript.worldTransform.position.x + myControlsScript.worldTransform.position.x) / 2);
            Fix64 leftCameraBounds = centerX - (UFE.config.cameraOptions._maxDistance / 2) - data.cameraBoundsOffSet;
            Fix64 rightCameraBounds = centerX + (UFE.config.cameraOptions._maxDistance / 2) + data.cameraBoundsOffSet;

            if (fpTransform.position.x < leftCameraBounds || fpTransform.position.x > rightCameraBounds)
            {
                if (data.destroyWhenOffCameraBounds) destroyMe = true;
                onView = false;
            }
            else
            {
                onView = true;
            }

            // Update Unity Transform
            transform.position = fpTransform.position.ToVector();
        }


        public void IsCollidingProjectile(ControlsScript opControlsScript)
        {
            if (opControlsScript.projectiles.Count > 0)
            {
                foreach (ProjectileMoveScript projectile in opControlsScript.projectiles)
                {
                    if (projectile == null) continue;
                    if (!projectile.isActiveAndEnabled) continue;
                    if (projectile.hitBox == null) continue;
                    if (projectile.hurtBox == null) continue;

                    if (CollisionManager.TestCollision(new HitBox[] { projectile.hitBox }, new HurtBox[] { hurtBox }, HitConfirmType.Hit, projectile.mirror > 0, mirror > 0).Length > 0)
                    {
                        ProjectileHit();
                        projectile.ProjectileHit();
                        break;
                    }
                }
            }
        }

        public void IsCollidingCharacter(ControlsScript opControlsScript)
        {
            FPVector[] collisionVectors = CollisionManager.TestCollision(opControlsScript.HitBoxes.hitBoxes, new HurtBox[] { hurtBox }, HitConfirmType.Hit, false, mirror > 0);
            if (collisionVectors.Length > 0 && opControlsScript.ValidateHit(hit))
            {
                ProjectileHit();

                if (opControlsScript.currentSubState != SubStates.Stunned && !data.unblockable && opControlsScript.isBlocking && opControlsScript.TestBlockStances(hit.hitType))
                {
                    myControlsScript.AddGauge(data.gaugeGainOnBlock, 0);
                    opControlsScript.AddGauge(data.opGaugeGainOnBlock, 0);
                    opControlsScript.GetHitBlocking(hit, 20, collisionVectors, data.obeyDirectionalHit, myControlsScript);

                    if (data.moveLinkOnBlock != null)
                        myControlsScript.CastMove(data.moveLinkOnBlock, true, data.forceGrounded);

                }
                else if (opControlsScript.potentialParry > 0 && opControlsScript.TestParryStances(hit.hitType))
                {
                    opControlsScript.AddGauge(data.opGaugeGainOnParry, 0);
                    opControlsScript.GetHitParry(hit, 20, collisionVectors, myControlsScript);

                    if (data.moveLinkOnParry != null)
                        myControlsScript.CastMove(data.moveLinkOnParry, true, data.forceGrounded);

                }
                else
                {
                    myControlsScript.AddGauge(data.gaugeGainOnHit, 0);
                    opControlsScript.AddGauge(data.opGaugeGainOnHit, 0);

                    if (data.hitEffectsOnHit)
                    {
                        opControlsScript.GetHit(hit, 30, collisionVectors, data.obeyDirectionalHit, myControlsScript);
                    }
                    else
                    {
                        opControlsScript.GetHit(hit, 30, new FPVector[0], data.obeyDirectionalHit, myControlsScript);
                    }

                    if (data.moveLinkOnStrike != null)
                        myControlsScript.CastMove(data.moveLinkOnStrike, true, data.forceGrounded);

                }

                opControlsScript.CheckBlocking(false);
            }
        }

        public void ProjectileHit()
        {
            if (data.impactPrefab != null)
            {
                string uniqueId = data.impactPrefab.name + myControlsScript.playerNum.ToString() + UFE.currentFrame;
                UFE.SpawnGameObject(data.impactPrefab, fpTransform.position.ToVector(), Quaternion.Euler(0, 0, data.directionAngle), Mathf.RoundToInt(data.impactDuration * UFE.config.fps), false, uniqueId);
            }
            totalHits--;
            if (totalHits <= 0) destroyMe = true;

            isHit = spaceBetweenHits;
            fpTransform.Translate(movement * -1 * UFE.fixedDeltaTime);
        }


        public Rect GetBounds()
        {
            if (projectileRenderer != null)
            {
                return new Rect(projectileRenderer.bounds.min.x,
                                projectileRenderer.bounds.min.y,
                                projectileRenderer.bounds.max.x,
                                projectileRenderer.bounds.max.y);
            }
            else
            {
                // alternative bounds
            }

            return new Rect();
        }

        private void GizmosDrawRectangle(Vector3 topLeft, Vector3 bottomLeft, Vector3 bottomRight, Vector3 topRight)
        {
            Gizmos.DrawLine(topLeft, bottomLeft);
            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
        }

        void OnDrawGizmos()
        {
            // COLLISION BOX SIZE
            // HURTBOXES
            if (hurtBox != null)
            {
                Gizmos.color = Color.cyan;

                Vector3 hurtBoxPosition = transform.position;
                if (UFE.config == null || !UFE.config.detect3D_Hits) hurtBoxPosition.z = -1;

                if (hurtBox.shape == HitBoxShape.circle)
                {
                    hurtBoxPosition += new Vector3((float)hurtBox._offSet.x * -mirror, (float)hurtBox._offSet.y, 0);
                    Gizmos.DrawWireSphere(hurtBoxPosition, (float)hurtBox._radius);
                }
                else
                {
                    Vector3 topLeft = new Vector3((float)hurtBox._rect.x * -mirror, (float)hurtBox._rect.y) + hurtBoxPosition;
                    Vector3 topRight = new Vector3((float)(hurtBox._rect.x + hurtBox._rect.width) * -mirror, (float)hurtBox._rect.y) + hurtBoxPosition;
                    Vector3 bottomLeft = new Vector3((float)hurtBox._rect.x * -mirror, (float)(hurtBox._rect.y + hurtBox._rect.height)) + hurtBoxPosition;
                    Vector3 bottomRight = new Vector3((float)(hurtBox._rect.x + hurtBox._rect.width) * -mirror, (float)(hurtBox._rect.y + hurtBox._rect.height)) + hurtBoxPosition;

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
                    GizmosDrawRectangle(topLeft, bottomLeft, bottomRight, topRight);
                }
            }

            // BLOCKBOXES
            if (blockableArea != null)
            {
                Gizmos.color = Color.blue;

                if (!data.unblockable)
                {
                    Vector3 blockableAreaPosition;
                    blockableAreaPosition = transform.position;
                    if (UFE.config == null || !UFE.config.detect3D_Hits) blockableAreaPosition.z = -1;
                    if (blockableArea.shape == HitBoxShape.circle)
                    {
                        blockableAreaPosition += new Vector3((float)blockableArea._offSet.x * -mirror, (float)blockableArea._offSet.y, 0);
                        Gizmos.DrawWireSphere(blockableAreaPosition, (float)blockableArea._radius);
                    }
                    else
                    {
                        Vector3 topLeft = new Vector3(blockableArea.rect.x * -mirror, blockableArea.rect.y) + blockableAreaPosition;
                        Vector3 topRight = new Vector3((blockableArea.rect.x + blockableArea.rect.width) * -mirror, blockableArea.rect.y) + blockableAreaPosition;
                        Vector3 bottomLeft = new Vector3(blockableArea.rect.x * -mirror, blockableArea.rect.y + blockableArea.rect.height) + blockableAreaPosition;
                        Vector3 bottomRight = new Vector3((blockableArea.rect.x + blockableArea.rect.width) * -mirror, blockableArea.rect.y + blockableArea.rect.height) + blockableAreaPosition;
                        GizmosDrawRectangle(topLeft, bottomLeft, bottomRight, topRight);
                    }
                }
            }
        }
    }
}