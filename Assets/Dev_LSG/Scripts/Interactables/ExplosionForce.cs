using System.Collections;
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
            foreach (var rig in rigidbodies)
            {
                var explosionPos = transform.position;
                rig.isKinematic = false;
                rig.AddExplosionForce(power, explosionPos, explosionRadius, upward);
                //rig.AddRelativeForce(Vector3.up * Random.Range(upward - 50, upward + 50));
                //rig.AddRelativeForce(Vector3.right * Random.Range(power - 50, power + 50));
                rig.AddRelativeForce(Vector3.right * power);
            }
        }
    }
}
