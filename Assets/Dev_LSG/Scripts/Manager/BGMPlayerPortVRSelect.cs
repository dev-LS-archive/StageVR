using UnityEngine;
using UnityEngine.SceneManagement;

namespace Dev_LSG.Scripts.Manager
{
    public class BGMPlayerPortVRSelect : MonoBehaviour
    {
        public AudioSource bgmAudioSource;
        public AudioSource waveAudioSource;
        public AudioSource buildingAudioSource;
        public AudioClip waves;
        public AudioClip buildingAudio;
        public AudioClip wind;

        // called first
        void OnEnable()
        {
            //Debug.Log("OnEnable called");
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        // called second
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            print(scene.name);
            bgmAudioSource.loop = true;
            bgmAudioSource.volume = 0.2f;
            
            waveAudioSource.loop = true;
            waveAudioSource.volume = 0.2f;
            
            buildingAudioSource.loop = true;
            buildingAudioSource.volume = 0.2f;
            
            if (scene.name == "MainMenu_Port" || scene.name == "SelectMenu_Port" || scene.name == "SelectMenu_Port" || scene.name == "SelectMenu_Port"
                || scene.name == "GangForm_Play"|| scene.name == "GangForm_Explain")
            {
                if (bgmAudioSource.isPlaying) 
                    Stop(bgmAudioSource);
                if (waveAudioSource.clip != waves)
                    Play(waveAudioSource, waves);
                if (buildingAudioSource.clip != buildingAudio)
                    Play(buildingAudioSource, buildingAudio);
            }
            else if (scene.name == "Bollard-pull_Tugboat_TBM" || scene.name == "Bollard-pull_Tugboat" || scene.name == "Bollard-pull_Tugboat_Explain")
            {
                if (bgmAudioSource.isPlaying) 
                    Stop(bgmAudioSource);
                if (waveAudioSource.clip != waves)
                    Play(waveAudioSource, waves);
                if (buildingAudioSource.isPlaying) 
                    Stop(buildingAudioSource);
            }
            else if (scene.name == "Fall_Test_TBM" || scene.name == "Fall_Test" || scene.name == "Fall_Test_Explain")
            {
                if (bgmAudioSource.isPlaying) 
                    Stop(bgmAudioSource);
                if (waveAudioSource.clip != waves)
                    Play(waveAudioSource, waves);
                if (buildingAudioSource.clip != buildingAudio)
                    Play(buildingAudioSource, buildingAudio);
            }
            else if (scene.name == "Pipe_Test_TBM" || scene.name == "Pipe_Test" || scene.name == "Pipe_Test_Explain")
            {
                if (bgmAudioSource.isPlaying) 
                    Stop(bgmAudioSource);
                if (waveAudioSource.clip != waves)
                    Play(waveAudioSource, waves);
                if (buildingAudioSource.clip != buildingAudio)
                    Play(buildingAudioSource, buildingAudio);
            }
            else
            {
                Stop(bgmAudioSource);
                Stop(waveAudioSource);
                Stop(buildingAudioSource);
            }
            //Debug.Log("OnSceneLoaded: " + scene.name);
            //Debug.Log(mode);
        }

        void Play(AudioSource source,AudioClip clip)
        {
            source.clip = clip;
            source.Play();
        }

        void Stop(AudioSource source)
        {
            source.Stop();
            source.clip = null;
            source.loop = false;
            source.volume = 1f;
        }
        // called when the game is terminated
        void OnDisable()
        {
            //Debug.Log("OnDisable");
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
