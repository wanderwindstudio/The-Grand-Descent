using UnityEngine;
using UnityEngine.InputSystem;

// Mouse look. Turns the body left/right and the head up/down.
public class PlayerLook : MonoBehaviour
{
    public Transform head;
    float pitch;

    void Start() => LockCursor();

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null || Game.InputBlocked || Time.timeScale == 0f) return;

        // Click the game view to grab the mouse again after pressing Escape.
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (mouse.leftButton.wasPressedThisFrame) LockCursor();
            return;
        }

        Vector2 delta = mouse.delta.ReadValue() * Game.Tuning.mouseSensitivity;
        transform.Rotate(0f, delta.x, 0f);
        pitch = Mathf.Clamp(pitch - delta.y, -Game.Tuning.pitchClamp, Game.Tuning.pitchClamp);
        head.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    public static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public static void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
