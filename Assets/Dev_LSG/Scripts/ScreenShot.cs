using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Dev_LSG.Scripts
{
    public class ScreenShot : MonoBehaviour
    {
        public Camera cam;

        public void Capture(Image image)
        {
            StartCoroutine(CaptureScreen(image));
        }
        
        IEnumerator CaptureScreen(Image image)
        {
            yield return new WaitForEndOfFrame();
            
            var targetTexture = cam.targetTexture;
            var newTexture = new Texture2D(targetTexture.width, targetTexture.height, TextureFormat.RGBA32, false);
            RenderTexture.active = targetTexture;
            
            //Read pixels & PNGs should be sRGB so convert to sRGB color space when rendering in linear.
            newTexture.ReadPixels(new Rect(0, 0, targetTexture.width, targetTexture.height), 0, 0);
            if (QualitySettings.activeColorSpace == ColorSpace.Linear)
            {
                Color[] pixels = newTexture.GetPixels();
                for (int p = 0; p < pixels.Length; p++)
                {
                    pixels[p] = pixels[p].gamma;
                }
                newTexture.SetPixels(pixels);
            }
            
            newTexture.Apply();
            
            //Create Sprite
            Sprite tempSprite = Sprite.Create(newTexture, new Rect(0, 0, targetTexture.width, targetTexture.height),
                Vector2.one * 0.5f);
            image.sprite = tempSprite;
        }
    }
}
