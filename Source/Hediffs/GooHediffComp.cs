using Rimimorpho;
using RVCRestructured;
using RVCRestructured.Shifter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Rimimorpho
{
    public class GooHediffCompProps : HediffCompProperties
    {
        public GooHediffCompProps()
        {
            compClass=typeof(GooHediffComp);
        }
    }
    public class GooHediffComp : HediffComp
    {
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            float severity = parent.Severity;
            if (severity < 100)
            {
                severityAdjustment += (float)Math.Pow(parent.Severity,2);

                if (Pawn.def != AmphiDefs.RimMorpho_Amphimorpho) return;

                if(Rand.Chance(severity/100))
                {
                    Job job = JobMaker.MakeJob(AmphiDefs.RimMorpho_TransformTarget, parent.pawn);
                    parent.pawn.jobs.TryTakeOrderedJob(job);
                }
                return;
            }
            if (Pawn.def != AmphiDefs.RimMorpho_Amphimorpho)
                PawnChanger.ChangePawnRace(Pawn, AmphiDefs.RimMorpho_Amphimorpho);
            Pawn.health.RemoveHediff(parent);

        }
    }
}
