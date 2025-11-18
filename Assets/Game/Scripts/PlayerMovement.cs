using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Ñןאניעû")]
    public Sprite frontsprite;
    public Sprite backsprite;
    public Sprite rightsprite;
    public Sprite leftsprite;
    public SpriteRenderer spriterenderer;


    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        movement.x = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;

        movement.y = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;

        if (Input.GetKey(KeyCode.D))
        {
            spriterenderer.sprite = rightsprite;

        }
        if (Input.GetKey(KeyCode.A))
        {
            spriterenderer.sprite = leftsprite;
        }

        if (Input.GetKey(KeyCode.W))
        {
            spriterenderer.sprite = backsprite;

        }
        if (Input.GetKey(KeyCode.S))
        {
            spriterenderer.sprite = frontsprite;
        }

        movement = movement.normalized;
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
