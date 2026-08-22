using UnityEngine;

public class BallController : MonoBehaviour
{
    // tạo biến lưu game logic
    private GameLogic gameLogic;
    // awake : gọi hàm tìm object
    void Start()
    {
        // tìm object game logic
        gameLogic = FindObjectOfType<GameLogic>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(gameLogic == null)
        {
            Debug.Log("game logic is null");
            return;
        }
        if (collision.CompareTag("Ball"))
        {
            gameLogic.StopFlying();
        }
        else if (collision.CompareTag("TopBlock"))
        {
            gameLogic.StopFlying();
        }
        else if (collision.CompareTag("RLBlock"))
        {
            gameLogic.BounceOffWall();
        }
        else if (collision.CompareTag("DestroyZone"))
        {
            Destroy(gameObject);
        }
    }
}
