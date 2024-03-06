using RimWorld;
using RVCRestructured;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Rimimorpho
{
    public static class MeleeVerbsPatch
    {

        public static void Postfix(Pawn_MeleeVerbs __instance, Thing target, Verb verbToUse, bool surpriseAttack)
        {
            Pawn targetPawn = target as Pawn;
            Pawn pawn = __instance.Pawn;
            AmphiShifter shifter = pawn.TryGetComp<AmphiShifter>();
            if (shifter == null) { return; }
            shifter.AddPawnToAttackedList(targetPawn);
            if (verbToUse.tool.untranslatedLabel != "teeth") { return; }

            if (RimimorphoSettings.pawnsCanGetInfected && RimimorphoSettings.pawnsCanGetInfectedViaBite && Rand.Chance(0.05f))
            {
                targetPawn.health.AddHediff(AmphiDefs.RimMorpho_AmphimorphoGooInfection);
            }
        }
    }
}
