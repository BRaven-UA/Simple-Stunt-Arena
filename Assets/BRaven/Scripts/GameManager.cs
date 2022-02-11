using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region GUI references
    [SerializeField] private GameObject guiMenu; // game menu
    [SerializeField] private GameObject guiPause; // pause label
    [SerializeField] private GameObject guiVehicleSelection; // new vehicle selection panel
    [SerializeField] private GameObject guiResetVehicle; // reset vehicle button
    [SerializeField] private GameObject guiControls; // controls layout panel
    [SerializeField] private Image guiVehicleImage; // current vehicle image
    [SerializeField] private Text guiVehicleName; // current vehicle name label
    [SerializeField] private Text guiTimer; // timer label
    [SerializeField] private Text guiTimePlayed; // time played label
    [SerializeField] private Text guiBestTime; // best time played label
    [SerializeField] private GameObject guiGameOver; // game over label
    [SerializeField] private Text guiNewBestTime; // new best time label (on game over)
    #endregion

    [SerializeField] private Image startVehicle; // default vehicle
    [SerializeField] private CheckpointManager checkpoints;
    private MSSceneControllerFree controller;
    private AudioSource beepSound; // last 10 sec timer's ticks sound

    private GameObject currentVehicle;
    private GameState gameState = GameState.Inactive; // current state of the game (see GameState enum)
    private enum GameState {Active, Paused, Inactive}; // available states of the game

    private int timer; // time left in seconds
    private int timePlayed = 0; // time played in seconds
    private int bestTime = 0; // best time in seconds
    const int BonusTime = 15; // time added to the timer when a checkpoint is reached
    
    const string SavePath = "/SimpleStuntArena.txt";

    void Awake()
    {
        Application.targetFrameRate = 60; // FPS limit

        controller = GetComponent<MSSceneControllerFree>();
        var _audioSources = GetComponents<AudioSource>();
        beepSound = _audioSources[0];
        _audioSources[1].ignoreListenerPause = true; // GUI button click sound should ignore pause

        LoadGame();
    }

    void Start()
    {
        SelectVehicle(startVehicle); // set default vehicle
    }


    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (guiVehicleSelection.activeInHierarchy) guiVehicleSelection.SetActive(false); // hide the selection panel if it's visible
            else if (guiControls.activeInHierarchy) guiControls.SetActive(false); // hide the controls panel if it's visible
            else GameMenu(); // toggle menu
        }
     
    }

    public void NewGame()
    {
        StopAllCoroutines();

        guiTimePlayed.color = Color.white;
        guiBestTime.color = Color.green;
        guiBestTime.text = string.Format("Best time: {0}:{1:00}", bestTime / 60, bestTime % 60);

        guiGameOver.SetActive(false);
        guiNewBestTime.gameObject.SetActive(false);

        gameState = GameState.Active;
        timer = 60;
        timePlayed = 0;

        GameMenu(false);
        checkpoints.EnableCheckpoints(true);
        EnableVehicle(true);
        StartCoroutine(StartCountdown()); // start the countdown to the end of the game
    }

    private IEnumerator StartCountdown() // playing time management coroutine
    {
        while (timer > 0) // while time is left
        {
            yield return new WaitForSeconds(1);
            timer--;
            timePlayed++;

            guiTimer.text = string.Format("Time left: {0}:{1:00}", timer / 60, timer % 60);
            guiTimePlayed.text = string.Format("Time played: {0}:{1:00}", timePlayed / 60, timePlayed % 60);
            
            if (timer < 10) // last 10 seconds the text displayed in red and play beep sound each second
            {
                guiTimer.color = Color.red;
                beepSound.Play();
            }
            else guiTimer.color = Color.white;

            if (timePlayed > bestTime) // new best time
            {
                guiTimePlayed.color = Color.green;
                guiBestTime.color = Color.white;
            }
        }

        GameOver(); // time is over
    }

    public void AddBonusTime() {timer += BonusTime;}

    private void GameMenu(bool? state = null) // show, hide, toggle (no arguments) game menu
    {
        bool _enabled = state.HasValue? state.Value : !guiMenu.activeSelf; // defined or toggled value
        Cursor.lockState = _enabled? CursorLockMode.None : CursorLockMode.Locked; // show cursor when game menu enabled, otherwise lock (hide) cursor
        Input.ResetInputAxes(); // reset all inputs
        
        guiMenu.SetActive(_enabled); // show/hide game menu
        guiPause.SetActive(gameState != GameState.Inactive); // hide pause label when game is inactive
        guiResetVehicle.SetActive(gameState != GameState.Inactive); // hide reset button when game is inactive

        if (gameState != GameState.Inactive) Pause(_enabled); // do not toggle pause when inactive
    }

    private void Pause(bool enabled) // ingame pause
    {
        if (enabled)
        {
            gameState = GameState.Paused;
            Time.timeScale = 0; // stop time
        }
        else
        {
            gameState = GameState.Active;
            Time.timeScale = 1; // real time
        }
        
        AudioListener.pause = enabled; // also set sound pause
    }

    public void SelectVehicle(Image image) // select vehicle by image name
    {
        string _vehicleName = image.sprite.name;
        guiVehicleName.text = _vehicleName;
        guiVehicleImage.sprite = image.sprite;

        EnableVehicle(false); // deactivate current vehicle

        int _index = Array.FindIndex(controller.vehicles, g => g.name == _vehicleName); // all available vehicles listed in controller script
        controller.SetVehicle(_index); // set new vehicle via the controller
        currentVehicle = controller.GetVehicle(); // get back reference to vehicle gameobject

        EnableVehicle(gameState != GameState.Inactive); // hide the vehicle until game started
    }

    private void EnableVehicle(bool state)
    {
        if (currentVehicle)
        {
            currentVehicle.SetActive(state);
            if (state == true) ResetVehicle();
        }
    }

    public void ResetVehicle() // reset position and settings of current vehicle
    {
        currentVehicle.GetComponent<VehicleControllerExtension>().ResetVehicle();
        GameMenu(false); // also hide game menu
    }

    private void GameOver()
    {
        Input.ResetInputAxes(); // reset all gameinput
        gameState = GameState.Inactive;

        guiGameOver.SetActive(true);
        if (timePlayed > bestTime) // new best time
        {
            bestTime = timePlayed;
            guiNewBestTime.text = string.Format("New Best Time !\n{0}:{1:00}", bestTime / 60, bestTime % 60);
            guiNewBestTime.gameObject.SetActive(true);

            SaveGame(); // save only when new best time
        }

        EnableVehicle(false); // deactivate current vehicle
        checkpoints.EnableCheckpoints(false);
        StartCoroutine(WaitForInput());
    }

    private IEnumerator WaitForInput() // awaits user input before show game menu
    {
        yield return new WaitForSeconds(1); // accidental input protection

        while (!Input.anyKeyDown) yield return null;
        
        GameMenu(true);
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void SaveGame()
    {
        File.WriteAllText(Application.persistentDataPath + SavePath, bestTime.ToString());
    }

    public void LoadGame()
    {
        string _path = Application.persistentDataPath + SavePath;
        if (File.Exists(_path)) bestTime = Convert.ToInt32((File.ReadAllText(_path)));
    }
}
