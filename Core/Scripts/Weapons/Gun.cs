/// kur$
using UnityEngine;
using static WeaponManager;

public class Gun : MonoBehaviour
{
    /// All private weapon settings:
    #region Private Members
    private PlayerController _playerController;
    #endregion

    /// All public weapon settings:
    #region Public Members
    [Header("Parameters")]
    public string m_name = "Name";  // Weapon name
    public float m_damage = 36f;    // Weapon damage
    public float m_range = 100f;    // Ray distance
    public float m_fireRate = 15f;  // Break between fire
    public float m_penetrationPower = 30f;      // Weapon power
    [HideInInspector]
    public float m_nextTimeToFire = 0f;     // For fire rate

    public WeaponManager m_gunSettings;    // Weapons Settings

    [Space(5)]
    [Header("Components")]
    public ParticleSystem m_muzzleflash;
    #endregion
}
