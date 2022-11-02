using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dev_LSG.Scripts.Manager
{
    public class BGMPlayer : MonoBehaviour
    {
        public AudioSource bgmAudioSource;
        public AudioClip roadBGM;
        public AudioClip construction;
        public AudioClip hammering;

        // called first
        void OnEnable()
        {
            //Debug.Log("OnEnable called");
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        // called second
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "RemovalofSafety_1")
            {
                bgmAudioSource.clip = roadBGM;
                bgmAudioSource.Play();
            }
            else if (scene.name == "RemovalofSafety_2")
            {
                bgmAudioSource.clip = roadBGM;
                bgmAudioSource.Play();
            }
            else if (scene.name == "ShoulderofSafety")
            {
                bgmAudioSource.clip = roadBGM;
                bgmAudioSource.Play();
            }
            else
            {
                bgmAudioSource.Stop();
                bgmAudioSource.clip = null;
            }
            //Debug.Log("OnSceneLoaded: " + scene.name);
            //Debug.Log(mode);
        }
        
        // called when the game is terminated
        void OnDisable()
        {
            //Debug.Log("OnDisable");
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
