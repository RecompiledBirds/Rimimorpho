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
                    bool hasInfection = pawn.health.hediffSet.HasHediff(AmphiDefs.RimMorpho_AmphimorphoGooInfection);
                    if (!Rand.Chance(0.1f)) return;
                    if (hasInfection)
                    {
                        pawn.health.hediffSet.GetFirstHediffOfDef(AmphiDefs.RimMorpho_AmphimorphoGooInfection).Severity += 0.3f;
                        return;
                    }
                    pawn.health.AddHediff(AmphiDefs.RimMorpho_AmphimorphoGooInfection);
                }
            };

            for(int i=index; i<toils.Count; i++)
            {
                yield return toils[i];
            }
        }
    }
}
