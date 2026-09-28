using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public bool open;
    public float openAngle = -90f;

    float angle;

    void Update()
    {
        var target = open ? openAngle : 0f;
        angle = Mathf.MoveTowards(angle, target, 140f * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(0f, angle, 0f);
    }

    public void Interact(Interactor interactor)
    {
        open = !open;
    }
}
