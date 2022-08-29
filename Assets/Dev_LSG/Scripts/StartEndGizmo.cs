using UnityEngine;

namespace Dev_LSG.Scripts
{
    public class StartEndGizmo : MonoBehaviour
    {
        [Header("Direction")]
        [SerializeField] private Transform start = null;
        [SerializeField] private Transform end = null;
        
        private void OnDrawGizmos()
        {
            /*Shows the general direction of the interaction*/
            if (start && end)
                Gizmos.DrawLine(start.position, end.position);
        }
    }
}
