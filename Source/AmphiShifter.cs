﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        
        public List<StoredRace> knownRaces= new List<StoredRace>();

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
                },
                action = delegate (LocalTargetInfo target)
                {
                    Job job = JobMaker.MakeJob(AmphiDefs.RimMorpho_TouchPawn, target.Pawn);
                    pawn.jobs.TryTakeOrderedJob(job);
                }
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

        private RaceProperties raceProperties = null;
        public override RaceProperties RaceProperties
        {
            get
            {
                if (raceProperties == null)
                {
                    raceProperties =base.RaceProperties;
                    FieldInfo field = typeof(RaceProperties).GetField("bloodDef", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    field.SetValue(raceProperties, field.GetValue(parent.def.race));
                }
                return raceProperties;
            }
        }

        public override void SetForm(Pawn pawn)
        {
            base.SetForm(pawn);
            /*
            if (!knownRaces.Any(x => x.storedDef == pawn.def && (!(x is StoredRaceWithXenoType xenoRace) || xenoRace.storedXenotypeDef == pawn.genes.Xenotype)))
            {
                knownRaces.Add(new StoredRaceWithXenoType() { storedDef=pawn.def,storedXenotypeDef=pawn.genes.Xenotype });
            }
            */
        }

        public override void SetForm(ThingDef def)
        {
            base.SetForm(def);
            raceProperties = null;
            /*
            if (!knownRaces.Any(x => x.storedDef == def))
                knownRaces.Add(new StoredRace() { storedDef = def });*/
        }

        private int ticksDownedFor = 0;
        public override void CompTick()
        {
            if (CurrentForm == parent.def) return;
            Pawn pawn = parent as Pawn;
            if (!pawn.Downed)
            {
                ticksDownedFor = 0;
                return;
            }
            ticksDownedFor++;

            if (ticksDownedFor >= 3600)
            {
                RevertForm();
            }
            base.CompTick();
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref ticksDownedFor, nameof(ticksDownedFor));
            Scribe_Values.Look(ref shifted, nameof(shifted));
            base.PostExposeData();
          //  Scribe_Collections.Look(ref knownRaces,nameof(knownRaces),LookMode.Deep);
        }
    }

 
}
