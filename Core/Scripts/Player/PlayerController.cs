/// kur$
using UnityEngine;
using Nicromis.GameData;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Collider))]
public class PlayerController : MonoBehaviour
{
    /// All public player settings:
    #region Public Members
    [Header("Moving")]
    [Range(1.0f, 20.0f)]
    public float m_movementSpeed = 10f;     // Player Speed
    [Range(0.05f, 10.0f)]
    public float m_crouchedMoveSpeed = 3f;  // Player Speed in crouch mode
    [Range(0.1f, 10.0f)]
    public float m_jumpPower = 3f;      // Jump Height
    public float m_gravity = -9.81f;     // Gravitational constant
    public float m_groundDistance = 0.4f;   // Ground Distance

    // Current player state
    public enum PlayerState
    {
        Standing,
        Crouching
    }

    [Space(5)]
    [Header("Player State")]
    public PlayerState m_playerState;
    public float m_playerStateSmoothing;    // State smooth speed

    public CharacterState m_standCharState;     // Player states to pass
    public CharacterState m_crouchCharState;    //     under objects

    [Space(5)]
    [Header("Looking")]
    [Range(0.01f, 10.0f)]
    public float m_sensitivityMouse = 150f;    // Look mouse speed

    #if DEBUG
    public bool m_cursorLock = true;    // Lock your cursor in center
    #endif

    public Vector2 m_cameraRotationOffset = new Vector2(-85, 50);  // Look rotation offset

    [Space(5)]
    [Header("Guns")]
    public List<Gun> m_allGuns = new List<Gun>();   // All player guns
    [HideInInspector]
    public int m_currentGunIndex = 0;   // Current weapon in hands

    [Space(5)]
    [Header("PlayerUI")]
    public PlayerUIManager m_uiManager;

    [Space(5)]
    [Header("Components")]
    public Camera m_playerCamera;   // Player Camera

    public Transform m_groundChecker;   // Ground checker position
    public LayerMask m_groundMask;  // Ground layer

    #if PLATFORM_ANDROID
    public FloatingJoystick m_joystick;     // Mobile joystick input
    public TouchField m_touchField;     // Touch area for camera rotation
    #endif

    public GameObject m_impactEffectPrefab;     // Weapon hit effect
    #endregion

    /// All private settings:
    #region Private Members
    [Tooltip("Controlling Player")]
    private float _rotationX = 0f;  // X rotation value
    private float _cameraHeight;    // Current camera height
    private float _cameraHeightVelocity;    // Camera height acceleration move
    private float _defaultMoveSpeed;     // Default Player Speed
    private float _stateCapsuleHeightVelocity;      // Capsule Height velocity

    private bool _isGrounded;
    private bool _isCrouching = false;
    private bool _isAiming = false;

    private Vector3 _velocity;  // Player gravity vector
    private Vector3 _stateCapsuleCenterVelocity;    // Collider velocity center

    [Tooltip("Weapon")]
    private Gun _currentGun;       // Current gun in player hands
    private Quaternion _originWeaponRotation;   // Origin gun rotation points

    [Tooltip("Other")]
    private GameData.Platform _platform;    // Current platform
    #endregion

    /// Methods bound to MonoBehaviour:
    #region MonoBehaviour Callbacks
    private void Awake()
    {
        _cameraHeight = m_playerCamera.transform.localPosition.y;
        _currentGun = m_allGuns[m_currentGunIndex];

        // Set platform:
        _platform = GameData.Platform.Android;
    }
    private void Start()
    {
        // Locking cursor:
        if (m_cursorLock)
            Cursor.lockState = CursorLockMode.Locked;

        // Set default movement speed:
        _defaultMoveSpeed = m_movementSpeed;

        // Set weapon sway:
        _originWeaponRotation = _currentGun.transform.localRotation;

        // Set for Platform:
        if (_platform == GameData.Platform.Windows || _platform == GameData.Platform.Editor)
        {
            m_uiManager.m_mobileControls.SetActive(false);
        }
        else
        {
            m_uiManager.m_mobileControls.SetActive(true);
        }
    }
    private void FixedUpdate()
    {
        // Checking if the player is on the ground:
        _isGrounded = Physics.CheckSphere(m_groundChecker.position, m_groundDistance, m_groundMask);

        // Rotation Axes:
        float _mouseX;
        float _mouseY;

        // Movement Axes:
        float _xMove;
        float _zMove;

        // Set axes:
        if (_platform == GameData.Platform.Windows || _platform == GameData.Platform.Editor)
        {
            // Binding axes of movement:
            _xMove = Input.GetAxis("Horizontal");
            _zMove = Input.GetAxis("Vertical");

            // Assign mouse rotation values ​​along the axes:
            _mouseX = Input.GetAxis("Mouse X") * m_sensitivityMouse * Time.deltaTime;
            _mouseY = Input.GetAxis("Mouse Y") * m_sensitivityMouse * Time.deltaTime;
        }
        else
        {
            _xMove = m_joystick.Horizontal;
            _zMove = m_joystick.Vertical;

            _mouseX = m_touchField.m_touchDist.x * m_sensitivityMouse * Time.deltaTime;
            _mouseY = m_touchField.m_touchDist.y * m_sensitivityMouse * Time.deltaTime;
        }  

        // Set frame by frame a new value of rotation along the X axis:
        _rotationX -= _mouseY;
        _rotationX = Mathf.Clamp(_rotationX, m_cameraRotationOffset.x, m_cameraRotationOffset.y);

        // Rotation:
        m_playerCamera.transform.localRotation = Quaternion.Euler(_rotationX, 0f, 0f);
        transform.Rotate(Vector3.up * _mouseX);

        // Moving:
        Vector3 _move = transform.right * _xMove + transform.forward * _zMove;
        GetComponent<CharacterController>().Move(_move * m_movementSpeed * Time.deltaTime);

        // Jumping:
        if (Input.GetButtonDown("Jump"))
            Jump();

        // Gravity:
        _velocity.y += m_gravity * Time.deltaTime;
        GetComponent<CharacterController>().Move(_velocity * Time.deltaTime);

        // Shooting:
        if (_platform == GameData.Platform.Windows || _platform == GameData.Platform.Editor)
        {
            if (Input.GetButtonDown("Fire1"))
                Shoot();
        }

        // Crouching:
        var _currentState = m_standCharState;
        if (m_playerState == PlayerState.Crouching)
        {
            _currentState = m_crouchCharState;
        }
        _cameraHeight = Mathf.SmoothDamp(m_playerCamera.transform.localPosition.y, _currentState.m_camHeight, ref _cameraHeightVelocity, m_playerStateSmoothing);
        m_playerCamera.transform.localPosition = new Vector3(0f, _cameraHeight, 0f);
        GetComponent<CharacterController>().height = Mathf.SmoothDamp(GetComponent<CharacterController>().height, _currentState.m_playerCollider.height, ref _stateCapsuleHeightVelocity, m_playerStateSmoothing);
        GetComponent<CharacterController>().center = Vector3.SmoothDamp(GetComponent<CharacterController>().center, _currentState.m_playerCollider.center, ref _stateCapsuleCenterVelocity, m_playerStateSmoothing); ;

        // Weapon Sway:
        Quaternion _Xadj = Quaternion.AngleAxis(_currentGun.m_gunSettings.m_swayIntensitivity * _mouseX, Vector3.up);
        Quaternion _Yadj = Quaternion.AngleAxis(_currentGun.m_gunSettings.m_swayIntensitivity * _mouseY, Vector3.right);
        Quaternion _targetWeaponRotation = _originWeaponRotation * _Xadj * _Yadj;
        _currentGun.transform.localRotation = Quaternion.Lerp(_currentGun.transform.localRotation, _targetWeaponRotation, Time.deltaTime * _currentGun.m_gunSettings.m_swaySmooth);

        // Weapon Aiming:
        Transform _weaponModel = _currentGun.m_gunSettings.m_modelPosition;
        Transform _weaponOriginPos = _currentGun.m_gunSettings.m_hipStatePosition;
        Transform _aimStatePos = _currentGun.m_gunSettings.m_aimStatePosition;
        if (_isAiming)
        {
            _weaponModel.position = Vector3.Lerp(_weaponModel.position, _aimStatePos.position, Time.deltaTime * _currentGun.m_gunSettings.m_aimSpeed);
        }
        else
        {
            _weaponModel.position = Vector3.Lerp(_weaponModel.position, _weaponOriginPos.position, Time.deltaTime * _currentGun.m_gunSettings.m_aimSpeed);
        }
    }
    #endregion

    /// Moblie input methods:
    #region Mobile
    public void Jump()
    {
        // Giving meaning to the jump:
        if(_isGrounded)
            _velocity.y = Mathf.Sqrt(m_jumpPower * -2f * m_gravity);
    }
    public void Crouch()
    {
        if (!_isCrouching)
        {
            m_playerState = PlayerState.Crouching;
            m_movementSpeed = m_crouchedMoveSpeed;
            _isCrouching = true;
        }
        else if (_isCrouching)
        {
            m_playerState = PlayerState.Standing;
            m_movementSpeed = _defaultMoveSpeed;
            _isCrouching = false;
        }
           
    }
    public void Shoot()
    {
        Gun _currentGun = m_allGuns[m_currentGunIndex];

        if(Time.time >= _currentGun.m_nextTimeToFire)
        {
            _currentGun.m_nextTimeToFire = Time.time + 1f / _currentGun.m_fireRate;

            RaycastHit _hit;
            if (Physics.Raycast(m_playerCamera.transform.position, m_playerCamera.transform.forward, out _hit, _currentGun.m_range))
            {
                _currentGun.m_muzzleflash.Play();

                #if DEBUG
                Debug.Log(_hit.transform.name);
                #endif

                // Give damage to bot:
                Bot _currentBot = _hit.transform.GetComponent<Bot>();
                if (_currentBot != null)
                    _currentBot.TakeDamage(_currentGun.m_damage);

                // Give force:
                if (_hit.rigidbody != null)
                    _hit.rigidbody.AddForce(-_hit.normal * _currentGun.m_penetrationPower);

                // Instantiate bullet impact:
                GameObject _impactGameobject = Instantiate(m_impactEffectPrefab, _hit.point, Quaternion.LookRotation(_hit.normal));
                Destroy(_impactGameobject, 2f);
            }
        }
    }
    public void Aim()
    {
        if (!_isAiming)
        {
            _isAiming = true;
            m_uiManager.m_crosshair.SetActive(false);
        }
        else
        {
            _isAiming = false;
            m_uiManager.m_crosshair.SetActive(true);
        }
    }
    #endregion
}

[Serializable]
public class CharacterState
{
    /// All public states settings:
    #region Public Members
    public float m_camHeight;
    public CapsuleCollider m_playerCollider;
    #endregion
}

[Serializable]
public class WeaponManager
{
    /// All public weapon settings:
    #region Public Members
    [Header("Aiming")]
    public float m_aimSpeed;    // Speed for Aiming

    public Transform m_modelPosition;       // Weapon model position
    public Transform m_aimStatePosition;      // Weapon position in aiming state
    public Transform m_hipStatePosition;      // Weapon position in origin state

    [Space(5)]
    [Header("Sway")]
    public float m_swayIntensitivity = 1f;      // Weapon Sway Velocity
    public float m_swaySmooth = 10f;      // Smooth for weapon sway
    #endregion
}
