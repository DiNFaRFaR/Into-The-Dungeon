using UnityEngine;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    public Slider musicSlider;

    void OnEnable()
    {
        musicSlider.value = MusicManager.Instance.GetSavedVolume();
        musicSlider.onValueChanged.RemoveAllListeners();
        musicSlider.onValueChanged.AddListener(MusicManager.Instance.SetMusicVolume);
    }
}