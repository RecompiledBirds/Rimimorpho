using RimWorld;
using RVCRestructured.Shifter;
using System;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse.AI;

namespace Rimimorpho
{
    public class AmphiShifterProps : CompProperties
    {
        public AmphiShifterProps() { 
            this.compClass=typeof(AmphiShifter);
        }
    }
    public class AmphiShifter : ShapeshifterComp
    {
        private bool shifted = false;
        
        public List<StoredRace> knownRaces = new List<StoredRace>();

        //TODO: Make strings translateable
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
                    canTargetHumans = true,
                    canTargetSelf = false
                },
                action = delegate (LocalTargetInfo target)
                {
                    Job job = JobMaker.MakeJob(AmphiDefs.RimMorpho_TouchPawn, target.Pawn, pawn);
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
            Command_Action transMenu = new Command_Action
            {
                defaultLabel = "Open Transformation Menu",
                icon = AmphiDefs.RimMorpho_Amphimorpho.uiIcon,
                action = delegate ()
                {
                    Find.WindowStack.Add(new TransformationSelectionWindow(pawn));
                }
            };
            yield return command;
            yield return revert;
            yield return transMenu;
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

            if (!Enumerable.Any(knownRaces, race => race.ContainsFeature(pawn.def, pawn?.genes?.Xenotype)))
            {
                knownRaces.Add(new StoredRace(pawn.def, pawn?.genes?.Xenotype));
            }
        }

        public override void SetForm(ThingDef def)
        {
            base.SetForm(def);
            raceProperties = null;

            if (!Enumerable.Any(knownRaces, race => race.ContainsFeature(def) == true)) knownRaces.Add(new StoredRace(def));
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
            Scribe_Collections.Look(ref knownRaces, nameof(knownRaces), LookMode.Deep);
        }
    }

 
}
