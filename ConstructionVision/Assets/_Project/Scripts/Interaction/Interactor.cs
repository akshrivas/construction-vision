using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    public float reach = 2.6f;

    IInteractable current;
    string infoTitle;
    string infoBody;
    Camera view;

    void Awake()
    {
        view = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        current = null;
        var origin = view.transform.position;
        var direction = view.transform.forward;
        if (Physics.Raycast(origin, direction, out var hit, reach, ~0, QueryTriggerInteraction.Ignore))
            current = hit.collider.GetComponentInParent<IInteractable>();

        if (current == null)
            ClearRoom();

        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.eKey.wasPressedThisFrame && current != null)
            current.Interact(this);
    }

    public void ShowRoom(string title, string detail)
    {
        if (infoTitle == title)
        {
            ClearRoom();
            return;
        }

        infoTitle = title;
        infoBody = detail;
    }

    void ClearRoom()
    {
        infoTitle = null;
        infoBody = null;
    }

    void OnGUI()
    {
        if (current != null)
        {
            var prompt = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
            };
            prompt.normal.textColor = Color.white;
            GUI.Label(new Rect(0f, Screen.height * 0.62f, Screen.width, 36f), "[E]  Interact", prompt);
        }

        if (string.IsNullOrEmpty(infoTitle))
            return;

        var box = new GUIStyle(GUI.skin.box)
        {
            fontSize = 24,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
        };
        GUI.Box(new Rect(Screen.width * 0.5f - 190f, Screen.height * 0.5f - 48f, 380f, 96f), infoTitle + "\n" + infoBody, box);
    }
}
