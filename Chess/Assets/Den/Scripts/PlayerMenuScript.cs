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

    public Toggle drowSettingPanel;
    public Toggle drawEmptyCell;
    public Toggle drawProtectCell;
    public Toggle drawUnderAttackCell;
    public Toggle drawShields;
    public Toggle drawRelations;

    private AsyncOperation asyncOperationLoad;

    public void Awake()
    {

        playButton1.onClick.AddListener(Play_ButtonClick);
        playButton2.onClick.AddListener(Play_ButtonClick);
        settingButton.onClick.AddListener(Setting_ButtonClick);
        exitButton.onClick.AddListener(Exit_ButtonClick);
        backButton.onClick.AddListener(Back_ButtonClick);

        drowSettingPanel.onValueChanged.AddListener(DrowSettingPanel);
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
    }


    public void DrowSettingPanel(bool isOn) { GameFieldSettingsPack.DrowSettingPanel = isOn; }
    public void DrawEmptyCell(bool isOn) { GameFieldSettingsPack.DrawEmptyCell = isOn; }
    public void DrawProtectCell(bool isOn) { GameFieldSettingsPack.DrawProtectCell = isOn; }
    public void DrawUnderAttackCell(bool isOn) { GameFieldSettingsPack.DrawUnderAttackCell = isOn; }
    public void DrawShields(bool isOn) { GameFieldSettingsPack.DrawShields = isOn; }
    public void DrawRelations(bool isOn) { GameFieldSettingsPack.DrawRelations = isOn; }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) | Input.GetKeyDown(KeyCode.Space))
            SetTimeMode(!mainMenuPanel.activeSelf);
    }

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
    }

    public void Back_ButtonClick()
    {
        mainMenuPanel.SetActive(true);
        settingPanel.SetActive(false);
        settingPanel2.SetActive(false);
    }

    public void Exit_ButtonClick()
    {
        asyncOperationLoad.allowSceneActivation = true;
    }
}
