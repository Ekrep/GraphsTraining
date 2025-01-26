using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    private float deltaTime = 0.0f;

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
        // DeltaTime biriktirilerek FPS hesaplanır.
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        // FPS değerini hesaplayın
        float fps = 1.0f / deltaTime;
        string text = $"FPS: {Mathf.Ceil(fps)}";

        // Ekrana yazdırın
        GUIStyle style = new GUIStyle();
        style.fontSize = 24;
        style.normal.textColor = Color.white;

        // Sağ üst köşeye yazdır
        Rect rect = new Rect(10, 10, 200, 50);
        GUI.Label(rect, text, style);

        GUIStyle burstStyle = new GUIStyle();
        burstStyle.fontSize = 24;
        burstStyle.normal.textColor = Color.red;

        Rect burstRect = new Rect(10, 50, 200, 50);
        GUI.Label(burstRect, burstMode, style);
    }
}