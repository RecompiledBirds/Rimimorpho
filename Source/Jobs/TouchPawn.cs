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
    public class TouchPawn : JobDriver
    {
        private static readonly Random random = new Random();
        private TransformData transformData;

        private float workLeft = -1000f;
        private float workOriginal = -1000f;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref workLeft, nameof(workLeft));
            Scribe_Values.Look(ref workOriginal, nameof(workOriginal));
            Scribe_Deep.Look(ref transformData, nameof(transformData));
        }

        public override string GetReport()
        {
            return "Rimimorpho_LearningFromPawn".Translate((TargetA.Pawn.Name != null ? TargetA.Pawn.Name.ToStringShort : TargetA.Pawn.Label).Named("TARGET_NAME"));
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil getAndMakeData = ToilMaker.MakeToil("GetAndMakeDataToil");
            getAndMakeData.defaultCompleteMode = ToilCompleteMode.Instant;

            Toil pathing = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            
            Toil learningSpecies = ToilMaker.MakeToil("LearningSpeciesToil");
            learningSpecies.activeSkill = () => AmphiDefs.RimMorpho_Shifting;
            learningSpecies.socialMode = RandomSocialMode.SuperActive;
            learningSpecies.defaultCompleteMode = ToilCompleteMode.Never;
            learningSpecies.WithProgressBar(TargetIndex.B, () => 1f - workLeft / workOriginal);

            getAndMakeData.initAction = () =>
            {
                transformData = ShiftUtils.GetTransformData(learningSpecies.actor, pawn.TryGetComp<AmphiShifter>(), TargetA.Pawn.def, difficultyScale: 0.3333f);
                transformData.Active = true;

                workLeft = transformData.CalculatedWorkTicks;
                workOriginal = transformData.CalculatedWorkTicks;

                RVCLog.Log($"Workamount: {workLeft}, " +
                    $"current food level: {pawn.needs.food.CurLevel}, " +
                    //$"predicted food level: {learningSpecies.actor.needs.food.CurLevel - energy / 2f}, " +
                    $"predicted tick duration: {workLeft / pawn.GetStatValue(AmphiDefs.RimMorpho_TransformationStat)}, " +
                    $"processed speed: {pawn.GetStatValue(AmphiDefs.RimMorpho_TransformationStat)}", debugOnly: true);
            };

            learningSpecies.tickAction = () =>
            { 
                float dist = pawn.Position.DistanceTo(TargetA.Pawn.Position);
                if (dist > 5)
                {
                    if(!pawn.pather.Moving)
                        pawn.pather.StartPath(TargetA.Pawn.Position,PathEndMode.ClosestTouch);
                    return;
                }
                float learningBonus = 5;
                learningBonus -= dist;
                float adjustedSkillVal = pawn.GetStatValue(AmphiDefs.RimMorpho_TransformationStat);
                workLeft -= adjustedSkillVal + learningBonus;

                pawn.skills?.Learn(AmphiDefs.RimMorpho_Shifting, 1f, false);

                if (random.Next(1, 100) >= 99)
                {
                    if (CellFinder.TryFindRandomReachableCellNear(pawn.Position, pawn.Map, 2, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false, false, false), (IntVec3 x) => x.Standable(pawn.Map), (Region x) => true, out IntVec3 cell, 999999))
                    {
                        FilthMaker.TryMakeFilth(cell, pawn.Map, AmphiDefs.RimMorpho_AmphimorphoGoo, 1, FilthSourceFlags.Pawn);
                    }
                }

                if (workLeft <= 0f)
                {
                    RVCLog.Log($"Workamount: {workLeft}, current food level: {pawn.needs.food.CurLevel}", debugOnly: true);
                    pawn.TryGetComp<AmphiShifter>().LearnSpecies(TargetA.Pawn);
                    ReadyForNextToil();
                }
            };
            
            yield return getAndMakeData;
            yield return pathing;
            yield return learningSpecies;
        }
    }
}
