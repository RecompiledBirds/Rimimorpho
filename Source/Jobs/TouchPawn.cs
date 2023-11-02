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
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }


        public override string GetReport()
        {
            return $"Touching {(TargetA.Pawn.Name!=null?TargetA.Pawn.Name.ToStringShort:TargetA.Pawn.Label)}.";
        }

        public void ConsumeEnergy()
        {
            if (energyConsumed < energy)
            {
                pawn.needs.food.CurLevel -= 0.05f;
                pawn.needs.rest.CurLevel -= 0.05f;
                energyConsumed += 0.1;
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref energy, nameof(energy));
            Scribe_Values.Look(ref energyConsumed, nameof(energyConsumed));
            base.ExposeData();
        }
        private double energyConsumed;
        private double energy;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            
            AmphiShifter amphiShifter = pawn.TryGetComp<AmphiShifter>();
            ShiftUtils.GetTransformData(pawn, amphiShifter,TargetA.Pawn,out int ticks, out double energy);
            this.energy = energy;
            
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
            RVCLog.Log(ticks);
            Toil waitToil =  Toils_General.Wait(ticks).WithProgressBarToilDelay(TargetIndex.B);
            waitToil.AddPreTickAction(ConsumeEnergy);
            yield return waitToil;
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
