﻿using Rimimorpho.Windows;
using RimWorld;
using RVCRestructured.Shifter;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
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
        
        public Dictionary<ThingDef,List<StoredRace>> knownSpecies = new Dictionary<ThingDef, List<StoredRace>>();

        public void LearnSpecies(Pawn pawn)
        {
            Log.Message("test");
            if (knownSpecies == null) { knownSpecies = new Dictionary<ThingDef, List<StoredRace>>(); }
            if (!knownSpecies.ContainsKey(pawn.def))
            {
                knownSpecies.Add(pawn.def, new List<StoredRace>());
                knownSpecies[pawn.def].Add(new StoredRace(pawn.def, pawn?.genes?.Xenotype));
                return;
            }
            
            if (!Enumerable.Any(knownSpecies[pawn.def], race => race.ContainsFeature(pawn.def, pawn?.genes?.Xenotype)))
            {
                knownSpecies[pawn.def].Add(new StoredRace(pawn.def, pawn.genes?.Xenotype));
            }
        }

        public void LearnSpecies(ThingDef def, XenotypeDef xenotypeDef)
        {
            if (knownSpecies[def].NullOrEmpty())
            {
                knownSpecies[def] = new List<StoredRace>
                {
                    new StoredRace(def, xenotypeDef)
                };
                return;
            }
            if (!Enumerable.Any(knownSpecies[def], race => race.ContainsFeature(def, xenotypeDef)))
            {
                knownSpecies[def].Add(new StoredRace(def, xenotypeDef));
            }
        }

        public void LearnSpecies(ThingDef def)
        {
            if (knownSpecies[def].NullOrEmpty())
            {
                knownSpecies[def] = new List<StoredRace>
                {
                    new StoredRace(def)
                };
                return;
            }
            if (!Enumerable.Any(knownSpecies[def], race => race.ContainsFeature(def)))
            {
                knownSpecies[def].Add(new StoredRace(def));
            }
        }
        //TODO: Make strings translateable
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            Pawn pawn = parent as Pawn;
            Command_Target command = new Command_Target
            {
                defaultLabel = "Rimimorpho_LearnRace".Translate(),
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
                defaultLabel = "Rimimorpho_RevertForm".Translate(),
                icon = AmphiDefs.RimMorpho_Amphimorpho.uiIcon,
                action = delegate ()
                {
                    Job job = JobMaker.MakeJob(AmphiDefs.RimMorpho_RevertForm, pawn);
                    pawn.jobs.TryTakeOrderedJob(job);
                }

            };
            Command_Action transMenu = new Command_Action
            {
                defaultLabel = "Rimimorpho_OpenTransformationWindow".Translate(),
                icon = AmphiDefs.RimMorpho_Amphimorpho.uiIcon,
                action = delegate ()
                {
                    Find.WindowStack.Add(new Ivy_TransformationWindow(pawn));
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

            
        }

        public override void SetForm(ThingDef def)
        {
            base.SetForm(def);
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
            int raceCount = knownSpecies.Values.Count;
            Scribe_Values.Look(ref raceCount, nameof(raceCount));
            List<ThingDef> saveDefs;
            List<List<StoredRace>> raceLists;
            base.PostExposeData();
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                saveDefs = knownSpecies.Keys.ToList();
                raceLists = knownSpecies.Values.ToList();
                Scribe_Collections.Look(ref saveDefs, nameof(saveDefs));
                for (int i = 0; i < raceCount; i++)
                {
                    List<StoredRace> raceList = raceLists[i];
                    Scribe_Collections.Look(ref raceList, $"{nameof(raceList)}_{saveDefs[i].defName}", LookMode.Deep);
                }
            }
            if(Scribe.mode==LoadSaveMode.LoadingVars)
            {
                saveDefs = new List<ThingDef>();
                raceLists = new List<List<StoredRace>>();
                Scribe_Collections.Look(ref saveDefs, nameof(saveDefs));
                for (int i = 0; i < raceCount; i++)
                {
                    List<StoredRace> raceList = new List<StoredRace>();
                    Scribe_Collections.Look(ref raceList, $"{nameof(raceList)}_{saveDefs[i].defName}", LookMode.Deep);
                    raceLists.Add(raceList);
                }
                for(int i  = 0; i < saveDefs.Count; i++)
                {
                    knownSpecies.Add(saveDefs[i], raceLists[i]);
                }
            }
        }
    }

 
}
