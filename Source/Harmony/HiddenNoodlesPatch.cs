using HarmonyLib;
using RimWorld;
using RVCRestructured;
using RVCRestructured.RVR.HarmonyPatches;
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
        private static Dictionary<Pawn,ThingDef> pawns= new Dictionary<Pawn, ThingDef>();
        public static bool HasPawn(Pawn pawn) {  return pawns.Keys.Contains(pawn); }
        public static void RemovePawn(Pawn pawn) { pawns.Remove(pawn);}
        public static ThingDef PawnDef(Pawn pawn) { return pawns[pawn]; }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();

            for (int a = 0; a < codes.Count; a++)
            {
                //Look for where the pawn is created.
                if (codes[a].opcode == OpCodes.Call && (codes[a].Calls(typeof(ThingMaker).GetMethod("MakeThing"))|| codes[a].Calls(typeof(PawnBlender).GetMethod("GetHumanoidRace"))))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldobj, typeof(PawnGenerationRequest));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HiddenNoodlesPatch), "ReplaceWithAmphi", new Type[] { typeof(PawnGenerationRequest) }));
                }
                else
                {
                    yield return codes[a];
                }
            }
        }
        
        public static Thing ReplaceWithAmphi(PawnGenerationRequest request)
        {

            ThingDef def = request.KindDef.race;
            //saftey check for scenario pawns
            if (request.Context.HasFlag(PawnGenerationContext.PlayerStarter))
                return ThingMaker.MakeThing(def);
            if (!RimimorphoSettings.somePawnsAreAmphimorpho || !Rand.Chance(0.05f)) return PawnBlender.GetHumanoidRace(request);
            Pawn pawn = (Pawn)PawnBlender.GetHumanoidRace(request);
            pawns.Add(pawn,def);
            return pawn;
        }
    }
}
