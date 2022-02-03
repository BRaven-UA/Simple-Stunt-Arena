using UnityEngine;

public class MyScript : MonoBehaviour
{
    [SerializeField] private GameObject guiMenu; // ESC menu
    private bool gamePaused = false;

    void Awake()
    {
        Application.targetFrameRate = 60; // FPS limit
        TogglePause();
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel")) TogglePause();
    }

    public void TogglePause()
    {
        gamePaused = !gamePaused;
        Cursor.lockState = gamePaused? CursorLockMode.None: CursorLockMode.Locked; // hide cursor wned drive, unhide when GUI showed
        AudioListener.pause = gamePaused; // also pause sound
        Time.timeScale = gamePaused? 0: 1; // ON/OFF ingame time
        guiMenu.SetActive(gamePaused); // show/hide ESC menu
    }

    public void SelectVehicle(int index)
    {
        var _controller = GetComponent<MSSceneControllerFree>();
        _controller.GetCurrentVehicle().SetActive(false); // deactivate current vehicle
        _controller.SetVehicle(index); // set new vehicle
        
        var _newVehicle = _controller.GetCurrentVehicle();
        _newVehicle.SetActive(true); //activane new vehicle
        _newVehicle.GetComponent<VehicleControllerExtension>().ResetPosition(); // reset its position and settings
    }
}
