using UnityEngine;

public class MyScript : MonoBehaviour
{

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // lock and hide cursor
        Application.targetFrameRate = 60; // FPS limit
    }
}
