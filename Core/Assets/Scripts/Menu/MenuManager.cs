/// kur$
using System.Collections;
using UnityEngine;

namespace Nicromis.Menu
{
    public class MenuManager : MonoBehaviour
    {
        /// All public Members in menu:
        #region Public Members
        public static MenuManager m_instance;

        [Header("All Windows")]
        public Window[] m_windows;
        #endregion

        /// Methods bound to MonoBehaviour:
        #region MonoBehaviour Callbacks
        private void Awake()
        {
            if (m_instance == null)
                m_instance = this;
        }
        private void Start()
        {
            StartCoroutine("Loading");
        }
        #endregion

        /// Menu Logic:
        #region Logic
        public void OpenWindow(string windowName)
        {
            for (int i = 0; i < m_windows.Length; i++)
            {
                if (m_windows[i].m_name == windowName)
                    m_windows[i].Open();
                else if (m_windows[i].m_isOpenned)
                    CloseWindow(m_windows[i]);
            }
        }
        public void OpenWindow(Window window)
        {
            for (int i = 0; i < m_windows.Length; i++)
            {
                if (m_windows[i].m_isOpenned)
                    CloseWindow(m_windows[i]);
            }
            window.Open();
        }
        public void CloseWindow(Window window)
        {
            window.Close();
        }

        IEnumerator Loading()
        {
            yield return new WaitForSeconds(2);
            OpenWindow("home");
            StopAllCoroutines();
        }
        #endregion
    }
}

