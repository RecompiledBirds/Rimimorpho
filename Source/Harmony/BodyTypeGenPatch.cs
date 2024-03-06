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

namespace Rimimorpho
{
    public static class BodyTypeGenPatch
    {
        public static void Posfix(ref Pawn pawn)
        {
            if(!RimimorphoSettings.somePawnsAreAmphimorpho) { return; }
            if(!HiddenNoodlesPatch.HasPawn(pawn)) return;
            BodyTypeDef bodyTypeDef = pawn.story.bodyType;
            PawnChanger.ChangePawnRaceUnspawned(pawn, AmphiDefs.RimMorpho_Amphimorpho);
            ThingDef def = HiddenNoodlesPatch.PawnDef(pawn);
            AmphiShifter shifter = pawn.TryGetComp<AmphiShifter>();
            if (shifter == null) return;
            shifter.LearnSpecies(def,XenotypeDefOf.Baseliner,bodyTypeDef);
            shifter.SetForm(def,bodyTypeDef, false, true);
           
        }
    }
}
