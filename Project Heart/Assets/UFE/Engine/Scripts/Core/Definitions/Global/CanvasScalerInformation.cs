using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CanvasScalerInformation
{
    public float defaultSpriteDPI = 96f;
    public float fallbackScreenDPI = 96f;
    public float matchWidthOrHeight = 0f;
    public CanvasScaler.Unit physicalUnit = CanvasScaler.Unit.Points;
    public float referencePixelsPerUnit = 100f;
    public Vector2 referenceResolution = new Vector2(1920f, 1080f);
    public float scaleFactor = 1f;
    public CanvasScaler.ScreenMatchMode screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
    public CanvasScaler.ScaleMode scaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

    //---------------------------------------------------------------------------------------------------------
    // We use comment the next line because we use a "Screen Space - Overlay" canvas
    // and the "dynamicPixelsPerUnit" property is only used in "World Space" Canvas.
    //---------------------------------------------------------------------------------------------------------
    //public float dynamicPixelsPerUnit = 100f;
}