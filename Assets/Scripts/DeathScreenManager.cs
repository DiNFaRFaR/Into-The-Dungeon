using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class DeathScreenManager : MonoBehaviour
{
    public static DeathScreenManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Canvas _deathCanvas;
    [SerializeField] private Image _blackOverlay;
    [SerializeField] private TMP_Text _youDiedText;
    [SerializeField] private CanvasGroup _buttonPanel;
    [SerializeField] private Button _mainMenuButton;
    [SerializeField] private Button _quitButton;

    [Header("Scene Names")]
    [SerializeField] private string _mainMenuSceneName = "Main Menu";

    [Header("Audio")]
    [SerializeField] private AudioClip _deathMusic;
    [SerializeField] private float _deathMusicVolume = 1f;
    private AudioSource _audioSource;

    [Header("Timing")]
    [SerializeField] private float _initialDelay = 0.6f;
    [SerializeField] private float _overlayFadeDuration = 1.8f;
    [SerializeField] private float _textDelay = 0.5f;
    [SerializeField] private float _textFadeDuration = 1.6f;
    [SerializeField] private float _buttonDelay = 2.2f;
    [SerializeField] private float _buttonFadeDuration = 0.8f;

    [Header("Text Animation")]
    [SerializeField] private float _textStartScale = 1.15f;
    [SerializeField] private float _textEndScale = 1.0f;

    private bool _isShowing = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;

        if (_deathCanvas != null)
            _deathCanvas.gameObject.SetActive(false);
    }

    void Start()
    {
        if (_mainMenuButton != null) _mainMenuButton.onClick.AddListener(GoToMainMenu);
        if (_quitButton != null) _quitButton.onClick.AddListener(QuitGame);
    }

    public void ShowDeathScreen()
    {
        if (_isShowing) return;
        _isShowing = true;
        Time.timeScale = 0f;
        StartCoroutine(DeathSequence());
    }

    public void HideDeathScreen()
    {
        StopAllCoroutines();
        _isShowing = false;
        Time.timeScale = 1f;
        if (_deathCanvas != null)
            _deathCanvas.gameObject.SetActive(false);
    }

    private IEnumerator DeathSequence()
    {
        _deathCanvas.gameObject.SetActive(true);

        Color overlayColor = _blackOverlay.color;
        overlayColor.a = 0f;
        _blackOverlay.color = overlayColor;

        Color textColor = _youDiedText.color;
        textColor.a = 0f;
        _youDiedText.color = textColor;
        _youDiedText.transform.localScale = Vector3.one * _textStartScale;

        if (_buttonPanel != null)
        {
            _buttonPanel.alpha = 0f;
            _buttonPanel.interactable = false;
            _buttonPanel.blocksRaycasts = false;
        }

        yield return new WaitForSecondsRealtime(_initialDelay);

        if (_deathMusic != null)
        {
            _audioSource.clip = _deathMusic;
            _audioSource.volume = _deathMusicVolume;
            _audioSource.Play();
        }

        yield return StartCoroutine(FadeOverlay(0f, 0.85f, _overlayFadeDuration));
        yield return new WaitForSecondsRealtime(_textDelay);
        yield return StartCoroutine(FadeAndScaleText(_textFadeDuration));
        yield return new WaitForSecondsRealtime(_buttonDelay);
        yield return StartCoroutine(FadeButtonPanel(_buttonFadeDuration));
    }

    private IEnumerator FadeOverlay(float from, float to, float duration)
    {
        float elapsed = 0f;
        Color c = _blackOverlay.color;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(from, to, elapsed / duration);
            _blackOverlay.color = c;
            yield return null;
        }
        c.a = to;
        _blackOverlay.color = c;
    }

    private IEnumerator FadeAndScaleText(float duration)
    {
        float elapsed = 0f;
        Color c = _youDiedText.color;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            c.a = t;
            _youDiedText.color = c;
            _youDiedText.transform.localScale = Vector3.one * Mathf.Lerp(_textStartScale, _textEndScale, t);
            yield return null;
        }
        c.a = 1f;
        _youDiedText.color = c;
        _youDiedText.transform.localScale = Vector3.one * _textEndScale;
    }

    private IEnumerator FadeButtonPanel(float duration)
    {
        if (_buttonPanel == null) yield break;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _buttonPanel.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }
        _buttonPanel.alpha = 1f;
        _buttonPanel.interactable = true;
        _buttonPanel.blocksRaycasts = true;
    }

    private void GoToMainMenu()
    {
        Time.timeScale = 1f;
        _isShowing = false;
        SceneManager.LoadScene(_mainMenuSceneName);
        Debug.Log("Loaded main menu..");
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}