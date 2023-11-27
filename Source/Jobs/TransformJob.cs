using Rimimorpho;
using RVCRestructured;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using System.Data.Odbc;
using RimWorld;

namespace Rimimorpho
{
    public class TransformJob : JobDriver
    {
        private ThingDef morphDef;
        private float workLeft = -1000f;
        private float workOriginal = -1000f;

        private double energyConsumed;
        private double energy;
     
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref energy, nameof(energy));
            Scribe_Values.Look(ref workLeft, nameof(workLeft));
            Scribe_Values.Look(ref workOriginal, nameof(workOriginal));
            Scribe_Values.Look(ref energyConsumed, nameof(energyConsumed));
            Scribe_Defs.Look(ref morphDef, nameof(morphDef));
        }

        //TODO: Translation strings
        public override string GetReport()
        {
            return $"Transforming.";
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            //Toil gotoToil = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
            Toil doWork = ToilMaker.MakeToil("MakeNewToils");
            doWork.initAction = () =>
            {
                morphDef = DefDatabase<ThingDef>.AllDefs.Where(x => x.race != null).RandomElement();
                ShiftUtils.GetTransformData(doWork.actor, doWork.actor.TryGetComp<AmphiShifter>(), morphDef, out workLeft, out energy);
                workOriginal = workLeft;
                Log.Message($"workLeft: {workLeft}");
            };
            doWork.tickAction = () =>
            {
                float adjustedSkillVal = doWork.actor.GetStatValue(AmphiDefs.RimMorpho_TransformationStat) * 1.7f;
                workLeft -= adjustedSkillVal;

                float energyConsumed = (float)(workLeft / workOriginal * energy);
                pawn.needs.food.CurLevel -= energyConsumed / 2f;
                pawn.needs.rest.CurLevel -= energyConsumed / 2f;

                Log.Message($"workLeft: {workLeft}, adjustedSkillVal: {adjustedSkillVal}, energyConsumed: {energyConsumed}");
                doWork.actor.skills?.Learn(AmphiDefs.RimMorpho_Shifting, 1f, false);
                if (workLeft <= 0f) ReadyForNextToil();
            };
            doWork.AddFinishAction(() =>
            {
                doWork.actor.TryGetComp<AmphiShifter>().SetForm(morphDef);
            });

            //yield return gotoToil;
            yield return doWork;
        }
    }
}
