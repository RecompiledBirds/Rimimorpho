using RimWorld;
using RVCRestructured.Shifter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Rimimorpho
{
    public class TransformData : IExposable
    {
        private Pawn pawn;
        private ThingDef targetDef;
        private XenotypeDef xenoDef;

        private bool active = false;
        private int calculatedWorkTicks;
        private double calculatedEnergyUsed;

        public ThingDef TargetRace => targetDef;

        public XenotypeDef TargetXenoDef => xenoDef;

        public bool Active { get => active; set => active = value; }

        public int CalculatedWorkTicks => calculatedWorkTicks;

        public double CalculatedEnergyUsed => calculatedEnergyUsed;

        public float PredictedTotalFoodUse => (float)CalculatedEnergyUsed / 2 + pawn.needs.food.FoodFallPerTick * (CalculatedWorkTicks / SkillStatVal);

        public float PredictedTotalRestUse => (float)CalculatedEnergyUsed / 2 + pawn.needs.rest.RestFallPerTick * (CalculatedWorkTicks / SkillStatVal);

        public float SkillStatVal => pawn.GetStatValue(AmphiDefs.RimMorpho_TransformationStat) * 1.7f;

        public bool AbleToTransformFullWork => HasEnoughFoodLeft(CalculatedWorkTicks) && HasEnoughRestLeft(CalculatedWorkTicks);

        public TransformData() { }

        public TransformData(Pawn pawn, ThingDef targetDef, XenotypeDef xenoDef, int calculatedWorkTicks, double calculatedEnergyUsed)
        {
            this.pawn = pawn;
            this.targetDef = targetDef;
            this.xenoDef = xenoDef;
            this.calculatedWorkTicks = calculatedWorkTicks;
            this.calculatedEnergyUsed = calculatedEnergyUsed;
        }

        public float PredictedFoodUse(float remainingTicks) => (float)CalculatedEnergyUsed / 2 + pawn.needs.food.FoodFallPerTick * (remainingTicks / SkillStatVal);

        public float PredictedRestUse(float remainingTicks) => (float)CalculatedEnergyUsed / 2 + pawn.needs.rest.RestFallPerTick * (remainingTicks / SkillStatVal);

        public bool HasEnoughFoodLeft(float remainingTicks, bool muteErrors = false)
        {
            bool flag = pawn.needs.food.CurLevel > PredictedFoodUse(remainingTicks);
            if (!muteErrors && !flag)  Messages.Message($"{pawn.LabelShortCap} doesn't have enough food left!", MessageTypeDefOf.RejectInput, false); //TODO: Translationstring
            return flag;
        }

        public bool HasEnoughRestLeft(float remainingTicks, bool muteErrors = false)
        {
            bool flag = pawn.needs.rest.CurLevel > PredictedRestUse(remainingTicks);
            if (!muteErrors && !flag) Messages.Message($"{pawn.LabelShortCap} doesn't have enough rest left!", MessageTypeDefOf.RejectInput, false); //TODO: Translationstring
            return flag;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref pawn, nameof(pawn));

            Scribe_Values.Look(ref active, nameof(active));
            Scribe_Values.Look(ref calculatedWorkTicks, nameof(calculatedWorkTicks));
            Scribe_Values.Look(ref calculatedEnergyUsed, nameof(calculatedEnergyUsed));

            Scribe_Defs.Look(ref xenoDef, nameof(xenoDef));
            Scribe_Defs.Look(ref targetDef, nameof(targetDef));
        }
    }
}
