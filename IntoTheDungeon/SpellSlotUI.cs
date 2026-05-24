using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Attach this to a UI Panel (the spell box).
/// The box will "flex" (scale up briefly) when UseSpell() is called.
/// </summary>
public class SpellSlotUI : MonoBehaviour
{
    [Header("Spell Info")]
    public Sprite spellIcon;

    [Header("References")]
    public Image iconImage;           // Assign spell icon Image in inspector
    public Image borderImage;         // Assign border/panel Image (optional tint)

    [Header("Flex Animation")]
    [Tooltip("How much the box scales up on use (e.g. 1.12 = 12% bigger)")]
    public float flexScale = 1.12f;

    [Tooltip("How quickly it scales up (seconds)")]
    public float flexUpDuration = 0.08f;

    [Tooltip("How quickly it returns to normal (seconds)")]
    public float flexDownDuration = 0.18f;

    [Tooltip("Color flash on use")]
    public Color flashColor = new Color(1f, 0.85f, 0.2f, 1f); // golden flash

    [Header("Cooldown (optional)")]
    public float cooldownDuration = 2f;

    // ── Private ──────────────────────────────────────────────────────────────

    private Vector3 _originalScale;
    private Color _originalBorderColor;
    private bool _isOnCooldown = false;
    private Coroutine _flexRoutine;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    void Awake()
    {
        _originalScale = transform.localScale;

        if (borderImage != null)
            _originalBorderColor = borderImage.color;

        Refresh();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Call this when the player casts the spell.</summary>
    public void UseSpell()
    {
        
        // Stop any ongoing flex so it doesn't stack
        if (_flexRoutine != null) StopCoroutine(_flexRoutine);
        _flexRoutine = StartCoroutine(FlexAnimation());

        StartCoroutine(Cooldown());
    }

    /// <summary>Refresh displayed icon.</summary>
    public void Refresh()
    {
        if (iconImage != null && spellIcon != null) iconImage.sprite = spellIcon;
    }

    // ── Coroutines ────────────────────────────────────────────────────────────

    private IEnumerator FlexAnimation()
    {
        // 1. Scale UP
        yield return ScaleTo(transform.localScale, _originalScale * flexScale,
                             flexUpDuration, borderImage, _originalBorderColor, flashColor);

        // 2. Scale DOWN back to normal
        yield return ScaleTo(transform.localScale, _originalScale,
                             flexDownDuration, borderImage, flashColor, _originalBorderColor);

        // Guarantee exact reset
        transform.localScale = _originalScale;
        if (borderImage != null) borderImage.color = _originalBorderColor;
    }

    private IEnumerator ScaleTo(Vector3 from, Vector3 to,
                                 float duration,
                                 Image img, Color colorFrom, Color colorTo)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            transform.localScale = Vector3.Lerp(from, to, smooth);
            if (img != null) img.color = Color.Lerp(colorFrom, colorTo, smooth);

            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = to;
        if (img != null) img.color = colorTo;
    }

    private IEnumerator Cooldown()
    {
        _isOnCooldown = true;
        yield return new WaitForSeconds(cooldownDuration);
        _isOnCooldown = false;
    }
}