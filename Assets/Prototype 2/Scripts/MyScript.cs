using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuantumTek.QuantumUI;

public class MyScript : MonoBehaviour
{
    [SerializeField] private MSSceneControllerFree controlManager; // reference to MS Vehicle System (third party) scene controller
    private MSVehicleControllerFree vehicleManager; // reference to MS Vehicle System (third party) vehicle controller
    [SerializeField] private CheckpointManager checkpointManager; // reference to checkpoint system manager
    [SerializeField] private AudioClip nitroSound;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private QUI_Bar progressBar; // reference to GUI visualisation of nitro value
    private int jumpForce;
    private int nitroForce ;
    private GameObject vehicle;
    private Rigidbody vehicleRB;
    private AudioSource vehicleAudioPlayer;
    private ParticleSystem[] nitroFX;
    private float nitroValue = 0; // amount of nitro to spend
    private float nitroPerFrame; // nitro consumption per fixed frame
    private bool isJumping = false;
    private bool isNitro = false;
    private const int FPS = 60;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // lock and hide cursor
        Application.targetFrameRate = FPS; // FPS limit
        
        nitroPerFrame = Time.fixedDeltaTime * 0.5f;

        vehicle = controlManager.GetCurrentVehicle(); // current vehicle defined via MS Vehicle System (third party)
        vehicleRB = vehicle.GetComponent<Rigidbody>();
        vehicleRB.centerOfMass = vehicle.transform.Find("CoM").localPosition;
        
        vehicleManager = vehicle.GetComponent<MSVehicleControllerFree>();
        vehicleAudioPlayer = vehicle.GetComponent<AudioSource>();
        nitroFX = vehicle.GetComponentsInChildren<ParticleSystem>();

        int _totalMass = GetVehicleTotalMass(vehicleManager);
        jumpForce = _totalMass * 10;
        nitroForce = _totalMass * 20;
    }

    void FixedUpdate()
    {
        if (Input.GetMouseButtonDown(0) && vehicleManager.GetGroundedWheels() > 1) // Left mouse button press is for jump initial impulse (2 or more wheels should be grounded)
        {
            isJumping = true;
            VehicleSFX(jumpSound);
            VehicleJump(ForceMode.Impulse);
        }

        if (Input.GetMouseButtonUp(0)) // left mouse button released means no more jumping
        {
            isJumping = false;
            VehicleSFX();
        }

        if (isJumping) // Holding left mouse button is for continuous jumping
            VehicleJump();

        if (Input.GetMouseButtonDown(1) && (nitroValue > nitroPerFrame)) // Right mouse button is for speed up (there must be enough nitro)
        {
            isNitro = true;    
            VehicleVFX(true);
            VehicleSFX(nitroSound);
            VehicleNitro();
        }
        
        if (Input.GetMouseButtonUp(1) || (nitroValue < nitroPerFrame))
        {
            isNitro = false;
            VehicleVFX(false);
            VehicleSFX();
        }

        if (isNitro)
            VehicleNitro();
        else
            nitroValue += Time.fixedDeltaTime / 10; // full value in 10 sec

        if (Input.GetKeyDown(KeyCode.F))
            ResetVehiclePosition();

        nitroValue = Mathf.Clamp(nitroValue, 0, 1);
        progressBar.SetFill(nitroValue);
    }

    private int GetVehicleTotalMass(MSVehicleControllerFree vehicle)
    {
        float _result = vehicle.GetComponentInChildren<Rigidbody>().mass;
        
        List<WheelCollider> _wheels = new();
        vehicle.GetComponentsInChildren<WheelCollider>(false, _wheels);
        _wheels.ForEach(wheel => _result += wheel.mass);
        
        return ((int)_result);
    }

    public void ResetVehiclePosition() // Move the vehicle to tle last checkpoint, reset its transform, velocity, and shut down the engine
    {
        vehicleRB.velocity = Vector3.zero;
        vehicleRB.angularVelocity = Vector3.zero;
        
        Transform _lastPoint = checkpointManager.GetLastPoint();
        vehicle.transform.SetPositionAndRotation(_lastPoint.position, _lastPoint.rotation);
        vehicle.GetComponent<MSVehicleControllerFree>().StartCoroutine ("StartEngineCoroutine", false);
    }

    public void VehicleJump(ForceMode mode = ForceMode.Force)
    {
        vehicleRB.AddRelativeForce(Vector3.up * jumpForce, mode);
    }

    public void VehicleNitro()
    {
        vehicleRB.AddRelativeForce(Vector3.forward * nitroForce, ForceMode.Force);
        nitroValue -= nitroPerFrame;
    }

    private void VehicleSFX(AudioClip clip = null)
    {
        if (clip == null){
            vehicleAudioPlayer.volume = 1;
            StartCoroutine(FadeAudioSource.StartFade(vehicleAudioPlayer, 1, 0));
        }
        else
            vehicleAudioPlayer.PlayOneShot(clip);
    }

    private void VehicleVFX(bool enable){
        if (nitroFX == null) return;

        foreach (ParticleSystem particleSystem in nitroFX){
            if (enable == true) particleSystem.Play();
            else particleSystem.Stop();
        }
    }
}
