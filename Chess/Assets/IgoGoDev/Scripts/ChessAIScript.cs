using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ChessAIScript : MonoBehaviour
{
    public int AI_Rate; //количество шагов в глубину для каждого хода.
    public GameFieldOrigin chessboard;
    public Army army = Army.Black;
    public KingScript aiKing;
    [Range(1, 4)]
    public int AttackMulti = 1;
    [Range(1, 4)]
    public int ProtectMulti = 1;

    public void Initiolize()
    {
        AI_Rate = GameFieldSettingsPack.AIStepRate;
        army = GameFieldSettingsPack.AIArmy;

        aiKing = army == Army.White ? (KingScript)chessboard.kings[0] : (KingScript)chessboard.kings[1];

        if (GameFieldSettingsPack.AISetting > 0)
            AttackMulti = GameFieldSettingsPack.AISetting;
        if (GameFieldSettingsPack.AISetting < 0)
            ProtectMulti = GameFieldSettingsPack.AISetting;
    }

    public void GetStep()
    {
        int kingState = ProtectionKing();
       if (kingState == 1)
            return;
       else if(kingState == -1)
        {
            chessboard.RemoveFigure(aiKing);
            return;
        }

        var list = chessboard.figures.Where(c => c.army == army);

        List<(GameFigure figure, GameFieldPoint point, int weight)> fpw = new List<(GameFigure figure, GameFieldPoint point, int weight)>();

        foreach (var l in list)
        {
            chessboard.CheckFieldLinksForFigure(l);
            var pfs = l.GetPointsForStepWithOtherFigures();
            var pfa = l.GetPointsUnderAttackWithOtherFigures();

            if (pfs.Count == 0 && pfa.Count == 0) break;

            int currentWeight = CalculateWeight(l, l.currentPoint, AI_Rate);

            foreach (var p in pfs)
            {
                int newWeight = CalculateWeight(l, p, AI_Rate);

                if ((!p.emptyField && pfa.Contains(p)) || p.emptyField)
                    fpw.Add((l, p, newWeight - currentWeight));
            }

            foreach (var p in pfa)
            {
                if (!pfs.Contains(p))
                {
                    int newWeight = CalculateWeight(l, p, AI_Rate-1);

                    if (!p.emptyField)
                        fpw.Add((l, p, newWeight - currentWeight));
                }
            }
        }

        if(fpw.Count == 0)
        {
            Debug.Log("Мат!");
            chessboard.RemoveFigure(aiKing);
            return;
        }

        var (figure, point, weight) = fpw.Where(k => k.weight == fpw.Max(c => c.weight)).First();

        figure.SetTargetPos(point.GetVector);

        if (!point.emptyField)
            chessboard.RemoveFigure(point.figureOnThisPoint);

        chessboard.ClearAllAreas();
        chessboard.ClearAllAttackLinks();
        //chessboard.ClearBoardDrawing();

        //chessboard.CheckArmy();    //у нас дважды менялась сторона. Я сделал это оптимальней.
                                    //Можешь смело удалять отсюда весть закомменченый код.
    }
    private int CalculateWeight(GameFigure l, GameFieldPoint point, int rate)
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

        if(rate > 0)
        {
            int weightFromNextPoint = calcWeight;
            var pointsForStep = l.GetPointsForStepWithOtherFigures();
            var pointsForAttack = l.GetPointsUnderAttackWithOtherFigures();
            foreach (var p in pointsForStep)
            {
                int newWeight = CalculateWeight(l, p, AI_Rate-1);

                if(weightFromNextPoint > calcWeight + newWeight)
                {
                    weightFromNextPoint = calcWeight + newWeight;
                }
            }

            foreach (var p in pointsForAttack)
            {
                int newWeight = CalculateWeight(l, p, AI_Rate - 1);

                if (weightFromNextPoint > calcWeight + newWeight)
                {
                    weightFromNextPoint = calcWeight + newWeight;
                }
            }

            calcWeight = weightFromNextPoint; 
        }

        return calcWeight;
    }

    /// <summary>
    /// Проверка, находится ли король под ударом. Если да, то пытаемся уйти сначала так, чтоб ещё кого-то срубить, потом хоть просто ноги унести, иначе считаем,
    /// что мат (да, знаю, что это не совсем правильно, но пока достаточно и этого). 
    /// 0 - спасение короля не потребовалось, продолжаем обычный ход
    /// 1 - удачно увели короля
    /// -1 - спасти короля не удлось. Мат
    /// </summary>
    /// <returns></returns>
    private int ProtectionKing()
    {
        if (aiKing.underAttack)
        {
            chessboard.CheckFieldLinksForFigure(aiKing);
            var pointsForStep = aiKing.GetPointsUnderAttackWithOtherFigures();
            int currentWeight = -100;
            GameFieldPoint point = aiKing.currentPoint;
            int weightBufer;
            if (pointsForStep.Count > 0)
            {
                foreach (var item in pointsForStep)
                {
                    weightBufer = CalculateWeight(aiKing, item, AI_Rate);
                    if(weightBufer > currentWeight)
                    {
                        point = item;
                        currentWeight = weightBufer;
                    }
                }
                aiKing.SetTargetPos(point.GetVector);

                if (!point.emptyField)
                    chessboard.RemoveFigure(point.figureOnThisPoint);

                chessboard.ClearAllAreas();
                chessboard.ClearAllAttackLinks();
                //chessboard.CheckArmy();
                return 1;
            }

            pointsForStep = aiKing.GetPointsForStepWithOtherFigures();
            if (pointsForStep.Count > 0)
            {
                foreach (var item in pointsForStep)
                {
                    List<GameFigure> attackFigures = item.attackFigures.Where(c => c.army != aiKing.army).ToList();
                    if (attackFigures.Count == 0)
                    {
                        weightBufer = CalculateWeight(aiKing, item, AI_Rate);
                        if (weightBufer > currentWeight)
                        {
                            point = item;
                            currentWeight = weightBufer;
                        }
                    }
                }
                aiKing.SetTargetPos(point.GetVector);
                chessboard.ClearAllAreas();
                chessboard.ClearAllAttackLinks();
                //chessboard.CheckArmy();
                return 1;
            }

            return -1;
        }

        return 0;
    }
}
