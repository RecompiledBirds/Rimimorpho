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
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }


        public override string GetReport()
        {
            return $"Touching {TargetA.Pawn.Name.ToStringShort}.";
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            
            AmphiShifter amphiShifter = pawn.TryGetComp<AmphiShifter>();
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
            yield return Toils_General.Wait(100).WithProgressBarToilDelay(TargetIndex.B);
            yield return new Toil
            {
                initAction = delegate
                {
                    amphiShifter.SetForm(TargetA.Pawn);
                }
            };
        }

      
    }
}
