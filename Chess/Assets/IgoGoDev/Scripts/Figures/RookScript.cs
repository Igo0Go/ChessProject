using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RookScript : GameFigure
{
    public override List<GameFieldPoint> GetDrawPointsWithoutFigure(GameFigure setFigure)
    {
        List<GameFieldPoint> pointsForDraw = new List<GameFieldPoint>();
        Vector2Int newCell = currentPosition;

        bool iCanMoveInThisDirection = true;
        while (iCanMoveInThisDirection) //вперёд
        {
            newCell += new Vector2Int(0, 1);
            if (OpportunityToMove(newCell))
            {
                pointsForDraw.Add(gameField[newCell.y, newCell.x]);
                if (!IsEmptyCell(newCell))
                {
                    if (gameField[newCell.y, newCell.x].figureOnThisPoint != setFigure)
                    {
                        iCanMoveInThisDirection = false;
                    }
                }
            }
            else
            {
                iCanMoveInThisDirection = false;
            }
        }

        newCell = currentPosition;
        iCanMoveInThisDirection = true;
        while (iCanMoveInThisDirection) //назад
        {
            newCell += new Vector2Int(0, -1);
            if (OpportunityToMove(newCell))
            {
                pointsForDraw.Add(gameField[newCell.y, newCell.x]);
                if (!IsEmptyCell(newCell))
                {
                    if (gameField[newCell.y, newCell.x].figureOnThisPoint != setFigure)
                    {
                        iCanMoveInThisDirection = false;
                    }
                }
            }
            else
            {
                iCanMoveInThisDirection = false;
            }
        }

        newCell = currentPosition;
        iCanMoveInThisDirection = true;
        while (iCanMoveInThisDirection) //вправо
        {
            newCell += new Vector2Int(1, 0);
            if (OpportunityToMove(newCell))
            {
                pointsForDraw.Add(gameField[newCell.y, newCell.x]);
                if (!IsEmptyCell(newCell))
                {
                    if (gameField[newCell.y, newCell.x].figureOnThisPoint != setFigure)
                    {
                        iCanMoveInThisDirection = false;
                    }
                }
            }
            else
            {
                iCanMoveInThisDirection = false;
            }
        }

        newCell = currentPosition;
        iCanMoveInThisDirection = true;
        while (iCanMoveInThisDirection) //влево
        {
            newCell += new Vector2Int(-1, 0);
            if (OpportunityToMove(newCell))
            {
                pointsForDraw.Add(gameField[newCell.y, newCell.x]);
                if (!IsEmptyCell(newCell))
                {
                    if (gameField[newCell.y, newCell.x].figureOnThisPoint != setFigure)
                    {
                        iCanMoveInThisDirection = false;
                    }
                }
            }
            else
            {
                iCanMoveInThisDirection = false;
            }
        }
        return pointsForDraw;
    }

    public override List<GameFieldPoint> GetPointsForStep()
    {
        List<GameFieldPoint> pointsForDraw = new List<GameFieldPoint>();
        Vector2Int newCell = currentPosition;

        bool iCanMoveInThisDirection = true;
        while (iCanMoveInThisDirection) //вперёд
        {
            newCell += new Vector2Int(0, 1);
            if (OpportunityToMove(newCell))
            {
                pointsForDraw.Add(gameField[newCell.y, newCell.x]);
                if (!IsEmptyCell(newCell))
                {
                    iCanMoveInThisDirection = false;
                }
            }
            else
            {
                iCanMoveInThisDirection = false;
            }
        }

        newCell = currentPosition;
        iCanMoveInThisDirection = true;
        while (iCanMoveInThisDirection) //назад
        {
            newCell += new Vector2Int(0, -1);
            if (OpportunityToMove(newCell))
            {
                pointsForDraw.Add(gameField[newCell.y, newCell.x]);
                if (!IsEmptyCell(newCell))
                {
                    iCanMoveInThisDirection = false;
                }
            }
            else
            {
                iCanMoveInThisDirection = false;
            }
        }

        newCell = currentPosition;
        iCanMoveInThisDirection = true;
        while (iCanMoveInThisDirection) //вправо
        {
            newCell += new Vector2Int(1, 0);
            if (OpportunityToMove(newCell))
            {
                pointsForDraw.Add(gameField[newCell.y, newCell.x]);
                if (!IsEmptyCell(newCell))
                {
                    iCanMoveInThisDirection = false;
                }
            }
            else
            {
                iCanMoveInThisDirection = false;
            }
        }

        newCell = currentPosition;
        iCanMoveInThisDirection = true;
        while (iCanMoveInThisDirection) //влево
        {
            newCell += new Vector2Int(-1, 0);
            if (OpportunityToMove(newCell))
            {
                pointsForDraw.Add(gameField[newCell.y, newCell.x]);
                if (!IsEmptyCell(newCell))
                {
                    iCanMoveInThisDirection = false;
                }
            }
            else
            {
                iCanMoveInThisDirection = false;
            }
        }
        return pointsForDraw;
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
        List<GameFieldPoint> pointsForDraw = new List<GameFieldPoint>();
        Vector2Int newCell = new Vector2Int(point.j, point.i);

        bool iCanMoveInThisDirection = true;
        while (iCanMoveInThisDirection) //вперёд
        {
            newCell += new Vector2Int(0, 1);
            if (OpportunityToMove(newCell))
            {
                pointsForDraw.Add(gameField[newCell.y, newCell.x]);
                if (!IsEmptyCell(newCell))
                {
                    iCanMoveInThisDirection = false;
                }
            }
            else
            {
                iCanMoveInThisDirection = false;
            }
        }

        newCell = new Vector2Int(point.j, point.i);
        iCanMoveInThisDirection = true;
        while (iCanMoveInThisDirection) //назад
        {
            newCell += new Vector2Int(0, -1);
            if (OpportunityToMove(newCell))
            {
                pointsForDraw.Add(gameField[newCell.y, newCell.x]);
                if (!IsEmptyCell(newCell))
                {
                    iCanMoveInThisDirection = false;
                }
            }
            else
            {
                iCanMoveInThisDirection = false;
            }
        }

        newCell = new Vector2Int(point.j, point.i);
        iCanMoveInThisDirection = true;
        while (iCanMoveInThisDirection) //вправо
        {
            newCell += new Vector2Int(1, 0);
            if (OpportunityToMove(newCell))
            {
                pointsForDraw.Add(gameField[newCell.y, newCell.x]);
                if (!IsEmptyCell(newCell))
                {
                    iCanMoveInThisDirection = false;
                }
            }
            else
            {
                iCanMoveInThisDirection = false;
            }
        }

        newCell = new Vector2Int(point.j, point.i);
        iCanMoveInThisDirection = true;
        while (iCanMoveInThisDirection) //влево
        {
            newCell += new Vector2Int(-1, 0);
            if (OpportunityToMove(newCell))
            {
                pointsForDraw.Add(gameField[newCell.y, newCell.x]);
                if (!IsEmptyCell(newCell))
                {
                    iCanMoveInThisDirection = false;
                }
            }
            else
            {
                iCanMoveInThisDirection = false;
            }
        }
        return pointsForDraw;
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
    public override List<GameFieldPoint> GetPointsUnderAttack(GameFieldPoint point)
    {
        return GetPointsForStep(point);
    }
    public override List<GameFieldPoint> GetPointsUnderAttackFromPoint(GameFieldPoint point)
    {
        return GetPointsForStepWithFromPoint(point);
    }
}
