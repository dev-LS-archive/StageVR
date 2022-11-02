using UnityEngine;

namespace Dev_LSG.Scripts.Interactables
{
    public class ExplosionForce : MonoBehaviour
    {
        [SerializeField] private  Rigidbody[] rigidbodies;
        [SerializeField] private  float power = 10.0F;
        [SerializeField] private float explosionRadius = 5f;
        [SerializeField] private float upward = 3f;

        [ContextMenu("Explosion")]
        public void Explosion()
        {
            Vector3 explosionPos = transform.position;

            foreach (var rig in rigidbodies)
            {
                rig.AddExplosionForce(power, explosionPos, explosionRadius, upward);
            }
        }
    }
}
