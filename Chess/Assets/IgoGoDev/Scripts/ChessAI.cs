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

        List<(GameFigure figure, GameFieldPoint point, int weight)> fpw = new List<(GameFigure figure, GameFieldPoint point, int weight)>();

        foreach (var l in list)
        {
            var pfs = l.GetPointsForStepWithOtherFigures().Union(l.GetPointsUnderAttackWithOtherFigures());

            int currentWeight = CalculateWeight(l, l.currentPoint);

            foreach (var p in pfs)
            {
                int newWeight = CalculateWeight(l, p);

                fpw.Add((l, p, newWeight - currentWeight));
            }
        }

        var selectStep = fpw.Where(c => c.weight == fpw.Max(c => c.weight)).First();

        chessboard.ClearAllAreas();
        chessboard.ClearAllAttackLinks();
        chessboard.ClearBoardDrawing();

        selectStep.figure.SetTargetPos(selectStep.point.GetVector);
    }

    private int CalculateWeight(GameFigure l, GameFieldPoint point)
    {
        int calcWeight = 0;
        var laf = point.attackFigures;
        bool ha = false;
        int hp = 0;
        if (laf.Count() > 0)
            foreach (var c in laf)
                if (c.army == army)
                    hp++;
                else
                    ha = true;

        if (hp > 0)
            calcWeight += ((int)l.type * hp);

        if (ha)
            calcWeight -= ((int)l.type * (int)l.type);

        var gpuawopf = l.GetPointsUnderAttackFromPoint(point);
        int aa = 0;
        int hs = 0;

        foreach (var g in gpuawopf)
        {
            if (!g.emptyField)
            {
                if (g.figureOnThisPoint.army != army && (int)g.figureOnThisPoint.type > aa)
                    aa = (int)g.figureOnThisPoint.type;
                if (g.figureOnThisPoint.army == army)
                    hs += (int)g.figureOnThisPoint.type * (int)g.figureOnThisPoint.type;
            }
        }

        if (aa > 0)
            calcWeight += aa * aa;

        if (hs > 0)
            calcWeight += hs;


        return calcWeight;
    }
}
