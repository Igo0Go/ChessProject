using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void PositionHandler(Vector2Int pos);

[System.Serializable]
public class GameFieldPointArea
{
    public GameObject positionArea;
    public MeshRenderer meshRenderer;
    public Color moveColor;
    public Color underSwordColor;
    public Color blockColor;
}

public enum PointState
{
    empty,
    underAttack,
    block
}

public class GameFieldPoint : MonoBehaviour
{
    public Transform figurePoint;
    public MeshRenderer mRenderer;
    public GameFieldPointArea gameFieldPointArea;
    public List<GameFigure> attackFigures;

    [HideInInspector] public GameFigure figureOnThisPoint;
    [HideInInspector] public bool emptyField;
    [HideInInspector] public int i;
    [HideInInspector] public int j;

    private PositionHandler OnPositionClick;
    private bool pointForMove;

    public PointState PointState
    {
        get { return _state; }
        set
        {
            _state = value;
            switch (_state)
            {
                case PointState.empty:
                    gameFieldPointArea.meshRenderer.material.color = gameFieldPointArea.moveColor;
                    break;
                case PointState.block:
                    gameFieldPointArea.meshRenderer.material.color = gameFieldPointArea.blockColor;
                    break;
                case PointState.underAttack:
                    gameFieldPointArea.meshRenderer.material.color = gameFieldPointArea.underSwordColor;
                    break;
                default:
                    break;
            }
        }
    }
    private PointState _state;

    #region Внешний вид
    public void SetMaterial(Material mat)
    {
        mRenderer.material = mat;
    }
    public void DrawPointState(GameFigure setFigure)
    {
        if (attackFigures.Count == 0 || (attackFigures.Count == 1 && attackFigures[0] == setFigure))
        {
            PointState = PointState.empty;
            if (GameFieldSettingsPack.DrawEmptyCell)
            {
                gameFieldPointArea.positionArea.SetActive(true);
            }
        }
        else
        {
            bool protectOnly = true;
            bool drawKey = true;
            PointState = PointState.block;
            foreach (var item in attackFigures)
            {
                if (drawKey && item.army != setFigure.army)
                {
                    PointState = PointState.underAttack;
                    protectOnly = false;
                    if (GameFieldSettingsPack.DrawUnderAttackCell)
                    {
                        gameFieldPointArea.positionArea.SetActive(true);
                    }
                    else if (GameFieldSettingsPack.DrawEmptyCell)
                    {
                        gameFieldPointArea.meshRenderer.material.color = gameFieldPointArea.moveColor;
                        gameFieldPointArea.positionArea.SetActive(true);
                    }
                    drawKey = false;
                }
                item.DrawRelations(transform.position);
            }
            if (GameFieldSettingsPack.DrawProtectCell && protectOnly)
            {
                gameFieldPointArea.positionArea.SetActive(true);
            }
            else if (GameFieldSettingsPack.DrawEmptyCell && protectOnly)
            {
                gameFieldPointArea.meshRenderer.material.color = gameFieldPointArea.moveColor;
                gameFieldPointArea.positionArea.SetActive(true);
            }
        }
    }
    #endregion

    #region Основное
    public void SetI_J(int I, int J)
    {
        i = I;
        j = J;
    }
    public void SetFigure(GameFigure setFigure)
    {
        figureOnThisPoint = setFigure;
        emptyField = false;
    }
    public void SetPointAsWaypointForFigure(GameFigure setFigure)
    {
        //figureOnThisPoint = setFigure;
        OnPositionClick += setFigure.SetTargetPos;
        pointForMove = true;
    }
    public void InvokeOnPositionClick()
    {
        OnPositionClick?.Invoke(new Vector2Int(j, i));
    }
    public void ClearPointSettings()
    {
        OnPositionClick = null;
        pointForMove = true;
        gameFieldPointArea.positionArea.SetActive(false);
    }
    #endregion

    private void Start()
    {
    }
    private void OnMouseDown()
    {
        if(!GameFieldSettingsPack.IsMenu && pointForMove)
        {
            OnPositionClick?.Invoke(new Vector2Int(j, i));
        }
    }
}
