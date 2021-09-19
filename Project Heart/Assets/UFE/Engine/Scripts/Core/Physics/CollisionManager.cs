using FPLibrary;

namespace UFE3D
{
    public static class CollisionManager
    {
        #region public methods
        /// <summary>
        /// Test Collision between Hit Boxes and Blockable Areas.
        /// <returns>
        /// Returns an array with 3 vectors:<br/>
        /// 0 - The blockable area position.<br/>
        /// 1 - The position of the hitbox position that collided with it.<br/>
        /// 2 - The average distance between both vectors.<br/>
        /// </returns>
        /// </summary>
        /// <param name="hitBoxes">Array of hitboxes.</param>
        /// <param name="blockableArea">Blockable area to be tested against.</param>
        /// <param name="invertHitBoxes">Mirror hitboxes positions horizontally.</param>
        /// <param name="invertBlockableArea">Mirror blockable area horizontally.</param>
        public static FPVector[] TestCollision(HitBox[] hitBoxes, BlockArea blockableArea, bool invertHitBoxes = false, bool invertBlockableArea = false)
        {
            if (blockableArea == null) return new FPVector[0];

            foreach (HitBox hitBox in hitBoxes)
            {
                if (PushForce(hitBox, blockableArea, invertHitBoxes, invertBlockableArea) > 0)
                    return new FPVector[] { blockableArea.position, hitBox.mappedPosition, (blockableArea.position + hitBox.mappedPosition) / 2 };
            }

            return new FPVector[0];
        }

        /// <summary>
        /// Test Collision between Hit Boxes and Hurt Boxes.
        /// <returns>
        /// Returns an array with 3 vectors:<br/>
        /// 0 - The position of the hurtbox that collided with the hitbox.<br/>
        /// 1 - The position of the hitbox position that collided with the hurtbox.<br/>
        /// 2 - The average distance between both vectors.
        /// </returns>
        /// </summary>
        /// <param name="hitBoxes">Array of hitboxes.</param>
        /// <param name="hurtBoxes">Blockable area to be tested against.</param>
        /// <param name="hitConfirmType">Hit confirm type being tested.</param>
        /// <param name="invertHitBoxes">Mirror hitboxes positions horizontally.</param>
        /// <param name="invertHurtBoxes">Mirror hurtboxes positions horizontally.</param>
        public static FPVector[] TestCollision(HitBox[] hitBoxes, HurtBox[] hurtBoxes, HitConfirmType hitConfirmType, bool invertHitBoxes = false, bool invertHurtBoxes = false)
        {
            foreach (HitBox hitBox in hitBoxes)
            {
                if (hitBox.hide) continue;
                if (hitBox.collisionType == CollisionType.noCollider) continue;
                if (hitConfirmType == HitConfirmType.Throw && hitBox.collisionType != CollisionType.throwCollider) continue;
                if (hitConfirmType == HitConfirmType.Hit && hitBox.collisionType == CollisionType.throwCollider) continue;

                hitBox.state = 0;

                foreach (HurtBox hurtBox in hurtBoxes)
                {
                    if (hitBox.collisionType == CollisionType.projectileInvincibleCollider && hurtBox.type == HurtBoxType.projectile) continue;
                    if (hitBox.collisionType == CollisionType.physicalInvincibleCollider && hurtBox.type == HurtBoxType.physical) continue;

                    if (PushForce(hitBox, hurtBox, invertHitBoxes, invertHurtBoxes) > 0)
                    {
                        if (hitConfirmType == HitConfirmType.Hit) hitBox.state = 1;
                        return new FPVector[] { hurtBox.position, hitBox.mappedPosition, (hurtBox.position + hitBox.mappedPosition) / 2 };
                    }
                }
            }

            /*foreach (HitBox hitBox in hitBoxes)
            {
                if (hitBox.state == 1) hitBox.state = 0;
            }*/
            return new FPVector[0];
        }

        /// <summary>
        /// Test Collision between 2 groups of Hit Boxes.
        /// <returns>Returns how close the hitboxes are from one another.</returns>
        /// </summary>
        /// <param name="hitBoxes">Array of hitboxes.</param>
        /// <param name="opHitBoxes">Array of hitboxes.</param>
        /// <param name="invertHitBoxes">Mirror hitboxes positions horizontally.</param>
        /// <param name="invertOpHitBoxes">Mirror opposing hitboxes positions horizontally.</param>
        public static Fix64 TestCollision(HitBox[] hitBoxes, HitBox[] opHitBoxes, bool invertHitBoxes = false, bool invertOpHitBoxes = false)
        {
            Fix64 totalPushForce = 0;
            foreach (HitBox hitBox in hitBoxes)
            {
                if (hitBox.collisionType != CollisionType.bodyCollider) continue;
                foreach (HitBox opHitBox in opHitBoxes)
                {
                    if (opHitBox.collisionType != CollisionType.bodyCollider) continue;

                    totalPushForce += PushForce(hitBox, opHitBox, invertHitBoxes, invertOpHitBoxes);
                }
            }
            return totalPushForce;
        }
        #endregion


        #region private methods
        /// <summary>Turn variables from hitbox and blockableArea into generic values</summary>
        private static Fix64 PushForce(HitBox hitBox, BlockArea blockableArea, bool invertHitBoxes = false, bool invertBlockableArea = false)
        {
            return PushForce(hitBox.shape, hitBox._rect, hitBox._radius, hitBox.mappedPosition, invertHitBoxes, blockableArea.shape, blockableArea._rect, blockableArea._radius, blockableArea.position, invertBlockableArea);
        }

        /// <summary>Turn variables from hitbox and hurtbox into generic values</summary>
        private static Fix64 PushForce(HitBox hitBox, HurtBox hurtBox, bool invertHitBoxes = false, bool invertHurtBoxes = false)
        {
            return PushForce(hitBox.shape, hitBox._rect, hitBox._radius, hitBox.mappedPosition, invertHitBoxes, hurtBox.shape, hurtBox._rect, hurtBox._radius, hurtBox.position, invertHurtBoxes);
        }

        /// <summary>Turn variables from both hitboxes into generic values</summary>
        private static Fix64 PushForce(HitBox hitBox, HitBox opHitBox, bool invertHitBoxes = false, bool invertOpHitBoxes = false)
        {
            return PushForce(hitBox.shape, hitBox._rect, hitBox._radius, hitBox.mappedPosition, invertHitBoxes, opHitBox.shape, opHitBox._rect, opHitBox._radius, opHitBox.mappedPosition, invertOpHitBoxes);
        }

        /// <summary>Return the intersection value between the 2 objects</summary>
        private static Fix64 PushForce(HitBoxShape shape1, FPRect rect1, Fix64 radius1, FPVector position1, bool invert1, HitBoxShape shape2, FPRect rect2, Fix64 radius2, FPVector position2, bool invert2)
        {
            Fix64 pushForce = 0;

            if (!UFE.config.detect3D_Hits)
            {
                position1.z = 0;
                position2.z = 0;
            }

            if (shape1 == HitBoxShape.rectangle)
                rect1 = UpdateRect(rect1, position1, invert1);

            if (shape2 == HitBoxShape.rectangle)
                rect2 = UpdateRect(rect2, position2, invert2);

            if (shape1 == HitBoxShape.circle)
            {
                if (shape2 == HitBoxShape.circle)
                {
                    pushForce = Intersect(radius1, position1, radius2, position2);
                }
                else if (shape2 == HitBoxShape.rectangle)
                {
                    pushForce = Intersect(rect2, radius1, position1);
                }
            }
            else if (shape1 == HitBoxShape.rectangle)
            {
                if (shape2 == HitBoxShape.circle)
                {
                    pushForce = Intersect(rect1, radius2, position2);
                }
                else if (shape2 == HitBoxShape.rectangle)
                {
                    pushForce = Intersect(rect1, rect2);
                }
            }

            return pushForce;
        }

        /// <summary>Update Rect with world position and invert it if needed</summary>
        private static FPRect UpdateRect(FPRect rect, FPVector position, bool invert)
        {
            if (invert)
            {
                rect.x += rect.width;
                rect.x *= -1;
            }
            rect.x += position.x;
            rect.y += position.y;

            return rect;
        }

        /// <summary>Rect - Rect</summary>
        private static Fix64 Intersect(FPRect rect1, FPRect rect2)
        {
            Fix64 pushForce = 0;
            pushForce = rect1.IntersectArea(rect2);
            return pushForce;
        }

        /// <summary>Rect - Sphere</summary>
        private static Fix64 Intersect(FPRect rect1, Fix64 radius, FPVector position)
        {
            Fix64 pushForce = 0;
            Fix64 dist = rect1.DistanceToPoint(position);
            if (dist <= radius)
                pushForce = radius - dist;
            return pushForce;
        }

        /// <summary>Sphere - Sphere</summary>
        private static Fix64 Intersect(Fix64 radius1, FPVector position1, Fix64 radius2, FPVector position2)
        {
            Fix64 pushForce = 0;
            Fix64 dist = FPVector.Distance(position1, position2);
            if (dist <= radius1 + radius2)
                pushForce = radius1 + radius2 - dist;
            return pushForce;
        }
        #endregion
    }
}