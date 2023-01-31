using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dev_LSG.Scripts.Manager
{
    public class BGMPlayerTetrapod : MonoBehaviour
    {
        public AudioSource[] bgmAudioSource;

        // called first
        void OnEnable()
        {
            //Debug.Log("OnEnable called");
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        // called second
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            foreach (var audioSource in bgmAudioSource)
            {
                audioSource.loop = true;
                audioSource.volume = 0.2f;
            }
            //print(scene.name);

            if (scene.name == "MainMenu_Sea")
            {
                foreach (var audioSource in bgmAudioSource)
                {
                    audioSource.Play();
                }
            }

            //Debug.Log("OnSceneLoaded: " + scene.name);
            //Debug.Log(mode);
        }
        
        void OnDisable()
        {
            //Debug.Log("OnDisable");
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
