using UnityEngine;

// ReSharper disable once IdentifierTypo
namespace Dev_LSG.Scripts.Interactables
{
    public class SetMaterial : MonoBehaviour
    {
        public MeshRenderer render;
        public Material origin;
        public Material change;

        public void OriginMat()
        {
            if (render.material != origin)
            {
                render.material = origin;
            }
        }

        public void ChangeMat()
        {
            if (render.material != change)
            {
                render.material = change;
            }
        }
    }
}
