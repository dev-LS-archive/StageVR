using UnityEngine;
using Cinemachine;

namespace Dev_LSG.Scripts.Event
{
    public class SetDollyCartSpeed : MonoBehaviour
    {
        public CinemachineDollyCart cart;

        public void SetCartSpeed(float speed)
        {
            cart.m_Speed = speed;
        }
    }
}
