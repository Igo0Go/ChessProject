using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ChessAIScript : MonoBehaviour
{
    public GameFieldOrigin chessboard;
    public Army army = Army.Black;
    [Range(1, 4)]
    public int AttackMulti = 1;
    [Range(1, 4)]
    public int ProtectMulti = 1;
    public void Start()
    {
        if (GameFieldSettingsPack.AISetting > 0)
            AttackMulti = GameFieldSettingsPack.AISetting;
        if (GameFieldSettingsPack.AISetting < 0)
            ProtectMulti = GameFieldSettingsPack.AISetting;
    }

    public void GetStep()
    {
        var list = chessboard.figures.Where(c => c.army == army);

        List<(GameFigure figure, GameFieldPoint point, int weight)> fpw = new List<(GameFigure figure, GameFieldPoint point, int weight)>();

        foreach (var l in list)
        {
            var pfs = l.GetPointsForStepWithOtherFigures();
            var pfa = l.GetPointsUnderAttackWithOtherFigures();

            int currentWeight = CalculateWeight(l, l.currentPoint);

            foreach (var p in pfs)
            {
                int newWeight = CalculateWeight(l, p);

                if ((!p.emptyField && pfa.Contains(p)) || p.emptyField)
                    fpw.Add((l, p, newWeight - currentWeight));
            }

            foreach (var p in pfa)
            {
                if (!pfs.Contains(p))
                {
                    int newWeight = CalculateWeight(l, p);

                    if (!p.emptyField)
                        fpw.Add((l, p, newWeight - currentWeight));
                }
            }
        }

        var (figure, point, weight) = fpw.Where(k => k.weight == fpw.Max(c => c.weight)).First();

        figure.SetTargetPos(point.GetVector);

        if (!point.emptyField)
            chessboard.RemoveFigure(point.figureOnThisPoint);

        chessboard.ClearAllAreas();
        chessboard.ClearAllAttackLinks();
        //chessboard.ClearBoardDrawing();

        chessboard.CheckArmy();
    }

    private int CalculateWeight(GameFigure l, GameFieldPoint point)
    {
        int calcWeight = 0;
        var laf = point.attackFigures;
        bool Ua = false;
        int hp = 0;
        if (laf.Count() > 0)
            foreach (var c in laf)
                if (c.army == army)
                    hp++;
                else
                    Ua = true;

        if (hp > 0)
            calcWeight += ((int)l.type * hp) * ProtectMulti;

        if (Ua)
            calcWeight -= ((int)l.type * (int)l.type) * ProtectMulti;

        var gpuawopf = l.GetPointsUnderAttackFromPoint(point);
        int Ha = 0;
        int hs = 0;

        foreach (var g in gpuawopf)
        {
            if (!g.emptyField)
            {
                if (g.figureOnThisPoint.army != army && (int)g.figureOnThisPoint.type > Ha)
                    Ha = (int)g.figureOnThisPoint.type;
                if (g.figureOnThisPoint.army == army)
                    hs += (int)g.figureOnThisPoint.type * (int)g.figureOnThisPoint.type;
            }
        }

        if (Ha > 0)
            calcWeight += Ha * Ha * AttackMulti;

        if (hs > 0)
            calcWeight += hs * ProtectMulti;


        return calcWeight;
    }
}
