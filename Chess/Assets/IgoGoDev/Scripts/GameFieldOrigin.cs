using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UI;

[Serializable]
public class RowContainer
{
    public List<GameFieldPoint> elements;

    public RowContainer()
    {
        elements = new List<GameFieldPoint>();
    }
}

public class GameFieldOrigin : MonoBehaviour
{
    public int rows, columns;
    public GameObject instancePrefab;
    public List<RowContainer> fieldMatrix;
    public Material blackMat;
    public GameObject winPanel;
    public Text winText;
    public GameObject chooseFigurePanel;
    public List<GameObject> chooseFigureButtons;

    public ChessAIScript chessAI;

    public List<Animator> animForPanels; //0 - Шах, 1 - И мат
    public List<GameFigure> whiteSpawnFigure;
    public List<GameFigure> blackSpawnFigure;

    [SerializeField]
    private float sideLenght = 1;
    [Space(20), SerializeField] private bool create = false;
    [SerializeField] private bool clear = false;
    public List<GameFigure> figures;
    public List<Material> relationsMaterials;

    [HideInInspector] public GameFigure[] kings = new GameFigure[2]; //0- белые, 1 - чёрные;
    private Army activeArmy;
    private GameFieldPoint spawnBufer;
    private Dictionary<int, int> whiteArmyBufer;
    private Dictionary<int, int> blackArmyBufer;
    private bool activeGame;

    public event Action OnClickToFigure;
    public event Action OnCheckDefeat;

    public void CreateMatrix()
    {
        ClearMatrix();
        if (rows <= 0 || columns <= 0)
        {
            Debug.LogError("Неправильно введено количество строк или столбцов" + name);
            return;
        }
        fieldMatrix = new List<RowContainer>();
        Vector3 currentPointPos = transform.position - transform.right * sideLenght * rows / 2 - transform.forward * sideLenght * columns / 2;
        for (int i = 0; i < rows; i++)
        {
            fieldMatrix.Add(new RowContainer());
            for (int j = 0; j < columns; j++)
            {
                GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(instancePrefab);
                obj.transform.parent = transform;
                obj.transform.rotation = transform.rotation;
                obj.transform.position = currentPointPos;
                fieldMatrix[i].elements.Add(obj.GetComponent<GameFieldPoint>());
                if (!((i + j) % 2 == 0))
                {
                    fieldMatrix[i].elements[j].SetMaterial(blackMat);
                }
                fieldMatrix[i].elements[j].SetI_J(i, j);
                currentPointPos += transform.right * sideLenght;
            }
            currentPointPos -= transform.right * sideLenght * columns;
            currentPointPos += transform.forward * sideLenght;
        }
    }
    public void ClearMatrix()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
            i--;
        }
        fieldMatrix = null;
    }
    public void ClearAllAreas()
    {
        foreach (var item in fieldMatrix)
        {
            foreach (var cell in item.elements)
            {
                cell.ClearPointSettings();
            }
        }
    }
    public void CheckArmy()
    {
        foreach (var item in fieldMatrix)
        {
            foreach (var point in item.elements)
            {
                point.attackFigures.Clear();
                point.PointState = PointState.empty;
            }
        }

        foreach (var item in figures)
        {
            if (item.army == activeArmy)
            {
                item.iCanMove = false;
                item.SetRelationsMaterial(relationsMaterials[1]);
            }
            else if ((!GameFieldSettingsPack.PlayWithAI) || (GameFieldSettingsPack.PlayWithAI && activeArmy == chessAI.army))
            {
                item.iCanMove = true;
                item.SetRelationsMaterial(relationsMaterials[0]);
            }
            item.underAttack = false;
        }
        activeArmy = activeArmy == Army.White ? Army.Black : Army.White;
    }
    public void ComputerStep()
    {
        if (GameFieldSettingsPack.PlayWithAI && activeArmy == chessAI.army)
        {
            chessAI.GetStep();
            //activeArmy = activeArmy == Army.White ? Army.Black : Army.White;
        }
    }
    public void CheckDefeat()
    {
        animForPanels[0].SetBool("Open", false);
        OnCheckDefeat?.Invoke();
        int index;

        index = activeArmy == Army.White ? 0 : 1;

        if (kings[index].underAttack)
        {
            animForPanels[0].SetBool("Open", true);
        }
    }
    public void RemoveFigure(GameFigure figure)
    {
        if (figure.type == FigureType.King) FinalGame(figure.army);
        figure.RemoveEventLinks(this);
        figures.Remove(figure);
        Destroy(figure.gameObject);
        ClearBoardDrawing();
        ClearAllAttackLinks();
    }
    public void ClearAllAttackLinks()
    {
        OnClickToFigure?.Invoke();
    }
    public void CheckFieldLinksForFigure(GameFigure setFigure)  //чистка отрисовки на всех клетках и установка конфигурации относительно выбранной фигуры
    {
        ClearBoardDrawing();
        List<GameFieldPoint> figureAttackPoints;
        foreach (var item in figures)
        {
            figureAttackPoints = item.GetDrawPointsWithoutFigure(setFigure);
            foreach (var point in figureAttackPoints)
            {
                point.attackFigures.Add(item);
            }
            item.ClearMove();
        }
    }
    public void SpawnFigure(GameFieldPoint point)
    {
        spawnBufer = point;
        GameFieldSettingsPack.IsMenu = true;
        chooseFigurePanel.SetActive(true);
        CheckButtons(point.figureOnThisPoint.army);
    }
    public void SpawnFigureToPoint(int figureIndex)
    {
        GameFigure figureBufer = spawnBufer.figureOnThisPoint;
        GameFigure spawnFigureBufer = Instantiate(figureBufer.army == Army.White ? whiteSpawnFigure[figureIndex] : blackSpawnFigure[figureIndex],
                    figureBufer.transform.position, figureBufer.transform.rotation);
        spawnFigureBufer.currentPosition = figureBufer.currentPosition;
        spawnFigureBufer.moveSpeed = figureBufer.moveSpeed;
        spawnFigureBufer.Initialize(this);
        spawnFigureBufer.SetRelationsMaterial(relationsMaterials[1]);

        if (spawnFigureBufer.army == Army.White) whiteArmyBufer[figureIndex]--;
        else blackArmyBufer[figureIndex]--;

        figures.Add(spawnFigureBufer);

        RemoveFigure(figureBufer);
        ClearBoardDrawing();
        chooseFigurePanel.SetActive(false);
        GameFieldSettingsPack.IsMenu = false;
        CheckArmy();
        CheckDefeat();
        ComputerStep();
    }
    public void ClearBoardDrawing()
    {
        foreach (var item in fieldMatrix)
        {
            foreach (var point in item.elements)
            {
                point.attackFigures.Clear();
                point.PointState = PointState.empty;
                point.ClearPointSettings();
            }
        }
        foreach (var item in figures)
        {
            item.ClearRelations();
            item.ClearFigureUnderAttackLink();
        }
    }
    public void FreezeFigures()
    {
        foreach (var item in figures)
        {
            item.iCanMove = false;
        }
    }

    private void CheckMatrix()
    {
        foreach (var item in fieldMatrix)
        {
            foreach (var cell in item.elements)
            {
                cell.emptyField = true;
                cell.attackFigures = new List<GameFigure>();
            }
        }
    }
    private void FinalGame(Army army)
    {
        foreach (var item in figures)
        {
            item.iCanMove = false;
        }

        winPanel.SetActive(true);
        winText.text = army == Army.White ? "Чёрные выигрывают партию!" : "Белые выигрывают партию!";
        activeGame = false;
    }
    private void CheckButtons(Army army)
    {
        for (int i = 0; i < chooseFigureButtons.Count; i++)
        {
            if (army == Army.White ? whiteArmyBufer[i] == 0 : blackArmyBufer[i] == 0)
                chooseFigureButtons[i].SetActive(false);
            else
                chooseFigureButtons[i].SetActive(true);
        }
    }
   
    void Start()
    {
        chooseFigurePanel.SetActive(false);
        winPanel.SetActive(false);
        whiteArmyBufer = new Dictionary<int, int>();
        blackArmyBufer = new Dictionary<int, int>();

        for (int i = 0; i < 4; i++)
        {
            whiteArmyBufer.Add(i, 2);
            blackArmyBufer.Add(i, 2);
        }

        CheckMatrix();
        foreach (var item in figures)
        {
            item.Initialize(this);
            if (item.type == FigureType.King)
            {
                if (item.army == Army.White)
                {
                    kings[0] = item;
                }
                else
                {
                    kings[1] = item;
                }
            }
        }
        activeArmy = Army.Black;
        chessAI.Initiolize();
        CheckArmy();
        ComputerStep();
    }

    private void OnDrawGizmosSelected()
    {
        if (create)
        {
            CreateMatrix();
            create = false;
        }
        if (clear)
        {
            ClearMatrix();
            clear = false;
        }
    }
}
