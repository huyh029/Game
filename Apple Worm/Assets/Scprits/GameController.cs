using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(GameLogic))]
[RequireComponent(typeof(GameLevel))]
public class GameController : MonoBehaviour
{
    private enum GameState
    {
        Title,
        Playing,
        Win,
        Lose,
        LevelSelect
    }

    [Header("Game refs")]
    [SerializeField] private GameLogic gameLogic;
    [SerializeField] private GameLevel gameLevel;

    [Header("Panels")]
    [SerializeField] private GameObject titlePanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject levelSelectPanel;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button quickRetryButton;
    [SerializeField] private Button quickSelectLevelButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button selectLevelButton;
    [SerializeField] private Button levelButtonPrefab;
    [SerializeField] private Transform levelButtonContainer;

    [Header("Texts")]
    [SerializeField] private Text titleText;
    [SerializeField] private Text winText;
    [SerializeField] private Text loseText;

    private int currentLevel = 0;
    private bool physicsWasMoving = false;
    private GameState currentState = GameState.Title;

    private void Awake()
    {
        gameLogic = GetComponent<GameLogic>();
        gameLevel = GetComponent<GameLevel>();

        if (gameLogic != null)
            gameLogic.LevelCompleted += OnWin;

        EnsureUiExists();
        BindUiButtons();
    }

    private void Start()
    {
        BuildLevelButtons();
        ShowTitle();
        LoadLevel(currentLevel);
        Time.timeScale = 2;
    }

    private void Update()
    {
        if (currentState != GameState.Playing)
            return;

        if (!gameLogic.AllPhysicsStable())
        {
            physicsWasMoving = true;
            return;
        }

        if (physicsWasMoving)
        {
            physicsWasMoving = false;
            gameLogic.UpdateMapFromObjects();
        }

        HandleInput();
    }

    private void EnsureUiExists()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            GameObject eventSystemGO = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            eventSystemGO.transform.SetParent(canvas.transform, false);
        }

        Transform uiRoot = canvas.transform.Find("GameUI");
        if (uiRoot == null)
        {
            uiRoot = new GameObject("GameUI", typeof(RectTransform)).transform;
            uiRoot.SetParent(canvas.transform, false);
            RectTransform rt = uiRoot.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        if (titlePanel == null)
            titlePanel = CreatePanel(uiRoot, "TitlePanel");

        if (winPanel == null)
            winPanel = CreatePanel(uiRoot, "WinPanel");

        if (losePanel == null)
            losePanel = CreatePanel(uiRoot, "LosePanel");

        if (levelSelectPanel == null)
            levelSelectPanel = CreatePanel(uiRoot, "LevelSelectPanel");

        if (playButton == null)
            playButton = CreateButton(titlePanel.transform, "PlayButton", "Play", new Vector2(0, -80));

        if (retryButton == null)
            retryButton = CreateButton(losePanel.transform, "RetryButton", "Retry", new Vector2(0, -80));

        if (quickRetryButton == null)
            quickRetryButton = CreateButton(uiRoot, "QuickRetryButton", "Retry", new Vector2(250, 200));

        if (quickRetryButton != null)
        {
            RectTransform quickRetryRT = quickRetryButton.GetComponent<RectTransform>();
            quickRetryRT.sizeDelta = new Vector2(120, 50);
        }

        if (quickSelectLevelButton == null)
            quickSelectLevelButton = CreateButton(uiRoot, "QuickSelectLevelButton", "Màn", new Vector2(130, 200));

        if (quickSelectLevelButton != null)
        {
            RectTransform quickSelectRT = quickSelectLevelButton.GetComponent<RectTransform>();
            quickSelectRT.sizeDelta = new Vector2(120, 50);
        }

        if (nextLevelButton == null)
            nextLevelButton = CreateButton(winPanel.transform, "NextLevelButton", "Next Level", new Vector2(0, -80));

        if (selectLevelButton == null)
            selectLevelButton = CreateButton(titlePanel.transform, "SelectLevelButton", "Select Level", new Vector2(0, -160));

        if (titleText == null)
            titleText = CreateText(titlePanel.transform, "TitleText", "APPLE WORM", 40, new Vector2(0, 90));

        if (winText == null)
            winText = CreateText(winPanel.transform, "WinText", "YOU WIN!", 36, new Vector2(0, 80));

        if (loseText == null)
            loseText = CreateText(losePanel.transform, "LoseText", "YOU LOSE!", 36, new Vector2(0, 80));

        if (levelButtonContainer == null)
            levelButtonContainer = new GameObject("LevelButtonContainer", typeof(RectTransform)).transform;

        levelButtonContainer.SetParent(levelSelectPanel.transform, false);
        RectTransform containerRT = levelButtonContainer.GetComponent<RectTransform>();
        containerRT.anchorMin = new Vector2(0.5f, 0.5f);
        containerRT.anchorMax = new Vector2(0.5f, 0.5f);
        containerRT.sizeDelta = new Vector2(420, 260);
        containerRT.anchoredPosition = new Vector2(0, -20);

        if (levelButtonPrefab == null)
        {
            levelButtonPrefab = CreateButton(levelButtonContainer, "LevelButtonPrefab", "Level", Vector2.zero);
            levelButtonPrefab.gameObject.SetActive(false);
        }

        titlePanel.transform.SetParent(uiRoot, false);
        winPanel.transform.SetParent(uiRoot, false);
        losePanel.transform.SetParent(uiRoot, false);
        levelSelectPanel.transform.SetParent(uiRoot, false);

        SetPanelLayout(titlePanel, 0.25f, 0.2f, 0.75f, 0.8f);
        SetPanelLayout(winPanel, 0.25f, 0.2f, 0.75f, 0.8f);
        SetPanelLayout(losePanel, 0.25f, 0.2f, 0.75f, 0.8f);
        SetPanelLayout(levelSelectPanel, 0.15f, 0.1f, 0.85f, 0.9f);
    }

    private GameObject CreatePanel(Transform parent, string name)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.6f);
        panel.SetActive(false);
        return panel;
    }

    private void SetPanelLayout(GameObject panel, float xMin, float yMin, float xMax, float yMax)
    {
        if (panel == null)
            return;

        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition)
    {
        GameObject buttonGO = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonGO.transform.SetParent(parent, false);

        RectTransform rt = buttonGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(220, 60);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPosition;

        Image image = buttonGO.GetComponent<Image>();
        image.color = new Color(0.2f, 0.6f, 1f, 1f);

        Button button = buttonGO.GetComponent<Button>();
        button.targetGraphic = image;

        Text text = CreateText(buttonGO.transform, "ButtonText", label, 22, Vector2.zero);
        text.alignment = TextAnchor.MiddleCenter;

        return button;
    }

    private Text CreateText(Transform parent, string name, string content, int fontSize, Vector2 anchoredPosition)
    {
        GameObject textGO = new GameObject(name, typeof(RectTransform), typeof(Text));
        textGO.transform.SetParent(parent, false);

        RectTransform rt = textGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(300, 80);
        rt.anchoredPosition = anchoredPosition;

        Text text = textGO.GetComponent<Text>();
        if (text == null)
        {
            text = textGO.AddComponent<Text>();
        }

        text.text = content;
        text.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.resizeTextForBestFit = false;
        return text;
    }

    private void BindUiButtons()
    {
        if (playButton != null)
            playButton.onClick.AddListener(StartGame);

        if (retryButton != null)
            retryButton.onClick.AddListener(ReloadLevel);

        if (quickRetryButton != null)
            quickRetryButton.onClick.AddListener(ReloadLevel);

        if (quickSelectLevelButton != null)
            quickSelectLevelButton.onClick.AddListener(ShowLevelSelect);

        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(NextLevel);

        if (selectLevelButton != null)
            selectLevelButton.onClick.AddListener(ShowLevelSelect);
    }

    private void HideAllPanels()
    {
        if (titlePanel != null) titlePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (quickRetryButton != null) quickRetryButton.gameObject.SetActive(false);
        if (quickSelectLevelButton != null) quickSelectLevelButton.gameObject.SetActive(false);
    }

    private void BuildLevelButtons()
    {
        if (gameLevel == null || levelSelectPanel == null)
            return;

        if (levelButtonContainer == null)
            levelButtonContainer = new GameObject("LevelButtonContainer", typeof(RectTransform)).transform;

        levelButtonContainer.SetParent(levelSelectPanel.transform, false);
        RectTransform containerRT = levelButtonContainer.GetComponent<RectTransform>();
        containerRT.anchorMin = new Vector2(0.5f, 0.5f);
        containerRT.anchorMax = new Vector2(0.5f, 0.5f);
        containerRT.sizeDelta = new Vector2(420, 200);
        containerRT.anchoredPosition = Vector2.zero;

        for (int i = levelButtonContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(levelButtonContainer.GetChild(i).gameObject);
        }

        int totalLevels = gameLevel.TotalLevels();
        for (int i = 0; i < totalLevels; i++)
        {
            GameObject levelButtonGO = new GameObject("LevelButton_" + (i + 1), typeof(RectTransform), typeof(Image), typeof(Button));
            levelButtonGO.transform.SetParent(levelButtonContainer, false);

            RectTransform rt = levelButtonGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(160, 60);
            rt.anchoredPosition = new Vector2((i % 2 == 0 ? -120 : 120), (i / 2 == 0 ? 40 : -40));

            Image image = levelButtonGO.GetComponent<Image>();
            image.color = new Color(0.3f, 0.75f, 1f, 1f);

            Button levelButton = levelButtonGO.GetComponent<Button>();
            levelButton.targetGraphic = image;

            int levelIndex = i;
            levelButton.onClick.AddListener(() => SelectLevel(levelIndex));

            Text buttonText = CreateText(levelButtonGO.transform, "LevelText", "Màn " + (i + 1), 18, Vector2.zero);
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.white;
            levelButtonGO.transform.SetAsLastSibling();
        }
    }

    public void StartGame()
    {
        currentState = GameState.Playing;
        HideAllPanels();
        LoadLevel(currentLevel);
        if (quickRetryButton != null)
            quickRetryButton.gameObject.SetActive(true);
        if (quickSelectLevelButton != null)
            quickSelectLevelButton.gameObject.SetActive(true);
    }

    public void ShowTitle()
    {
        currentState = GameState.Title;
        HideAllPanels();

        if (titlePanel != null)
            titlePanel.SetActive(true);

        if (titleText != null)
            titleText.text = "APPLE WORM";
    }

    public void ShowLevelSelect()
    {
        currentState = GameState.LevelSelect;
        HideAllPanels();

        if (levelSelectPanel != null)
        {
            levelSelectPanel.SetActive(true);
            levelSelectPanel.transform.SetAsLastSibling();
        }

        BuildLevelButtons();

        if (titleText != null)
        {
            titleText.transform.SetParent(levelSelectPanel != null ? levelSelectPanel.transform : transform, false);
            titleText.text = "CHỌN MÀN";
            RectTransform titleRT = titleText.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 1f);
            titleRT.anchorMax = new Vector2(0.5f, 1f);
            titleRT.sizeDelta = new Vector2(500, 80);
            titleRT.anchoredPosition = new Vector2(0, -60);
            titleText.transform.SetAsLastSibling();
        }

        if (levelButtonContainer != null)
        {
            levelButtonContainer.gameObject.SetActive(true);
            levelButtonContainer.SetParent(levelSelectPanel != null ? levelSelectPanel.transform : transform, false);
            RectTransform containerRT = levelButtonContainer.GetComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0.5f, 0.5f);
            containerRT.anchorMax = new Vector2(0.5f, 0.5f);
            containerRT.sizeDelta = new Vector2(420, 200);
            containerRT.anchoredPosition = new Vector2(0, 0);
            levelButtonContainer.SetAsLastSibling();
        }
    }

    public void OnWin()
    {
        currentState = GameState.Win;
        HideAllPanels();

        if (winPanel != null)
            winPanel.SetActive(true);

        if (nextLevelButton != null)
            nextLevelButton.gameObject.SetActive(true);

        if (winText != null)
            winText.text = "YOU WIN!";
    }

    public void OnLose()
    {
        currentState = GameState.Lose;
        HideAllPanels();

        if (losePanel != null)
            losePanel.SetActive(true);

        if (retryButton != null)
            retryButton.gameObject.SetActive(true);

        if (loseText != null)
            loseText.text = "YOU LOSE!";
    }

    public void OnReplay()
    {
        ReloadLevel();
    }

    public void OnNextLevel()
    {
        NextLevel();
    }

    public void ReloadLevel()
    {
        currentState = GameState.Playing;
        HideAllPanels();
        LoadLevel(currentLevel);
        if (quickRetryButton != null)
            quickRetryButton.gameObject.SetActive(true);
        if (quickSelectLevelButton != null)
            quickSelectLevelButton.gameObject.SetActive(true);
    }

    public void NextLevel()
    {
        if (gameLevel == null)
            return;

        currentLevel = (currentLevel + 1) % gameLevel.TotalLevels();
        currentState = GameState.Playing;
        HideAllPanels();
        LoadLevel(currentLevel);
        if (quickRetryButton != null)
            quickRetryButton.gameObject.SetActive(true);
        if (quickSelectLevelButton != null)
            quickSelectLevelButton.gameObject.SetActive(true);
    }

    public void SelectLevel(int levelIndex)
    {
        if (gameLevel == null)
            return;

        currentLevel = Mathf.Clamp(levelIndex, 0, gameLevel.TotalLevels() - 1);
        currentState = GameState.Playing;
        HideAllPanels();
        LoadLevel(currentLevel);
        if (quickRetryButton != null)
            quickRetryButton.gameObject.SetActive(true);
        if (quickSelectLevelButton != null)
            quickSelectLevelButton.gameObject.SetActive(true);
    }

    private void LoadLevel(int levelIndex)
    {
        if (gameLogic == null)
            return;

        if (gameLevel != null)
        {
            LevelData levelData = gameLevel.GetLevel(levelIndex);
            if (levelData != null)
            {
                gameLogic.ApplyLevel(levelData);
                Debug.Log("Load level: " + (levelIndex + 1) + "/" + gameLevel.TotalLevels());
            }
        }

        gameLogic.InitializePrefabs();
        gameLogic.MapToUI();
    }

    private void HandleInput()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            if (gameLogic.IsStraight())
                return;

            gameLogic.Move(new Vector2(-1, 0));
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            gameLogic.Move(new Vector2(1, 0));
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            gameLogic.Move(new Vector2(0, -1));
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            gameLogic.Move(new Vector2(0, 1));
        }
    }
}
