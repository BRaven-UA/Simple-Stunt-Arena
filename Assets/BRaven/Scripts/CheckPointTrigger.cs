using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointTrigger : MonoBehaviour
{
    [SerializeField] CheckpointManager manager;

    private void OnTriggerEnter(Collider other)
    {
        manager.OnCheckPointReached();
    }
}
