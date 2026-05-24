using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [System.Serializable]
    public class SceneMusic
    {
        public string sceneName;
        public AudioClip clip;
    }

    [Header("Music Per Scene")]
    public SceneMusic[] sceneMusics;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    private AudioSource musicSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        musicSource = GetComponent<AudioSource>();

        // Restore saved volume on startup
        float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        ApplyVolume(savedVolume);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (SceneMusic sm in sceneMusics)
        {
            if (sm.sceneName == scene.name && sm.clip != null)
            {
                if (musicSource.clip == sm.clip) return; // already playing, don't restart
                musicSource.clip = sm.clip;
                musicSource.Play();
                return;
            }
        }

        musicSource.Stop(); // no music assigned to this scene
    }

    public void SetMusicVolume(float value)
    {
        ApplyVolume(value);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public float GetSavedVolume()
    {
        return PlayerPrefs.GetFloat("MusicVolume", 1f);
    }

    private void ApplyVolume(float value)
    {
        float dB = value > 0.0001f ? Mathf.Log10(value) * 20f : -80f;
        audioMixer.SetFloat("MusicVolume", dB);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}