using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ChessAIScript : MonoBehaviour
{
    public int AI_Depth; //количество шагов в глубину для каждого хода.
    public GameFieldOrigin chessboard;
    public Army army = Army.Black;
    public KingScript aiKing;
    [Range(1, 4)]
    public int AttackMulti = 1;
    [Range(1, 4)]
    public int ProtectMulti = 1;

    public void Initiolize()
    {
        AI_Depth = GameFieldSettingsPack.AIStepRate;
        army = GameFieldSettingsPack.AIArmy;

        aiKing = army == Army.White ? (KingScript)chessboard.kings[0] : (KingScript)chessboard.kings[1];

        if (GameFieldSettingsPack.AISetting > 0)
            AttackMulti = GameFieldSettingsPack.AISetting;
        if (GameFieldSettingsPack.AISetting < 0)
            ProtectMulti = GameFieldSettingsPack.AISetting;
    }
    public void AIStep()
    {
        chessboard.ClearAllAreas();
        chessboard.ClearAllAttackLinks();

        var list = chessboard.figures.Where(c => c.army == army).ToList(); // my army
        foreach (var l in list)
            chessboard.CheckFieldLinksForFigure(l);


        var steps = Culc(list);

        if (steps.Count() == 0)
        {
            Debug.Log("ZERO");
            aiKing.SetTargetPos(aiKing.currentPosition);
            chessboard.RemoveFigure(aiKing);
            return;
        }

        int maxWi = steps.Select(c => c.newWeight.Weight - c.oldWeight.Weight).Max();

        var filteredSteps = steps.Where(c => c.newWeight.Weight - c.oldWeight.Weight == maxWi).Select(f => f).ToList();

        if (AI_Depth > 0 && filteredSteps.Count() > 1)
        {
            List<int> depthWeigth = new List<int>();
            foreach (var fs in filteredSteps)
            {
                List<(GameFigure figure, GameFieldPoint point, PointWeight oldWeight, PointWeight newWeight)> ps
                    = new List<(GameFigure figure, GameFieldPoint point, PointWeight oldWeight, PointWeight newWeight)>
                {fs };

                List<GameFigure> ignoredfigures = new List<GameFigure>();
                if (!fs.point.emptyField)
                    ignoredfigures.Add(fs.point.figureOnThisPoint);

                depthWeigth.Add(DepthStep(list, ps, ignoredfigures));
            }

            int depthMax = depthWeigth.Max();
            List<int> depthMaxIndexs = new List<int>();
            for (int i = 0; i < depthWeigth.Count(); i++)
                if (depthWeigth[i] == depthMax)
                    depthMaxIndexs.Add(i);

            filteredSteps = depthMaxIndexs.Select(i => filteredSteps[i]).ToList();
        }


        if (filteredSteps.Count() > 1)
            RandomStep(filteredSteps);
        else
            FirstStep(filteredSteps);

        chessboard.ClearAllAreas();
        chessboard.ClearAllAttackLinks();
        return;
    }
    int DepthStep(List<GameFigure> list,
            List<(GameFigure figure, GameFieldPoint point, PointWeight oldWeight, PointWeight newWeight)> fipoww,
            IEnumerable<GameFigure> ignoredfigures,
            int depth = 0)
    {
        depth++;

        var steps2 = Culc(list, fipoww.Select(c => (c.figure, c.point)), ignoredfigures);

        int maxWi2 = steps2.Select(c => c.newWeight.Weight - c.oldWeight.Weight).Max();

        if (depth < AI_Depth)
        {
            var filteredSteps2 = steps2.Where(c => c.newWeight.Weight - c.oldWeight.Weight == maxWi2).Select(f => f).ToList();
            List<int> depthWeigth2 = new List<int>();
            foreach (var fs in filteredSteps2)
            {
                var fs3 = fipoww.Select(c => c).ToList();
                fs3.Add(fs);

                var igFig = ignoredfigures.Select(f => f).ToList();
                if (!fs.point.emptyField)
                    igFig.Add(fs.point.figureOnThisPoint);

                depthWeigth2.Add(DepthStep(list, fs3, igFig, depth));
            }
            maxWi2 += depthWeigth2.Max();
        }
        return maxWi2;
    }
    void RandomStep(List<(GameFigure figure, GameFieldPoint point, PointWeight oldWeight, PointWeight newWeight)> steps1)
    {
        System.Random random = new System.Random();

        var (figure, point, _, _) = steps1[random.Next(0, steps1.Count())];

        figure.SetTargetPos(point.GetVector);

        if (!point.emptyField)
            chessboard.RemoveFigure(point.figureOnThisPoint);
    }

    void FirstStep(List<(GameFigure figure, GameFieldPoint point, PointWeight oldWeight, PointWeight newWeight)> steps2)
    {
        var (figure, point, _, _) = steps2.First();

        figure.SetTargetPos(point.GetVector);

        if (!point.emptyField)
            chessboard.RemoveFigure(point.figureOnThisPoint);
    }
    List<(GameFigure figure, GameFieldPoint point, PointWeight oldWeight, PointWeight newWeight)> Culc(
           List<GameFigure> figures,
           IEnumerable<(GameFigure figure, GameFieldPoint point)> oldFigPoi = null,
           IEnumerable<GameFigure> ignoredfigures = null)
    {
        List<(GameFigure figure, GameFieldPoint point, PointWeight oldWeight, PointWeight newWeight)> FigPoiWiWi
       = new List<(GameFigure figure, GameFieldPoint point, PointWeight oldWeight, PointWeight newWeight)>();
        //figure - point - old weight - new weight

        foreach (var fig in figures)
        {
            var pfs = fig.GetPointsForStepWithOtherFigures();
            var pfa = fig.GetPointsUnderAttackWithOtherFigures();

            if (pfs.Count == 0 && pfa.Count == 0) continue;

            GameFieldPoint gamePoint = fig.currentPoint;

            if (oldFigPoi != null)
            {
                var olds = oldFigPoi.Where(c => c.figure == fig);
                if (olds.Count() > 0)
                    gamePoint = olds.Last().point;
            }

            PointWeight oldW = CalculateWeightClass(fig, gamePoint, ignoredfigures);

            foreach (var p in pfs)
            {
                PointWeight newW = CalculateWeightClass(fig, p, ignoredfigures);
                FigPoiWiWi.Add((fig, p, oldW, newW));
            }

            foreach (var p in pfa)
                if (!pfs.Contains(p))
                {
                    PointWeight newW = CalculateWeightClass(fig, p, ignoredfigures);
                    if (!p.emptyField)
                        FigPoiWiWi.Add((fig, p, oldW, newW));
                }
        }

        return FigPoiWiWi;
    }
    PointWeight CalculateWeightClass(GameFigure l,
        GameFieldPoint point,
        IEnumerable<GameFigure> ignoredfigures = null)
    {
        List<int> myarmy = new List<int>();
        List<int> oppositearmy = new List<int>();

        var laf = point.attackFigures;

        if (laf.Count() > 0)
            foreach (var c in laf)
                if (c != l && (ignoredfigures == null || (ignoredfigures != null && !ignoredfigures.Contains(c))))
                    if (c.army == army)
                        myarmy.Add((int)c.type);
                    else
                        oppositearmy.Add((int)c.type);


        var gpuawopf = l.GetPointsUnderAttackFromPoint(point);
        bool army2 = true;
        FigureType figureType2 = FigureType.Pawn;
        if (!point.emptyField
            && point.figureOnThisPoint != l
           && (ignoredfigures == null || (ignoredfigures != null && !ignoredfigures.Contains(point.figureOnThisPoint))))
        {
            figureType2 = point.figureOnThisPoint.type;
            army2 = point.figureOnThisPoint.army == army;
        }

        List<int> support = new List<int>();
        List<int> attack = new List<int>();


        foreach (var g in gpuawopf)
        {
            if (!g.emptyField
                && g.figureOnThisPoint != l
                && (ignoredfigures == null || (ignoredfigures != null && !ignoredfigures.Contains(g.figureOnThisPoint))))
            {
                if (g.figureOnThisPoint.army != army)
                    attack.Add((int)g.figureOnThisPoint.type);
                else
                    support.Add((int)g.figureOnThisPoint.type);
            }
        }

        return new PointWeight(
            ProtectMulti,
            AttackMulti,
            l.type,
            myarmy.ToArray(),
            oppositearmy.ToArray(),
            support.ToArray(),
            attack.ToArray(),
            point.emptyField,
            figureType2,
            army2);
    }
    public int RecGetStep(int currentDepth = -1, IEnumerable<TupleForStep> ps = null)
    {
        if (ps == null)
            ps = new List<TupleForStep>();

        //int kingState = ProtectionKing();
        //if (kingState == 1)
        //    return 0;
        //else if (kingState == -1)
        //{
        //    chessboard.RemoveFigure(aiKing);
        //    return 0;
        //}

        var list = chessboard.figures.Where(c => c.army == army).ToList(); // my army

        List<TupleForStep> fpw = new List<TupleForStep>(); //figure-point-weight

        foreach (var l in list)
        {
            chessboard.CheckFieldLinksForFigure(l);

            var pfs = l.GetPointsForStepWithOtherFigures();
            var pfa = l.GetPointsUnderAttackWithOtherFigures();
            if (l.type == FigureType.King)
            {

            }

            if (pfs.Count == 0 && pfa.Count == 0) continue;
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

        if (fpw.Count == 0)
        {
            Debug.Log("Мат!");
            chessboard.RemoveFigure(aiKing);
            return 0;
        }

        int maxWeight = fpw.Max(c => c.Weight);


        var lfpw = fpw.Where(k => k.Weight == maxWeight); // list figure-point with max Weight

        currentDepth++;
        GameFigure figure = null;
        GameFieldPoint point = null;
        int weight = 0;
        System.Random random = new System.Random();

        List<TupleForStep> list1 = new List<TupleForStep>();
        if (lfpw.Count() > 1 && currentDepth <= AI_Depth)
        {
            foreach (var (f, p, w) in lfpw)
            {
                list1.Add(new TupleForStep(f, p, w + RecGetStep(currentDepth, ps.Union(new[] { new TupleForStep(f, p, w) }))));
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
        if (currentDepth > AI_Depth)
            (figure, point, weight) = lfpw.ElementAt(random.Next(0, lfpw.Count()));


        if (currentDepth == 0)
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

            int currentWeight = RecCalcWight(l, l.currentPoint, AI_Depth);

            foreach (var p in pfs)
            {
                int newWeight = RecCalcWight(l, p, AI_Depth);

                if ((!p.emptyField && pfa.Contains(p)) || p.emptyField)
                    fpw.Add(new TupleForStep(l, p, newWeight - currentWeight));
            }

            foreach (var p in pfa)
            {
                if (!pfs.Contains(p))
                {
                    int newWeight = RecCalcWight(l, p, AI_Depth - 1);

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
                if (c != l)
                    if (c.army == army)
                        hp++;
                    else
                        Ua = true;


        if (l.type == FigureType.King && aiKing.underAttack)
            if (!Ua)
                calcWeight += 9000;

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
        int has = 0; // have attack figure at this point

        if (!point.emptyField && point.figureOnThisPoint.army != army)
        {
            if (point.figureOnThisPoint.type == FigureType.King)
            {
                has = (int)point.figureOnThisPoint.type + 100;
            }
            else
            {
                has = (int)point.figureOnThisPoint.type;
            }
        }

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

        if (has > 0)
            calcWeight += has * has * has * AttackMulti;

        if (aiKing.underAttack)
        {
            calcWeight -= 1000;
            var AttackingKing = aiKing.currentPoint.attackFigures;
            if (AttackingKing.Count() > 0)
                foreach (var ak in AttackingKing)
                {
                    if (ak.currentPoint == point)
                        calcWeight += 1000;
                }
        }

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
                int newWeight = RecCalcWight(l, p, AI_Depth - 1);

                if (weightFromNextPoint > calcWeight + newWeight)
                {
                    weightFromNextPoint = calcWeight + newWeight;
                }
            }

            foreach (var p in pointsForAttack)
            {
                int newWeight = RecCalcWight(l, p, AI_Depth - 1);

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
                    weightBufer = RecCalcWight(aiKing, item, AI_Depth);
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
                        weightBufer = RecCalcWight(aiKing, item, AI_Depth);
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