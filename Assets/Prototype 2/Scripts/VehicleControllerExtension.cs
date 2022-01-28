using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuantumTek.QuantumUI;

public class VehicleControllerExtension : MonoBehaviour
{
    [SerializeField] private MSSceneControllerFree controlManager; // reference to MS Vehicle System (third party) scene controller
    private MSVehicleControllerFree MSVehicleManager; // reference to MS Vehicle System (third party) vehicle controller
    [SerializeField] private CheckpointManager checkpointManager; // reference to checkpoint system manager
    [SerializeField] private QUI_Bar progressBar; // reference to GUI visualisation of boost value
    private int jumpForce;
    private int boostForce ;
    private Rigidbody rigidBody;
    private AudioSource jumpSFX;
    private AudioSource boostSFX;
    private float jumpSFXvolume; // to be able restore original volume after fade effect
    private float boostSFXvolume; // to be able restore original volume after fade effect
    private ParticleSystem[] boostVFX;
    private float boostValue = 0; // amount of boost to spend
    private float boostPerFrame; // boost consumption per fixed frame
    private bool isJumping = false;
    private bool isBoosting = false;
    private bool isMidairControlEnabled = false; // switch vehicle control when in the air

    void Start()
    {
        boostPerFrame = Time.fixedDeltaTime * 0.5f;

        rigidBody = GetComponent<Rigidbody>();
        rigidBody.centerOfMass = transform.Find("CoM").localPosition;
        
        MSVehicleManager = GetComponent<MSVehicleControllerFree>();
        foreach (var SFX in GetComponents<AudioSource>())
        {
            if (SFX.clip.name.Contains("jump", System.StringComparison.OrdinalIgnoreCase))
            {
                jumpSFX = SFX;
                jumpSFXvolume = SFX.volume;
            }
            if (SFX.clip.name.Contains("boost", System.StringComparison.OrdinalIgnoreCase))
            {
                boostSFX = SFX;
                boostSFXvolume = SFX.volume;
            }
        } 
        boostVFX = GetComponentsInChildren<ParticleSystem>();

        int _totalMass = GetTotalMass();
        jumpForce = _totalMass * 10;
        boostForce = _totalMass * 20;
    }

    void FixedUpdate()
    {
        bool isMidair = (MSVehicleManager.GetGroundedWheels() == 0);

        // JUMPING. Initial jump is an impulse and only allowed when grounded.
        // Holding down the mouse button increases the height of the jump
        // TODO: fix double jump when spamming the button
        if (Input.GetButton("Jump")) // Right mouse button
        {
            if (!isJumping && !isMidair) // initialization
            {
                isJumping = true;
                PlaySFX(jumpSFX, true, jumpSFXvolume);
                Jump(ForceMode.Impulse);
            }

            Jump(); 
        }
        else if (isJumping) // stop jumping
        {
            isJumping = false;
            PlaySFX(jumpSFX, false);
        }

        // BOOSTING. Allowed any time if enough boost value has been accumulated.
        // Consumes boost value when active, otherwise slowly refreshes its value.
        // TODO: fix endless holding
        if (Input.GetButton("Boost") && boostValue > boostPerFrame) // Left mouse button
        {
            if (!isBoosting) // initialization
            {
                isBoosting = true;    
                PlayVFX(true);
                PlaySFX(boostSFX, true, boostSFXvolume);
            }

            Boost();
        }
        else if (isBoosting) // stop accelerating
        {
            isBoosting = false;
            PlayVFX(false);
            PlaySFX(boostSFX, false);
        }
        else boostValue += Time.fixedDeltaTime / 10; // refreshing to full value within 10 secs

        // MIDAIR CONTROL. When in the air vertical input becomes the pitch axis and horizontal input becomes the yaw axis.
        // Immediately after takeoff, the current input state must be reset to prevent unexpected behaviour of the vehicle.
        if (isMidair)
        {
            if (isMidairControlEnabled)
            {
                rigidBody.AddRelativeTorque(Vector3.up * controlManager.horizontalInput * 0.1f, ForceMode.VelocityChange);
                rigidBody.AddRelativeTorque(Vector3.right * controlManager.verticalInput * 0.1f, ForceMode.VelocityChange);
            }
            else // switching to the midair control
            {
                // Input.ResetInputAxes(); TODO: find workaround to keep mouse events
                isMidairControlEnabled = true;
            }
        }
        else isMidairControlEnabled = false; // reverting to normal control

        if (Input.GetKeyDown(KeyCode.F)) ResetPosition();
        
        boostValue = Mathf.Clamp(boostValue, 0, 1);
        progressBar.SetFill(boostValue);
    }

    private int GetTotalMass()
    {
        float _result = rigidBody.mass;
        
        List<WheelCollider> _wheels = new();
        GetComponentsInChildren<WheelCollider>(false, _wheels);
        _wheels.ForEach(wheel => _result += wheel.mass);
        
        return ((int)_result);
    }

    public void ResetPosition() // Move the vehicle to tle last checkpoint, reset its transform, velocity, and shut down the engine
    {
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
        
        Transform _lastPoint = checkpointManager.GetLastPoint();
        transform.SetPositionAndRotation(_lastPoint.position, _lastPoint.rotation);
        MSVehicleManager.StartCoroutine ("StartEngineCoroutine", false);

        Input.ResetInputAxes();
    }

    public void Jump(ForceMode mode = ForceMode.Force)
    {
        rigidBody.AddRelativeForce(Vector3.up * jumpForce, mode);
    }

    public void Boost()
    {
        rigidBody.AddRelativeForce(Vector3.forward * boostForce, ForceMode.Force);
        boostValue -= boostPerFrame;
    }

    private void PlaySFX(AudioSource SFX, bool enabled, float volume = 1)
    {
        if (enabled)
        {
            SFX.volume = volume;
            SFX.Play();
        }
        else StartCoroutine(FadeAudioSource.StartFade(SFX, 0.5f, 0));
    }

    private void PlayVFX(bool enabled){
        if (boostVFX == null) return;

        foreach (ParticleSystem particleSystem in boostVFX){
            if (enabled == true) particleSystem.Play();
            else particleSystem.Stop();
        }
    }
}
