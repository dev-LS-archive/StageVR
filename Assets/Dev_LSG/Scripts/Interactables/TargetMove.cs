using UnityEngine;
using UnityEngine.AI;

namespace Dev_LSG.Scripts.Interactables
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class TargetMove : MonoBehaviour
    {
        // ReSharper disable once InconsistentNaming
        NavMeshAgent m_Agent;
        public Transform target;
        void Start()
        {
            m_Agent = GetComponent<NavMeshAgent>();
        }

        // Update is called once per frame
        void Update()
        {
            m_Agent.destination = target.position;
        }
    }
}
