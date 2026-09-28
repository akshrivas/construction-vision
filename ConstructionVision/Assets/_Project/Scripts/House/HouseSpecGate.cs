using System.IO;
using UnityEngine;

public class HouseSpecGate : MonoBehaviour
{
    public FirstPersonController movement;
    public Interactor interactor;

    bool allowed = true;
    string message;

    void Awake()
    {
        var path = Path.Combine(Application.streamingAssetsPath, "HouseSpec.json");
        HouseSpecFile spec = null;
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            spec = JsonUtility.FromJson<HouseSpecFile>(json);
        }

        allowed = spec != null && spec.IsSpikeHouse();
        if (allowed)
            return;

        message = "This version is not built yet.";
        if (movement != null) movement.enabled = false;
        if (interactor != null) interactor.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnGUI()
    {
        if (allowed)
            return;

        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 28,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
        };
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), message, style);
    }
}
