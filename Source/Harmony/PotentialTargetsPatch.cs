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
                Pawn tPawn = targThing as Pawn;
                AmphiShifter shifter = targThing.TryGetComp<AmphiShifter>();
                if (shifter == null)
                {
                    yield return target;
                    continue;
                }

                shifter.CleanupAttackedPawns();
                if (shifter.CurrentForm.race.Humanlike) {
                    shifter.AddPawnToAttackedList(pawn);
                    yield return target; 
                    continue; 
                }
                if (shifter.AttackedPawns.ContainsKey(pawn)) { yield return target; continue; }

                float dist = IntVec3Utility.DistanceTo(pawn.Position, targThing.Position);

                float value = Math.Max(2f, 6f - tPawn.skills.GetSkill(AmphiDefs.RimMorpho_Shifting).Level);
                if (pawn.Faction.IsPlayer) value += 5;
                if (dist < value)
                {
                    shifter.AddPawnToAttackedList(pawn);
                    yield return target;
                }

            }
        }
    }
}
