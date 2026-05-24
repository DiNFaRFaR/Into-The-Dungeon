using UnityEngine;
#if UNITY_STANDALONE_WIN
using UnityEngine.Windows.Speech;
#endif
using System.Collections.Generic;
using System.Linq;

public class VoiceSpellCasting : MonoBehaviour
{
    [Header("Spell Prefabs")]
    public GameObject fireballPrefab;
    public GameObject iceshardPrefab;
    public GameObject firewallPrefab;

    [Header("Firewall Spell")]
    public GameObject firewallControllerPrefab;

    [Header("References")]
    public Transform player;
    public PlayerController playerController;

    [Header("Spell Settings")]
    public float fireballSpeed = 8f;
    public float iceshardSpeed = 8f;

    public float fireballManaCost = 20f;
    public float iceshardManaCost = 10f;
    public float firewallManaCost = 30f;

    public SpellSlotUI spellSlotIce;
    public SpellSlotUI spellSlotBall;
    public SpellSlotUI spellSlotWall;


#if UNITY_STANDALONE_WIN
    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, System.Action> commands = new Dictionary<string, System.Action>();
#endif

    void Start()
    {
        // Röstigenkänning (behålls som den är)
#if UNITY_STANDALONE_WIN
        try
        {
            commands.Add("fireball", CastFireball);
            commands.Add("ice shard", CastIceShard);
            commands.Add("fire ball", CastFireball);
            commands.Add("iceshard", CastIceShard);
            commands.Add("Firewall", CastFirewall);
            commands.Add("Fire wall", CastFirewall);

            keywordRecognizer = new KeywordRecognizer(commands.Keys.ToArray());
            keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
            keywordRecognizer.Start();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Voice commands not available: " + e.Message);
        }
#endif
    }

    // --- NYTT: Kontrollera knapptryck i Update ---
    void Update()
    {
        // Tryck 1 för Ice Shard
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            CastIceShard();
            spellSlotIce.UseSpell();
        }

        // Tryck 2 för Fireball
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            CastFireball();
            spellSlotBall.UseSpell();
        }


        // Tryck 3 för Firewall
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            CastFirewall();
            spellSlotWall.UseSpell();
        }
    }

#if UNITY_STANDALONE_WIN
    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        if (commands.ContainsKey(args.text))
        {
            commands[args.text].Invoke();
        }
    }
#endif

    // --- Spell-logik (Samma som innan, men nu nåbara via knappar) ---

    public void CastFirewall()
    {
        if (!playerController.UseMana(firewallManaCost))
        {
            Debug.Log("Not enough mana!");
            return;
        }

        GameObject controller = Instantiate(firewallControllerPrefab, Vector3.zero, Quaternion.identity);
        controller.GetComponent<AoESpell>().StartFirewall();
    }

    public void CastFireball()
    {
        if (!playerController.UseMana(fireballManaCost))
        {
            Debug.Log("Not enough mana!");
            return;
        }
        SpawnSpell(fireballPrefab, fireballSpeed);
    }

    public void CastIceShard()
    {
        if (!playerController.UseMana(iceshardManaCost))
        {
            Debug.Log("Not enough mana!");
            return;
        }
        SpawnSpell(iceshardPrefab, iceshardSpeed);
    }

    void SpawnSpell(GameObject spellPrefab, float speed)
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        Vector2 direction = (mousePosition - player.position).normalized;

        GameObject spell = Instantiate(spellPrefab, player.position, Quaternion.identity);
        Rigidbody2D rb = spell.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        spell.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void OnDestroy()
    {
#if UNITY_STANDALONE_WIN
        if (keywordRecognizer != null)
        {
            if (keywordRecognizer.IsRunning)
                keywordRecognizer.Stop();
            keywordRecognizer.Dispose();
        }
#endif
    }
}