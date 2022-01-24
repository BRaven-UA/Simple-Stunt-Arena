using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// looking at target in 3D space regarding the main camera view
public class Pointer2 : MonoBehaviour
{
    private Transform target;
    private Transform pointerMesh;
    private Camera pointerCamera;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main; // get a camera tagget as 'MainCamera'
        pointerCamera = GetComponentInChildren<Camera>();
        pointerMesh = transform.Find("PointerMesh");
        // target = transform.parent.Find("StartPoint");
    }
    void LateUpdate()
    {
        if (target == null || pointerCamera == null || mainCamera == null) return;

        pointerMesh.transform.position = mainCamera.transform.position;
        pointerMesh.transform.LookAt(target); // face to the target direction

        pointerCamera.transform.SetPositionAndRotation(mainCamera.transform.position, mainCamera.transform.rotation);
        pointerCamera.transform.Translate(Vector3.back * 10, Space.Self);
    }

    public void SetTarget(Transform target)
    {
        if (target != null) this.target = target;
    }
}
