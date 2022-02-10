using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// looking at target in 3D space regarding the main camera view
public class Pointer : MonoBehaviour
{
    private Transform target;
    private Transform pointerMesh;
    private Camera pointerCamera;
    private Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main; // get a camera tagget as 'MainCamera'
        pointerCamera = GetComponentInChildren<Camera>();
        pointerMesh = transform.Find("PointerMesh");
        // target = transform.parent.Find("StartPoint");
        SetTarget(null); // hide itself after initialization
    }
    void LateUpdate()
    {
        pointerMesh.transform.position = mainCamera.transform.position;
        pointerMesh.transform.LookAt(target); // face to the target direction

        pointerCamera.transform.SetPositionAndRotation(mainCamera.transform.position, mainCamera.transform.rotation);
        pointerCamera.transform.Translate(Vector3.back * 10, Space.Self);
    }

    public void SetTarget(Transform target) // set a target to look at
    {
        this.target = target;
        gameObject.SetActive(target && pointerCamera && mainCamera); // deactivate itself if one of the conditions aren't fulfilled
    }
}
