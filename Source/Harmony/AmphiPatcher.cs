using HarmonyLib;
using RimWorld;
using RVCRestructured;
using RVCRestructured.RVR.Harmony;
using RVCRestructured.RVR.HarmonyPatches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Rimimorpho
{
    [StaticConstructorOnStartup]
    public static class AmphiPatcher
    {
        static AmphiPatcher()
        {
            RVCLog.MSG("Running amphi patches");
            Harmony harmony = new Harmony("RecompiledBirds.Rimimorpho");
           
            harmony.Patch(AccessTools.Method(typeof(Pawn_FilthTracker), "TryPickupFilth"), postfix: new HarmonyMethod(typeof(TryPickupFilthPatch), nameof(TryPickupFilthPatch.Postfix)));
            harmony.Patch(AccessTools.Method(typeof(JobDriver_CleanFilth), "MakeNewToils"), postfix: new HarmonyMethod(typeof(CleaningPatch), nameof(CleaningPatch.Postfix)));
            harmony.Patch(AccessTools.Method(typeof(SkillUI), nameof(SkillUI.DrawSkillsOf)), prefix: new HarmonyMethod(typeof(SkillPatch), nameof(SkillPatch.Prefix)));
            harmony.Patch(AccessTools.Method(typeof(AttackTargetsCache), "GetPotentialTargetsFor"), postfix: new HarmonyMethod(typeof(PotentialTargetsPatch),nameof(PotentialTargetsPatch.Postfix)));
            harmony.Patch(AccessTools.Method(typeof(Pawn_MeleeVerbs), "TryMeleeAttack"), postfix: new HarmonyMethod(typeof(MeleeVerbsPatch), nameof(MeleeVerbsPatch.Postfix)));
            harmony.Patch(AccessTools.Method(typeof(PawnBlenderPatches), nameof(PawnBlenderPatches.ThingDefgenerator)), postfix: new HarmonyMethod(typeof(HiddenNoodlesPatch), nameof(HiddenNoodlesPatch.Postfix)));
            

            //Some patches need to run after Vine
            //Those patches are defined in their class.
            harmony.PatchAll();
            RVCLog.MSG($"Rimimorpho completed {harmony.GetPatchedMethods().Count()} patches!");
        }
    }
}
