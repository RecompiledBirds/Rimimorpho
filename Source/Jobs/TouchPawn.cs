using RimWorld;
using RVCRestructured;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Rimimorpho
{
    public class TouchPawn :JobDriver
    {
        private static readonly Random random = new Random();

        private ThingDef morphDef;
        private float workLeft = -1000f;
        private float workOriginal = -1000f;

        private double energyConsumedTotal;
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
            Scribe_Values.Look(ref energyConsumed, nameof(energyConsumed));
            Scribe_Values.Look(ref energyConsumedTotal, nameof(energyConsumedTotal));
            Scribe_Values.Look(ref workLeft, nameof(workLeft));
            Scribe_Values.Look(ref workOriginal, nameof(workOriginal));
            Scribe_Defs.Look(ref morphDef, nameof(morphDef));
        }

        public override string GetReport()
        {
            return $"Touching {(TargetA.Pawn.Name!=null?TargetA.Pawn.Name.ToStringShort:TargetA.Pawn.Label)}.";
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil preWorkSetup = ToilMaker.MakeToil("MakeNewToils");
            Toil gotoToil = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil doWork = ToilMaker.MakeToil("MakeNewToils");
            bool EnergyFailCondition()
            {
                double energyNeeded = (energy - energyConsumedTotal) / 2;

                if (energyNeeded > 1f)
                {
                    Messages.Message("Rimimorpho_CanNeverTouchPawn".Translate(doWork.actor.NameShortColored.Named("NAME"), TargetA.Pawn?.def.label.Named("RACE") ?? "Unknown".Named("RACE")), doWork.actor, MessageTypeDefOf.RejectInput);
                    return true; 
                }

                if (energyNeeded > doWork.actor.needs.food.CurLevel || energyNeeded > doWork.actor.needs.rest.CurLevel)
                {
                    Messages.Message("Rimimorpho_CanNotTouchPawn".Translate(doWork.actor.NameShortColored.Named("NAME"),TargetA.Pawn?.def.label.Named("RACE") ??"Unknown".Named("RACE")), doWork.actor, MessageTypeDefOf.RejectInput);
                    return true;
                }

                return false;
            }

            preWorkSetup.initAction = () =>
            {
                ShiftUtils.GetTransformData(doWork.actor, doWork.actor.TryGetComp<AmphiShifter>(), TargetA.Pawn.def, out workLeft, out energy);
                workOriginal = workLeft;
                RVCLog.Log($"Workamount: {workLeft}, " +
                    $"current food level: {doWork.actor.needs.food.CurLevel}, " +
                    $"predicted food level: {doWork.actor.needs.food.CurLevel - energy / 2f}, " +
                    $"predicted tick duration: {workLeft / doWork.actor.GetStatValue(AmphiDefs.RimMorpho_TransformationStat)}, " +
                    $"processed speed: {doWork.actor.GetStatValue(AmphiDefs.RimMorpho_TransformationStat)}", debugOnly: true);
            };

            doWork.tickAction = () =>
            {
                float adjustedSkillVal = doWork.actor.GetStatValue(AmphiDefs.RimMorpho_TransformationStat);
                workLeft -= adjustedSkillVal;

                float energyConsumed = (float)(adjustedSkillVal / workOriginal * energy);
                doWork.actor.needs.food.CurLevel -= energyConsumed / 2f;
                doWork.actor.needs.rest.CurLevel -= energyConsumed / 2f;
                energyConsumedTotal += energyConsumed;

                doWork.actor.skills?.Learn(AmphiDefs.RimMorpho_Shifting, 1f, false);

                if (random.Next(1, 100) >= 95)
                {
                    if (CellFinder.TryFindRandomReachableCellNear(doWork.actor.Position, doWork.actor.Map, 2, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false, false, false), (IntVec3 x) => x.Standable(doWork.actor.Map), (Region x) => true, out IntVec3 cell, 999999))
                    {
                        FilthMaker.TryMakeFilth(cell, doWork.actor.Map, AmphiDefs.RimMorpho_AmphimorphoGoo, 1, FilthSourceFlags.Pawn);
                    }
                }

                if (workLeft <= 0f)
                {
                    RVCLog.Log($"Workamount: {workLeft}, current food level: {doWork.actor.needs.food.CurLevel}", debugOnly: true);
                    doWork.actor.TryGetComp<AmphiShifter>().SetForm(TargetA.Pawn);
                    ReadyForNextToil();
                }
            };

            preWorkSetup.defaultCompleteMode = ToilCompleteMode.Instant; 

            gotoToil.AddFailCondition(EnergyFailCondition);
            doWork.AddFailCondition(EnergyFailCondition);

            doWork.WithProgressBar(TargetIndex.B, () => 1f - workLeft / workOriginal);
            doWork.socialMode = RandomSocialMode.Quiet;
            doWork.defaultCompleteMode = ToilCompleteMode.Never;
            doWork.activeSkill = () => AmphiDefs.RimMorpho_Shifting;

            yield return preWorkSetup;
            yield return gotoToil;
            yield return doWork;
        }
    }
}
