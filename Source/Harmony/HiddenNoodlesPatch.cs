using HarmonyLib;
using RimWorld;
using RVCRestructured;
using RVCRestructured.RVR.HarmonyPatches;
using RVCRestructured.Shifter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Rimimorpho
{
    public static class HiddenNoodlesPatch
    {
       
        public static bool HasDef(ThingDef def) {  return amphiQueue.Count>0 && amphiQueue.Contains(def); }
        public static ThingDef PawnDef() {
            ThingDef def = amphiQueue[0];
            amphiQueue.RemoveAt(0);
            return def;
        }

        private static readonly List<ThingDef> amphiQueue = new List<ThingDef>();
        public static void Postfix(ref ThingDef __result)
        {
            if (!RimimorphoSettings.somePawnsAreAmphimorpho || !Rand.Chance(1.0f)) return;
            amphiQueue.Add(__result);
            __result = AmphiDefs.RimMorpho_Amphimorpho;
        }

        public static void AmphiMakeThingPostfix(ref Thing __result)
        {
            if (!RimimorphoSettings.somePawnsAreAmphimorpho) { return; }
            if (!(__result is Pawn pawn)) return;
            if (!HasDef(__result.def)) return;
            BodyTypeDef bodyTypeDef = pawn.story.bodyType;
            PawnChanger.ChangePawnRaceUnspawned(pawn, AmphiDefs.RimMorpho_Amphimorpho);
            AmphiShifter shifter = pawn.TryGetComp<AmphiShifter>();
            if (shifter == null) return;
            ThingDef def = PawnDef();
            shifter.LearnSpecies(def, XenotypeDefOf.Baseliner, bodyTypeDef);
            shifter.SetForm(def, bodyTypeDef, false, true);
        }
    }
}
