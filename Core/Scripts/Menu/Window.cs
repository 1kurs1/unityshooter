/// kur$
using UnityEngine;

namespace Nicromis.Menu { 
    public class Window : MonoBehaviour
    {
        /// All public window parameters:
        #region Public Parameters
        [Header("Name")]
        public string m_name;

        [HideInInspector]
        public bool m_isOpenned;
        #endregion

        /// Windows logic:
        #region Logic
        public void Open()
        {
            m_isOpenned = true;
            gameObject.SetActive(true);
        }
        public void Close()
        {
            m_isOpenned = false;
            gameObject.SetActive(false);
        }
        #endregion
    }
}

