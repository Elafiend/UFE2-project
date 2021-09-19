[System.Serializable]
public class TrainingModeOptions
{
    public bool freezeTime;
    public float p1StartingLife = 100f;
    public float p2StartingLife = 100f;
    public float p1StartingGauge = 0f;
    public float p2StartingGauge = 0f;
    public LifeBarTrainingMode p1Life;
    public LifeBarTrainingMode p1Gauge;
    public LifeBarTrainingMode p2Life;
    public LifeBarTrainingMode p2Gauge;
    public float refillTime = 3f;
}