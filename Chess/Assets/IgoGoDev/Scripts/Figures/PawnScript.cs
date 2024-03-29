﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public delegate void SpawnFigure(GameFieldPoint point);

public class PawnScript : GameFigure
{
    public event SpawnFigure OnSpawnFigure;

    private int stepMultiplicator;
    private bool firstStep;

    private void Start()
    {
        stepMultiplicator = army == Army.White ? 1 : -1;
        firstStep = true;
    }

    public override void Initialize(GameFieldOrigin gameFieldOrigin)
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

        OnSpawnFigure += gameFieldOrigin.SpawnFigure;
    }
    public override void RemoveEventLinks(GameFieldOrigin gameFieldOrigin)
    {
        base.RemoveEventLinks(gameFieldOrigin);
        OnSpawnFigure -= gameFieldOrigin.SpawnFigure;
    }

    public override List<GameFieldPoint> GetDrawPointsWithoutFigure(GameFigure setFigure)
    {
        return GetPointsUnderAttack();
    }

    public override List<GameFieldPoint> GetPointsForStep()
    {
        List<GameFieldPoint> pointsForStep = new List<GameFieldPoint>();
        Vector2Int newCell = currentPosition;
        newCell += new Vector2Int(0, stepMultiplicator);
        if (OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
        }
        if(firstStep)
        {
            newCell += new Vector2Int(0, stepMultiplicator);
            if (OpportunityToMove(newCell))
            {
                pointsForStep.Add(gameField[newCell.y, newCell.x]);
            }
        }
        return pointsForStep;
    }
    public override List<GameFieldPoint> GetPointsUnderAttack()
    {
        List<GameFieldPoint> pointsUnderAttack = new List<GameFieldPoint>();
        Vector2Int newCell = currentPosition;
        newCell += new Vector2Int(1, stepMultiplicator);
        if (OpportunityToMove(newCell))
        {
            pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;
        newCell += new Vector2Int(-1, stepMultiplicator);
        if (OpportunityToMove(newCell))
        {
            pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
        }
        return pointsUnderAttack;
    }
    public override List<GameFieldPoint> GetPointsUnderAttackWithOtherFigures()
    {
        List<GameFieldPoint> points = GetPointsUnderAttack();

        for (int i = 0; i < points.Count; i++)
        {
            if(!points[i].emptyField && points[i].figureOnThisPoint.army == army)
            {
                points.Remove(points[i]);
                i--;
            }
        }
        return points;
    }
    public override List<GameFieldPoint> GetPointsForStepWithOtherFigures()
    {
        List<GameFieldPoint> points = GetPointsForStep();

        if (points[0].emptyField)
        {
            if (firstStep)
            {
                if (!points[1].emptyField) points.Remove(points[1]);
            }
            else if(points.Count> 1) points.Remove(points[1]);
        }
        else
        {
            points.Clear();
        }
        return points;
    }

    public override List<GameFieldPoint> GetPointsUnderAttack(GameFieldPoint point)
    {
        List<GameFieldPoint> pointsUnderAttack = new List<GameFieldPoint>();
        Vector2Int newCell = new Vector2Int(point.j, point.i);
        newCell += new Vector2Int(1, stepMultiplicator);
        if (OpportunityToMove(newCell))
        {
            pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = new Vector2Int(point.j, point.i);
        newCell += new Vector2Int(-1, stepMultiplicator);
        if (OpportunityToMove(newCell))
        {
            pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
        }
        return pointsUnderAttack;
    }
    public override List<GameFieldPoint> GetPointsForStep(GameFieldPoint point)
    {
        List<GameFieldPoint> pointsForStep = new List<GameFieldPoint>();
        Vector2Int newCell = new Vector2Int(point.j, point.i);
        newCell += new Vector2Int(0, stepMultiplicator);
        if (OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
        }
        newCell += new Vector2Int(0, stepMultiplicator);
        if (OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
        }
        return pointsForStep;
    }
    public override List<GameFieldPoint> GetPointsUnderAttackFromPoint(GameFieldPoint point)
    {
        List<GameFieldPoint> points = GetPointsUnderAttack(point);

        for (int i = 0; i < points.Count; i++)
        {
            if (!points[i].emptyField && points[i].figureOnThisPoint.army == army)
            {
                points.Remove(points[i]);
                i--;
            }
        }
        return points;
    }
    public override List<GameFieldPoint> GetPointsForStepWithFromPoint(GameFieldPoint point)
    {
        List<GameFieldPoint> points = GetPointsForStep(point);

        if (points[0].emptyField)
        {
            if (firstStep)
            {
                if (!points[1].emptyField) points.Remove(points[1]);
            }
            else if (points.Count > 1) points.Remove(points[1]);
        }
        else
        {
            points.Clear();
        }
        return points;
    }

    public override void SetTargetPos(Vector2Int pos)
    {
        firstStep = false;
        base.SetTargetPos(pos);
    }

    public override void StopMove()
    {
        currentPoint = gameField[currentPosition.y, currentPosition.x];
        currentPoint.SetFigure(this);
        if (currentPosition.y == (army == Army.White ? gameField.GetLength(0) - 1 : 0))
            OnSpawnFigure?.Invoke(currentPoint);
        else
        {
            InvokeOnFinalMove();
        }
    }
}
