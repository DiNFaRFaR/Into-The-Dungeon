using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("Referenser")]
    [SerializeField] private GameObject _Spell1prefab; // Din skott-prefab fr�n Project-mappen
    [SerializeField] private Transform _Spell1Offset; // Det tomma objektet (siktet) p� din Player

    [Header("Inst�llningar")]
    [SerializeField] private float _Spell1Speed = 15f;    // Hur snabbt skottet flyger
    [SerializeField] private float _timeBetweenShots = 0.3f; // Cooldown (eldhastighet)
    [SerializeField] private float _lifeTime = 2.0f;      // <--- TIDEN INNAN DE F�RSVINNER

    private float _lastShotTime;

    void Update()
    {
        // 1. F� siktet att peka mot muspekaren
        RotateTowardsMouse();

        // 2. Skjut n�r man h�ller ner v�nster musknapp (0)
        if (Input.GetMouseButton(0))
        {
            if (Time.time >= _lastShotTime + _timeBetweenShots)
            {
                ShootSpell1();
                _lastShotTime = Time.time;
            }
        }
    }

    private void RotateTowardsMouse()
    {
        if (_Spell1Offset == null) return;

        // R�kna ut var musen �r i spelv�rlden
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // R�kna ut riktningen fr�n siktet till musen
        Vector2 direction = new Vector2(
            mousePos.x - _Spell1Offset.position.x,
            mousePos.y - _Spell1Offset.position.y
        );

        // Vrid siktets r�da pil (right) mot musen
        _Spell1Offset.right = direction;
    }

    private void ShootSpell1()
    {
        if (_Spell1prefab == null || _Spell1Offset == null) return;

        // Skapa skottet vid siktets position och rotation
        GameObject spell1 = Instantiate(_Spell1prefab, _Spell1Offset.position, _Spell1Offset.rotation);

        // Ge skottet fart fram�t (dit siktet pekar)
        Rigidbody2D rb = spell1.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = _Spell1Offset.right * _Spell1Speed;
        }

        // --- FIXEN F�R ATT DE SKA F�RSVINNA ---
        // Denna rad raderar skottet fr�n spelet efter _lifeTime sekunder
        Destroy(spell1, _lifeTime);
    }
}