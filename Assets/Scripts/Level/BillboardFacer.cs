using UnityEngine;

// Turns a billboard prop to face the camera (around the vertical axis only), every frame.
public class BillboardFacer : MonoBehaviour
{
    void LateUpdate()
    {
        if (Game.Camera != null) Art.FaceCamera(transform);
    }
}
