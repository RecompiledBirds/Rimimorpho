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
            Thing thing = th.Thing;
            foreach (IAttackTarget target in values) {
                Thing targThing = target.Thing;
                AmphiShifter shifter = targThing.TryGetComp<AmphiShifter>();
                if (shifter == null)
                {
                    yield return target;
                }
                else
                {
                    if (shifter.IsParentDef() || shifter.RaceProperties.Humanlike) { yield return target; } 
                    else
                    {
                        float dist = Vector3.Distance(new Vector3(thing.Position.x,thing.Position.y,thing.Position.z), new Vector3(targThing.Position.x, targThing.Position.y, targThing.Position.z));

                        if (dist < 10f)
                        {
                            yield return target;
                        }
                    }
                }
            }
        }
    }
}
