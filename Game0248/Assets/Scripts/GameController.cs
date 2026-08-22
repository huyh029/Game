using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    // =========================================================
    // GAME LOGIC
    // =========================================================

    public GameLogic gameLogic;


    // =========================================================
    // GRID
    // =========================================================

    public GameObject container;


    private string[] nameRow =
    {
        "Row1",
        "Row2",
        "Row3",
        "Row4"
    };


    private string[] nameCell =
    {
        "Cell1",
        "Cell2",
        "Cell3",
        "Cell4"
    };


    // =========================================================
    // UI
    // =========================================================

    public TextMeshProUGUI scoreText;


    public GameObject startTitle;
    public Button startButton;


    public GameObject gameOverTitle;
    public Button retryButton;


    // =========================================================
    // START
    // =========================================================
    void Awake()
    {
        gameLogic = GetComponent<GameLogic>();

        startButton.onClick.AddListener(OnStartButton);
        retryButton.onClick.AddListener(OnRetryButton);
    }
    void Start()
    {
        // Hiện màn hình Start
        SetStartScreen(true);

        // Ẩn Game Over
        SetGameOver(false);

        // Score ban đầu
        SetScore(0);

        // Xóa UI grid
        ClearContent();
    }


    // =========================================================
    // START BUTTON
    // =========================================================

    public void OnStartButton()
    {
        SetStartScreen(false);

        SetGameOver(false);

        gameLogic.StartGame();
    }


    // =========================================================
    // RETRY BUTTON
    // =========================================================

    public void OnRetryButton()
    {
        SetGameOver(false);

        gameLogic.RestartGame();
    }


    // =========================================================
    // SET GRID CONTENT
    // =========================================================

    public void SetContent(int[][] grid)
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                TextMeshProUGUI text =
                    IndexCell(i, j)
                    .GetComponent<TextMeshProUGUI>();


                if (grid[i][j] == 0)
                {
                    text.text = "";
                }
                else
                {
                    text.text =
                        grid[i][j]+"";
                }
                Debug.Log(grid[i][j]);
            }
        }
    }


    // =========================================================
    // CLEAR GRID UI
    // =========================================================

    void ClearContent()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                TextMeshProUGUI text =
                    IndexCell(i, j)
                    .GetComponent<TextMeshProUGUI>();


                text.text = "";
            }
        }
    }


    // =========================================================
    // GET CELL
    // =========================================================

    GameObject IndexCell(int i, int j)
    {
        return container.transform
            .Find(nameRow[i])
            .Find(nameCell[j])
            .gameObject;
    }


    // =========================================================
    // SCORE
    // =========================================================

    public void SetScore(int score)
    {
        scoreText.text = score.ToString();
    }


    // =========================================================
    // START SCREEN
    // =========================================================

    public void SetStartScreen(bool value)
    {
        startTitle.SetActive(value);

        startButton.gameObject.SetActive(value);
    }


    // =========================================================
    // GAME OVER
    // =========================================================

    public void SetGameOver(bool value)
    {
        gameOverTitle.SetActive(value);

        retryButton.gameObject.SetActive(value);
    }
}