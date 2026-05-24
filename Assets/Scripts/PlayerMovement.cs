using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 8f;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 input;
    private Vector2 velocity;
    private Vector2 lastMoveDir;
    public bool isPossessed = false;
    private float nextDirectionChange = 0f;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isPossessed)
        {
            // CRAZY random movement - changes direction very frequently
            if (Time.time >= nextDirectionChange)
            {
                input = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                nextDirectionChange = Time.time + Random.Range(0.08f, 0.25f); // super erratic
            }
            return; // skip normal player input
        }

        // Normal player input (only runs when NOT possessed)
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        input = input.normalized;
    }

    void FixedUpdate()
    {
        // Smooth movement
        velocity = Vector2.Lerp(
            rb.linearVelocity,
            input * speed,
            0.2f
        );

        rb.linearVelocity = velocity;

        // ---- ANIMATION BASED ON VELOCITY ----
        // detect actual movement for idle
        bool isMoving = rb.linearVelocity.magnitude > 0.1f;
        animator.SetBool("IsMoving", isMoving);

        // direction should react instantly to input
        if (input != Vector2.zero)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                lastMoveDir = new Vector2(Mathf.Sign(input.x), 0);
            }
            else
            {
                lastMoveDir = new Vector2(0, Mathf.Sign(input.y));
            }
        }

        // always use last input direction for animation
        animator.SetFloat("MoveX", lastMoveDir.x);
        animator.SetFloat("MoveY", lastMoveDir.y);
    }
}