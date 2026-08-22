using UnityEngine;

public class InscreaseZone : MonoBehaviour
{
    GameController controller;
    private void Start()
    {
        controller = FindAnyObjectByType<GameController>();
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("IncreaseScoreZone"))
        {
            controller.IncreaseScore();
        }
    }
}
