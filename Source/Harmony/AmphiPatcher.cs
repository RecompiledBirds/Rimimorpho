﻿using HarmonyLib;
using RimWorld;
using RVCRestructured;
using RVCRestructured.RVR.HarmonyPatches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Rimimorpho
{
    [StaticConstructorOnStartup]
    public static class AmphiPatcher
    {
        static AmphiPatcher()
        {
            Log.Message("Running amphi patches");
            Harmony harmony = new Harmony("RecompiledBirds.Rimimorpho");
            harmony.Patch(AccessTools.Method(typeof(Pawn_FilthTracker), "TryPickupFilth"), postfix: new HarmonyMethod(typeof(TryPickupFilthPatch), nameof(TryPickupFilthPatch.Postfix)));
            Log.Message("Amphi patches completed!");
        }
    }
}
