using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Manages ingame checkpoints: placing on the field, trigger, SFX
public class CheckpointManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    private Pointer GUI; // reference to pointer visualization
    
    private Transform[] points; // array of potential places for checkpoint
    private Transform startPoint; // default starting location
    private Transform lastPoint; // previous checkpoint location
    private Transform currentPoint; // current place for checkpoint
    private Transform nextPoint; // next checkpoint after current
    
    private Transform checkPointGameObject;   // reference to gameobject with visuals and physics
    private Transform pointer;   // gameobject that points to the next checkpoint
    private AudioSource sound; // sound of triggered checkpoint

    void Awake()
    {
        GUI = GetComponentInChildren<Pointer>();
        sound = gameObject.GetComponent<AudioSource>();

        checkPointGameObject = transform.Find("CheckPoint");
        pointer = checkPointGameObject.transform.Find("Pointer");

        // fill the array with manually placed points
        var _container = transform.Find("Points");
        var _count = _container.childCount;
        points = new Transform[_count];
        for (int i = 0; i < _count; i++)
        {
            points[i] = _container.GetChild(i);
        }

        startPoint = transform.Find("StartPoint"); // initial position (placed manually)
        lastPoint = startPoint; // must always be set to be accessed from outside the class
    }

    public void EnableCheckpoints(bool state) // ON/OFF checkpoint system
    {
        if (points.Length < 2) state = false; // not enough points

        checkPointGameObject.gameObject.SetActive(state); // visibility of ingame object
        
        if (state == true)
        {
            lastPoint = startPoint; // for correct work of GetRandomPoint()
            currentPoint = startPoint;
            nextPoint =  GetRandomPoint();
            SetNextCheckpoint();
        }
    }

    private Transform GetRandomPoint() // chooses randomly one of overall available points
    {
        Transform _result;
        
        do _result = points[Random.Range(0, points.Length)];
        while (_result == currentPoint || _result == lastPoint); // new point must be different from recent points
    
        return _result;
    }

    private void SetNextCheckpoint() // defines current checkpoint, updates GUI and set checkpoint on the field
    {
        lastPoint = currentPoint;   
        currentPoint = nextPoint;
        nextPoint = GetRandomPoint();

        if (GUI) GUI.SetTarget(currentPoint);

        checkPointGameObject.SetPositionAndRotation(currentPoint.position, currentPoint.rotation);
        pointer.LookAt(nextPoint); // direct inner pointer to the next point
    }

    public void OnCheckPointReached() // called by checkpoint collider
    {
        sound.Play();
        gameManager.AddBonusTime();
        SetNextCheckpoint();
    }

    public Transform GetLastPoint() {return lastPoint;} // TODO: replace with set/get
}
