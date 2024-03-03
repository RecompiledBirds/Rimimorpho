using Rimimorpho.Windows;
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
        public AmphiShifterProps() 
        { 
            this.compClass=typeof(AmphiShifter);
        }
    }
    public class AmphiShifter : ShapeshifterComp
    {
        private bool shifted = false;

        public Dictionary<ThingDef, RaceList<StoredRace>> knownSpecies = new Dictionary<ThingDef, RaceList<StoredRace>>();

        public void LearnSpecies(Pawn pawn)
        {
            Log.Message("test");
            if (knownSpecies == null) { knownSpecies = new Dictionary<ThingDef, RaceList<StoredRace>>(); }
            if (!knownSpecies.ContainsKey(pawn.def))
            {
                knownSpecies.Add(pawn.def, new RaceList<StoredRace>());
                knownSpecies[pawn.def].Add(new StoredRace(pawn.def, pawn?.genes?.Xenotype));
                return;
            }
            
            if (!Enumerable.Any((IEnumerable<StoredRace>)knownSpecies[pawn.def], race => race.ContainsFeature(pawn.def, pawn?.genes?.Xenotype)))
            {
                knownSpecies[pawn.def].Add(new StoredRace(pawn.def, pawn.genes?.Xenotype));
            }
        }

        public void LearnSpecies(ThingDef def, XenotypeDef xenotypeDef)
        {
            if (knownSpecies[def].Empty)
            {
                knownSpecies[def] = new RaceList<StoredRace>
                {
                    new StoredRace(def, xenotypeDef)
                };
                return;
            }
            if (!Enumerable.Any((IEnumerable<StoredRace>)knownSpecies[def], race => race.ContainsFeature(def, xenotypeDef)))
            {
                knownSpecies[def].Add(new StoredRace(def, xenotypeDef));
            }
        }

        public void LearnSpecies(ThingDef def)
        {
            if (knownSpecies[def].Empty)
            {
                knownSpecies[def] = new RaceList<StoredRace>
                {
                    new StoredRace(def)
                };
                return;
            }
            if (!Enumerable.Any((IEnumerable<StoredRace>)knownSpecies[def], race => race.ContainsFeature(def)))
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
                action = () => Find.WindowStack.Add(new TransformationSelectionWindow(pawn))
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
            Scribe_Collections.Look(ref knownSpecies, nameof(knownSpecies), LookMode.Def, LookMode.Deep);
        }
    }
}
