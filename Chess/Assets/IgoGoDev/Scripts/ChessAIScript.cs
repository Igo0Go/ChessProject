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

    public int RecGetStep(int currentRate = 0, IEnumerable<TupleForStep> ps = null)
    {
        if (ps == null)
            ps = new List<TupleForStep>();


        var list = chessboard.figures.Where(c => c.army == army);

        List<TupleForStep> fpw = new List<TupleForStep>(); //figure-point-weight

        foreach (var l in list)
        {
            chessboard.CheckFieldLinksForFigure(l);
            var pfs = l.GetPointsForStepWithOtherFigures();
            var pfa = l.GetPointsUnderAttackWithOtherFigures();

            if (pfs.Count == 0 && pfa.Count == 0) break;
            var d = ps.Where(c => c.figure == l);
            int currentWeight = 0;
            if (d.Count() > 0)
                currentWeight = CalculateWeight(l, d.Last().point);
            else
                currentWeight = CalculateWeight(l, l.currentPoint);

            foreach (var p in pfs)
            {
                int newWeight = CalculateWeight(l, p);

                if ((!p.emptyField && pfa.Contains(p)) || p.emptyField)
                    fpw.Add(new TupleForStep(l, p, newWeight - currentWeight));
            }

            foreach (var p in pfa)
            {
                if (!pfs.Contains(p))
                {
                    int newWeight = CalculateWeight(l, p);

                    if (!p.emptyField)
                        fpw.Add(new TupleForStep(l, p, newWeight - currentWeight));
                }
            }
        }

        var maxWeight = fpw.Max(c => c.Weight);

        var lfpw = fpw.Where(k => k.Weight == maxWeight); // list figure-point with max Weight

        currentRate++;
        GameFigure figure = null;
        GameFieldPoint point = null;
        int weight = 0;
        System.Random random = new System.Random();

        List<TupleForStep> list1 = new List<TupleForStep>();
        if (lfpw.Count() > 1 && currentRate <= AI_Rate)
        {
            foreach (var (f, p, w) in lfpw)
            {
                list1.Add(new TupleForStep(f, p, w + RecGetStep(currentRate, ps.Union(new[] { new TupleForStep(f, p, w) }))));
            }
            var newMaxWeight = list1.Max(c => c.Weight);

            IEnumerable<TupleForStep> newMaxlist = list1.Where(k => k.Weight == newMaxWeight); // 


            if (newMaxlist.Count() == 1)
                (figure, point, weight) = list1.First();
            else if (newMaxlist.Count() > 1)
                (figure, point, weight) = list1[random.Next(0, list1.Count())];
            else
                throw new Exception("if (newMaxlist.Count() < 1)");

        }
        else
        if (lfpw.Count() == 1)
            (figure, point, weight) = lfpw.First();
        else 
        if (currentRate > AI_Rate)
            (figure, point, weight) = lfpw.ElementAt(random.Next(0, lfpw.Count()));


        if (currentRate == 1)
        {
            figure.SetTargetPos(point.GetVector);

            if (!point.emptyField)
                chessboard.RemoveFigure(point.figureOnThisPoint);

            chessboard.ClearAllAreas();
            chessboard.ClearAllAttackLinks();
        }

        return weight;
    }

    public void GetStep()
    {
        int kingState = ProtectionKing();
        if (kingState == 1)
            return;
        else if (kingState == -1)
        {
            chessboard.RemoveFigure(aiKing);
            return;
        }

        var list = chessboard.figures.Where(c => c.army == army);

        List<TupleForStep> fpw = new List<TupleForStep>();

        foreach (var l in list)
        {
            chessboard.CheckFieldLinksForFigure(l);
            var pfs = l.GetPointsForStepWithOtherFigures();
            var pfa = l.GetPointsUnderAttackWithOtherFigures();

            if (pfs.Count == 0 && pfa.Count == 0) break;

            int currentWeight = RecCalcWight(l, l.currentPoint, AI_Rate);

            foreach (var p in pfs)
            {
                int newWeight = RecCalcWight(l, p, AI_Rate);

                if ((!p.emptyField && pfa.Contains(p)) || p.emptyField)
                    fpw.Add(new TupleForStep(l, p, newWeight - currentWeight));
            }

            foreach (var p in pfa)
            {
                if (!pfs.Contains(p))
                {
                    int newWeight = RecCalcWight(l, p, AI_Rate - 1);

                    if (!p.emptyField)
                        fpw.Add(new TupleForStep(l, p, newWeight - currentWeight));
                }
            }
        }

        if (fpw.Count == 0)
        {
            Debug.Log("Мат!");
            chessboard.RemoveFigure(aiKing);
            return;
        }

        var (figure, point, weight) = fpw.Where(k => k.Weight == fpw.Max(c => c.Weight)).First();

        figure.SetTargetPos(point.GetVector);

        if (!point.emptyField)
            chessboard.RemoveFigure(point.figureOnThisPoint);

        chessboard.ClearAllAreas();
        chessboard.ClearAllAttackLinks();
        //chessboard.ClearBoardDrawing();

        //chessboard.CheckArmy();    //у нас дважды менялась сторона. Я сделал это оптимальней.
        //Можешь смело удалять отсюда весть закомменченый код.
    }
    private int CalculateWeight(GameFigure l, GameFieldPoint point)
    {
        int calcWeight = 0; // wight for point
        var laf = point.attackFigures; //list figure can go at this point
        bool Ua = false; // under attack - if figure from opposite army can attack at this point 
        int hp = 0; // have protect - count figure from my army can attack at this point
        if (laf.Count() > 0)
            foreach (var c in laf)
                if (c.army == army)
                    hp++;
                else
                    Ua = true;

        if (hp > 0)
            calcWeight += ((int)l.type * hp) * ProtectMulti;

        if (Ua)
        {
            calcWeight -= ((int)l.type * (int)l.type) * ProtectMulti;
            if (l.type == FigureType.King)
                calcWeight -= 9000;
        }

        var gpuawopf = l.GetPointsUnderAttackFromPoint(point);
        int Ha = 0; // have attack - figure can attack at this point
        int hs = 0; // have support - figure can support at this point

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
            calcWeight += Ha * Ha * Ha * AttackMulti;

        if (hs > 0)
            calcWeight += hs * ProtectMulti;



        return calcWeight;
    }

    private int RecCalcWight(GameFigure l, GameFieldPoint point, int rate)
    {
        int calcWeight = CalculateWeight(l, point);

        if (rate > 0)
        {
            int weightFromNextPoint = calcWeight;
            var pointsForStep = l.GetPointsForStepWithOtherFigures();
            var pointsForAttack = l.GetPointsUnderAttackWithOtherFigures();
            foreach (var p in pointsForStep)
            {
                int newWeight = RecCalcWight(l, p, AI_Rate - 1);

                if (weightFromNextPoint > calcWeight + newWeight)
                {
                    weightFromNextPoint = calcWeight + newWeight;
                }
            }

            foreach (var p in pointsForAttack)
            {
                int newWeight = RecCalcWight(l, p, AI_Rate - 1);

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
                    weightBufer = RecCalcWight(aiKing, item, AI_Rate);
                    if (weightBufer > currentWeight)
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
                        weightBufer = RecCalcWight(aiKing, item, AI_Rate);
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

public class TupleForStep
{
    public GameFigure figure = null;
    public GameFieldPoint point = null;
    public ref int Weight => ref weight;
    private int weight = 0;

    public void Deconstruct(out GameFigure figure, out GameFieldPoint point, out int weight)
    {
        figure = this.figure;
        point = this.point;
        weight = this.weight;
    }

    public TupleForStep(GameFigure figure, GameFieldPoint point, int weight)
    {
        this.figure = figure;
        this.point = point;
        this.weight = weight;
    }

    public override string ToString()
    {
        return $"figure = {figure.type},point = {point.GetVector},weight = {weight}";
    }
}