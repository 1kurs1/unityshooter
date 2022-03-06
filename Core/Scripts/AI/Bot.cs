/// kur$
using UnityEngine;

public class Bot : MonoBehaviour
{
    /// All public bot settings:
    #region Public Members
    [Header("Parameters")]
    public float m_health = 100f;    // Bot health
    #endregion

    /// Damaging this
    #region Take Shoot
    public void TakeDamage(float amount)
    {
        if (m_health > 0f)
            m_health -= amount;
        else
            Die();
    }
    private void Die()
    {
        Destroy(gameObject);
    }
    #endregion
}
