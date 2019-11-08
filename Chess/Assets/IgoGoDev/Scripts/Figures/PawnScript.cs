using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnScript : GameFigure
{
    private int stepMultiplicator;
    private bool firstStep;

    private void Start()
    {
        stepMultiplicator = army == Army.white ? 1 : -1;
        firstStep = true;
    }

    public override List<GameFieldPoint> GetDrawPointsWithoutFigure(GameFigure setFigure)
    {
        return GetPointsUnderAttackWithOtherFigures();
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
        newCell += new Vector2Int(0, stepMultiplicator);
        if (OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
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
            else points.Remove(points[1]);
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
}
