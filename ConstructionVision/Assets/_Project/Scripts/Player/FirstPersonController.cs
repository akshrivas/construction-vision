using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonController : MonoBehaviour
{
    public float moveSpeed = 2.15f;
    public float lookSensitivity = 0.08f;

    CharacterController body;
    Transform view;
    float pitch;
    float vertical;

    void Awake()
    {
        body = GetComponent<CharacterController>();
        view = GetComponentInChildren<Camera>().transform;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
        if (keyboard == null || mouse == null)
            return;

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            var locked = Cursor.lockState != CursorLockMode.Locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        if (mouse.leftButton.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            var look = mouse.delta.ReadValue() * lookSensitivity;
            transform.Rotate(0f, look.x, 0f);
            pitch = Mathf.Clamp(pitch - look.y, -80f, 80f);
            view.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        var wish = Vector3.zero;
        if (keyboard.wKey.isPressed) wish += Vector3.forward;
        if (keyboard.sKey.isPressed) wish += Vector3.back;
        if (keyboard.dKey.isPressed) wish += Vector3.right;
        if (keyboard.aKey.isPressed) wish += Vector3.left;

        var world = transform.TransformDirection(wish);
        world.y = 0f;
        if (world.sqrMagnitude > 1f)
            world.Normalize();

        if (body.isGrounded && vertical < 0f)
            vertical = -2f;
        vertical -= 20f * Time.deltaTime;

        var velocity = world * moveSpeed;
        velocity.y = vertical;
        body.Move(velocity * Time.deltaTime);
    }
}
