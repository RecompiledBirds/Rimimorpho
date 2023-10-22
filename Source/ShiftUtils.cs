using RimWorld;
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
        public static int StatDifferenceBetweenThings(StatDef stat, ThingDef a, ThingDef b)
        {
            float statA = a.statBases.GetStatValueFromList(stat,0);
            float statB = b.statBases.GetStatValueFromList(stat,0);

            return (int)Math.Abs(statA - statB);
        }

        public static int AvgStatDiff(ThingDef a, ThingDef b)
        {
            int result = 0;
            foreach(StatModifier stat in a.statBases)
            {
                result+=StatDifferenceBetweenThings(stat.stat,a,b);
            }
            result /= a.statBases.Count;
            RVCLog.Log(result);
            return result;
        }

        public static int StatDifferenceBetweenGenes(StatDef stat, GeneDef a, GeneDef b)
        {
            float statA = StatFromGene(stat, a);
            float statB = StatFromGene(stat, b);

            return (int)Math.Abs(statA - statB);
        }

        public static float StatFromGene(StatDef stat, GeneDef gene) => gene.statOffsets.GetStatValueFromList(stat,0) * gene.statFactors.GetStatValueFromList(stat, 1);

        public static int AvgStatDiff(XenotypeDef a, XenotypeDef b)
        {
            return 0;
        }

        public static int ShiftDifficulty(Pawn pawn, ShapeshifterComp shapeshifterComp, ThingDef targetDef,XenotypeDef other = null)
        {
            if (other == null) other = XenotypeDefOf.Baseliner;
            int result = 0;
            result += AvgStatDiff(shapeshifterComp.CurrentForm, targetDef);
            result += AvgStatDiff(pawn.genes.Xenotype,other);
            return result;
        }

        public static int ShiftDifficulty(Pawn pawn, ShapeshifterComp shapeshifterComp, Pawn target)
        {
            return ShiftDifficulty(pawn,shapeshifterComp,target.def,target.genes.Xenotype);
        }


        

        public static double DifficultyToEnergy(float energy)
        {
            return Math.Min(Math.Log(energy + 1), 2);
        }
    }
}
