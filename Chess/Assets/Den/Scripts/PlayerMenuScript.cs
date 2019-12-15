using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerMenuScript : MonoBehaviour
{

    public GameObject mainMenuPanel;
    public GameObject settingPanel;
    public GameObject settingPanel2;

    public Button playButton1;
    public Button playButton2;
    public Button settingButton;
    public Button exitButton;
    public Button backButton;

    public Toggle drawSettingPanel;
    public Toggle drawEmptyCell;
    public Toggle drawProtectCell;
    public Toggle drawUnderAttackCell;
    public Toggle drawShields;
    public Toggle drawRelations;

    public GameObject drawSettingsPanelGO;

    private GameFieldSettings drawSettingsController;

    private AsyncOperation asyncOperationLoad;

    public void Awake()
    {
        playButton1.onClick.AddListener(Play_ButtonClick);
        playButton2.onClick.AddListener(Play_ButtonClick);
        settingButton.onClick.AddListener(Setting_ButtonClick);
        exitButton.onClick.AddListener(Exit_ButtonClick);
        backButton.onClick.AddListener(Back_ButtonClick);

        if (drawSettingsPanelGO != null) drawSettingPanel.onValueChanged.AddListener(CheckDrawSettingsPanel);
        CheckToggles();

        drawSettingPanel.onValueChanged.AddListener(DrowSettingPanel);
        drawEmptyCell.onValueChanged.AddListener(DrawEmptyCell);
        drawProtectCell.onValueChanged.AddListener(DrawProtectCell);
        drawUnderAttackCell.onValueChanged.AddListener(DrawUnderAttackCell);
        drawShields.onValueChanged.AddListener(DrawShields);
        drawRelations.onValueChanged.AddListener(DrawRelations);
    }

    // Start is called before the first frame update
    void Start()
    {
        Back_ButtonClick();

        SetTimeMode(false);

        asyncOperationLoad = SceneManager.LoadSceneAsync(0);
        asyncOperationLoad.allowSceneActivation = false;
        drawSettingsController = drawSettingsPanelGO.GetComponent<GameFieldSettings>();
    }

    private void CheckToggles()
    {
        drawSettingPanel.isOn = GameFieldSettingsPack.DrowSettingPanel;
        drawEmptyCell.isOn = GameFieldSettingsPack.DrawEmptyCell;
        drawProtectCell.isOn = GameFieldSettingsPack.DrawProtectCell;
        drawUnderAttackCell.isOn = GameFieldSettingsPack.DrawUnderAttackCell;
        drawShields.isOn = GameFieldSettingsPack.DrawShields;
        drawRelations.isOn = GameFieldSettingsPack.DrawRelations;
    }

    public void DrowSettingPanel(bool isOn) { GameFieldSettingsPack.DrowSettingPanel = isOn; drawSettingsController.UpdateButtons(); }
    public void DrawEmptyCell(bool isOn) { GameFieldSettingsPack.DrawEmptyCell = isOn; drawSettingsController.UpdateButtons(); }
    public void DrawProtectCell(bool isOn) { GameFieldSettingsPack.DrawProtectCell = isOn; drawSettingsController.UpdateButtons(); }
    public void DrawUnderAttackCell(bool isOn) { GameFieldSettingsPack.DrawUnderAttackCell = isOn; drawSettingsController.UpdateButtons(); }
    public void DrawShields(bool isOn) { GameFieldSettingsPack.DrawShields = isOn; drawSettingsController.UpdateButtons(); }
    public void DrawRelations(bool isOn) { GameFieldSettingsPack.DrawRelations = isOn; drawSettingsController.UpdateButtons(); }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) | Input.GetKeyDown(KeyCode.Space))
            SetTimeMode(!mainMenuPanel.activeSelf);
    }

    public void CheckDrawSettingsPanel(bool value) => drawSettingsPanelGO.SetActive(value);

    public void Play_ButtonClick()
    {
        SetTimeMode(false);
    }

    public void SetTimeMode(bool isPause)
    {
        mainMenuPanel.SetActive(isPause);

        GameFieldSettingsPack.IsMenu = isPause;

        if (isPause)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
    }

    public void Setting_ButtonClick()
    {
        mainMenuPanel.SetActive(false);
        settingPanel.SetActive(true);
        settingPanel2.SetActive(true);
        CheckToggles();
    }

    public void Back_ButtonClick()
    {
        mainMenuPanel.SetActive(true);
        settingPanel.SetActive(false);
        settingPanel2.SetActive(false);
    }

    public void Exit_ButtonClick()
    {
        Time.timeScale = 1;
        asyncOperationLoad.allowSceneActivation = true;
    }
}
