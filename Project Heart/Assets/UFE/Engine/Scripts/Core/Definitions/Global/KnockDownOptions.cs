namespace UFE3D
{
    [System.Serializable]
    public class KnockDownOptions
    {
        public SubKnockdownOptions air;
        public SubKnockdownOptions high;
        public SubKnockdownOptions highLow;
        public SubKnockdownOptions sweep;
        public SubKnockdownOptions crumple;
        public SubKnockdownOptions wallbounce;
    }
}