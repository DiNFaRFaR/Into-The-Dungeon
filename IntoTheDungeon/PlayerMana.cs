using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerMana : MonoBehaviour
{
    public PlayerMana playerMana;
    public float currentMana;
    public float maxMana = 100f;

    public float regenRate = 10f;

    public Slider manaSlider;
    public TMP_Text manaText;

    public float changeDuration = 0.3f;

    private Coroutine manaRoutine;

    void Start()
    {
        currentMana = maxMana;

        manaSlider.maxValue = maxMana;
        manaSlider.value = currentMana;
        manaText.text = currentMana + " / " + maxMana;
    }

    void Update()
    {
        RegenerateMana();
    }

    void RegenerateMana()
    {
        if (currentMana < maxMana)
        {
            ChangeMana(regenRate * Time.deltaTime);
        }
    }

    public bool UseMana(float amount)
    {
        if (currentMana >= amount)
        {
            ChangeMana(-amount);
            return true;
        }

        return false;
    }

    public void ChangeMana(float amount)
    {
        float oldMana = currentMana;

        currentMana += amount;
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);

        if (manaRoutine != null)
            StopCoroutine(manaRoutine);

        manaRoutine = StartCoroutine(AnimateMana(oldMana, currentMana));
    }

    private IEnumerator AnimateMana(float from, float to)
    {
        float elapsed = 0f;

        while (elapsed < changeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / changeDuration;

            float smoothValue = Mathf.Lerp(from, to, t);

            manaSlider.value = smoothValue;
            manaText.text = Mathf.RoundToInt(smoothValue) + " / " + maxMana;

            yield return null;
        }

        manaSlider.value = to;
        manaText.text = Mathf.RoundToInt(to) + " / " + maxMana;
    }
}