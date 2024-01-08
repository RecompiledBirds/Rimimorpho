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
            return "Rimimorpho_LearningFromPawn".Translate((TargetA.Pawn.Name != null ? TargetA.Pawn.Name.ToStringShort : TargetA.Pawn.Label).Named("TARGET_NAME"));
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil preWorkSetup = ToilMaker.MakeToil("MakeNewToils");
            Toil gotoToil = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            
            Toil doWork = ToilMaker.MakeToil("MakeNewToils");
            doWork.activeSkill = () => AmphiDefs.RimMorpho_Shifting;
            doWork.socialMode = RandomSocialMode.SuperActive;
            doWork.defaultCompleteMode = ToilCompleteMode.Never;
            
            

            preWorkSetup.initAction = () =>
            {
                ShiftUtils.GetTransformData(doWork.actor, doWork.actor.TryGetComp<AmphiShifter>(), TargetA.Pawn.def, out workLeft, out energy);
                //The pawn isnt actually shifting, so we can make this task a bit easier.
                workLeft /= 3;
                workOriginal = workLeft;
                RVCLog.Log($"Workamount: {workLeft}, " +
                    $"current food level: {doWork.actor.needs.food.CurLevel}, " +
                    $"predicted food level: {doWork.actor.needs.food.CurLevel - energy / 2f}, " +
                    $"predicted tick duration: {workLeft / doWork.actor.GetStatValue(AmphiDefs.RimMorpho_TransformationStat)}, " +
                    $"processed speed: {doWork.actor.GetStatValue(AmphiDefs.RimMorpho_TransformationStat)}", debugOnly: true);
            };

            doWork.tickAction = () =>
            {
                float dist = doWork.actor.Position.DistanceTo(TargetA.Pawn.Position);
                if (dist > 9)
                {
                    if(!doWork.actor.pather.Moving)
                        doWork.actor.pather.StartPath(TargetA.Pawn.Position,PathEndMode.ClosestTouch);
                    return;
                }
                float learningBonus = 5;
                learningBonus -= dist;
                float adjustedSkillVal = doWork.actor.GetStatValue(AmphiDefs.RimMorpho_TransformationStat);
                workLeft -= adjustedSkillVal+learningBonus;

                doWork.actor.skills?.Learn(AmphiDefs.RimMorpho_Shifting, 1f, false);

                if (random.Next(1, 100) >= 99)
                {
                    if (CellFinder.TryFindRandomReachableCellNear(doWork.actor.Position, doWork.actor.Map, 2, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false, false, false), (IntVec3 x) => x.Standable(doWork.actor.Map), (Region x) => true, out IntVec3 cell, 999999))
                    {
                        FilthMaker.TryMakeFilth(cell, doWork.actor.Map, AmphiDefs.RimMorpho_AmphimorphoGoo, 1, FilthSourceFlags.Pawn);
                    }
                }

                if (workLeft <= 0f)
                {
                    RVCLog.Log($"Workamount: {workLeft}, current food level: {doWork.actor.needs.food.CurLevel}", debugOnly: true);
                    doWork.actor.TryGetComp<AmphiShifter>().LearnRace(TargetA.Pawn);
                    ReadyForNextToil();
                }
            };

            preWorkSetup.defaultCompleteMode = ToilCompleteMode.Instant; 


            doWork.WithProgressBar(TargetIndex.B, () => 1f - workLeft / workOriginal);

            
            yield return preWorkSetup;
            yield return gotoToil;
            yield return doWork;
        }
    }
}
