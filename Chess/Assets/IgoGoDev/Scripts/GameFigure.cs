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

public enum Army
{
    white,
    black
}

public delegate List<GameFieldPoint> GetPointsUnderAttackHandler();
public delegate List<GameFieldPoint> GetDrawPointsWithFigureHandler(GameFigure setFigure);
public delegate void FigureHandler(GameFigure figure);

[RequireComponent(typeof(LineRenderer))]
public class GameFigure : MonoBehaviour
{
    public FigureType type;
    public Vector2Int currentPosition;
    public Army army = Army.white;
    [Range(1,3)]public float moveSpeed = 1;
    public GameObject enemyLink;
    [HideInInspector] public bool iCanMove;
    public GameObject shields;
    [HideInInspector] public bool selectedFigure;

    public GetPointsUnderAttackHandler getUnderAttackPoints;
    public GetDrawPointsWithFigureHandler getDrawPointsWithFigure;

    private GameFieldPoint[,] gameField;
    private GameFieldPoint currentPoint;
    private List<GameFieldPoint> pointsForStep;
    private GameFigure enemyFigure;
    private bool firstStep;
    private int stepMultiplicator;
    private int moveToTarget;

    private bool NearWithTarget => Vector3.Distance(transform.position, currentPoint.transform.position) <= 0.1f;
    private Action setCells;
    private event Action onClick;
    private event Action onFigureMove;
    public event Action onChoosenTargetEnemy;
    private event FigureHandler onDead;
    private event FigureHandler onClickToFigureWithDraw;
    private LineRenderer lineRenderer;

    void Start()
    {
        
    }

    void Update()
    {
        SmoothMove();
    }

    private void OnMouseDown()
    {
        OnSelectFigure();
    }

    public void OnSelectFigure()
    {
        if (iCanMove)
        {
            onClick?.Invoke();
            onClickToFigureWithDraw?.Invoke(this);
            selectedFigure = true;
            setCells();
        }
        if (enemyLink.activeSelf)
        {
            currentPoint.SetAttackFigureToThisPoint(enemyFigure);
            currentPoint.InvokeOnPositionClick();
            currentPoint.ClearPoint();
            enemyFigure.InvokeClearAttackLinks();
            onDead?.Invoke(this);
            onFigureMove = null;
            onClick = null;
            onDead = null;
        }
    }

    public void InvokeClearAttackLinks()
    {
        onChoosenTargetEnemy?.Invoke();
        onChoosenTargetEnemy = null;
        if(lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }
        else
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 0;
        }
        selectedFigure = false;
        lineRenderer.positionCount = 0;
    }
    public void Initialize(GameFieldOrigin gameFieldOrigin)
    {
        switch (type)
        {
            case FigureType.pawn:
                getUnderAttackPoints = CheckAttackPointsForPawn;
                getDrawPointsWithFigure = GetDrawPointsForPawnWithFigure;
                break;
            case FigureType.horse:
                getUnderAttackPoints = CheckAttackPointsForHorse;
                getDrawPointsWithFigure = GetDrawPointsForHorseWithFigure;
                break;
            case FigureType.rook:
                getUnderAttackPoints = CheckAttackPointsForRook;
                getDrawPointsWithFigure = GetDrawPointsForRookWithFigure;
                break;
            case FigureType.bishop:
                getUnderAttackPoints = CheckAttackPointsForBishop;
                getDrawPointsWithFigure = GetDrawPointsForBishopWithFigure;
                break;
            case FigureType.queen:
                getUnderAttackPoints = CheckAttackPointsForQuin;
                getDrawPointsWithFigure = GetDrawPointsForQuinWithFigure;
                break;
            case FigureType.king:
                getUnderAttackPoints = CheckAttackPointsForKing;
                getDrawPointsWithFigure = GetDrawPointsForKingWithFigure;
                break;
        }
        if (type == FigureType.pawn)
        {
            setCells = SetCellsForPawn;
        }
        else if(type == FigureType.king)
        {
            setCells = SetCellsForKing;
        }
        else
        {
            setCells = SetCells;
        }
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
        firstStep = true;
        stepMultiplicator = army == Army.white ? 1 : -1;
        pointsForStep = new List<GameFieldPoint>();
        moveToTarget = 0;
        CheckPoint();
        onClick += gameFieldOrigin.ClearAllAreas;
        onClick += gameFieldOrigin.ClearAllAttackLinks;
        onFigureMove += gameFieldOrigin.CheckArmy;
        onDead += gameFieldOrigin.RemoveFigure;
        onClickToFigureWithDraw += gameFieldOrigin.CheckFieldLinksForFigure;
        gameFieldOrigin.onClickToFigure += InvokeClearAttackLinks;
        enemyLink.SetActive(false);
        shields.SetActive(false);
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
    }
    public void SetTargetPos(Vector2Int pos)
    {
        firstStep = false;
        currentPoint.emptyField = true;
        currentPosition = pos;
        moveToTarget = 1;
        onFigureMove?.Invoke();
        currentPoint = gameField[pos.y, pos.x];
        foreach (var item in pointsForStep)
        {
            item.ClearPoint();
        }
    }
    public void SetUnderAttackState(GameFigure figure)
    {
        enemyFigure = figure;
        enemyLink.SetActive(true);
        if(GetSupportFigures().Count > 0 && GameFieldSettingsPack.DrawShields)
        {
            shields.SetActive(true);
        }
        figure.onChoosenTargetEnemy += ClearFigureUnderAttackLink;
    }
    public void DrawMove(Vector3 point)
    {
        if(!selectedFigure && GameFieldSettingsPack.DrawRelations)
        {
            lineRenderer.positionCount++;
            lineRenderer.SetPosition(lineRenderer.positionCount-1, transform.position + Vector3.up);
            lineRenderer.positionCount++;
            lineRenderer.SetPosition(lineRenderer.positionCount-1, point + Vector3.up);
        }
        else
        {
            lineRenderer.positionCount = 0;
        }
    }
    public void ClearMove() => lineRenderer.positionCount = 0;

    public List<GameFieldPoint> GetDrawPointsForPawnWithFigure(GameFigure setFigure)
    {
        List<GameFieldPoint> pointsForDraw = new List<GameFieldPoint>();
        Vector2Int newCell = currentPosition;
        newCell += new Vector2Int(1, stepMultiplicator);
        if (OpportunityToMove(newCell))
        {
            pointsForDraw.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;
        newCell += new Vector2Int(-1, stepMultiplicator);
        if (OpportunityToMove(newCell))
        {
            pointsForDraw.Add(gameField[newCell.y, newCell.x]);
        }
        return pointsForDraw;
    }
    public List<GameFieldPoint> GetDrawPointsForHorseWithFigure(GameFigure setFigure)
    {
        List<GameFieldPoint> pointsForDraw = new List<GameFieldPoint>();
        Vector2Int newCell = currentPosition;

        newCell += new Vector2Int(-1, 2); //вперёд влево
        if (OpportunityToMove(newCell))
        {
            pointsForDraw.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;

        newCell += new Vector2Int(1, 2);//вперёд вправо
        if (OpportunityToMove(newCell))
        {
            pointsForDraw.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;

        newCell += new Vector2Int(2, 1);//вправо вперёд
        if (OpportunityToMove(newCell))
        {
            pointsForDraw.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;

        newCell += new Vector2Int(2, -1);//вправо назад
        if (OpportunityToMove(newCell))
        {
            pointsForDraw.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;

        newCell += new Vector2Int(1, -2);//назад вправо
        if (OpportunityToMove(newCell))
        {
            pointsForDraw.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;

        newCell += new Vector2Int(-1, -2);//назад влево
        if (OpportunityToMove(newCell))
        {
            pointsForDraw.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;

        newCell += new Vector2Int(-2, -1);//влево назад
        if (OpportunityToMove(newCell))
        {
            pointsForDraw.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;

        newCell += new Vector2Int(-2, 1);//влево вперёд
        if (OpportunityToMove(newCell))
        {
            pointsForDraw.Add(gameField[newCell.y, newCell.x]);
        }
        return pointsForDraw;
    }
    public List<GameFieldPoint> GetDrawPointsForRookWithFigure(GameFigure setFigure)
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
                    if(gameField[newCell.y, newCell.x].GetFigure() != setFigure)
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
                    if (gameField[newCell.y, newCell.x].GetFigure() != setFigure)
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
                    if (gameField[newCell.y, newCell.x].GetFigure() != setFigure)
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
                    if (gameField[newCell.y, newCell.x].GetFigure() != setFigure)
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
    public List<GameFieldPoint> GetDrawPointsForBishopWithFigure(GameFigure setFigure)
    {
        List<GameFieldPoint> pointsForDraw = new List<GameFieldPoint>();
        Vector2Int newCell = currentPosition;

        bool iCanMoveInThisDirection = true;
        while (iCanMoveInThisDirection) //вперёд вправо
        {
            newCell += new Vector2Int(1, 1);
            if (OpportunityToMove(newCell))
            {
                pointsForDraw.Add(gameField[newCell.y, newCell.x]);
                if (!IsEmptyCell(newCell))
                {
                    if (gameField[newCell.y, newCell.x].GetFigure() != setFigure)
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
        while (iCanMoveInThisDirection) //назад вправо
        {
            newCell += new Vector2Int(1, -1);
            if (OpportunityToMove(newCell))
            {
                pointsForDraw.Add(gameField[newCell.y, newCell.x]);
                if (!IsEmptyCell(newCell))
                {
                    if (gameField[newCell.y, newCell.x].GetFigure() != setFigure)
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
        while (iCanMoveInThisDirection) //назад влево
        {
            newCell += new Vector2Int(-1, -1);
            if (OpportunityToMove(newCell))
            {
                pointsForDraw.Add(gameField[newCell.y, newCell.x]);
                if (!IsEmptyCell(newCell))
                {
                    if (gameField[newCell.y, newCell.x].GetFigure() != setFigure)
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
        while (iCanMoveInThisDirection) //вперёд влево
        {
            newCell += new Vector2Int(-1, 1);
            if (OpportunityToMove(newCell))
            {
                pointsForDraw.Add(gameField[newCell.y, newCell.x]);
                if (!IsEmptyCell(newCell))
                {
                    if (gameField[newCell.y, newCell.x].GetFigure() != setFigure)
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
    public List<GameFieldPoint> GetDrawPointsForQuinWithFigure(GameFigure setFigure)
    {
        List<GameFieldPoint> pointsForDraw = new List<GameFieldPoint>();
        Vector2Int newCell = currentPosition;

        bool iCanMoveInThisDirection = true;
        while (iCanMoveInThisDirection) //вперёд вправо
        {
            newCell += new Vector2Int(1, 1);
            if (OpportunityToMove(newCell))
            {
                pointsForDraw.Add(gameField[newCell.y, newCell.x]);
                if (!IsEmptyCell(newCell))
                {
                    if (gameField[newCell.y, newCell.x].GetFigure() != setFigure)
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
        while (iCanMoveInThisDirection) //назад вправо
        {
            newCell += new Vector2Int(1, -1);
            if (OpportunityToMove(newCell))
            {
                pointsForDraw.Add(gameField[newCell.y, newCell.x]);
                if (!IsEmptyCell(newCell))
                {
                    if (gameField[newCell.y, newCell.x].GetFigure() != setFigure)
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
        while (iCanMoveInThisDirection) //назад влево
        {
            newCell += new Vector2Int(-1, -1);
            if (OpportunityToMove(newCell))
            {
                pointsForDraw.Add(gameField[newCell.y, newCell.x]);
                if (!IsEmptyCell(newCell))
                {
                    if (gameField[newCell.y, newCell.x].GetFigure() != setFigure)
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
        while (iCanMoveInThisDirection) //вперёд влево
        {
            newCell += new Vector2Int(-1, 1);
            if (OpportunityToMove(newCell))
            {
                pointsForDraw.Add(gameField[newCell.y, newCell.x]);
                if (!IsEmptyCell(newCell))
                {
                    if (gameField[newCell.y, newCell.x].GetFigure() != setFigure)
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
        while (iCanMoveInThisDirection) //вперёд
        {
            newCell += new Vector2Int(0, 1);
            if (OpportunityToMove(newCell))
            {
                pointsForDraw.Add(gameField[newCell.y, newCell.x]);
                if (!IsEmptyCell(newCell))
                {
                    if (gameField[newCell.y, newCell.x].GetFigure() != setFigure)
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
                    if (gameField[newCell.y, newCell.x].GetFigure() != setFigure)
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
                    if (gameField[newCell.y, newCell.x].GetFigure() != setFigure)
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
                    if (gameField[newCell.y, newCell.x].GetFigure() != setFigure)
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
    public List<GameFieldPoint> GetDrawPointsForKingWithFigure(GameFigure setFigure)
    {
        List<GameFieldPoint> pointsUnderAttack = new List<GameFieldPoint>();
        Vector2Int newCell = currentPosition;
        newCell += new Vector2Int(0, 1); //вперёд
        if (OpportunityToMove(newCell))
        {
            pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;
        newCell += new Vector2Int(1, 1); //вперёд вправо
        if (OpportunityToMove(newCell))
        {
            pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;
        newCell += new Vector2Int(1, 0); //вправо
        if (OpportunityToMove(newCell))
        {
            pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;
        newCell += new Vector2Int(1, -1);//вправо назад
        if (OpportunityToMove(newCell))
        {
            pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;
        newCell += new Vector2Int(0, -1);//назад
        if (OpportunityToMove(newCell))
        {
            pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;
        newCell += new Vector2Int(-1, -1);//влево назад
        if (OpportunityToMove(newCell))
        {
            pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;
        newCell += new Vector2Int(-1, 0);//влево
        if (OpportunityToMove(newCell))
        {
            pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;
        newCell += new Vector2Int(-1, 1);//вперёд влево
        if (OpportunityToMove(newCell))
        {
            pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
        }
        return pointsUnderAttack;
    }

    public List<GameFieldPoint> CheckAttackPointsForPawn()
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
    public List<GameFieldPoint> CheckAttackPointsForHorse()
    {
        List<GameFieldPoint> pointsUnderAttack = new List<GameFieldPoint>();
        Vector2Int newCell = currentPosition;

        newCell += new Vector2Int(-1, 2); //вперёд влево
        if (OpportunityToMove(newCell))
        {
            pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;

        newCell += new Vector2Int(1, 2);//вперёд вправо
        if (OpportunityToMove(newCell))
        {
            pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;

        newCell += new Vector2Int(2, 1);//вправо вперёд
        if (OpportunityToMove(newCell))
        {
            pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;

        newCell += new Vector2Int(2, -1);//вправо назад
        if (OpportunityToMove(newCell))
        {
            pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;

        newCell += new Vector2Int(1, -2);//назад вправо
        if (OpportunityToMove(newCell))
        {
            pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;

        newCell += new Vector2Int(-1, -2);//назад влево
        if (OpportunityToMove(newCell))
        {
            pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;

        newCell += new Vector2Int(-2, -1);//влево назад
        if (OpportunityToMove(newCell))
        {
            pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;

        newCell += new Vector2Int(-2, 1);//влево вперёд
        if (OpportunityToMove(newCell))
        {
            pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
        }
        return pointsUnderAttack;
    }
    public List<GameFieldPoint> CheckAttackPointsForRook()
    {
        List<GameFieldPoint> pointsUnderAttack = new List<GameFieldPoint>();
        Vector2Int newCell = currentPosition;

        bool iCanMoveInThisDirection = true;
        while (iCanMoveInThisDirection) //вперёд
        {
            newCell += new Vector2Int(0, 1);
            if (OpportunityToMove(newCell))
            {
                pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
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
                pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
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
                pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
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
                pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
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
        return pointsUnderAttack;
    }
    public List<GameFieldPoint> CheckAttackPointsForBishop()
    {
        List<GameFieldPoint> pointsUnderAttack = new List<GameFieldPoint>();
        Vector2Int newCell = currentPosition;

        bool iCanMoveInThisDirection = true;
        while (iCanMoveInThisDirection) //вперёд вправо
        {
            newCell += new Vector2Int(1, 1);
            if (OpportunityToMove(newCell))
            {
                pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
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
        while (iCanMoveInThisDirection) //назад вправо
        {
            newCell += new Vector2Int(1, -1);
            if (OpportunityToMove(newCell))
            {
                pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
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
        while (iCanMoveInThisDirection) //назад влево
        {
            newCell += new Vector2Int(-1, -1);
            if (OpportunityToMove(newCell))
            {
                pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
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
        while (iCanMoveInThisDirection) //вперёд влево
        {
            newCell += new Vector2Int(-1, 1);
            if (OpportunityToMove(newCell))
            {
                pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
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
        return pointsUnderAttack;
    }
    public List<GameFieldPoint> CheckAttackPointsForQuin()
    {
        List<GameFieldPoint> pointsUnderAttack = new List<GameFieldPoint>();
        Vector2Int newCell = currentPosition;

        bool iCanMoveInThisDirection = true;
        while (iCanMoveInThisDirection) //вперёд вправо
        {
            newCell += new Vector2Int(1, 1);
            if (OpportunityToMove(newCell))
            {
                pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
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
        while (iCanMoveInThisDirection) //назад вправо
        {
            newCell += new Vector2Int(1, -1);
            if (OpportunityToMove(newCell))
            {
                pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
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
        while (iCanMoveInThisDirection) //назад влево
        {
            newCell += new Vector2Int(-1, -1);
            if (OpportunityToMove(newCell))
            {
                pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
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
        while (iCanMoveInThisDirection) //вперёд влево
        {
            newCell += new Vector2Int(-1, 1);
            if (OpportunityToMove(newCell))
            {
                pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
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
        while (iCanMoveInThisDirection) //вперёд
        {
            newCell += new Vector2Int(0, 1);
            if (OpportunityToMove(newCell))
            {
                pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
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
                pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
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
                pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
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
                pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
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
        return pointsUnderAttack;
    }
    public List<GameFieldPoint> CheckAttackPointsForKing()
    {
        List<GameFieldPoint> pointsUnderAttack = new List<GameFieldPoint>();
        Vector2Int newCell = currentPosition;
        newCell += new Vector2Int(0, 1); //вперёд
        if (OpportunityToMove(newCell))
        {
            pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;
        newCell += new Vector2Int(1, 1); //вперёд вправо
        if (OpportunityToMove(newCell))
        {
            pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;
        newCell += new Vector2Int(1, 0); //вправо
        if (OpportunityToMove(newCell))
        {
            pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;
        newCell += new Vector2Int(1, -1);//вправо назад
        if (OpportunityToMove(newCell))
        {
            pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;
        newCell += new Vector2Int(0, -1);//назад
        if (OpportunityToMove(newCell))
        {
            pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;
        newCell += new Vector2Int(-1, -1);//влево назад
        if (OpportunityToMove(newCell))
        {
            pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;
        newCell += new Vector2Int(-1, 0);//влево
        if (OpportunityToMove(newCell))
        {
            pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
        }
        newCell = currentPosition;
        newCell += new Vector2Int(-1, 1);//вперёд влево
        if (OpportunityToMove(newCell))
        {
            pointsUnderAttack.Add(gameField[newCell.y, newCell.x]);
        }
        return pointsUnderAttack;
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
        return true;
    }
    private bool IsEmptyCell(Vector2Int toPos)
    {
        if (!gameField[toPos.y, toPos.x].emptyField)
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
    private void ClearFigureUnderAttackLink()
    {
        enemyLink.SetActive(false);
        shields.SetActive(false);
        enemyFigure = null;
    }
    private List<GameFigure> GetSupportFigures()
    {
        List<GameFigure> result = new List<GameFigure>();
        foreach (var item in currentPoint.attackFigures)
        {
            if(item != this && item.army == army)
            {
                result.Add(item);
            }
        }
        return result;
    }

    private void SetCellsForPawn()
    {
        pointsForStep.Clear();
        Vector2Int newCell = currentPosition + new Vector2Int(0, stepMultiplicator);
        if(OpportunityToMove(newCell))
        {
            if (IsEmptyCell(newCell))
            {
                pointsForStep.Add(gameField[newCell.y, newCell.x]);
                newCell.y += stepMultiplicator;
                if (firstStep)
                {
                    if (OpportunityToMove(newCell) && IsEmptyCell(newCell))
                    {
                        pointsForStep.Add(gameField[newCell.y, newCell.x]);
                    }
                }
            }
        }
        for (int i = 0; i < pointsForStep.Count; i++)
        {
            pointsForStep[i].SetAttackFigureToThisPoint(this);
            pointsForStep[i].DrawPointState(this);
        }
        FightPawn();
    }
    private void SetCells()
    {
        pointsForStep.Clear();
        List<GameFieldPoint> points = getUnderAttackPoints();

        foreach (var item in points)
        {
            if(item.emptyField)
            {
                pointsForStep.Add(item);
            }
            else
            {
                GameFigure enemy = item.GetFigure();
                if (enemy.army != army)
                {
                    enemy.SetUnderAttackState(this);
                }
            }
        }

        for (int i = 0; i < pointsForStep.Count; i++)
        {
            pointsForStep[i].SetAttackFigureToThisPoint(this);
        }
        for (int i = 0; i < pointsForStep.Count; i++)
        {
            pointsForStep[i].DrawPointState(this);
        }
    }
    private void SetCellsForKing()
    {
        pointsForStep.Clear();
        List<GameFieldPoint> points = getUnderAttackPoints();

        foreach (var item in points)
        {
            if (item.emptyField)
            {
                bool danger = false;
                foreach (var attackFigure in item.attackFigures)
                {
                    if(attackFigure != this && attackFigure.army != army)
                    {
                        danger = true;
                        break;
                    }
                }
                if(!danger)
                {
                    pointsForStep.Add(item);
                }
            }
            else
            {
                bool danger = false;
                foreach (var attackFigure in item.attackFigures)
                {
                    if (attackFigure != this && attackFigure.army != army)
                    {
                        danger = true;
                        break;
                    }
                }
                if (!danger)
                {
                    GameFigure enemy = item.GetFigure();
                    if (enemy.army != army)
                    {
                        enemy.SetUnderAttackState(this);
                    }
                }
            }
        }

        for (int i = 0; i < pointsForStep.Count; i++)
        {
            pointsForStep[i].SetAttackFigureToThisPoint(this);
        }
        for (int i = 0; i < pointsForStep.Count; i++)
        {
            pointsForStep[i].DrawPointState(this);
        }
    }
    private void FightPawn()
    {
        List<GameFieldPoint> pointsUnderAttack = CheckAttackPointsForPawn();

        foreach (var item in pointsUnderAttack)
        {
            if(!item.emptyField)
            {
                GameFigure enemy = item.GetFigure();
                if (enemy.army != army)
                {
                    enemy.SetUnderAttackState(this);
                }
            }
        }
    }
}
