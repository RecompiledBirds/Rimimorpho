using RVCRestructured;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Rimimorpho
{
    public static class PotentialTargetsPatch
    {
        public static IEnumerable<IAttackTarget> Postfix(IEnumerable<IAttackTarget> values, IAttackTargetSearcher th)
        {
            Pawn pawn = (Pawn)th.Thing;

            foreach (IAttackTarget target in values)
            {
                Thing targThing = target.Thing;
                AmphiShifter shifter = targThing.TryGetComp<AmphiShifter>();
                if (shifter == null)
                {
                    yield return target;
                    continue;
                }

                shifter.CleanupAttackedPawns();
                if (shifter.CurrentForm.race.Humanlike) { yield return target; continue; }
                if (shifter.AttackedPawns.ContainsKey(pawn)) { yield return target; continue; }

                float dist = IntVec3Utility.DistanceTo(pawn.Position, targThing.Position);


                if (dist < 2f)
                {
                    shifter.AddPawnToAttackedList(pawn);
                    yield return target;
                }

            }
        }
    }
}
