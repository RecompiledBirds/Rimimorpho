using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Rimimorpho
{
    public static class TryPickupFilthPatch
    {
        public static void Postfix(Pawn_FilthTracker __instance)
        {
            Pawn pawn = (Pawn)typeof(Pawn_FilthTracker).GetField("pawn",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(__instance);
            List<Thing> thingList = pawn.Position.GetThingList(pawn.Map);
            for (int j = thingList.Count - 1; j >= 0; j--)
            {
                if (thingList[j] is Filth filth && filth.CanFilthAttachNow)
                {
                    if (filth.def != AmphiDefs.RimMorpho_AmphimorphoGoo)
                    {
                        return;
                    }
                    Random random = new Random();
                    if (random.Next(0,100)>=99)
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
            }
        }
    }
}
