﻿using RimWorld;
using RVCRestructured;
using RVCRestructured.Shifter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Rimimorpho
{
    public static class ShiftUtils
    {
        public const int ticksASecond = 60;
        public static int StatDifferenceBetweenThings(StatDef stat, ThingDef a, ThingDef b)
        {
            float statA = a.statBases.GetStatValueFromList(stat,0);
            float statB = b.statBases.GetStatValueFromList(stat,0);

            return (int)Math.Abs(statA - statB);
        }

        public static int StatDiff(ThingDef a, ThingDef b)
        {
            int result = 0;
            foreach(StatModifier stat in a.statBases)
            {
                result+=StatDifferenceBetweenThings(stat.stat,a,b);
            }
            return result/a.statBases.Count;
        }

        public static int StatDifferenceBetweenGenes(StatDef stat, GeneDef a, GeneDef b)
        {
            float statA = StatFromGene(stat, a);
            float statB = StatFromGene(stat, b);

            return (int)Math.Abs(statA - statB);
        }

        public static float StatFromGene(StatDef stat, GeneDef gene) => gene.statOffsets.GetStatOffsetFromList(stat)+gene.statFactors.GetStatFactorFromList(stat);

        public static int StatDiff(XenotypeDef a, XenotypeDef b)
        {
            float resultA = 0;
            float resultB = 0;
            if (a != null)
            {
                foreach (GeneDef gene in a.AllGenes)
                {
                    if (gene.statOffsets != null)
                    {
                        foreach (StatModifier stat in gene.statOffsets)
                        {
                            resultA += StatFromGene(stat.stat, gene);
                        }
                    }
                    if (gene.statFactors != null)
                    {
                        foreach (StatModifier stat in gene.statFactors)
                        {
                            resultA += StatFromGene(stat.stat, gene);
                        }
                    }
                }
            }
            if (b != null)
            {
                foreach (GeneDef gene in b.AllGenes)
                {
                    if (gene.statOffsets != null)
                    {
                        foreach (StatModifier stat in gene.statOffsets)
                        {
                            resultB += StatFromGene(stat.stat, gene);
                        }
                    }
                    if (gene.statFactors != null)
                    {
                        foreach (StatModifier stat in gene.statFactors)
                        {
                            resultA += StatFromGene(stat.stat, gene);
                        }
                    }
                }
            }
            return (int)Math.Abs(resultA-resultB);
        }

        public static int ShiftDifficulty(Pawn pawn, ShapeshifterComp shapeshifterComp, ThingDef targetDef,XenotypeDef other = null)
        {
            if (other == null) other = XenotypeDefOf.Baseliner;
            int result = 0;
            result += StatDiff(shapeshifterComp.CurrentForm, targetDef);
            if(ModLister.BiotechInstalled)
                result += StatDiff(pawn.genes.Xenotype,other);
            return result/((pawn.skills.GetSkill(AmphiDefs.RimMorpho_Shifting).Level/5)+1);
        }

        public static int ShiftDifficulty(Pawn pawn, ShapeshifterComp shapeshifterComp, Pawn target)
        {
            return ShiftDifficulty(pawn,shapeshifterComp,target.def,target.genes.Xenotype);
        }

        public static TransformData GetTransformData(Pawn pawn, ShapeshifterComp shapeshifterComp, ThingDef targetDef, XenotypeDef other = null, float difficultyScale = 1f)
        {
            float difficulty = ShiftDifficulty(pawn, shapeshifterComp, targetDef, other) * difficultyScale;
            double x = DifficultyToEnergyXVal(difficulty);
            double energyUsed = DifficultyToEnergy(difficulty, x);
            int workTicks = TicksForTransform(difficulty, energyUsed, x);

            return new TransformData(pawn, targetDef, other, workTicks, energyUsed);
        }

        public static TransformData GetTransformData(Pawn pawn, ShapeshifterComp shapeshifterComp, Pawn target, float difficultyScale = 1f)
        {
            return GetTransformData(pawn, shapeshifterComp, target.def, target.genes?.Xenotype, difficultyScale);
        }

        /// <summary>
        /// Get the X value. mostly used to avoid weird cases if we somehow got a negative.
        /// </summary>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public static double DifficultyToEnergyXVal(float difficulty)
        {
            //D(x)=abs(max(x,1))+1
            return Math.Abs(Math.Max(difficulty,1))+1;
        }

        /// <summary>
        /// Get the energy used from a given difficulty
        /// </summary>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public static double DifficultyToEnergy(float difficulty, double energyX)
        {
            //f(x)=min(log(D(x)+1)*log(2)/2),2)
            return Math.Min((Math.Log(energyX) * 0.301) / 2, 2);
        }

        public static int TicksForTransform(float difficulty, double energy, double energyX)
        {
            //t(x)=10((d(x)/4)^2k(x))+60
            return Convert.ToInt32(10d * Math.Pow(energyX / 4d, 2d * energy) + 60d);
        }

    }
}
