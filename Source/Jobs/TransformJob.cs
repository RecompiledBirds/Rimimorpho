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

namespace Rimimorpho
{
    public class TransformJob : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        public void SetDef(ThingDef def)
        {
            morphDef= def;
        }
        private ThingDef morphDef;

        public override void ExposeData()
        {
            Scribe_Defs.Look(ref morphDef,nameof(morphDef));
            base.ExposeData();
        }

        public override string GetReport()
        {
            return $"Transforming.";
        }

        public void ConsumeEnergy()
        {
            pawn.needs.food.CurLevel -= 0.05f;
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {

            AmphiShifter amphiShifter = pawn.TryGetComp<AmphiShifter>();
            int morphTicks = ShiftUtils.ShiftDifficulty(pawn, amphiShifter, TargetA.Pawn.def);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
            RVCLog.Log(morphTicks);
            Toil waitToil = Toils_General.Wait(morphTicks).WithProgressBarToilDelay(TargetIndex.B);
            waitToil.AddPreTickAction(ConsumeEnergy);
            yield return waitToil;
            yield return new Toil
            {
                initAction = delegate
                {
                    amphiShifter.SetForm(morphDef);
                }
            };
        }
    }
}
