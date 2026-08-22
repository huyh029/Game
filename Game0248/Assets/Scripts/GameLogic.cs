using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    // =========================================================
    // GRID
    // =========================================================

    private int[][] grid =
    {
        new int[] { 0, 0, 0, 0 },
        new int[] { 0, 0, 0, 0 },
        new int[] { 0, 0, 2, 0 },
        new int[] { 0, 0, 0, 4 }
    };

    private int score = 0;

    private bool isPlaying = false;

    // GameController
    public GameController gameController;


    // =========================================================
    // UNITY
    // =========================================================

    void Start()
    {
        // Chưa bắt đầu game
        isPlaying = false;
    }


    void Update()
    {
        if (!isPlaying)
            return;

        HandleInput();

    }


    // =========================================================
    // INPUT
    // =========================================================

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Rotate");
            RotateGrid();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveLeft();
        }

        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            // Xoay 180°
            RotateGrid();
            RotateGrid();

            MoveLeft();

            // Xoay lại
            RotateGrid();
            RotateGrid();
        }

        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            // Xoay 270°
            RotateGrid();
            RotateGrid();
            RotateGrid();

            MoveLeft();

            // Xoay lại
            RotateGrid();
        }

        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            // Xoay 90°
            RotateGrid();

            MoveLeft();

            // Xoay lại
            RotateGrid();
            RotateGrid();
            RotateGrid();
        }
    }


    // =========================================================
    // START GAME
    // =========================================================

    public void StartGame()
    {
        ResetGame();

        isPlaying = true;

        SpawnRandomTile();
        SpawnRandomTile();

        UpdateUI();

        Debug.Log("GAME START");
    }


    // =========================================================
    // RESTART GAME
    // =========================================================

    public void RestartGame()
    {
        ResetGame();

        isPlaying = true;

        SpawnRandomTile();
        SpawnRandomTile();

        UpdateUI();

        Debug.Log("GAME RESTART");
    }


    // =========================================================
    // RESET GAME
    // =========================================================

    void ResetGame()
    {
        score = 0;

        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                grid[row][col] = 0;
            }
        }
    }


    // =========================================================
    // MOVE LEFT
    // =========================================================
    bool MoveLeft()
    {
        bool moved = false;

        // Random bool: true hoặc false
        bool rand = Random.value < 0.5f;

        for (int row = 0; row < 4; row++)
        {
            // =========================================
            // 1. LƯU ROW CŨ
            // =========================================

            int[] oldRow = (int[])grid[row].Clone();


            // =========================================
            // 2. GOM CÁC SỐ KHÁC 0
            // =========================================

            List<int> numbers = new List<int>();

            for (int col = 0; col < 4; col++)
            {
                if (grid[row][col] != 0)
                {
                    numbers.Add(grid[row][col]);
                }
            }


            // =========================================
            // 3. MERGE
            // =========================================

            List<int> merged = new List<int>();

            int i = 0;

            while (i < numbers.Count)
            {
                if (i + 1 < numbers.Count &&
                    numbers[i] == numbers[i + 1])
                {
                    int value = numbers[i] * 2;

                    merged.Add(value);

                    IncreaseScore(value);

                    i += 2;
                }
                else
                {
                    merged.Add(numbers[i]);

                    i++;
                }
            }


            // =========================================
            // 4. ĐƯA VỀ TRÁI
            // =========================================

            for (int col = 0; col < 4; col++)
            {
                if (col < merged.Count)
                {
                    grid[row][col] = merged[col];
                }
                else
                {
                    grid[row][col] = 0;
                }
            }


            // =========================================
            // 5. KIỂM TRA CÓ THAY ĐỔI
            // =========================================

            for (int col = 0; col < 4; col++)
            {
                if (oldRow[col] != grid[row][col])
                {
                    moved = true;
                    break;
                }
            }
        }


        // =========================================
        // 6. NẾU CÓ DI CHUYỂN
        // =========================================

        if (moved)
        {
            // rand đang là bool random
            Debug.Log("Random bool: " + rand);

            SpawnRandomTile();

            UpdateUI();

            if (IsGameOver())
            {
                FinishGame();
            }
        }

        return moved;
    }

    // =========================================================
    // INCREASE SCORE
    // =========================================================

    void IncreaseScore(int value)
    {
        score += value;

        Debug.Log("Score +" + value);
        Debug.Log("Total Score: " + score);
    }


    // =========================================================
    // SPAWN RANDOM TILE
    // =========================================================

    void SpawnRandomTile()
    {
        List<Vector2Int> emptyCells =
            new List<Vector2Int>();


        // Tìm ô trống
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                if (grid[row][col] == 0)
                {
                    emptyCells.Add(
                        new Vector2Int(row, col)
                    );
                }
            }
        }


        // Không còn ô trống
        if (emptyCells.Count == 0)
            return;


        // Chọn vị trí random
        Vector2Int randomCell =
            emptyCells[
                Random.Range(
                    0,
                    emptyCells.Count
                )
            ];


        // 90% = 2
        // 10% = 4
        int value =
            Random.value < 0.9f
            ? 2
            : 4;


        grid[randomCell.x][randomCell.y] = value;
    }


    // =========================================================
    // ROTATE GRID 90°
    // =========================================================

    void RotateGrid()
    {
        int[][] newGrid =
        {
            new int[] { 0, 0, 0, 0 },
            new int[] { 0, 0, 0, 0 },
            new int[] { 0, 0, 0, 0 },
            new int[] { 0, 0, 0, 0 }
        };


        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                newGrid[col][3 - row] =
                    grid[row][col];
            }
        }


        grid = newGrid;
        gameController.SetContent( grid );
    }


    // =========================================================
    // CHECK GAME OVER
    // =========================================================

    bool IsGameOver()
    {
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                // Còn ô trống
                if (grid[row][col] == 0)
                {
                    return false;
                }


                // Có thể merge sang phải
                if (col < 3 &&
                    grid[row][col] ==
                    grid[row][col + 1])
                {
                    return false;
                }


                // Có thể merge xuống
                if (row < 3 &&
                    grid[row][col] ==
                    grid[row + 1][col])
                {
                    return false;
                }
            }
        }


        return true;
    }


    // =========================================================
    // FINISH GAME
    // =========================================================

    void FinishGame()
    {
        isPlaying = false;

        Debug.Log("GAME OVER");

        if (gameController != null)
        {
            gameController.SetGameOver(true);
        }
    }


    // =========================================================
    // UPDATE UI
    // =========================================================

    void UpdateUI()
    {
        if (gameController == null)
            return;

        gameController.SetContent(grid);

        gameController.SetScore(score);
    }


    // =========================================================
    // GETTERS
    // =========================================================

    public bool IsPlaying()
    {
        return isPlaying;
    }

    public int GetScore()
    {
        return score;
    }
}