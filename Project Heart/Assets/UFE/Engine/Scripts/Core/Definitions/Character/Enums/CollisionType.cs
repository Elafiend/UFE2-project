namespace UFE3D
{
    /// <summary>
    /// Collision Type is used to determine the hit boxes of the character at a given time:
    /// <para/>Body Collider: Movement and hit collision. Opposing Body Colliders are expected to not overlap.
    /// <para/>Hit Collider: Hit collision only. Opposing Hit Colliders can overlap
    /// <para/>No Collider: No collision. Useful for body parts that are not critical but can be a hurt box if needed.
    /// <para/>Projectile Invincible Collider: No collision for only Projectile type hurt boxes.
    /// <para/>Physical Invincible Collider: No collision for all but Projectile type hurt boxes.
    /// </summary>
    public enum CollisionType
    {
        bodyCollider,
        hitCollider,
        noCollider,
        throwCollider,
        projectileInvincibleCollider,
        physicalInvincibleCollider
    }
}