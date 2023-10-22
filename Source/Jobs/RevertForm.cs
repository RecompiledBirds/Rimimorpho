using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Rimimorpho
{
    public class RevertForm : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            AmphiShifter amphiShifter = pawn.TryGetComp<AmphiShifter>();
            int morphTicks = ShiftUtils.ShiftDifficulty(pawn, amphiShifter, TargetA.Pawn.def);
            yield return Toils_General.Wait(morphTicks).WithProgressBarToilDelay(TargetIndex.A);
            yield return new Toil
            {
                initAction = delegate
                {
                    amphiShifter.RevertForm();
                }
            };
        }
    }
}
