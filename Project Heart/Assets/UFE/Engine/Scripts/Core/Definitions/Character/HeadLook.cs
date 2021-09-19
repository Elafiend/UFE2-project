namespace UFE3D
{
    [System.Serializable]
    public class HeadLook
    {
        public bool enabled = false;
        public BendingSegment[] segments = new BendingSegment[0];
        public NonAffectedJoints[] nonAffectedJoints = new NonAffectedJoints[0];
        public BodyPart target = BodyPart.head;
        public float effect = 1;
        public bool overrideAnimation = true;
        public bool disableOnHit = true;
    }
}