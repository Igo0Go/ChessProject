using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void PositionHandler(Vector2Int pos);

[System.Serializable]
public class GameFieldPointArea
{
    public GameObject positionArea;
    public MeshRenderer meshRenderer;
    public Color moveColor;
    public Color underSwordColor;
    public Color blockColor;
}


public class GameFieldPoint : MonoBehaviour
{
    public Transform figurePoint;
    public MeshRenderer mRenderer;
    public bool emptyField;
    public GameFieldPointArea gameFieldPointArea;

    [HideInInspector]public int i;
    [HideInInspector]public int j;

    private PositionHandler OnPositionClick;
    private bool pointForMove;


    public void SetMaterial(Material mat)
    {
        mRenderer.material = mat;
    }
    public void SetI_J(int I, int J)
    {
        i = I;
        j = J;
    }

    public void SetFigure(GameFigure figure)
    {
        OnPositionClick += figure.SetTargetPos;
        pointForMove = true;
        gameFieldPointArea.positionArea.SetActive(true);
        gameFieldPointArea.meshRenderer.material.color = gameFieldPointArea.moveColor;
    }
    public void ClearPoint()
    {
        OnPositionClick = null;
        pointForMove = true;
        gameFieldPointArea.positionArea.SetActive(false);
    }

    private void Start()
    {
    }
    private void OnMouseDown()
    {
        if(pointForMove)
        {
            OnPositionClick?.Invoke(new Vector2Int(j, i));
        }
    }
}
