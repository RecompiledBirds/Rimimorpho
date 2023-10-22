using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Rimimorpho
{
    public static class TryPickupFilthPatch
    {
        public static void Postfix(Pawn_FilthTracker __instance)
        {
            Pawn pawn = (Pawn)typeof(Pawn_FilthTracker).GetField("pawn").GetValue(__instance);
            List<Thing> thingList = pawn.Position.GetThingList(pawn.Map);
            for (int j = thingList.Count - 1; j >= 0; j--)
            {
                Filth filth = thingList[j] as Filth;
                if (filth != null && filth.CanFilthAttachNow)
                {
                    //add hediff
                }
            }
        }
    }
}
