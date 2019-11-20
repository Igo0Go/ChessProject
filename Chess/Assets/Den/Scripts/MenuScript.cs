using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{

    public GameObject mainMenuPanel;
    public GameObject AISettingPanel;
    public GameObject settingPanel;
    public GameObject settingPanel2;

    public Button playPlayerButton;
    public Button playAIButton;
    public Button playWithAIButton;
    public Button settingButton;
    public Button exitButton;
    public Button backButton;

    public Slider AISettingSlider;
    public Slider AIStepSlider;
    public Slider ArmySlider;

    public Toggle drowSettingPanel;
    public Toggle drawEmptyCell;
    public Toggle drawProtectCell;
    public Toggle drawUnderAttackCell;
    public Toggle drawShields;
    public Toggle drawRelations;

    private AsyncOperation asyncOperationLoad;

    public void Awake()
    {
        playPlayerButton.onClick.AddListener(() => Play_ButtonClick(false));
        playAIButton.onClick.AddListener(() => Play_ButtonClick(true));
        playWithAIButton.onClick.AddListener(PlayAI_ButtonClick);
        settingButton.onClick.AddListener(Setting_ButtonClick);
        exitButton.onClick.AddListener(Exit_ButtonClick);
        backButton.onClick.AddListener(Back_ButtonClick);

        AISettingSlider.value = GameFieldSettingsPack.AISetting;
        AIStepSlider.value = GameFieldSettingsPack.AIStepRate;
        AIStepSlider.value = 0;

        drowSettingPanel.isOn = GameFieldSettingsPack.DrowSettingPanel;
        drawEmptyCell.isOn = GameFieldSettingsPack.DrawEmptyCell;
        drawProtectCell.isOn = GameFieldSettingsPack.DrawProtectCell;
        drawUnderAttackCell.isOn = GameFieldSettingsPack.DrawUnderAttackCell;
        drawShields.isOn = GameFieldSettingsPack.DrawShields;
        drawRelations.isOn = GameFieldSettingsPack.DrawRelations;

        AISettingSlider.onValueChanged.AddListener(AISettingChanged);
        AIStepSlider.onValueChanged.AddListener(AIStepRateChanged);
        ArmySlider.onValueChanged.AddListener(AIStepRateChanged);

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

        asyncOperationLoad = SceneManager.LoadSceneAsync(1);
        asyncOperationLoad.allowSceneActivation = false;
    }


    public void AISettingChanged(float value) { GameFieldSettingsPack.AISetting = (int)value; }
    public void AIStepRateChanged(float value) { GameFieldSettingsPack.AIStepRate = (int)value; }
    public void AIArmyChanged(float value) { GameFieldSettingsPack.AIArmy = ArmySlider.value == 0? Army.Black : Army.White; }
    public void DrowSettingPanel(bool isOn) { GameFieldSettingsPack.DrowSettingPanel = isOn; }
    public void DrawEmptyCell(bool isOn) { GameFieldSettingsPack.DrawEmptyCell = isOn; }
    public void DrawProtectCell(bool isOn) { GameFieldSettingsPack.DrawProtectCell = isOn; }
    public void DrawUnderAttackCell(bool isOn) { GameFieldSettingsPack.DrawUnderAttackCell = isOn; }
    public void DrawShields(bool isOn) { GameFieldSettingsPack.DrawShields = isOn; }
    public void DrawRelations(bool isOn) { GameFieldSettingsPack.DrawRelations = isOn; }


    public void PlayAI_ButtonClick()
    {
        AISettingPanel.SetActive(true);
    }

    public void Play_ButtonClick(bool withAI)
    {
        GameFieldSettingsPack.PlayWithAI = withAI;
        asyncOperationLoad.allowSceneActivation = true;
    }

    public void Setting_ButtonClick()
    {
        mainMenuPanel.SetActive(false);
        AISettingPanel.SetActive(false);
        settingPanel.SetActive(true);
        settingPanel2.SetActive(true);
    }

    public void Back_ButtonClick()
    {
        mainMenuPanel.SetActive(true);
        AISettingPanel.SetActive(false);
        settingPanel.SetActive(false);
        settingPanel2.SetActive(false);
    }

    public void Exit_ButtonClick()
    {
        Application.Quit();
    }
}
