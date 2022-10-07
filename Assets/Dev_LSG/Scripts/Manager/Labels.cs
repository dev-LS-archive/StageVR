using UnityEngine;

namespace Dev_LSG.Scripts.Manager
{
    public class Labels : MonoBehaviour
    {
        public static Labels Instance;
        public Transform[] labels;
        
        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }
    }
}
