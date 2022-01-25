using UnityEngine;

public class ForceArea : MonoBehaviour
{
    void OnTriggerStay(Collider other)
    {
        var _rigidBody = other.attachedRigidbody;
        if (_rigidBody)
        {
            _rigidBody.AddForce(transform.up * _rigidBody.mass * 25, ForceMode.Force);
        }
    }
}
