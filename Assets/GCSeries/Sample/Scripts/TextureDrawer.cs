using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureDrawer : MonoBehaviour
{
    public RenderTexture[] renderTextures;
    private void OnGUI()
    {
        for (int i = 0; i < renderTextures.Length; i++)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), renderTextures[i]);
        }
    }
}
