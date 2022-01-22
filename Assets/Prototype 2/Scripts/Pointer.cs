using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// looking at target in 3D space regarding the main camera view
public class Pointer : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Camera renderCamera;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main; // get a camera tagget as 'MainCamera'
    }
    void LateUpdate()
    {
        if (target == null || renderCamera == null || mainCamera == null) return;

        transform.LookAt(target); // face to the target direction
        // transform the render camera as main camera but distance set to 10
        renderCamera.transform.localPosition = mainCamera.transform.localPosition.normalized * 10;
        renderCamera.transform.localRotation = mainCamera.transform.localRotation;
    }

    public void SetTarget(Transform target)
    {
        if (target != null) this.target = target;
    }
}
