using UnityEngine;

[System.Serializable]
public class GUIBarOptions
{
    public bool editorToggle;
    public bool previewToggle;
    public bool flip;
    public Texture2D backgroundImage;
    public Color backgroundColor;
    public Texture2D fillImage;
    public Color fillColor;
    public Rect backgroundRect;
    public Rect fillRect;
    public GameObject bgPreview;
    public GameObject fillPreview;
}
