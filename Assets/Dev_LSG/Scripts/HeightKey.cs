using System;
using System.Globalization;
using HurricaneVR.Framework.Core.Player;
using TMPro;
using UnityEngine;

namespace Dev_LSG.Scripts
{
    public class HeightKey : MonoBehaviour
    {
        private float _heightKey;
        public TextMeshProUGUI uiHeight;
        [SerializeField] private HVRCameraRig cameraRig;

        private void Start()
        {
            if (PlayerPrefs.HasKey("SaveHVRHeight"))
                UpdateHeight();
        }

        public void UpdateHeight()
        {
            _heightKey = PlayerPrefs.GetFloat("SaveHVRHeight");
            uiHeight.text = Math.Round(_heightKey,3).ToString(CultureInfo.InvariantCulture);
        }
    }
}
