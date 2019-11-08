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

    public List<Animator> animForPanels; //0 - Шах, 1 - И мат

    [SerializeField]
    private float sideLenght = 1;
    [Space(20), SerializeField]private bool create = false;
    [SerializeField] private bool clear = false;
    public List<GameFigure> figures;

    private GameFigure[] kings = new GameFigure[2]; //0- белые, 1 - чёрные;
    private Army activeArmy;
    private bool pause;

    public event Action onClickToFigure;
    public event Action onCheckDefeat;

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
                if(!((i+j)%2 == 0))
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
            }
            else
            {
                item.iCanMove = true;
            }
            item.underAttack = false;
        }
        activeArmy = activeArmy == Army.white? Army.black : Army.white;
    }
    public void CheckDefeat()
    {
        animForPanels[0].SetBool("Open", false);
        onCheckDefeat?.Invoke();
        int index = 0;

        index = activeArmy == Army.white ? 0 : 1;

        if (kings[index].underAttack)
        {
            animForPanels[0].SetBool("Open", true);
        }
    }
    public void RemoveFigure(GameFigure figure)
    {
        if (figure.type == FigureType.king) FinalGame(figure.army);
        onClickToFigure -= figure.InvokeClearAttackLinks;
        figures.Remove(figure);
        Destroy(figure.gameObject);
    }
    public void ClearAllAttackLinks()
    {
        onClickToFigure?.Invoke();
    }
    public void CheckFieldLinksForFigure(GameFigure setFigure)  //чистка отрисовки на всех клетках и установка конфигурации относительно выбранной фигуры
    {
        foreach (var item in fieldMatrix)
        {
            foreach (var point in item.elements)
            {
                point.attackFigures.Clear();
                point.PointState = PointState.empty;
            }
        }
        List<GameFieldPoint> figureAttackPoints = new List<GameFieldPoint>();
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
        winText.text = army == Army.white ? "Чёрные выигрывают партию!" : "Белые выигрывают партию!";
    }

    void Start()
    {
        winPanel.SetActive(false);
        CheckMatrix();
        foreach (var item in figures)
        {
            item.Initialize(this);
            if(item.type == FigureType.king)
            {
                if(item.army == Army.white)
                {
                    kings[0] = item;
                }
                else
                {
                    kings[1] = item;
                }
            }
        }
        activeArmy = Army.black;
        CheckArmy();
    }
    void Update()
    {
        
    }

    private void OnDrawGizmosSelected()
    {
        if(create)
        {
            CreateMatrix();
            create = false;
        }
        if(clear)
        {
            ClearMatrix();
            clear = false;
        }
    }
}
