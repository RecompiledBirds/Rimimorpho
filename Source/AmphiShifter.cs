﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using static UnityEngine.GraphicsBuffer;

namespace Rimimorpho
{
    public class AmphiShifterProps : CompProperties
    {
        public AmphiShifterProps() { 
            this.compClass=typeof(AmphiShifter);
        }
    }
    public class AmphiShifter : RVCRestructured.Shifter.ShapeshifterComp
    {
        private bool shifted = false;
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            Pawn pawn = parent as Pawn;
            Command_Target command = new Command_Target
            {
                defaultLabel = "tuch",
                icon = AmphiDefs.RimMorpho_Amphimorpho.uiIcon,
                targetingParams = new TargetingParameters
                {
                    canTargetAnimals = true,
                    canTargetPawns = true,
                    canTargetHumans = true
                }

            };
            command.action = delegate (LocalTargetInfo target)
            {
                Job job = JobMaker.MakeJob(AmphiDefs.RimMorpho_TouchPawn, target.Pawn);
                pawn.jobs.TryTakeOrderedJob(job);
            };
            Command_Action revert = new Command_Action
            {
                defaultLabel = "fuck go back",
                icon = AmphiDefs.RimMorpho_Amphimorpho.uiIcon,
                action = delegate ()
                {
                    Job job = JobMaker.MakeJob(AmphiDefs.RimMorpho_RevertForm, pawn);
                    pawn.jobs.TryTakeOrderedJob(job);
                }

            };
            yield return command;
            yield return revert;
        }



        public override void PostExposeData()
        {

            Scribe_Values.Look(ref shifted, nameof(shifted));
            base.PostExposeData();
        }
    }

 
}