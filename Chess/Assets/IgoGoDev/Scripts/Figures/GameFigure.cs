﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public enum FigureType : int
{
    Pawn = 1,
    Rook = 4,
    Bishop = 3,
    Horse = 2,
    Queen = 5,
    King = 6,
}
public enum Army
{
    White,
    Black
}

public delegate List<GameFieldPoint> GetPointsUnderAttackHandler();
public delegate List<GameFieldPoint> GetDrawPointsWithFigureHandler(GameFigure setFigure);
public delegate void FigureHandler(GameFigure figure);

[RequireComponent(typeof(LineRenderer))]
public abstract class GameFigure : MonoBehaviour
{
    [Header("Основные настройки")]
    public FigureType type;
    public Army army = Army.White;
    public Vector2Int currentPosition;
    [Range(1, 3)] public float moveSpeed = 1;

    [Space(10)]
    [Header("Ссылки")]
    public GameObject enemyLink;
    public GameObject shields;

    [HideInInspector] public bool underAttack;
    [HideInInspector] public bool selectedFigure;
    public bool iCanMove;
    [HideInInspector] public GameFieldPoint currentPoint;


    protected GameFieldPoint[,] gameField;
    protected List<GameFieldPoint> pointsForStep;
    protected LineRenderer lineRenderer;
    protected GameFigure currentEnemy;

    protected int moveToTarget;


    public event Action OnClick;
    public event Action OnChoosenTargetEnemy;

    protected event FigureHandler OnClickToFigureWithDraw;
    protected event FigureHandler OnDead;
    protected event Action OnFinalMove;
    protected event Action OnFigureMove;

    private bool NearWithTarget => Vector3.Distance(transform.position, currentPoint.transform.position) <= 0.1f;



    void Update()
    {
        SmoothMove();
    }
    private void OnMouseDown()
    {
        if (!GameFieldSettingsPack.IsMenu) OnSelectFigure();
    }

    public virtual void Initialize(GameFieldOrigin gameFieldOrigin)
    {
        gameField = new GameFieldPoint[gameFieldOrigin.rows, gameFieldOrigin.columns];
        for (int i = 0; i < gameField.GetLength(0); i++)
        {
            for (int j = 0; j < gameField.GetLength(1); j++)
            {
                gameField[i, j] = gameFieldOrigin.fieldMatrix[i].elements[j];
            }
        }
        currentPoint = gameField[currentPosition.y, currentPosition.x];
        currentPoint.SetFigure(this);
        pointsForStep = new List<GameFieldPoint>();
        moveToTarget = 0;

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        enemyLink.SetActive(false);
        shields.SetActive(false);

        OnClick += gameFieldOrigin.ClearAllAreas;
        OnClick += gameFieldOrigin.ClearAllAttackLinks;
        OnClickToFigureWithDraw += gameFieldOrigin.CheckFieldLinksForFigure;
        OnFigureMove += gameFieldOrigin.ClearBoardDrawing;
        OnFigureMove += gameFieldOrigin.FreezeFigures;
        OnFinalMove += gameFieldOrigin.CheckArmy;
        OnFinalMove += gameFieldOrigin.CheckDefeat;
        OnFinalMove += gameFieldOrigin.ComputerStep;
        OnDead += gameFieldOrigin.RemoveFigure;

        gameFieldOrigin.OnClickToFigure += InvokeClearAttackLinks;
        gameFieldOrigin.OnCheckDefeat += ChekFiguresUnderMyAttack;
    }
    public virtual void RemoveEventLinks(GameFieldOrigin gameFieldOrigin)
    {
        OnClick -= gameFieldOrigin.ClearAllAreas;
        OnClick -= gameFieldOrigin.ClearAllAttackLinks;
        OnClickToFigureWithDraw -= gameFieldOrigin.CheckFieldLinksForFigure;
        OnFigureMove -= gameFieldOrigin.ClearBoardDrawing;
        OnFinalMove -= gameFieldOrigin.CheckArmy;
        OnFinalMove -= gameFieldOrigin.CheckDefeat;
        OnDead -= gameFieldOrigin.RemoveFigure;
        OnChoosenTargetEnemy = null;

        gameFieldOrigin.OnClickToFigure -= InvokeClearAttackLinks;
        gameFieldOrigin.OnCheckDefeat -= ChekFiguresUnderMyAttack;
    }
    public void OnSelectFigure()
    {
        if (iCanMove)
        {
            OnClick?.Invoke(); //Убираем старую отрисовку
            OnClickToFigureWithDraw?.Invoke(this); //собираем инфу для отрисовки новой
            selectedFigure = true;
            SetCells(); //рисуем новую
        }
        if (enemyLink.activeSelf)
        {
            currentPoint.SetPointAsWaypointForFigure(currentEnemy);
            currentPoint.InvokeOnPositionClick();
            currentPoint.ClearPointSettings();
            //currentEnemy.InvokeClearAttackLinks();
            OnDead?.Invoke(this);
            OnFigureMove = null;
            OnClick = null;
            OnDead = null;
        }
    }

    public abstract List<GameFieldPoint> GetDrawPointsWithoutFigure(GameFigure setFigure); //клетки для трисовки без учёта указанной фигуры
                                                                   //(к примеру, чтоб король не мог пойти на клетки, где его всё равно достанет ферзь)
    public abstract List<GameFieldPoint> GetPointsUnderAttack(); //все клетки под ударом
    public abstract List<GameFieldPoint> GetPointsForStep();//все клетки для перемещения
    public abstract List<GameFieldPoint> GetPointsUnderAttackWithOtherFigures(); //все клетки под ударом у чётом других фигур
    public abstract List<GameFieldPoint> GetPointsForStepWithOtherFigures();//все клетки для перемещения с учётом других фигур
    public abstract List<GameFieldPoint> GetPointsUnderAttack(GameFieldPoint point); //все клетки под ударом
    public abstract List<GameFieldPoint> GetPointsForStep(GameFieldPoint point);//все клетки для перемещения
    public abstract List<GameFieldPoint> GetPointsUnderAttackFromPoint(GameFieldPoint point); //все клетки под ударом у чётом других фигур
    public abstract List<GameFieldPoint> GetPointsForStepWithFromPoint(GameFieldPoint point);//все клетки для перемещения с учётом других фигур

    public virtual void SetTargetPos(Vector2Int pos)
    {
        currentPoint.emptyField = true;
        currentPosition = pos;
        moveToTarget = 1;
        OnFigureMove?.Invoke();
        currentPoint = gameField[pos.y, pos.x];
        selectedFigure = false;
        foreach (var item in pointsForStep)
        {
            item.ClearPointSettings();
        }
    }

    public void SetCells()//назначить статус клеткам, на которые можем пойти
    {
        List<GameFieldPoint> points = GetPointsForStepWithOtherFigures();

        foreach (var item in points)
        {
            item.SetPointAsWaypointForFigure(this);
            item.DrawPointState(this);
        }

        points = GetPointsUnderAttackWithOtherFigures();

        foreach (var item in points)
        {
            if(!item.emptyField && item.figureOnThisPoint.army != army)
                item.figureOnThisPoint.SetUnderSwordState(this);
        }
    }


    public List<GameFigure> GetSupportFigures() //все фигуры, которые нас прикрывают
    {
        List<GameFigure> result = new List<GameFigure>();
        foreach (var item in currentPoint.attackFigures)
        {
            if (item != this && item.army == army)
            {
                result.Add(item);
            }
        }
        return result;
    }
    public void ClearMove()
    {
        if (lineRenderer != null) lineRenderer.positionCount = 0;
    }
    public void InvokeClearAttackLinks()
    {
        OnChoosenTargetEnemy?.Invoke();
        OnChoosenTargetEnemy = null;
        ClearRelations();
    }
    public void ClearRelations()
    {
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }
        else
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 0;
        }
        selectedFigure = false;
    }
    public void SetUnderSwordState(GameFigure figure) //помечает фигуру как возможную для сруба (Метка "Под ударом")
    {
        currentEnemy = figure;
        enemyLink.SetActive(true);
        if (GetSupportFigures().Count > 0 && GameFieldSettingsPack.DrawShields)
        {
            shields.SetActive(true);
        }
        figure.OnChoosenTargetEnemy += ClearFigureUnderAttackLink;
    }
    public void DrawRelations(Vector3 point) //отрисовать связи
    {
        if(lineRenderer != null)
        {
            if (!selectedFigure && GameFieldSettingsPack.DrawRelations)
            {
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, transform.position + Vector3.up);
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, point + Vector3.up);
            }
            else
            {
                lineRenderer.positionCount = 0;
            }
        }
    }
    public void ClearFigureUnderAttackLink() //Убрать метку "Под ударом"
    {
        enemyLink.SetActive(false);
        shields.SetActive(false);
        currentEnemy = null;
    }
    public void SetRelationsMaterial(Material mat)
    {
        if (lineRenderer != null) lineRenderer.material = mat;
    }
    public void ChekFiguresUnderMyAttack()
    {
        List<GameFieldPoint> points = GetPointsUnderAttack();

        foreach (var item in points)
        {
            if (!item.emptyField)
            {
                GameFigure enemy = item.figureOnThisPoint;
                if (enemy.army != army)
                {
                    enemy.underAttack = true;
                }
            }
            else if (!item.attackFigures.Contains(this)) item.attackFigures.Add(this);
        }
    }
    
    protected bool OpportunityToMove(Vector2Int toPos)
    {
        if (toPos.x < 0 || toPos.x > gameField.GetLength(1) - 1 || toPos.y < 0 || toPos.y > gameField.GetLength(0) - 1)
        {
            return false;
        }
        return true;
    }
    protected bool IsEmptyCell(Vector2Int toPos)
    {
        if (!gameField[toPos.y, toPos.x].emptyField)
        {
            return false;
        }
        return true;
    }
    private void SmoothMove()
    {
        if (moveToTarget != 0)
        {
            if (NearWithTarget)
            {
                transform.position = currentPoint.transform.position;
                moveToTarget = 0;
                StopMove();
            }
            else
            {
                transform.position += (currentPoint.transform.position - transform.position).normalized * moveSpeed * Time.deltaTime;
            }
        }
    }
    public virtual void StopMove()
    {
        currentPoint = gameField[currentPosition.y, currentPosition.x];
        currentPoint.SetFigure(this);
        InvokeOnFinalMove();
    }

    public void InvokeOnFinalMove()
    {
        OnFinalMove?.Invoke();
    }
}
