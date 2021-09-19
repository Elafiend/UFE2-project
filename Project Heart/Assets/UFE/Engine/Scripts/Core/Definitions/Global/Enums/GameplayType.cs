namespace UFE3D
{
    public enum GameplayType
    {
#if !UFE_LITE && !UFE_BASIC
        _2DFighter,
        _3DFighter,
        //_3DArena
#else
    _2DFighter
#endif
    }
}