using RimWorld;
using RVCRestructured.RVR;
using RVCRestructured;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using System.Drawing;
using System.Security.Cryptography;
using RVCRestructured.Shifter;
using HarmonyLib;

namespace Rimimorpho
{
    [HarmonyPatch(typeof(PawnGenerator), "GenerateBodyType")]
    public static class BodyTypeGenPatch
    {
        [HarmonyAfter("RecompiledBirds.RVC.RVR")]
        public static void Postfix(ref Pawn pawn)
        {
            if(!RimimorphoSettings.somePawnsAreAmphimorpho) { return; }
            if(!HiddenNoodlesPatch.HasPawn(pawn)) return;
            BodyTypeDef bodyTypeDef = pawn.story.bodyType;
            PawnChanger.ChangePawnRaceUnspawned(pawn, AmphiDefs.RimMorpho_Amphimorpho);
            AmphiShifter shifter = pawn.TryGetComp<AmphiShifter>();
            if (shifter == null) return;
            ThingDef def = HiddenNoodlesPatch.PawnDef(pawn);
            shifter.LearnSpecies(def,XenotypeDefOf.Baseliner,bodyTypeDef);
            shifter.SetForm(def,bodyTypeDef, false, true);
           
        }
    }
}
