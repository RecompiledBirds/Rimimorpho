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
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }


        public override string GetReport()
        {
            return $"Touching {(TargetA.Pawn.Name!=null?TargetA.Pawn.Name.ToStringShort:TargetA.Pawn.Label)}.";
        }
        Random random = new Random();
        public void ShapeShift()
        {

            if (random.Next(1, 100) >= 95)
            {
                if (CellFinder.TryFindRandomReachableCellNear(pawn.Position, pawn.Map, 2, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false, false, false), (IntVec3 x) => x.Standable(pawn.Map), (Region x) => true, out IntVec3 cell, 999999))
                {
                    FilthMaker.TryMakeFilth(cell, pawn.Map, AmphiDefs.RimMorpho_AmphimorphoGoo, 1, FilthSourceFlags.Pawn);
                }
               
            }
            pawn.skills.Learn(AmphiDefs.RimMorpho_Shifting, 1);
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
            //too intensive
            //TODO: signal to player that pawn cant transform
            if (energy > (pawn.needs.food.CurLevel + pawn.needs.rest.CurLevel) / 2) yield break;

            Toil toils_Goto = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
            toils_Goto.AddFailCondition(() =>
            {
                if (energy > (pawn.needs.food.CurLevel + pawn.needs.rest.CurLevel) / 2)
                {
                    Messages.Message("Rimmorpho_CanNotTouchPawn".Translate(pawn.NameShortColored), MessageTypeDefOf.RejectInput);
                    return true;
                }

                return false;
            });

            yield return toils_Goto;
            Toil waitToil =  Toils_General.Wait(ticks).WithProgressBarToilDelay(TargetIndex.B);
            waitToil.AddPreTickAction(ShapeShift);
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
