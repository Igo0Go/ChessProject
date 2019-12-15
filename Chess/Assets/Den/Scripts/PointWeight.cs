using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PointWeight
{
    private readonly int protectMulti;
    private readonly int attackMulti;

    /// <summary>
    /// calculated wight for point
    /// </summary>
    public int Weight => attackWeight * attackMulti + protectWeight * protectMulti;

    public int attackWeight =>
        (CanAttack ? KillMax * KillMax : 0)
        + (CanKill ? FigurePointWeight * FigurePointWeight : 0);
    public int protectWeight =>
        -(UnderAttack ? FigureWeight * FigureWeight * FigureWeight : 0)
        + (CanKillProtect ? CanKillProtectsMax : 0)
        + (UnderProtect ? 1 : 0)
        + (CanProtect ? 1 : 0);

    public override string ToString()
    {
        return $"Weight: {Weight}";
    }
    private int GetWeight()
    {

        int attackWeight = 0;
        if (CanAttack)
        {
            attackWeight += KillMax * KillMax;
        }
        if (CanKill)
        {
            attackWeight += FigurePointWeight * FigurePointWeight;
        }
        attackWeight *= attackMulti;

        int protectWeight = 0;
        if (UnderAttack)
        {
            protectWeight -= FigureWeight * FigureWeight * FigureWeight;
        }
        if (UnderProtect)
        {
            protectWeight += ProtectMax;
        }
        if (CanProtect)
        {
            protectWeight += SupportMax;
        }

        //if (CanSave)
        //{
        //    protectWeight += FigurePointWeight;
        //}

        protectWeight *= protectMulti;

        return attackWeight + protectWeight;
    }


    /// <summary>
    /// my figure when go at this point, can kill opposite figure
    /// </summary>
    public bool CanAttack => KillCount > 0;
    /// <summary>
    /// my figure kill opposite figure
    /// </summary>
    public bool CanKill => emptyPoint ? false : (figurePointArmy ? false : true);


    /// <summary>
    /// if figure from opposite army can attack at this point 
    /// </summary>
    public bool UnderAttack => AttackCount > 0;
    /// <summary>
    /// my figure can receive protect at this point
    /// </summary>
    public bool UnderProtect => ProtectCount > 0;
    /// <summary>
    /// my figure can protect other my figure, at this point
    /// </summary>
    public bool CanProtect => SupportCount > 0;
    /// <summary>
    /// my figure can save other my figure, if kill opposite figure at this point
    /// </summary>
    public bool CanKillProtect => CanKillProtectsCount > 0;

    ///// <summary>
    ///// my figure save other my figure
    ///// </summary>
    //public bool CanSave => emptyPoint ? false : (figurePointArmy ? true : false);

    /// <summary>
    /// figure, which can go to this point
    /// </summary>
    private FigureType myFigure;
    private int FigureWeight => (int)myFigure;

   

    /// <summary>
    /// figure, which stay at this point
    /// </summary>
    private FigureType figurePoint;
    private int FigurePointWeight => (int)figurePoint;

    /// <summary>
    /// true - my army; 
    /// false - opposite army
    /// </summary>
    public bool figurePointArmy;
    /// <summary>
    /// point is ampty?
    /// true - empty; 
    /// false - filled
    /// </summary>
    public bool emptyPoint;

    private int CanKillProtectsCount => canKillProtects.Count();
    private int CanKillProtectsMax => canKillProtects.Max();
    private int CanKillProtectsMin => canKillProtects.Min();
    private readonly int[] canKillProtects;


    /// <summary>
    /// quantity figure from my army can attack at this point
    /// (save my figure)
    /// </summary>
    private int ProtectCount => haveProtects.Count();
    private int ProtectMax => haveProtects.Max();
    private int ProtectMin => haveProtects.Min();
    private readonly int[] haveProtects;


    /// <summary>
    /// quantity figure from oppesite can attack at this point
    /// (kill my figure)
    /// </summary>
    private int AttackCount => haveAttacks.Count();
    private int AttackMax => haveAttacks.Max();
    private int AttackMin => haveAttacks.Min();
    private readonly int[] haveAttacks;


    /// <summary>
    /// quantity figure, which my figure can save at this point
    /// (my figure save)
    /// </summary>
    private int SupportCount => haveSupports.Count();
    private int SupportMax => haveSupports.Max();
    private int SupportMin => haveSupports.Min();
    private readonly int[] haveSupports;


    /// <summary>
    /// quantity figure, which my figure can kill at this point
    /// (my figure kill)
    /// </summary>
    private int KillCount => haveKills.Count();
    private int KillMax => haveKills.Max();
    private int KillMin => haveKills.Min();
    private readonly int[] haveKills;


    public PointWeight(
        int protectMulti,
        int attackMulti,
        FigureType myFigure,
        int[] haveProtects,
        int[] haveAttacks,
        int[] haveSupports,
        int[] haveKills,
        int[] canKillProtects,
        bool emptyPoint = true,
        FigureType figurePoint = FigureType.Pawn,
        bool figurePointArmy = true)
    {
        this.protectMulti = protectMulti;
        this.attackMulti = attackMulti;
        this.myFigure = myFigure;
        this.figurePoint = figurePoint;
        this.figurePointArmy = figurePointArmy;
        this.emptyPoint = emptyPoint;
        this.haveProtects = haveProtects;
        this.haveAttacks = haveAttacks;
        this.haveSupports = haveSupports;
        this.haveKills = haveKills;
        this.canKillProtects = canKillProtects;
    }
}