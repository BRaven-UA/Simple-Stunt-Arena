using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuantumTek.QuantumUI;

public class SFX // referenced storage for sound effects
{
    public AudioSource source; // the audio source
    public Coroutine coroutine; // reference to sound fade away coroutine
    public float volume; // backup of original volume

    public SFX(AudioSource source)
    {
        this.source = source;
        this.volume = source.volume;
    }
}

// My extension over borrowed vehicle control script
public class VehicleControllerExtension : MonoBehaviour
{
    [SerializeField] private MSSceneControllerFree controlManager; // reference to MS Vehicle System (third party) scene controller
    private MSVehicleControllerFree MSVehicleManager; // reference to MS Vehicle System (third party) vehicle controller
    [SerializeField] private CheckpointManager checkpointManager; // reference to checkpoint system manager
    [SerializeField] private QUI_Bar progressBar; // reference to GUI visualisation of boost value
    
    private SFX jumpSFX; // jump sound effect
    [SerializeField] private float jumpForce;
    private bool canJump = true;
    private bool isJumping = false;
    
    private ParticleSystem[] boostVFX;
    private SFX boostSFX; // boost sound effect
    private float boostValue = 0; // amount of boost to spend
    private float boostPerFrame; // boost consumption per fixed frame
    private float boostForce ;
    private bool isBoosting = false;
    
    private Rigidbody rigidBody;
    private bool isMidair = false; // none of the vehicle's wheels are on the ground
    private bool isMidairControlEnabled = false; // switch vehicle control when in the air

    void Awake()
    {
        boostPerFrame = Time.fixedDeltaTime * 0.5f;

        rigidBody = GetComponent<Rigidbody>();
        rigidBody.centerOfMass = transform.Find("CoM").localPosition;
        
        MSVehicleManager = GetComponent<MSVehicleControllerFree>();
        foreach (var _audioSource in GetComponents<AudioSource>())
        {
            if (_audioSource.clip.name.Contains("jump", System.StringComparison.OrdinalIgnoreCase))
            {
                jumpSFX = new SFX(_audioSource);
            }
            if (_audioSource.clip.name.Contains("boost", System.StringComparison.OrdinalIgnoreCase))
            {
                boostSFX = new SFX(_audioSource);
            }
        } 
        boostVFX = GetComponentsInChildren<ParticleSystem>();
        boostForce = rigidBody.mass * 25;
    }

    void FixedUpdate()
    {
        isMidair = (MSVehicleManager.GetGroundedWheels() == 0);

        Jump();
        Boost();
        Midair();
    }

    private void Jump()
    {
        // JUMPING. Initial jump is an impulse directed up in global coordinates, enabled only when grounded.
        // Holding down the mouse button increases the height and the time of the jump
        // FIXME: walljumping
        if (Input.GetButton("Jump")) // Right mouse button
        {
            if (canJump && !isJumping && !isMidair) // initialization
            {
                isJumping = true;
                PlaySFX(jumpSFX, true);
                rigidBody.angularVelocity = Vector3.zero;
                rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); // apply global up impulse
                canJump = false; // disable jumping
                Invoke("EnableJump", 1); // enable jumping after 1 second
            }

            if (isJumping) rigidBody.AddRelativeForce(Vector3.up * jumpForce, ForceMode.Force); // extend jumping: apply local up force
        }
        else if (isJumping) // stop jumping
        {
            isJumping = false;
            PlaySFX(jumpSFX, false);
        }
    }

    private void EnableJump() {canJump = true;} // for invoke

    private void Boost()
    {
        // BOOSTING. Allowed any time if enough boost value has been accumulated.
        // Consumes boost value when active, otherwise slowly refreshes its value.

        bool _isButtonPressed = Input.GetButton("Boost"); // left mouse button
        
        if (_isButtonPressed)
        {
            if (isBoosting) // adding force in cost of boost value
            {
                rigidBody.AddRelativeForce(Vector3.forward * boostForce, ForceMode.Force);
                boostValue -= boostPerFrame;
            }
            else if (boostValue > 0.2f) BoostState(true); // start acceleration (20% of boost value to prevent rattling)
        }

        if (isBoosting && (!_isButtonPressed || boostValue < boostPerFrame)) BoostState(false); // stop acceleration

        if (!isBoosting) boostValue += Time.fixedDeltaTime / 10; // refreshing to full value within 10 secs
        boostValue = Mathf.Clamp(boostValue, 0, 1);
        progressBar.SetFill(boostValue);
    }

    private void BoostState(bool state) // ON/OFF boosting
    {
        isBoosting = state;
        PlayVFX(state);
        PlaySFX(boostSFX, state);
    }

    private void Midair()
    {
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
        else
        {
            isMidairControlEnabled = false; // reverting to normal control
            isJumping = false;
        }
    }

    public void ResetVehicle() // Move the vehicle to tle last checkpoint, reset its transform, velocity, and shut down the engine
    {
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
        
        Transform _lastPoint = checkpointManager.GetLastPoint();
        transform.SetPositionAndRotation(_lastPoint.position, _lastPoint.rotation);
        MSVehicleManager.StartCoroutine ("StartEngineCoroutine", false);

        Input.ResetInputAxes();

        boostValue = 0;
        canJump = true;
        isJumping = false;
        isBoosting = false;
        isMidairControlEnabled = false;

        MSVehicleManager.StartCoroutine ("StartEngineCoroutine", true);
    }

    private void PlaySFX(SFX SFX, bool enabled)
    {
        if (enabled)
        {
            if (SFX.coroutine != null) StopCoroutine(SFX.coroutine); // stop current fide away coroutine
            SFX.source.volume = SFX.volume; // restore original volume
            SFX.source.Play(); // play from the start
        }
        else SFX.coroutine = StartCoroutine(FadeAudioSource.StartFade(SFX.source, 0.5f, 0)); // using volume fade away coroutine instead of stopping immediately
    }

    private void PlayVFX(bool enabled){
        if (boostVFX == null) return;

        foreach (ParticleSystem particleSystem in boostVFX){
            if (enabled == true) particleSystem.Play();
            else particleSystem.Stop();
        }
    }
}
