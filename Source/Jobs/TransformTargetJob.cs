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
using System.Security.Cryptography;

namespace Rimimorpho
{
    public class TransformTargetJob : JobDriver
    {
        private const float DifficultyModifier = 0.25f;

        private AmphiShifter amphiShifter;
        private TransformData transformData;

        private float workLeft = -1000f;

        private double energyConsumed;
        private double energy;

        private TransformData TransformData => transformData;

        private AmphiShifter ShifterComp => amphiShifter ?? (amphiShifter = pawn.TryGetComp<AmphiShifter>());

        public static StoredRace NextRaceTarget { get; internal set; }
        public static XenotypeDef NextXenoTarget { get; internal set; }

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

            transform.initAction = () =>
            {
                transformData = ShiftUtils.GetTransformData(pawn, ShifterComp, NextRaceTarget.ThingDef, NextXenoTarget);
                workLeft = TransformData.CalculatedWorkTicks;
                RVCLog.Log($"Workamount: {workLeft}, " +
                    $"current food level: {pawn.needs.food.CurLevel}, " +
                    $"predicted food level: {pawn.needs.food.CurLevel - energy / 2f}, " +
                    $"predicted tick duration: {workLeft / pawn.GetStatValue(AmphiDefs.RimMorpho_TransformationStat)}, " +
                    $"processed speed: {pawn.GetStatValue(AmphiDefs.RimMorpho_TransformationStat)}", debugOnly: true);
            };

            transform.tickAction = () =>
            {
                float adjustedSkillVal = pawn.GetStatValue(AmphiDefs.RimMorpho_TransformationStat) * 1.7f;
                workLeft -= adjustedSkillVal;

                float energyConsumedThisTick = adjustedSkillVal / TransformData.CalculatedWorkTicks * (float) energy;
                pawn.needs.food.CurLevel -= energyConsumedThisTick / 2f;
                pawn.needs.rest.CurLevel -= energyConsumedThisTick / 2f;

                RVCLog.Log($"workLeft: {workLeft}, adjustedSkillVal: {adjustedSkillVal}, energyConsumed: {energyConsumedThisTick}", debugOnly: true);
                pawn.skills?.Learn(AmphiDefs.RimMorpho_Shifting, 1f, false);
                if (workLeft <= 0f) ReadyForNextToil();
            };

            transform.AddFinishAction(() =>
            {
                if (workLeft > 0f) return;

                if (TransformData.TargetXenoDef == null)
                {
                    pawn.TryGetComp<AmphiShifter>().SetForm(TransformData.TargetRace);
                    return;
                }

                pawn.TryGetComp<AmphiShifter>().SetForm(TransformData.TargetRace, TransformData.TargetXenoDef);
            });

            AddFailCondition(() => !ShifterComp.CanPawnShift(DifficultyModifier));

            yield return stopDead;
            yield return transform;
        }
    }
}
