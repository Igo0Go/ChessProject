using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorseScript : GameFigure
{
    public override List<GameFieldPoint> GetDrawPointsWithoutFigure(GameFigure setFigure)
    {
        return GetPointsForStepWithOtherFigures();
    }

    public override List<GameFieldPoint> GetPointsForStep()
    {
        List<GameFieldPoint> pointsForStep = new List<GameFieldPoint>();
        Vector2Int newCell = currentPosition;

        newCell += new Vector2Int(-1, 2); //вперёд влево
        if (OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;

        newCell += new Vector2Int(1, 2);//вперёд вправо
        if (OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;

        newCell += new Vector2Int(2, 1);//вправо вперёд
        if (OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;

        newCell += new Vector2Int(2, -1);//вправо назад
        if (OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;

        newCell += new Vector2Int(1, -2);//назад вправо
        if (OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;

        newCell += new Vector2Int(-1, -2);//назад влево
        if (OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;

        newCell += new Vector2Int(-2, -1);//влево назад
        if (OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;

        newCell += new Vector2Int(-2, 1);//влево вперёд
        if (OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
        }
        return pointsForStep;
    }
    public override List<GameFieldPoint> GetPointsForStepWithOtherFigures()
    {
        List<GameFieldPoint> points = GetPointsForStep();

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
    public override List<GameFieldPoint> GetPointsUnderAttack()
    {
        return GetPointsForStep();
    }
    public override List<GameFieldPoint> GetPointsUnderAttackWithOtherFigures()
    {
        return GetPointsForStepWithOtherFigures();
    }

    public override List<GameFieldPoint> GetPointsForStep(GameFieldPoint point)
    {
        List<GameFieldPoint> pointsForStep = new List<GameFieldPoint>();
        Vector2Int newCell = new Vector2Int(point.j, point.i);

        newCell += new Vector2Int(-1, 2); //вперёд влево
        if (OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = new Vector2Int(point.j, point.i);

        newCell += new Vector2Int(1, 2);//вперёд вправо
        if (OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = new Vector2Int(point.j, point.i);

        newCell += new Vector2Int(2, 1);//вправо вперёд
        if (OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = new Vector2Int(point.j, point.i);

        newCell += new Vector2Int(2, -1);//вправо назад
        if (OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = new Vector2Int(point.j, point.i);

        newCell += new Vector2Int(1, -2);//назад вправо
        if (OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = new Vector2Int(point.j, point.i);

        newCell += new Vector2Int(-1, -2);//назад влево
        if (OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = new Vector2Int(point.j, point.i);

        newCell += new Vector2Int(-2, -1);//влево назад
        if (OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = new Vector2Int(point.j, point.i);

        newCell += new Vector2Int(-2, 1);//влево вперёд
        if (OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
        }
        return pointsForStep;
    }
    public override List<GameFieldPoint> GetPointsUnderAttack(GameFieldPoint point)
    {
        return GetPointsForStep(point);
    }
    public override List<GameFieldPoint> GetPointsUnderAttackFromPoint(GameFieldPoint point)
    {
        return GetPointsForStepWithFromPoint(point);
    }
    public override List<GameFieldPoint> GetPointsForStepWithFromPoint(GameFieldPoint point)
    {
        List<GameFieldPoint> points = GetPointsForStep(point);

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
}
