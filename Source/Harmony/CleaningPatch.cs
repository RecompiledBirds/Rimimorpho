using RimWorld;
using RVCRestructured;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Rimimorpho
{
    public static class CleaningPatch
    {
        public static IEnumerable<Toil> Postfix(IEnumerable<Toil> __result, JobDriver_CleanFilth __instance)
        {
            Pawn pawn = __instance.pawn;
            List<Toil> toils = __result.ToList();

            int index = (__result.Count()) - 1;
            for(int i = 0; i<index; i++)
            {
                yield return toils[i];
            }


            yield return new Toil()
            {
                initAction = delegate
                {

                    Filth filth = (Filth)__instance.job.GetTarget(TargetIndex.A).Thing;
                    if (filth.def != AmphiDefs.RimMorpho_AmphimorphoGoo)
                    {
                        return;
                    }
                    System.Random random = new System.Random();
                    if (random.Next(0, 100) >= 95)
                    {
                        bool hasInfection = pawn.health.hediffSet.HasHediff(AmphiDefs.RimMorpho_AmphimorphoGooInfection);
                        if (hasInfection)
                        {
                            pawn.health.hediffSet.GetFirstHediffOfDef(AmphiDefs.RimMorpho_AmphimorphoGooInfection).Severity += 0.3f;
                            return;
                        }
                        pawn.health.AddHediff(AmphiDefs.RimMorpho_AmphimorphoGooInfection);
                    }
                }
            };

            for(int i=index; i<toils.Count; i++)
            {
                yield return toils[i];
            }
        }
    }
}
