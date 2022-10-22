using UnityEngine;

namespace HurricaneVR.Framework.Components
{
    public class HVRDontDestroy : MonoBehaviour
    {
        private static HVRDontDestroy _instance;
        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
