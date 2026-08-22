using System.Collections.Generic;
using UnityEngine;

public class GameLevel : MonoBehaviour
{
    private const string LEVEL_FOLDER = "Levels";


    // =========================================================
    // LEVEL LIST
    // =========================================================

    private List<LevelData> levels =
        new List<LevelData>();


    // =========================================================
    // CURRENT LEVEL
    // =========================================================

    private int currentLevelIndex;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        LoadLevels();
    }


    // =========================================================
    // LOAD ALL LEVELS
    // =========================================================

    private void LoadLevels()
    {
        levels.Clear();

        LevelData[] loadedLevels =
            Resources.LoadAll<LevelData>(LEVEL_FOLDER);

        levels.AddRange(loadedLevels);

        // Sắp xếp theo levelNumber
        levels.Sort(
            (a, b) =>
                a.levelNumber.CompareTo(
                    b.levelNumber
                )
        );
    }


    // =========================================================
    // GET LEVEL
    // =========================================================

    public LevelData GetLevel(int index)
    {
        if (index < 0 || index >= levels.Count)
            return null;

        return levels[index];
    }


    // =========================================================
    // GET CURRENT LEVEL
    // =========================================================

    public LevelData GetCurrentLevel()
    {
        return GetLevel(currentLevelIndex);
    }


    // =========================================================
    // LOAD LEVEL
    // =========================================================

    public LevelData LoadLevel(int index)
    {
        if (index < 0 || index >= levels.Count)
            return null;

        currentLevelIndex = index;

        return levels[currentLevelIndex];
    }


    // =========================================================
    // NEXT LEVEL
    // =========================================================

    public LevelData NextLevel()
    {
        if (!HasNextLevel())
            return null;

        currentLevelIndex++;

        return GetCurrentLevel();
    }


    // =========================================================
    // RETRY
    // =========================================================

    public LevelData Retry()
    {
        return GetCurrentLevel();
    }


    // =========================================================
    // TOTAL LEVELS
    // =========================================================

    public int TotalLevels()
    {
        return levels.Count;
    }


    // =========================================================
    // HAS NEXT LEVEL
    // =========================================================

    public bool HasNextLevel()
    {
        return currentLevelIndex + 1 < levels.Count;
    }


    // =========================================================
    // CURRENT INDEX
    // =========================================================

    public int GetCurrentLevelIndex()
    {
        return currentLevelIndex;
    }
}