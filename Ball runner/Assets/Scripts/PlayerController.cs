using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rigidbody2;
    public float force = 10f;
    private bool isJumb = true;
    GameController gameController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidbody2 = GetComponent<Rigidbody2D>();
        gameController = FindAnyObjectByType<GameController>();
    }

    // Update is called once per frame
    void Update()
    {
        //.Log(isJumb);
        if (Input.GetKey(KeyCode.Space) && isJumb) {
            rigidbody2.AddForce(new Vector2(0,force), ForceMode2D.Impulse);
            isJumb = false;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("BotGround")) { 
            isJumb = true;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Block"))
        {
            gameController.GameOver();
        }
    }
}
