using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class GameFieldSettingsPack
{
    public static bool IsMenu { get; set; }
    public static bool PlayWithAI { get; set; } = false;
    public static int AISetting { get; set; }
    public static int AIStepRate { get; set; }
    public static Army AIArmy { get; set; }
    public static bool DrowSettingPanel { get; set; }
    public static bool DrawEmptyCell { get; set; }
    public static bool DrawProtectCell { get; set; }
    public static bool DrawUnderAttackCell { get; set; }
    public static bool DrawShields { get; set; }
    public static bool DrawRelations { get; set; }
}

public class GameFieldSettings : MonoBehaviour
{
    public List<Text> buttonsTexts;
    public GameFieldOrigin gameField;

    private void Start()
    {
        CheckButtons(0);
        CheckButtons(1);
        CheckButtons(2);
        CheckButtons(3);
        CheckButtons(4);
    }

    public void ChangeSettingsItem(int index)
    {
        switch (index)
        {
            case 0:
                GameFieldSettingsPack.DrawEmptyCell = !GameFieldSettingsPack.DrawEmptyCell;
                break;
            case 1:
                GameFieldSettingsPack.DrawProtectCell = !GameFieldSettingsPack.DrawProtectCell;
                break;
            case 2:
                GameFieldSettingsPack.DrawUnderAttackCell = !GameFieldSettingsPack.DrawUnderAttackCell;
                break;
            case 3:
                GameFieldSettingsPack.DrawShields = !GameFieldSettingsPack.DrawShields;
                break;
            case 4:
                GameFieldSettingsPack.DrawRelations = !GameFieldSettingsPack.DrawRelations;
                break;
        }
        CheckButtons(index);
    }
    private void CheckButtons(int index)
    {
        switch (index)
        {
            case 0:
                if (GameFieldSettingsPack.DrawEmptyCell)
                {
                    buttonsTexts[index].text = "Не помечать свободные клетки";
                }
                else
                {
                    buttonsTexts[index].text = "Помечать свободные клетки";
                }
                break;
            case 1:
                if (GameFieldSettingsPack.DrawProtectCell)
                {
                    buttonsTexts[index].text = "Не помечать клетки под защитой";
                }
                else
                {
                    buttonsTexts[index].text = "Помечать клетки под защитой";
                }
                break;
            case 2:
                if (GameFieldSettingsPack.DrawUnderAttackCell)
                {
                    buttonsTexts[index].text = "Не помечать клетки под ударом";
                }
                else
                {
                    buttonsTexts[index].text = "Помечать клетки под ударом";
                }
                break;
            case 3:
                if (GameFieldSettingsPack.DrawShields)
                {
                    buttonsTexts[index].text = "Не показывать щиты прикрытия";
                }
                else
                {
                    buttonsTexts[index].text = "Показывать щиты прикрытия";
                }
                break;
            case 4:
                if (GameFieldSettingsPack.DrawRelations)
                {
                    buttonsTexts[index].text = "Не указывать атакующие фигуры";
                }
                else
                {
                    buttonsTexts[index].text = "Указывать атакующие фигуры";
                }
                break;
        }
        CheckFigures();
    }

    private void CheckFigures()
    {
        foreach (var item in gameField.figures)
        {
            if(item.selectedFigure)
            {
                item.OnSelectFigure();
            }
        }
    }
}
