using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    // Button Start / Retry
    [SerializeField] private Button startOrRetryButton;

    // Text hiển thị Start / Game Over
    [SerializeField] private TMP_Text startTitleOrGameOverTitle;

    // Text hiển thị điểm
    [SerializeField] private TMP_Text scoreText;

    // Điểm hiện tại
    private int score;

    private void Start()
    {
        // Dừng thời gian game
        Time.timeScale = 0f;

        // Đặt điểm về 0
        score = 0;

        // Cập nhật điểm
        UpdateScoreText();

        // Hiện Game Over title
        startTitleOrGameOverTitle.gameObject.SetActive(true);

        // Hiện button
        startOrRetryButton.gameObject.SetActive(true);

        // Đổi text
        startTitleOrGameOverTitle.text = "START";
        startOrRetryButton.GetComponentInChildren<TMP_Text>().text = "START";

        // Xóa tất cả listener cũ
        startOrRetryButton.onClick.RemoveAllListeners();

        // Add sự kiện Start Game
        startOrRetryButton.onClick.AddListener(StartGame);
    }

    // =========================
    // START GAME
    // =========================

    private void StartGame()
    {
        // Ẩn title
        startTitleOrGameOverTitle.gameObject.SetActive(false);

        // Ẩn button
        startOrRetryButton.gameObject.SetActive(false);

        // Cho game chạy
        Time.timeScale = 1f;
    }

    // =========================
    // ADD SCORE
    // =========================

    public void AddScore(int amount)
    {
        score += amount;

        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        scoreText.text = score.ToString();
    }

    // =========================
    // GAME OVER
    // =========================

    public void EndGame()
    {
        // Dừng game

        // Hiện title
        startTitleOrGameOverTitle.gameObject.SetActive(true);

        // Hiện button
        startOrRetryButton.gameObject.SetActive(true);

        // Đổi text
        startTitleOrGameOverTitle.text = "GAME OVER";
        startOrRetryButton.GetComponentInChildren<TMP_Text>().text = "RETRY";

        // Xóa listener cũ
        startOrRetryButton.onClick.RemoveAllListeners();

        // Add sự kiện Retry
        startOrRetryButton.onClick.AddListener(Retry);
    }

    // =========================
    // RETRY
    // =========================

    private void Retry()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}