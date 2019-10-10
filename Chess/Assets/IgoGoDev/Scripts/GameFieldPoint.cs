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
    public bool emptyField;
    public GameFieldPointArea gameFieldPointArea;
    public List<GameFigure> attackFigures;

    [HideInInspector]public int i;
    [HideInInspector]public int j;

    private PositionHandler OnPositionClick;
    private GameFigure figure;
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

    public void SetMaterial(Material mat)
    {
        mRenderer.material = mat;
    }
    public void SetI_J(int I, int J)
    {
        i = I;
        j = J;
    }
    public void InvokeOnPositionClick()
    {
        OnPositionClick?.Invoke(new Vector2Int(j, i));
    }
    public GameFigure GetFigure()
    {
        return figure;
    }
    public void SetAttackFigureToThisPoint(GameFigure setFigure)
    {
        figure = setFigure;
        OnPositionClick += setFigure.SetTargetPos;
        pointForMove = true;
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
                    drawKey = false;
                }
                item.DrawMove(transform.position);
            }
            if (GameFieldSettingsPack.DrawProtectCell && protectOnly)
            {
                gameFieldPointArea.positionArea.SetActive(true);
            }
        }
    }
    public void SetFigure(GameFigure setFigure)
    {
        figure = setFigure;
        emptyField = false;
    }
    public void ClearPoint()
    {
        OnPositionClick = null;
        pointForMove = true;
        gameFieldPointArea.positionArea.SetActive(false);
    }

    private void Start()
    {
    }
    private void OnMouseDown()
    {
        if(pointForMove)
        {
            OnPositionClick?.Invoke(new Vector2Int(j, i));
        }
    }
}
