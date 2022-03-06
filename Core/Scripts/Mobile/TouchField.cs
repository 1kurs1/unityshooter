/// kur$
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchField : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	/// All public touch settings:
	#region Public Members
	[HideInInspector]
	public Vector2 m_touchDist;     // Touch distance
	[HideInInspector]
	public Vector2 m_pointerOld;	// Pointer
	[HideInInspector]
	protected int m_pointerId;      // Pointer identification
	[HideInInspector]
	public bool m_pressed;		// Pressed check
    #endregion

    /// Methods bound to MonoBehaviour:
    #region MonoBehaviour Callbacks
    private void Update()
	{
		// Touch check:
		if (m_pressed)
		{
			if (m_pointerId >= 0 && m_pointerId < Input.touches.Length)
			{
				m_touchDist = Input.touches[m_pointerId].position - m_pointerOld;
				m_pointerOld = Input.touches[m_pointerId].position;
			}
			else
			{
				m_touchDist = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - m_pointerOld;
				m_pointerOld = Input.mousePosition;
			}
		}
		else
		{
			m_touchDist = new Vector2();
		}
	}
    #endregion

    /// Touch methods:
    #region Touch
    public void OnPointerDown(PointerEventData eventData)
	{
		// Making a touch:
		m_pressed = true;
		m_pointerId = eventData.pointerId;
		m_pointerOld = eventData.position;
	}


	public void OnPointerUp(PointerEventData eventData)
	{
		// No more touching:
		m_pressed = false;
	}
	#endregion
}