using UnityEngine;
using RayFire;

namespace Dev_LSG.Scripts.Event
{
    public class InitializationEventRayFire : MonoBehaviour
    {
        public RayfireRigid rigidComponent;

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown ("space") == true)
            {
                if (rigidComponent != null)
                {
                    InitializationEvent();
                }
            }
        
        }

        public void InitializationEvent()
        {
            if (rigidComponent != null)
            {
                rigidComponent.simulationType = SimType.Dynamic;
                rigidComponent.demolitionType = DemolitionType.Runtime;
                rigidComponent.objectType     = ObjectType.Mesh;
                rigidComponent.Initialize();
            }
        }
    }
}
