using UnityEngine;

public class Coin : MonoBehaviour
{
    public int value = 1;

    [Header("Spin Animation")]
    public Sprite[] spinSprites; // assign 4 sprites in Inspector
    public float frameRate = 0.1f;

    private SpriteRenderer sr;
    private int currentFrame;
    private float timer;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        AnimateCoin();
    }

    private void AnimateCoin()
    {
        if (spinSprites.Length == 0) return;

        timer += Time.deltaTime;

        if (timer >= frameRate)
        {
            timer = 0f;

            currentFrame = (currentFrame + 1) % spinSprites.Length;
            sr.sprite = spinSprites[currentFrame];
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CoinManager.instance.AddCoin(value);
            Destroy(gameObject);
        }
    }
}