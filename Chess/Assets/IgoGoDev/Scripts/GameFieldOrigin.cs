using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
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

    [SerializeField]
    private float sideLenght = 1;
    [Space(20), SerializeField]private bool create = false;
    [SerializeField] private bool clear = false;
    public List<GameFigure> figures;

    private Army activeArmy;

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
                cell.ClearPoint();
            }
        }
    }
    public void CheckArmy()
    {
        foreach (var item in figures)
        {
            if(item.army == activeArmy)
            {
                item.iCanMove = false;
            }
            else
            {
                item.iCanMove = true;
            }
        }
        if(activeArmy == Army.white)
        {
            activeArmy = Army.black;
        }
        else
        {
            activeArmy = Army.white;
        }
    }
    public void RemoveFigure(GameFigure figure)
    {
        figures.Remove(figure);
        Destroy(figure.gameObject);
    }

    private void CheckMatrix()
    {
        foreach (var item in fieldMatrix)
        {
            foreach (var cell in item.elements)
            {
                cell.emptyField = true;
            }
        }
    }

    void Start()
    {
        CheckMatrix();
        foreach (var item in figures)
        {
            item.Initialize(this);
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
