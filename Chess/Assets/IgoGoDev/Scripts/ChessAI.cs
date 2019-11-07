using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ChessAI : MonoBehaviour
{
    public GameFieldOrigin chessboard;
    public Army army;

    public void Start()
    {
        

    }

    public void GetStep()
    {
        var list = chessboard.figures.Where(c => c.army == army);

        foreach(var l in list)
        {
            l.getDrawPointsWithFigure(l);
        }
    }
}
