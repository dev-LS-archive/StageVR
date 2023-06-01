using System;
using System.Linq;
using UnityEngine;

namespace Dev_LSG.Scripts
{
    public class TestOnView : MonoBehaviour
    {
        public Camera cam;
        public GameObject target;

        private bool IsVisible(Camera c, GameObject obj)
        {
            var planes = GeometryUtility.CalculateFrustumPlanes(c);
            var point = obj.transform.position;

            return planes.All(plane => !(plane.GetDistanceToPoint(point) < 0));
        }

        private void Update()
        {
            print("절두체: " + IsVisible(cam, target));
            var viewPos = cam.WorldToViewportPoint(target.transform.position);
            bool view = viewPos.x is >= 0 and <= 1 && viewPos.y is >= 0 and <= 1 && viewPos.z > 0;
            print("뷰포트: " + view);
        }
    }
}
