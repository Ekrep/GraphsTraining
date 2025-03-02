using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    private float frameDuration;
    private int frames;
    private float duration;

    private string avgFrameText;
    private string frameText;

    private string burstMode = "MONO";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            burstMode = "BURST";
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            burstMode = "MONO";
        }
        frameDuration = Time.unscaledDeltaTime;
        frameText = $"FRAME:\t{1f / frameDuration:0}";
        frames += 1;
        duration += frameDuration;
        if (duration >= 1)
        {
            avgFrameText = $"AVG:\t{frames / duration:0}";
            frames = 0;
            duration = 0;
        }
    }

    void OnGUI()
    {
        //burst mode label
        CreateLabel(24, Color.red, new Rect(10, 10, 200, 50), "MODE:\t" + burstMode);
        //FrameRate 
        CreateLabel(24, Color.white, new Rect(10, 30, 200, 50), frameText);
        //AvarageFrameRate
        CreateLabel(24, Color.green, new Rect(10, 50, 200, 50), avgFrameText);
    }

    private void CreateLabel(int fontSize, Color textColor, Rect textRect, string text)
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = fontSize;
        style.normal.textColor = textColor;
        GUI.Label(textRect, text, style);
    }
}