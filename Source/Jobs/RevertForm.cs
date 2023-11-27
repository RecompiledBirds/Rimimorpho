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
        public void ShapeShift()
        {
            pawn.skills.Learn(AmphiDefs.RimMorpho_Shifting, 0.3f);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            AmphiShifter amphiShifter = pawn.TryGetComp<AmphiShifter>();
            ShiftUtils.GetTransformData(pawn, amphiShifter, TargetA.Pawn, out float ticks, out double energy);
            //Toil waitToil= Toils_General.Wait(ticks).WithProgressBarToilDelay(TargetIndex.A);
            //waitToil.AddPreTickAction(ShapeShift);
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
