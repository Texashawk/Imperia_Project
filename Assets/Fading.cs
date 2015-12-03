using UnityEngine;
using System.Collections;

public class Fading : MonoBehaviour {

    public Texture2D fadeoutTexture;
    public float fadeSpeed = 1.5f;

    private int drawDepth = -1000;
    private float alpha = 1.0f;
    private int fadeDir = -1;

    void OnGUI()
    {
        alpha += fadeDir * fadeSpeed * Time.deltaTime;
        alpha = Mathf.Clamp01(alpha);

        GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, alpha);            // set the alpha value
        GUI.depth = drawDepth;                                                          // make the black texture render on top
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeoutTexture);   // draw the texture to fit the entire width
    }

    public float BeginFade(int direction)
    {
        fadeDir = direction;
        return (fadeSpeed);
    }

    void OnLevelWasLoaded() //happens anytime a level was loaded
    {
        alpha = 1;   //use this if the alpha is not set to 1 by default
        BeginFade(-1);
    }
}
