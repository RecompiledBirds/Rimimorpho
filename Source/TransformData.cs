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
        private ThingDef targetDef;
        private XenotypeDef xenoDef;

        private float calculatedWorkTicks;
        private double calculatedEnergyUsed;

        public ThingDef TargetRace => targetDef;

        public XenotypeDef TargetXenoDef => xenoDef;

        public float CalculatedWorkTicks => calculatedWorkTicks;

        public double CalculatedEnergyUsed => calculatedEnergyUsed;

        public TransformData() { }

        public TransformData(ThingDef targetDef, XenotypeDef other, float calculatedWorkTicks, double calculatedEnergyUsed)
        {
            this.targetDef = targetDef;
            this.xenoDef = other;
            this.calculatedWorkTicks = calculatedWorkTicks;
            this.calculatedEnergyUsed = calculatedEnergyUsed;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref calculatedWorkTicks, nameof(calculatedWorkTicks));
            Scribe_Values.Look(ref calculatedEnergyUsed, nameof(calculatedEnergyUsed));

            Scribe_Defs.Look(ref xenoDef, nameof(xenoDef));
            Scribe_Defs.Look(ref targetDef, nameof(targetDef));
        }
    }
}
