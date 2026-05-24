using UnityEngine;

public class Spell1 : MonoBehaviour
{
    public GameObject hitEffect;
    public AudioClip hitSound;
    public float volume = 1f;

    void OnTriggerEnter2D(Collider2D other)
    {
        GhostEnemy ghost = other.GetComponent<GhostEnemy>();
        if (ghost != null)
        {
            ghost.HitByMagic();
        }

        if (other.CompareTag("Enemy") || other.CompareTag("Wall"))
        {

            AudioSource.PlayClipAtPoint(hitSound, transform.position, volume);


            if (hitEffect != null)
            {
                GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
                ParticleSystem ps = effect.GetComponent<ParticleSystem>();
                if (ps != null)
                    Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
                else
                    Destroy(effect, 1f);
            }

            GetComponent<Collider2D>().enabled = false;
            Destroy(gameObject, 0.5f);
        }
    }
}