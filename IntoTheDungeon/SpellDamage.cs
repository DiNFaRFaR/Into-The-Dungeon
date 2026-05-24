using UnityEngine;

public class Spelldamage : MonoBehaviour
{
    public int damage = 10;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Handle Ghost (reveal + damage)
        GhostEnemy ghost = collision.GetComponent<GhostEnemy>();
        if (ghost != null)
        {
            ghost.TakeDamage(damage);
            Destroy(gameObject); // destroy spell projectile
            return;
        }

        // Handle Goblin
        GoblinAI goblin = collision.GetComponent<GoblinAI>();
        if (goblin != null)
        {
            goblin.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        SpiderAI spider = collision.GetComponent<SpiderAI>();
        if (spider != null)
        {
            spider.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        BatAI bat = collision.GetComponent<BatAI>();
        if (bat != null)
        {
            bat.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        DragonAI dragon = collision.GetComponent<DragonAI>();
        if (dragon != null)
        {
            dragon.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        SkeleDragonAI skeledragon = collision.GetComponent<SkeleDragonAI>();
        if (skeledragon != null)
        {
            skeledragon.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        SkeletonKnightBoss knightboss = collision.GetComponent<SkeletonKnightBoss>();
        if (knightboss != null)
        {
            knightboss.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Original slime damage
        SlimeEnemy slime = collision.GetComponent<SlimeEnemy>();
        if (slime != null)
        {
            slime.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Optional: handle other enemies or walls later
        // e.g. if (collision.CompareTag("Wall")) Destroy(gameObject);
    }
}