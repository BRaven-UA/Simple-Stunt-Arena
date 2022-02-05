using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject guiMenu; // ESC menu
    [SerializeField] private Text guiTimer; // timer label
    [SerializeField] private Text guiPlayed; // time played label
    private AudioSource beepSound;

    private bool gamePaused = false;

    private int timer = 60; // time left in seconds
    private int timePlayed = 0; // time played in seconds
    const int BonusTime = 10; // time added to the timer when a checkpoint is reached

    void Awake()
    {
        beepSound = GetComponent<AudioSource>();
        Application.targetFrameRate = 60; // FPS limit
        StartCoroutine("StartCountdown"); // start the countdown to the end of the game
        TogglePause();
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel")) TogglePause();
    }

    private IEnumerator StartCountdown() // playing time management coroutine
    {
        while (timer > 0) // while time is left
        {
            yield return new WaitForSeconds(1);
            timer--;
            timePlayed++;

            guiTimer.text = string.Format("Time left: {0}:{1:00}", timer / 60, timer % 60);
            guiPlayed.text = string.Format("Time played: {0}:{1:00}", timePlayed / 60, timePlayed % 60);
            
            if (timer < 10) // last 10 seconds the text displayed in red and play beep sound each second
            {
                guiTimer.color = Color.red;
                beepSound.Play();
            }
            else guiTimer.color = Color.white;
        }

        GameOver(); // time is over
    }

    public void AddBonusTime() {timer += BonusTime;}

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

    private void GameOver()
    {
        Debug.Log("GAME OVER !");
        TogglePause();
    }
}
