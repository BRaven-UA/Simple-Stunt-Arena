using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Manages ingame checkpoints: placing on the level, trigger, SFX
public class CheckpointManager : MonoBehaviour
{
    [SerializeField] private Pointer GUI;
    private Transform[] points; // array of potential places for checkpoint
    private Transform currentPoint; // current place for checkpoint
    private Transform lastPoint; // previous checkpoint location
    private Transform checkPoint;   // reference to gameobject with visuals and physics
    private AudioSource sound; // sound of triggered checkpoint

    void Start()
    {
        sound = gameObject.GetComponent<AudioSource>();

        checkPoint = transform.Find("CheckPoint");
        lastPoint = transform.Find("StartPoint"); // initial position (placed manually)

        // fill the array with manually placed points
        var _container = transform.Find("Points");
        var _count = _container.childCount;
        points = new Transform[_count];
        for (int i = 0; i < _count; i++)
        {
            points[i] = _container.GetChild(i);
        }

        SetNextCheckpoint();
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
        checkPoint.SetPositionAndRotation(currentPoint.position, currentPoint.rotation);
        checkPoint.gameObject.SetActive(true);
    }

    private void SetNextCheckpoint() // defines which point (place) will be next, updates GUI and set checkpoint
    {
        var _newPoint = GetRandomPoint();
        if (_newPoint == null) return;

        if (currentPoint != null) lastPoint = currentPoint;
        currentPoint = _newPoint;

        if (GUI != null) GUI.SetTarget(currentPoint);

        SetCheckPoint();
    }

    public void OnCheckPointReached() // called by checkpoint gameobject, plays sound, removes current checkpoint and sets next checpoint
    {
        sound.Play();
        checkPoint.gameObject.SetActive(false);
        SetNextCheckpoint();
    }

    public Transform GetLastPoint() {return lastPoint;} // TODO: replace with set/get
}
