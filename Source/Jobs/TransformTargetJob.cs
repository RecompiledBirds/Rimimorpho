using RVCRestructured;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using RimWorld;

namespace Rimimorpho
{
    public class TransformTargetJob : JobDriver
    {
        private const float DifficultyModifier = 0.25f;

        private AmphiShifter amphiShifter;
        private TransformData transformData;

        private IntVec3 lookingAt;
        private float workLeft = -1000f;

        private double energyConsumed;
        private double energy;

        public TransformData TransformData => transformData;

        private AmphiShifter ShifterComp => amphiShifter ?? (amphiShifter = pawn.TryGetComp<AmphiShifter>());

        public static StoredRace NextRaceTarget { get; internal set; }
        public static XenotypeDef NextXenoTarget { get; internal set; }
        public static BodyTypeDef NextBodyTypeTarget { get; internal set; }

        public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref energy, nameof(energy));
            Scribe_Values.Look(ref workLeft, nameof(workLeft));
            Scribe_Values.Look(ref energyConsumed, nameof(energyConsumed));

            Scribe_Deep.Look(ref transformData, nameof(transformData));
        } 

        //TODO: Translation strings
        public override string GetReport()
        {
            return $"Transforming.";
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil stopDead = Toils_Goto.GotoCell(pawn.Position, PathEndMode.OnCell);
            Toil transform = ToilMaker.MakeToil("TransformToil");
            transform.activeSkill = () => AmphiDefs.RimMorpho_Shifting;
            transform.socialMode = RandomSocialMode.SuperActive;
            transform.defaultCompleteMode = ToilCompleteMode.Never;
            transform.WithProgressBar(TargetIndex.B, () => 1f - workLeft / TransformData.CalculatedWorkTicks);
            BodyTypeDef bodyTypeDef=NextBodyTypeTarget;
            transform.initAction = () =>
            {
                transformData = ShiftUtils.GetTransformData(pawn, ShifterComp, NextRaceTarget.ThingDef, NextXenoTarget);
                transformData.Active = true;
                lookingAt = pawn.Position + new IntVec3(1, 0, 1);

                energy = TransformData.CalculatedEnergyUsed;
                workLeft = TransformData.CalculatedWorkTicks;
                RVCLog.Log($"Workamount: {workLeft}, " +
                    $"current food level: {pawn.needs.food.CurLevel}, " +
                    $"predicted food level: {TransformData.PredictedTotalFoodUse}, " +
                    $"current rest level: {pawn.needs.rest.CurLevel}, " +
                    $"predicted rest level: {TransformData.PredictedTotalRestUse}, " +
                    $"predicted tick duration: {workLeft / pawn.GetStatValue(AmphiDefs.RimMorpho_TransformationStat)}, " +
                    $"processed speed: {pawn.GetStatValue(AmphiDefs.RimMorpho_TransformationStat)}", debugOnly: true);
            };

            transform.tickAction = () =>
            {
                workLeft -= TransformData.SkillStatVal;

                float energyConsumedThisTick = TransformData.SkillStatVal / TransformData.CalculatedWorkTicks * (float)energy;
                pawn.needs.food.CurLevel -= energyConsumedThisTick / 2f;
                pawn.needs.rest.CurLevel -= energyConsumedThisTick / 2f;

                if (Rand.Chance(0.1f * (1f - workLeft / TransformData.CalculatedWorkTicks)))
                {
                    lookingAt = lookingAt.RotatedBy(RotationDirection.Clockwise);
                    pawn.rotationTracker.FaceCell(pawn.Position + lookingAt);
                }

                if (Rand.Chance(0.001f / TransformData.SkillStatVal))
                {
                    FilthMaker.TryMakeFilth(pawn.Position, pawn.Map, AmphiDefs.RimMorpho_AmphimorphoGoo);
                }

                RVCLog.Log($"workLeft: {workLeft}, " +
                    $"adjustedSkillVal: {TransformData.SkillStatVal}, " +
                    $"energyConsumed: {energyConsumedThisTick}, " +
                    $"predicted food/rest: {TransformData.PredictedFoodUse(Convert.ToInt32(workLeft))}" +
                    $"/{TransformData.PredictedRestUse(Convert.ToInt32(workLeft))}, " +
                    $"current food/rest: {pawn.needs.food.CurLevel}/{pawn.needs.rest.CurLevel}", debugOnly: true);

                pawn.skills?.Learn(AmphiDefs.RimMorpho_Shifting, 1f, false);
                if (workLeft <= 0f) ReadyForNextToil();
            };

            transform.AddFinishAction(() =>
            {
                transformData.Active = false;
                if (workLeft > 0f) return;

                if (TransformData.TargetXenoDef == null)
                {
                    pawn.TryGetComp<AmphiShifter>().SetForm(TransformData.TargetRace);
                    return;
                }

                pawn.TryGetComp<AmphiShifter>().SetForm(TransformData.TargetRace, TransformData.TargetXenoDef,bodyTypeDef);
                
            });

            AddFailCondition(() => TransformData?.HasEnoughFoodLeft(workLeft) == false || TransformData?.HasEnoughRestLeft(workLeft) == false);

            yield return stopDead;
            yield return transform;
        }
    }
}
