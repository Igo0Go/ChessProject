using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFieldPoint : MonoBehaviour
{
    public Transform figurePoint;
    public MeshRenderer mRenderer;
    public bool emptyField;

    [HideInInspector]public int i;
    [HideInInspector]public int j;


    public void SetMaterial(Material mat)
    {
        mRenderer.material = mat;
    }
    public void SetI_J(int I, int J)
    {
        i = I;
        j = J;
    }
}
