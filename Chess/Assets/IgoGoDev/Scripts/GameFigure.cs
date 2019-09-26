using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public enum FigureType
{
    pawn,
    rook,
    bishop,
    horse,
    queen,
    king
}

public class GameFigure : MonoBehaviour
{
    public FigureType type;
    public Vector2Int currentPosition;
    public bool whiteArmy;
    [Range(1,3)]public float moveSpeed = 1;

    private GameFieldPoint[,] gameField;
    private GameFieldPoint currentPoint;
    private List<GameFieldPoint> pointsForStep; 
    private bool firstStep;
    private int stepMultiplicator;
    private int moveToTarget;

    private bool NearWithTarget => Vector3.Distance(transform.position, currentPoint.transform.position) <= 0.1f;
    private Action drawCells;
    private event Action onClick;

    void Start()
    {
        switch(type)
        {
            case FigureType.pawn:
                drawCells = DrawCellsForPawn;
                break;
            case FigureType.horse:
                drawCells = DrawCellsForHorse;
                break;
            case FigureType.rook:
                drawCells = DrawCellsForRook;
                break;
            case FigureType.bishop:
                drawCells = DrawCellsForBishop;
                break;
            case FigureType.queen:
                drawCells = DrawCellsForQuin;
                break;
            case FigureType.king:
                drawCells = DrawCellsForKing;
                break;
        }
    }

    void Update()
    {
        SmoothMove();
    }

    private void OnMouseDown()
    {
        onClick?.Invoke();
        drawCells();
    }

    public void Initialize(GameFieldOrigin gameFieldOrigin)
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
        firstStep = true;
        stepMultiplicator = whiteArmy ? 1 : -1;
        pointsForStep = new List<GameFieldPoint>();
        moveToTarget = 0;
        CheckPoint();
        onClick += gameFieldOrigin.ClearAllAreas;
    }
    public void SetTargetPos(Vector2Int pos)
    {
        firstStep = false;
        currentPoint.emptyField = true;
        currentPosition = pos;
        moveToTarget = 1;
        currentPoint = gameField[pos.y, pos.x];
        foreach (var item in pointsForStep)
        {
            item.ClearPoint();
        }
    }
    private void SmoothMove()
    {
        if (moveToTarget != 0)
        {
            if (NearWithTarget)
            {
                transform.position = currentPoint.transform.position;
                moveToTarget = 0;
                CheckPoint();
            }
            else
            {
                transform.position += (currentPoint.transform.position - transform.position).normalized * moveSpeed * Time.deltaTime;
            }
        }
    }
    private bool OpportunityToMove(Vector2Int toPos)
    {
        if(toPos.x < 0 || toPos.x > gameField.GetLength(1)-1 || toPos.y < 0 || toPos.y > gameField.GetLength(0)-1)
        {
            return false;
        }
        if(!gameField[toPos.y, toPos.x].emptyField)
        {
            return false;
        }
        return true;
    }
    private void CheckPoint()
    {
        currentPoint = gameField[currentPosition.y, currentPosition.x];
        currentPoint.emptyField = false;
    }

    private void DrawCellsForPawn()
    {
        pointsForStep.Clear();
        Vector2Int newCell = currentPosition + new Vector2Int(0, stepMultiplicator);
        if(OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
        }
        newCell.y += stepMultiplicator;
        if(firstStep)
        {
            if (OpportunityToMove(newCell))
            {
                pointsForStep.Add(gameField[newCell.y, newCell.x]);
            }
        }

        for (int i = 0; i < pointsForStep.Count; i++)
        {
            pointsForStep[i].SetFigure(this);
        }
    }
    private void DrawCellsForHorse()
    {
        pointsForStep.Clear();
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
        newCell = currentPosition;

        for (int i = 0; i < pointsForStep.Count; i++)
        {
            pointsForStep[i].SetFigure(this);
        }
    }
    private void DrawCellsForRook()
    {
        pointsForStep.Clear();

        Vector2Int newCell = currentPosition;
        bool iCanMoveInThisDirection = true;
        while(iCanMoveInThisDirection) //вперёд
        {
            newCell += new Vector2Int(0, 1); 
            if (OpportunityToMove(newCell))
            {
                pointsForStep.Add(gameField[newCell.y, newCell.x]);
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
                pointsForStep.Add(gameField[newCell.y, newCell.x]);
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
                pointsForStep.Add(gameField[newCell.y, newCell.x]);
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
                pointsForStep.Add(gameField[newCell.y, newCell.x]);
            }
            else
            {
                iCanMoveInThisDirection = false;
            }
        }

        for (int i = 0; i < pointsForStep.Count; i++)
        {
            pointsForStep[i].SetFigure(this);
        }
    }
    private void DrawCellsForBishop()
    {
        pointsForStep.Clear();

        Vector2Int newCell = currentPosition;
        bool iCanMoveInThisDirection = true;
        while (iCanMoveInThisDirection) //вперёд вправо
        {
            newCell += new Vector2Int(1, 1);
            if (OpportunityToMove(newCell))
            {
                pointsForStep.Add(gameField[newCell.y, newCell.x]);
            }
            else
            {
                iCanMoveInThisDirection = false;
            }
        }

        newCell = currentPosition;
        iCanMoveInThisDirection = true;
        while (iCanMoveInThisDirection) //назад вправо
        {
            newCell += new Vector2Int(1, -1);
            if (OpportunityToMove(newCell))
            {
                pointsForStep.Add(gameField[newCell.y, newCell.x]);
            }
            else
            {
                iCanMoveInThisDirection = false;
            }
        }

        newCell = currentPosition;
        iCanMoveInThisDirection = true;
        while (iCanMoveInThisDirection) //назад влево
        {
            newCell += new Vector2Int(-1, -1);
            if (OpportunityToMove(newCell))
            {
                pointsForStep.Add(gameField[newCell.y, newCell.x]);
            }
            else
            {
                iCanMoveInThisDirection = false;
            }
        }

        newCell = currentPosition;
        iCanMoveInThisDirection = true;
        while (iCanMoveInThisDirection) //вперёд влево
        {
            newCell += new Vector2Int(-1, 1);
            if (OpportunityToMove(newCell))
            {
                pointsForStep.Add(gameField[newCell.y, newCell.x]);
            }
            else
            {
                iCanMoveInThisDirection = false;
            }
        }

        for (int i = 0; i < pointsForStep.Count; i++)
        {
            pointsForStep[i].SetFigure(this);
        }
    }
    private void DrawCellsForQuin()
    {
        pointsForStep.Clear();

        Vector2Int newCell = currentPosition;
        bool iCanMoveInThisDirection = true;
        while (iCanMoveInThisDirection) //вперёд вправо
        {
            newCell += new Vector2Int(1, 1);
            if (OpportunityToMove(newCell))
            {
                pointsForStep.Add(gameField[newCell.y, newCell.x]);
            }
            else
            {
                iCanMoveInThisDirection = false;
            }
        }

        newCell = currentPosition;
        iCanMoveInThisDirection = true;
        while (iCanMoveInThisDirection) //назад вправо
        {
            newCell += new Vector2Int(1, -1);
            if (OpportunityToMove(newCell))
            {
                pointsForStep.Add(gameField[newCell.y, newCell.x]);
            }
            else
            {
                iCanMoveInThisDirection = false;
            }
        }

        newCell = currentPosition;
        iCanMoveInThisDirection = true;
        while (iCanMoveInThisDirection) //назад влево
        {
            newCell += new Vector2Int(-1, -1);
            if (OpportunityToMove(newCell))
            {
                pointsForStep.Add(gameField[newCell.y, newCell.x]);
            }
            else
            {
                iCanMoveInThisDirection = false;
            }
        }

        newCell = currentPosition;
        iCanMoveInThisDirection = true;
        while (iCanMoveInThisDirection) //вперёд влево
        {
            newCell += new Vector2Int(-1, 1);
            if (OpportunityToMove(newCell))
            {
                pointsForStep.Add(gameField[newCell.y, newCell.x]);
            }
            else
            {
                iCanMoveInThisDirection = false;
            }
        }

        newCell = currentPosition;
        iCanMoveInThisDirection = true;
        while (iCanMoveInThisDirection) //вперёд
        {
            newCell += new Vector2Int(0, 1);
            if (OpportunityToMove(newCell))
            {
                pointsForStep.Add(gameField[newCell.y, newCell.x]);
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
                pointsForStep.Add(gameField[newCell.y, newCell.x]);
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
                pointsForStep.Add(gameField[newCell.y, newCell.x]);
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
                pointsForStep.Add(gameField[newCell.y, newCell.x]);
            }
            else
            {
                iCanMoveInThisDirection = false;
            }
        }

        for (int i = 0; i < pointsForStep.Count; i++)
        {
            pointsForStep[i].SetFigure(this);
        }
    }
    private void DrawCellsForKing()
    {
        pointsForStep.Clear();

        Vector2Int newCell = currentPosition;
        newCell += new Vector2Int(0, 1); //вперёд
        if (OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;
        newCell += new Vector2Int(1, 1); //вперёд вправо
        if (OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;
        newCell += new Vector2Int(1, 0); //вправо
        if (OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;
        newCell += new Vector2Int(1, -1);//вправо назад
        if (OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;
        newCell += new Vector2Int(0, -1);//назад
        if (OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;
        newCell += new Vector2Int(-1, -1);//влево назад
        if (OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;
        newCell += new Vector2Int(-1, 0);//влево
        if (OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;
        newCell += new Vector2Int(-1, 1);//вперёд влево
        if (OpportunityToMove(newCell))
        {
            pointsForStep.Add(gameField[newCell.y, newCell.x]);
        }

        for (int i = 0; i < pointsForStep.Count; i++)
        {
            pointsForStep[i].SetFigure(this);
        }
    }
}
