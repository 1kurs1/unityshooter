/// kur$
using UnityEngine;

namespace Nicromis.GameData
{
    public class GameData : ScriptableObject
    {
        /// All public game settings:
        #region Public Members
        // Instance:
        public static GameData m_instance;

        // Change platform:
        public enum Platform{
            Android = 0,
            Editor = 1,
            IOS = 2,
            Windows = 3
        }
        [Header("Game Platform")]
        public Platform m_platform = 0;
        #endregion

        /// Methods bound to MonoBehaviour:
        #region MonoBehaviourCallbacks
        private void Awake()
        {
            if (m_instance == null)
                m_instance = this;
        }
        #endregion
    }
}

