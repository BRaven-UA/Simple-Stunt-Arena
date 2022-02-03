using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Manages ingame checkpoints: placing on the level, trigger, SFX
public class CheckpointManager : MonoBehaviour
{
    private Pointer2 GUI;
    private Transform[] points; // array of potential places for checkpoint
    private Transform lastPoint; // previous checkpoint location
    private Transform currentPoint; // current place for checkpoint
    private Transform nextPoint; // next checkpoint after current
    private Transform checkPointGameObject;   // reference to gameobject with visuals and physics
    private Transform pointer;   // gameobject that points to the next checkpoint
    private AudioSource sound; // sound of triggered checkpoint

    void Awake()
    {
        GUI = GetComponentInChildren<Pointer2>();
        sound = gameObject.GetComponent<AudioSource>();

        checkPointGameObject = transform.Find("CheckPoint");
        pointer = checkPointGameObject.transform.Find("Pointer");
        lastPoint = transform.Find("StartPoint"); // initial position (placed manually)

        // fill the array with manually placed points
        var _container = transform.Find("Points");
        var _count = _container.childCount;
        points = new Transform[_count];
        for (int i = 0; i < _count; i++)
        {
            points[i] = _container.GetChild(i);
        }
    }

    void Start()
    {
        SetCurrentCheckpoint();
    }

    private Transform GetRandomPoint() // chooses randomly one of overall available points
    {
        Transform _result = null;
        
        if (points.Length > 0)
        {
            do _result = points[Random.Range(0, points.Length)];
            while (_result == currentPoint && points.Length > 1);
        }
        
        return _result;
    }

    private void SetCheckPoint() // sets checkpoint gameobject on current point (place)
    {
        checkPointGameObject.SetPositionAndRotation(currentPoint.position, currentPoint.rotation);
        pointer.LookAt(nextPoint);
        checkPointGameObject.gameObject.SetActive(true);
    }

    private void SetCurrentCheckpoint() // defines current checkpoint, updates GUI and set checkpoint
    {
        var _newPoint = GetRandomPoint();
        if (_newPoint == null) return; // point generator doesn't work, check points aren't available
       
        if (currentPoint) // at least one checkpoint has already been reached
        {
            lastPoint = currentPoint;
            currentPoint = nextPoint;
            nextPoint = _newPoint;
        }
        else // for the very first checkpoint
        {
            currentPoint = _newPoint;
            nextPoint = GetRandomPoint(); // another random point
        }

        if (GUI) GUI.SetTarget(currentPoint);

        SetCheckPoint();
    }

    public void OnCheckPointReached() // called by checkpoint gameobject, plays sound, removes current checkpoint and sets next checkpoint
    {
        sound.Play();
        checkPointGameObject.gameObject.SetActive(false);
        SetCurrentCheckpoint();
    }

    public Transform GetLastPoint() {return lastPoint;} // TODO: replace with set/get
}
