using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public Button startOrRetryButton;
    public TMP_Text statusGameTitle;
    public TMP_Text scoreText;
    public float timeSpawn = 2f;
    public GameObject block;
    public GameObject cloud;

    private int score = 0;
    private float m_timeSpawn;
    private float m_timeSpawnCloud;
    private float statusGame = 1; //1 : start , 2: retry
    public float maxSpeedForTimeScale = 5;
    public float timeIncreaseLevel = 10;
    private List<GameObject> blocks =  new List<GameObject>();
    private bool isFinish = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_timeSpawn = timeSpawn;
        m_timeSpawnCloud = 3 * timeSpawn;
        Time.timeScale = 0;
        startOrRetryButton.onClick.AddListener(ContiuneGame);
    }

    // Update is called once per frame
    void Update()
    {
        if (isFinish) return;
        if (m_timeSpawn > timeSpawn) {
            SpawnBlock();
            m_timeSpawn = 0;
        }
        else
        {
            m_timeSpawn += Time.deltaTime;
        }
        if (m_timeSpawnCloud > timeSpawn * 3)
        {
            SpawnCloud();
            m_timeSpawnCloud = 0;
        }
        else
        {
            m_timeSpawnCloud += Time.deltaTime;
        }
        if (Time.timeScale < maxSpeedForTimeScale)
        {
            Time.timeScale += Time.deltaTime/ timeIncreaseLevel;
        }
    }

    void SpawnBlock()
    {
        //-2.5 -> 3.33
        // 15
        blocks.Add(Instantiate(block, new Vector3(15, Random.Range(-3.3f, -2.5f), 0), Quaternion.identity));
    }
    void SpawnCloud()
    {
        Instantiate(cloud, new Vector3(15, Random.Range(0f, 2f), 0), Quaternion.identity);
    }
    public void GameOver()
    {
        isFinish = true;
        Time.timeScale = 0;
        statusGameTitle.gameObject.SetActive(true);
        startOrRetryButton.gameObject.SetActive(true);
        statusGameTitle.text = "GameOver";
        startOrRetryButton.GetComponentInChildren<TMP_Text>().text = "Retry";
    }
    public void IncreaseScore()
    {
        score++;
        scoreText.text = score + "";
    }
    void ContiuneGame() {
        foreach (var item in blocks)
        {
            Destroy(item);
        }

        blocks.Clear();
        statusGameTitle.gameObject.SetActive(false);
        startOrRetryButton.gameObject.SetActive(false);
        scoreText.text = "0";
        score = 0;
        
        Time.timeScale = 1;
        isFinish = false;
    }
}
