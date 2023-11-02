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
            Scribe_Values.Look(ref energy, nameof(energy));
            Scribe_Values.Look(ref energyConsumed, nameof(energyConsumed));
            Scribe_Defs.Look(ref morphDef,nameof(morphDef));
            base.ExposeData();
        }

        public override string GetReport()
        {
            return $"Transforming.";
        }

        public void ShapeShift()
        {
            pawn.skills.Learn(AmphiDefs.RimMorpho_Shifting, 1);
            if (energyConsumed < energy)
            {
                pawn.needs.food.CurLevel -= 0.05f;
                pawn.needs.rest.CurLevel -= 0.05f;
                energyConsumed += 0.1;
            }
        }

        private double energyConsumed;
        private double energy;
        protected override IEnumerable<Toil> MakeNewToils()
        {

            AmphiShifter amphiShifter = pawn.TryGetComp<AmphiShifter>();
            
            SetDef(DefDatabase<ThingDef>.AllDefs.Where(x=>x.race!=null).RandomElement());
            ShiftUtils.GetTransformData(pawn, amphiShifter, morphDef, out int ticks, out double energy);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
            this.energy=energy;
            Toil waitToil = Toils_General.Wait(ticks).WithProgressBarToilDelay(TargetIndex.B);
            waitToil.AddPreTickAction(ShapeShift);
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
